using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Limcap.FTerminal {
	public partial class Terminal {

		private readonly Dictionary<string, Type> _cmdList;


		public void RegisterCommand<T>( string invokeString = null ) where T : ICommand, new() {
			RegisterCommand( typeof( T ), invokeString );
		}

		public void RegisterCommand<T>( params T[] commands ) where T : ICommand, new() {
			foreach (var command in commands) RegisterCommand( typeof( T ) );
		}








		public void RegisterCommandsInNamespaces( params string[] nspaces ) {
			var typeICommand = typeof( ICommand );
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var nspace in nspaces) {
				var types = assemblies
				.SelectMany( s => s.GetTypes() )
				.Where( p => typeICommand.IsAssignableFrom( p ) && p.Namespace == nspace );
				foreach (var t in types) RegisterCommand( t );
			}
		}








		void RegisterCommand( Type type, string customInvokeText = null ) {
			string defaultLocale = (string)(type.GetConst( "DEFAULT_LOCALE" ) ?? Locale);
			string invokeText;
			if (customInvokeText != null) invokeText = customInvokeText;
			else {
				invokeText = type.GetConst( "INVOKE_TEXT" ) as string;
				if (type.IsSubclassOf( typeof( ACommand ) ) && defaultLocale != Locale) {
					var translated = GetTranslatedInvokeText( type, Locale );
					if (translated != "INVOKE_TEXT") invokeText = translated;
				}
			}
			if (invokeText is null) throw new UninvokableCommandException( type );
			_cmdList.Add( invokeText, type );
			//_predictor.ExtendInternalTree( invokeText );
		}








		string GetTranslatedInvokeText( Type type, string locale ) {
			var translator = Translator.LoadTranslator( type, locale );
			//var invokeText = type.GetConst( "INVOKE_TEXT" ) as string;
			return translator.Translate( "INVOKE_TEXT" );
		}








		public string CommandInterpreter( string input ) {
			var (ok, result) = TryRunningNativeCommand( input );
			if (ok) return result;

			bool endsWithTerminator = Assistant.SplitInput( input ).inpCmd.EndsWith( CmdParser.CMD_TERMINATOR );
			if (!endsWithTerminator) {
				_screen.AppendText( CmdParser.CMD_TERMINATOR_AS_STRING );
				//if (_assistant.ParsedCommand == null)
				_assistant.TryAdvanceTerminator();
			}

			var cmd = _assistant.ParsedCommand;
			if (cmd == null) {
				// If the command does not exist and thus won't be executed, we need to clean old info in the assistant,
				// so that if user press tab right away, there wont be any confirmed nodes in the CmdParser.
				_assistant.Reset();
				return "Comando não reconhecido.";
			}

			var args = _assistant.RawArgs;
			if (args == "?") {
				_assistant.Reset();
				return cmd.Info;
			}

			Type cmdType = cmd.GetType();
			int requiredPrivilege = (int)(cmdType.GetConst( "REQUIRED_PRIVILEGE" ) ?? 0);
			if (CurrentPrivilege < requiredPrivilege) {
				_assistant.Reset();
				return INSUFICIENT_PRIVILEGE_MESSAGE;
			}

			if (IsParametersComplete( cmd, _assistant.ParsedArgs ))
				return RunCommand( cmd, _assistant.ParsedArgs );
			else {
				//TypeText( " (Forneça os parâmetros)", FadedFontColor );
				return CommandRunnerHelper( cmd, _assistant.ParsedArgs.ToList(), !endsWithTerminator );
			}
		}








		private (bool ok, string result) TryRunningNativeCommand( string input ) {
			if (input == "exit") {
				Clear();
				onExit?.Invoke();
				return (true, string.Empty);
			}
			if (input == "clear") {
				Clear();
				GC.Collect();
				return (true, null);
			}
			if (input == "reset") {
				_screen = BuildTextScreen();
				_screen.Focus();
				DockPanel.SetDock( _screen.UIControlHook, Dock.Top );
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				return (true, null);
			}
			return (false, null);
		}








		public string GetCommandInfo( Type t ) {
			if (t.IsSubclassOf( typeof( ACommand ) )) {
				var instance = Activator.CreateInstance( t, Locale ) as ACommand;
				return instance.Info;
			}
			else if (t.IsAssignableFrom( typeof( ICommand ) ))
				return t.GetConst( "HELP_INFO" ) as string ?? "Este comando não possui informação de ajuda.";
			return null;
		}







		private bool IsParametersComplete( ACommand cmd, Arg[] args ) {
			return cmd.Parameters?.All( p => args.Where( a => a.name == p.name ).Count() > 0 ) ?? true;
		}








		private string CommandRunnerHelper( ACommand cmd, List<Arg> args, bool includeOptional, Parameter missingArg = null, string inputValue = null ) {
			try {
				AssistParameterFilling( cmd, args, includeOptional, missingArg, inputValue );
				var fullStr = AssembleFullInvokeString( cmd, args );
				if( fullStr != null ) _cmdHistory.Add( fullStr );
				return RunCommand( cmd, args.ToArray() );
			}
			catch (ParameterFillingInProgress) {
				return null;
			}
			catch (Exception ex) {
				return ex.ToString();
			}
		}








		private string AssembleFullInvokeString( ACommand cmd, List<Arg> args ) {
			var cmdStr = cmd.GetType().GetConst( "INVOKE_TEXT" ) + ": ";
			var argList = args.Where( a => cmd.Parameters.ToList()
				.FindIndex( p => a.name == p.name && (!a.ValueIsEmpty || !p.optional) ) > -1 )
				.Select( arg => $"{arg.name}={arg.value}" );
			if (argList.Count() == 0) return null;
			var argStr = string.Join( ", ", argList );
			return cmdStr + argStr;
		}








		private void AssistParameterFilling( ACommand cmd, List<Arg> args, bool includeOptional, Parameter missingArg = null, string inputValue = null ) {
			if (args.Capacity < cmd.Parameters.Length) args.Capacity = cmd.Parameters.Length;

			if (cmd.Parameters != null) {
				if (missingArg != null)
					args.Add( new Arg() { name = missingArg.name, value = inputValue, parameter = missingArg } );

				foreach (var p in cmd.Parameters.Where( p => !p.optional )) RequireParam( p, false );
				if (includeOptional) foreach (var p in cmd.Parameters.Where( p => p.optional )) RequireParam( p, true );
				void RequireParam( Parameter p, bool optional ) {
					if (args.FindIndex( a => a.name == p.name ) == -1) {
						// Start a new input line for the input of parameters
						_screen.NewBuffer().AppendText( NEW_LINE );
						_customInterpreter = ( input ) => CommandRunnerHelper( cmd, args, includeOptional, p, input );
						if (optional) TypeText( $"  [{p.name}] = " );
						else TypeText( $"  {p.name} = " );
						_statusArea.Text = $"{(optional ? "Optional parameter" : "Parameter")} '{p.name}': {p.description}";
						_assistantArea.Text = "Value: " + p.type.ToString();
						_screen.NewBuffer();
						throw new ParameterFillingInProgress();
					}
				}
			}
		}








		private string RunCommand( ACommand cmd, Arg[] args ) {
			// Start a new block for the text printed by the command.
			_screen.NewBlock( FadedFontColor );
			var result = cmd.MainFunction( this, args );
			_assistant.Reset();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			return result;
		}
	}
}
