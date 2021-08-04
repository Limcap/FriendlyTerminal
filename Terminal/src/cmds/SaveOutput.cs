using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Limcap.TextboxTerminal.Cmds {
	public class SaveOutput : ICommand {

		public const string INVOKE_STRING = "save output";


		public const string HELP_INFO =
			"DESCRIPTION:\n" +
			"Saves the current text on screen to a file.\n" +
			"USAGE:\n" +
			"\tsave output: <filename>\n" +
			"PARAMETERS:\n" +
			"\tfilename: Desired name of file to save without extension. alphanumeric and underline only.";


		public string MainFunction( Terminal t, Args args ) {
			string filename = args;
			if (args.Value.Length == 0) 
				return HELP_INFO;
			else if (!filename.All( c => char.IsLetterOrDigit( c ) || c == '_' )) {
				return "O nome do arquivo escolhido é inválido.\nDeve conter somente letras, números ou o caractere 'underline' ";
			}
			else {
				File.WriteAllText( filename + ".txt", t.Text );
				return "File saved: " + filename + ".txt";
			}
		}
	}
}
