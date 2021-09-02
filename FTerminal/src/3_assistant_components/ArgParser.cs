using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Limcap.FTerminal {

	[DebuggerDisplay( "{Length, nq} Arguments" )]
	public unsafe partial class ArgParser {

		protected const char ARGS_SEPARATOR = ',';
		protected const char ARG_VALUE_SEPARATOR = '=';
		protected const string PREDICTIONS_SEPARATOR = "     ";




		public readonly List<Arg> list;
		public readonly List<Parameter> possible;
		private Parameter[] _parameters;
		public bool IsLastParamInvalid { get; private set; }








		public ArgParser( int len ) {
			list = new List<Arg>( len );
			possible = new List<Parameter>( len );
		}








		public Arg this[int i] {
			get => list[i];
			set => list[i] = value;
		}








		[DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
		public List<Arg> Elements => list;
		public int Length => list.Count;
		public bool IsNull => list == null;
		public Arg Last => IsNull ? new Arg( null ) : this[Length - 1];
		//public ref Arg Last1 => ref this[Length - 1];
		//public static Arg Null => new Arg( null );








		public void Parse( PString input, Parameter[] parameters ) {
			if (parameters?.IsNullOrEmpty() ?? true || input.IsNull) return;
			_parameters = parameters;
			Expand( input );
			MatchParameters();
			CollectPossibilities();
		}
		//public void Parse( List<Arg> args, Parameter[] parameters ) {
		//	list.Clear();
		//	list.AddRange( args );
		//	_parameters = parameters;
		//	MatchParameters();
		//	CollectPossibilities();
		//}








		public void Expand( PString input ) {
			list.Clear();
			var slicer = input.GetSlicer( ARGS_SEPARATOR );
			while (slicer.HasNext) {
				var slice = slicer.Next();
				list.Add( new Arg( ref slice ) );
			}
		}








		public void MatchParameters() {
			for (int i = 0; i < Length; i++) {
				var arg = list[i];
				if (arg.NameIsComplete) {
					var paramIndex = _parameters.GetIndexByName( this[i].name );
					var p = paramIndex > -1 ? _parameters[paramIndex] : null;
					if (p != null) {
						arg.parameter = p;
						list[i] = arg;
					}
				}
			}
		}








		public void CollectPossibilities() {
			possible.Clear();

			if (list.Count == 0) return;
			var arg = list[list.Count - 1];
			int index = 0;
			if (!arg.NameIsComplete) {
				while ((index = _parameters.GetIndexByNamePrefix( arg.name, index )) > -1) {
					var p = _parameters[index];
					if (!IsAlreadyMatched( p )) possible.Add( p );
					index++;
				}
				IsLastParamInvalid = possible.Count == 0;
			}
			else IsLastParamInvalid = false;
		}








		public bool IsAlreadyMatched( Parameter p ) {
			for (int i = 0; i < list.Count - 1; i++)
				if (list[i].NameIsComplete && list[i].parameter == p) return true;
			return false;
		}








		public void GetPossibilities( StringBuilder res_text ) {
			res_text.Clear();
			// Last, build the string
			//if( possible.Count == 1 && possible[0] == InvalidParam )
			//text.Append( possible[0].description );
			if (IsLastParamInvalid)
				res_text.Append( "Invalid parameter" );
			else if (possible.Count == 0 && list.Count < (_parameters?.Length ?? 0))
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







		public StringBuilder GetConfirmedText( StringBuilder res_text = null ) {
			if (Length == 0) return null;
			if (res_text is null) res_text = new StringBuilder();
			Arg a;
			for (int i = 0; i < Length - 1; i++) {
				a = this[i];
				res_text.Append( a.name ).Append( ARG_VALUE_SEPARATOR ).Append( a.value )
					.Append( ARGS_SEPARATOR ).Append( CmdParser.CMD_WORD_SEPARATOR );
			}
			a = this[Length - 1];
			if (a.NameIsComplete) {
				res_text.Append( a.name ).Append( ARG_VALUE_SEPARATOR ).Append( a.value );
				//if (possible.Count == 0) res_text.Append( ARGS_SEPARATOR );
				// case there are no possible predictions AND all parameters have been already entered correctly, will
				// not return a arg separator.
				if (possible.Count == 0 && list.Where( p => p.NameIsComplete ).Count() < _parameters.Length)
					res_text.Append( ARGS_SEPARATOR );
			}
			//// Append the predicted parameter
			//else if (index != -1)
			//	res_text.Append( possible?[index].name ).Append( ARG_VALUE_SEPARATOR );
			return res_text;
		}








		public void GetSelected( int index, StringBuilder res_text ) {
			if (index > -1)
				res_text.Append( possible?[index].name ).Append( ARG_VALUE_SEPARATOR );
		}








		public void Reset() {
			list.Clear();
			possible.Clear();
		}








		public string GetFullString() {
			//return GetFullString( list, parameters ?? _parameters, true );
			var str = list
				.Where( a => a.NameIsComplete && !(a.parameter.optional && a.ValueIsEmpty) )
				.Select( a => $"{a.name}={a.value}" ).JoinStrings( ", " );
			return str;
		}

		//public static string GetFullString( List<Arg> args, Parameter[] parameters, bool excludeEmptyOptional = false ) {
		//	var list = parameters
		//		.Select( p => args
		//			.FindIndex( a => a.name == p.name && (!p.optional || excludeEmptyOptional && !a.ValueIsEmpty) )
		//			.As( i => i > -1 ? $"{p.name}={args[i].value}" : null )
		//		).Where( item => item != null );
		//	return string.Join( ", ", list );

		//	//var argList = args.Where( a => parameters.ToList()
		//	//.FindIndex( p => a.name == p.name && (excludeEmptyOptional ? !p.optional || !a.ValueIsEmpty : true) ) > -1 )
		//	//.Select( a2 => $"{a2.name}={a2.value}" );
		//	//if (argList.Count() == 0) return null;
		//	//return  string.Join( ", ", argList );
		//}
	}
}
