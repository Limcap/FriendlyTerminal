using System;
using System.Reflection;

namespace Limcap.TextboxTerminal {
	public class ToggleStatusBar : ICommand {

		//public static void Register( TerminalClient term ) {
		//	term.RegisterCommand( "command2", MainFunction, HelpInfo );
		//	Assembly assembly = Assembly.LoadFrom( "MyNice.dll" );
		//	Type type = assembly.GetType( "MyType" );
		//	ACommand instanceOfMyType = (ACommand) Activator.CreateInstance( type );
		//}

		public const string INVOKE_STRING = "toggle status bar";


		public string HelpInfo {
			get => 
				"DESCRIPTION:\n" +
				"\tMakes the status bar visible or invisible.\n" +
				"USAGE:\n" +
				"\ttoggle status bar: [state]\n" +
				"PARAMETERS:\n" +
				"\tstate: Optional; The word 'on' or 'off'. If not provided will toggle the state of the bar";
		}


		public string MainFunction( Terminal t, Args args ) {
			if (args == "on") t.ShowStatusBar = true;
			else if (args == "off") t.ShowStatusBar = false;
			else if (args?.Value.Length == 0) t.ShowStatusBar = !t.ShowStatusBar;
			else return HelpInfo;
			return null;
		}
	}
}
