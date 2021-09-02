using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Limcap.FTerminal {
	public partial class Terminal {

		public bool IsAutocompleting { get; private set; }








		private void Handle_KeyboardInput( object sender, KeyEventArgs e ) {
			//if (e.IsRepeat) UpdateTraceInfo( e.Key );
			if (_usePasswordMask) Handle_PasswordInput( e );
			//else if (e.Key.IsIn( Key.Left, Key.Right, Key.Up, Key.Down, Key.Home, Key.End, Key.PageUp, Key.PageDown )) {}
			//else if (e.Key.IsIn( Key.LeftShift, Key.RightShift, Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl, Key.CapsLock, Key.Insert, Key.RWin, Key.LWin )) {}
			else if (e.Key == Key.Return) Handle2_ConfirmBuffer( e );
			else if (e.Key == Key.Tab) Handle2_Autocomplete( e );
			else if (e.Key == Key.Up || e.Key == Key.Down) Handle2_NavigateHistory( e );
			else if (e.Key == Key.Back) Handle2_DeleteFromBuffer( e );
			else Handle2_AddToBuffer( e );
		}








		private void Handle2_AddToBuffer( KeyEventArgs e ) {
			if (e.Key != Key.Escape) {
				var c = KeyGrabber.GetCharFromKey( e.Key );
				if (c != 0)
					_screen.AppendText( c.ToString() );
			}
			Handle3_TextChanged();
		}








		private void Handle2_DeleteFromBuffer( KeyEventArgs e ) {
			//InputRun.ContentEnd.GetPositionAtOffset( -1 ).DeleteTextInRun( 1 );
			_screen.Backspace();
			Handle3_TextChanged();
		}








		private void Handle3_TextChanged() {//object sender, TextChangedEventArgs args
			if (_customInterpreterIsActive) return;
			IsAutocompleting = false;
			if (!_allowAssistant) return;
			var input = _screen.Buffer;
			var text = _assistant.GetPredictions( input ).ToString();
			_assistantArea.Text = text;
		}








		private void Handle2_Autocomplete( KeyEventArgs e ) {
			IsAutocompleting = true;
			if (!e.IsRepeat) {
				var autocomplete = _assistant.GetNextAutocompleteEntry();
				//if (autocomplete != null && autocomplete.Length > 0)
				SetInputBuffer( autocomplete?.ToString(), predict: false );
				ScrollToEnd();
			}
			e.Handled = true;
		}








		private void Handle2_NavigateHistory( KeyEventArgs e ) {
			if (_customInterpreter == null) {
				IsAutocompleting = true;
				SetInputBuffer( e.Key == Key.Up ? _cmdHistory.Prev() : _cmdHistory.Next() );
				ScrollToEnd();
				e.Handled = true;
			}
		}








		private void Handle2_ConfirmBuffer( KeyEventArgs e ) {
			e.Handled = true;
			var input = _screen.Buffer;
			if (IsAutocompleting) {
				IsAutocompleting = false;
				_assistant.ProcessInput( input );
			}
			if (_customInterpreter is null)
				Handle3_RunDefaultInterpreter( input );
			else
				Handle3_RunCustomInterpreter( input );
		}








		private void Handle3_RunDefaultInterpreter( string input ) {
			if (((PString)input).Trim().len == 0)
				NewPrompt( false );
			else {
				if (!_customInterpreterIsActive) _cmdHistory.Add( input );
				else _customInterpreterIsActive = false;
				_statusArea.Text = string.Empty;
				_assistantArea.Text = string.Empty;
				var output = CommandInterpreter( input );
				Handle4_InterpreterResult( output );
			}
		}








		private void Handle3_RunCustomInterpreter( string input ) {
			_statusArea.Text = string.Empty;
			_assistantArea.Text = string.Empty;
			_customInterpreterIsActive = true;
			// Must set the _customInterpreter field to null before invoking it because it may or may not, itself,
			// set an other custom interpreter. After its execution we won't know this if we don't set it to
			// null now, and we must know this because we must know wether to execute the default or the newly set
			// interpreter on the next conformation of the input buffer.
			var customInterpreter = _customInterpreter;
			_customInterpreter = null;
			var output = customInterpreter( input );
			Handle4_InterpreterResult( output );
		}








		private void Handle4_InterpreterResult( string output ) {
			if (_customInterpreter is null) {
				if (!Ext.IsNullOrEmpty( output )) {
					if (output.Length > 100000) {
						NotepadRunner.Show( output );
					}
					else {
						_screen.AppendText( output );
						_statusArea.Text = $"Saída: {output.Length} caracteres";
					}
				}
				else
					_statusArea.Text = $"Comando executado";
				ScrollToEnd();
				// pede um novo prompt somente se não existir um input handler.
				//StartNewPrompt( usePrompt: _customInterpreter is null );
				_customInterpreterIsActive = false;
				NewPrompt();
				if (output?.Length > 500) ScrollToEnd();
				Handle3_TextChanged();
			}
			else {
				_customInterpreterIsActive = true;
			}
		}








		private void Handle_PasswordInput( KeyEventArgs e ) {
			ScrollToEnd();
			if (e.Key == Key.Back) {
				if (_passwordInput.Length > 0) {
					_passwordInput.Remove( _passwordInput.Length - 1, 1 );
					_screen.Backspace();
				}
			}
			else if (e.Key == Key.Return) {
				var interpreter = _customInterpreter is null ? CommandInterpreter : _customInterpreter;
				_customInterpreter = null;
				var input = _passwordInput.ToString();
				var output = interpreter( input );
				_passwordInput.Clear();
				_usePasswordMask = false;
				if (output != null) {
					_screen.AppendText( NEW_LINE + output );
					_statusArea.Text = $"Saída: {output.Length} caracteres";
					if (output.Length > 500) ScrollToEnd();
				}
				// pede um novo prompt somente se não existir um input handler.
				NewPrompt();
				Handle3_TextChanged();
			}
			else {
				var c = e.Key.ToPasswordAllowedChar();
				if (c.HasValue) {
					_screen.AppendText( "*" );
					_passwordInput.Append( c );
				}
				e.Handled = true;
			}
		}
	}
}
