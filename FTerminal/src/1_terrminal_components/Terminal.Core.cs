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

		private ITerminalScreen _screen;
		//private FlowDocumentScrollViewer _mainContainer;
		//private ScrollViewer _scrollview => _mainContainer.Template.FindName( "PART_ContentHost", _mainContainer ) as ScrollViewer;
		//private FlowDocument _mainArea => _mainContainer.Document;
		//public BlockCollection Blocks => _mainArea?.Blocks;
		//public readonly Run CaretRun = new Run( "█" );
		//public Paragraph LastParagraph => Blocks.LastBlock as Paragraph; //CaretRun.Parent as Paragraph;
		//private Run LastConsolidatedRun => CaretRun.PreviousInline.PreviousInline as Run;
		//public Run InputBufferRun => CaretRun.PreviousInline as Run;


		private readonly TextBlock _assistantArea;
		private readonly TextBlock _statusArea;
		private readonly HistoryNavigator _cmdHistory = new HistoryNavigator( 15 );
		public Assistant _assistant;
		private bool _allowAssistant = true;

		public Action onExit;
		public event PropertyChangedEventHandler PropertyChanged;
		public DuxNamedList vars;
		private Func<string, string> _customInterpreter;
		private bool _customInterpreterIsActive;
		private bool _usePasswordMask;
		private readonly StringBuilder _passwordInput = new StringBuilder();


		public Brush DefaultMainFontColor { get; set; } = new SolidColorBrush( Color.FromRgb( 171, 255, 46 ) );
		//public Brush ColorF2 { get; set; } = new SolidColorBrush( Color.FromArgb( 100, 171, 255, 46 ) ); //120,179,32
		public Brush DefaultBackColor { get; set; } = new SolidColorBrush( Color.FromArgb( 255, 20, 22, 22 ) ); //25,27,27 

		public string Text => _screen.ToString();




		public Panel Panel { get; private set; }
		public int CurrentPrivilege { get; set; }
		public string Locale { get; set; }




		//public Brush FontColor { get => _screen.BufferFontColor; set => (_screen as TerminalScreenV05).SetFontColor(value); }
		//public int FontSize { get => (int)_screen.DefaultFontSize; set => (_screen as TerminalScreenV05).SetFontSize(value); }
		public double FontSize { get => _screen.DefaultFontSize; set => _screen.DefaultFontSize = value; }
		public Brush BackColor { get => _screen.BackgroundColor; set => _screen.BackgroundColor = value; }
		public Brush FontColor {
			get => _screen.DefaultFontColor;
			set {
				var c = ((SolidColorBrush)value).Color;
				var newFaded = new SolidColorBrush( Color.FromArgb( 100, c.R, c.G, c.B ) );
				_screen.SwapFontColor( FadedFontColor, newFaded );
				_screen.DefaultFontColor = value;
				FadedFontColor = newFaded;
			}
		}
		public SolidColorBrush FadedFontColor { get; private set; }








		public Terminal( string introText, Action onExit = null ) {
			//ColorF1 = new SolidColorBrush( Color.FromRgb( 171, 255, 46 ) );
			//ColorF2 = new SolidColorBrush( Color.FromRgb( 120, 179, 32 ) );
			//ColorB1 = new SolidColorBrush( Color.FromArgb( 200, 25, 27, 27 ) );
			//InputBufferRun = new Run( String.Empty );

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

			_screen = BuildTextScreen();
			Panel.Children.Add( _screen.UIControlHook );
			DockPanel.SetDock( _screen.UIControlHook, Dock.Top );
			//_screen.View.GotFocus += ( o, a ) => _screen.Doc.Focus();

			FontColor = DefaultMainFontColor;

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
			_screen.AppendText( _introText );
			NewPrompt();
			_assistantArea.Text = _assistant.GetPredictions( string.Empty ).ToString();
			_screen.Focus();
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








		private ITerminalScreen BuildTextScreen() {
			var screen = new TerminalScreenV05() {
				BackgroundColor = DefaultBackColor,
				DefaultFontColor = DefaultMainFontColor,
				DefaultFontSize = 14,
			};
			screen.OnPreviewKeyDown += Handle_KeyboardInput;
			return screen;
		}


		//private TextBlock BuildMainArea() {
		//	var mainArea = new TextBlock() {
		//		Background = ColorB1,
		//		Foreground = ColorF1,
		//		FontFamily = new FontFamily( "Consolas" ),
		//		FontSize = 14,
		//		Padding = new Thickness( 5 ),
		//		//BorderThickness = new Thickness( 0 ),
		//		//AcceptsReturn = false,
		//		//AcceptsTab = true,
		//		VerticalAlignment = VerticalAlignment.Stretch,
		//		//SelectionBrush = Brushes.White,
		//		//IsUndoEnabled = false,
		//		//UndoLimit = 0,
		//		IsEnabled = true,
		//		Focusable = true,
		//	};
		//	mainArea.Inlines.Add( InputBufferRun );
		//	mainArea.Inlines.Add( CaretRun );

		//	mainArea.PreviewMouseDown += ( o, a ) =>(o as TextBlock).Focus();
		//	mainArea.Loaded += ( o, a ) => mainArea.Focus();
		//	mainArea.PreviewKeyDown += Handle_KeyboardInput;
		//	//mainArea.PreviewKeyUp += ( o, a ) => UpdateTraceArea( a.Key );
		//	mainArea.PreviewKeyUp += ( o, a ) => {
		//		if (a.Key.IsIn( Key.Down, Key.PageDown )) {
		//			//var curlineIndex = mainArea.GetLineIndexFromCharacterIndex( CaretIndex );
		//			//var lastLineIndex = mainArea.GetLineIndexFromCharacterIndex( _bufferStartIndex );
		//			//if (curlineIndex == lastLineIndex)
		//				_scrollArea.ScrollToBottom();
		//		}
		//	};
		//	//mainArea.TextChanged += HandleTextChanged;
		//	return mainArea;
		//}








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
