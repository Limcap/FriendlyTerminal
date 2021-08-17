using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;

namespace Limcap.UTerminal {
	public unsafe ref partial struct InputSolver {
		private PString _cmdText;
		private PString _argsText;
		
		public ACommand cmd;
		public Arg.Array args;




		public unsafe InputSolver( string input ) {
			var splitIndex = input.IndexOf( ':' );

			_cmdText = new PString() {
				ptr = ((PString)input).ptr,
				len = splitIndex == -1 ? input.Length : splitIndex
			};
			//_cmdText = (PString)input;
			//_cmdText.len = splitIndex == -1 ? input.Length : splitIndex;

			_argsText = new PString() {
				ptr = splitIndex == -1 ? null : _cmdText.ptr + splitIndex + 1,
				len = Math.Max( 0, input.Length - splitIndex - 1 )
			};

			args = new Arg.Array();

			cmd = null;
		}
		public unsafe InputSolver( ReadOnlySpan<char> fullInput ) {
			var splitIndex = fullInput.IndexOf( ':' );

			_cmdText = new PString() {
				ptr = Util.GetPointer( fullInput ),
				len = splitIndex == -1 ? fullInput.Length : splitIndex
			};

			_argsText = new PString() {
				ptr = splitIndex == -1 ? null : _cmdText.ptr + splitIndex + 1,
				len = Math.Max( 0, fullInput.Length - splitIndex - 1 )
			};

			args = new Arg.Array();

			cmd = null;
		}




		public void SolveCommand( Dictionary<string, Type> commandsSet, ref ACommand currentCmd, string locale ) {
			var cmdTextString = _cmdText.ToString();
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
			return _argsText.Count( ',' );
		}




		public void SetMemoryForArgs( Arg* argumentMemoryPtr, int argumentMemoryLength ) {
			args = new Arg.Array( argumentMemoryPtr, argumentMemoryLength );
		}




		public unsafe void SolveArguments() {
			if (args.Length < 1) return;
			var slicer = _argsText.GetSlicer( ',' );
			for (int i = 0; i < args.Length; i++) {
				var slice = slicer.NextSlice();
				args[i] = new Arg( ref slice );
			}
		}
	}
}
