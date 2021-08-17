﻿using System;
using System.Buffers;
using System.Diagnostics;

namespace Limcap.UTerminal {

	[DebuggerDisplay( "{ToString()}" )]
	public unsafe partial struct PString {
		public char* ptr;
		public int len;




		public PString( ReadOnlySpan<char> text ) {
			ptr = Util.GetPointer( text );
			len = text.Length;
		}




		public PString( Span<char> text ) {
			ptr = Util.GetPointer( text );
			len = text.Length;
		}




		public PString( string text ) {
			ptr = Util.GetPointer( text.AsSpan() );
			len = text.Length;
		}








		#region PROPERTIES
		#endregion
		public bool IsNull => len < 0;
		public bool IsNullOrEmty => len < 1;








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




		public PString Slice( int startIndex, int length=int.MaxValue ) {
			if (length > len) length = len - startIndex;
			//if (startIndex > len - 1) throw new IndexOutOfRangeException( "Slice start index is out of bounds" );
			return new PString() { ptr = ptr + startIndex, len = length };
		}




		public PString SliceToChar( char c, int startIndex = 0 ) {
			int index = IndexOf( ',', startIndex );
			if (index == -1) return PString.Null;
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
		public static PString Null => new PString() { ptr = null, len = -1 };








		#region OPERATORS
		#endregion
		public char this[int i] {
			get => ptr[i];
			set => ptr[i] = value;
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
			if (a.len != b.Length) return false;
			for (int i = 0; i < a.len; i++) if (a[i] != b[i]) return false;
			return true;
		}




		public static bool operator !=( PString a, string b ) {
			return !(a == b);
		}








		#region CONVERSION
		#endregion
		public static implicit operator PString( string txt ) => new PString( txt.AsSpan() );
		public static implicit operator PString( Span<char> txt ) => new PString() { ptr = Util.GetPointer( txt ), len = txt.Length };
		public static implicit operator PString( ReadOnlySpan<char> txt ) => new PString() { ptr = Util.GetPointer( txt ), len = txt.Length };
		//public static implicit operator PString( Memory<char> txt ) => new PString() { ptr = Util.GetPointer( txt ), len = txt.Length };
		//public static implicit operator PString( ReadOnlyMemory<char> txt ) => new PString() { ptr = Util.GetPointer( txt ), len = txt.Length };




		public Span<char> AsSpan => IsNull ? null : new Span<char>( ptr, len );




		public string AsString => IsNull ? null : new string( ptr, 0, len );




		public override string ToString() => AsString;
	}







	#region EXTENSIONS
	#endregion
	public static partial class Extensions {
		public static bool StartsWith( this string str, PString txt ) {
			if (txt.len > str.Length) return false;
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
	}




	//public unsafe PArray<PString> Split( char c, PString* ptr = null, int ptrLen = 0 ) {
	//	var a = MemoryPool<PString>.Shared.Rent( 2 );
	//	a.Memory.Pin().Pointer;
	//}
}