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

namespace Limcap.UTerminal.Mantest {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			var t = new Terminal( null, Close ) { Locale = "ptbr" };
			t.RegisterCommandsInNamespaces(
				"Limcap.UTerminal.Cmds.AccessControl",
				"Limcap.UTerminal.Cmds.Basic",
				"Limcap.UTerminal.Cmds.Customization",
				"Limcap.UTerminal.Cmds.Dev"
			);
			t.Start();

			MainPanel.Children.Add( t.Panel );
			//MainPanel.Children.Add( new TextBox() );
		}
	}
}
