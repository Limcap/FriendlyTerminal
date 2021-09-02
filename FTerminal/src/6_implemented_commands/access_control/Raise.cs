using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Limcap.FriendlyTerminal.Cmds.AccessControl {

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
			//t.TypeText( "Aqua ", Brushes.Aqua );
			//t.TypeText( "Pink ", Brushes.Pink );
			//t.TypeText( "Default " );
			t.ReadPassword( input => Callback1( t, input ) );
			return null;
		}


		private readonly List<string> _levels = new List<string>() { "admin1234", "techsupport" };


		private string Callback1( Terminal t, string input ) {
			var level = _levels.IndexOf( input ) + 1;
			t.CurrentPrivilege = level;
			t.TypeText( NEW_LINE );
			t.TypeText( Txt( "1", "Privilege level" ) + ": " + level );
			return null;
		}


		//protected override TextSource DefaultTextSource { get; set; } = new TextSource {
		//	["desc"] = "Raises your user privilege in this terminal.",
		//	["0"] = "Password: ",
		//	["1"] = "Privilege level: "
		//};
	}
}
