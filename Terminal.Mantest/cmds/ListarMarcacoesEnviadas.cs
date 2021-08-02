using System;
using System.Reflection;

namespace Limcap.TextboxTerminal {
	public class ListarMarcacoesEnviadas : ICommand {

		//public static void Register( TerminalClient term ) {
		//	term.RegisterCommand( "command2", MainFunction, HelpInfo );

		//	Assembly assembly = Assembly.LoadFrom( "MyNice.dll" );

		//	Type type = assembly.GetType( "MyType" );

		//	ACommand instanceOfMyType = (ACommand) Activator.CreateInstance( type );
			
		//}

		public const string INVOKE_STRING = "listar marcacoes enviadas";

		public string HelpInfo { get => "Chame o comando com o parametro igual ao valor que desejar"; }
		public string MainFunction( Terminal t, string param ) {
			return "exemplo de marcacoes";
			//var report = PunchVerifier.ProcessFile( "data/Req.esv" );
			//return report;
		}
	}
}
