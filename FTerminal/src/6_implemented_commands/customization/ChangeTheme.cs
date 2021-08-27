using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Limcap.FTerminal.Cmds.Customization {

	public class ChangeTheme : ACommand {
		public ChangeTheme( string locale ) : base( locale ) { }

		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_TEXT = "terminal, change-theme";


		//public override Information GetInfo() => new Information(
		//	"1#Change the appearance of this terminal.",
		//	new Parameter( "5#fontcolor", Parameter.Type.LETTERS, true, "2#Name of the color for the font." ),
		//	new Parameter( "6#fontsize", Parameter.Type.NUMBER, true, "3#Size of the font." ),
		//	new Parameter( "7#backcolor", Parameter.Type.LETTERS, true, "4#Name of the color for the background." )
		//);

		//public override Information InfoBuilder() => new Information(
		//	DefaultTextSource["desc"],
		//	new Parameter( DefaultTextSource["param1"], Parameter.Type.LETTERS, true, DefaultTextSource["param1desc"] ),
		//	new Parameter( DefaultTextSource["param2"], Parameter.Type.NUMBER, true, DefaultTextSource["param2desc"] ),
		//	new Parameter( DefaultTextSource["param3"], Parameter.Type.LETTERS, true, DefaultTextSource["param3desc"] )
		//);


		protected private override string DescriptionBuilder() {
			return Txt( "desc" );
		}


		protected private override Parameter[] ParametersBuilder() {
			return new Parameter[] {
				new Parameter( Txt("param1"), Parameter.Type.LETTERS, true, Txt("param1desc") ),
				new Parameter( Txt("param2"), Parameter.Type.NUMBER, true, Txt("param2desc") ),
				new Parameter( Txt("param3"), Parameter.Type.LETTERS, true, Txt("param3desc") )
			};
		}
		

		public override string MainFunction( Terminal t, Arg[] args ) {
			var arg_fontsize = args.Get( Parameters[0] );
			var arg_fontcolor = args.Get( Parameters[1] );
			var arg_backcolor = args.Get( Parameters[2] );

			if (arg_fontsize == "") t.FontSize = 14;
			if (arg_fontcolor == "") t.FontColor = t.ColorF1;
			if (arg_backcolor == "") t.BackColor = t.ColorB1;//new SolidColorBrush( Color.FromRgb( 25, 25, 27 ) );

			arg_fontsize.SafeParseInt()?.As( s => t.FontSize = s.MinMax(9,28) );
			arg_fontcolor.SafeParseBrush()?.As( b => t.FontColor = b );
			arg_backcolor.SafeParseBrush()?.As( b => t.BackColor = b );

			return string.Empty;
		}


		//protected override TextSource DefaultTextSource { get; set; } = new TextSource() {
		//	["desc"] = "Change the appearance of this terminal.",
		//	["param1"] = "fontsize",
		//	["param1desc"] = "Size of the font.",
		//	["param2"] = "fontcolor",
		//	["param2desc"] = "Name of the color for the font.",
		//	["param3"] = "backcolor",
		//	["param3desc"] = "Name of the color for the background.",
		//};
	}
}
