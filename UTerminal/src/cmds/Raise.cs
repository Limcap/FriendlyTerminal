using System;
using System.Collections.Generic;

namespace Limcap.UTerminal.Cmds {

	public class Raise : ACommand {
		public Raise( string locale ) : base( locale ) { }


		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_STRING = "invoke#raise";
		public const string HELP_INFO = "Raises your user privilege in this terminal.";


		public override Information GetInfo() => new Information( Txt["desc"] );


		public override string MainFunction( Terminal t, Args args ) {
			t.TypeText( Txt["0"] );
			t.ReadPassword( input => {
				var level = levels.IndexOf( input ) + 1;
				t.CurrentPrivilege = level;
				return Txt["1"] + level;
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


		protected override TextLibrary Txt { get; set; } = new TextLibrary {
			["desc"] = "Raises your user privilege in this terminal.",
			["0"] = "Password: ",
			["1"] = "Privilege level: "
		};
		//protected override string[] Txt { get; set; } = {
		//	"desc#Raises your user privilege in this terminal.",
		//	"0#Password: ",
		//	"1#Privilege level: "
		//};
	}
}
