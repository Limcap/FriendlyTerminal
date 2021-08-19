using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;

namespace Limcap.UTerminal {
	public unsafe partial struct InputSolver {
		public PString cmdText;
		public PString argsText;
		
		public ACommand cmd;
		public Arg.Array args;




		public unsafe InputSolver( string input ) {
			var splitIndex = input.IndexOf( ':' );

			//cmdText = new PString() {
			//	ptr = ((PString)input).ptr,
			//	len = splitIndex == -1 ? input.Length : splitIndex
			//};
			cmdText = (PString)input;
			cmdText.len = splitIndex == -1 ? input.Length : splitIndex;

			argsText = new PString() {
				ptr = splitIndex == -1 ? null : cmdText.ptr + splitIndex + 1,
				len = Math.Max( 0, input.Length - splitIndex - 1 )
			};

			args = new Arg.Array();

			cmd = null;
		}




		public unsafe InputSolver( ACommand cmd, PString inpArgs ) {
			this.cmd = cmd;
			cmdText = PString.Null;
			argsText = inpArgs;
			args = new Arg.Array();
		}




		public void SolveCommand( Dictionary<string, Type> commandsSet, ref ACommand currentCmd, string locale ) {
			var cmdTextString = cmdText.ToString();
			if (!commandsSet.ContainsKey( cmdTextString )) {
				cmd = null;
				return;
			}
		
			var cmdType = commandsSet[cmdTextString];
			cmd = currentCmd?.GetType() == cmdType ? currentCmd
			: cmdType.IsSubclassOf( typeof( ACommand ) ) ? (ACommand)Activator.CreateInstance( cmdType, locale )
			: null;
		}




		public int CountArguments() {
			return argsText.Count( ',' );
		}




		public void SetMemoryForArgs( Arg* argumentMemoryPtr, int argumentMemoryLength ) {
			args = new Arg.Array( argumentMemoryPtr, argumentMemoryLength );
		}




		public unsafe void SolveArguments() {
			if (args.Length < 1) return;
			var slicer = argsText.GetSlicer( ',' );
			for (int i = 0; i < args.Length; i++) {
				var slice = slicer.Next();
				args[i] = new Arg( ref slice );
			}
		}
	}
}
