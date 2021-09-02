using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Limcap.FriendlyTerminal.Cmds.Dev {
	public class MemUsage : ACommand {

		public MemUsage( string locale ) : base( locale ) { }


		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_TEXT = "dev, mem-usage";



		protected override string DescriptionBuilder() {
			return Txt( "desc" );
		}
		

		protected override Parameter[] ParametersBuilder() {
			return null;
		}


		public override string MainFunction( Terminal t, Arg[] args ) {
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			Process currentProcess = Process.GetCurrentProcess();
			long usedMemory = currentProcess.PrivateMemorySize64;
			var mem = GC.GetTotalMemory( true );
			NumberFormatInfo nfi = new CultureInfo( "en-US", false ).NumberFormat;
			nfi.NumberDecimalSeparator = ",";
			nfi.NumberGroupSeparator = ".";
			nfi.CurrencySymbol = "";
			nfi.NumberDecimalDigits = 0;
			t.TypeText( "Used heap memory: " + mem.ToString( "N", nfi ) + ", private bytes: " + usedMemory.ToString( "N", nfi ) );
			return null;
		}
	}
}
