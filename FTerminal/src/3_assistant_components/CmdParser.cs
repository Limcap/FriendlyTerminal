using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.FriendlyTerminal {
	public partial class CmdParser {

		public const char CMD_WORD_SEPARATOR = ' ';
		public const char CMD_TERMINATOR = ':';
		public const string CMD_TERMINATOR_AS_STRING = ":";
		public const string PREDICTIONS_SEPARATOR = "     ";

		protected readonly Node _invalidNode;
		protected readonly Node _startNode;

		// Output variables. Will be used to store the results from parsing.
		public Node confirmedNode;
		public List<Node> predictedNodes;
		public ACommand parsedCmd;







		public CmdParser( Dictionary<string, Type> commandsSet, string locale ) {
			_invalidNode = new Node() { word = "Invalid command" };
			_startNode = new Node() { word = "@" };

			// Results
			confirmedNode = _startNode;
			predictedNodes = new List<Node>();

			Stopwatch sw = new Stopwatch();
			sw.Start();
			var memoryBefore = GC.GetTotalMemory( true );
			Node.BuildGraph( commandsSet, CMD_WORD_SEPARATOR, CMD_TERMINATOR_AS_STRING, _startNode );
			var memoryAfter = GC.GetTotalMemory( true );
			var memoryOccupied = memoryAfter - memoryBefore;
			sw.Stop();
			Trace.WriteLine( "memory occupied by tree: " + memoryOccupied );
			Trace.WriteLine( "time taken to build tree: " + sw.ElapsedMilliseconds );
		}








		public void Parse( PString inputString, string locale )
			=> Parse( locale, inputString, _startNode, _invalidNode, ref confirmedNode, ref parsedCmd, ref predictedNodes );

		//public void ConstructParsedCmd( string locale )
		//	=>ConstructCommand( confirmedNode, locale, ref parsedCmd );
		public bool TryAdvanceTerminator( string locale ) {
			if (confirmedNode is null) return false;
			var input = (PString)CMD_TERMINATOR_AS_STRING;
			var newConfirmedNode = confirmedNode.Traverse( ref input );
			var hasFoundCommand = newConfirmedNode.word == CMD_TERMINATOR_AS_STRING && newConfirmedNode != confirmedNode;
			confirmedNode = newConfirmedNode;
			ConstructCommand( confirmedNode, locale, ref parsedCmd );
			return hasFoundCommand;
		}

		public void GetPredictionPossibilitiesText( StringBuilder res_string )
			=> GetPossibilities( predictedNodes, res_string );

		public StringBuilder GetConfirmedText( StringBuilder res_string = null )
			=> GetConfirmedText( _startNode, res_string );

		public void GetPredictionText( int indexOfPrediction, StringBuilder res_string )
			=> GetSelected( predictedNodes, indexOfPrediction, res_string );

		public void Reset() {
			parsedCmd = null;
			confirmedNode = null;
			_startNode.next = null;
			predictedNodes.Clear();
		}








		#region PURE FUNCTIONS
		#endregion




		public unsafe static void Parse(
			string locale, PString input, Node startNode, Node invalidNode,
			ref Node res_confirmed, ref ACommand res_cmd, ref List<Node> res_predicted
			) {
			res_predicted.Clear();
			res_confirmed = startNode;

			// If the input is empty, the confirmed is the start node, and the predicted will be all reachable from it.
			if (input.IsNullOrEmpty) {
				res_predicted.AddRange( res_confirmed.edges );
				return;
			}

			// Creates a stack-alloc copy of the input so it can be modified freely.
			var ptr = stackalloc char[input.len];
			for (int i = 0; i < input.len; i++) ptr[i] = input[i];
			var inputTemp = new PString( ptr, input.len );

			FixInputTermination( ref inputTemp );

			FindConfirmedWords( startNode, ref inputTemp, ref res_confirmed );

			ConstructCommand( res_confirmed, locale, ref res_cmd );

			if (res_cmd is null) {
				// Ignores one whitespace at the end of the input. Since the end word of the invoke command does not end
				// in separator, this space would render the input invalid. This is important because when we press tab
				// to autocompete a word, the predictions wont change until we type space (or a solid char). So when use
				// autocomplete on the last word and then press space, the status bar would show invalid command.
				// Doing this, we allow the input to have this extra space between the end of the string and the terminator
				// and the status bar will show, instead of 'invalid command', the prediction for the terminator char,
				// even though the actual invoke string does not end in space.
				if (inputTemp == ' ') inputTemp.len--;

				PredictNextWord( res_confirmed, inputTemp, invalidNode, ref res_predicted );
			}
		}








		public static void FixInputTermination( ref PString ps ) {
			if (ps.len > 1 && ps[ps.len - 1] == CMD_TERMINATOR) {
				while (ps[ps.len - 2] == CMD_WORD_SEPARATOR) {
					ps[ps.len - 2] = CMD_TERMINATOR;
					ps[ps.len - 1] = CMD_WORD_SEPARATOR;
					ps.len--;
				}
			}
		}








		public static void FindConfirmedWords( Node startNode, ref PString input, ref Node res_confirmed ) {
			res_confirmed = startNode.Traverse( ref input );
		}








		public static void PredictNextWord( Node confirmedNode, PString inputResidue, Node invalidNode, ref List<Node> res_predicted ) {
			// Select the possible next nodes.
			res_predicted.AddRange( confirmedNode.edges.Where( n => n.word.StartsWith( inputResidue ) ).OrderBy( n => n.word ) );
			if (res_predicted.Count == 0 && inputResidue.EndsWith( CMD_TERMINATOR )) res_predicted.Add( invalidNode );
		}








		public static void ConstructCommand( Node confirmedNode, string locale, ref ACommand res_cmd ) {
			if (confirmedNode.value == null) {
				res_cmd = null;
			}
			else if (res_cmd == null || res_cmd.GetType() != confirmedNode.value)
				res_cmd = confirmedNode.value.IsSubclassOf( typeof( ACommand ) )
					? (ACommand)Activator.CreateInstance( confirmedNode.value, locale )
					: null;
		}








		public static void GetPossibilities( List<Node> possibleNextNodes, StringBuilder result ) {
			if (possibleNextNodes.IsNullOrEmpty()) result.Reset( "Command not found" );
			//Index = -1;
			result.Reset();
			foreach (var n in possibleNextNodes) result.Append( n.word ).Append( PREDICTIONS_SEPARATOR );
		}








		public static StringBuilder GetConfirmedText( Node startNode, StringBuilder result = null ) {
			if (result is null) result = new StringBuilder( 30 );
			var node = startNode;
			while (node.next != null) {
				result.Append( node.next.word );
				node = node.next;
				if (node.IsLeafNode) result.Append( CMD_WORD_SEPARATOR );// .word == CMD_TERMINATOR_AS_STRING )
			}
			return result;
		}








		public static void GetSelected( List<Node> predictedNodes, int predictionIndex, StringBuilder res_text ) {
			if (predictionIndex == -1) return;
			res_text.Append( ((PString)predictedNodes[predictionIndex].word).Trim() );
		}







		public string GetFullString() {
			if (confirmedNode.IsLeafNode) return GetConfirmedText().ToString();
			else return null;
		}
		//public static string GetFullString( Node node ) {
		//	var list = new List<string>( 10 );
		//	do {
		//		list.Add( node.word );
		//		node = node.prev;
		//	}
		//	while (node.prev != null);
		//	list.Reverse();
		//	var separator = CMD_WORD_SEPARATOR.ToString();
		//	return string.Join( separator, list );
		//}
	}
}
