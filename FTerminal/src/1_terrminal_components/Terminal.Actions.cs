﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Limcap.FTerminal {
	public partial class Terminal {

		public void Exit() {
			onExit();
		}



		public void Clear() {
			_screen.Clear();
		}




		public void TypeText( string text, Brush color = null ) {
			_screen.NewColor( color ?? ColorF2 ).AppendText( text );
			ScrollToEnd();
		}




		public void ScrollToEnd() {
			// Only setting the caret to the last index will not scroll the scroll viewer completely to the bottom,
			// a few pixels will still have to be scrolled manually. To counteract this, we manually scroll the 
			// scroll viewer to the bottom.
			_screen.ScrollToEnd();
		}




		public void ReadLine( Func<string, string> inputHandler ) {
			_usePasswordMask = false;
			_customInterpreter = inputHandler;
		}




		public void ReadPassword( Func<string, string> inputHandler ) {
			_usePasswordMask = true;
			_customInterpreter = inputHandler;
		}





		public void NewPrompt( bool newBlock = true ) {
			if (!newBlock) _screen.AppendText( NEW_LINE );
			else if (!_screen.IsEmpty ) _screen.NewBlock( ColorF1 );
				_screen.AppendText( PROMPT_STRING );
			_screen.NewBuffer();
		}
		//public void StartNewPrompt( bool usePrompt = true ) {
		//	if (usePrompt) {
		//		if (!_screen.IsEmpty) _screen.NewBlock();
		//		_screen.Append( PROMPT_STRING );
		//		_screen.NewBuffer( ColorF1 );
		//	}
		//}




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