using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Limcap.UTerminal {
	public partial class Terminal {

		private readonly Dictionary<string, Type> _cmdList;


		public void RegisterCommand<T>( string invokeString = null ) where T : ICommand, new() {
			RegisterCommand( typeof( T ), invokeString );
		}
		
		public void RegisterCommand<T>( params T[] commands  ) where T : ICommand, new() {
			foreach( var command in commands ) RegisterCommand( typeof( T ) );
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
			if (input == "exit") {
				Clear();
				onExit?.Invoke();
				return string.Empty;
			}
			if (input == "clear") {
				Clear();
				GC.Collect();
				return null;
			}
			if (input == "reset") {
				_mainArea = BuildMainArea();
				_scrollArea.Content = _mainArea;
				//StartNewInputBuffer();
				_mainArea.Focus();
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				DockPanel.SetDock( _mainArea, Dock.Top );
				return null;
			}

			var cmd = _assistant.ParsedCommand;
			if (cmd == null) {
				// If the command does not exist and thus won't be executed, we need to clean old info in the assistant,
				// so that if user press tab right away, there wont be any confirmed nodes in the CmdParser.
				_assistant.Reset();
				return "Comando não reconhecido.";
			}

			var args = _assistant.RawArgs;
			if (args == "?")
				return cmd.Info;

			Type cmdType = cmd.GetType();
			int requiredPrivilege = (int)(cmdType.GetConst( "REQUIRED_PRIVILEGE" ) ?? 0);
			if (CurrentPrivilege < requiredPrivilege)
				return INSUFICIENT_PRIVILEGE_MESSAGE;

			try {
				var result = cmd.MainFunction( this, _assistant.ParsedArgs );
				// Clean up the data in the assistant.
				_assistant.Reset();
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				return result;
			}
			catch (Exception ex) {
				return ex.ToString();
			}
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
	}
}
