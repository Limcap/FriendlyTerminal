using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
				if (type.IsSubclassOf( typeof( ACommand ) ) && defaultLocale != Locale)
					invokeText = GetTranslatedInvokeText( type, Locale );
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




		public string ProcessInput( string input ) {
			int splitter = Math.Max( 0, input.IndexOf( ':' ) );
			var cmd = splitter == 0 ? input : input.Remove( splitter ).Trim();
			var arg = splitter == 0 ? string.Empty : input.Substring( splitter + 1 ).Trim();

			if (cmd == "exit") {
				Clear();
				onExit?.Invoke();
				return string.Empty;
			}
			if (cmd == "clear") {
				Clear();
				return null;
			}
			if (!_cmdList.ContainsKey( cmd )) {
				return "Comando não reconhecido.";
			}
			else {
				Type cmdType = _cmdList[cmd];
				int requiredPrivilege = (int)(cmdType.GetConst( "REQUIRED_PRIVILEGE" ) ?? 0);
				if (arg == "?")
					return GetCommandInfo( cmdType );
				if (CurrentPrivilege < requiredPrivilege)
					return INSUFICIENT_PRIVILEGE_MESSAGE;
				var instance = cmdType.IsSubclassOf( typeof( ACommand ) )
					? (ICommand)Activator.CreateInstance( cmdType, Locale )
					: (ICommand)Activator.CreateInstance( cmdType );
				try {
					return instance.MainFunction( this, arg );
				}
				catch (Exception ex) {
					return ex.ToString();
				}
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
