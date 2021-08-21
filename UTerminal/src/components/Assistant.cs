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
		protected readonly List<ACommand.Parameter> _predictedParams = new List<ACommand.Parameter>( 8 );
		// Auxiliary objects, (used temporarily inside methods to avoid allocating new discardable objects)


		protected readonly StringBuilder _predictionResult = new StringBuilder( 60 );
		protected readonly StringBuilder _autocompleteResult = new StringBuilder( 30 );

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
			_invokeStrings = commandsSet?.Keys?.OrderBy( c => c ).ToList() ?? new List<string>();

			Stopwatch sw = new Stopwatch();
			sw.Start();
			var memoryBefore = GC.GetTotalMemory( true );
			Node.BuildTree( _invokeStrings, CMD_WORD_SEPARATOR, CMD_TERMINATOR_AS_STRING, _startNode );
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
				ProcessArgsInput( _currentCmd, inpArgs, result: _predictedParams );
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
			if (parameters.IsNullOrEmpty()) {
				result.Append( "'Enter' to execute the command" );
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
			// Ignores the one whitespace at the end of the input. Since the last word of the invoke command does not end in separator,
			// this would make the command be invalid, so we ignore it if it existe. this way, when autocompleting something and then
			// pressing space to get the next prediction wont cause an invalid command.
			if (inputP1temp == ' ') inputP1temp.len--;
			// Calculates the predications.
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




		protected static unsafe void ProcessArgsInput( ACommand cmd, PString inpArgs, List<ACommand.Parameter> result ) {
			if (cmd?.Parameters.IsNullOrEmpty() ?? true || inpArgs.IsNull) return;

			var argsCount = inpArgs.Count( ARGS_SEPARATOR );
			var argsArrPtr = stackalloc Arg[argsCount];
			var argsArr = new Arg.Array( argsArrPtr, argsCount );
			ConstructArgsArray( inpArgs, ref argsArr );

			FindPossibleParams( cmd, ref argsArr, result );
		}








		protected static unsafe Arg.Array ConstructArgsArray( PString inpArgs, ref Arg.Array result ) {
			if (result.Length > 0) {
				var slicer = inpArgs.GetSlicer( ARGS_SEPARATOR );
				for (int i = 0; i < result.Length; i++) {
					var slice = slicer.Next();
					result[i] = new Arg( ref slice );
				}
			}
			return result;
		}








		protected static unsafe void FindPossibleParams( ACommand cmd, ref Arg.Array args, List<ACommand.Parameter> result ) {
			result.Clear();

			//Initially we add the parameters to the result, based on the name of the last argument.
			if (args.IsNull || args.Last.name.IsNullOrEmpty)
				result.AddRange( cmd.Parameters );
			else if (!args.Last.NameIsComplete || args.Last.ValueIsEmpty)
				cmd.Parameters.GetByNamePrefix( args.Last.name, result );

			// Then we remove from the result every parameter that has already been confirmed.
			for (int i = 0; i < args.Length - 1; i++) {
				if (args[i].NameIsComplete) {
					var paramIndex = result.GetIndexByName( args[i].name );
					if (paramIndex > -1) result.RemoveAt( paramIndex );
				}
			}
		}








		#region OTHER
		#endregion




		public StringBuilder GetNextAutocompleteEntry( string input ) {
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
