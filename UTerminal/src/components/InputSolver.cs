using System;
using System.Collections.Generic;
using Chan = System.Span<char>;
using Stan = System.ReadOnlySpan<char>;

namespace Limcap.UTerminal {
	public unsafe ref partial struct InputSolver {
		private PtrText _cmdText;
		private PtrText _argsText;
		
		public ACommand cmd;
		public Arg.Array args;


		//public PtrText Command => _cmdText;
		//public Arg.Array Args => _argArray;


		//private Argument[] ArgArray {
		//	get {
		//		var aarr = new Argument[_argArr.len];
		//		for (var i = 0; i < _argArr.len; i++) aarr[i] = ((Argument*)_argArr.ptr)[i];
		//		return aarr;
		//	}
		//}




		public unsafe InputSolver( Stan fullInput ) {
			var splitIndex = fullInput.IndexOf( ':' );

			_cmdText = new PtrText() {
				ptr = Util.GetPointer( fullInput ),
				len = splitIndex == -1 ? fullInput.Length : splitIndex
			};

			_argsText = new PtrText() {
				ptr = splitIndex == -1 ? null : _cmdText.ptr + splitIndex + 1,
				len = Math.Max( 0, fullInput.Length - splitIndex - 1 )
			};

			//_argArr = new PtrArray<Argument>() {
			//	ptr = null,
			//	len = 0
			//};
			args = new Arg.Array();

			cmd = null;
		}




		public void SolveCommand( Dictionary<string, Type> commandsSet, ref ACommand currentCmd, string locale ) {
			var cmdTextString = _cmdText.ToString();
			if (!commandsSet.ContainsKey(cmdTextString))
				cmd = null;
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
			//_argArr.ptr = argumentMemoryPtr;
			//_argArr.len = argumentMemoryLength;
		}




		public unsafe void SolveArguments() {
			if (args.Length < 1) return;
			var slicer = _argsText.GetSlicer( ',' );
			for (int i = 0; i < args.Length; i++) {
				var slice = slicer.NextSlice();
				args[i] = new Arg( ref slice );
				//((ArgSolver*)_argArr._rawArr.ptr)[i] = new ArgSolver( slice );
			}

			//var a = ((ArgSolver*)_argArr._rawArr.ptr)[2];
			//for(int u=0;u<10;u++) ((long*)_argArr._rawArr.ptr)[u] = 0;

			//int lastIndex = -1;
			//for (int i = 0; i < _argArr.Length - 1; i++) {
			//	int index = _argsTxt.IndexOf( ',', lastIndex + 1 );
			//	if (index == -1) index = lastIndex + 1;
			//	int length = index - (lastIndex + 1);
			//	var argText = _argsTxt.Slice( lastIndex + 1, length );
			//	//((Argument*)_argArr.ptr)[i] = new Argument( ref argText );
			//	_argArr[i] = new Argument( ref argText );
			//	lastIndex = index;
			//}
		}
	}
}
