using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Limcap.UTerminal.Cmds.IO {
	public class MemUsage : ACommand {

		public MemUsage( string locale ) : base( locale ) { }


		public const string DEFAULT_LOCALE = "enus";
		public const string INVOKE_TEXT = "mem usage";



		protected private override string DescriptionBuilder() {
			return Txt( "desc" );
		}
		

		protected private override Parameter[] ParametersBuilder() {
			return null;
		}


		public override string MainFunction( Terminal t, Arg[] args ) {
			System.GC.Collect();
			var mem = GC.GetTotalMemory( true );
			return "\nUsed memory: " + mem;
		}
	}
}
