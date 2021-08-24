using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Limcap.UTerminal {
	public partial class Assistant {
		public unsafe partial struct Arg {

			[DebuggerDisplay( "{Length, nq} Arguments" )]
			public unsafe class Analyzer {
				public readonly List<Arg> list;
				public readonly List<ACommand.Parameter> possible;
				public bool IsLastParamInvalid { get; private set; }
				//public readonly StringBuilder prediction;
				//public static readonly ACommand.Parameter InvalidParam = new ACommand.Parameter("invalid",ACommand.Parameter.Type.ANY,false, "Invalid parameter" );




				public Analyzer( int len ) {
					list = new List<Arg>( len );
					possible = new List<ACommand.Parameter>( len );
					//prediction = new StringBuilder();
				}




				public Arg this[int i] {
					get => list[i];
					set => list[i] = value;
				}




				[DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
				public Arg[] Elements => list.ToArray();
				public int Length => list.Count;
				public bool IsNull => list == null;
				public Arg Last => IsNull ? new Arg( null ) : this[Length - 1];
				//public ref Arg Last1 => ref this[Length - 1];



				//public static Arg Null => new Arg( null );




				public void Process( ACommand cmd, PString input ) {
					if (cmd?.Parameters.IsNullOrEmpty() ?? true || input.IsNull) return;
					Expand( input );
					MatchParameters( cmd );
					CollectPossibilities( cmd );
				}




				public void Expand( PString input ) {
					list.Clear();
					var slicer = input.GetSlicer( ARGS_SEPARATOR );
					while (slicer.HasNext) {
						var slice = slicer.Next();
						list.Add( new Arg( ref slice ) );
					}
				}




				//public void Match( ACommand cmd ) { //List<ACommand.Parameter> outPredictionList, StringBuilder outPredictionString

				//	possible.Clear();
				//	prediction.Clear();

				//	// If user has not typed in any args, mark all of them.
				//	if (list.Count == 0) {
				//		possible.AddRange( cmd.Parameters );
				//	}

				//	// Else check each parameters already typed in
				//	else {

				//		// If the last parameter has an incomplete name:
				//		var arg = list[list.Count - 1];
				//		if (!arg.NameIsComplete) {

				//			// Add the possible parameters to the aux list
				//			int index = 0;
				//			while ((index = cmd.Parameters.GetIndexByNamePrefix( arg.name, index )) > -1) {
				//				//arg.parameter = cmd.Parameters[index];
				//				possible.Add( cmd.Parameters[index] );
				//				index++;
				//			}

				//			// If there's no possible parameters, say 'invalid parameters'
				//			if (possible.Count == 0) {
				//				prediction.Append( "Invalid parameter" );
				//			}

				//			// Else if there are:
				//			else {

				//				// Verify previous parametes with complete and if they exist within the aux list, remove them.
				//				for (int i = 0; i < list.Count - 1; i++) {
				//					arg = list[i];
				//					if (arg.NameIsComplete) {
				//						index = possible.GetIndexByName( arg.name );
				//						if (index > -1) possible.RemoveAt( index );
				//					}
				//				}
				//			}


				//		}

				//		// If the name of the last parameter is complete, check if it is valid or not.
				//		else {
				//			if (cmd.Parameters.GetIndexByName( arg.name ) == -1)
				//				prediction.Append( "Invalid parameter" );
				//			else prediction.Append( "," );
				//		}
				//	}

				//	// Last, build the string
				//	foreach (var p in possible)
				//		prediction.Append( p.optional ? $"[{p.name}=]" : $"{p.name}=" ).Append( PREDICTIONS_SEPARATOR );
				//}




				public void MatchParameters( ACommand cmd ) {
					for (int i = 0; i < Length; i++) {
						var arg = list[i];
						if (arg.NameIsComplete) {
							var paramIndex = cmd.Parameters.GetIndexByName( this[i].name );
							var p = paramIndex > -1 ? cmd.Parameters[paramIndex] : null;
							if( p != null ) {
								arg.parameter = p;
								list[i] = arg;
							}
						}
					}
				}




				public void CollectPossibilities( ACommand cmd ) {
					possible.Clear();

					if (list.Count == 0) return;
					var arg = list[list.Count - 1];
					int index = 0;
					if( !arg.NameIsComplete ) {
						while ((index = cmd.Parameters.GetIndexByNamePrefix( arg.name, index )) > -1) {
							var p = cmd.Parameters[index];
							if (!IsAlreadyMatched( p )) possible.Add( p );
							index++;
						}
						IsLastParamInvalid = possible.Count == 0;
					}
					else IsLastParamInvalid = false;
				}




				public bool IsAlreadyMatched( ACommand.Parameter p ) {
					for (int i = 0; i < list.Count - 1; i++)
						if (list[i].NameIsComplete && list[i].parameter == p) return true;
					return false;
				}




				public void ForgeText( StringBuilder text ) {
					text.Clear();
					// Last, build the string
					//if( possible.Count == 1 && possible[0] == InvalidParam )
					//text.Append( possible[0].description );
					if (IsLastParamInvalid)
						text.Append( "Invalid parameter" );
					else if (possible.Count == 0)
						text.Append( ',' );
					else foreach (var p in possible)
							text.Append( p.optional ? $"[{p.name}=]" : $"{p.name}=" ).Append( PREDICTIONS_SEPARATOR );
				}
			}
		}
	}
}