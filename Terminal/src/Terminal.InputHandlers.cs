using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Limcap.TextboxTerminal {
	public partial class Terminal {

		private void HandlePasswordInput( KeyEventArgs e ) {
			CaretToEnd();
			if (e.Key == Key.Back) {
				if (CaretIndex <= _bufferStartIndex) {
					CaretToEnd();
					e.Handled = true;
				}
				else if (_passwordInput.Length > 0)
					_passwordInput.Remove( _passwordInput.Length - 1, 1 );
			}
			else if (e.Key == Key.Return) {
				var processor = _inputHandler is null ? ProcessInput : _inputHandler;
				_inputHandler = null;
				var input = _passwordInput.ToString();
				var caretIndexBefore = CaretIndex;
				TypeText( NewLine );
				var output = processor( input );
				_passwordInput.Clear();
				_usePasswordMask = false;
				if (output != null) {
					if (!Text.EndsWith( NewLine )) AppendText( NewLine );
					AppendText( output.TrimEnd() );
					_statusArea.Text = $"Saída: {output.Length} caracteres";
				}
				// pede um novo prompt somente se não existir um input handler.
				StartNewInputBuffer( usePrompt: _inputHandler is null );
				if (output.Length > 500) CaretIndex = caretIndexBefore;
			}
			else {
				var c = e.Key.ToPasswordAllowedChar();
				if (c.HasValue) {
					TypeText( "*" );
					_passwordInput.Append( c );
				}
				e.Handled = true;
			}
		}




		private void HandleRegularInput( object sender, KeyEventArgs e ) {
			if (e.IsRepeat)
				UpdateTraceArea( e.Key );

			if (_usePasswordMask) {
				HandlePasswordInput( e );
				return;
			}

			if (e.Key == Key.Up && IsControlDown) {
				if (_inputHandler == null) {
					SetInputBuffer( _cmdHistory.Prev() );
					CaretToEnd();
					e.Handled = true;
				}
			}

			else if (e.Key == Key.Down && IsControlDown) {
				if (_inputHandler == null) {
					SetInputBuffer( _cmdHistory.Next() );
					CaretToEnd();
					e.Handled = true;
				}
			}


			else if (e.Key.IsIn( Key.Left, Key.Right, Key.Up, Key.Down, Key.Home, Key.End, Key.PageUp, Key.PageDown )) {
				// freedom of caret movement
			}


			else if (e.Key == Key.Back || e.Key == Key.Left) {
				if (CaretIndex <= _bufferStartIndex) e.Handled = true;
			}


			else if (e.Key == Key.Return) {
				var input = InputBuffer;
				UpdateTraceArea( e.Key );
				if (input.Trim().Length == 0) {
					e.Handled = true;
					// pede um novo prompt somente se não existir um input handler.
					StartNewInputBuffer( usePrompt: _inputHandler is null );
				}
				else {
					_cmdHistory.Add( input );
					// Se houver um inputHandler esperando por input, copia ele para a variavel processos e já reseta
					// oo _inputHandler para que caso o handler atual defina outro inputHandler, não haja problema de 
					// substituição e para que 
					var processor = _inputHandler is null ? ProcessInput : _inputHandler;
					_inputHandler = null;
					var caretIndexBefore = CaretIndex;
					TypeText( NewLine );
					var output = processor( input );
					if (output != null) {
						if (!Text.EndsWith( NewLine )) AppendText( NewLine );
						AppendText( output.TrimEnd() );
						_statusArea.Text = $"Saída: {output.Length} caracteres";
					}
					// pede um novo prompt somente se não existir um input handler.
					StartNewInputBuffer( usePrompt: _inputHandler is null );
					if (output?.Length > 500) CaretIndex = caretIndexBefore;
				}

			}
			else if (e.Key.IsIn( Key.LeftShift, Key.RightShift, Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl, Key.CapsLock, Key.Insert, Key.RWin, Key.LWin )) {
			}
			else {
				if (CaretIndex < _bufferStartIndex) CaretToEnd();
				//if (_usePasswordMaskForInput && e.Key != Key.Back && e.Key != Key.Delete ) {
				//	char? c = e.Key.ToPasswordAllowedChar();
				//	if (c.HasValue) Text += c.Value;
				//	//if (c.HasValue) input.Append( c.Value );
				//	e.Handled = true;
				//}
				UpdateTraceArea( e.Key );
			}
		}
	}
}
