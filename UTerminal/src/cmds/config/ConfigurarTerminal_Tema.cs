﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Limcap.UTerminal.Cmds.TerminalConfiguration {

	public class ConfigurarTerminal_Tema : ConfigTerminal_Visuals {

		new public const string INVOKE_STRING = "configurar terminal, tema";

		public ConfigurarTerminal_Tema( string locale ) : base( locale ) { }

		public override Information GetInfo() => new Information(
			"1#Change the appearance of this terminal.",
			new Parameter( "cor-da-fonte", Parameter.Type.LETTERS, true, "2#Name of the color for the font." ),
			new Parameter( "tamanho-da-fonte", Parameter.Type.NUMBER, true, "3#Size of the font." ),
			new Parameter( "cor-do-fundo", Parameter.Type.LETTERS, true, "4#Name of the color for the background." )
		);




		public override string MainFunction( Terminal t, Args args ) {
			var arg_fontcolor = args.GetArg( Info.parameters[0] );
			if (arg_fontcolor == "") t.FontColor = Brushes.GreenYellow;
			arg_fontcolor.SafeParseBrush()?.As( b => t.FontColor = b );
			
			var arg_fontsize = args.GetArg( Info.parameters[1] );
			if (arg_fontsize == "") t.FontSize = 14;
			arg_fontsize.SafeParseInt()?.As( s => t.FontSize = s.MinMax(9,28) );

			var arg_backcolor = args.GetArg( Info.parameters[2] );
			if (arg_backcolor == "") t.BackColor = new SolidColorBrush( Color.FromRgb( 25, 25, 27 ) );
			arg_backcolor.SafeParseBrush()?.As( b => t.BackColor = b );

			return string.Empty;
		}



		//public static Translator translator;
		//public override Translator GetTranslator( string locale ) {
		//	var resourceName = GetType().Name + ".locale.json";
		//	var json = Util.LoadTextResource( resourceName );// "ConfigTerminal_Visuals.locale.json"
		//	var translationSheet = Dux.Dux.Import.FromJson( json );
		//	return new Translator( translationSheet );
		//	//return new Translator(locale) {
		//	//	["1"] = new Dictionary<string,string> { ["ptbr"] = "Altera a configuração de fonte deste terminal." },
		//	//	["2"] = new Dictionary<string, string> { ["ptbr"] = "cor" },
		//	//	["3"] = new Dictionary<string, string> { ["ptbr"] = "Nome da cor." },
		//	//	["4"] = new Dictionary<string, string> { ["ptbr"] = "tamanho" },
		//	//	["5"] = new Dictionary<string, string> { ["ptbr"] = "Tamanho da fonte." },
		//	//};
		//}
	}
}