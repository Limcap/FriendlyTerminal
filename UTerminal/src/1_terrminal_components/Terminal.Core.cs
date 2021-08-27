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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Cmds = Limcap.FTerminal.Cmds;

namespace Limcap.FTerminal {

	public partial class Terminal : INotifyPropertyChanged {

		public const string NEW_LINE = "\n";
		public const string PROMPT_STRING = "» ";//›»
		public const string INSUFICIENT_PRIVILEGE_MESSAGE = "Este comando requer um nível de privilégio maior do que o definido no momento.";

		private readonly string _introText = "Limcap Friendly Terminal";
		private readonly ScrollViewer _scrollArea;
		private TextBlock _mainArea;
		private readonly TextBlock _assistantArea;
		private readonly TextBlock _statusArea;
		private readonly HistoryNavigator _cmdHistory = new HistoryNavigator(15);
		public Assistant _assistant;
		private bool _allowAssistant;

		public Action onExit;
		public event PropertyChangedEventHandler PropertyChanged;
		public DuxNamedList vars;
		private Func<string, string> _customInterpreter;
		private bool _usePasswordMask;
		private readonly StringBuilder _passwordInput = new StringBuilder();


		public Brush ColorF1 { get; set; }
		public Brush ColorF2 { get; set; }
		public Brush ColorB1 { get; set; }
		public readonly Run CaretRun;
		public readonly Run InputBufferRun;
		public InlineCollection Inlines => _mainArea.Inlines;
		public Run LastRun => Inlines.LastInline.PreviousInline.PreviousInline as Run;
		public string Text => _mainArea.Text;




		public Panel Panel { get; private set; }
		public int CurrentPrivilege { get; set; }
		public string Locale { get; set; }




		public Brush FontColor { get => _mainArea.Foreground; set => _mainArea.Foreground = value; }
		public Brush BackColor { get => _mainArea.Background; set => _mainArea.Background = value; }
		public double FontSize { get => _mainArea.FontSize; set => _mainArea.FontSize = value; }








		public Terminal( string introText, Action onExit = null ) {
			ColorF1 = new SolidColorBrush( Color.FromRgb( 171, 255, 46 ) );
			ColorF2 = new SolidColorBrush( Color.FromRgb( 120, 179, 32 ) );
			ColorB1 = new SolidColorBrush( Color.FromArgb( 200, 25, 27, 27 ) );
			CaretRun = new Run( "█" ) { IsEnabled = false, Background = ColorF1 };//█
			InputBufferRun = new Run( String.Empty );

			_introText = introText ?? _introText;
			this.onExit = onExit;
			vars = new DuxNamedList();
			_cmdList = new Dictionary<string, Type>();
			Panel = new DockPanel() { LastChildFill = true };

			_assistantArea = BuildStatuArea();
			Panel.Children.Add( _assistantArea );
			DockPanel.SetDock( _assistantArea, Dock.Bottom );
			ShowAssistBar = true;

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

			_scrollArea.GotFocus += ( o, a ) => _mainArea.Focus();
			//Binding myBinding = new Binding {
			//	Source = this,
			//	Path = new PropertyPath( "Text" ),
			//	Mode = BindingMode.TwoWay,
			//	UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			//};
			//_ = BindingOperations.SetBinding( _mainArea, TextBox.TextProperty, myBinding );
		}








		public void Start() {
			_assistant = new Assistant( _cmdList, Locale );
			_mainArea.IsEnabled = true;
			AppendText( _introText );
			StartNewInputBuffer();
			_assistantArea.Text = _assistant.GetPredictions( string.Empty ).ToString();

			_mainArea.Focus();
		}








		public string[] AvailableCommands {
			get => _cmdList.Select( entry => entry.Key ).ToArray();
		}








		public string Status {
			get => _assistantArea.Text;
			set => _assistantArea.Text = value;
		}








		public bool IsControlDown {
			get => Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl );
		}








		public bool IsShiftDown {
			get => Keyboard.IsKeyDown( Key.LeftShift ) || Keyboard.IsKeyDown( Key.RightShift );
		}








		private char CurrentChar {
			//get => CaretIndex == Text.Length ? ' ' : Text[CaretIndex];
			get => '-';
		}








		public void OnPropertyChanged( string propertyName ) {
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}








		private TextBlock BuildMainArea() {
			var mainArea = new TextBlock() {
				Background = ColorB1,
				Foreground = ColorF1,
				FontFamily = new FontFamily( "Consolas" ),
				FontSize = 14,
				Padding = new Thickness( 5 ),
				//BorderThickness = new Thickness( 0 ),
				//AcceptsReturn = false,
				//AcceptsTab = true,
				VerticalAlignment = VerticalAlignment.Stretch,
				//SelectionBrush = Brushes.White,
				//IsUndoEnabled = false,
				//UndoLimit = 0,
				IsEnabled = true,
				Focusable = true,
			};
			mainArea.Inlines.Add( InputBufferRun );
			mainArea.Inlines.Add( CaretRun );

			mainArea.PreviewMouseDown += ( o, a ) =>(o as TextBlock).Focus();
			mainArea.Loaded += ( o, a ) => mainArea.Focus();
			mainArea.PreviewKeyDown += Handle_KeyboardInput;
			//mainArea.PreviewKeyUp += ( o, a ) => UpdateTraceArea( a.Key );
			mainArea.PreviewKeyUp += ( o, a ) => {
				if (a.Key.IsIn( Key.Down, Key.PageDown )) {
					//var curlineIndex = mainArea.GetLineIndexFromCharacterIndex( CaretIndex );
					//var lastLineIndex = mainArea.GetLineIndexFromCharacterIndex( _bufferStartIndex );
					//if (curlineIndex == lastLineIndex)
						_scrollArea.ScrollToBottom();
				}
			};
			//mainArea.TextChanged += HandleTextChanged;
			return mainArea;
		}








		private TextBlock BuildStatuArea() {
			return new TextBlock() {
				Background = new SolidColorBrush( Color.FromRgb( 25, 27, 27 ) ),
				Foreground = Brushes.Gray,
				FontFamily = new FontFamily( "Consolas" ),
				Padding = new Thickness( 5 ),
				//BorderThickness = new Thickness( 0 ),
				//AcceptsReturn = false,
				Height = 25,
				//IsReadOnly = true,
				//IsUndoEnabled = false,
				//UndoLimit = 0,
			};
		}
	}
}
