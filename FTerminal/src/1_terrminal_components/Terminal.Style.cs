using Limcap.Dux;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Cmds = Limcap.FriendlyTerminal.Cmds;

namespace Limcap.FriendlyTerminal {

	public partial class Terminal {

		public static readonly SolidColorBrush defaultPrimaryColor = new SolidColorBrush( Color.FromRgb( 171, 255, 46 ) );
		public static readonly SolidColorBrush defaultBackgroundColor = new SolidColorBrush( Color.FromRgb( 16, 18, 18 ) );

		public double FontSize { get => _screen.DefaultFontSize; set => _screen.DefaultFontSize = value; }
		
		
		public SolidColorBrush BackgroundColor {
			get => _screen.BackgroundColor;
			set {
				if (value is null) value = defaultBackgroundColor;
				_screen.BackgroundColor = value;
			}
		}
		
		
		public SolidColorBrush FontSecondaryColor { get; private set; }
		
		
		public SolidColorBrush FontPrimaryColor {
			get => _screen.DefaultFontColor;
			set {
				if (value is null) value = defaultPrimaryColor;
				var c = value.Color;
				var newFaded = new SolidColorBrush( Color.FromArgb( 128, c.R, c.G, c.B ) );
				_screen.SwapFontColor( FontSecondaryColor, newFaded );
				_screen.DefaultFontColor = value;
				FontSecondaryColor = newFaded;
			}
		}
	}
}
