using System;
using System.Diagnostics;

namespace Limcap.UTerminal {
	public partial class Assistant {

		[DebuggerDisplay( "{Preview(), nq}" )]
		//[DebuggerDisplay( "name: {(name.IsNull ? \"{null}\" : name)}    value: {(value.IsNull ? \"{" +  "null}\" : value), nq}" )]
		public partial struct Arg {
			public PString name;
			public PString value;
			public ACommand.Parameter parameter;



			public unsafe Arg( void* nullPointer ) {
				name = PString.Null;
				value = PString.Null;
				parameter = null;
			}




			public Arg( ref PString ptxt ) {
				parameter = null;
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




			public void SetParameter( ACommand.Parameter p ) {
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
	}
}