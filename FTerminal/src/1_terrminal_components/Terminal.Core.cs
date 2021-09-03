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
using Cmds = Limcap.FriendlyTerminal.Cmds;

namespace Limcap.FriendlyTerminal {

	public partial class Terminal : INotifyPropertyChanged {

		public const string NEW_LINE = "\n";
		public const string PROMPT_STRING = "» ";//›»
		public const string INSUFICIENT_PRIVILEGE_MESSAGE = "Este comando requer um nível de privilégio maior do que o definido no momento.";

		private readonly string _introText = "Limcap Friendly Terminal";

		private ITerminalScreen _screen;

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

		public string Text => _screen.ToString();

		public Panel Panel { get; private set; }
		public int CurrentPrivilege { get; set; }
		public string Locale { get; set; }








		public Terminal( string introText, Action onExit = null ) {

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

			_screen = BuildTerminalScreen();
			Panel.Children.Add( _screen.UIControlHook );
			DockPanel.SetDock( _screen.UIControlHook, Dock.Top );

			FontPrimaryColor = defaultPrimaryColor;
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








		private ITerminalScreen BuildTerminalScreen() {
			var screen = new TerminalScreenV05() {
				BackgroundColor = defaultBackgroundColor,
				DefaultFontColor = defaultPrimaryColor,
				DefaultFontSize = 14,
			};
			screen.OnPreviewKeyDown += Handle_KeyboardInput;
			return screen;
		}








		private TextBlock BuildStatuArea() {
			return new TextBlock() {
				Background = new SolidColorBrush( Color.FromRgb( 25, 27, 27 ) ),
				Foreground = Brushes.Gray,
				FontFamily = new FontFamily( "Consolas" ),
				Padding = new Thickness( 5 ),
				Height = 25,
			};
		}
	}
}
