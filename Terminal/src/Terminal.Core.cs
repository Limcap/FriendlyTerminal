using Limcap.Dux;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Limcap.TextboxTerminal {

	public partial class Terminal : INotifyPropertyChanged {

		private static readonly string NewLine = Environment.NewLine;
		private static readonly string PromptLine = NewLine + "> ";

		private readonly TextBox _mainArea;
		private readonly TextBox _statusArea;
		private readonly string _introText = "Limcap Terminal";
		private readonly Action _onExit;

		public event PropertyChangedEventHandler PropertyChanged;
		public DuxNamedList vars;
		private Func<string, string> _inputHandler; 

		public Panel Panel { get; private set; }

		//private int _lastLineCaretIndex;
		//private string _lastLine = string.Empty;
		//private readonly Func<string, string> _cmdProcessor = ( cmd ) => cmd;

		public Terminal( string introText, Action onExit ) {
			_introText = introText ?? _introText;
			_onExit = onExit;
			//_cmdProcessor = cmdProcessor ?? _cmdProcessor;
			_mainArea = BuildMainArea();
			_statusArea = BuildStatuArea();
			_cmdList = new Dictionary<string, Type>();

			Panel = new DockPanel() { LastChildFill = true };
			//panel.Children.Add( statusArea );
			Panel.Children.Add( _mainArea );
			DockPanel.SetDock( _statusArea, Dock.Bottom );
			DockPanel.SetDock( _mainArea, Dock.Top );

			Binding myBinding = new Binding {
				Source = this,
				Path = new PropertyPath( "Text" ),
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			};
			_ = BindingOperations.SetBinding( _mainArea, TextBox.TextProperty, myBinding );

			TypeText( introText + PromptLine );
		}




		private string _text;
		public string Text {
			get => _text;
			set {
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




		private int CaretIndex {
			get => _mainArea.CaretIndex - (_mainArea.Text.Length - LastLine.Length);
		}




		private char CurrentChar {
			get {
				var i = CaretIndex;
				var ll = LastLine;
				return i == ll.Length ? ' ' : ll[i];
			}
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
				Padding = new Thickness( 5 ),
				BorderThickness = new Thickness( 0 ),
				AcceptsReturn = false,
				VerticalAlignment = VerticalAlignment.Stretch
			};

			mainArea.PreviewKeyDown += ( object sender, KeyEventArgs e ) => {
				if (CaretIndex < 0) mainArea.CaretIndex = mainArea.Text.Length;

				if (e.IsRepeat)
					UpdateStatus( e.Key );

				if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.PageUp || e.Key == Key.PageDown) {
					e.Handled = true;
					return;
				}
				else if (e.Key == Key.Back || e.Key == Key.Left) {
					if (CaretIndex < NewLine.Length + 3) e.Handled = true;
					//else {
					//	Send( e.Key );
					//	e.Handled = true;
					//	UpdateStatus( e.Key );
					//}
				}
				else if (e.Key == Key.Home) {
					var lastLineStartIndex = mainArea.Text.LastIndexOf( NewLine );
					lastLineStartIndex = lastLineStartIndex < 0 ? 0 : lastLineStartIndex;
					mainArea.CaretIndex = lastLineStartIndex + NewLine.Length + 2;
					e.Handled = true;
				}
				else if (e.Key == Key.Return) {
					var input = LastLine.Replace( PromptLine, "" );
					UpdateStatus( e.Key );
					Text += NewLine;
					var output = _inputHandler is null ? ProcessInput( input ) : _inputHandler( input );
					if (output != null)
						mainArea.Text += NewLine + output + PromptLine;
					mainArea.CaretIndex = mainArea.Text.Length;
				}
				else {
					UpdateStatus( e.Key );
				}
			};

			mainArea.PreviewKeyUp += ( object sender, KeyEventArgs e ) => {
				UpdateStatus( e.Key );
			};

			mainArea.Loaded += ( object sender, RoutedEventArgs e ) => mainArea.Focus();

			//mainArea.PreviewMouseUp += ( object sender, MouseButtonEventArgs e ) => {
			//	if (LastLineCaretIndex < 0) mainArea.CaretIndex = mainArea.Text.Length;
			//};

			mainArea.CaretIndex = mainArea.Text.Length;

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




		public static void Send( Key key ) {
			if (Keyboard.PrimaryDevice != null) {
				if (Keyboard.PrimaryDevice.ActiveSource != null) {
					var e1 = new KeyEventArgs( Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Down ) { RoutedEvent = Keyboard.KeyDownEvent };
					InputManager.Current.ProcessInput( e1 );
				}
			}
		}




		private void UpdateStatus( Key pressedKey ) {
			//var lastLineStartIndex = mainArea.Text.LastIndexOf( NewLine );
			//lastLineStartIndex = lastLineStartIndex < 0 ? 0 : lastLineStartIndex;
			//_lastLine = mainArea.Text.Substring( lastLineStartIndex );
			//_lastLineCaretIndex = mainArea.CaretIndex - (mainArea.Text.Length - _lastLine.Length);
			//var curChar = _lastLineCaretIndex == _lastLine.Length ? ' ' : _lastLine[_lastLineCaretIndex];
			//statusArea.Text = $"{pressedKey} - {curChar} - ({_lastLineCaretIndex})";
			_statusArea.Text = $"{pressedKey} - {CurrentChar} - ({CaretIndex})";
		}
	}
}
