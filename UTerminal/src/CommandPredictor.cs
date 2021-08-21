using Limcap.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Limcap.UTerminal {
	public class CommandPredictor {
		public CommandPredictor( IEnumerable<string> commands = null ) {
			_startNode = new NodeOld();
			_confirmedNodes = new List<NodeOld>();
			_currentPrediction = new List<NodeOld>();
			_commands = new List<string>();
			SetAvailableCommands( commands );
		}
		//public CommandPredictor( Dictionary<string,Type> commands = null ) {
		//	_startNode = new Node();
		//	_confirmedNodes = new List<Node>();
		//	_currentPrediction = new List<Node>();
		//	_commands = new List<string>();
		//	SetAvailableCommands( commands.Keys );
		//	_commands2 = commands;
		//}




		private readonly NodeOld _startNode;
		private List<string> _commands;
		private List<NodeOld> _confirmedNodes;
		private List<NodeOld> _currentPrediction;
		//private Dictionary<string, Type> _commands2;



		public int Index { get; private set; }
		//public bool Activated { get; internal set; }




		public void SetAvailableCommands( IEnumerable<string> commands ) {
			_commands = commands?.OrderBy( c => c ).ToList() ?? new List<string>();
			foreach (var term in _commands)
				AddCommand( term );
		}





		private void AddCommand( string term ) {
			//_commands.Add( term );
			var words = term.Split( ' ' );
			var _currentNode = _startNode;
			words.ForEach( ( i, word ) => _currentNode = _currentNode.AddIfNotPresent( word + (i+1==words.Count()?":":"") ) );
		}




		public string GetPredictions( string inputBuffer ) {
			//if (!Activated) return null;
			if(inputBuffer.Contains( ':' )) { }
			var words = inputBuffer.Split( ' ' );
			int length = words.Length;
			NodeOld lastConfirmedNode = _startNode;
			int unpredictedWordIndex = -1;
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

			var unpredictedWord = unpredictedWordIndex < 0 ? words[0] : words[unpredictedWordIndex];
			_currentPrediction = lastConfirmedNode.nextNodes.Where( n => n.word.StartsWith( unpredictedWord ) ).OrderBy( n => n.word ).ToList();
			Index = -1;
			var display = _currentPrediction.Aggregate( string.Empty, ( a, c ) => a += c.word + "     " ); //"          "
			return display;
		}




		public string GetNextPredictionEntry() {
			Index++;
			if (Index >= _currentPrediction.Count) Index = -1;
			string output = _confirmedNodes.Aggregate( string.Empty, ( agg, c ) => agg += c.word + ' ' );
			if( Index > -1 ) output += _currentPrediction[Index].word;
			return output;
		}




		public string GetConfirmedNodes( string fullInput ) {
			bool hasColon = fullInput.Contains( ':' );
			var input = ((PString)fullInput).SliceToChar( ':' );//.Split( ':' );
			input.len++;
			var words = input.AsString.Split( ' ' );
			int length = words.Length;
			NodeOld lastConfirmedNode = _startNode;
			int unpredictedWordIndex = -1;
			for (int i = 0; i < words.Length-(hasColon?0:1); i++) {
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

			return _confirmedNodes.Aggregate( string.Empty, ( agg, c ) => agg += c.word + ' ' );
		}


		//private string GetParametersPrediction(string input) {
		//	var inputParts = input.Split( ':' );
		//	var invokeText = inputParts[0];
		//	var parameters = inputParts.Length > 0 ? inputParts[1] : string.Empty;
		//	if (!_commands2.ContainsKey( inputParts[0] )) return "Command not found";
		//	var cmdType = _commands2[invokeText];
		//	var cmd = Activator.CreateInstance( _commands2[invokeText] );
		//	var instance = cmdType.IsSubclassOf( typeof( ACommand ) )
		//	? (ICommand)Activator.CreateInstance( cmdType, Locale )
		//	: (ICommand)Activator.CreateInstance( cmdType );
		//}
	}








	public class NodeOld {
		//[DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
		public KeyedSet<string, NodeOld> nextNodes = new KeyedSet<string, NodeOld>( ( n ) => n.word );
		public NodeOld prev;
		public string word;
		public NodeOld GetNext( string value ) => nextNodes.FirstOrDefault( n => n.word == value );
		

		public NodeOld AddIfNotPresent( string word ) {
			var node = nextNodes.FirstOrDefault( n => n.word == word );
			if (node is null) {
				node = new NodeOld() { prev = this, word = word };
				nextNodes.Add( node );
			}
			return node;
		}
	}
}
