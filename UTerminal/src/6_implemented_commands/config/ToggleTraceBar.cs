using System;
using System.IO;

namespace Limcap.UTerminal.Cmds.Dev {
	public class ToggleTraceBar : ACommand {

		//public static void Register( TerminalClient term ) {
		//	term.RegisterCommand( "command2", MainFunction, HelpInfo );
		//	Assembly assembly = Assembly.LoadFrom( "MyNice.dll" );
		//	Type type = assembly.GetType( "MyType" );
		//	ICommand instanceOfMyType = (ICommand) Activator.CreateInstance( type );
		//}

		public ToggleTraceBar( string locale ) : base( locale ) { }


		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_TEXT = "toggle trace bar";
		public const int REQUIRED_PRIVILEGE = 1;


		protected private override string DescriptionBuilder() {
			return Txt( "desc" );
		}


		protected private override Parameter[] ParametersBuilder() {
			return new[] {
				new Parameter( Txt( "param1" ), Parameter.Type.BOOLEAN, true, Txt( "param1desc" ) )
			};
		}


		public override string MainFunction( Terminal t, Arg[] args ) {
			var arg = args.Get( Parameters[0] );
			if (arg == "on") t.ShowTraceBar = true;
			else if (arg == "off") t.ShowTraceBar = false;
			else if (args?.Length == 0) t.ShowTraceBar = !t.ShowTraceBar;
			else return Info;
			return null;
		}


		//protected override TextSource DefaultTextSource { get; set; } = new TextSource {
		//	["desc"] = "Makes the trace bar visible or invisible.\n",
		//	["param1"] = "state",
		//	["param1desc"] = "on/off",
		//	["on"] = "on",
		//	["off"] = "off",
		//};
	}
}
