using System;
using System.Collections.Generic;

namespace Limcap.UTerminal.Cmds {

	public class Raise : ICommand {

		public const string INVOKE_STRING = "raise";

		public const string HELP_INFO = "Raises your user privilege in this terminal.";

		public string MainFunction( Terminal t, Args args ) {
			t.TypeText( "Password: " );
			t.ReadPassword( input => {
				var level = levels.IndexOf( input ) + 1;
				t.CurrentPrivilege = level;
				return "Privilege level: " + level;
				//if (input == "admin1234") {
				//	t.vars.Add( new Dux.DuxValue( "privilegios", "1", null ) );
				//	return "Privilege level: 1";
				//}
				//t.vars["privilegios"] = "0";
				//return "Nível de privilégios: 0";
			} );
			return null;
		}

		private readonly List<string> levels = new List<string>() { "admin1234", "techsupport" };
	}
}
