using System;
using System.Reflection;

namespace Limcap.FTerminal.Cmds.Customization {
	public class ToggleAssistBar : ACommand {

		//public static void Register( TerminalClient term ) {
		//	term.RegisterCommand( "command2", MainFunction, HelpInfo );
		//	Assembly assembly = Assembly.LoadFrom( "MyNice.dll" );
		//	Type type = assembly.GetType( "MyType" );
		//	ICommand instanceOfMyType = (ICommand) Activator.CreateInstance( type );
		//}

		public ToggleAssistBar( string locale ) : base( locale ) { }


		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_TEXT = "terminal, config assist-bar";
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

		protected override string DescriptionBuilder() { return Txt("desc"); }
		protected override Parameter[] ParametersBuilder() {
			return new[] {
				new Parameter( Txt( "param1" ), Parameter.Type.BOOLEAN, true, Txt( "param1desc" ) )
			};
		}


		public override string MainFunction( Terminal t, Arg[] args ) {
			var arg = args.Get( Parameters[0] );
			if (arg == Txt("on") ) t.ShowAssistBar = true;
			else if (arg == Txt("off") ) t.ShowAssistBar = false;
			else if (args?.Length == 0) t.ShowAssistBar = !t.ShowAssistBar;
			else t.TypeText( Info );
			return null;
		}


		//protected override TextSource DefaultTextSource { get; set; } = new TextSource {
		//	["desc"] = "Makes the assist bar visible or invisible.\n",
		//	["param1"] = "state",
		//	["param1desc"] = "on/off",
		//	["on"] = "on",
		//	["off"] = "off",
		//};
	}
}
