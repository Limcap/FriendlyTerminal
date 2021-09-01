﻿using System;
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
			var c = KeyGrabber.GetCharFromKey( e.Key );
			if (c != 0)
				_screen.AppendText( c.ToString() );
			Handle3_TextChanged();
		}








		private void Handle2_DeleteFromBuffer( KeyEventArgs e ) {
			//InputRun.ContentEnd.GetPositionAtOffset( -1 ).DeleteTextInRun( 1 );
			_screen.Backspace();
			Handle3_TextChanged();
		}








		private void Handle3_TextChanged() {//object sender, TextChangedEventArgs args 
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
			var input = _screen.Buffer;
			if (IsAutocompleting) {
				IsAutocompleting = false;
				_assistant.ProcessInput( input );
			}
			if (((PString)input).Trim().len == 0) {
				e.Handled = true;
				// pede um novo prompt somente se não existir um input handler.
				// cria um novo prompt sem criar um novo paragrafo.
				NewPrompt( false );
			}
			else {
				_cmdHistory.Add( input );
				// Se houver um interpretador customizado (que pode ser definido pelos comandos) copia sua ref para uma
				// variável local e já anula a referencia principal, para que caso este interpretador customizado queria
				// definir ainda um outro, não haja problema de sobrescrever.
				var interpreter = _customInterpreter is null ? CommandInterpreter : _customInterpreter;
				_customInterpreter = null;
				_screen.NewBlock( ColorF2 );
				var output = interpreter( input );
				if (output != null) {
					if (output.Length > 100001) {
						
						var w = new Window();
						var f = new FlowDocument();
						w.Content = f;
						f.ContentStart.InsertTextInRun( output );
						w.Show();
					}
					else {
						_screen.AppendText( output );
						_statusArea.Text = $"Saída: {output.Length} caracteres";
					}
					ScrollToEnd();
				}
				// pede um novo prompt somente se não existir um input handler.
				//StartNewPrompt( usePrompt: _customInterpreter is null );
				if(_customInterpreter is null) NewPrompt();
				if (output?.Length > 500) ScrollToEnd();
				Handle3_TextChanged();
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