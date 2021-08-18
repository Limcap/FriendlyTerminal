using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Limcap.UTerminal.InputSolver;

namespace Limcap.UTerminal {
	internal partial class Assistant {

		// USED BY COMMAND PROCESSING
		private readonly Node _startNode;
		private readonly Node _confirmedNode;
		private List<string> _invokeStrings;
		// Auxiliary objects, (used temporarily inside methods to avoid allocating new discardable objects)
		private List<Node> _predictedNodes;


		// USED BYT ARGUMENT PROCESSING
		private readonly Dictionary<string, Type> _commandsSet;
		private readonly string _locale;
		private ACommand _currentCmd;
		private readonly List<ACommand.Parameter> _predictedParams = new List<ACommand.Parameter>( 8 );
		// Auxiliary objects, (used temporarily inside methods to avoid allocating new discardable objects)

		
		private readonly StringBuilder _predictionResult = new StringBuilder( 60 );
		private readonly StringBuilder _autocompleteResult = new StringBuilder( 30 );

		//private readonly List<ACommand.Parameter> _aux_predictedParams { get; set; } = new List<ACommand.Parameter>( 8 );






		public int Index { get; private set; }
		public bool CurrentPredictedPart { get; private set; } //false is command prediction, true is parameter prediction

		#region SETUP
		#endregion




		public Assistant( Dictionary<string, Type> commandsSet, string locale ) {
			_startNode = new Node() { word = "@" };
			_confirmedNode = _startNode;
			_predictedNodes = new List<Node>();
			_locale = locale;
			_commandsSet = commandsSet;
			_invokeStrings = commandsSet?.Keys?.OrderBy( c => c ).ToList() ?? new List<string>();
			foreach (var term in _invokeStrings)
				AddCommand( term );
		}








		private void AddCommand( string invokeTerm ) {
			var words = invokeTerm.Split( ' ' );
			var node = _startNode;
			for( int i=0; i<words.Length;i++) {
				// skips words that have length 0, cause when there are multiple spaces between the words.
				if (words[i].Length == 0) continue;
				
				//var word = i < words.Length - 1 ? words[i] : words[i] + ':';
				if(i < words.Length - 1)
					node = node.AddIfNotPresent( words[i] );
				else
					node = node.AddIfNotPresent( words[i] + ':', _commandsSet[invokeTerm] );
			}
			//words.ForEach( ( i, word ) => _currentNode = _currentNode.AddIfNotPresent( word ) );
		}







		#region PREDICTION MAIN LOGIC
		#endregion




		public StringBuilder GetPredictions( string input ) {
			var (inpCmd, inpArgs) = SplitInput( input );
			FixCommandInput( ref inpCmd, ref inpArgs );

			// We start by processing the command part of the input. This will update the nextWord variable and
			// the confirmedNode variable:
			// nextWord = the first encountered word in the input that doesn't match with any command.
			// confirmedNode = the furthest in node in the node tree that could be matched against the input.
			var (unconfirmedWord, confirmedNode) = ProcessCommandInput( inpCmd, _startNode, ref _predictedNodes );
			
			// After processing the command input, if the nextWord is not null, it means it could not be matched,
			// thus we must provide prediction assistance for it. This might be a partially typed word.
			// Yet, if nextWord is indeed null but the confirmed node is not a leaf node, it means every words in the
			// input has been matched but they do not configure a full invoke string yet, so we must provide assistance
			// showing the possible next words for this half-entered command.
			//if (!nextWord.IsNullOrEmpty || confirmedNode.edges.Count > 0) {
			if (!unconfirmedWord.IsNullOrEmpty || !confirmedNode.IsLeafNode ) {
				AssembleCommandPrediction( _predictedNodes, result: _autocompleteResult );
				//AssembleCommandPrediction( unconfirmedWord, confirmedNode, result: _assistantResult );
				// remove any current confirmed command. This is important when invoking the autocomplete method.
				_currentCmd = null;
				CurrentPredictedPart = false;
			}


			// If an final node is confirmed,  command is identified from the command input, there's no need for assistance in the command part,
			// and then we must process the argument input.
			else if(inpArgs.IsNull) {
				_autocompleteResult.Reset( "now it's time for parameter assistance" );
			}
			
			
			else {
				// Construct the command object in case the command has been confirmed
				if (confirmedNode.IsLeafNode)
					ConstructCommandObject( confirmedNode, _locale, ref _currentCmd );

				ProcessArgsInput( _currentCmd, inpArgs, result: _predictedParams );
				AssembleParametersPrediction( _predictedParams, result: _autocompleteResult );
				CurrentPredictedPart = true;
			}
			Index = -1;
			return _autocompleteResult;
		}








		private (PString inpCmd, PString inpArgs) SplitInput( string input ) {
			var slicer = ((PString)input).GetSlicer( ':' );
			var inpCmd = slicer.NextSlice();
			var inpArgs = slicer.Remaining();
			return (inpCmd, inpArgs);
		}


		




		
		//private void AssembleCommandPrediction( PString nextWord, Node confirmedNode, StringBuilder result ) {
			//_aux_predictedNodes = confirmedNode.edges.Where( n => n.word.StartsWith( nextWord ) ).OrderBy( n => n.word ).ToList();
		private void AssembleCommandPrediction( List<Node> predictedNodes, StringBuilder result ) {
			if (predictedNodes.IsNullOrEmpty()) result.Reset("Command not found");
			Index = -1;
			result.Reset();
			foreach (var n in _predictedNodes) result.Append( n.word ).Append( "     " );
			//var display = _aux_predictedNodes.Aggregate( string.Empty, ( a, c ) => a += c.value + "     " );
			//return display;
		}








		private void AssembleParametersPrediction( IEnumerable<ACommand.Parameter> parameters, StringBuilder result ) {
			result.Reset();
			if (parameters.IsNullOrEmpty()) return;
			foreach (var p in parameters) result.Append( p.optional ? $"[{p.name}=]" : $"{p.name}=" ).Append( "     " );
		}








		#region PROCESSING OF COMMAND INPUT
		#endregion




		private static (PString, Node) ProcessCommandInput( PString inpCmd, Node initialNode, ref List<Node> result ) {
			var slicer = inpCmd.GetSlicer( ' ' );

			Node confirmed = initialNode;
			PString nextWord = PString.Empty;

			// A confirmed word/node means it has been matched against an invoke string and we can move on.
			// However for a word (nextWord) to be considered confirmed, there are two requirements:
			// 1. The word must be found in one of the next nodes of the current confirmed node;
			// 2. It can't be the last word of the input OR the last char of the input must be a space.
			// we must check for 2 because if the word is the last word of the input and there's no space after it,
			// the user might type another letter and change that word, so it can't be considered complete until there's
			// a space after it.
			while (slicer.HasNextSlice) {
				nextWord = slicer.NextSlice();
				// Skips words with legth 0, caused by inputs with multiple spaces between solid characters.
				if (nextWord.len == 0) continue;
				// if a next node with the word was not found OR there's no space after the word...
				// break, because this is the word that needs assistance.
				if (!confirmed.FindNext( nextWord ) || !(slicer.HasNextSlice || inpCmd.EndsWith( ' ' ) || inpCmd.EndsWith( ':' )))
					break;
				confirmed = confirmed.next;
				nextWord = PString.Empty;
			}

			if (!nextWord.IsNullOrEmpty || !confirmed.IsLeafNode)
				result = confirmed.edges.Where( n => n.word.StartsWith( nextWord ) ).OrderBy( n => n.word ).ToList();
			else result.Clear();

			return (nextWord, confirmed);
		}








		private static void FixCommandInput( ref PString inpCmd, ref PString inpArgs ) {
			// remove trailing space in the arguments string
			inpArgs.Trim();
			// fix space before colon in the invoke string
			if (!inpCmd.IsNull && inpCmd[inpCmd.len - 1] == ' ' && !inpArgs.IsNull) {
				inpCmd.Trim();
				inpCmd.len++;
				inpCmd[inpCmd.len - 1] = ':';
			}
		}








		private static void ConstructCommandObject(Node confirmedNode, string locale, ref ACommand result ) {
			if (confirmedNode.cmdType != null) {
				if (result == null || result.GetType() != confirmedNode.cmdType)
					result = confirmedNode.cmdType.IsSubclassOf( typeof( ACommand ) )
						? (ACommand)Activator.CreateInstance( confirmedNode.cmdType, locale )
						: null;
			}
		}








		#region PROCESSING OF ARGUMENT INPUT
		#endregion




		private static unsafe void ProcessArgsInput( ACommand cmd, PString inpArgs, List<ACommand.Parameter> result ) {
			//var inputSolver = new InputSolver( cmd, inpArgs );
			//inputSolver.SolveCommand( _commandsSet, ref _currentCmd, _locale );
			result.Clear();
			if (cmd.Parameters.Length == 0 || inpArgs.len == 0 ) return;

			var argsCount = inpArgs.Count( ',' );
			var argsArrPtr = stackalloc Arg[argsCount];
			var argsArr = new Arg.Array( argsArrPtr, argsCount );
			ConstructArgsArray( inpArgs, ref argsArr );

			FindPossibleParams( cmd, ref argsArr, result );

			//FindPossibleParams( cmd, ref argsArr, _aux_possibleParams_temp );
			//_aux_possibleParams_main.Clear();
			//_aux_possibleParams_main.AddRange( _aux_possibleParams_temp );
			//FormatParamsNames( _aux_possibleParams_main, _aux_assistantResult );

			//_currentCmd = inputSolver.cmd;
			//_currentConfirmedText = inputSolver.cmdText.AsString + ": ";
			//for (int i = 0; i < inputSolver.args.Length - 1; i++) {
			//	var arg = inputSolver.args[i];
			//	_currentConfirmedText += arg.name + "=" + arg.value + ", ";
			//}
			//_currentSelectedPossibility = -1;
			//return _aux_assistantResult;
		}








		private static unsafe Arg.Array ConstructArgsArray( PString inpArgs, ref Arg.Array result ) {
			if (result.Length > 0) {
				var slicer = inpArgs.GetSlicer( ',' );
				for (int i = 0; i < result.Length; i++) {
					var slice = slicer.NextSlice();
					result[i] = new Arg( ref slice );
				}
			}
			return result;
		}








		private static unsafe void FindPossibleParams( ACommand cmd, ref Arg.Array args, List<ACommand.Parameter> result ) {
			result.Clear();

			//Initially we add the parameters to the result, based on the name of the last argument.
			if (args.Last.name.IsNullOrEmpty)
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
			var a =_autocompleteResult.Reset( input );
			for( int i=a.Length; i>0; i--) {
				if (a[i - 1] != ' ') a.Length--;
				else break;
			}
			
			if (Index == -1) return _autocompleteResult;

			//_autocompleteResult.Append( ' ' );
			if (CurrentPredictedPart == false)
				_autocompleteResult.Append( _predictedNodes[Index].word );
			else
				_autocompleteResult.Append( _predictedParams[Index].name ).Append( '=' );
			
			return _autocompleteResult;
		}




		//public string GetConfirmedNodes( string input ) {
		//	var inp = (PString)input;
		//	var slicer = inp.GetSlicer( ':' );
		//	var inp_cmd = slicer.NextSlice();
		//	var inp_args = slicer.NextSlice();
		//	var sb = new StringBuilder();
		//	var node = _startNode;
		//	while ((node = node.next) != null)
		//		sb.Append( node.word + ' ' );
		//	return sb.ToString();
		//}
	}
}
