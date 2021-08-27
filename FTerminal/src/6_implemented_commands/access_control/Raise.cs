using System;
using System.Collections.Generic;

namespace Limcap.FTerminal.Cmds.AccessControl {

	public class Raise : ACommand {
		public Raise( string locale ) : base( locale ) { }


		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_TEXT = "raise";
		public const string HELP_INFO = "Raises your user privilege in this terminal.";


		protected override string DescriptionBuilder() {
			return Txt("desc");
		}
		
		
		protected override Parameter[] ParametersBuilder() {
			return null;
		}


		public override string MainFunction( Terminal t, Arg[] args ) {
			t.TypeText( Txt("0","Password") + ": " );
			t.ReadPassword( input => {
				var level = _levels.IndexOf( input ) + 1;
				t.CurrentPrivilege = level;
				return Txt("1","Privilege level") + ": " + level;
			} );
			return null;
		}


		private readonly List<string> _levels = new List<string>() { "admin1234", "techsupport" };


		//protected override TextSource DefaultTextSource { get; set; } = new TextSource {
		//	["desc"] = "Raises your user privilege in this terminal.",
		//	["0"] = "Password: ",
		//	["1"] = "Privilege level: "
		//};
	}
}
