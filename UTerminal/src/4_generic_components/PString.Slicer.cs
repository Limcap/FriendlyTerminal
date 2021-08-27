using System;

namespace Limcap.FTerminal {
	public unsafe partial struct PString {
		public ref struct Slicer {
			public int curSeparatorIndex;
			public PString text;
			public char sliceAt;
			public Mode mode;




			public Slicer( char sliceAt, PString text, Mode mode ) {
				this.text = text;
				this.curSeparatorIndex = -1;
				this.sliceAt = sliceAt;
				this.Index = -1;
				this.mode = mode;
			}




			public int Count => text.Count( sliceAt );
			public bool HasNext => curSeparatorIndex < text.len;
			public bool IsFirst => curSeparatorIndex == -1;
			public int Index { get; private set; }





			public PString Next() {
				unchecked {
					int lastIndex = text.len - 1;
					int newSeparatorIndex = text.len;
					PString result;

					if (curSeparatorIndex > lastIndex) {
						result = PString.Null;
					}
					else if (curSeparatorIndex == lastIndex) {
						result = (mode == Mode.IncludeSeparatorAtStart && lastIndex > -1) ? sliceAt : PString.Empty;
					}
					else {
						int startIndex = curSeparatorIndex + 1 - ((mode == Mode.IncludeSeparatorAtStart && curSeparatorIndex >= 0) ? 1 : 0);
						newSeparatorIndex = text.IndexOf( sliceAt, curSeparatorIndex + 1 ).Swap( -1, text.len );
						var endIndex = newSeparatorIndex - 1 + ((mode == Mode.IncludeSeparatorAtEnd && newSeparatorIndex <= lastIndex) ? 1 : 0);
						var length = endIndex - startIndex + 1;
						if (startIndex <= 0 && endIndex < 0)
							result = Empty;
						else
							result = text.Slice( startIndex, length );
					}

					Index++;
					curSeparatorIndex = newSeparatorIndex;
					return result;
				}
			}




			public PString PeekNext() {
				unchecked {
					int lastIndex = text.len - 1;
					PString result;

					if (curSeparatorIndex > lastIndex) {
						result = PString.Null;
					}
					else if (curSeparatorIndex == lastIndex) {
						result = (mode == Mode.IncludeSeparatorAtStart && lastIndex > -1) ? sliceAt : PString.Empty;
					}
					else {
						int startIndex = curSeparatorIndex + 1 - ((mode == Mode.IncludeSeparatorAtStart && curSeparatorIndex >= 0) ? 1 : 0);
						int newSeparatorIndex = text.IndexOf( sliceAt, curSeparatorIndex + 1 ).Swap( -1, text.len );
						var endIndex = newSeparatorIndex - 1 + ((mode == Mode.IncludeSeparatorAtEnd && newSeparatorIndex <= lastIndex) ? 1 : 0);
						var length = endIndex - startIndex + 1;
						if (startIndex <= 0 && endIndex < 0)
							result = Empty;
						else
							result = text.Slice( startIndex, length );
					}
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
				Index = -1;
			}



			public enum Mode {
				ExcludeSeparator, IncludeSeparatorAtStart, IncludeSeparatorAtEnd
			}
		}
	}
}
