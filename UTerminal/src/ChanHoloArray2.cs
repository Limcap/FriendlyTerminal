using System;
using Stan = System.ReadOnlySpan<char>;

namespace Limcap.UTerminal {
	internal partial class ParameterTypeAssistant_optm1 {
		public unsafe ref struct ChanHoloArray2 {
			private char _gapStart;
			private char _gapEnd;

			private readonly char* _txtPtr;
			private readonly int _txtPtrLen;

			private readonly Range* _rangePtr;
			private readonly int _rangePtrLen;

			public Span<char> Text => new Span<char>( _txtPtr, _txtPtrLen );
			public Span<Range> Ranges => new Span<Range>( _rangePtr, _rangePtrLen );


			public Span<char> this[int i] {
				get => Text.Slice( _rangePtr[i].start, _rangePtr[i].length );
			}


			public override string ToString() {
				return string.Join( "; ", AsArray );
			}


			public string[] AsArray {
				get {
					var arr = new string[_rangePtrLen];
					for (int i = 0; i < _rangePtrLen; i++) arr[i] = this[i].ToString();
					return arr;
				}
			}







			public unsafe ChanHoloArray2( ref Stan source, ref Span<Range> rangeSpan, char gapStart = '=', char gapEnd = ',' ) {
				_gapStart = gapStart;
				_gapEnd = gapEnd;

				fixed (char* ptr = &source.GetPinnableReference()) { _txtPtr = ptr; }
				_txtPtrLen = source.Length;
				
				rangeSpan = new Range[_txtPtrLen];
				fixed (Range* ptr = &rangeSpan.GetPinnableReference()) { _rangePtr = ptr; }
				_rangePtrLen = source.Length > 0 ? 1 : 0;
				foreach (var c in source) if (c == gapStart) _rangePtrLen++;

				bool skipStartingSpace = true;
				int l = 0;
				for (int i = 0; i < source.Length; i++) {
					while (skipStartingSpace && i < source.Length && source[i] == ' ') i++;
					if (skipStartingSpace) {
						_rangePtr[l].start = i;
						skipStartingSpace = false;
					}
					var c = source[i];
					if (c == gapStart) {
						_rangePtr[l].length = i - _rangePtr[l].start;
						l++;
					}
					else if (c == gapEnd) {
						skipStartingSpace = true;
						_rangePtr[l].start = i;
					}
				}
				for (int i = source.Length - 1; i >= 0; i--) {
					var lastLimit = _rangePtr[_rangePtrLen - 1];
					if (source[i] == ' ') continue;
					_rangePtr[_rangePtrLen - 1].length = i + 1 - _rangePtr[_rangePtrLen - 1].start;
					break;
				}
			}
		}
	}
}
