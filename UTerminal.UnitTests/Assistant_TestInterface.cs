using System;
using System.Collections.Generic;
using System.Text;
using static Limcap.UTerminal.InputSolver;

namespace Limcap.UTerminal {

	[System.Diagnostics.DebuggerNonUserCode()]
	public class Assistant_TestInterface : Assistant {

		#region CONSTANTS
		#endregion

		public char CMD_WORD_SEPARATOR => Assistant.CMD_WORD_SEPARATOR;
		public char CMD_TERMINATOR => Assistant.CMD_TERMINATOR;
		public char ARGS_SEPARATOR => Assistant.ARGS_SEPARATOR;
		public char ARG_VALUE_SEPARATOR => Assistant.ARG_VALUE_SEPARATOR;
		public string PREDICTIONS_SEPARATOR => Assistant.PREDICTIONS_SEPARATOR;

		#region FIELDS
		#endregion

		//public readonly Node _startNode;
		//public readonly List<string> _invokeStrings;
		public Dictionary<string, Type> _commandsSet => base._commandsSet;
		public string _locale => base._locale;
		public List<ACommand.Parameter> _predictedParams => base._predictedParams;
		public StringBuilder _predictionResult => base._predictionResult;
		public Node _confirmedNode => base._confirmedNode;
		public Node _invalidCmdNode => Assistant._invalidCmdNode;
		public Node _startNode => base._startNode;
		public List<string> _invokeStrings => base._invokeStrings;
		public List<Node> _predictedNodes => base._predictedNodes;
		public ACommand _currentCmd => base._currentCmd;

		#region SETUP
		#endregion

		public Assistant_TestInterface( Dictionary<string, Type> commandsSet, string locale ) : base( commandsSet, locale ) {}

		public Arg.Array ConstructArgsArray( PString inpArgs, ref Arg.Array result ) => Assistant.ConstructArgsArray( inpArgs, ref result );
		
		public void ConstructCommandObject( Node confirmedNode, string locale, ref ACommand result ) => Assistant.ConstructCommandObject( confirmedNode, locale, ref result );
		
		public void FindPossibleParams( ACommand cmd, ref Arg.Array args, List<ACommand.Parameter> result ) => Assistant.FindPossibleParams( cmd, ref args, result );

		public static void FixCommandPartTermination( ref PString inpCmd ) => Assistant.FixCommandPartTermination( ref inpCmd );

		#region PROCESSING OF ARGUMENT INPUT
		#endregion

		public void ProcessArgsInput( ACommand cmd, PString inpArgs, List<ACommand.Parameter> result ) => Assistant.ProcessArgsInput( cmd, inpArgs, result );

		#region PROCESSING OF COMMAND INPUT
		#endregion

		public void ProcessCommandInput( PString inpCmd, Node initialNode, ref Node result1, ref List<Node> result2 ) => Assistant.ProcessCommandInput( inpCmd, initialNode, ref result1, ref result2 );

		#region OTHER
		#endregion

		//public StringBuilder GetNextAutocompleteEntry( string input ) => GetNextAutocompleteEntry( input );

		#region PREDICTION MAIN LOGIC
		#endregion

		//public StringBuilder GetPredictions( string input ) => GetPredictions( input );

		public void AssembleCommandPrediction( List<Node> predictedNodes, StringBuilder result ) => base.AssembleCommandPrediction( predictedNodes, result );

		public void AssembleParametersPrediction( IEnumerable<ACommand.Parameter> parameters, StringBuilder result ) => base.AssembleParametersPrediction( parameters, result );

		public (PString inpCmd, PString inpArgs) SplitInput( string input ) => base.SplitInput( input );




		public void Reset() {
			base._confirmedNode = base._startNode;
			base._predictedNodes.Clear();
			base._predictedParams.Clear();
			base._predictionResult.Clear();
			base._autocompleteResult.Clear();
			base.Index = 0;
		}
	}
}