using System;
using Stan = System.ReadOnlySpan<char>;

namespace Limcap.UTerminal {
	internal partial class ParameterTypeAssistant_optm1 {
		public unsafe ref struct ChanHoloArray1 {
			public Span<char> fulltext;
			public Span<Range> ranges;
			public int count;
			private char _gapStart;
			private char _gapEnd;

			private char* txtPtr;
			private Range* rangePtr;



			public Span<char> this[int i] {
				get => fulltext.Slice( ranges[i].start, ranges[i].length );
			}


			public override string ToString() {
				return string.Join( "; ", AsArray );
			}


			public string[] AsArray {
				get {
					var arr = new string[count];
					for (int i = 0; i < count; i++) arr[i] = this[i].ToString();
					return arr;
				}
			}





			public unsafe ChanHoloArray1( ref Stan source, char gapStart = '=', char gapEnd = ',' ) {
				rangePtr = null;
				count = source.Length > 0 ? 1 : 0;
				foreach (var c in source) if (c == gapStart) count++;
				fixed (char* ptr = &source.GetPinnableReference()) { txtPtr = ptr; }
				ranges = new Span<Range>( rangePtr, count );
				fixed (Range* ptr = &ranges.GetPinnableReference()) { rangePtr = ptr; }
				fulltext = new Span<char>( txtPtr, source.Length );
				_gapStart = gapStart;
				_gapEnd = gapEnd;
			}


			public unsafe void Split() {
				bool skipStartingSpace = true;
				int l = 0;
				for (int i = 0; i < fulltext.Length; i++) {
					while (skipStartingSpace && i < fulltext.Length && fulltext[i] == ' ') i++;
					if (skipStartingSpace) {
						ranges[l].start = i;
						skipStartingSpace = false;
					}
					var c = fulltext[i];
					if (c == _gapStart) {
						ranges[l].length = i - ranges[l].start;
						l++;
					}
					else if (c == _gapEnd) {
						skipStartingSpace = true;
						ranges[l].start = i;
					}
				}
				for (int i = fulltext.Length - 1; i >= 0; i--) {
					var lastLimit = ranges[count - 1];
					if (fulltext[i] == ' ') continue;
					ranges[count - 1].length = i + 1 - ranges[count - 1].start;
					break;
				}
			}
		}
	}
}
