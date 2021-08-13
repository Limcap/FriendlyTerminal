using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Limcap.UTerminal {
	public class Util {
		public enum MapType : uint {
			MAPVK_VK_TO_VSC = 0x0,
			MAPVK_VSC_TO_VK = 0x1,
			MAPVK_VK_TO_CHAR = 0x2,
			MAPVK_VSC_TO_VK_EX = 0x3,
		}

		[DllImport( "user32.dll" )]
		public static extern int ToUnicode(
			 uint wVirtKey,
			 uint wScanCode,
			 byte[] lpKeyState,
			 [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
				StringBuilder pwszBuff,
			 int cchBuff,
			 uint wFlags );

		[DllImport( "user32.dll" )]
		public static extern bool GetKeyboardState( byte[] lpKeyState );

		[DllImport( "user32.dll" )]
		public static extern uint MapVirtualKey( uint uCode, MapType uMapType );

		public static char GetCharFromKey( Key key ) {
			char ch = ' ';

			int virtualKey = KeyInterop.VirtualKeyFromKey( key );
			byte[] keyboardState = new byte[256];
			GetKeyboardState( keyboardState );

			uint scanCode = MapVirtualKey( (uint)virtualKey, MapType.MAPVK_VK_TO_VSC );
			StringBuilder stringBuilder = new StringBuilder( 2 );

			int result = ToUnicode( (uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0 );
			switch (result) {
				case -1:
					break;
				case 0:
					break;
				case 1: {
					ch = stringBuilder[0];
					break;
				}
				default: {
					ch = stringBuilder[0];
					break;
				}
			}
			return ch;
		}




		public static unsafe char* GetPointer( ReadOnlySpan<char> span ) {
			fixed (char* ptr = &span.GetPinnableReference()) { return ptr; }
		}
	}


	public static partial class Extensions {

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

		public static int? MinMax( this int? num, int min, int max ) {
			if (num is null) return null;
			return num.Value < min ? min : num.Value > max ? max : num.Value;
		}

		public static int MinMax( this int num, int min, int max ) {
			return num < min ? min : num > max ? max : num;
		}

		public static bool IsInBetween( this int num, int min, int max ) {
			return num >= min && num <= max;
		}

		public static char? ToPasswordAllowedChar( this Key key ) {
			char c = Util.GetCharFromKey( key );
			int i = c;
			//if (i == 32) return ' ';
			if (i.IsInBetween( 33, 126 ) && i != 92) return c; else return null;
			//int keyCode = (int)key;
			//return keyCode.IsInBetween( 34, 79 ) || keyCode.IsIn(143);
		}
		public static bool IsPasswordAllowedChar( this Key key ) {
			int c = (int)Util.GetCharFromKey( key );
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
	}
}
