using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Limcap.UTerminal.Cmds {
	public class SaveOutput : ACommand {

		public SaveOutput( string locale ) : base( locale ) { }


		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_STRING = "invoke#save output";


		public const string HELP_INFO =
			"DESCRIPTION:\n" +
			"Saves the current text on screen to a file.\n" +
			"USAGE:\n" +
			"\tsave output: <filename>\n" +
			"PARAMETERS:\n" +
			"\tfilename: Desired name of file to save. Only Letters, numbers, underline and dots are accepted.";


		public override Information GetInfo() => new Information(
			Txt["desc"],
			new Parameter( Txt["param1"], Parameter.Type.ALPHANUMERIC, false, Txt["param1desc"] )
		);


		public override string MainFunction( Terminal t, Args args ) {
			string filename = args;
			if (args.Value.Length == 0)
				return HELP_INFO;
			else if (!filename.All( c => char.IsLetterOrDigit( c ) || c == '_' || c == '.' )) {
				return Txt["invalid"];
			}
			else {
				File.WriteAllText( filename + ".txt", t.Text );
				return Txt["success"] + filename + ".txt";
			}
		}


		protected override TextLibrary Txt { get; set; } = new TextLibrary() {
			["desc"] = "Saves the current text on screen to a file.",
			["param1"] = "filename",
			["param1desc"] = "Name of the file to be saved",
			["invalid"] = "The chosen name is invalid. Only letters, numbers, underline or dot are acceptable characters.",
			["success"] = "File saved: "
		};


		//protected override string[] Txt { get; set; } ={
		//	"desc#Saves the current text on screen to a file.",
		//	"param1#filename",
		//	"param1desc#Name of the file to be saved",
		//	"invalid#The chosen name is invalid. Only letters, numbers, underline or dot are acceptable characters.",
		//	"success#File saved: "
		//};
	}
}
