using System;
using Stan = System.ReadOnlySpan<char>;

namespace Limcap.UTerminal {
	internal partial class ParameterTypeAssistant_optm1 {
		public unsafe ref struct ChanHoloArray1 {
			public Span<char> fulltext;
			public Span<Range> ranges;
			public int count;

			public unsafe ChanHoloArray1( ref Stan source, char gapStart = '=', char gapEnd = ',' ) {
				count = source.Length > 0 ? 1 : 0;
				foreach (var c in source) if (c == gapStart) count++;
				var ptr = stackalloc Range[count];
				ranges = new Span<Range>( ptr, count );
				bool skipStartingSpace = true;
				int l = 0;
				for (int i = 0; i < source.Length; i++) {
					while (skipStartingSpace && i < source.Length && source[i] == ' ') i++;
					if (skipStartingSpace) {
						ranges[l].start = i;
						skipStartingSpace = false;
					}
					var c = source[i];
					if (c == gapStart) {
						ranges[l].length = i - ranges[l].start;
						l++;
					}
					else if (c == gapEnd) {
						skipStartingSpace = true;
						//while (i<source.Length && source[++i] == ' ');
						ranges[l].start = i;
					}
				}
				for (int i = source.Length - 1; i >= 0; i--) {
					var lastLimit = ranges[count - 1];
					if (source[i] == ' ') continue;
					ranges[count - 1].length = i + 1 - ranges[count - 1].start;
					break;
				}
				//int i = source.Length;
				//while (source[i] == ' ') i--;
				//limits[length - 1].length = source.Length - limits[length - 1].start;
				var ptr2 = stackalloc char[source.Length];
				fulltext = new Span<char>( ptr2, source.Length );
				source.CopyTo( fulltext );
			}


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
		}
	}
}
