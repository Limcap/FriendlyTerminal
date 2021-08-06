using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.UTerminal {
	public partial class Terminal {

		private List<ICommand> _cmds = new List<ICommand>();
		private Dictionary<string, Type> _cmdList;




		public void RegisterCommand<T>( string invokeString = null ) where T : ICommand, new() {
			RegisterCommand( typeof( T ), invokeString );
		}




		public void RegisterAllCommandsInNamespace( string nspace ) {
			//var q = from t in Assembly.GetExecutingAssembly().GetTypes()
			//		  where t.IsClass && t.Namespace == nspace
			//		  select t;
			var type = typeof( ICommand );
			var types = AppDomain.CurrentDomain.GetAssemblies()
				 .SelectMany( s => s.GetTypes() )
				 .Where( p => type.IsAssignableFrom( p ) && p.Namespace == nspace );
			foreach (var t in types) RegisterCommand( t );
		}




		private void RegisterCommand( Type cmdType, string invokeString = null ) {
			if (string.IsNullOrEmpty( invokeString )) {
				invokeString = cmdType.GetConst( "INVOKE_STRING" ) as string ?? cmdType.Name;
				//invokeString = (string)cmdType.GetField( "INVOKE_STRING" )?.GetValue( null );
				//if (invokeString is null) invokeString = cmdType.Name;
			}
			_cmdList.Add( invokeString, cmdType );
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
					//return cmdType.GetConst("HELP_INFO") as string ?? "Este comando não possui informação de ajuda.";
				if (CurrentPrivilege < requiredPrivilege)
					return INSUFICIENT_PRIVILEGE_MESSAGE;
				var instance = (ICommand)Activator.CreateInstance( cmdType );
				try {
					return instance.MainFunction( this, arg );
				}
				catch( Exception ex ) {
					return ex.ToString();
				}
			}
		}




		public string GetCommandInfo( Type t ) {
			if (t.IsSubclassOf( typeof( ACommand ) )) {
				var instance = Activator.CreateInstance( t ) as ACommand;
				//return instance?.TranslateInfo( Locale ).ToString();
				var translator = instance.LoadTranslator( Locale );
				var translatedInfo = translator.Translate( instance.Info );
				return translatedInfo.ToString();
			}
			else if (t.IsAssignableFrom( typeof( ICommand ) ))
				return t.GetConst( "HELP_INFO" ) as string ?? "Este comando não possui informação de ajuda.";
			return null;
		}
	}
}
