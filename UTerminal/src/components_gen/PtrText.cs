using System;
using System.Diagnostics;

namespace Limcap.UTerminal {

	[DebuggerDisplay( "{ToString()}" )]
	public unsafe partial struct PtrText {
		public char* ptr;
		public int len;




		public PtrText( ReadOnlySpan<char> text ) {
			ptr = Util.GetPointer( text );
			len = text.Length;
		}




		public PtrText( Span<char> text ) {
			ptr = Util.GetPointer( text );
			len = text.Length;
		}




		public PtrText( string text ) {
			ptr = Util.GetPointer( text.AsSpan() );
			len = text.Length;
		}








		#region PROPERTIES
		#endregion
		public bool IsNull => len < 0;








		#region ACTIONS
		#endregion
		public void Trim( char c = ' ' ) {
			TrimStart( c );
			TrimEnd( c );
		}




		public void TrimStart( char c = ' ' ) {
			if (ptr == null || len < 1) return;
			while (ptr[0] == c) {
				ptr++;
				len--;
			}
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




		public PtrText Slice( int startIndex, int length ) {
			//if (startIndex > len - 1) throw new IndexOutOfRangeException( "Slice start index is out of bounds" );
			return new PtrText() { ptr = ptr + startIndex, len = length };
		}




		public PtrText SliceToChar( char c, int startIndex = 0 ) {
			int index = IndexOf( ',', startIndex );
			if (index == -1) return PtrText.Null;
			int length = index - startIndex;
			return Slice( startIndex, length );
		}




		public int Count( char c ) {
			int num = len > 0 ? 1 : 0;
			for (int i = 0; i < len; i++) if (ptr[i] == c) num++;
			return num;
		}




		public Slicer GetSlicer( char slicerChar ) {
			return new Slicer( slicerChar, this );
		}








		#region STATIC
		#endregion
		public static PtrText Null => new PtrText() { ptr = null, len = -1 };








		#region OPERATORS
		#endregion
		public char this[int i] {
			get => ptr[i];
			set => ptr[i] = value;
		}




		public static bool operator ==( PtrText a, PtrText b ) {
			if (a.len != b.len) return false;
			for (int i = 0; i < a.len; i++) if (a[i] != b[i]) return false;
			return true;
		}




		public static bool operator !=( PtrText a, PtrText b ) {
			return !(a == b);
		}




		public static bool operator ==( PtrText a, string b ) {
			if (a.len != b.Length) return false;
			for (int i = 0; i < a.len; i++) if (a[i] != b[i]) return false;
			return true;
		}




		public static bool operator !=( PtrText a, string b ) {
			return !(a == b);
		}








		#region CONVERSION
		#endregion
		public static implicit operator PtrText( string txt ) => new PtrText( txt.AsSpan() );




		public Span<char> AsSpan => IsNull ? null : new Span<char>( ptr, len );




		public string AsString => IsNull ? null : new string( ptr, 0, len );




		public override string ToString() => AsString;
	}







	#region EXTENSIONS
	#endregion
	public static partial class Extensions {
		public static bool StartsWith( this string str, PtrText txt ) {
			if (txt.len > str.Length) return false;
			for (int i = 0; i < txt.len; i++) if (str[i] != txt[i]) return false;
			return true;
		}




		public static bool Contains( ref this PtrText text, char c ) {
			for (int i = 0; i < text.len; i++) if (text[i] == c) return true;
			return false;
		}
	}
}
