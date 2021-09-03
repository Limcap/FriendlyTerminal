using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Limcap.FriendlyTerminal.Cmds.Customization {

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


		protected override string DescriptionBuilder() {
			return Txt( "desc" );
		}


		protected override Parameter[] ParametersBuilder() {
			return new Parameter[] {
				new Parameter( Txt("param1"), Parameter.Type.NUMBER, true, Txt("param1desc") ),
				new Parameter( Txt("param2"), Parameter.Type.ALPHANUMERIC, true, Txt("param2desc") ),
				new Parameter( Txt("param3"), Parameter.Type.ALPHANUMERIC, true, Txt("param3desc") )
			};
		}




		public override string MainFunction( Terminal t, Arg[] args ) {
			var arg_fontsize = args.Get( Parameters[0] );
			var arg_fontcolor = args.Get( Parameters[1] );
			var arg_backcolor = args.Get( Parameters[2] );

			if (arg_fontsize == "reset") t.FontSize = 14;
			else arg_fontsize.SafeParseInt()?.As( s => t.FontSize = s );

			if (!Ext.IsNullOrEmpty( arg_fontcolor )) {
				if (arg_fontcolor == "reset")
					t.FontPrimaryColor = null;
				else if (isRGB( arg_fontcolor )) {
					var rgb = ParseRGB( arg_fontcolor );
					if (rgb.r > 0 | rgb.g > 0 | rgb.b > 0) t.FontPrimaryColor = new SolidColorBrush( Color.FromRgb( rgb.r, rgb.g, rgb.b ) );
				}
				else {
					var brush = arg_fontcolor.SafeParseBrush();
					if (brush is null) t.TypeText( $"\nO valor definido em '{Parameters[1].name}' é inválido." );
					else t.FontPrimaryColor = brush as SolidColorBrush;
				}
			}

			if (!Ext.IsNullOrEmpty( arg_backcolor )) {
				if (arg_backcolor == "reset")
					t.BackgroundColor = null;
				else if (isRGB( arg_backcolor )) {
					var rgb = ParseRGB( arg_backcolor );
					if (rgb.r > 0 | rgb.g > 0 | rgb.b > 0) t.BackgroundColor = new SolidColorBrush( Color.FromRgb( rgb.r, rgb.g, rgb.b ) );
				}
				else {
					var brush = arg_backcolor.SafeParseBrush();
					if (brush is null) t.TypeText( $"\nO valor definido em '{Parameters[2].name}' é inválido." );
					else t.BackgroundColor = brush as SolidColorBrush;
				}
			}
			//arg_backcolor.SafeParseBrush()?.As( b => t.BackgroundColor = b as SolidColorBrush );

			return null;
		}




		private bool isRGB( string value ) {
			if (value is null) return false;
			Regex rgbPattern = new Regex( @"^\d+\s*;\s*\d+\s*;\s*\d+\s*\z" );
			var r = rgbPattern.IsMatch( value );
			return r;
		}




		private (byte r, byte g, byte b) ParseRGB( string value ) {
			var sli = ((PString)value).GetSlicer( ';' );
			byte r=0, g=0, b=0;
			byte.TryParse( sli.Next().Trim().AsString, out r );
			byte.TryParse( sli.Next().Trim().AsString, out g );
			byte.TryParse( sli.Next().Trim().AsString, out b );
			return (r, g, b);
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
