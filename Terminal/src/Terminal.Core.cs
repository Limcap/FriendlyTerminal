using Limcap.Dux;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Limcap.TextboxTerminal {

	public partial class Terminal : INotifyPropertyChanged {

		//private static readonly string NewLine = Environment.NewLine;
		private const string NewLine = "\n";
		private const string PromptString = "> ";

		private readonly string _introText = "Limcap Textbox Terminal";
		private readonly ScrollViewer _scrollArea;
		private readonly TextBox _mainArea;
		private readonly TextBox _statusArea;
		private readonly TextBox _traceArea;
		private int _minCaretIndex;
		private CmdHistory _cmdHistory = new CmdHistory();

		public Action onExit;
		public event PropertyChangedEventHandler PropertyChanged;
		public DuxNamedList vars;
		private Func<string, string> _inputHandler;
		private bool _usePasswordMask;
		private StringBuilder _passwordInput = new StringBuilder();

		public Panel Panel { get; private set; }




		public Terminal( string introText, Action onExit = null) {
			_introText = introText ?? _introText;
			this.onExit = onExit;
			vars = new DuxNamedList();
			_cmdList = new Dictionary<string, Type>();
			Panel = new DockPanel() { LastChildFill = true };
			
			_traceArea = BuildStatuArea();
			Panel.Children.Add( _traceArea );
			DockPanel.SetDock( _traceArea, Dock.Bottom );
			ShowTraceBar = false;

			_statusArea = BuildStatuArea();
			Panel.Children.Add( _statusArea );
			DockPanel.SetDock( _statusArea, Dock.Bottom );
			ShowStatusBar = true;

			_scrollArea = new ScrollViewer() { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
			_mainArea = BuildMainArea();
			_scrollArea.Content = _mainArea;
			Panel.Children.Add( _scrollArea );
			DockPanel.SetDock( _mainArea, Dock.Top );

			Binding myBinding = new Binding {
				Source = this,
				Path = new PropertyPath( "Text" ),
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			};
			_ = BindingOperations.SetBinding( _mainArea, TextBox.TextProperty, myBinding );

			RegisterCommand<Raise>();
			RegisterCommand<ToggleStatusBar>();
			RegisterCommand<ToggleTraceBar>();
			RegisterCommand<SaveOutput>();

			Text += _introText;
			StartNewInput();
		}



		private string _oldText;
		private string _text;
		public string Text {
			get => _text;
			set {
				_oldText = _text;
				_text = value;
				OnPropertyChanged( "Text" );
			}
		}




		private string LastLine {
			get {
				if (_mainArea.Text.Length == 0) return string.Empty;
				var lastLineStartIndex = _mainArea.Text.LastIndexOf( NewLine );
				lastLineStartIndex = lastLineStartIndex < 0 ? 0 : lastLineStartIndex;
				return _mainArea.Text.Substring( lastLineStartIndex );
			}
		}




		//private bool _showStatusArea = true;
		//public bool ShowStatusArea {
		//	get => _showStatusArea;
		//	set {
		//		_showStatusArea = value;
		//		_statusArea.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
		//	}
		//}

		//private int _lastLineCaretIndex;
		//private string _lastLine = string.Empty;
		//private readonly Func<string, string> _cmdProcessor = ( cmd ) => cmd;

		//private int CaretIndex {
		//	get => _mainArea.CaretIndex - (_mainArea.Text.Length - LastLine.Length);
		//}

		//private char CurrentChar {
		//	get {
		//		var i = CaretIndex;
		//		var ll = LastLine;
		//		return i == ll.Length ? ' ' : ll[i];
		//	}
		//}
		
		


		private int CaretIndex {
			get => _mainArea.CaretIndex;
			set => _mainArea.CaretIndex = value;
		}




		private char CurrentChar {
			get => CaretIndex == Text.Length ? ' ' : Text[CaretIndex];
		}




		public void OnPropertyChanged( string propertyName ) {
			var handler = PropertyChanged;
			if (handler != null) {
				handler( this, new PropertyChangedEventArgs( propertyName ) );
			}
		}




		private TextBox BuildMainArea() {
			var mainArea = new TextBox() {
				Background = new SolidColorBrush( Color.FromArgb( 200, 25, 27, 27 ) ),
				Foreground = Brushes.GreenYellow,
				FontFamily = new FontFamily( "Consolas" ),
				FontSize = 14,
				Padding = new Thickness( 5 ),
				BorderThickness = new Thickness( 0 ),
				AcceptsReturn = false,
				VerticalAlignment = VerticalAlignment.Stretch
			};

			mainArea.PreviewKeyDown += HandleRegularInput;
			mainArea.PreviewKeyUp += ( object sender, KeyEventArgs e ) => UpdateDebugArea( e.Key );
				//if (_usePasswordMask) {
				//	if (Text[Text.Length - 1] == 'p') Text = Text.Remove(Text.Length-1) + '*';
				//	CaretIndex = Text.Length;
				//}
			mainArea.Loaded += ( object sender, RoutedEventArgs e ) => mainArea.Focus();
			return mainArea;
		}




		private TextBox BuildStatuArea() {
			return new TextBox() {
				Background = new SolidColorBrush( Color.FromRgb( 25, 27, 27 ) ),
				Foreground = Brushes.Gray,
				FontFamily = new FontFamily( "Consolas" ),
				Padding = new Thickness( 5 ),
				BorderThickness = new Thickness( 0 ),
				AcceptsReturn = false,
				Height = 25,
				IsReadOnly = true,
			};
		}




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




		private void HandleRegularInput( object sender, KeyEventArgs e ) {
			var shift = Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown(Key.RightCtrl);

			if (e.IsRepeat)
				UpdateDebugArea( e.Key );

			if (_usePasswordMask) {
				HandlePasswordInput( e );
				return;
			}

			if (e.Key == Key.Up) {
				if (_inputHandler == null && shift) {
					if (_cmdHistory.TempText is null) _cmdHistory.TempText = Text;
					Text = _cmdHistory.TempText + _cmdHistory.Prev();
					CaretIndex = Text.Length;
					e.Handled = true;
				}
			}

			else if (e.Key == Key.Down) {
				if (_inputHandler == null && shift) {
					if (_cmdHistory.TempText is null) _cmdHistory.TempText = Text;
					Text = _cmdHistory.TempText + _cmdHistory.Down();
					CaretIndex = Text.Length;
					e.Handled = true;
				}
			}

			else if(e.Key.IsIn(Key.Left,Key.Right)) {

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
