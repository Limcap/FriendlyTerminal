using System;
using System.IO;

namespace Limcap.FriendlyTerminal.Cmds.Customization {
	public class ToggleStatusBar : ACommand {

		//public static void Register( TerminalClient term ) {
		//	term.RegisterCommand( "command2", MainFunction, HelpInfo );
		//	Assembly assembly = Assembly.LoadFrom( "MyNice.dll" );
		//	Type type = assembly.GetType( "MyType" );
		//	ICommand instanceOfMyType = (ICommand) Activator.CreateInstance( type );
		//}

		public ToggleStatusBar( string locale ) : base( locale ) { }


		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_TEXT = "terminal, config status-bar";
		public const int REQUIRED_PRIVILEGE = 1;


		protected override string DescriptionBuilder() {
			return Txt( "desc" );
		}


		protected override Parameter[] ParametersBuilder() {
			return new[] {
				new Parameter( Txt( "param1" ), Parameter.Type.BOOLEAN, true, Txt( "param1desc" ) )
			};
		}


		public override string MainFunction( Terminal t, Arg[] args ) {
			var arg = args.GetString( Parameters[0] );
			if (arg == Txt("on")) t.ShowStatusBar = true;
			else if (arg == Txt("off")) t.ShowStatusBar = false;
			else if (args?.Length == 0) t.ShowStatusBar = !t.ShowStatusBar;
			else t.TypeText( Info );
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
