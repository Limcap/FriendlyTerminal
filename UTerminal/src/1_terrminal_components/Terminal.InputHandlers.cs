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
				TypeText( NEW_LINE );
				var output = processor( input );
				_passwordInput.Clear();
				_usePasswordMask = false;
				if (output != null) {
					if (!Text.EndsWith( NEW_LINE )) AppendText( NEW_LINE );
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
					var caretIndexBefore = CaretIndex;
					AppendText( NEW_LINE );
					var output = processor( input );
					if (output != null) {
						if (!Text.EndsWith( NEW_LINE )) AppendText( NEW_LINE );
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




		private void HandleTextChanged( object sender, TextChangedEventArgs args ) {
			if (!_allowAssistant) return;
			var input = GetInputBuffer();
			_statusArea.Text = _assistant.GetPredictions( input ).ToString();

			//if (input.Count() > 0 && input.Contains( ':' ) && input.Last() != ':') {
			//	var autocompleteString = _assistant.GetPredictions( input );
			//	_statusArea.Text = autocompleteString.ToString();
			//}
			//else if (_useCmdAssist)
			//	_statusArea.Text = _assistant.GetPredictions( input ).ToString();

			//if (InputBuffer.Contains( ':' )) {
			//	var inputParts = InputBuffer.Split( ':' );
			//	var invokeText = inputParts[0];
			//	var parameters = inputParts.Length > 0 ? inputParts[1] : string.Empty;
			//	if (!_cmdList.ContainsKey( inputParts[0] )) {
			//		_statusArea.Text = "Command not found";
			//		return;
			//	}
			//	var cmdType = _cmdList[invokeText];
			//	var instance = cmdType.IsSubclassOf( typeof( ACommand ) )
			//	? (ACommand)Activator.CreateInstance( cmdType, Locale )
			//	: null;
			//	var output = instance?.Parameters?.Aggregate( string.Empty, ( agg, p ) => agg += (p.optional ? $"[{p.name}=]" : $"{p.name}=") + "     " );
			//	_statusArea.Text = output;
			//}
		}




		private void HandleSelectionChanged( object sender, RoutedEventArgs args ) {
			var isHelperSelection = !(_mainArea.SelectionStart != _bufferStartIndex || _mainArea.SelectionLength != _mainArea.Text.Length - _bufferStartIndex);
			_mainArea.SelectionOpacity = isHelperSelection ? 0 : 0.5;
		}




		//private void AdvanceAutoComplete( TextChangedEventArgs a ) {
		//	if (InputBuffer.Length == 0) {
		//		_statusArea.Text = string.Empty;
		//		return;
		//	}
		//	var candidates = _cmdList.Where( c => c.Key.StartsWith( InputBuffer ) ).ToList();
		//	string output = string.Empty;
		//	foreach (var candidate in candidates) {
		//		var threshold = candidate.Key.IndexOf( " ", InputBuffer.Length - 1 );
		//		//threshold = threshold == -1 ? candidate.Key.Length - 1 : threshold;
		//		output += threshold == -1 ? candidate.Key : candidate.Key.Remove( threshold );
		//		output += "          ";
		//	}
		//	_statusArea.Text = output;
		//}




		//private void SelectNextAutocomplete() {
		//	var options = _statusArea.Text.Split( new string[] { "          " }, StringSplitOptions.None );
		//	SetInputBuffer( options[0] + ' ' );
		//	CaretToEnd();
		//}


		//private void AdvanceAutoComplete( KeyEventArgs a = null ) {
		//	if (a != null && !a.IsRepeat && CaretIndex < _bufferStartIndex) {
		//		if (InputBuffer.Length == 0) return;
		//		var candidates = _cmdList.Where( c => c.Key.StartsWith( InputBuffer ) ).ToList();
		//		string output = string.Empty;
		//		foreach (var candidate in candidates) {
		//			var threshold = candidate.Key.IndexOf( " ", InputBuffer.Length - 1 );
		//			threshold = threshold == -1 ? candidate.Key.Length - 1 : threshold;
		//			output += candidate.Key.Remove( threshold ) + "          ";
		//		}
		//		_statusArea.Text = output;
		//	}
		//}
	}
}
