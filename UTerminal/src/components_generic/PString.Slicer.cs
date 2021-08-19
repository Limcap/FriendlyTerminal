using System;

namespace Limcap.UTerminal {
	public unsafe partial struct PString {
		public ref struct Slicer {
			public int curSeparatorIndex;
			public PString text;
			public char sliceAt;




			public Slicer( char sliceAt, PString text ) {
				this.text = text;
				this.curSeparatorIndex = -1;
				this.sliceAt = sliceAt;
			}




			public int Count => text.Count( sliceAt );
			public bool HasNext => curSeparatorIndex < text.len;
			public bool IsFirst => curSeparatorIndex == -1;





			public PString Next( Mode option = Mode.ExcludeSeparator ) {
				unchecked {
					int lastIndex = text.len - 1;
					int newSeparatorIndex = text.len;
					PString result;

					if (curSeparatorIndex > lastIndex) {
						result = PString.Null;
					}
					else if (curSeparatorIndex == lastIndex) {
						result = option == Mode.IncludeSeparatorAtStart ? sliceAt : PString.Empty;
					}
					else {
						int startIndex = curSeparatorIndex + 1 - (option == Mode.IncludeSeparatorAtStart ? 1 : 0);
						//int startIndex = curSeparatorIndex + 1;
						newSeparatorIndex = text.IndexOf( sliceAt, curSeparatorIndex + 1 ).Swap(-1, text.len);
						var endIndex = newSeparatorIndex - 1 + (option == Mode.IncludeSeparatorAtEnd ? 1 : 0);
						//var endIndex = newSeparatorIndex - 1;
						var length = endIndex - startIndex + 1;
						if (startIndex <= 0 && endIndex < 0 )
							result = Empty;
						else 
							result = text.Slice( startIndex, length );

						//if (option == Mode.IncludeSeparatorAtEnd && result[result.len] == sliceAt)
						//	result.len++;
						//else if (option == Mode.IncludeSeparatorAtStart && result.ptr != (void*)0 && *(result.ptr - 1) == sliceAt)
						//	result.ptr--;
					}
					
					curSeparatorIndex = newSeparatorIndex;
					return result;
				}
			}




			public PString Remaining( Mode option = Mode.ExcludeSeparator ) {
				curSeparatorIndex++;
				if (curSeparatorIndex == 0) return text;
				if (curSeparatorIndex > text.len) return Null;
				var slice = text.Slice( curSeparatorIndex, text.len - curSeparatorIndex );
				if (option == Mode.IncludeSeparatorAtStart && *(slice.ptr - 1) == sliceAt) slice.ptr--;
				return slice;
			}




			public void Reset() {
				curSeparatorIndex = -1;
			}



			public enum Mode {
				ExcludeSeparator, IncludeSeparatorAtStart, IncludeSeparatorAtEnd
			}
		}
	}
}
