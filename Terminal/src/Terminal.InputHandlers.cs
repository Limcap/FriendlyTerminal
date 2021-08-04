﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Limcap.TextboxTerminal {
	public partial class Terminal {

		private void HandlePasswordInput( KeyEventArgs e ) {
			//var shift = Keyboard.IsKeyDown( Key.LeftShift ) || Keyboard.IsKeyDown( Key.RightShift );
			CaretIndex = Text.Length;
			if (e.Key == Key.Back) {
				if (CaretIndex <= _minCaretIndex) {
					CaretIndex = Text.Length;
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
				Text += NewLine;
				var output = processor( input );
				_passwordInput.Clear();
				_usePasswordMask = false;
				if (output != null) {
					if (!Text.EndsWith( NewLine )) Text += NewLine;
					Text += output.TrimEnd();
					_statusArea.Text = $"Saída: {output.Length} caracteres";
				}
				// pede um novo prompt somente se não existir um input handler.
				StartNewInput( usePrompt: _inputHandler is null );
				if (output.Length > 500) CaretIndex = caretIndexBefore;
			}
			else {
				var c = e.Key.ToPasswordAllowedChar();
				if (c.HasValue) {
					Text += '*';
					CaretIndex = Text.Length;
					_passwordInput.Append( c );
				}
				e.Handled = true;
			}
		}




		private void ClearInputBuffer() {
			//var brush = _mainArea.SelectionBrush;
			//_mainArea.SelectionBrush = System.Windows.Media.Brushes.Transparent;
			_mainArea.SelectionOpacity = 0;
			_mainArea.Select( _minCaretIndex, _mainArea.Text.Length - _minCaretIndex );
			_mainArea.SelectedText = string.Empty;
			_mainArea.SelectionOpacity = 0.5;
			//_mainArea.SelectionBrush = brush;
			//_mainArea.Cut();
		}
		private void SetInputBuffer( string text ) {
			ClearInputBuffer();
			_mainArea.AppendText( text );
		}




		private void HandleRegularInput( object sender, KeyEventArgs e ) {
			var control = Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl );
			_mainArea.SelectionBrush = System.Windows.Media.Brushes.Green;

			if (e.IsRepeat)
				UpdateDebugArea( e.Key );

			if (_usePasswordMask) {
				HandlePasswordInput( e );
				return;
			}

			if (e.Key == Key.PageUp || e.Key == Key.Up && control) {
				if (_inputHandler == null) {
					SetInputBuffer( _cmdHistory.Prev() );
					CaretToEnd();
					e.Handled = true;
				}
			}

			else if (e.Key == Key.PageDown || e.Key == Key.Down && control) {
				if (_inputHandler == null) {
					SetInputBuffer( _cmdHistory.Next() );
					CaretToEnd();
					e.Handled = true;
				}
			}

			else if (e.Key == Key.Tab || e.Key == Key.Return && control) {
				if (_cmdHistory.IsSelected) {
					Status = string.Empty;
					Text += _cmdHistory.Current();
					_cmdHistory.Deselect();
					CaretToEnd();
				}
				e.Handled = true;
			}

			else if (e.Key.IsIn( Key.Left, Key.Right )) {

			}

			else if (e.Key == Key.PageUp || e.Key == Key.PageDown) {
				e.Handled = true;
			}

			else if (e.Key == Key.Back || e.Key == Key.Left) {
				if (CaretIndex <= _minCaretIndex) e.Handled = true;
			}

			else if (e.Key == Key.Home) {
				//var lastLineStartIndex = mainArea.Text.LastIndexOf( NewLine );
				//lastLineStartIndex = lastLineStartIndex < 0 ? 0 : lastLineStartIndex;
				CaretIndex = _minCaretIndex;
				e.Handled = true;
			}

			else if (e.Key == Key.Return) {
				//var input = LastLine.Replace( NewLine, "" ).Replace( "> ", "" );
				var input = Text.Substring( _minCaretIndex );
				UpdateDebugArea( e.Key );
				//Text += NewLine;
				if (input.Trim().Length == 0) {
					e.Handled = true;
					// pede um novo prompt somente se não existir um input handler.
					StartNewInput( usePrompt: _inputHandler is null );
				}
				else {
					_cmdHistory.Add( input );
					// Se houver um inputHandler esperando por input, copia ele para a variavel processos e já reseta
					// oo _inputHandler para que caso o handler atual defina outro inputHandler, não haja problema de 
					// substituição e para que 
					var processor = _inputHandler is null ? ProcessInput : _inputHandler;
					_inputHandler = null;
					var caretIndexBefore = CaretIndex;
					Text += NewLine;
					var output = processor( input );
					if (output != null) {
						if (!Text.EndsWith( NewLine )) Text += NewLine;
						Text += output.TrimEnd();
						_statusArea.Text = $"Saída: {output.Length} caracteres";
					}
					// pede um novo prompt somente se não existir um input handler.
					StartNewInput( usePrompt: _inputHandler is null );
					if (output?.Length > 500) CaretIndex = caretIndexBefore;
				}

			}
			else if (e.Key.IsIn( Key.LeftShift, Key.RightShift, Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl, Key.CapsLock, Key.Insert, Key.RWin, Key.LWin )) {
			}
			else {
				if (CaretIndex < _minCaretIndex) CaretIndex = Text.Length;
				//if (_usePasswordMaskForInput && e.Key != Key.Back && e.Key != Key.Delete ) {
				//	char? c = e.Key.ToPasswordAllowedChar();
				//	if (c.HasValue) Text += c.Value;
				//	//if (c.HasValue) input.Append( c.Value );
				//	e.Handled = true;
				//}
				UpdateDebugArea( e.Key );
			}
		}
	}
}
