using Limcap.Dux;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Cmds = Limcap.UTerminal.Cmds;

namespace Limcap.UTerminal {

	public partial class Terminal : INotifyPropertyChanged {

		//private static readonly string NewLine = Environment.NewLine;
		public const string NEW_LINE = "\n";
		public const string PROMPT_STRING = "> ";
		public const string INSUFICIENT_PRIVILEGE_MESSAGE = "Este comando requer um nível de privilégio maior do que o definido no momento.";

		private readonly string _introText = "Limcap Utility Terminal";
		private readonly ScrollViewer _scrollArea;
		private readonly TextBox _mainArea;
		private readonly TextBox _statusArea;
		private readonly TextBox _traceArea;
		private readonly HistoryNavigator _cmdHistory = new HistoryNavigator();
		//private CommandPredictor _cmdAssist;
		public Assistant _assistant;
		private bool _allowAssistant;
		//private ParameterTypeAssistant _paramAssist;
		private int _bufferStartIndex;

		public Action onExit;
		public event PropertyChangedEventHandler PropertyChanged;
		public DuxNamedList vars;
		private Func<string, string> _inputHandler;
		private bool _usePasswordMask;
		private readonly StringBuilder _passwordInput = new StringBuilder();

		public Panel Panel { get; private set; }
		public int CurrentPrivilege { get; set; }
		public string Locale { get; set; }


		public Brush FontColor { get => _mainArea.Foreground; set => _mainArea.Foreground = value; }
		public Brush BackColor { get => _mainArea.Background; set => _mainArea.Background = value; }
		public double FontSize { get => _mainArea.FontSize; set => _mainArea.FontSize = value; }




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
			_mainArea.IsEnabled = false;
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
		}




		public void Start() {
			//_paramAssist = new ParameterTypeAssistant( _cmdList, Locale );
			_assistant = new Assistant( _cmdList, Locale );
			_mainArea.IsEnabled = true;
			AppendText( _introText );
			StartNewInputBuffer();
			_statusArea.Text = _assistant.GetPredictions( string.Empty ).ToString();
		}




		public string[] AvailableCommands {
			get => _cmdList.Select( entry => entry.Key ).ToArray();
		}




		private string _text;
		public string Text {
			get => _text;
			set {
				_text = value;
				OnPropertyChanged( "Text" );
			}
		}




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




		public string GetInputBuffer() {
			var lastLineStartIndex = _mainArea.GetCharacterIndexFromLineIndex( _mainArea.LineCount - 1 );
			var lastLine = _mainArea.GetLineText( _mainArea.LineCount - 1 ) ?? string.Empty;
			var lasLineBufferStartIndex = _bufferStartIndex - lastLineStartIndex;
			if (lasLineBufferStartIndex < 0) return string.Empty;
			return lastLine == string.Empty ? lastLine : lastLine.Substring( lasLineBufferStartIndex );
			//return _mainArea.Text.Substring( _bufferStartIndex );
		}
		//public PString InputBuffer {
		//	get => ((PString)_mainArea.Text).Slice( _bufferStartIndex );
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
			//mainArea.TextChanged += ( o, a ) => AdvanceAutoComplete( a );
			mainArea.TextChanged += HandleTextChanged;
			mainArea.SelectionChanged += HandleSelectionChanged;
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
