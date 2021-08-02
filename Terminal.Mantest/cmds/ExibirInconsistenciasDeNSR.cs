using System;
using System.IO;
using System.Reflection;

namespace Limcap.TextboxTerminal {
	public class ExibirInconsistenciasDeNsr : ICommand {

		//public static void Register( TerminalClient term ) {
		//	term.RegisterCommand( "command2", MainFunction, HelpInfo );

		//	Assembly assembly = Assembly.LoadFrom( "MyNice.dll" );

		//	Type type = assembly.GetType( "MyType" );

		//	ACommand instanceOfMyType = (ACommand) Activator.CreateInstance( type );
			
		//}

		public const string INVOKE_STRING = "exibir inconsistencias de nsr";

		public string HelpInfo { get => "Chame o comando com o parametro igual ao valor que desejar"; }
		public string MainFunction( Terminal t, string param ) {
			//if (File.Exists( Config.appDataFolder + "/UltimosNSRsEnviados.cfg" )) {
			//	var txt = File.ReadAllText( Config.appDataFolder + "/UltimosNSRsEnviados.cfg" );
			//	return txt;
			//}
			return "Nenhuma inconsistência.";
		}
	}
}
