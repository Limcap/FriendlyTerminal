using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace Limcap.UTerminal {
	public partial class Assistant {

		// CONSTANTS
		//protected const char CMD_WORD_SEPARATOR = ' ';
		//protected const char CMD_TERMINATOR = ':';
		//protected const string CMD_TERMINATOR_AS_STRING = ":";
		protected const char ARGS_SEPARATOR = ',';
		protected const char ARG_VALUE_SEPARATOR = '=';
		protected const string PREDICTIONS_SEPARATOR = "     ";

		// USED BY COMMAND PROCESSING
		//protected static readonly Node _invalidCmdNode = new Node() { word = "Invalid command" };
		//protected readonly Node _startNode = new Node() { word = "@" };
		protected readonly List<string> _invokeStrings;
		//protected Node _confirmedNode;
		//protected List<Node> _predictedNodes;

		// USED BYT ARGUMENT PROCESSING
		protected readonly Dictionary<string, Type> _commandsSet;
		protected readonly string _locale;
		protected ACommand _currentCmd;
		private PString _lastInput;
		private PString _lastInputArgs;

		protected readonly StringBuilder _predictionResult = new StringBuilder( 60 );
		protected readonly StringBuilder _autocompleteResult = new StringBuilder( 30 );

		protected readonly ArgParser _argsParser;
		protected readonly CmdParser _cmdParser;




		public int Index { get; protected set; }
		public bool AutocompleteParams { get; protected set; } //false is command prediction, true is parameter prediction








		#region SETUP
		#endregion




		public Assistant( Dictionary<string, Type> commandsSet, string locale ) {

			_cmdParser = new CmdParser( commandsSet, locale );
			_argsParser = new ArgParser( 8 );

			_locale = locale;
			_commandsSet = commandsSet;
		}








		#region PREDICTION MAIN LOGIC
		#endregion




		public StringBuilder GetPredictions( string input ) {
			var (inpCmd, inpArgs) = SplitInput( input );
			
			// Saves the input for the autocomplete functionality.
			_lastInput = input;
			_lastInputArgs = inpArgs;
			
			_cmdParser.Parse( inpCmd, _locale );
	
			if (_cmdParser.parsedCmd == null) {
				_cmdParser.GetPredictionPossibilitiesText( _predictionResult );//AssembleCommandPrediction( _cmdParser.predictedNodes, result: _predictionResult );
				AutocompleteParams = false;
			}

			else {
				_argsParser.Parse( inpArgs, _cmdParser.parsedCmd?.Parameters );
				_argsParser.GetPredictionPossibilities( _predictionResult );//AssembleParametersPrediction( _predictedParams, result: _autocompleteResult );
				AutocompleteParams = true;
			}

			// Resets the autocomplete index;
			Index = -1;

			return _predictionResult;
		}








		protected (PString inpCmd, PString inpArgs) SplitInput( string input ) {
			var slicer = ((PString)input).GetSlicer( CmdParser.CMD_TERMINATOR, PString.Slicer.Mode.IncludeSeparatorAtEnd );
			var inpCmd = slicer.Next();
			var inpArgs = slicer.Remaining();
			inpArgs.Trim();
			return (inpCmd, inpArgs);
		}



		//protected static void ConstructCommandObject( Node confirmedNode, string locale, ref ACommand result ) {
		//	if (confirmedNode.cmdType != null) {
		//		if (result == null || result.GetType() != confirmedNode.cmdType)
		//			result = confirmedNode.cmdType.IsSubclassOf( typeof( ACommand ) )
		//				? (ACommand)Activator.CreateInstance( confirmedNode.cmdType, locale )
		//				: null;
		//	}
		//}








		//protected void AssembleCommandPrediction( List<Node> predictedNodes, StringBuilder result ) {
		//	if (predictedNodes.IsNullOrEmpty()) result.Reset( "Command not found" );
		//	Index = -1;
		//	result.Reset();
		//	foreach (var n in _predictedNodes) result.Append( n.word ).Append( PREDICTIONS_SEPARATOR );
		//}








		//protected void AssembleParametersPrediction( IEnumerable<ACommand.Parameter> parameters, StringBuilder result ) {
		//	result.Reset();

		//	if (parameters.Count() == 0) {
		//		if (!_lastInputArgs.IsEmpty) result.Append( "--" + _lastInputArgs + "--" );
		//		else result.Append( string.Empty );
		//		//result.Append( "'Enter' to execute the command" );
		//	}
		//	else {
		//		foreach (var p in parameters)
		//			result.Append( p.optional ? $"[{p.name}=]" : $"{p.name}=" ).Append( PREDICTIONS_SEPARATOR );
		//	}
		//}








		#region PROCESSING OF COMMAND INPUT
		#endregion




		/*
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
		*/








		#region PROCESSING OF ARGUMENT INPUT
		#endregion




		//protected static unsafe void ProcessArgsInput( ACommand cmd, PString inpArgs, Arg.Analyzer outArgs, List<ACommand.Parameter> outPredictionList, StringBuilder outPredictionString ) {
		//protected static unsafe void ProcessArgsInput( ACommand cmd, PString inpArgs, ArgParser outArgs ) {
		//	if (cmd?.Parameters.IsNullOrEmpty() ?? true || inpArgs.IsNull) return;		
		//	outArgs.Parse( cmd, inpArgs );
		//}







		#region OTHER
		#endregion




		public StringBuilder GetNextAutocompleteEntry( string input ) {
			// Adjust Index
			 Index++;
			//if (AutocompleteParams == false && Index >= _cmdParser.predictedNodes.Count ||
			//	 AutocompleteParams == true && Index >= _argsParser.possible.Count )
			//	Index = -1;

			_autocompleteResult.Clear();
			
			if (!AutocompleteParams) {
				if (Index >= _cmdParser.predictedNodes.Count) Index = -1;
				_cmdParser.GetConfirmedText( _autocompleteResult );
				_cmdParser.GetPredictionText( Index, _autocompleteResult );
			}
			else {
				if (Index >= _argsParser.possible.Count) Index = -1;
				_cmdParser.GetConfirmedText( _autocompleteResult );
				_argsParser.GetConfirmedText( _autocompleteResult );
				_argsParser.GetPredictionText( Index, _autocompleteResult );
			}

			return _autocompleteResult;
		}
	}
}
