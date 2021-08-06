using Limcap.UTerminal;
using Limcap.Dux;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Limcap.UTerminal.Cmds {
	public class Help : ACommand {

		public Help( string locale ) : base( locale ) {}

		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_STRING = "0#help";


		public override Information GetInfo() => new Information(
			"1#Displays help information on the usage of this terminal."
		);


		public override string MainFunction( Terminal t, Args args ) {
			var part1 = CmdTranslator.Translate( "2#\nThe following commands are available in this terminal. For more information on any of them,\n" +
				"type the corresponding command followed by :? (colon and question mark)\n\n" );
			var part2 = CmdTranslator.Translate( "3#No command available" );

			string output = part1;
			//"Available commands:\n";
			//·•▸►▪▫
			var cmds = string.Join( "\n • ", t.AvailableCommands );
			output += cmds.Length == 0 ? part2 : (" • " + cmds);
			return output + "\n\n";
		}
	}
}
