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




			public bool HasNextSlice() {
				return lastIndex < text.len;
			}




			public PString NextSlice() {
				lastIndex++;
				if (lastIndex < text.len) {
					var index = text.IndexOf( sliceAt, lastIndex );
					PString slice;
					if (index == -1)
						slice = text.Slice( lastIndex, text.len - lastIndex );
					else
						slice = text.Slice( lastIndex, index - lastIndex );
					lastIndex = index == -1 ? text.len : index;
					return slice;
				}
				return Null;
			}




			public void Reset() {
				lastIndex = -1;
			}
		}
	}
}
