using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.TextboxTerminal {
	public class Args {
		public readonly string Value;

		public Args(string value) {
			this.Value = value;
		}

		public string GetArg( string arg ) {
			if (!Value.Contains( arg + '=' )) return null;
			var temp = Value.Substring( Value.IndexOf( arg ) + 4 );
			return temp.IndexOf( ' ' ) == -1 ? temp : temp.Remove( temp.IndexOf( ' ' ) );
		}

		public static implicit operator Args(string value) {
			return new Args( value );
		}

		public static implicit operator string(Args args) {
			return args.Value;
		}
	}
}
