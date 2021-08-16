using System;

namespace Limcap.UTerminal {
	public unsafe struct RawTxt {
		public char* ptr;
		public int len;




		public RawTxt( ReadOnlySpan<char> text ) {
			ptr = Util.GetPointer( text );
			len = text.Length;
		}




		public RawTxt( string text ) {
			ptr = Util.GetPointer( text.AsSpan() );
			len = text.Length;
		}




		public Span<char> AsSpan => new Span<char>( ptr, len );
		public string AsString => new string( ptr, 0, len );
		public override string ToString() => AsString;




		public void Trim( char c = ' ' ) {
			TrimStart( c );
			TrimEnd( c );
		}




		public void TrimStart( char c = ' ' ) {
			if (ptr == null || len < 1) return;
			while (ptr[0] == c) ptr++;
		}




		public void TrimEnd( char c = ' ' ) {
			if (ptr == null || len < 1) return;
			while (len > 0 && ptr[len - 1] == c)
				len--;
		}




		public int IndexOf( char searchedChar, int startIndex = 0 ) {
			startIndex = startIndex < 0 ? 0 : startIndex;
			for (int i = startIndex; i < len; i++)
				if (ptr[i] == searchedChar) return i;
			return -1;
		}




		public RawTxt Slice( int startIndex, int length ) {
			//if (startIndex > len - 1) throw new IndexOutOfRangeException( "Slice start index is out of bounds" );
			return new RawTxt() { ptr = ptr + startIndex, len = length };
		}




		public static RawTxt Null => new RawTxt() { ptr = null, len = -1 };
		public bool IsNull => ptr == null;


		public static implicit operator RawTxt(string txt) => new RawTxt(txt.AsSpan());
		public char this[int i] => ptr[i];   
	}
}
