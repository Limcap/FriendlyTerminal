﻿using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Limcap.TextboxTerminal {
	public class Print_a_lot : ICommand {

		//public static void Register( TerminalClient term ) {
		//	term.RegisterCommand( "command2", MainFunction, HelpInfo );
		//	Assembly assembly = Assembly.LoadFrom( "MyNice.dll" );
		//	Type type = assembly.GetType( "MyType" );
		//	ACommand instanceOfMyType = (ACommand) Activator.CreateInstance( type );
		//}

		public const string INVOKE_STRING = "print a lot";


		public string HelpInfo {
			get =>
				"DESCRIPTION:\n" +
				"\tVery long repetitive text.";
		}


		public string MainFunction( Terminal t, Args args ) {
			var sb = new StringBuilder();
			for (int i = 0; i < 2000; i++)
				sb.Append( "this is a hundred characters long string, with the solo purpose of testing long strings. the end...\n" );
			return sb.ToString();
		}
	}
}
