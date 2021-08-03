using System;

namespace Limcap.TextboxTerminal {
	public class Raise : ICommand {

		public const string INVOKE_STRING = "raise";

		public string HelpInfo => "Ask for a password to raise your privileges in this terminal.";

		public string MainFunction( Terminal t, Args args ) {
			t.TypeText( "Password: " );
			t.ReadPassword( input => {
				if (input == "admin1234") {
					t.vars.Add( new Dux.DuxValue( "privilegios", "1", null ) );
					return "Privilege level: 1";
				}
				t.vars["privilegios"] = "0";
				return "Nível de privilégios: 0";
			} );
			return null;
		}
	}
}
