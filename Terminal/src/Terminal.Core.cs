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

		private readonly string _introText = "Limcap Textbox Terminal";
		private readonly ScrollViewer _scrollArea;
		private readonly TextBox _mainArea;
		private readonly TextBox _statusArea;
		private readonly TextBox _debugArea;
		private int _minCaretIndex;

		public Action onExit;
		public event PropertyChangedEventHandler PropertyChanged;
		public DuxNamedList vars;
		private Func<string, string> _inputHandler; 

		public Panel Panel { get; private set; }

		private bool _showStatusArea = true;
		public bool ShowStatusArea {
			get => _showStatusArea;
			set {
				_showStatusArea = value;
				_statusArea.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		//private int _lastLineCaretIndex;
		//private string _lastLine = string.Empty;
		//private readonly Func<string, string> _cmdProcessor = ( cmd ) => cmd;

		public Terminal( string introText, Action onExit = null) {
			_introText = introText ?? _introText;
			this.onExit = onExit;
			vars = new DuxNamedList();
			_cmdList = new Dictionary<string, Type>();
			Panel = new DockPanel() { LastChildFill = true };
			
			_debugArea = BuildStatuArea();
			//Panel.Children.Add( _debugArea );
			DockPanel.SetDock( _debugArea, Dock.Bottom );

			_statusArea = BuildStatuArea();
			Panel.Children.Add( _statusArea );
			DockPanel.SetDock( _statusArea, Dock.Bottom );
			ShowStatusArea = ShowStatusArea;
			//_statusArea.Visibility = _showStatusArea ? Visibility.Visible : Visibility.Collapsed;

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

			//TypeText( _introText );
			Text += _introText;
			StartNewInputLine();
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
				if (e.IsRepeat)
					UpdateDebugArea( e.Key );

				if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.PageUp || e.Key == Key.PageDown) {
					e.Handled = true;
					return;
				}
				else if (e.Key == Key.Back || e.Key == Key.Left) {
					if (mainArea.CaretIndex <= _minCaretIndex) e.Handled = true;
				}
				else if (e.Key == Key.Home) {
					//var lastLineStartIndex = mainArea.Text.LastIndexOf( NewLine );
					//lastLineStartIndex = lastLineStartIndex < 0 ? 0 : lastLineStartIndex;
					mainArea.CaretIndex = _minCaretIndex;
					e.Handled = true;
				}
				else if (e.Key == Key.Return) {
					var input = LastLine.Replace( PromptLine, "" );
					UpdateDebugArea( e.Key );
					Text += NewLine;
					if (input.Trim().Length == 0) {
						e.Handled = true;
						//return;
					}
					else {
						var output = _inputHandler is null ? ProcessInput( input ) : _inputHandler( input );
						if (output != null) {
							mainArea.Text += output;
						}
					}
					StartNewInputLine();
					//mainArea.CaretIndex = mainArea.Text.Length;
				}
				else {
					if (mainArea.CaretIndex < _minCaretIndex) mainArea.CaretIndex = mainArea.Text.Length;
					UpdateDebugArea( e.Key );
				}
			};

			mainArea.PreviewKeyUp += ( object sender, KeyEventArgs e ) => {
				UpdateDebugArea( e.Key );
			};

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
	}
}
