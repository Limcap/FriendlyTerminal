using System;
using System.Collections.Generic;
using Chan = System.Span<char>;
using Stan = System.ReadOnlySpan<char>;

namespace Limcap.UTerminal {
	public unsafe ref partial struct SorttedInput {
		public ACommand cmd;
		private RawTxt _cmdTxt;
		//private char* _cmdTxtPtr;
		//private int _cmdTxtLen;

		private RawTxt _argsTxt;
		//private char* _argsTxtPtr;
		//private int _argsTxtLen;

		//public Span<Argument> argsSpan => new Span<Argument>( _argsTxtPtr, _argsTxtLen );
		private RawArray<Argument> _argArr;
		//private Argument* _argArrPtr;
		//private int _argArrLen;




		private Argument[] ArgArray {
			get {
				var aarr = new Argument[_argArr.len];
				for (var i = 0; i < _argArr.len; i++) aarr[i] = ((Argument*)_argArr.ptr)[i];
				return aarr;
			}
		}




		public unsafe SorttedInput( Stan fullInput ) {
			var splitIndex = fullInput.IndexOf( ':' );

			_cmdTxt = new RawTxt() {
				ptr = Util.GetPointer( fullInput ),
				len = splitIndex == -1 ? fullInput.Length : splitIndex
			};

			_argsTxt = new RawTxt() {
				ptr = splitIndex == -1 ? null : _cmdTxt.ptr + splitIndex + 1,
				len = Math.Max( 0, fullInput.Length - splitIndex - 1 )
			};

			_argArr = new RawArray<Argument>() {
				ptr = null,
				len = 0
			};

			cmd = null;
		}




		public void SortCommand( Dictionary<string, Type> commandsSet, ref ACommand currentCmd, string locale ) {
			if (!commandsSet.ContainsKey( _cmdTxt.ToString() ))
				cmd = null;
			var cmdType = commandsSet[_cmdTxt.ToString()];
			cmd = currentCmd?.GetType() == cmdType ? currentCmd
			: cmdType.IsSubclassOf( typeof( ACommand ) ) ? (ACommand)Activator.CreateInstance( cmdType, locale )
			: null;
		}




		public void SetSorttedArgsMem( Argument* argumentMemoryPtr, int argumentMemoryLength ) {
			_argArr.ptr = argumentMemoryPtr;
			_argArr.len = argumentMemoryLength;
		}



		public int ProspectAmountOfArguments() {
			int num = _argsTxt.len > 0 ? 1 : 0;
			for (int i=0; i<_argsTxt.len; i++) if (_argsTxt.ptr[i] == ',') num++;
			//foreach (var c in _argsTxt.AsSpan) if (c == ',') num++;
			return num;
		}



		public unsafe void SortArguments() {
			if (_argArr.len < 1) return;

			int lastIndex = -1;
			for (int i = 0; i < _argArr.len - 1; i++) {
				int index = _argsTxt.IndexOf( ',', lastIndex + 1 );
				if (index == -1) index = lastIndex + 1;
				int length = index - (lastIndex + 1);
				//var argText = new Stan( _argsTxt.ptr + lastIndex + 1, length );
				//var argText = new RawTxt() { ptr = _argsTxt.ptr + lastIndex + 1, len = length };
				var argText = _argsTxt.Slice( lastIndex + 1, length );
				((Argument*)_argArr.ptr)[i] = new Argument( ref argText );
				lastIndex = index;
			}
		}
	}
}
