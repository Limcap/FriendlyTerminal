using Limcap.DataStructures;
using System.Collections.Generic;
using System.Linq;

namespace Limcap.UTerminal {
	public class CommandPredictor {
		public CommandPredictor( IEnumerable<string> commands = null ) {
			_startNode = new Node();
			_confirmedNodes = new List<Node>();
			_currentPrediction = new List<Node>();
			_commands = commands?.ToList() ?? new List<string>();
			if (commands != null) CreateInternalTree();
		}

		private List<string> _commands;
		private Node _startNode;
		private List<Node> _confirmedNodes;
		private List<Node> _currentPrediction;
		//int lastAutoCompleteIndex = -1;
		public int Index { get; private set; }
		public bool Activated { get; internal set; }

		public class Node {
			//[DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
			public KeyedSet<string, Node> nextNodes = new KeyedSet<string, Node>( ( n ) => n.word );
			public Node prev;
			public string word;
			//public Node GetNext(string value) => nextNodes.HasKey(value) ? nextNodes.Get(value) : null;
			public Node GetNext( string value ) => nextNodes.FirstOrDefault( n => n.word == value );
			public override string ToString() => word;

			internal Node AddIfNotPresent( string word ) {
				var node = nextNodes.FirstOrDefault( n => n.word == word );
				if (node is null) {
					node = new Node() { prev = this, word = word };
					nextNodes.Add( node );
				}
				return node;
			}
		}

		private void CreateInternalTree() {
			foreach (var term in _commands)
				ExtendInternalTree( term );
		}
		internal void ExtendInternalTree( string term ) {
			var words = term.Split( ' ' );
			var _currentNode = _startNode;
			//foreach( var word in words ) {
			//	_currentNode = _currentNode.AddIfNotPresent( word );
			//}
			words.ForEach( ( i, word ) => _currentNode = _currentNode.AddIfNotPresent( word ) );
		}
		//private void CreateInternalTree() {
		//	Index = 0;
		//	foreach( var term in _candidates ) {
		//		Node prev = null;
		//		for( int i=0; i<term.Length; i++ ) {
		//			string partial = i == term.Length-1 ? term : term.Remove( i + 1 );
		//			var next = _initialNodes.FirstOrDefault( n => n.value == partial ) ?? new Node() { value = partial };
		//			if (prev is null) _initialNodes.Add( next );
		//			else prev.nextNodes.Add( next );
		//			prev = next;
		//		}
		//	}
		//}
		public string GetPredictions( string inputBuffer ) {
			if (!Activated) return null;
			//if (inputBuffer.Length == 0) return string.Empty;
			var words = inputBuffer.Split( ' ' );
			int length = words.Length;
			Node lastConfirmedNode = _startNode;
			int unpredictedWordIndex = -1;
			//if (words.Length < 2) _predictedNodes.Clear();
			if (_confirmedNodes.Count > words.Length - 1) _confirmedNodes = _confirmedNodes.Take( words.Length - 1 ).ToList();

			for (int i = 0; i < words.Length - 1; i++) {
				if (_confirmedNodes.Count > i) {
					if (_confirmedNodes[i].word == words[i]) {
						lastConfirmedNode = _confirmedNodes[i];
						unpredictedWordIndex = i + 1;
						continue;
					}
					else {
						unpredictedWordIndex = i;
						_confirmedNodes = _confirmedNodes.Take( i ).ToList();
						break;
					}
				}
				else {
					var existingNode = lastConfirmedNode.GetNext( words[i] );
					if (existingNode != null) {
						_confirmedNodes.Add( existingNode );
						lastConfirmedNode = existingNode;
						unpredictedWordIndex = i + 1;
					}
					else {
						unpredictedWordIndex = i;
						break;
					}
				}
			}

			//if (_predictedNodes.Count == 0) {
			//	lastPredictedNode = _initialNode;
			//	for (int i = 0; i < length; i++) {
			//		var existingNode = lastPredictedNode.GetNext( words[i] );
			//		if (existingNode != null) {
			//			_predictedNodes.Add( existingNode );
			//			lastPredictedNode = existingNode;
			//		}
			//		else {
			//			unpredictedWordIndex = i;
			//			break;
			//		}
			//	}
			//}
			//else {
			//	for (int i = 0; i < length; i++) {
			//		if (i < _predictedNodes.Count) {
			//			if (_predictedNodes[i].word == words[i]) {
			//				lastPredictedNode = _predictedNodes[i];
			//				continue;
			//			}
			//		}
			//		else {
			//			unpredictedWordIndex = i;
			//			_predictedNodes = _predictedNodes.Take( i ).ToList();
			//			break;
			//		}
			//	}
			//}

			//if (unpredictedWordIndex < _predictedNodes.Count - 1) {
			//	_predictedNodes = _predictedNodes.Take( unpredictedWordIndex ).ToList();
			//}

			var unpredictedWord = unpredictedWordIndex < 0 ? words[0] : words[unpredictedWordIndex];
			_currentPrediction = lastConfirmedNode.nextNodes.Where( n => n.word.StartsWith( unpredictedWord ) ).OrderBy( n => n.word ).ToList();
			Index = -1;
			var display = _currentPrediction.Aggregate( string.Empty, ( a, c ) => a += c.word + "     " ); //"          "
			return display;
		}
		//private void SelectNextOption() {
		//	var options = _statusArea.Text.Split( new string[] { "          " }, StringSplitOptions.None );
		//	SetInputBuffer( options[0] + ' ' );
		//	CaretToEnd();
		//}

		public string GetNextPredictionEntry() {
			Index++;
			if (Index >= _currentPrediction.Count) Index = -1;
			//if (Index == -1) return string.Empty;
			string output = _confirmedNodes.Aggregate( string.Empty, ( agg, c ) => agg += c.word + ' ' );
			if( Index > -1 ) {
				output += _currentPrediction[Index].word;
			}
			return output;
		}
	}
}
