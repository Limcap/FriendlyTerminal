using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Limcap.FTerminal.Cmds.Basic {
	public class SaveOutput : ACommand {

		public SaveOutput( string locale ) : base( locale ) { }


		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_TEXT = "terminal, save-output";


		//public const string HELP_INFO =
		//	"DESCRIPTION:\n" +
		//	"Saves the current text on screen to a file.\n" +
		//	"USAGE:\n" +
		//	"\tsave output: <filename>\n" +
		//	"PARAMETERS:\n" +
		//	"\tfilename: Desired name of file to save. Only Letters, numbers, underline and dots are accepted.";


		//public override Information InfoBuilder() => new Information(
		//	DefaultTextSource["desc"],
		//);

		protected private override string DescriptionBuilder() {
			return Txt( "desc" );
		}
		

		protected private override Parameter[] ParametersBuilder() {
			return new[]{
				new Parameter( Txt("param1"), Parameter.Type.ALPHANUMERIC, false, Txt("param1desc") )
			};
		}


		public override string MainFunction( Terminal t, Arg[] args ) {
			var filename = args.Get(Parameters[0]);
			if (args.Length == 0)
				return Info;
			else if (!filename.All( c => char.IsLetterOrDigit( c ) || c == '_' || c == '.' )) {
				return Txt("invalid");
			}
			else {
				File.WriteAllText( filename + ".txt", t.Text );
				return Txt("success") + filename + ".txt";
			}
		}


		//protected override TextSource DefaultTextSource { get; set; } = new TextSource() {
		//	["desc"] = "Saves the current text on screen to a file.",
		//	["param1"] = "filename",
		//	["param1desc"] = "Name of the file to be saved",
		//	["invalid"] = "The chosen name is invalid. Only letters, numbers, underline or dot are acceptable characters.",
		//	["success"] = "File saved: "
		//};


		//protected override string[] Txt { get; set; } ={
		//	"desc#Saves the current text on screen to a file.",
		//	"param1#filename",
		//	"param1desc#Name of the file to be saved",
		//	"invalid#The chosen name is invalid. Only letters, numbers, underline or dot are acceptable characters.",
		//	"success#File saved: "
		//};
	}
}
