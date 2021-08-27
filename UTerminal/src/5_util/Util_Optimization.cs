using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stan = System.ReadOnlySpan<char>;
using Chan = System.Span<char>;

namespace Limcap.FTerminal {


	public ref struct StanTuple {
		//public StanTuple( ref Stan item1, ref Stan item2 ) {
		//	this.item1 = item1;
		//	this.item2 = item2;
		//}
		public Stan item1;
		public Stan item2;
	}




	public ref struct SpanTuple<T> {
		public SpanTuple( ref Span<T> first, ref Span<T> second ) {
			this.first = first;
			this.second = second;
		}
		public Span<T> first;
		public Span<T> second;
	}




	public static partial class Extensions {
		public static int IndexOf( this Chan chan, char searchedChar, int startIndex = 0 ) {
			startIndex = startIndex < 0 ? 0 : startIndex;
			for (int i = startIndex; i < chan.Length; i++)
				if (chan[i] == searchedChar) return i;
			return -1;
		}

		public static int IndexOf( this Stan stan, char searchedChar, int startIndex = 0 ) {
			startIndex = startIndex < 0 ? 0 : startIndex;
			for (int i = startIndex; i < stan.Length; i++)
				if (stan[i] == searchedChar) return i;
			return -1;
		}

		public static Stan SliceAtChar( this Stan stan, char threshold, int startIndex = 0 ) {
			for (int i = startIndex; i < stan.Length; i++)
				if (stan[i] == threshold) return stan.Slice( startIndex, i - startIndex );
			return null;
		}

		public static bool StartsWith( this string str, Stan part ) {
			if (str.Length < part.Length) return false;
			for (int i = 0; i < part.Length; i++)
				if (str[i] != part[i]) return false;
			return true;
		}


		public static Span<T> FindAndRemove<T, K>( ref this Span<T> span, Func<T, K> identifier, Span<K> elements ) {
			Span<int> indexesFound = stackalloc int[span.Length];
			int count = 0;
			foreach (ref var i in indexesFound) i = -1;
			for (int i = 0; i < span.Length; i++) if (elements.Contains( identifier( span[i] ) )) indexesFound[count++] = i;
			Span<T> missing = span.Slice( 0, count );
			for (int i = 0; i < indexesFound.Length; i++) if (indexesFound[i] > -1) missing[count++] = span[indexesFound[i]];
			return missing;
		}

		public static bool Contains<T>( ref this Span<T> collection, T element ) {
			foreach (T item in collection) if (item.Equals( element )) return true;
			return false;
		}


		public static int CountChar( ref this Stan stran, char searchedChar ) {
			int count = 0;
			foreach (char c in stran) if (c == searchedChar) count++;
			return count;
		}


		public static bool EqualsString( ref this Stan stan, string other ) {
			if (stan.Length != other.Length) return false;
			for (int i = 0; i < stan.Length; i++) if (stan[i] != other[i]) return false;
			return true;
		}


		public static bool Contains( this Stan stan, char element ) {
			for (int i = 0; i < stan.Length; i++)
				if (stan[i] == element) return true;
			return false;
		}


		public static StringBuilder Reset( this StringBuilder sb, string newString = null ) {
			sb.Length = 0;
			if (newString != null) sb.Append( newString );
			return sb;
		}
	}
}
