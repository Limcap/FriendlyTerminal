using Limcap.Dux;
using Limcap.UTerminal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Limcap.UTerminal.Cmds.Misc {
	public class Help : ACommand {

		public Help( string locale ) : base( locale ) { }

		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_STRING = "invoke#help";


		//public override Information GetInfo() => new Information(
		//	"1#Displays help information on the usage of this terminal."
		//);


		//public override string MainFunction( Terminal t, Args args ) {
		//	var part1 = CmdTranslator.Translate( "2#\nThe following commands are available in this terminal. For more information on any of them,\n" +
		//		"type the corresponding command followed by :? (colon and question mark)\n\n" );
		//	var part2 = CmdTranslator.Translate( "3#No command available" );

		//	string output = part1;
		//	//"Available commands:\n";
		//	//·•▸►▪▫
		//	var cmds = string.Join( "\n • ", t.AvailableCommands );
		//	output += cmds.Length == 0 ? part2 : (" • " + cmds);
		//	return output + "\n\n";
		//}


		public override Information GetInfo() => new Information( Txt["desc"] );


		public override string MainFunction( Terminal t, Args args ) {
			string output = NEW_LINE + Txt["intro"];
			var cmds = string.Join( NEW_LINE + " • ", t.AvailableCommands );
			output += cmds.Length == 0 ? Txt["no-cmds"] : (" • " + cmds);
			return output + NEW_LINE + NEW_LINE;
		}


		protected override TextLibrary Txt { get; set; } = new TextLibrary() {
			["desc"] = "Displays help information on the usage of this terminal.",
			["intro"] = "The following commands are available in this terminal. For more information on any of them,\ntype the corresponding command followed by :? (colon and question mark)\n\n",
			["no-cmds"] = "No command available",
		};
	}
}
