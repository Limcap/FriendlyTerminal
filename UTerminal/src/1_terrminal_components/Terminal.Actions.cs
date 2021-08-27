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
			Inlines.Add( InputRun );
			Inlines.Add( CaretRun );
		}




		public void TypeText( string text ) {
			AppendText( text, false );
			CaretToEnd();
		}




		public void AppendText( string txt, bool noPredictions = true ) {
			_allowAssistant = !noPredictions;
			Inlines.InsertBefore( InputRun, new Run(txt) );
			//_mainArea.Inlines.Remove( CaretRun );
			//_mainArea.Inlines.Add( txt );
			//_mainArea.Inlines.Add( CaretRun );
			_allowAssistant = true;
		}
		public void AppendText( string txt, Brush color ) {
			Inlines.InsertBefore( InputRun, new Run( txt ) { Foreground = color } );
		}




		public void CaretToEnd() {
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
				var inl = _mainArea.Inlines;
				if (inl.Count == 0) inl.Add( CaretRun );
				bool newLineNeeded = !inl.Last().ContentStart.GetTextInRun(LogicalDirection.Forward).EndsWith( NEW_LINE );
				AppendText( (newLineNeeded ? NEW_LINE : string.Empty) + PROMPT_STRING );
			}
		}




		private void ClearInputBuffer() {
			InputRun.Text = string.Empty;
			//var len = InputRun.ContentEnd.GetTextRunLength( LogicalDirection.Forward );
			//InputRun.ContentStart.DeleteTextInRun( len );
		}




		private void SetInputBuffer( string text, bool predict = true ) {
			_allowAssistant = predict;
			InputRun.Text = text;
			_allowAssistant = true;
		}
		public string GetInputBuffer() {
			return InputRun.Text;
		}




		private void UpdateTraceArea( Key pressedKey ) {
			//var lastLineStartIndex = mainArea.Text.LastIndexOf( NewLine );
			//lastLineStartIndex = lastLineStartIndex < 0 ? 0 : lastLineStartIndex;
			//_lastLine = mainArea.Text.Substring( lastLineStartIndex );
			//_lastLineCaretIndex = mainArea.CaretIndex - (mainArea.Text.Length - _lastLine.Length);
			//var curChar = _lastLineCaretIndex == _lastLine.Length ? ' ' : _lastLine[_lastLineCaretIndex];
			//statusArea.Text = $"{pressedKey} - {curChar} - ({_lastLineCaretIndex})";
			var c = KeyGrabber.GetCharFromKey( pressedKey );
			//_traceArea.Text = $"pressed:{pressedKey}   typed:{c}   current:{CurrentChar}   caret:{CaretIndex}";
			_traceArea.Text = $"pressed:{pressedKey}   typed:{c}   current:{CurrentChar}";
		}




		public bool ShowStatusBar {
			get => _statusArea.Visibility == Visibility.Visible;
			set => _statusArea.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
		}




		public bool ShowTraceBar {
			get => _traceArea.Visibility == Visibility.Visible;
			set => _traceArea.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
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
