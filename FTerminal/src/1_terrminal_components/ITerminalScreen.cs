using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Limcap.FriendlyTerminal {
	public interface ITerminalScreen {
		SolidColorBrush BackgroundColor { get; set; }
		SolidColorBrush DefaultFontColor { get; set; }
		SolidColorBrush BufferFontColor { get; set; }
		double DefaultFontSize { get; set; }
		string Buffer { get; set; }
		bool IsEmpty { get; }
		Control UIControlHook { get; }

		event KeyEventHandler OnPreviewKeyDown;

		ITerminalScreen NewBlock( Brush color = null );
		ITerminalScreen NewBuffer( Brush color = null );
		ITerminalScreen NewColor( Brush color );
		ITerminalScreen AppendText( string text );
		//void NewBlock( Brush color, string text = null );
		//void NewBlock( string text );
		//void NewBuffer( Brush color, string text = null );
		//void NewBuffer( string text );
		//void Append( Brush color, string text = null );
		//void Append( string text );
		void Backspace( int i = 1 );
		void Clear();
		void Focus();
		void ScrollToEnd();
		bool CurrentBlockIsEmpty();
		void ResetCurrentBlockFormatting();
		void SwapFontColor( SolidColorBrush oldColor, SolidColorBrush newColor );
		//void SetFontSize( int size );
		//void SetFontColor( Brush brush );
	}
}