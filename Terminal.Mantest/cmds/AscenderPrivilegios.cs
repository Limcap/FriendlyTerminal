using System;

namespace Limcap.TextboxTerminal {
	public class AscenderPrivilegios : ICommand {

		public const string INVOKE_STRING = "ascender privilegios";

		public string HelpInfo => "Ajuda do comando 1";

		public string MainFunction( Terminal t, string args ) {
			return "comando 1 executado";
			t.TypeText( "Digite a senha: " );
			t.ReadLine( input => {
				if (input == "admin1234") {
					t.vars.Add( new Dux.DuxValue( "privilegios", "1", null ) );
					return "Nível de privilégios: 1";
				}
				t.vars["privilegios"] = "0";
				return "Nível de privilégios: 0";
			} );
			return null;
		}
	}
}
