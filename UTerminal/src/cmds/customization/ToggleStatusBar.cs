using System;
using System.Reflection;

namespace Limcap.UTerminal.Cmds {
	public class ToggleStatusBar : ACommand {

		//public static void Register( TerminalClient term ) {
		//	term.RegisterCommand( "command2", MainFunction, HelpInfo );
		//	Assembly assembly = Assembly.LoadFrom( "MyNice.dll" );
		//	Type type = assembly.GetType( "MyType" );
		//	ICommand instanceOfMyType = (ICommand) Activator.CreateInstance( type );
		//}

		public ToggleStatusBar( string locale ) : base( locale ) { }


		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_STRING = "invoke#toggle status bar";
		public const string HELP_INFO =
			"DESCRIPTION:\n" +
			"\tMakes the status bar visible or invisible.\n" +
			"USAGE:\n" +
			"\ttoggle status bar: [state]\n" +
			"PARAMETERS:\n" +
			"\tstate: Optional; The word 'on' or 'off'. If not provided will toggle the state of the bar";


		public override Information GetInfo() => new Information(
			Txt["desc"],
			new Parameter( Txt["param1"], Parameter.Type.BOOLEAN, true, Txt["param1desc"] )
		);


		public override string MainFunction( Terminal t, Args args ) {
			if (args == Txt["on"]) t.ShowStatusBar = true;
			else if (args == Txt["off"]) t.ShowStatusBar = false;
			else if (args?.Value.Length == 0) t.ShowStatusBar = !t.ShowStatusBar;
			else return HELP_INFO;
			return null;
		}


		protected override TextLibrary Txt { get; set; } = new TextLibrary {
			["desc"] = "Makes the status bar visible or invisible.\n",
			["param1"] = "state",
			["param1desc"] = "on/off",
			["on"] = "on",
			["off"] = "off",
		};
	}
}
