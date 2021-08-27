using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Limcap.UTerminal {
	public partial class Terminal {

		//public int TextLength;
		//public int CaretIndexPrevious;
		public bool IsAutocompleting { get; private set; }








		private void Handle_KeyboardInput( object sender, KeyEventArgs e ) {
			if (e.IsRepeat) UpdateTraceArea( e.Key );
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
			var c = KeyGrabber.GetCharFromKey( e.Key );
			if (c != 0) {
				InputRun.ContentEnd.InsertTextInRun( c.ToString() );
				UpdateTraceArea( e.Key );
			}
			Handle3_TextChanged( null, null );
		}








		private void Handle2_DeleteFromBuffer( KeyEventArgs e ) {
			InputRun.ContentEnd.DeleteTextInRun( -1 );
			//InputRun.ContentEnd.GetPositionAtOffset( -1 ).DeleteTextInRun( 1 );
			Handle3_TextChanged( null, null );
		}








		private void Handle3_TextChanged( object sender, TextChangedEventArgs args ) {
			IsAutocompleting = false;
			if (!_allowAssistant) return;
			var input = GetInputBuffer();
			var text = _assistant.GetPredictions( input ).ToString();
			_statusArea.Text = text;
		}








		private void Handle2_Autocomplete( KeyEventArgs e ) {
			IsAutocompleting = true;
			if (!e.IsRepeat) {
				var autocomplete = _assistant.GetNextAutocompleteEntry();
				if (autocomplete != null && autocomplete.Length > 0)
					SetInputBuffer( autocomplete.ToString(), predict: false );
				CaretToEnd();
			}
			e.Handled = true;
		}








		private void Handle2_NavigateHistory( KeyEventArgs e ) {
			if (_customInterpreter == null) {
				IsAutocompleting = true;
				SetInputBuffer( e.Key == Key.Up ? _cmdHistory.Prev() : _cmdHistory.Next() );
				CaretToEnd();
				e.Handled = true;
			}
		}








		private void Handle2_ConfirmBuffer( KeyEventArgs e ) {
			var input = GetInputBuffer();
			if (IsAutocompleting) {
				IsAutocompleting = false;
				_assistant.ProcessInput( input );
				//HandleTextChanged( null, null );
				//return;
			}
			UpdateTraceArea( e.Key );
			if (input.Trim().Length == 0) {
				e.Handled = true;
				// pede um novo prompt somente se não existir um input handler.
				StartNewInputBuffer( usePrompt: _customInterpreter is null );
			}
			else {
				_cmdHistory.Add( input );
				// Se houver um interpretador customizado (que pode ser definido pelos comandos) copia sua ref para uma
				// variável local e já anula a referencia principal, para que caso este interpretador customizado queria
				// definir ainda um outro, não haja problema de sobrescrever.
				var interpreter = _customInterpreter is null ? CommandInterpreter : _customInterpreter;
				_customInterpreter = null;
				AppendText( InputRun.Text );
				AppendText( NEW_LINE );
				InputRun.Text = string.Empty;
				var output = interpreter( input );
				if (output != null) {
					if (!LastRun.Text.EndsWith( NEW_LINE )) AppendText( NEW_LINE );
					AppendText( output.TrimEnd(), ColorF2 );
					AppendText( NEW_LINE );
					_statusArea.Text = $"Saída: {output.Length} caracteres";
					CaretToEnd();
				}
				// pede um novo prompt somente se não existir um input handler.
				StartNewInputBuffer( usePrompt: _customInterpreter is null );
				if (output?.Length > 500) CaretToEnd();
			}
		}








		private void Handle_PasswordInput( KeyEventArgs e ) {
			CaretToEnd();
			if (e.Key == Key.Back) {
				if (_passwordInput.Length > 0)
					_passwordInput.Remove( _passwordInput.Length - 1, 1 );
			}
			else if (e.Key == Key.Return) {
				var interpreter = _customInterpreter is null ? CommandInterpreter : _customInterpreter;
				_customInterpreter = null;
				var input = _passwordInput.ToString();
				//var caretIndexBefore = CaretIndex;
				TypeText( NEW_LINE );
				var output = interpreter( input );
				_passwordInput.Clear();
				_usePasswordMask = false;
				if (output != null) {
					if (!LastRun.Text.EndsWith( NEW_LINE )) AppendText( NEW_LINE );
					AppendText( output.TrimEnd() );
					_statusArea.Text = $"Saída: {output.Length} caracteres";
				}
				// pede um novo prompt somente se não existir um input handler.
				StartNewInputBuffer( usePrompt: _customInterpreter is null );
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
	}
}
