using System;
using Stan = System.ReadOnlySpan<char>;

namespace Limcap.UTerminal {
	public unsafe ref partial struct SorttedInput {
		public unsafe struct Argument_Old {

			private char* _paramPtr;
			private int _paramLen;
			private char* _argPtr;
			private int _argLen;




			public Argument_Old( ref Stan paramSpan, ref Stan argSpan ) {
				_paramPtr = Util.GetPointer( paramSpan );
				_paramLen = paramSpan.Length;
				_argPtr = argSpan == null ? null : Util.GetPointer( argSpan );
				_argLen = argSpan == null ? -1 : argSpan.Length;
			}




			public Argument_Old( ref Stan rawArg ) {
				var firstIndex = rawArg.IndexOf( '=' );
				var paramSpan = rawArg.Slice( 0, firstIndex > -1 ? firstIndex : rawArg.Length );
				var argSpan = firstIndex > -1 ? rawArg.Slice( Math.Max( firstIndex + 1, rawArg.Length - 1 ), rawArg.Length - (firstIndex + 1) ) : null;
				_paramPtr = Util.GetPointer( paramSpan );
				_paramLen = paramSpan.Length;
				_argPtr = argSpan == null ? null : Util.GetPointer( argSpan );
				_argLen = argSpan == null ? -1 : argSpan.Length;
				Trim();
			}




			public Stan Name => new Stan( _paramPtr, _paramLen );
			public Stan Value => _argPtr == null ? null : new Stan( _argPtr, _argLen );




			public void Trim( char c = ' ' ) {
				TrimStart( c );
				TrimEnd( c );
			}




			public void TrimStart( char c = ' ' ) {
				while (_paramPtr[0] == c) {
					//_paramPtr = &_paramPtr[1];
					_paramPtr++;
					_paramLen--;
				}
				if (_argPtr == null) return;
				while (_argPtr[0] == c) {
					//_argPtr = &_argPtr[1];
					_argPtr++;
					_argLen--;
				}
			}




			public void TrimEnd( char c = ' ' ) {
				while (_paramLen > 0 && _paramPtr[_paramLen - 1] == c)
					_paramLen--;
				if (_argPtr == null) return;
				while (_argLen > 0 && _argPtr[_argLen - 1] == c)
					_argLen--;
			}
		}
	}
}