using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace WpfApp1 {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
			var margin = new Thickness( 1 );



			var doc = new FlowDocument() { Background = Brushes.LightGray, PagePadding = margin,  };
			var view = new FlowDocumentScrollViewer() { Focusable = false, Background = Brushes.LightGray, Height = 30, Padding = margin, IsToolBarVisible =false };
			view.Document = doc;
			doc.PreviewMouseDown += ( o, a ) => doc.Focus();
			doc.GotFocus += ( o, a ) => doc.Background = Brushes.LightBlue;
			doc.LostFocus += ( o, a ) => doc.Background = Brushes.LightGray;
			doc.PreviewKeyDown += ( o, a ) => {
				var key = KeyGrabber.GetCharFromKey( a.Key );
				if ( key != 0 ) doc.ContentEnd.InsertTextInRun( key.ToString() );
			};
			doc.PreviewKeyUp += ( o, a ) => { var b = KeyGrabber.GetCharFromKey( a.Key ); };



			var txt = new TextBlock() { Focusable = true, Background = Brushes.LightGray, Height = 30, IsEnabled = true, Padding = margin };
			txt.PreviewMouseDown += ( o, a ) => txt.Focus();
			txt.GotFocus += ( o, a ) => txt.Background = Brushes.LightBlue;
			txt.LostFocus += ( o, a ) => txt.Background = Brushes.LightGray;
			txt.PreviewKeyDown += ( o, a ) => {
				var key = KeyGrabber.GetCharFromKey( a.Key );
				if (key != 0) txt.ContentEnd.InsertTextInRun( key.ToString() );
			};
			txt.PreviewKeyUp += ( o, a ) => { var b = KeyGrabber.GetCharFromKey( a.Key ); };



			stackPanel.Children.Add( view );
			stackPanel.Children.Add( txt );
		}
	}
}
