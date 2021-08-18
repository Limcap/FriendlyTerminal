using Limcap.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Limcap.UTerminal {
	public class Assistant {
		public Assistant( IEnumerable<string> commands = null ) {
			_confirmedNodes = new List<Node>();
			_startNode = new Node() { value = "@" };
			_currentPrediction = new List<Node>();
			_commands = new List<string>();
			SetAvailableCommands( commands );
		}




		private readonly Node _startNode;
		private List<string> _commands;
		private List<Node> _confirmedNodes;
		private List<Node> _currentPrediction;



		public int Index { get; private set; }




		public void SetAvailableCommands( IEnumerable<string> commands ) {
			_commands = commands?.OrderBy( c => c ).ToList() ?? new List<string>();
			foreach (var term in _commands)
				AddCommand( term );
		}





		private void AddCommand( string term ) {
			var words = term.Split( ' ' );
			var _currentNode = _startNode;
			words.ForEach( ( i, word ) => _currentNode = _currentNode.AddIfNotPresent( word ) );
		}




		public string GetPredictions( string input ) {
			var inp = (PString)input;
			var slicer = inp.GetSlicer( ':' );
			var inp_cmd = slicer.NextSlice();
			var inp_args = slicer.NextSlice();

			slicer = inp_cmd.GetSlicer( ' ' );
			var words_len = slicer.Count;

			Node confirmed = _startNode;
			PString word = PString.Empty;
			PString nextWord = PString.Empty;

			// A confirmed word/node means the assitance for that word is completed and we can move on.
			// However for a word (nextWord) to be considered confirmed, there are a few requirements:
			// 1. The word must be found in one of the next nodes of the current confirmed node;
			// 2. It can't be the last word of the input OR the last char of the input must be a space.
			while (slicer.HasNextSlice()) {
				nextWord = slicer.NextSlice();
				// if found next node AND (there's a next word OR its the last word but it ends with space)
				if (!confirmed.FindNext( nextWord ) || !(slicer.HasNextSlice() || inp_cmd.EndsWith( ' ' ))) break;
				confirmed = confirmed.next;
				nextWord = PString.Empty;
			}

			// When the confirmed words step is over, if the nextWord is not null, it means the input was not
			// fully confirmed.
			if (!nextWord.IsNullOrEmpty || confirmed.edges.Count > 0) {
				_currentPrediction = confirmed.edges.Where( n => n.value.StartsWith( nextWord ) ).OrderBy( n => n.value ).ToList();
				if (_currentPrediction.IsNullOrEmpty()) return "Command not found";
				Index = -1;
				var display = _currentPrediction.Aggregate( string.Empty, ( a, c ) => a += c.value + "     " );
				return display;
			}
			else return "now it's time for parameter assistance";
		}




		public string GetNextPredictionEntry() {
			Index++;
			if (Index >= _currentPrediction.Count) Index = -1;
			string output = _confirmedNodes.Aggregate( string.Empty, ( agg, c ) => agg += c.value + ' ' );
			if (Index > -1) output += _currentPrediction[Index].value;
			return output;
		}




		public string GetConfirmedNodes( string input ) {
			var inp = (PString)input;
			var slicer = inp.GetSlicer( ':' );
			var inp_cmd = slicer.NextSlice();
			var inp_args = slicer.NextSlice();
			var sb = new StringBuilder();
			var node = _startNode;
			while( (node = node.next) != null )
				sb.Append( node.value + ' ' );
			return sb.ToString();
		}






		//[DebuggerDisplay( "node: {value}  |  next: {next?.value}  |  edges: {edges.Count}" )]
		[DebuggerDisplay( "{Preview(),nq}" )]
		public class Node {
			[DebuggerDisplay( "{edges.Count} edges" )]
			public KeyedSet<string, Node> edges = new KeyedSet<string, Node>( ( n ) => n.value );
			public Node prev;
			public Node next;
			public string value;
			public bool FindNext( PString value ) {
				if (next != null && next.value == value) return true;
				next = edges.FirstOrDefault( n => n.value == value );
				return next != null;
			}

			public Node GetNext( PString value ) {
				if (next != null && next.value == value) return next;
				return next = edges.FirstOrDefault( n => n.value == value );
			}


			internal Node AddIfNotPresent( string word ) {
				var node = edges.FirstOrDefault( n => n.value == word );
				if (node is null) {
					node = new Node() { prev = this, value = word };
					edges.Add( node );
				}
				return node;
			}

			public override string ToString() {
				return value;
			}

			public string Preview() {
				if (value == "@") {
					Node n = this;
					string r = value;
					while ((n = n.next) != null) r += "  →  " + n;
					return r;
				}
				else return $"\"{value}\"";
			}
		}
	}
}
