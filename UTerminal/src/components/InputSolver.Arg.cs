using System;
using System.Diagnostics;

namespace Limcap.UTerminal {
	public unsafe ref partial struct InputSolver {

		[DebuggerDisplay( "{Preview(), nq}" )]
		//[DebuggerDisplay( "name: {(name.IsNull ? \"{null}\" : name)}    value: {(value.IsNull ? \"{" +  "null}\" : value), nq}" )]
		public unsafe partial struct Arg {
			public PString name;
			public PString value;




			public Arg( void* nullPointer ) {
				name = PString.Null;
				value = PString.Null;
			}




			public Arg( ref PString ptxt ) {
				if( ptxt.IsNull) {
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




			public string Preview() {
				var nameStr = name.IsNull ? "{null}" : $"\"{name}\"";
				var valueStr = value.IsNull ? "{null}" : $"\"{value}\"";
				return $"name: {nameStr}    value: {valueStr}";
				//return name.IsNull ? "NAME={null}" : $"NAME=\"{name}\"   VALUE=\"{value}\"";
				//return $"name: {(name.IsNull ? "{null}" : $"\"{name}\"")}    value: {(value.IsNull ? "{null}" : $"\"{value}\"")}";
			}
		}
	}
}