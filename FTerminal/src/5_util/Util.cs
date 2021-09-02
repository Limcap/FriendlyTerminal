using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Limcap.FriendlyTerminal {
	public class Util {

		public static unsafe char* GetPointer( ReadOnlySpan<char> span ) {
			fixed (char* ptr = &span.GetPinnableReference()) { return ptr; }
		}
		public static unsafe char* GetPointer( string span ) {
			var alloc = GCHandle.Alloc( span, GCHandleType.Pinned );
			char* ptr = (char*)alloc.AddrOfPinnedObject();
			alloc.Free();
			return ptr;
		}
		public static unsafe char* Pin( string text, out GCHandle alloc ) {
			alloc = GCHandle.Alloc( text, GCHandleType.Pinned );
			char* ptr = (char*)alloc.AddrOfPinnedObject();
			return ptr;
		}

		public static void CallGarbageCollector() {
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}
	}








	public static partial class Ext {

		public static bool IsIn<T>( this T searched, params T[] group ) {
			foreach (T item in group) if (item.Equals( searched )) return true;
			return false;
		}
		public static bool IsIn<T>( this T searched, IEnumerable<T> group ) {
			foreach (T item in group) if (item.Equals( searched )) return true;
			return false;
		}

		public static R As<T, R>( this T obj, Func<T, R> action ) {
			return action( obj );
		}

		public static void As<T>( this T obj, Action<T> action ) {
			action( obj );
		}

		//public static void AsNotNull<T>( this T o, Action<T> a ) {
		//	if (o != null) a( o );
		//}

		//public static void AsNotNull<T, R>( this T o, Func<T, R> a ) {
		//	if (o != null) a( o );
		//}

		public static O NullIf<O>( this O obj, Func<O, bool> check ) => check( obj ) ? obj : default( O );

		public static int Swap( this int num, int thisNumber, int forThis ) {
			return num == thisNumber ? forThis : num;
		}

		public static int? MinMax( this int? num, int min, int max ) {
			if (num is null) return null;
			return num.Value < min ? min : num.Value > max ? max : num.Value;
		}

		public static int MinMax( this int num, int min, int max ) {
			return num < min ? min : num > max ? max : num;
		}
		public static double MinMax( this double num, double min, double max ) {
			return num < min ? min : num > max ? max : num;
		}

		public static bool IsInBetween( this int num, int min, int max ) {
			return num >= min && num <= max;
		}

		public static char? ToPasswordAllowedChar( this Key key ) {
			char c = KeyGrabber.GetCharFromKey( key );
			int i = c;
			//if (i == 32) return ' ';
			if (i.IsInBetween( 33, 126 ) && i != 92) return c; else return null;
			//int keyCode = (int)key;
			//return keyCode.IsInBetween( 34, 79 ) || keyCode.IsIn(143);
		}
		public static bool IsPasswordAllowedChar( this Key key ) {
			int c = (int)KeyGrabber.GetCharFromKey( key );
			return c.IsInBetween( 32, 126 ) && c != 92;
		}

		public static object GetConst( this Type type, string fieldName ) {
			var field = type.GetField( fieldName );
			return field != null && field.IsLiteral
				? field.GetValue( null )
				: null;
		}

		public static int? SafeParseInt( this string s, int? d = null ) {
			return int.TryParse( s, out int i ) ? i : d;
		}

		public static Brush SafeParseBrush( this string s, Brush defaultValue = null ) {
			return typeof( Brushes ).GetProperties()
			.FirstOrDefault( p => p.Name.ToLower() == s?.ToLower() )
			.As( c => c != null ? (Brush)c.GetValue( null ) : defaultValue );
		}

		public static void ForEach<T>( this IEnumerable<T> list, Action<int, T> action ) {
			for (int i = 0; i < list.Count(); i++) {
				var item = list.ElementAt( i );
				action( i, item );
			}
		}

		public static bool IsNullOrEmpty<T>( this IEnumerable<T> collection ) {
			return collection is null || collection.Count() == 0;
		}

		public static string JoinStrings( this IEnumerable<string> list, string separator ) {
			return string.Join( separator, list );
		}
	}
}
