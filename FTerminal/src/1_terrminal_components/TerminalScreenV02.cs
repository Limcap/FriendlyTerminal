using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Limcap.FTerminal {

	/// <summary>
	/// UI Control that works as a Terminal Screen. <see cref="ITerminalScreen"/>
	/// Uses <see cref="FlowDocumentScrollViewer"/>, <see cref="FlowDocument"/> and <see cref="Run"/> internally.
	/// </summary>
	/// <version>0.2</version>
	public class TerminalScreenV02 : ITerminalScreen {

		public const string NEW_LINE = "\n";
		public const string PROMPT_STRING = "» ";//›»
		public readonly Thickness marginThickness = new Thickness( 5 );




		private readonly Run _caretRun = new Run( "█" );
		private FlowDocumentScrollViewer _view;




		public bool SpaceBetweenBlocks = true;
		public string Buffer { get => _InputRun.Text; set => _InputRun.Text = value; }
		public Brush BufferFontColor { get => _InputRun.Foreground; set => _InputRun.Foreground = value; }




		private FlowDocument _Doc => _view.Document;
		private ScrollViewer _Scroll => _view.Template.FindName( "PART_ContentHost", _view ) as ScrollViewer;
		private BlockCollection _Blocks => _view.Document?.Blocks;
		private Paragraph _LastParagraph => _view.Document?.Blocks.LastBlock as Paragraph; //CaretRun.Parent as Paragraph;
		private Run _LastRun => _caretRun.PreviousInline.PreviousInline as Run;
		private Run _InputRun => _caretRun.PreviousInline as Run;




		public static implicit operator Control( TerminalScreenV02 screen ) => screen.UIControlHook;
		public Control UIControlHook => _view;
		public Brush DefaultFontColor { get; set; } = new SolidColorBrush( Color.FromRgb( 171, 255, 46 ) );
		public Brush BackgroundColor { get; set; } = new SolidColorBrush( Color.FromArgb( 200, 25, 27, 27 ) );
		public double DefaultFontSize { get => _Doc.FontSize; set => _Doc.FontSize = value; }











		public TerminalScreenV02() {
			_view = new FlowDocumentScrollViewer() {
				IsToolBarVisible = false,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				SelectionBrush = Brushes.White,
				Margin = new Thickness( 0 ),
			};
			var doc = new FlowDocument() {
				Background = BackgroundColor,
				Foreground = DefaultFontColor,
				FontFamily = new FontFamily( "Consolas" ),
				FontSize = 14,
				PagePadding = marginThickness,
				Focusable = false,
			};
			_view.Document = doc;
			NewBlock(null,null);
		}









		public void NewBlock( string text = null ) {
			NewBlock( DefaultFontColor, text );
		}


		public void NewBlock( Brush color, string text = null ) {
			color = color ?? DefaultFontColor;
			_LastParagraph?.Inlines.Remove( _caretRun );
			var p = new Paragraph() { Margin = new Thickness( 0 ), Foreground = color };
			if (_Blocks.Count > 0) p.Inlines.Add( new Run( NEW_LINE ) );
			p.Inlines.Add( new Run() );
			p.Inlines.Add( _caretRun );
			_Blocks.Add( p );
			_Append( null, text );
		}








		public void NewBuffer( string text = null ) {
			NewBuffer( null, text );
		}


		public void NewBuffer( Brush color, string text = null ) {
			var run = _NewBuffer_NoSplit( color );
			if (text == null || text == NEW_LINE || text.Length == 1) run.Text = text;
			else _Append_SplitLines( text );
		}


		private Run _NewBuffer_NoSplit( Brush color, string text = null ) {
			var run = new Run( text );
			if (color != null) run.Foreground = color;
			_LastParagraph?.Inlines.InsertBefore( _caretRun, run );
			return run;
		}








		public void Append( Brush color, string text = null ) {
			_Append( color, text );
		}


		public void Append( string text ) {
			_Append( null, text );
		}


		private void _Append( Brush color, string text ) {
			if (text is null) return;

			//Run run = text == NEW_LINE || color != null && color != _InputRun.Foreground
			//? _NewBufferSingleRun( color, null )
			//: _InputRun;

			if (text == NEW_LINE || color != null && color != _InputRun.Foreground)
				_NewBuffer_NoSplit( color, null );

			if (text.Length == 1) _Append_NoSplit( text );
			else _Append_SplitLines( text );
		}


		private void _Append_SplitLines( string text ) {
			var run = _InputRun;
			var sr = new StringReader( text );
			string line;
			bool hasLinefeed;
			// removes initial empty line
			if (SpaceBetweenBlocks && sr.Peek() == '\n') sr.ReadLine();
			while ((line = sr.ReadLine()) != null) {
				hasLinefeed = sr.Peek() > -1;
				// removes final empty line
				if (SpaceBetweenBlocks && !hasLinefeed && line == string.Empty) continue;
				run.ContentEnd.InsertTextInRun( line );
				//if (hasLinefeed) run = _NewBufferSingleRun( run.Foreground, NEW_LINE );
				if (hasLinefeed) run = _NewBuffer_NoSplit( run.Foreground, NEW_LINE );
				else if (line == string.Empty) _Append_NoSplit( NEW_LINE );
			}
			sr.Dispose();
		}


		private void _Append_NoSplit( string text ) {
			_InputRun.ContentEnd.InsertTextInRun( text );
		}








		public void Backspace( int i = 1 ) {
			_InputRun.ContentEnd.DeleteTextInRun( i * -1 );
		}








		public void Clear() {
			_Blocks.Clear();
			NewBlock( null, null );
		}









		public bool IsEmpty {
			get => _Blocks.Count == 1 && _LastParagraph.Inlines.Count == 2 && _InputRun.Text == string.Empty;
		}








		public void Focus() {
			_view.Focus();
		}








		public void ScrollToEnd() {
			_Scroll.ScrollToEnd();
		}





		public event KeyEventHandler OnPreviewKeyDown {
			add => _view.PreviewKeyDown += value;
			remove => _view.PreviewKeyDown -= value;
		}















		public ITerminalScreen NewBlock( Brush color = null ) {
			NewBlock( color, null );
			return this;
		}

		public ITerminalScreen NewBuffer( Brush color = null ) {
			NewBuffer( color, null );
			return this;
		}

		public ITerminalScreen NewColor( Brush color ) {
			_Append( color, string.Empty );
			return this;
		}

		public ITerminalScreen AppendText( string text ) {
			Append( text );
			return this;
		}

		public bool CurrentBlockIsEmpty() {
			throw new NotImplementedException();
		}

		public void ResetCurrentBlockFormatting() {
			throw new NotImplementedException();
		}

		public void SwapFontColor( SolidColorBrush oldColor, SolidColorBrush newColor ) {
			throw new NotImplementedException();
		}
	}
}
