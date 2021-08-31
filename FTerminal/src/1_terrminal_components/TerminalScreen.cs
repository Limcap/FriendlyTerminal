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
	public class TerminalScreen : ITerminalScreen {

		public const string NEW_LINE = "\n";
		public const string PROMPT_STRING = "» ";//›»
		public bool SpaceBetweenBlocks = true;




		private readonly Run CaretRun = new Run( "█" );
		private FlowDocumentScrollViewer _view;




		private FlowDocument _Doc => _view.Document;
		private ScrollViewer _Scroll => _view.Template.FindName( "PART_ContentHost", _view ) as ScrollViewer;
		private BlockCollection _Blocks => _Doc?.Blocks;
		private Paragraph _LastParagraph => _Blocks.LastBlock as Paragraph; //CaretRun.Parent as Paragraph;
		private Run _LastRun => CaretRun.PreviousInline.PreviousInline as Run;
		private Run _CurrentRun => CaretRun.PreviousInline as Run;




		public static implicit operator Control( TerminalScreen screen ) => screen.UIControlHook;
		public Control UIControlHook => _view;
		public Brush ForegroundDefault { get; set; } = new SolidColorBrush( Color.FromRgb( 171, 255, 46 ) );
		public Brush Background { get; set; } = new SolidColorBrush( Color.FromArgb( 200, 25, 27, 27 ) );
		public double FontSize { get => _Doc.FontSize; set => _Doc.FontSize = value; }

		public readonly Thickness marginThickness = new Thickness( 5 );









		public TerminalScreen() {
			_view = new FlowDocumentScrollViewer() {
				IsToolBarVisible = false,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				SelectionBrush = Brushes.White,
				Margin = new Thickness( 0 ),
			};
			var doc = new FlowDocument() {
				Background = Background,
				Foreground = ForegroundDefault,
				FontFamily = new FontFamily( "Consolas" ),
				FontSize = 14,
				PagePadding = marginThickness,
				Focusable = false,
			};
			_view.Document = doc;
			NewBlock();
		}








		public string Buffer {
			get => _CurrentRun.Text;
			set => _CurrentRun.Text = value;
		}








		public void Append( Brush color, string text = null ) {
			if (color != null && Foreground != color) NewBuffer( color );
			Append( text );
		}


		public void Append( string text ) {
			if (text is null) return;
			if (text == NEW_LINE) NewBuffer( NEW_LINE );
			else if (text.Length == 1) _CurrentRun.ContentEnd.InsertTextInRun( text );
			else {
				//var sr = new StringReader( text.Trim('\n') );
				var sr = new StringReader( text );
				string line;
				bool hasLinefeed;
				int i = 0;
				// removes initial empty line
				if (SpaceBetweenBlocks && sr.Peek() == '\n') sr.ReadLine();
				while ((line = sr.ReadLine()) != null) {
					hasLinefeed = sr.Peek() > -1;
					// removes final empty line
					if (SpaceBetweenBlocks && !hasLinefeed && line == string.Empty) continue;
					_CurrentRun.ContentEnd.InsertTextInRun( line );
					if (hasLinefeed) NewBuffer( NEW_LINE );
					else if (line == string.Empty) _CurrentRun.ContentEnd.InsertTextInRun( NEW_LINE );
					i++;
				}
				sr.Dispose();
			}
		}








		public void NewBlock( string text = null ) {
			NewBlock( ForegroundDefault, text );
		}


		public void NewBlock( Brush color, string text = null ) {
			color = color ?? ForegroundDefault;
			_LastParagraph?.Inlines.Remove( CaretRun );
			var p = new Paragraph() { Margin = new Thickness( 0 ), Foreground = color };
			if (_Blocks.Count > 0) p.Inlines.Add( new Run( NEW_LINE ) );
			p.Inlines.Add( new Run() );
			p.Inlines.Add( CaretRun );
			_Blocks.Add( p );
			Append( text );
		}








		public void NewBuffer( string text = null ) {
			NewBuffer( null, text );
		}


		public void NewBuffer( Brush color, string text = null ) {
			var run = new Run();
			if (color != null) run.Foreground = color;
			_LastParagraph?.Inlines.InsertBefore( CaretRun, run );
			if (text == NEW_LINE) run.Text = NEW_LINE;
			else Append( text );
		}








		public void Backspace( int i = 1 ) {
			_CurrentRun.ContentEnd.DeleteTextInRun( i * -1 );
		}








		public void Clear() {
			_Blocks.Clear();
			NewBlock();
		}









		public bool IsEmpty {
			get => _Blocks.Count == 1 && _LastParagraph.Inlines.Count == 2 && _CurrentRun.Text == string.Empty;
		}








		public Brush Foreground {
			get => _CurrentRun.Foreground;
			set => _CurrentRun.Foreground = value;
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
	}
}
