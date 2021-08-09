using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Limcap.UTerminal {
	public partial class Terminal {

		public void Exit() {
			onExit();
		}



		public void Clear() {
			Text = string.Empty;
		}




		public void TypeText( string text ) {
			AppendText( text, false );
			CaretToEnd();
		}




		public void AppendText( string txt, bool noPredictions = true ) {
			_predictor.Activated = !noPredictions;
			_mainArea.AppendText( txt );
			_predictor.Activated = true;
		}




		public void CaretToEnd() {
			CaretIndex = Text.Length;
			// Only setting the caret to the last index will not scroll the scroll viewer completely to the bottom,
			// a few pixels will still have to be scrolled manually. To counteract this, we manually scroll the 
			// scroll viewer to the bottom.
			_scrollArea.ScrollToBottom();
		}




		public void ReadLine( Func<string, string> inputHandler ) {
			_usePasswordMask = false;
			_inputHandler = inputHandler;
		}




		public void ReadPassword( Func<string, string> inputHandler ) {
			_usePasswordMask = true;
			_inputHandler = inputHandler;
		}




		public void StartNewInputBuffer( bool usePrompt = true ) {
			if (usePrompt) {
				bool newLineNeeded = !(Text.Length == 0 || Text.EndsWith( NEW_LINE ));
				//Text += (newLineNeeded ? NewLine : string.Empty) + PromptString;
				AppendText( (newLineNeeded ? NEW_LINE : string.Empty) + PROMPT_STRING );
			}
			_bufferStartIndex = Text.Length;
			CaretToEnd();
		}




		private void ClearInputBuffer() {
			_mainArea.SelectionOpacity = 0;
			_mainArea.Select( _bufferStartIndex, _mainArea.Text.Length - _bufferStartIndex );
			_mainArea.SelectedText = string.Empty;
			_mainArea.SelectionOpacity = 0.5;
		}




		private void SetInputBuffer( string text, bool predict = true ) {
			_predictor.Activated = predict;
			_mainArea.Select( _bufferStartIndex, _mainArea.Text.Length - _bufferStartIndex );
			_mainArea.SelectedText = text;
			_predictor.Activated = true;
		}




		private void UpdateTraceArea( Key pressedKey ) {
			//var lastLineStartIndex = mainArea.Text.LastIndexOf( NewLine );
			//lastLineStartIndex = lastLineStartIndex < 0 ? 0 : lastLineStartIndex;
			//_lastLine = mainArea.Text.Substring( lastLineStartIndex );
			//_lastLineCaretIndex = mainArea.CaretIndex - (mainArea.Text.Length - _lastLine.Length);
			//var curChar = _lastLineCaretIndex == _lastLine.Length ? ' ' : _lastLine[_lastLineCaretIndex];
			//statusArea.Text = $"{pressedKey} - {curChar} - ({_lastLineCaretIndex})";
			var c = Util.GetCharFromKey( pressedKey );
			_traceArea.Text = $"pressed:{pressedKey}   typed:{c}   current:{CurrentChar}   caret:{CaretIndex}";
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
