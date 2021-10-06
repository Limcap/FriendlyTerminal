using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Limcap.FriendlyTerminal {
	//public partial class ArgParser {

	[DebuggerDisplay( "{Preview(), nq}" )]
	//[DebuggerDisplay( "name: {(name.IsNull ? \"{null}\" : name)}    value: {(value.IsNull ? \"{" +  "null}\" : value), nq}" )]
	public partial struct Arg {

		public PString name;
		public PString value;
		public Parameter parameter;








		public unsafe Arg( void* nullPointer ) {
			name = PString.Null;
			value = PString.Null;
			parameter = null;
		}








		public Arg( ref PString ptxt ) {
			parameter = null;
			if (ptxt.IsNull) {
				name = PString.Null;
				value = PString.Null;
			}
			else {
				var firstIndex = ptxt.IndexOf( '=' );
				name = ptxt.Slice(
					startIndex: 0,
					length: firstIndex > -1 ? firstIndex : ptxt.len );
				name.Trim();

				value = firstIndex < 0
					? PString.Null
					: ptxt.Slice(
						startIndex: Math.Min( firstIndex + 1, ptxt.len - 1 ),
						length: ptxt.len - firstIndex - 1 );
				value.Trim();
			}
		}








		public bool NameIsComplete => !value.IsNull;
		public bool ValueIsEmpty => value.IsNullOrEmpty;








		public void SetParameter( Parameter p ) {
			this.parameter = p;
		}








		public string Preview() {
			var nameStr = name.IsNull ? "{null}" : $"\"{name}\"";
			var valueStr = value.IsNull ? "{null}" : $"\"{value}\"";
			return $"name: {nameStr}    value: {valueStr}";
			//return name.IsNull ? "NAME={null}" : $"NAME=\"{name}\"   VALUE=\"{value}\"";
			//return $"name: {(name.IsNull ? "{null}" : $"\"{name}\"")}    value: {(value.IsNull ? "{null}" : $"\"{value}\"")}";
		}
	}
	//}



	public static class Arg_Extensions {
		//public static string Get( this Arg[] array, Parameter param ) {
		//	foreach (var a in array) {
		//		if (a.parameter == param)
		//			return a.value.AsString;
		//	}
		//	return null;
		//}

		public static string GetString( this Arg[] array, Parameter param ) {
			return Get( array, param ).AsString();
		}

		public static List<string> GetList( this Arg[] array, Parameter param ) {
			return Get( array, param ).AsList();
		}

		public static Arg Get( this Arg[] array, Parameter param ) {
			foreach (var a in array) {
				if (a.parameter == param)
					return a;
			}
			return new Arg() { name = param.name, value = null };
		}

		public static string AsString( this Arg arg ) {
			return arg.value.AsString;
		}

		public static List<string> AsList( this Arg arg, char separator = ';' ) {
			if (arg.value.AsString is null) return null;
			var val = arg.value.AsString;
			var list = val.Split( separator ).Select( id => id.Trim() ).ToList() ?? new List<string>();
			list.Remove( string.Empty );
			return list;
		}
	}
}