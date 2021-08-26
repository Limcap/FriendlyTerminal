using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Limcap.UTerminal {
	public partial class Terminal {

		private void HandlePasswordInput( KeyEventArgs e ) {
			CaretToEnd();
			if (e.Key == Key.Back) {
				if (_passwordInput.Length > 0)
					_passwordInput.Remove( _passwordInput.Length - 1, 1 );
			}
			else if (e.Key == Key.Return) {
				var processor = _inputHandler is null ? ProcessInput : _inputHandler;
				_inputHandler = null;
				var input = _passwordInput.ToString();
				//var caretIndexBefore = CaretIndex;
				TypeText( NEW_LINE );
				var output = processor( input );
				_passwordInput.Clear();
				_usePasswordMask = false;
				if (output != null) {
					if (!LastRun.Text.EndsWith( NEW_LINE )) AppendText( NEW_LINE );
					AppendText( output.TrimEnd() );
					_statusArea.Text = $"Saída: {output.Length} caracteres";
				}
				// pede um novo prompt somente se não existir um input handler.
				StartNewInputBuffer( usePrompt: _inputHandler is null );
				//if (output.Length > 500) CaretIndex = caretIndexBefore;
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


			else if (e.Key == Key.Tab) {
				if (!e.IsRepeat) {
					var input = GetInputBuffer();
					var autocomplete = _assistant.GetNextAutocompleteEntry( input );
					if (autocomplete != null) SetInputBuffer( autocomplete.ToString(), predict: false );
					CaretToEnd();
				}
				e.Handled = true;
			}

			else if (e.Key == Key.Return) {
				var input = GetInputBuffer();
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
					//var caretIndexBefore = CaretIndex;
					AppendText( InputRun.Text );
					AppendText( NEW_LINE );
					InputRun.Text = string.Empty;
					var output = processor( input );
					if (output != null) {
						if (!LastRun.Text.EndsWith( NEW_LINE )) AppendText( NEW_LINE );
						AppendText( output.TrimEnd() );
						_statusArea.Text = $"Saída: {output.Length} caracteres";
					}
					// pede um novo prompt somente se não existir um input handler.
					StartNewInputBuffer( usePrompt: _inputHandler is null );
					//if (output?.Length > 500) CaretIndex = caretIndexBefore;
				}

			}

			else if (e.Key.IsIn( Key.LeftShift, Key.RightShift, Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl, Key.CapsLock, Key.Insert, Key.RWin, Key.LWin )) {
			}

			else if (e.Key == Key.Back) {
				InputRun.ContentEnd.DeleteTextInRun( -1 );
				//InputRun.ContentEnd.GetPositionAtOffset( -1 ).DeleteTextInRun( 1 );
			}
			else {
				var c = KeyGrabber.GetCharFromKey( e.Key );
				if( c != 0 ) {
					InputRun.ContentEnd.InsertTextInRun( c.ToString() );
					UpdateTraceArea( e.Key );
				}
			}
			HandleTextChanged( null, null );
		}



		private void HandleTextChanged( object sender, TextChangedEventArgs args ) {
			if (!_allowAssistant) return;
			var input = GetInputBuffer();
			var text = _assistant.GetPredictions( input ).ToString();
			_statusArea.Text = text;
		}



		public int TextLength;
		public int CaretIndexPrevious;
	}
}
