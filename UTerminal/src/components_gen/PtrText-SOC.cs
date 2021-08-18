using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.UTerminal {
	public unsafe partial struct PtrText {

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




		public Span<char> AsSpan => (len < 0) ? null : new Span<char>( ptr, len );




		public string AsString => (len < 0) ? null : new string( ptr, 0, len );
		
		
		
		
		public override string ToString() => AsString;
		
		
		
		
		//private string Preview() {
		//	//return ptr == null ? "\"{null pointer}\"" : $"\"{AsString}\"";
		//	var a = ((int)ptr).ToString( "X" );
		//	if (ptr == null) return $"[{a}] {{null pointer}}";
		//	else return $"[{a}] \"{AsString}\"";
		//}
	}
}
