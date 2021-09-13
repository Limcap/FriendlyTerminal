using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Limcap.FriendlyTerminal.Mantest {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			var t = new Terminal( "TESTE do terminal\nLinha 2", Dispatcher, Close ) { Locale = "ptbr" };
			t.RegisterCommandsInNamespaces( "Limcap.FriendlyTerminal",
				"Limcap.FriendlyTerminal.Cmds.AccessControl",
				"Limcap.FriendlyTerminal.Cmds.Basic",
				"Limcap.FriendlyTerminal.Cmds.Customization",
				"Limcap.FriendlyTerminal.Cmds.Dev"
			);
			MainPanel.Children.Add( t.Panel );
			t.Start();


			/*
			var window = new Window() {
				Width=800,Height=460,
			};
			var view = new FlowDocumentScrollViewer() {
				IsToolBarVisible = false,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				SelectionBrush = Brushes.White,
				
			};
			var doc = new FlowDocument() {
				Background = Brushes.Black,//new SolidColorBrush( Color.FromRgb( 25, 27, 27 ) ),
				Foreground = Brushes.GreenYellow,//new SolidColorBrush( Color.FromRgb( 120, 179, 32 ) ),
				FontFamily = new FontFamily( "Consolas" ),
				FontSize = 14,
				ColumnWidth = 800,
				IsColumnWidthFlexible = false,
				PagePadding = new Thickness( 5 ),
			};
			view.Document = doc;
			window.Content = view;
			doc.Blocks.Add( new Paragraph(new Run( "Hello Hello baby" ) ) );
			doc.Blocks.Add( new Paragraph( new Run( "You called I can't hear a thing." ) ) );
			doc.Blocks.LastBlock.ContentEnd.InsertTextInRun( " HHASJDKHSA " );
			_ = doc.Blocks;
			window.Show();


			//Task.Run( () => {
			//	foreach (char c in "Hello my friend, how are you, let's do this.") {
			//		Thread.Sleep( 100 );
			//		Dispatcher.Invoke( () => {
			//			doc.ContentEnd.InsertTextInRun( c.ToString() );
			//		} );
			//	}
			//} );

			var thick = new Thickness( 5 );

			ScrollViewer sv = view.Template.FindName( "PART_ContentHost", view ) as ScrollViewer;
			sv.ScrollToEnd();
			view.KeyDown += (obj,arg) => {
				var v = obj as FlowDocumentScrollViewer;
				var d = v.Document;
				if (arg.Key == Key.Insert) {
					var a = new Cmds.Dev.Print_a_lot( "ptbr" );
					var txt = a.MainFunction( t, null );
					txt = txt.Substring( 0, txt.Length / 20 );
					var sr = new StringReader( txt );
					string line;
					while( (line = sr.ReadLine()) != null )
						//(doc.Blocks.LastBlock as Paragraph).Inlines.Add( new Run( '\n'+line ) );
						doc.Blocks.Add( new Paragraph( new Run( '\n'+line ) ) { Margin = thick } );
				}
				else if (arg.Key == Key.Escape)
					d.Blocks.Clear();
				else {
					var c = KeyGrabber.GetCharFromKey( arg.Key );
					if( c != 0 ) d.ContentEnd.InsertTextInRun(c.ToString());
				}
			};
			*/
		}
	}
}
