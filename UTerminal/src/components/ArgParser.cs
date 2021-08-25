using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Limcap.UTerminal {

	[DebuggerDisplay( "{Length, nq} Arguments" )]
	public unsafe partial class ArgParser {

		protected const char ARGS_SEPARATOR = ',';
		protected const char ARG_VALUE_SEPARATOR = '=';
		protected const string PREDICTIONS_SEPARATOR = "     ";




		public readonly List<Arg> list;
		public readonly List<ACommand.Parameter> possible;
		public bool IsLastParamInvalid { get; private set; }








		public ArgParser( int len ) {
			list = new List<Arg>( len );
			possible = new List<ACommand.Parameter>( len );
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








		public void Parse( PString input, ACommand.Parameter[] parameters ) {
			if (parameters?.IsNullOrEmpty() ?? true || input.IsNull) return;
			Expand( input );
			MatchParameters( parameters );
			CollectPossibilities( parameters );
		}








		public void Expand( PString input ) {
			list.Clear();
			var slicer = input.GetSlicer( ARGS_SEPARATOR );
			while (slicer.HasNext) {
				var slice = slicer.Next();
				list.Add( new Arg( ref slice ) );
			}
		}








		public void MatchParameters( ACommand.Parameter[] parameters ) {
			for (int i = 0; i < Length; i++) {
				var arg = list[i];
				if (arg.NameIsComplete) {
					var paramIndex = parameters.GetIndexByName( this[i].name );
					var p = paramIndex > -1 ? parameters[paramIndex] : null;
					if (p != null) {
						arg.parameter = p;
						list[i] = arg;
					}
				}
			}
		}








		public void CollectPossibilities( ACommand.Parameter[] parameters ) {
			possible.Clear();

			if (list.Count == 0) return;
			var arg = list[list.Count - 1];
			int index = 0;
			if (!arg.NameIsComplete) {
				while ((index = parameters.GetIndexByNamePrefix( arg.name, index )) > -1) {
					var p = parameters[index];
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








		public void GetPredictionPossibilities( StringBuilder res_text ) {
			res_text.Clear();
			// Last, build the string
			//if( possible.Count == 1 && possible[0] == InvalidParam )
			//text.Append( possible[0].description );
			if (IsLastParamInvalid)
				res_text.Append( "Invalid parameter" );
			else if (possible.Count == 0)
				res_text.Append( ',' );
			else foreach (var p in possible)
					res_text.Append( p.optional ? $"[{p.name}=]" : $"{p.name}=" ).Append( PREDICTIONS_SEPARATOR );
		}








		//public void ForgePredictedText( int index, StringBuilder res_text ) {
		//	for (int i = 0; i < Length; i++) {
		//		var a = this[i];
		//		if (Length > 1 && i < Length - 1) {
		//			res_text.Append( a.name ).Append( ARG_VALUE_SEPARATOR ).Append( a.value )
		//				.Append( ARGS_SEPARATOR ).Append( CmdParser.CMD_WORD_SEPARATOR );
		//		}
		//		else {
		//			if (a.NameIsComplete) {
		//				res_text.Append( a.name ).Append( ARG_VALUE_SEPARATOR ).Append( a.value );
		//				if (possible.Count == 0) res_text.Append( ARGS_SEPARATOR );
		//			}
		//			// Append the predicted parameter
		//			else if (index != -1)
		//				res_text.Append( possible?[index].name ).Append( ARG_VALUE_SEPARATOR );
		//		}
		//	}
		//}







		public void GetConfirmedText( StringBuilder res_text ) {
			if (Length == 0) return;
			Arg a;
			for (int i = 0; i < Length - 1; i++) {
				a = this[i];
				res_text.Append( a.name ).Append( ARG_VALUE_SEPARATOR ).Append( a.value )
					.Append( ARGS_SEPARATOR ).Append( CmdParser.CMD_WORD_SEPARATOR );
			}
			a = this[Length - 1];
			if (a.NameIsComplete) {
				res_text.Append( a.name ).Append( ARG_VALUE_SEPARATOR ).Append( a.value );
				if (possible.Count == 0) res_text.Append( ARGS_SEPARATOR );
			}
			//// Append the predicted parameter
			//else if (index != -1)
			//	res_text.Append( possible?[index].name ).Append( ARG_VALUE_SEPARATOR );
		}








		public void GetPredictionText( int index, StringBuilder res_text ) {
			if (index == -1) return;
			else res_text.Append( possible?[index].name ).Append( ARG_VALUE_SEPARATOR );
		}
	}
}
