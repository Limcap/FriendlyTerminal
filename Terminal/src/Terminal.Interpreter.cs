using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.TextboxTerminal {
	public partial class Terminal {

		private List<ICommand> _cmds;
		private Dictionary<string, Type> _cmdList;




		//public void RegisterCommands( params ICommand[] cmds ) {
		//	_cmds = new List<ICommand>( cmds );
		//}
		//public void RegisterCommand( string invokeName, Func<string, string> function, string help ) {

		//}




		public void RegisterCommand<T>( string invokeString = null ) where T : ICommand, new() {
			_cmds = _cmds ?? new List<ICommand>();

			if (invokeString is null || invokeString.Length == 0) {
				try {
					invokeString = (string)typeof( T ).GetField( "INVOKE_STRING" ).GetValue( null );
				}
				catch {
					invokeString = typeof( T ).Name;
				}
			}
			_cmdList.Add( invokeString, typeof( T ) );
			//T instance = (T)Activator.CreateInstance( typeof(T) );
		}






		public string ProcessInput( string input ) {
			int splitter = Math.Max( 0, input.IndexOf( ':' ) );
			var cmd = splitter == 0 ? input : input.Remove( splitter ).Trim();
			var arg = splitter == 0 ? string.Empty : input.Substring( splitter+1 ).Trim();

			if (cmd == "exit") {
				Clear();
				onExit?.Invoke();
				return string.Empty;
			}
			if (cmd == "clear") {
				Clear();
				return null;
			}
			if ( !_cmdList.ContainsKey( cmd ) ) {
				return "Comando não reconhecido.";
			}
			else {
				Type cmdType = _cmdList[cmd];
				try {
					int requiredPrivilege = (int)cmdType.GetField( "REQUIRED_PRIVILEGE" ).GetValue( null );
					if (CurrentPrivilege < requiredPrivilege) return INSUFICIENT_PRIVILEGE_MESSAGE;
				}
				catch { }
				
				var instance = (ICommand) Activator.CreateInstance( _cmdList[cmd] );
				return instance.MainFunction(this,arg);
			}
		}
	}
}
