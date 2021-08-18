namespace Limcap.UTerminal {
	public unsafe partial struct PString {
		public ref struct Slicer {
			public int lastIndex;
			public PString text;
			public char sliceAt;




			public Slicer( char sliceAt, PString text ) {
				this.text = text;
				this.lastIndex = -1;
				this.sliceAt = sliceAt;
			}




			public int Count => text.Count( sliceAt );




			public bool HasNextSlice => lastIndex < text.len;
			




			public PString NextSlice( Mode option = Mode.ExcludeSeparator ) {
				lastIndex++;
				if (lastIndex < text.len) {
					var index = text.IndexOf( sliceAt, lastIndex );
					PString slice;
					if (index == -1)
						slice = text.Slice( lastIndex, text.len - lastIndex );
					else
						slice = text.Slice( lastIndex, index - lastIndex );
					lastIndex = index == -1 ? text.len : index;

					if (option == Mode.IncludeSeparatorAtEnd && slice[slice.len] == sliceAt) slice.len++;
					else if (option == Mode.IncludeSeparatorAtStart && *(slice.ptr - 1) == sliceAt) slice.ptr--;

					return slice;
				}
				return Null;
			}




			public PString Remaining( Mode option = Mode.ExcludeSeparator ) {
				lastIndex++;
				if (lastIndex == 0) return text;
				if (lastIndex > text.len) return Null;
				var slice = text.Slice( lastIndex, text.len - lastIndex );
				if (option == Mode.IncludeSeparatorAtStart && *(slice.ptr - 1) == sliceAt) slice.ptr--;
				return slice;
			}




			public void Reset() {
				lastIndex = -1;
			}



			public enum Mode {
				ExcludeSeparator, IncludeSeparatorAtStart, IncludeSeparatorAtEnd
			}
		}
	}
}
