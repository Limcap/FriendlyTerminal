using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Limcap.FTerminal.Cmds.Dev {
	public class Print_a_lot : ACommand {

		//public static void Register( TerminalClient term ) {
		//	term.RegisterCommand( "command2", MainFunction, HelpInfo );
		//	Assembly assembly = Assembly.LoadFrom( "MyNice.dll" );
		//	Type type = assembly.GetType( "MyType" );
		//	ICommand instanceOfMyType = (ICommand) Activator.CreateInstance( type );
		//}


		public const string INVOKE_TEXT = "dev, print-a-lot";


		public const int REQUIRED_PRIVILEGE = 2;


		//public const string HELP_INFO =
		//	"DESCRIPTION:\n" +
		//	"\tVery long repetitive text.";


		public Print_a_lot( string locale ) : base( locale ) {}


		public override string MainFunction( Terminal t, Arg[] args ) {
			var sb = new StringBuilder();
			for (int i = 0; i < 2000; i++)
				sb.Append( "this is a hundred characters long string, with the solo purpose of testing long strings. the end...\n" );
			return sb.ToString();
		}


		protected override string DescriptionBuilder() => "Very long repetitive text.";


		protected override Parameter[] ParametersBuilder() => null;
	}
}
