using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static Limcap.UTerminal.InputSolver;

namespace Limcap.UTerminal {
	public partial class Assistant {

		// CONSTANTS
		protected const char CMD_WORD_SEPARATOR = ' ';
		protected const char CMD_TERMINATOR = ':';
		protected const char ARGS_SEPARATOR = ',';
		protected const char ARG_VALUE_SEPARATOR = '=';
		protected const string PREDICTIONS_SEPARATOR = "     ";

		// USED BY COMMAND PROCESSING
		protected static readonly Node _invalidCmdNode = new Node() { word = "Invalid command." };
		protected readonly Node _startNode;
		protected readonly List<string> _invokeStrings;
		protected Node _confirmedNode;
		protected List<Node> _predictedNodes;


		// USED BYT ARGUMENT PROCESSING
		protected readonly Dictionary<string, Type> _commandsSet;
		protected readonly string _locale;
		protected ACommand _currentCmd;
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
			_startNode = new Node() { word = "@" };

			_confirmedNode = _startNode;
			_predictedNodes = new List<Node>();
			_locale = locale;
			_commandsSet = commandsSet;
			_invokeStrings = commandsSet?.Keys?.OrderBy( c => c ).ToList() ?? new List<string>();
			foreach (var term in _invokeStrings)
				AddCommand( term );
		}








		protected void AddCommand( string invokeTerm ) {
			var words = invokeTerm.Split( CMD_WORD_SEPARATOR );
			var node = _startNode;
			for (int i = 0; i < words.Length; i++) {
				// skips words that have length 0, cause when there are multiple spaces between the words.
				if (words[i].Length == 0) continue;

				if (i < words.Length - 1)
					node = node.AddIfNotPresent( words[i] + CMD_WORD_SEPARATOR );
				else
					node = node.AddIfNotPresent( words[i] + CMD_TERMINATOR, _commandsSet[invokeTerm] );
			}
		}







		#region PREDICTION MAIN LOGIC
		#endregion




		//public StringBuilder GetPredictions( string input ) {
		//	var (inpCmd, inpArgs) = SplitInput( input );
		//	FixInput( ref inpCmd, ref inpArgs );

		//	// We start by processing the command part of the input. This will update the nextWord variable and
		//	// the confirmedNode variable:
		//	// nextWord = the first encountered word in the input that doesn't match with any command.
		//	// confirmedNode = the furthest in node in the node tree that could be matched against the input.
		//	var (unconfirmedWord, confirmedNode) = ProcessCommandInput( inpCmd, _startNode, ref _predictedNodes );

		//	// After processing the command input, if the nextWord is not null, it means it could not be matched,
		//	// thus we must provide prediction assistance for it. This might be a partially typed word.
		//	// Yet, if nextWord is indeed null but the confirmed node is not a leaf node, it means every words in the
		//	// input has been matched but they do not configure a full invoke string yet, so we must provide assistance
		//	// showing the possible next words for this half-entered command.
		//	bool InputHasUnmatchedWord = !unconfirmedWord.IsNullOrEmpty;
		//	bool LastConfirmedNodeIsNotLeaf = !confirmedNode.IsLeafNode;
		//	bool inputIsInvokeCommand = confirmedNode.IsLeafNode;
		//	//if (!unconfirmedWord.IsNullOrEmpty || !confirmedNode.IsLeafNode) {
		//	if (!inputIsInvokeCommand) {
		//		AssembleCommandPrediction( _predictedNodes, result: _autocompleteResult );
		//		//AssembleCommandPrediction( unconfirmedWord, confirmedNode, result: _assistantResult );
		//		// remove any current confirmed command. This is important when invoking the autocomplete method.
		//		_currentCmd = null;
		//		CurrentPredictedPart = false;
		//	}


		//	// If the command part has the terminator char, then automatically (via Pstring.Slicer) the argsPart must
		//	// not be null, so the checks "argsPart.isNull" and "!confirmedNode.IsLeaf" must always have the same output.
		//	else if (inpArgs.IsNull) {
		//		_autocompleteResult.Reset( "now it's time for parameter assistance" );
		//	}


		//	else {
		//		// Construct the command object in case the command has been confirmed
		//		if (confirmedNode.IsLeafNode)
		//			ConstructCommandObject( confirmedNode, _locale, result: ref _currentCmd );

		//		ProcessArgsInput( _currentCmd, inpArgs, result: _predictedParams );
		//		AssembleParametersPrediction( _predictedParams, result: _autocompleteResult );
		//		CurrentPredictedPart = true;
		//	}
		//	Index = -1;
		//	return _autocompleteResult;
		//}
		public StringBuilder GetPredictions( string input ) {
			var (inpCmd, inpArgs) = SplitInput( input );
			FixInput( ref inpCmd, ref inpArgs );

			ProcessCommandInput( inpCmd, _startNode, result1: ref _confirmedNode, result2: ref _predictedNodes );
			bool inputIsValid = _confirmedNode.IsLeafNode;
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
			var slicer = ((PString)input).GetSlicer( CMD_TERMINATOR );
			var inpCmd = slicer.Next( PString.Slicer.Mode.IncludeSeparatorAtEnd );
			var inpArgs = slicer.Remaining();
			return (inpCmd, inpArgs);
		}








		protected static void FixInput( ref PString inpCmd, ref PString inpArgs ) {
			// remove trailing space in the arguments string
			inpArgs.Trim();
			// fix space before colon in the invoke string
			if (inpCmd.len > 1 && inpCmd[inpCmd.len - 1] == CMD_TERMINATOR) {
				while (inpCmd[inpCmd.len - 2] == CMD_WORD_SEPARATOR) {
					inpCmd[inpCmd.len - 2] = CMD_TERMINATOR;
					inpCmd[inpCmd.len - 1] = CMD_WORD_SEPARATOR;
					inpCmd.len--;
				}
			}
		}








		protected void AssembleCommandPrediction( List<Node> predictedNodes, StringBuilder result ) {
			if (predictedNodes.IsNullOrEmpty()) result.Reset( "Command not found" );
			Index = -1;
			result.Reset();
			foreach (var n in _predictedNodes) result.Append( n.word ).Append( PREDICTIONS_SEPARATOR );
		}








		protected void AssembleParametersPrediction( IEnumerable<ACommand.Parameter> parameters, StringBuilder result ) {
			result.Reset();
			if (parameters.IsNullOrEmpty()) return;
			foreach (var p in parameters)
				result.Append( p.optional ? $"[{p.name}=]" : $"{p.name}=" ).Append( PREDICTIONS_SEPARATOR );
		}








		#region PROCESSING OF COMMAND INPUT
		#endregion




		//protected static (PString, Node) ProcessCommandInput( PString inpCmd, Node initialNode, ref List<Node> result ) {
		//	var inputWordPicker = inpCmd.GetSlicer( CMD_WORD_SEPARATOR );

		//	Node confirmed = initialNode;
		//	PString curWord = PString.Empty;
		//	bool inputIsWrong = false;
		//	bool wordHasNoMatch;
		//	//bool wordIsLastAndHasNoTerminator;
		//	bool inputHasNoTerminator = !inpCmd.EndsWith( CMD_TERMINATOR );
		//	result.Clear();

		//	// A confirmed word/node means it has been matched against an invoke string and we can move on.
		//	// However for a word to be confirmed, there are two requirements:
		//	// 1. The word must be found in one of the next nodes of the current confirmed node;
		//	// 2. It can't be the last word of the command part OR it must end with a colon.
		//	// we must check for 2 because if the word is the last word of the input and there's no space after it,
		//	// the user might type another letter and change that word, so it can't be considered complete until there's
		//	// a space after it.
		//	while (inputWordPicker.HasNext && !inputIsWrong) {
		//		curWord = inputWordPicker.Next();
		//		// Skips words with legth 0, caused by inputs with multiple spaces between solid characters.
		//		if (curWord.len == 0) continue;
		//		// if a next node with the word was not found OR there's no space after the word...
		//		// break, because this is the word that needs assistance.
		//		//if (!confirmed.FindNext( nextWord ) || !(slicer.HasNextSlice || inpCmd.EndsWith( ' ' ) || inpCmd.EndsWith( ':' ))) break;
		//		wordHasNoMatch = !confirmed.FindNext( curWord );
		//		//wordIsLastAndHasNoTerminator = !inputWordPicker.HasNext && !inpCmd.EndsWith( CMD_TERMINATOR );
		//		//bool IsLastWord_and_IsIncomplete = !(slicer.HasNextSlice || inpCmd.EndsWith( ':' ));
		//		//bool IsLastWord_and_IsIncomplete = __wordHasNoMatch__ || (__isLastWordOfInput__ && __noColonOnLastWord__);
		//		if (inputIsWrong = wordHasNoMatch || !inputWordPicker.HasNext && inputHasNoTerminator) {
		//			result = confirmed.edges.Where( n => n.word.StartsWith( curWord ) ).OrderBy( n => n.word ).ToList();
		//			break;
		//		}
		//		else {
		//			confirmed = confirmed.next;
		//			curWord = PString.Empty;
		//		}
		//	}

		//	//if (!curWord.IsNullOrEmpty || !confirmed.IsLeafNode)
		//	//bool InputHasUnmatchedWord = !curWord.IsNullOrEmpty;
		//	//bool LastConfirmedNodeIsNotFinal = !confirmed.IsLeafNode;
		//	//if (InputHasUnmatchedWord || LastConfirmedNodeIsNotFinal)
		//	//if(inputDoesntMatchInvokeString)
		//	//	result = confirmed.edges.Where( n => n.word.StartsWith( curWord ) ).OrderBy( n => n.word ).ToList();
		//	//else result.Clear();

		//	return (curWord, confirmed);
		//}
		protected static void ProcessCommandInput( PString inpCmd, Node initialNode, ref Node result1, ref List<Node> result2 ) {
			result2.Clear();
			result1 = initialNode;

			if (inpCmd.IsEmpty) {
				result2.AddRange( result1.edges );
				return;
			}

			var inputWordPicker = inpCmd.GetSlicer( CMD_WORD_SEPARATOR );
			PString curWord = PString.Empty;
			bool inputIsIvalid = false;
			bool wordHasNoMatch;
			bool wordIsLastButHasNoTerminator;
			bool inputHasNoTerminator = !inpCmd.EndsWith( CMD_TERMINATOR );

			// Checks word for word of the input, and tries to match them against an edge of the last confirmed node.
			// While each word is matched, the confirmed node is updated. if all words are matched, the input is
			// considered a valid invoke string. There are 2 ways the input will not be considered as such:
			// 1. An unmatched word is identified;
			// 2. All words are matched but the last word does not have the terminator character.

			while (inputWordPicker.HasNext && !inputIsIvalid) {

				// Skips words with legth 0, derived from inputs with multiple spaces between solid characters.
				// Will not skip if it is the first word, because then we must 
				curWord = inputWordPicker.Next( PString.Slicer.Mode.IncludeSeparatorAtEnd );
				//if (curWord.len == 0 || curWord.len == 1 && curWord[0] == CMD_WORD_SEPARATOR ) continue;
				//if (curWord.len == 0 ) continue;
				
				wordHasNoMatch = !result1.FindNext( curWord );
				wordIsLastButHasNoTerminator = !inputWordPicker.HasNext && inputHasNoTerminator;

				if (inputIsIvalid = wordHasNoMatch || wordIsLastButHasNoTerminator) {
					if (curWord.EndsWith( CMD_WORD_SEPARATOR )) result2.Add( _invalidCmdNode );
					else result2.AddRange( result1.edges.Where( n => n.word.StartsWith( curWord ) ).OrderBy( n => n.word ) );
				}
				else {
					result1 = result1.next;
					curWord = PString.Empty;
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
			if (cmd.Parameters.IsNullOrEmpty() || inpArgs.IsNull) return;

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
			var a = _autocompleteResult.Reset( input );
			for (int i = a.Length; i > 0; i--) {
				if (a[i - 1] != CMD_WORD_SEPARATOR) a.Length--;
				else break;
			}

			if (Index == -1) return _autocompleteResult;

			//_autocompleteResult.Append( ' ' );
			if (CurrentPredictedPart == false)
				_autocompleteResult.Append( _predictedNodes[Index].word );
			else
				_autocompleteResult.Append( _predictedParams[Index].name ).Append( ARG_VALUE_SEPARATOR );

			return _autocompleteResult;
		}
	}
}
