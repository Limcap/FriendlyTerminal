using System;

namespace Limcap.FTerminal {

	public partial class Terminal {
		public class UninvokableCommandException : Exception {
			public UninvokableCommandException( Type cmd )
			: base( "Attempted to register a command without an invoke text: " + cmd.Name ) { }
			public UninvokableCommandException( ACommand cmd )
			: base( "Attempted to register a command without an invoke text: " + cmd.GetType().Name ) { }
		}
	}
}
