using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Limcap.FTerminal {
	public class TextScreen {

		public const string NEW_LINE = "\n";
		public const string PROMPT_STRING = "» ";//›»


		public FlowDocumentScrollViewer View { get; private set; }
		public FlowDocument Doc => View.Document;
		public ScrollViewer Scroll => View.Template.FindName( "PART_ContentHost", View ) as ScrollViewer;


		public readonly Run CaretRun = new Run( "█" );
		public BlockCollection Blocks => Doc?.Blocks;
		public Paragraph LastParagraph => Blocks.LastBlock as Paragraph; //CaretRun.Parent as Paragraph;
		private Run LastRun => CaretRun.PreviousInline.PreviousInline as Run;
		public Run Buffer => CaretRun.PreviousInline as Run;



		public Brush DefaultForeground { get; set; } = new SolidColorBrush( Color.FromRgb( 171, 255, 46 ) );
		public Brush Background { get; set; } = new SolidColorBrush( Color.FromArgb( 200, 25, 27, 27 ) );
		public readonly Thickness marginThickness = new Thickness( 5 );


		private Paragraph ParagraphBuilder() {
			return new Paragraph() { Margin = marginThickness };
		}








		public TextScreen() {
			View = new FlowDocumentScrollViewer() {
				IsToolBarVisible = false,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				SelectionBrush = Brushes.White,
				Margin = new Thickness(0),
			};
			var doc = new FlowDocument() {
				Background = Background,
				Foreground = DefaultForeground,
				FontFamily = new FontFamily( "Consolas" ),
				FontSize = 14,
				PagePadding = marginThickness,
				Focusable = false,
			};
			View.Document = doc;
			NewParagraph();
		}








		public void Append( string txt, Brush color = null ) {
			//color = color ?? DefaultForeground;
			AppendToBuffer( txt, color );
		}








		public void AppendWithSpace( string text, Brush color = null ) {
			//color = color ?? DefaultForeground;
			if (Buffer.Foreground != color) NewBuffer( color );
			string spaceAfter = null;
			string spaceBefore = null;
			if (!text.StartsWith( NEW_LINE )) spaceBefore = NEW_LINE;
			if (!text.EndsWith( NEW_LINE )) spaceAfter = NEW_LINE;
			AppendToBuffer( spaceBefore + text + spaceAfter, color );
		}







		private void AppendToBuffer( string text, Brush color = null ) {
			color = color ?? DefaultForeground;
			var sr = new StringReader( text );
			string line;
			while ((line = sr.ReadLine()) != null) {
				if( sr.Peek() > -1 )
					NewParagraph( color );
				Buffer.ContentEnd.InsertTextInRun( line );
			}
			
		}






		public void NewParagraph( Brush color = null ) {
			color = color ?? DefaultForeground;
			var p = new Paragraph() { Margin = new Thickness(0) };
			LastParagraph?.Inlines.Remove( CaretRun );
			p.Inlines.Add( new Run() { Foreground = color } );
			p.Inlines.Add( CaretRun );
			Blocks.Add( p );
		}








		public void NewBuffer( Brush color = null ) {
			color = color ?? DefaultForeground;
			LastParagraph.Inlines.InsertBefore( CaretRun, new Run() { Foreground = color } );
		}








		public void Clear() {
			Blocks.Clear();
			NewParagraph();
		}






		public void Focus() {
			View.Focus();
		}
		public bool IsEmpty => Blocks.Count == 1 && LastParagraph.Inlines.Count == 2 && Buffer.Text == string.Empty;
	}
}
