using System;
using System.Diagnostics;
using Stan = System.ReadOnlySpan<char>;

namespace Limcap.UTerminal {
	public unsafe ref partial struct InputSolver {

		[DebuggerDisplay( "{Preview(), nq}" )]
		public unsafe partial struct Arg {

			public PtrText name;
			public PtrText value;


			

			public Arg( ref PtrText rawArg ) {
				if( rawArg.IsNull ) {
					name = PtrText.Null;
					value = PtrText.Null;
				}
				else {
					var firstIndex = rawArg.IndexOf( '=' );
					name = rawArg.Slice(
						startIndex: 0,
						length: firstIndex > -1 ? firstIndex : rawArg.len );
					name.Trim();

					value = firstIndex < 0
						? PtrText.Null
						: rawArg.Slice(
							startIndex: Math.Min( firstIndex + 1, rawArg.len - 1 ),
							length: rawArg.len - firstIndex - 1 );
					value.Trim();
				}
			}




			public string Preview() {
				var nameStr = name.IsNull ? "{null}" : $"\"{name}\"";
				var valueStr = value.IsNull ? "{null}" : $"\"{value}\"";
				return $"name: {nameStr}    value: {valueStr}";
				//return name.IsNull ? "NAME={null}" : $"NAME=\"{name}\"   VALUE=\"{value}\"";
			}
		}
	}
}