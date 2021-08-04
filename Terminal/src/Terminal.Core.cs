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

			Text += _introText;
			StartNewInput();
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
				AcceptsTab = true,
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




	}
}
