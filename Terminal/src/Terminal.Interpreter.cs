using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.UTerminal {
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

			if (string.IsNullOrEmpty( invokeString )) {
				var cmdType = typeof( T );
				//invokeString = (string)cmdType.GetField( "INVOKE_STRING" )?.GetValue( null );
				//if (invokeString is null) invokeString = cmdType.Name;
				invokeString = cmdType.GetConst( "INVOKE_STRING" ) as string ?? cmdType.Name;
			}
			_cmdList.Add( invokeString, typeof( T ) );
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
					return cmdType.GetConst("HELP_INFO") as string ?? "Este comando não possui informação de ajuda.";
				if (CurrentPrivilege < requiredPrivilege)
					return INSUFICIENT_PRIVILEGE_MESSAGE;
				var instance = (ICommand)Activator.CreateInstance( cmdType );
				return instance.MainFunction( this, arg );
			}
		}
	}
}
