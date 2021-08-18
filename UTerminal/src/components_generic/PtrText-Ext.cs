using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.UTerminal {
	public static partial class Extensions {
		public static bool StartsWith( this string str, PtrText txt ) {
			if (txt.len < str.Length) return false;
			for (int i = 0; i < txt.len; i++) if (str[i] != txt[i]) return false;
			return true;
		}




		public static bool Contains( ref this PtrText text, char c ) {
			for (int i = 0; i < text.len; i++) if (text[i] == c) return true;
			return false;
		}
	}
}
