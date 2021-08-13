using System;
using Stan = System.ReadOnlySpan<char>;
using Chan = System.Span<char>;
using System.Collections.Generic;

namespace Limcap.UTerminal {
	public unsafe ref struct SorttedInput {
		public ACommand cmd;
		private Stan _cmdTxt;
		private char* _cmdTxtPtr;
		private int _cmdTxtLen;

		private Stan _argsTxt;
		private char* _argsTxtPtr;
		private int _argsTxtLen;

		public Span<Argument> argsSpan => new Span<Argument>( _argsTxtPtr, _argsTxtLen );
		private Argument* _argsSpanPtr;
		private int _argsSpanLen;

		public unsafe SorttedInput( Stan fullInput ) {
			var inp = fullInput;
			var splitIndex = fullInput.IndexOf( ':' );
			var inputPtr = Util.GetPointer( fullInput );

			_cmdTxt = splitIndex == -1 ? new Stan( inputPtr, fullInput.Length ) : new Stan( inputPtr, splitIndex ).Trim();
			_cmdTxtPtr = Util.GetPointer( _cmdTxt );
			_cmdTxtLen = _cmdTxt.Length;

			_argsTxt = splitIndex == -1 ? null : new Stan( &inputPtr[splitIndex + 1], Math.Max( 0, fullInput.Length - splitIndex - 1 ) ).Trim();
			_argsTxtPtr = Util.GetPointer( _argsTxt );
			_argsTxtLen = _argsTxt.Length;

			//argsSpan = null;
			_argsSpanPtr = null;
			_argsSpanLen = 0;

			cmd = null;
		}

		public void SortCommand( Dictionary<string,Type> commandsSet, ref ACommand currentCmd, string locale ) {
			if (!commandsSet.ContainsKey( _cmdTxt.ToString() ))
				cmd = null;
			var cmdType = commandsSet[_cmdTxt.ToString()];
			cmd = currentCmd?.GetType() == cmdType ? currentCmd
			: cmdType.IsSubclassOf( typeof( ACommand ) ) ? (ACommand)Activator.CreateInstance( cmdType, locale )
			: null;
		}


		public void SetSorttedArgsMem( Argument* argumentMemoryPtr, int argumentMemoryLength ) {
			_argsSpanPtr = argumentMemoryPtr;
			_argsSpanLen = argumentMemoryLength;
		}

		public int ProspectAmountOfArguments() {
			int num = _argsTxt.Length > 0 ? 1 : 0;
			foreach (var c in _argsTxt) if ( c == ',') num++;
			return num;
		}

		public unsafe void SortArguments() {
			if (_argsSpanLen < 1) return;
			
			int lastIndex = -1;
			for( int i=0; i<_argsSpanLen-1; i++ ) {
				int index = _argsTxt.IndexOf( ',',lastIndex+1 );
				if (index == -1) index = lastIndex + 1;
				int length = index - lastIndex + 1;
				var argText = new Stan( &_argsTxtPtr[lastIndex], length-1 );
				_argsSpanPtr[i] = new Argument( ref argText );
				lastIndex = index;
			}
		}
	}
}
