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
			
			var t = new Terminal( null, Close ) { Locale="enus"};

			t.RegisterCommandsInNamespaces( "Limcap.UTerminal.Cmds.Misc" );
			t.RegisterCommandsInNamespaces( "Limcap.UTerminal.Cmds.AccessControl" );
			t.RegisterCommandsInNamespaces( "Limcap.UTerminal.Cmds.Customization" );
			t.RegisterCommandsInNamespaces( "Limcap.UTerminal.Cmds.IO" );
			t.RegisterCommandsInNamespaces( "Limcap.UTerminal.Cmds.Config" );
			t.RegisterCommandsInNamespaces( "Limcap.UTerminal.Cmds.Dev" );

			MainPanel.Children.Add( t.Panel );

			//t.RegisterCommand<Cmds.Raise>();
			//t.RegisterCommand<Cmds.ToggleStatusBar>();
			//t.RegisterCommand<Cmds.ToggleTraceBar>();
			//t.RegisterCommand<Cmds.SaveOutput>();
			//t.RegisterCommand<Cmds.Print_a_lot>();
		}
	}
}
