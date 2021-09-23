using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Limcap.FriendlyTerminal {
	public partial class Terminal {

		public void Exit() {
			onExit();
		}



		public void Clear() {
			_dispatcher.Invoke( () => _screen.Clear() );
		}




		public void TypeText( string text, Brush color = null ) {
			_dispatcher.Invoke( () => {
				_screen.NewColor( color ).AppendText( text );
				ScrollToEnd();
			} );
		}




		public void ChangeText( string text, Brush color = null ) {
			_dispatcher.Invoke( () => {
				_screen.Buffer = text;
				_screen.ChangeColor( color );
				ScrollToEnd();
			} );
		}




		public void ScrollToEnd() {
			// Only setting the caret to the last index will not scroll the scroll viewer completely to the bottom,
			// a few pixels will still have to be scrolled manually. To counteract this, we manually scroll the 
			// scroll viewer to the bottom.
			_dispatcher.Invoke( () => _screen.ScrollToEnd() );
		}




		public void ReadLine( Func<string, string> inputHandler, string defaultText = "" ) {
			_dispatcher.Invoke( () => {
				_usePasswordMask = false;
				_customInterpreter = CreateCustomInterpreter( inputHandler );
				_screen.NewBuffer(_screen.DefaultFontColor);
			} );
		}




		public void ReadPassword( Func<string, string> inputHandler ) {
			_dispatcher.Invoke( () => {
				_usePasswordMask = true;
				_customInterpreter = CreateCustomInterpreter( inputHandler );
			} );
		}




		private Func<string,ValueTask<string>> CreateCustomInterpreter( Func<string, string> inputHandler ) {
			return async ( input ) => await Task.Run( () => inputHandler( input ) );
		}





		private void NewPrompt( bool newBlock = true ) {
			if (_screen.CurrentBlockIsEmpty())
				_screen.ResetCurrentBlockFormatting();
			else if (!newBlock)
				_screen.AppendText( NEW_LINE );
			else if (!_screen.IsEmpty)
				_screen.NewBlock();
			_screen.AppendText( PROMPT_STRING );
			_screen.NewBuffer();
		}




		private void ClearInputBuffer() {
			_screen.Buffer = string.Empty;
		}
		private void SetInputBuffer( string text, bool predict = true ) {
			_allowAssistant = predict;
			_screen.Buffer = text;
			_allowAssistant = true;
		}
		//public string GetInputBuffer() {
		//	return _screen.Buffer.Text;
		//}




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
