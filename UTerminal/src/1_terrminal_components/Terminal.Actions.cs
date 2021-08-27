using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Limcap.UTerminal {
	public partial class Terminal {

		public void Exit() {
			onExit();
		}



		public void Clear() {
			Inlines.Clear();
			Inlines.Add( InputBufferRun );
			Inlines.Add( CaretRun );
		}




		public void TypeText( string text ) {
			AppendText( text, false );
			ScrollToEnd();
		}




		public void AppendText( string txt, bool noPredictions = true ) {
			_allowAssistant = !noPredictions;
			Inlines.InsertBefore( InputBufferRun, new Run(txt) );
			//_mainArea.Inlines.Remove( CaretRun );
			//_mainArea.Inlines.Add( txt );
			//_mainArea.Inlines.Add( CaretRun );
			_allowAssistant = true;
		}
		public void AppendText( string txt, Brush color ) {
			Inlines.InsertBefore( InputBufferRun, new Run( txt ) { Foreground = color } );
		}
		private void AppendWithSpace( string text, Brush color ) {
			string spaceAfter = null;
			string spaceBefore = null;
			if (!text.StartsWith( NEW_LINE )) spaceBefore = NEW_LINE;
			//if (!text.StartsWith( NEW_LINE )) AppendText( NEW_LINE );
			//AppendText( text, ColorF2 );
			if (text.EndsWith( NEW_LINE + NEW_LINE )) { }
			else if (text.EndsWith( NEW_LINE )) spaceAfter = NEW_LINE;
			else spaceAfter = NEW_LINE + NEW_LINE;
			Inlines.InsertBefore( InputBufferRun, new Run( spaceBefore + text + spaceAfter ) { Foreground = color } );
		}




		public void ScrollToEnd() {
			// Only setting the caret to the last index will not scroll the scroll viewer completely to the bottom,
			// a few pixels will still have to be scrolled manually. To counteract this, we manually scroll the 
			// scroll viewer to the bottom.
			_scrollArea.ScrollToBottom();
		}




		public void ReadLine( Func<string, string> inputHandler ) {
			_usePasswordMask = false;
			_customInterpreter = inputHandler;
		}




		public void ReadPassword( Func<string, string> inputHandler ) {
			_usePasswordMask = true;
			_customInterpreter = inputHandler;
		}




		public void StartNewInputBuffer( bool usePrompt = true ) {
			if (usePrompt) {
				//bool newLineNeeded = Inlines.FirstInline != InputBufferRun && !Inlines.Last().ContentStart.GetTextInRun(LogicalDirection.Forward).EndsWith( NEW_LINE );
				var lastChar = LastRun?.ContentEnd.GetPositionAtOffset( -1 ).GetTextInRun( LogicalDirection.Forward );
				bool newLineNeeded = Inlines.FirstInline != InputBufferRun && lastChar != NEW_LINE;
				//if (Inlines.Count == 0) Inlines.Add( CaretRun );
				//AppendText( (newLineNeeded ? NEW_LINE : string.Empty) + PROMPT_STRING );
				AppendText( (newLineNeeded ? NEW_LINE : string.Empty) + PROMPT_STRING, ColorF1 );
			}
		}




		private void ClearInputBuffer() {
			InputBufferRun.Text = string.Empty;
			//var len = InputRun.ContentEnd.GetTextRunLength( LogicalDirection.Forward );
			//InputRun.ContentStart.DeleteTextInRun( len );
		}




		private void SetInputBuffer( string text, bool predict = true ) {
			_allowAssistant = predict;
			InputBufferRun.Text = text;
			_allowAssistant = true;
		}
		public string GetInputBuffer() {
			return InputBufferRun.Text;
		}




		private void UpdateTraceInfo( Key pressedKey ) {
			var c = KeyGrabber.GetCharFromKey( pressedKey );
			_statusArea.Text = $"pressed:{pressedKey}   typed:{c}   current:{CurrentChar}";
		}




		public bool ShowAssistBar {
			get => _assistantArea.Visibility == Visibility.Visible;
			set => _assistantArea.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
		}




		public bool ShowStatusBar {
			get => _statusArea.Visibility == Visibility.Visible;
			set => _statusArea.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
		}




		public static void Send( Key key ) {
			//var target = Keyboard.FocusedElement;    // Target element
			//var routedEvent = Keyboard.KeyDownEvent; // Event to send

			//target.RaiseEvent(
			//  new KeyEventArgs(
			//	 Keyboard.PrimaryDevice,
			//	 PresentationSource.FromVisual( target ),
			//	 0,
			//	 key ) { RoutedEvent = routedEvent }
			//);

			if (Keyboard.PrimaryDevice != null) {
				if (Keyboard.PrimaryDevice.ActiveSource != null) {
					var e1 = new KeyEventArgs( Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Down ) { RoutedEvent = Keyboard.KeyDownEvent };
					InputManager.Current.ProcessInput( e1 );
				}
			}
		}
	}
}
