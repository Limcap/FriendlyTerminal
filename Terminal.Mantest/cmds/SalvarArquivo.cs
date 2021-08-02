using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Limcap.TextboxTerminal {
	public class SalvarArquivo : ICommand {

		public const string INVOKE_STRING = "salvar arquivo";

		public string HelpInfo { get => "Chame o comando com o parametro igual ao valor que desejar"; }
		public string MainFunction( Terminal t, string filename ) {
			if (!filename.All( c => char.IsLetterOrDigit( c ) || c == '_' )) {
				return "O nome do arquivo escolhido é inválido.\nDeve conter somente letras, números ou o caractere 'underline' ";
			}
			else {
				//File.WriteAllText( Config.DATAPATH + filename + ".txt", t.Text );
				//return "Arquivo salvo: " + Config.DATAPATH + filename + ".txt";
				File.WriteAllText( filename + ".txt", t.Text );
				return "Arquivo salvo: " + filename + ".txt";
			}
		}
	}
}
