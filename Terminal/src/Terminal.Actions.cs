using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Limcap.TextboxTerminal {
	public partial class Terminal {

		public void Exit() {
			onExit();
		}



		public void Clear() {
			Text = string.Empty;
		}




		public void TypeText( string text ) {
			_mainArea.Text += text;
			_mainArea.CaretIndex = _mainArea.Text.Length;
		}




		public void ReadLine( Func<string, string> inputHandler ) {
			_inputHandler = inputHandler;
		}




		public static void Send( Key key ) {
			if (Keyboard.PrimaryDevice != null) {
				if (Keyboard.PrimaryDevice.ActiveSource != null) {
					var e1 = new KeyEventArgs( Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Down ) { RoutedEvent = Keyboard.KeyDownEvent };
					InputManager.Current.ProcessInput( e1 );
				}
			}
		}




		public void StartNewInputLine() {
			bool newLineNeeded = !(Text.Length == 0 || Text.EndsWith( NewLine ) );
			Text += (newLineNeeded ? NewLine : string.Empty) + "> ";
			_minCaretIndex = Text.Length;
			_mainArea.CaretIndex = Text.Length;
			_scrollArea.ScrollToBottom();
		}




		private void UpdateDebugArea( Key pressedKey ) {
			//var lastLineStartIndex = mainArea.Text.LastIndexOf( NewLine );
			//lastLineStartIndex = lastLineStartIndex < 0 ? 0 : lastLineStartIndex;
			//_lastLine = mainArea.Text.Substring( lastLineStartIndex );
			//_lastLineCaretIndex = mainArea.CaretIndex - (mainArea.Text.Length - _lastLine.Length);
			//var curChar = _lastLineCaretIndex == _lastLine.Length ? ' ' : _lastLine[_lastLineCaretIndex];
			//statusArea.Text = $"{pressedKey} - {curChar} - ({_lastLineCaretIndex})";
			_debugArea.Text = $"{pressedKey} - {CurrentChar} - ({CaretIndex})";
		}
	}
}
