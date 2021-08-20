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

			var memoryBefore = GC.GetTotalMemory( true );
			foreach (var term in _invokeStrings)
				AddCommand( term );
			var memoryAfter = GC.GetTotalMemory( true );
			var memoryOccupied = memoryAfter - memoryBefore;
			Trace.WriteLine( "memory occupied: " + memoryOccupied );
		}








		protected void AddCommand( string invokeTerm ) {

			//var before = GC.GetTotalMemory( true );
			//object a = new int();
			//var after = GC.GetTotalMemory( true );
			//var g = new Node() { word = "Hello" };
			//var res = after - before;
			//File.WriteAllText( $"size-of-node-{res}.txt", "" );
			//unsafe {
			//	var gch = GCHandle.Alloc( g, GCHandleType.Pinned );
			//	var ptr = gch.AddrOfPinnedObject();
			//	Trace.WriteLine( $"Node: {g.word} => " + GetSizeInMem( g ) );
			//}

			//var words = ((PString)invokeTerm).GetSlicer( CMD_WORD_SEPARATOR );
			//var node = _startNode;
			//Node n;
			//while (words.HasNext) {
			//	var word = words.Next( PString.Slicer.Mode.IncludeSeparatorAtEnd );
			//	// skips words that have length 0, cause when there are multiple spaces between the words.
			//	if (word.len == 0) continue;

			//	if (words.HasNext) {
			//		node = node.AddIfNotPresent( word.AsString );
			//	}
			//	else {
			//		node = node.AddIfNotPresent( word.AsString + CMD_WORD_SEPARATOR );
			//		node.AddIfNotPresent( CMD_TERMINATOR_AS_STRING, _commandsSet[invokeTerm] );
			//	}
			//}


			var words = invokeTerm.Split( CMD_WORD_SEPARATOR );
			var node = _startNode;
			for (int i = 0; i < words.Length; i++) {
				// skips words that have length 0, cause when there are multiple spaces between the words.
				if (words[i].Length == 0) continue;

				if (i < words.Length - 1) {
					node = node.AddIfNotPresent( words[i] + CMD_WORD_SEPARATOR );
				}
				else {
					node = node.AddIfNotPresent( words[i] + CMD_WORD_SEPARATOR );
					node.AddIfNotPresent( CMD_TERMINATOR_AS_STRING, _commandsSet[invokeTerm] );
				}
			}
		}

		public long GetSizeInMem( object o ) {
			long size = 0;
			using (Stream s = new MemoryStream()) {
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize( s, o );
				size = s.Length;
				return size;
			}
		}







		#region PREDICTION MAIN LOGIC
		#endregion




		public StringBuilder GetPredictions( string input ) {
			var (inpCmd, inpArgs) = SplitInput( input );
			FixInput( ref inpCmd, ref inpArgs );

			ProcessCommandInput( inpCmd, _startNode, result1: ref _confirmedNode, result2: ref _predictedNodes );
			bool inputIsValid = _confirmedNode.IsLeafNode;

			// Saves the input for the autocomplete functionality.
			_lastInput = input;
			
			if (!inputIsValid) {
				// Resets the 'current command' field, since the 'current input' doesn't match any known invoke string.
				// This is important becuase the autocomplete method (invoked by pressing TAB, will use this field).
				_currentCmd = null;
				AssembleCommandPrediction( _predictedNodes, inpCmd, result: _autocompleteResult );
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
			return (inpCmd, inpArgs);
		}








		protected static void FixInput( ref PString inpCmd, ref PString inpArgs ) {
			// remove trailing space in the arguments string
			inpArgs.Trim();
			// fix space before colon in the invoke string
			//if (inpCmd.len > 1 && inpCmd[inpCmd.len - 1] == CMD_TERMINATOR) {
			//	while (inpCmd[inpCmd.len - 2] == CMD_WORD_SEPARATOR) {
			//		inpCmd[inpCmd.len - 2] = CMD_TERMINATOR;
			//		inpCmd[inpCmd.len - 1] = CMD_WORD_SEPARATOR;
			//		inpCmd.len--;
			//	}
			//}
		}








		protected void AssembleCommandPrediction( List<Node> predictedNodes, PString inpCmd, StringBuilder result ) {
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




		protected static void ProcessCommandInput( PString inpCmd, Node initialNode, ref Node result1, ref List<Node> result2 ) {
			result2.Clear();
			result1 = initialNode;

			if (inpCmd.IsEmpty) {
				result2.AddRange( result1.edges );
				return;
			}

			PString curWord = PString.Empty;
			bool wordHasMatch = true;

			// Used to recall if the input has a terminator, because in the next line the terminator will be removed.
			// Swaps the terminator for word separator, so the last word can be matched if there's no separator between
			// it and the terminator. This is necessary because for every node the word ends with a separator character
			// except for the terminator node whose word is only the terminator character itself.
			bool endsInTerminator = inpCmd.EndsWith( CMD_TERMINATOR );
			bool endsInSeparator = inpCmd[inpCmd.len-2] == CMD_WORD_SEPARATOR;
			if (endsInTerminator && !endsInSeparator) inpCmd[inpCmd.len - 1] = CMD_WORD_SEPARATOR;
			else if (endsInTerminator) inpCmd.len--;
			//if (endsInTerminator) inpCmd[inpCmd.len - 1] = CMD_WORD_SEPARATOR;

			var wordPicker = inpCmd.GetSlicer( CMD_WORD_SEPARATOR, PString.Slicer.Mode.IncludeSeparatorAtEnd );

			// Checks word for word of the input, and tries to match them against an edge of the last confirmed node.
			// While each word is matched, the confirmed node is updated. if all words are matched, the input is
			// considered a valid invoke string. There are 2 ways the input will not be considered as such:
			// 1. An unmatched word is identified;
			// 2. All words are matched but the input does not end with the terminator character.
			while (wordPicker.HasNext) {
				curWord = wordPicker.Next();

				//if (!inputWordPicker.HasNext && endsInTerminator && curWord.len > 1 )
				//	curWord.Trim( CMD_TERMINATOR );
				//if (curWord.IsEmpty && !wordPicker.HasNext) break;
				wordHasMatch = result1.FindNext( curWord );

				if (wordHasMatch) {
					result1 = result1.next;
					curWord = PString.Empty;
					if ((!wordPicker.HasNext|| wordPicker.PeekNext().IsEmpty) && endsInTerminator) {
						wordHasMatch = result1.FindNext( CMD_TERMINATOR_AS_STRING );
						if (wordHasMatch) result1 = result1.next;
						else curWord = PString.Null;
						break;
					}
				}
				else {
					break;
					//if (curWord.EndsWith( CMD_WORD_SEPARATOR ) && inputHasTerminator) result2.Add( _invalidCmdNode );
					//if (endsInSeparator || endsInTerminator) result2.Add( _invalidCmdNode );
					//else result2.AddRange( result1.edges.Where( n => n.word.StartsWith( curWord ) ).OrderBy( n => n.word ) );
					//break;
				}
			}
			if (!wordHasMatch) {
				result2.AddRange( result1.edges.Where( n => n.word.StartsWith( curWord ) ).OrderBy( n => n.word ) );
				if(result2.Count == 0) result2.Add( _invalidCmdNode );
				//result1 = result1.FindNext( CMD_TERMINATOR ) ? result1.next : result1;
				//if (endsInSeparator || endsInTerminator) result2.Add( _invalidCmdNode );
				//result1 = result1.FindNext( CMD_TERMINATOR ) ? result1.next : result1;
				//break;
			}
			if (endsInTerminator && !endsInSeparator) inpCmd[inpCmd.len - 1] = CMD_TERMINATOR;
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
