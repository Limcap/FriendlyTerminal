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
		private const string NEW_LINE = "\n";
		private const string PROMPT_STRING = "> ";
		public const string INSUFICIENT_PRIVILEGE_MESSAGE = "Este comando requer um nível de privilégio maior do que o definido no momento.";

		private readonly string _introText = "Limcap Textbox Terminal";
		private readonly ScrollViewer _scrollArea;
		private readonly TextBox _mainArea;
		private readonly TextBox _statusArea;
		private readonly TextBox _traceArea;
		private int _bufferStartIndex;
		private readonly CmdHistory _cmdHistory = new CmdHistory();

		public Action onExit;
		public event PropertyChangedEventHandler PropertyChanged;
		public DuxNamedList vars;
		private Func<string, string> _inputHandler;
		private bool _usePasswordMask;
		private readonly StringBuilder _passwordInput = new StringBuilder();

		public Panel Panel { get; private set; }
		public int CurrentPrivilege { get; set; }




		public Terminal( string introText, Action onExit = null ) {
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
			RegisterCommand<Print_a_lot>();

			AppendText( _introText );
			StartNewInputBuffer();
		}



		private string _text;
		public string Text {
			get => _text;
			set {
				_text = value;
				OnPropertyChanged( "Text" );
			}
		}

		//private StringBuilder _text = new StringBuilder();
		//public string Text {
		//	get => _text.ToString();
		//	set {
		//		_text.Clear();
		//		_text.Append( value );
		//		OnPropertyChanged( "Text" );
		//	}
		//}




		public string Status {
			get => _statusArea.Text;
			set => _statusArea.Text = value;
		}




		public string LastLine {
			get {
				if (_mainArea.Text.Length == 0) return string.Empty;
				var lastLineStartIndex = _mainArea.Text.LastIndexOf( NEW_LINE ) + NEW_LINE.Length;
				return _mainArea.Text.Substring( lastLineStartIndex );
			}
		}




		public string InputBuffer {
			get => _mainArea.Text.Substring( _bufferStartIndex );
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




		public bool IsControlDown {
			get => Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl );
		}




		public bool IsShiftDown {
			get => Keyboard.IsKeyDown( Key.LeftShift ) || Keyboard.IsKeyDown( Key.RightShift );
		}




		private char CurrentChar {
			get => CaretIndex == Text.Length ? ' ' : Text[CaretIndex];
		}




		public void OnPropertyChanged( string propertyName ) {
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
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
				AcceptsTab = true,
				VerticalAlignment = VerticalAlignment.Stretch,
				SelectionBrush = Brushes.White,
			};

			mainArea.Loaded += ( o, a ) => mainArea.Focus();
			mainArea.PreviewKeyDown += HandleRegularInput;
			mainArea.PreviewKeyUp += ( o, a ) => UpdateTraceArea( a.Key );
			mainArea.PreviewKeyUp += ( o, a ) => {
				if (a.Key.IsIn( Key.Down, Key.PageDown )) {
					var curlineIndex = mainArea.GetLineIndexFromCharacterIndex( CaretIndex );
					var lastLineIndex = mainArea.GetLineIndexFromCharacterIndex( _bufferStartIndex );
					if (curlineIndex == lastLineIndex)
						_scrollArea.ScrollToBottom();
				}
			};
			mainArea.SelectionChanged += ( o, a ) => {
				var isHelperSelection = !(mainArea.SelectionStart != _bufferStartIndex || mainArea.SelectionLength != mainArea.Text.Length - _bufferStartIndex);
				_mainArea.SelectionOpacity = isHelperSelection ? 0 : 0.5;
			};
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
