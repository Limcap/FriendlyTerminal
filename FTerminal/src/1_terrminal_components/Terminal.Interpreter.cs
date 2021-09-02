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
			var (ok, result) = TryNativeCommand( input );
			if (ok) return result;

			// try parsing commands if input does not end with terminator.
			bool endsWithTerminator = Assistant.SplitInput( input ).inpCmd.EndsWith( CmdParser.CMD_TERMINATOR );
			if (!endsWithTerminator) {
				_screen.AppendText( CmdParser.CMD_TERMINATOR_AS_STRING );
				_assistant.TryAdvanceTerminator();
			}

			var cmd = _assistant.ParsedCommand;
			if (TrySpecialCases( cmd )) return null;

			// defines the special parameter cases
			var thereAreNoParams = Ext.IsNullOrEmpty( _assistant.ParsedCommand.Parameters );
			var allMandatoryParametersAreFilled = IsParametersComplete( cmd, _assistant.ParsedArgs );
			var allParametersAreOptionalAndNoArgWasDefined =
				_assistant.ParsedArgs.Where( a => a.NameIsComplete ).Count() == 0
				&& cmd.Parameters != null && cmd.Parameters.Where( p => !p.optional ).Count() == 0;

			if (allParametersAreOptionalAndNoArgWasDefined)
				return CommandRunnerHelper( true );
			else if (allMandatoryParametersAreFilled && (endsWithTerminator || thereAreNoParams))
				return RunCommand( cmd, _assistant.ParsedArgs.ToArray() );
			else
				return CommandRunnerHelper( !endsWithTerminator );
			//TypeText( " (Forneça os parâmetros)", FadedFontColor );
		}








		private bool TrySpecialCases(ACommand cmd) {
			if (cmd == null) {
				// If the command does not exist and thus won't be executed, we need to clean old info in the assistant,
				// so that if user press tab right away, there wont be any confirmed nodes in the CmdParser.
				_assistant.Reset();
				_screen.NewBlock( FadedFontColor ).AppendText( "Comando não reconhecido." );
				return true;
			}

			var args = _assistant.RawArgs;
			if (args == "?") {
				_assistant.Reset();
				_screen.NewBlock( FadedFontColor ).AppendText( cmd.Info );
				return true;
			}

			Type cmdType = cmd.GetType();
			int requiredPrivilege = (int)(cmdType.GetConst( "REQUIRED_PRIVILEGE" ) ?? 0);
			if (CurrentPrivilege < requiredPrivilege) {
				_assistant.Reset();
				_screen.NewBlock( FadedFontColor ).AppendText( INSUFICIENT_PRIVILEGE_MESSAGE );
				return true;
			}
			return false;
		}








		private (bool ok, string result) TryNativeCommand( string input ) {
			if (input == "exit") {
				Clear();
				onExit?.Invoke();
				return (true, string.Empty);
			}
			if (input == "clear") {
				Clear();
				Util.CallGarbageCollector();
				return (true, null);
			}
			if (input == "reset") {
				_screen = BuildTextScreen();
				_screen.Focus();
				DockPanel.SetDock( _screen.UIControlHook, Dock.Top );
				Util.CallGarbageCollector();
				return (true, null);
			}
			return (false, null);
		}








		//public string GetCommandInfo( Type t ) {
		//	if (t.IsSubclassOf( typeof( ACommand ) )) {
		//		var instance = Activator.CreateInstance( t, Locale ) as ACommand;
		//		return instance.Info;
		//	}
		//	else if (t.IsAssignableFrom( typeof( ICommand ) ))
		//		return t.GetConst( "HELP_INFO" ) as string ?? "Este comando não possui informação de ajuda.";
		//	return null;
		//}







		private bool IsParametersComplete( ACommand cmd, List<Arg> args ) {
			return cmd.Parameters?
				.Where( p => !p.optional )
				.All( p => args.Where( a => a.name == p.name ).Count() > 0 ) ?? true;
		}








		private string CommandRunnerHelper( bool includeOptional, Parameter missingArg = null, string inputValue = null ) {
			try {
				AssistParameterFilling( includeOptional, missingArg, inputValue );
				var fullStr = AssembleFullInvokeString();
				if (fullStr != null && _assistant.ParsedCommand.Parameters != null) _cmdHistory.Add( fullStr );
				return RunCommand( _assistant.ParsedCommand, _assistant.ParsedArgs.ToArray() );
			}
			catch (ParameterFillingInProgress) {
				return null;
			}
			catch (Exception ex) {
				return ex.ToString();
			}
		}








		private string AssembleFullInvokeString() {
			var cmdStr = _assistant.GetFullCmmdString();
			var argStr = _assistant.GetFullArgsString();
			return cmdStr + argStr;
		}








		private void AssistParameterFilling( bool includeOptional, Parameter missingArg = null, string inputValue = null ) {
			var args = _assistant.ParsedArgs;
			var cmd = _assistant.ParsedCommand;

			if (cmd.Parameters != null) {
				if (args.Capacity < cmd.Parameters.Length) args.Capacity = cmd.Parameters.Length;
				if (missingArg != null)
					args.Add( new Arg() { name = missingArg.name, value = inputValue, parameter = missingArg } );

				foreach (var p in cmd.Parameters.Where( p => !p.optional )) RequireParam( p, false );
				if (includeOptional) foreach (var p in cmd.Parameters.Where( p => p.optional )) RequireParam( p, true );
				void RequireParam( Parameter p, bool optional ) {
					if (args.FindIndex( a => a.name == p.name ) == -1) {
						// Start a new input line for the input of parameters
						_screen.NewBuffer().AppendText( NEW_LINE );
						_customInterpreter = ( input ) => CommandRunnerHelper( includeOptional, p, input );
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
			Util.CallGarbageCollector();
			return result;
		}
	}
}
