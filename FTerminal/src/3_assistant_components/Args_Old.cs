using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Limcap.FTerminal.ACommand;

namespace Limcap.FTerminal {
	public class Args_Old {
		public readonly string value;

		public Args_Old(string value) {
			this.value = value;
		}

		public string GetArg( string arg, bool caseSensitive = false  ) {
			var v = caseSensitive ? value : value.ToLower();
			var a = caseSensitive ? arg : arg.ToLower();
			if (!v.Contains( a + '=' )) return null;
			var temp = value.Substring( v.IndexOf( a ) + a.Length + 1 );
			return temp.IndexOf( ' ' ) == -1 ? temp : temp.Remove( temp.IndexOf( ' ' ) );
		}

		public string GetArg( Parameter param, bool caseSensitive = false ) {
			string arg = param.name;
			var v = caseSensitive ? value : value.ToLower();
			var a = caseSensitive ? arg : arg.ToLower();
			if (!v.Contains( a + '=' )) return null;
			var temp = value.Substring( v.IndexOf( a ) + a.Length + 1 );
			return temp.IndexOf( ' ' ) == -1 ? temp : temp.Remove( temp.IndexOf( ' ' ) );
		}

		public static implicit operator Args_Old(string value) {
			return new Args_Old( value );
		}

		public static implicit operator string(Args_Old args) {
			return args.value;
		}
	}
}
