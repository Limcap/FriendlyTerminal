using System;
using System.IO;
using System.Reflection;

namespace Limcap.TextboxTerminal {
	public class ToggleTraceBar : ICommand {

		//public static void Register( TerminalClient term ) {
		//	term.RegisterCommand( "command2", MainFunction, HelpInfo );
		//	Assembly assembly = Assembly.LoadFrom( "MyNice.dll" );
		//	Type type = assembly.GetType( "MyType" );
		//	ACommand instanceOfMyType = (ACommand) Activator.CreateInstance( type );
		//}

		public const string INVOKE_STRING = "toggle trace bar";


		public string HelpInfo {
			get =>
				"DESCRIPTION:\n" +
				"\tMakes the trace bar visible or invisible.\n" +
				"USAGE:\n" +
				"\ttoggle trace bar: <state>\n" +
				"PARAMETERS:\n" +
				"\tstate: Optional; The word 'on' or 'off'. If not provided will toggle the state of the bar";
		}


		public string MainFunction( Terminal t, Args args ) {
			if (args == "on") t.ShowTraceBar = true;
			else if (args == "off") t.ShowTraceBar = false;
			else if (args?.Value.Length == 0) t.ShowTraceBar = !t.ShowTraceBar;
			else return HelpInfo;
			return null;
		}
	}
}
