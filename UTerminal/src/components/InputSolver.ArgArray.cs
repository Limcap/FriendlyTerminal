﻿using System.Diagnostics;

namespace Limcap.UTerminal {
	public unsafe ref partial struct InputSolver {
		public unsafe partial struct Arg {

			[DebuggerDisplay( "{Preview(), nq}" )]
			public unsafe struct Array {
				public PtrArray<Arg> ptrArr;
				public int Length => ptrArr.len;





				public Array( void* ptr, int len ) {
					ptrArr.ptr = ptr;
					ptrArr.len = len;
				}




				[DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
				public Arg[] Elements => ptrArr.AsArray;




				public Arg this[int i] {
					get => ((Arg*)ptrArr.ptr)[i];
					set => ((Arg*)ptrArr.ptr)[i] = value;
				}




				private string Preview() {
					return $"{Length} Arguments";
					//return $"PtrArray of {typeof( Arg ).Name}, {Length} elements";
				}
			}
		}
	}
}