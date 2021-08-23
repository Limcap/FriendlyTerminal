using System.Diagnostics;

namespace Limcap.UTerminal {
	public partial class Assistant {
		public unsafe partial struct Arg {

			[DebuggerDisplay( "{Length, nq} Arguments" )]
			public unsafe struct Array {
				public PArray<Arg> ptrArr;




				public Array( void* ptr, int len ) {
					ptrArr.ptr = ptr;
					ptrArr.len = len;
				}




				public ref Arg this[int i] {
					get => ref ((Arg*)ptrArr.ptr)[i];
					//set => ((Arg*)ptrArr.ptr)[i] = value;
				}




				[DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
				public Arg[] Elements => ptrArr.AsArray;
				public int Length => ptrArr.len;
				public bool IsNull => ptrArr.ptr == null || ptrArr.len < 1;
				public Arg Last => IsNull ? new Arg( null ) : this[ptrArr.len - 1];
				public ref Arg Last1 => ref this[ptrArr.len - 1];



				public static Arg Null => new Arg( null );




				public void ConfirmParams( ACommand cmd ) {
					for (int i = 0; i < Length; i++) {
						var arg = this[i];
						if (arg.NameIsComplete) {
							var paramIndex = cmd.Parameters.GetIndexByName( this[i].name );
							arg.confirmed = paramIndex > -1;
							this[i].confirmed = true;
						}
					}
				}
			}
		}
	}
}