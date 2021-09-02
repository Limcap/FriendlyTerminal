using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace Limcap.FriendlyTerminal {

	[DebuggerDisplay( "{ToString()}" )]
	public unsafe partial struct PString {
		public string str;
		public int ini;
		public int len;
		//public char* ptr;




		public PString( ReadOnlySpan<char> span ) {
			//ptr = Util.GetPointer( text );
			len = span.Length;
			ini = 0;
			str = span.ToString();
		}




		public PString( Span<char> span ) {
			//ptr = Util.GetPointer( text );
			len = span.Length;
			ini = 0;
			str = span.ToString();
		}




		public PString( char* ptr, int len ) {
			//this.ptr = ptr;
			this.len = len;
			ini = 0;
			str = new string( ptr, ini, len );
		}




		public PString( string text ) {
			str = text;
			//ptr = Util.GetPointer( text );
			len = text?.Length ?? -1;
			ini = 0;
		}








		#region PROPERTIES
		#endregion

		public bool IsNull => len < 0;
		public bool IsNullOrEmpty => len < 1;
		public bool IsEmpty => len == 0;








		#region ACTIONS
		#endregion

		public PString Trim( char c = ' ' ) {
			TrimStart( c );
			TrimEnd( c );
			return this;
		}




		public PString TrimStart( char c = ' ' ) {
			if (len <= 0) return this;
			while (len > 0 && str[ini] == c) {
				ini++;
				len--;
			}
			return this;
		}




		public PString TrimEnd( char c = ' ' ) {
			if (len <= 0) return this;
			while (len > 0 && str[ini + len - 1] == c)
				len--;
			return this;
		}




		public int IndexOf( char searchedChar, int startIndex = 0 ) {
			startIndex = startIndex < 0 ? 0 : startIndex;
			for (int i = startIndex; i < len; i++)
				if (str[ini + i] == searchedChar) return i;
			return -1;
		}




		public PString Slice( int startIndex, int length = int.MaxValue ) {
			if (length > len) length = len - startIndex;
			//if (startIndex > len - 1) throw new IndexOutOfRangeException( "Slice start index is out of bounds" );
			return new PString() { str = str, ini = ini + startIndex, len = length };
		}




		public PString SliceToChar( char c, int startIndex = 0 ) {
			int index = IndexOf( c, startIndex );
			if (index == -1) return PString.Null;
			int length = index - startIndex;
			return Slice( startIndex, length );
		}




		public int Count( char c ) {
			int num = len > 0 ? 1 : 0;
			for (int i = 0; i < len; i++) if (str[ini + i] == c) num++;
			return num;
		}




		public bool StartsWith( PString prefix ) {
			if (len < prefix.len) return false;
			for (int i = 0; i < prefix.len; i++) if (this[i] != prefix[i]) return false;
			return true;
		}




		public bool EndsWith( PString suffix ) {
			if (len < suffix.len) return false;
			for (int i = suffix.len - 1; i >= 0; i--) if (this[i] != suffix[i]) return false;
			return true;
		}
		public bool EndsWith( char suffix ) {
			return len < 1 ? false : this[len - 1] == suffix;
		}




		public void ShiftStart( int amount ) {
			if (amount > len) throw new IndexOutOfRangeException();
			ini += amount;
			len -= amount;
		}




		public Slicer GetSlicer( char slicerChar, Slicer.Mode mode = Slicer.Mode.ExcludeSeparator ) {
			return new Slicer( slicerChar, this, mode );
		}








		#region STATIC
		#endregion

		public static PString Null => new PString() { len = -1 };
		public static PString Empty => new PString( string.Empty );








		#region OPERATORS
		#endregion

		public char this[int i] {
			get => str[ini + i];
			set { fixed (char* p = str) { p[i] = value; } }
		}




		public static bool operator ==( PString a, PString b ) {
			if (a.len != b.len) return false;
			for (int i = 0; i < a.len; i++) if (a[i] != b[i]) return false;
			return true;
		}




		public static bool operator !=( PString a, PString b ) {
			return !(a == b);
		}




		public static bool operator ==( PString a, string b ) {
			if (a.IsNull && b is null) return true;
			if (a.len != b.Length) return false;
			for (int i = 0; i < a.len; i++) if (a[i] != b[i]) return false;
			return true;
		}




		public static bool operator !=( PString a, string b ) {
			return !(a == b);
		}








		#region CONVERSION
		#endregion

		public static implicit operator PString( char c ) => new PString( c.ToString() );
		public static implicit operator PString( string txt ) => new PString( txt );
		public static implicit operator PString( Span<char> txt ) => new PString( txt );
		public static implicit operator PString( ReadOnlySpan<char> txt ) => new PString( txt );// { ptr = Util.GetPointer( txt ), len = txt.Length };
																															 //public static implicit operator PString( Memory<char> txt ) => new PString() { ptr = Util.GetPointer( txt ), len = txt.Length };
																															 //public static implicit operator PString( ReadOnlyMemory<char> txt ) => new PString() { ptr = Util.GetPointer( txt ), len = txt.Length };

		//public Span<char> AsSpan => IsNull ? null : new Span<char>( ptr, len );
		public string AsString => IsNull ? null : str.Substring( ini, len );
		public override string ToString() => AsString;





		#region EQUALITY
		#endregion

		public override bool Equals( object obj ) {
			if (obj is null && this.IsNull) return true;
			if (obj is string other) return this == other;
			if (obj is PString other1) return this == other1;
			return obj.ToString() == this;
			//return base.Equals( obj );
		}
	}







	#region EXTENSIONS
	#endregion

	public static partial class Ext {
		public static bool StartsWith( this string str, PString txt ) {
			if (txt.len > str.Length || txt.IsNull) return false;
			for (int i = 0; i < txt.len; i++) if (str[i] != txt[i]) return false;
			return true;
		}




		public static bool Contains( ref this PString text, char c ) {
			for (int i = 0; i < text.len; i++) if (text[i] == c) return true;
			return false;
		}




		public static PString ToPString( this string str ) {
			return str;
		}




		public unsafe static StringBuilder Append( this StringBuilder sb, PString pstr ) {
			fixed (char* p = pstr.str) {
				return sb.Append( p, pstr.len );
			}
		}
	}




	//public unsafe PArray<PString> Split( char c, PString* ptr = null, int ptrLen = 0 ) {
	//	var a = MemoryPool<PString>.Shared.Rent( 2 );
	//	a.Memory.Pin().Pointer;
	//}
}
