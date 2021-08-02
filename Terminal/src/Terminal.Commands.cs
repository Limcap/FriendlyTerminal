using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.TextboxTerminal {
	public partial class Terminal {

		private List<ICommand> _cmds;
		private Dictionary<string, Type> _cmdList;

		public void RegisterCommands( params ICommand[] cmds ) {
			_cmds = new List<ICommand>( cmds );
		}

		public void RegisterCommand<T>( string invokeString=null ) where T : ICommand, new() {
			_cmds = _cmds ?? new List<ICommand>();

			if (invokeString is null || invokeString.Length == 0)
				invokeString = (string)typeof( T ).GetField( "INVOKE_STRING" ).GetValue( null );
			_cmdList.Add( invokeString, typeof( T ) );
			//T instance = (T)Activator.CreateInstance( typeof(T) );
		}

		public void RegisterCommand( string invokeName, Func<string, string> function, string help ) {

		}



		//private Dictionary<string, Tuple<Func<string, string>, string>> _cmdList;




		public string ProcessInput( string input ) {
			int splitter = Math.Max( 0, input.IndexOf( ':' ) );
			var cmd = splitter == 0 ? input : input.Remove( splitter ).Trim();
			var arg = splitter == 0 ? string.Empty : input.Substring( splitter+1 ).Trim();

			if (cmd == "exit") {
				Clear();
				_onExit();
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
				var instance = (ICommand) Activator.CreateInstance( _cmdList[cmd] );
				return instance.MainFunction(this,arg);
			}

			//var param = parts.Length > 1 ? parts[1].Trim() : string.Empty;

			//if (cmd == "salvar arquivo" ) return SalvarArquivo( param );
			//if (cmd == "exibir marcacoes enviadas") return ListarEnvios();
			//if (cmd == "exibir inconsistencias de nsr") return ExibirInconsistencias();
			//if (cmd == "ascender privilegios") return AscenderPrivilegios();
		}
	}
}
