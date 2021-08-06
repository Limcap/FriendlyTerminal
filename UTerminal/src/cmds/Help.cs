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
		
		
		public const string INVOKE_STRING = "help";


		//public const string INVOKE_STRING = "help;pt-br:ajuda";


		public override Information GetInfo() => new Information(
			"Displays help information on the usage of this terminal."
		);


		public override string MainFunction( Terminal t, Args args ) {
			string output = "\nThe following commands are available in this terminal. For more information on any of them,\n" +
				"type the corresponding command followed by :? (colon and question mark)\n\n";
			//"Available commands:\n";
			//·•▸►▪▫
			var cmds = string.Join( "\n • ", t.AvailableCommands );
			output += cmds.Length == 0 ? "No command available" : (" • " + cmds);
			return output + "\n\n";
		}

		//public override Translator GetTranslator( string locale ) => new Translator(locale) {};
	}
}
