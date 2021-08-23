using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using static Limcap.UTerminal.InputSolver;

namespace Limcap.UTerminal {
	public partial class Assistant {

		// CONSTANTS
		protected const char CMD_WORD_SEPARATOR = ' ';
		protected const char CMD_TERMINATOR = ':';
		protected const string CMD_TERMINATOR_AS_STRING = ":";
		protected const char ARGS_SEPARATOR = ',';
		protected const char ARG_VALUE_SEPARATOR = '=';
		protected const string PREDICTIONS_SEPARATOR = "     ";

		// USED BY COMMAND PROCESSING
		protected static readonly Node _invalidCmdNode = new Node() { word = "Invalid command" };
		//protected static readonly Node _terminatorNode = new Node() { word = ":" };
		protected readonly Node _startNode = new Node() { word = "@" };
		protected readonly List<string> _invokeStrings;
		protected Node _confirmedNode;
		protected List<Node> _predictedNodes;


		// USED BYT ARGUMENT PROCESSING
		protected readonly Dictionary<string, Type> _commandsSet;
		protected readonly string _locale;
		protected ACommand _currentCmd;
		private PString _lastInput;
		private PString _lastInputArgs;
		protected readonly List<ACommand.Parameter> _predictedParams = new List<ACommand.Parameter>( 8 );
		// Auxiliary objects, (used temporarily inside methods to avoid allocating new discardable objects)


		protected readonly StringBuilder _predictionResult = new StringBuilder( 60 );
		protected readonly StringBuilder _autocompleteResult = new StringBuilder( 30 );
		protected readonly Arg[] _processedArgs = new Arg[8];

		//protected readonly List<ACommand.Parameter> _aux_predictedParams { get; set; } = new List<ACommand.Parameter>( 8 );






		public int Index { get; protected set; }
		public bool CurrentPredictedPart { get; protected set; } //false is command prediction, true is parameter prediction








		#region SETUP
		#endregion




		public Assistant( Dictionary<string, Type> commandsSet, string locale ) {
			//_startNode = new Node() { word = "@" };

			_confirmedNode = _startNode;
			_predictedNodes = new List<Node>();
			_locale = locale;
			_commandsSet = commandsSet;

			Stopwatch sw = new Stopwatch();
			sw.Start();
			var memoryBefore = GC.GetTotalMemory( true );
			Node.BuildTree( _commandsSet, CMD_WORD_SEPARATOR, CMD_TERMINATOR_AS_STRING, _startNode );
			var memoryAfter = GC.GetTotalMemory( true );
			var memoryOccupied = memoryAfter - memoryBefore;
			sw.Stop();
			Trace.WriteLine( "memory occupied by tree: " + memoryOccupied );
			Trace.WriteLine( "time taken to build tree: " + sw.ElapsedMilliseconds );
		}








		#region PREDICTION MAIN LOGIC
		#endregion




		public StringBuilder GetPredictions( string input ) {
			var (inpCmd, inpArgs) = SplitInput( input );

			ProcessCommandInput( inpCmd, _startNode, result1: ref _confirmedNode, result2: ref _predictedNodes );
			bool inputIsValid = _confirmedNode.IsLeafNode;

			// Saves the input for the autocomplete functionality.
			_lastInput = input;
			_lastInputArgs = inpArgs;
			
			if (!inputIsValid) {
				// Resets the 'current command' field, since the 'current input' doesn't match any known invoke string.
				// This is important becuase the autocomplete method (invoked by pressing TAB, will use this field).
				_currentCmd = null;
				AssembleCommandPrediction( _predictedNodes, result: _autocompleteResult );
				CurrentPredictedPart = false;
			}

			// If the command part has the terminator char, then automatically (via Pstring.Slicer) the argsPart must
			// not be null, so the checks "!argsPart.isNull" and "confirmedNode.IsLeaf" must always have the same output.
			else {
				// Construct the command object in case the command has been confirmed
				ConstructCommandObject( _confirmedNode, _locale, result: ref _currentCmd );
				//ProcessArgsInput( _currentCmd, inpArgs, result: _predictedParams );
				ProcessArgsInput( _currentCmd, inpArgs, args: _processedArgs, result: _predictedParams );
				AssembleParametersPrediction( _predictedParams, result: _autocompleteResult );
				CurrentPredictedPart = true;
			}
			Index = -1;
			return _autocompleteResult;
		}








		protected (PString inpCmd, PString inpArgs) SplitInput( string input ) {
			var slicer = ((PString)input).GetSlicer( CMD_TERMINATOR, PString.Slicer.Mode.IncludeSeparatorAtEnd );
			var inpCmd = slicer.Next();
			var inpArgs = slicer.Remaining();
			inpArgs.Trim();
			return (inpCmd, inpArgs);
		}








		//protected void AssembleCommandPrediction( List<Node> predictedNodes, PString inpCmd, StringBuilder result ) {
		protected void AssembleCommandPrediction( List<Node> predictedNodes, StringBuilder result ) {
			if (predictedNodes.IsNullOrEmpty()) result.Reset( "Command not found" );
			Index = -1;
			result.Reset();
			foreach (var n in _predictedNodes) result.Append( n.word ).Append( PREDICTIONS_SEPARATOR );
		}








		protected void AssembleParametersPrediction( IEnumerable<ACommand.Parameter> parameters, StringBuilder result ) {
			result.Reset();

			if (parameters.Count() == 0) {
				if (!_lastInputArgs.IsEmpty) result.Append( "--" + _lastInputArgs + "--" );
				else result.Append( string.Empty );
				//result.Append( "'Enter' to execute the command" );
			}
			else {
				foreach (var p in parameters)
					result.Append( p.optional ? $"[{p.name}=]" : $"{p.name}=" ).Append( PREDICTIONS_SEPARATOR );
			}
		}








		#region PROCESSING OF COMMAND INPUT
		#endregion




		protected unsafe static void ProcessCommandInput( PString inputP1, Node initialNode, ref Node result1, ref List<Node> result2 ) {
			result2.Clear();
			result1 = initialNode;
			// If the input is empty, theres no need for processing it. We return all edges from the start node.
			if (inputP1.IsNullOrEmpty) {
				result2.AddRange( result1.edges );
				return;
			}
			// Creates a stack-allocated copy of the inputP1 so it can be modified freely.
			var ptr = stackalloc char[inputP1.len];
			for (int i = 0; i < inputP1.len; i++) ptr[i] = inputP1[i];
			var inputP1temp = new PString( ptr, inputP1.len );
			// Fix the termination of the input copy.
			FixCommandPartTermination( ref inputP1temp );
			// Traverse the nodes.
			result1 = initialNode.Traverse( ref inputP1temp );
			// Ignores one whitespace at the end of the input. Since the last word of the invoke command does not end
			// in separator, this space would render the input invalid. This is important because when we press tab
			// to autocompete a word, the predictions wont change until we type space (or a solid char). So when use
			// autocomplete on the last word and then press space, the status bar would show invalid command.
			// Doing this, we allow the input to have this extra space between the end of the string and the terminator
			// and the status bar will show, instead of 'invalid command', the prediction for the terminator char,
			// even though the actual invoke string does not end in space.
			if (inputP1temp == ' ') inputP1temp.len--;
			// Select the possible next nodes.
			result2.AddRange( result1.edges.Where( n => n.word.StartsWith( inputP1temp ) ).OrderBy( n => n.word ) );
			if (result2.Count == 0) result2.Add( _invalidCmdNode );
		}








		protected static void FixCommandPartTermination( ref PString ps ) {
			//if (ps.len > 1 && ps[ps.len - 1] == CMD_WORD_SEPARATOR) ps.len--;
			if (ps.len > 1 && ps[ps.len - 1] == CMD_TERMINATOR) {
				while (ps[ps.len - 2] == CMD_WORD_SEPARATOR) {
					ps[ps.len - 2] = CMD_TERMINATOR;
					ps[ps.len - 1] = CMD_WORD_SEPARATOR;
					ps.len--;
				}
			}
		}








		protected static void ConstructCommandObject( Node confirmedNode, string locale, ref ACommand result ) {
			if (confirmedNode.cmdType != null) {
				if (result == null || result.GetType() != confirmedNode.cmdType)
					result = confirmedNode.cmdType.IsSubclassOf( typeof( ACommand ) )
						? (ACommand)Activator.CreateInstance( confirmedNode.cmdType, locale )
						: null;
			}
		}








		#region PROCESSING OF ARGUMENT INPUT
		#endregion




		protected static unsafe void ProcessArgsInput( ACommand cmd, PString inpArgs, Arg[] args, List<ACommand.Parameter> result ) {
			if (cmd?.Parameters.IsNullOrEmpty() ?? true || inpArgs.IsNull) return;

			var argsCount = inpArgs.Count( ARGS_SEPARATOR );
			var argsArrPtr = stackalloc Arg[argsCount];
			var argsArr = new Arg.Array( argsArrPtr, argsCount );
			ConstructArgsArray( inpArgs, cmd, ref argsArr );

			FindPossibleParams( cmd, ref argsArr, result );
		}








		protected static unsafe Arg.Array ConstructArgsArray( PString argsTxt, ACommand cmd, ref Arg.Array result ) {
			if (result.Length > 0) {
				var slicer = argsTxt.GetSlicer( ARGS_SEPARATOR );
				for (int i = 0; i < result.Length; i++) {
					var slice = slicer.Next();
					result[i] = new Arg( ref slice );
				}
			}
			result.ConfirmParams( cmd );
			return result;
		}








		protected static unsafe void FindPossibleParams( ACommand cmd, ref Arg.Array args, List<ACommand.Parameter> result ) {
			result.Clear();

			//Initially we add the parameters to the result, based on the name of the last argument.
			if (args.IsNull || args.Last.name.IsNullOrEmpty)
				result.AddRange( cmd.Parameters );
			if (!args.Last.NameIsComplete || args.Last.ValueIsEmpty)
				cmd.Parameters.GetByNamePrefix( args.Last.name, result );



			// Then we remove from the result every parameter that has already been confirmed.
			for (int i = 0; i < args.Length ; i++) {
				if (args[i].NameIsComplete) {
					var paramIndex = result.GetIndexByName( args[i].name );
					if (paramIndex > -1) result.RemoveAt( paramIndex );
				}
			}
		}
		protected static unsafe void PredictParams( ACommand cmd, ref Arg.Array args, StringBuilder result, List<ACommand.Parameter> result2 ) {
			result.Clear();

			// if there is no args or the name of the last arg beeing type is completed (has an = sign)
			// then will return nothing because the user is currently typing the value of the parameter.
			if (!args.IsNull && args.Last1.NameIsComplete) { }

			// if user has not typed in any args, show all of them
			else if( args.IsNull || args.Length == 0 ) {
				result2.AddRange( cmd.Parameters );
				return;
			}

			// Identify confirmed parameters.
			var confirmed = stackalloc int[cmd.Parameters.Length];
			for (int i = 0; i <args.Length; i++) {
				ref var arg = ref args[i];
				if (arg.NameIsComplete) {
					var paramIndex = cmd.Parameters.GetIndexByName( arg.name );
					if (paramIndex > -1) confirmed[paramIndex] = 1; result2.RemoveAt( paramIndex );
				}
			}

			// 
			if (!args.Last1.name.IsEmpty) {
				result2.GetByNamePrefix( args.Last.name, result2 );
			}

			for (int i = 0; i < cmd.Parameters.Length; i++) {
				if (confirmed[i] == 0) continue;
				if (!cmd.Parameters[i].name.StartsWith( args.Last1.name )) continue;
				result2.Add( cmd.Parameters[i] );
			}
			if (paramIndex == -1) result.Append( "Invalid parameter" );

		}








		#region OTHER
		#endregion




		public StringBuilder GetNextAutocompleteEntry( string input ) {
			Index++;
			if (Index >= _predictedNodes.Count) Index = -1;
			var a = _autocompleteResult.Clear();

			var node = _startNode;
			while(node.next != null) {
				_autocompleteResult.Append( node.next.word );
				node = node.next;
				if (node.IsLeafNode) _autocompleteResult.Append( ' ' );// .word == CMD_TERMINATOR_AS_STRING )
			}

			if (Index == -1) return _autocompleteResult;

			if (CurrentPredictedPart == false)
				_autocompleteResult.Append( ((PString)_predictedNodes[Index].word).Trim() );
			else
				_autocompleteResult.Append( _predictedParams?[Index].name ).Append( ARG_VALUE_SEPARATOR );

			return _autocompleteResult;
		}
		public StringBuilder GetNextAutocompleteEntry_old( string input ) {
			Index++;
			if (Index >= _predictedNodes.Count) Index = -1;
			var a = _autocompleteResult.Reset( _lastInput.ToString() );
			for (int i = a.Length; i > 0; i--) {
				if (a[i - 1] != CMD_WORD_SEPARATOR) a.Length--;
				else break;
			}

			if (Index == -1) return _autocompleteResult;

			//_autocompleteResult.Append( ' ' );
			if (CurrentPredictedPart == false)
				_autocompleteResult.Append( ((PString)_predictedNodes[Index].word).Trim() );
			else
				_autocompleteResult.Append( _predictedParams[Index].name ).Append( ARG_VALUE_SEPARATOR );

			return _autocompleteResult;
		}
	}
}
