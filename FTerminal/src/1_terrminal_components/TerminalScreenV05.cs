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

namespace Limcap.FriendlyTerminal {

	/// <summary>
	/// UI Control that works as a Terminal Screen. <see cref="ITerminalScreen"/>
	/// Uses <see cref="FlowDocumentScrollViewer"/>, <see cref="FlowDocument"/> and <see cref="Run"/> internally.
	/// </summary>
	/// <version>0.5</version>
	public class TerminalScreenV05 : ITerminalScreen {

		public const string NEW_LINE_STRING = "\n";
		public const char NEW_LINE_CHAR = '\n';
		//public const string PROMPT_STRING = "» ";//›»
		public readonly Thickness marginThickness = new Thickness( 5 );




		private readonly Run _caretRun = new Run( "█" );
		private readonly FlowDocumentScrollViewer _view;




		public bool UseSpaceBetweenBlocks { get; set; } = true;
		public string Buffer { get => _BufferRun.Text; set => _BufferRun.Text = value; }




		//private FlowDocument _Doc => _view.Document;
		//private BlockCollection _Blocks => _view.Document?.Blocks;
		private ScrollViewer _Scroll => _view.Template.FindName( "PART_ContentHost", _view ) as ScrollViewer;
		private Paragraph _PastBlock => _CurrentBlock?.PreviousBlock as Paragraph;
		private Paragraph _CurrentBlock => _caretRun.Parent as Paragraph; //CaretRun.Parent as Paragraph;
		private Run _PastRun => _caretRun.PreviousInline?.PreviousInline as Run;
		private Run _BufferRun => _caretRun.PreviousInline as Run;




		public static implicit operator Control( TerminalScreenV05 screen ) => screen.UIControlHook;
		public Control UIControlHook => _view;



		private Brush _defaultBackgroundColor = new SolidColorBrush( Color.FromArgb( 200, 25, 27, 27 ) );
		private Brush _defaultFontColor = new SolidColorBrush( Color.FromRgb( 171, 255, 46 ) );
		private double _defaultFontSize = 14;




		public Brush BackgroundColor {
			get => _view.Document.Background;
			set => _view.Document.SetValue( TextElement.BackgroundProperty, value ?? DependencyProperty.UnsetValue );
		}
		public Brush DefaultFontColor {
			get => _view.Document.Foreground;
			set => _view.Document.SetValue( TextElement.ForegroundProperty, value ?? DependencyProperty.UnsetValue );
		}
		public Brush BufferFontColor {
			get => _BufferRun.Foreground;
			set => _BufferRun.SetValue( TextElement.ForegroundProperty, value ?? DependencyProperty.UnsetValue );
		}
		public double DefaultFontSize {
			get => _view.Document.FontSize;
			set => _view.Document.FontSize = value.MinMax( 10, 28 );
		}











		public TerminalScreenV05() {
			_view = new FlowDocumentScrollViewer() {
				IsToolBarVisible = false,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				SelectionBrush = Brushes.White,
				Margin = new Thickness( 0 ),
			};
			var doc = new FlowDocument() {
				Background = _defaultBackgroundColor,
				Foreground = _defaultFontColor,
				FontSize = _defaultFontSize,
				FontFamily = new FontFamily( "Consolas" ),
				PagePadding = marginThickness,
				Focusable = false,
			};
			_view.Document = doc;

			NewBlock();
		}








		public ITerminalScreen NewBlock( Brush color = null ) {
			if (UseSpaceBetweenBlocks) AddBlockFinalSpacing();
			//color = color ?? DefaultFontColor;
			_CurrentBlock?.Inlines.Remove( _caretRun );
			var p = new Paragraph() { Margin = new Thickness( 0 ) };
			if (color != null) p.Foreground = color;
			var r = new Run();
			p.Inlines.Add( r );
			p.Inlines.Add( _caretRun );
			_view.Document.Blocks.Add( p );
			return this;
		}








		/// <summary>
		/// Auxiliar method (1 of 2) to enforce the effect of the option <see cref="UseSpaceBetweenBlocks"/>.
		/// <br/>Must be called at the begining of the <see cref="NewBlock(Brush)"/> method.
		/// </summary>
		/// <remarks>
		/// <br/>Conditions to add space to the end of the block:
		/// <br/>- Option 'UseSpaceBetweenBlocks' is activated;
		/// <br/>- It is not the first block;
		/// <br/>- The block has more then 3 Runs (prompt, buffer, caret) OR The buffer is not empty;
		/// <br/>- The last character of the previous paragraph is not a linebreak.
		/// </remarks>
		/// <seealso cref="RemoveInitialLinebreak(string)"/>
		private void AddBlockFinalSpacing() {
			if (!(!UseSpaceBetweenBlocks || _CurrentBlock == null || _CurrentBlock.Inlines.Count <= 3 && _BufferRun.Text == string.Empty)) {
				var curBlockLastChar = _BufferRun.ContentEnd.GetPositionAtOffset( -1 ).GetTextInRun( LogicalDirection.Forward );
				if (curBlockLastChar != NEW_LINE_STRING) NewBuffer().AppendText( NEW_LINE_STRING );
			}
		}








		public ITerminalScreen NewBuffer( Brush color = null ) {
			var run = new Run();
			if (color != null) run.Foreground = color;
			(_caretRun.Parent as Paragraph).Inlines.InsertBefore( _caretRun, run );
			return this;
		}








		public ITerminalScreen NewColor( Brush color ) {
			if (color == null && _BufferRun.Foreground.GetValue( TextElement.ForegroundProperty ) != DependencyProperty.UnsetValue)
				NewBuffer();
			else if (color != _BufferRun.Foreground)
				NewBuffer( color );
			return this;
		}








		public ITerminalScreen AppendText( string text ) {
			if (text == NEW_LINE_STRING || text.Length == 1) AppendText_NoSplit( text );
			else AppendText_SplitLines( text );
			//AddaragraphSpacing();
			return this;
		}








		private ITerminalScreen AppendText_NoSplit( string text ) {
			_BufferRun.ContentEnd.InsertTextInRun( text );
			return this;
		}








		private ITerminalScreen AppendText_SplitLines( string text ) {
			text = RemoveInitialLinebreak( text );
			var sli = ((PString)text).GetSlicer( NEW_LINE_CHAR, PString.Slicer.Mode.IncludeSeparatorAtStart );
			var color = BufferFontColor;
			// skips the first line if it is empty. This is because of how PString.Slicer works: If a string starts
			// with the separator char, the first slice will always be empty.
			if (sli.HasNext && sli.PeekNext().IsEmpty) sli.Next();
			while (sli.HasNext) {
				var line = sli.Next();
				if (line.StartsWith( NEW_LINE_CHAR )) {
					NewBuffer( color );
					//var curRunStartChar = _BufferRun.ContentStart.GetPositionAtOffset( 1 ).GetTextInRun( LogicalDirection.Backward );
					//if (curRunStartChar == NEW_LINE_STRING) NewBuffer( color );
				}
				AppendText_NoSplit( line.AsString );
			}
			return this;
		}









		/// <summary>
		/// Auxiliar method (2 of 2) to enforce the effect of the option <see cref="UseSpaceBetweenBlocks"/>.
		/// <br/>Must be called by the <see cref="AppendText(string)"/> method.
		/// <br/>Removes all linebreaks from the start of the string if this is the first time a text is
		/// appended to the corresponding paragraph
		/// </summary>
		/// <remarks>
		/// The <see cref="FrameworkContentElement.Tag"/> property is used to identify that a paragraph has not
		/// yet had text appended to it, so only the first time a text is appended, the removal of initial linebreaks
		/// will occur.
		/// </remarks>
		private string RemoveInitialLinebreak( string text ) {
			if (UseSpaceBetweenBlocks) {
				var curBlock = _CurrentBlock;
				if (curBlock.Tag != null) return text;
				_CurrentBlock.Tag = true;
				PString ptext = text;
				if (ptext.StartsWith( NEW_LINE_CHAR )) {
					ptext.TrimStart( NEW_LINE_CHAR );
					return ptext.AsString;
				}
			}
			return text;
		}








		public void Backspace( int i = 1 ) {
			_BufferRun.ContentEnd.DeleteTextInRun( i * -1 );
		}








		public void Clear() {
			_view.Document.Blocks.Clear();
			NewBlock();
		}









		public bool IsEmpty {
			get => _view.Document.Blocks.Count == 1 && _CurrentBlock.Inlines.Count == 2 && _BufferRun.Text == string.Empty;
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








		//public void SetFontColor( Brush brush ) {
		//	_view.Document.Foreground = brush;

		//	//foreach (Paragraph p in _view.Document.Blocks)
		//	//	if (p.Foreground == DefaultFontColor) p.Foreground = brush;
		//	//DefaultFontColor = brush;

		//	//	foreach (Run r in p.Inlines)
		//	//		if (r.Foreground == BufferFontColor) r.Foreground = brush;
		//	//BufferFontColor = brush;
		//}








		//public void SetFontSize( int size ) {
		//	_view.Document.FontSize = size.MinMax( 10, 28 );

		//	//FontSize = size;
		//	//foreach (Paragraph p in _view.Document.Blocks)
		//	//	foreach (Run r in p.Inlines)
		//	//		r.FontSize = size.MinMax( 10, 28 );
		//}








		public bool CurrentBlockIsEmpty() {
			return (_CurrentBlock.Inlines.Count == 2 && _BufferRun.Text.Length == 0);
		}








		public void ResetCurrentBlockFormatting() {
			var b = _CurrentBlock;
			if (b == null) return;
			b.SetValue( TextElement.FontSizeProperty, DependencyProperty.UnsetValue );
			b.SetValue( TextElement.ForegroundProperty, DependencyProperty.UnsetValue );
		}








		public void SwapFontColor( SolidColorBrush oldColor, SolidColorBrush newColor ) {
			foreach (Paragraph p in _view.Document.Blocks) {
				if (p.Foreground == oldColor) p.Foreground = newColor;
				foreach (Run r in p.Inlines)
					if (r.Foreground == oldColor) r.Foreground = newColor;
			}

		}
	}
}
