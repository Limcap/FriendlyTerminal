using System;
using Stan = System.ReadOnlySpan<char>;

namespace Limcap.UTerminal {
	public unsafe ref partial struct SorttedInput {
		public unsafe struct Argument {

			public RawTxt Name;
			public RawTxt Value;




			public Argument( ref RawTxt rawArg ) {
				var firstIndex = rawArg.IndexOf( '=' );
				Name = rawArg.Slice( 0, firstIndex > -1 ? firstIndex : rawArg.len );
				Name.Trim();
				Value = firstIndex > -1 ? rawArg.Slice( Math.Max( firstIndex + 1, rawArg.len - 1 ), rawArg.len - (firstIndex + 1) ) : RawTxt.Null;
				Value.Trim();
			}
		}
	}
}