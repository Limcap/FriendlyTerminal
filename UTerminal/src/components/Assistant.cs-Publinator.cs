using System;
using System.Collections.Generic;
using System.Text;
using static Limcap.UTerminal.InputSolver;

namespace Limcap.UTerminal {
	public class Assistant_Publinator : Assistant {

		#region CONSTANTS
		#endregion

		public char CMD_WORD_SEPARATOR => CMD_WORD_SEPARATOR;
		public char CMD_TERMINATOR => CMD_TERMINATOR;
		public char ARGS_SEPARATOR => ARGS_SEPARATOR;
		public char ARG_VALUE_SEPARATOR => ARG_VALUE_SEPARATOR;
		public string PREDICTIONS_SEPARATOR => PREDICTIONS_SEPARATOR;

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

		public Assistant_Publinator( Dictionary<string, Type> commandsSet, string locale ) : base( commandsSet, locale ) {}

		public unsafe Arg.Array ConstructArgsArray( PString inpArgs, ref Arg.Array result ) => Assistant.ConstructArgsArray( inpArgs, ref result );
		
		public void ConstructCommandObject( Node confirmedNode, string locale, ref ACommand result ) => Assistant.ConstructCommandObject( confirmedNode, locale, ref result );
		
		public unsafe void FindPossibleParams( ACommand cmd, ref Arg.Array args, List<ACommand.Parameter> result ) => Assistant.FindPossibleParams( cmd, ref args, result );

		public void FixInput( ref PString inpCmd, ref PString inpArgs ) => Assistant.FixInput( ref inpCmd, ref inpArgs );

		#region PROCESSING OF ARGUMENT INPUT
		#endregion

		public unsafe void ProcessArgsInput( ACommand cmd, PString inpArgs, List<ACommand.Parameter> result ) => Assistant.ProcessArgsInput( cmd, inpArgs, result );

		#region PROCESSING OF COMMAND INPUT
		#endregion

		public void ProcessCommandInput( PString inpCmd, Node initialNode, ref Node result1, ref List<Node> result2 ) => Assistant.ProcessCommandInput( inpCmd, initialNode, ref result1, ref result2 );

		#region OTHER
		#endregion

		//public StringBuilder GetNextAutocompleteEntry( string input ) => GetNextAutocompleteEntry( input );

		#region PREDICTION MAIN LOGIC
		#endregion

		//public StringBuilder GetPredictions( string input ) => GetPredictions( input );

		public void AddCommand( string invokeTerm ) => base.AddCommand( invokeTerm );

		public void AssembleCommandPrediction( List<Node> predictedNodes, StringBuilder result ) => base.AssembleCommandPrediction( predictedNodes, result );

		public void AssembleParametersPrediction( IEnumerable<ACommand.Parameter> parameters, StringBuilder result ) => base.AssembleParametersPrediction( parameters, result );

		public (PString inpCmd, PString inpArgs) SplitInput( string input ) => base.SplitInput( input );
	}
}