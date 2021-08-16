namespace Limcap.UTerminal {
	public unsafe partial struct PtrText {
		public ref struct Slicer {
			public int lastIndex;
			public PtrText text;
			public char sliceAt;




			public Slicer( char sliceAt, PtrText text ) {
				this.text = text;
				this.lastIndex = -1;
				this.sliceAt = sliceAt;
			}




			public int Length => text.Count( sliceAt );




			public PtrText NextSlice() {
				lastIndex++;
				if (lastIndex < text.len) {
					var index = text.IndexOf( sliceAt, lastIndex );
					PtrText slice;
					if (index == -1)
						slice = text.Slice( lastIndex, text.len - lastIndex );
					else
						slice = text.Slice( lastIndex, index - lastIndex );
					lastIndex = index;
					return slice;
				}
				return PtrText.Null;
			}




			public void Reset() {
				lastIndex = -1;
			}
		}
	}


}
