using Limcap.UTerminal;
using Limcap.Dux;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Limcap.UTerminal.Cmds {
	public class Help : ICommand {

		public const string INVOKE_STRING = "help";

		public const string HELP_INFO =
			"DESCRIPTION:\n" +
			"   list all commands available in this terminal.\n";

		public static readonly Param[] PARAMETERS = new Param[] {
			new Param{name="hhh",type="number",description="aaa"},
			new Param{name="aaa",type="number",description="aaa"},
			new Param{name="aaa",type="number",description="aaa"},
			new Param("nome",ParamType.ALPHANUMERIC,false,"descricao"),
		};

		public string MainFunction( Terminal t, Args args ) {
			string output = "\nThe following commands are available in this terminal. For more information on any of them,\n" +
				"type the corresponding command followed by :? (colon and question mark)\n\n";
				//"Available commands:\n";
			//·•▸►▪▫
			var cmds = string.Join( "\n • ", t.AvailableCommands );
			output += cmds.Length == 0 ? "No command available" : (" • " + cmds);
			return output + "\n\n";
		}
	}


}
