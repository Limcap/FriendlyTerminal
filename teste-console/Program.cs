using Limcap.FTerminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace teste_console {
	class Program {
		static void Main( string[] args ) {
			Thread thread = null;
			for(; ; ) {
				var inp = Console.ReadLine();
				if (inp == "exit") break;
				if (thread == null) {
					thread = new Thread( Window );
					thread.SetApartmentState( ApartmentState.STA );
					thread.Start();
				}
			}
		}

		[STAThread]
		static void Window() {
			var exit = false;
			try {
				var app = new Application();
				var w = new WpfApp1.MainWindow();
				app.Run( w );
				w.Show();
				w.Closing += (o,a) => exit = true;
				//for(; ; ) {
				//	Thread.Sleep( 100 );
				//	if (exit) break;
				//}
			}
			catch( Exception e ) {
				MessageBox.Show( e.ToString() );
			}

		}
	}
}
