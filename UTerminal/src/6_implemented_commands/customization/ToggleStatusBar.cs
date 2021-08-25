using System;
using System.Reflection;

namespace Limcap.UTerminal.Cmds.Config {
	public class ToggleStatusBar : ACommand {

		//public static void Register( TerminalClient term ) {
		//	term.RegisterCommand( "command2", MainFunction, HelpInfo );
		//	Assembly assembly = Assembly.LoadFrom( "MyNice.dll" );
		//	Type type = assembly.GetType( "MyType" );
		//	ICommand instanceOfMyType = (ICommand) Activator.CreateInstance( type );
		//}

		public ToggleStatusBar( string locale ) : base( locale ) { }


		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_TEXT = "toggle status bar";
		//public const string HELP_INFO =
		//	"DESCRIPTION:\n" +
		//	"\tMakes the status bar visible or invisible.\n" +
		//	"USAGE:\n" +
		//	"\ttoggle status bar: [state]\n" +
		//	"PARAMETERS:\n" +
		//	"\tstate: Optional; The word 'on' or 'off'. If not provided will toggle the state of the bar";


		//public override Information InfoBuilder() => new Information(
		//	DefaultTextSource["desc"],
		//	new Parameter( DefaultTextSource["param1"], Parameter.Type.BOOLEAN, true, DefaultTextSource["param1desc"] )
		//);

		protected private override string DescriptionBuilder() { return Txt("desc"); }
		protected private override Parameter[] ParametersBuilder() { return null; }


		public override string MainFunction( Terminal t, Arg[] args ) {
			var arg = args.Get( Parameters[0] );
			if (arg == Txt("on") ) t.ShowStatusBar = true;
			else if (arg == Txt("off") ) t.ShowStatusBar = false;
			else if (args?.Length == 0) t.ShowStatusBar = !t.ShowStatusBar;
			else return Info;
			return null;
		}


		//protected override TextSource DefaultTextSource { get; set; } = new TextSource {
		//	["desc"] = "Makes the status bar visible or invisible.\n",
		//	["param1"] = "state",
		//	["param1desc"] = "on/off",
		//	["on"] = "on",
		//	["off"] = "off",
		//};
	}
}
