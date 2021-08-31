using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Limcap.FTerminal {
	public interface ITerminalScreen {
		Brush Background { get; set; }
		string Buffer { get; set; }
		double FontSize { get; set; }
		Brush BufferFontColor { get; set; }
		Brush DefaultFontColor { get; set; }
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
	}
}