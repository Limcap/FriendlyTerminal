﻿using System;
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

namespace Limcap.TextboxTerminal.Mantest {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
			var t = new Terminal( null );
			MainPanel.Children.Add( t.Panel );

			//t.RegisterCommand<Raise>();
			//t.RegisterCommand<ToggleTraceBar>();
			//t.RegisterCommand<ToggleStatusBar>();
			//t.RegisterCommand<SaveOutput>();
		}
	}
}
