﻿using System;
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
	/// <version>0.3</version>
	public class TerminalScreenV04 : ITerminalScreen {

		public const string NEW_LINE_STRING = "\n";
		public const char NEW_LINE_CHAR = '\n';
		//public const string PROMPT_STRING = "» ";//›»
		public readonly Thickness marginThickness = new Thickness( 5 );




		private readonly Run _caretRun = new Run( "█" );
		private readonly FlowDocumentScrollViewer _view;




		public bool UseSpaceBetweenBlocks { get; set; } = true;
		public string Buffer { get => _BufferRun.Text; set => _BufferRun.Text = value; }
		public SolidColorBrush BufferFontColor { get => _BufferRun.Foreground as SolidColorBrush; set => _BufferRun.Foreground = value; }




		//private FlowDocument _Doc => _view.Document;
		//private BlockCollection _Blocks => _view.Document?.Blocks;
		private ScrollViewer _Scroll => _view.Template.FindName( "PART_ContentHost", _view ) as ScrollViewer;
		private Paragraph _PastBlock => _CurrentBlock?.PreviousBlock as Paragraph;
		private Paragraph _CurrentBlock => _caretRun.Parent as Paragraph; //CaretRun.Parent as Paragraph;
		private Run _PastRun => _caretRun.PreviousInline?.PreviousInline as Run;
		private Run _BufferRun => _caretRun.PreviousInline as Run;




		public static implicit operator Control( TerminalScreenV04 screen ) => screen.UIControlHook;
		public Control UIControlHook => _view;
		public SolidColorBrush DefaultFontColor { get; set; } = new SolidColorBrush( Color.FromRgb( 171, 255, 46 ) );
		public SolidColorBrush BackgroundColor { get; set; } = new SolidColorBrush( Color.FromArgb( 200, 25, 27, 27 ) );
		public double DefaultFontSize { get => _view.Document.FontSize; set => _view.Document.FontSize = value; }











		public TerminalScreenV04() {
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
			NewBlock();
		}








		public ITerminalScreen NewBlock( Brush color = null ) {
			color = color ?? DefaultFontColor;
			_CurrentBlock?.Inlines.Remove( _caretRun );
			var p = new Paragraph() { Margin = new Thickness( 0 ), Foreground = color };
			p.Inlines.Add( new Run() );
			p.Inlines.Add( _caretRun );
			_view.Document.Blocks.Add( p );
			return this;
		}








		public ITerminalScreen NewBuffer( Brush color = null ) {
			var run = new Run();
			if (color != null) run.Foreground = color;
			(_caretRun.Parent as Paragraph).Inlines.InsertBefore( _caretRun, run );
			return this;
		}








		public ITerminalScreen NewColor( Brush color ) {
			if (color == null || color == _BufferRun.Foreground) { }
			else NewBuffer( color );
			return this;
		}








		public ITerminalScreen AppendText( string text ) {
			if (text == NEW_LINE_STRING || text.Length == 1) AppendText_NoSplit( text );
			else AppendText_SplitLines( text );
			AddaragraphSpacing();
			return this;
		}








		private ITerminalScreen AppendText_NoSplit( string text ) {
			_BufferRun.ContentEnd.InsertTextInRun( text );
			return this;
		}








		private ITerminalScreen AppendText_SplitLines( string text ) {
			var sli = ((PString)text).GetSlicer( NEW_LINE_CHAR, PString.Slicer.Mode.IncludeSeparatorAtStart );
			var color = BufferFontColor;
			// skips the first line if it is empty. This is because of how PString.Slicer works: If a string starts
			// with the separator char, the first slice will always be empty.
			if (sli.HasNext && sli.PeekNext().IsEmpty) sli.Next();
			while (sli.HasNext) {
				var line = sli.Next();
				if (line.StartsWith( NEW_LINE_CHAR )) {
					var curRunStartChar = _BufferRun.ContentStart.GetPositionAtOffset( 1 ).GetTextInRun( LogicalDirection.Backward );
					if (curRunStartChar == NEW_LINE_STRING) NewBuffer( color );
				}
				AppendText_NoSplit( line.AsString );
			}
			return this;
		}








		private void AddaragraphSpacing() {
			if (UseSpaceBetweenBlocks && _PastBlock != null ) {
				var curBlock = _CurrentBlock;
				if (curBlock.Tag != null) return;
				var curBlockFirstChar = curBlock.Inlines.FirstInline?.ContentStart.GetPositionAtOffset( 1 ).GetTextInRun( LogicalDirection.Backward );
				if (curBlockFirstChar != NEW_LINE_STRING) {
					var pastBlockLastChar = _PastBlock?.Inlines.LastInline?.ContentEnd.GetPositionAtOffset( -1 ).GetTextInRun( LogicalDirection.Forward );
					if (pastBlockLastChar != NEW_LINE_STRING) {
						var color = curBlock.Inlines.FirstInline.Foreground;
						curBlock.Inlines.InsertBefore( curBlock.Inlines.FirstInline, new Run( NEW_LINE_STRING ) { Foreground = color } );
					}
				}
				_CurrentBlock.Tag = true;
			}
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
