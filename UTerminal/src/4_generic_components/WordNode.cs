using Limcap.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Limcap.UTerminal {

	[Serializable]
	[DebuggerDisplay( "{Preview(),nq}" )]
	public class WordNode<V> {

		[DebuggerDisplay( "{edges.Count} edges" )]
		public readonly KeyedSet<PString, WordNode<V>> edges = new KeyedSet<PString, WordNode<V>>( ( n ) => n.word );

		public WordNode<V> prev;
		public WordNode<V> next;
		public PString word;
		public V value;








		public bool IsLeafNode => edges.Count == 0;








		public bool FindNext( PString word ) {
			return (next = GetNext( word )) != null;
		}








		public WordNode<V> GetNext( PString word ) {
			if (next != null && next.word.Equals(word)) return next;
			return edges.FirstOrDefault( n => n.word.Equals(word) );
		}








		public WordNode<V> AddIfNotPresent( PString word, V val = default(V) ) {
			var node = edges.FirstOrDefault( n => n.word.Equals( word) );
			if (node is null) {
				node = new WordNode<V>() { prev = this, word = word, value = val };
				edges.Add( node );
			}
			return node;
		}








		public WordNode<V> Traverse( ref PString str ) {
			WordNode<V> best = this;
			WordNode<V> candidate = this;
			while (!str.IsEmpty && best.next != null && str.StartsWith( best.next.word )) {
				str.ShiftStart( best.next.word.len );
				best = best.next;
			}
			while (!(str.IsEmpty || candidate is null)) {
				candidate = null;
				//foreach (var node in best.edges) {
				for( int i=0; i<best.edges.Count; i++ ) {
					var node = best.edges[i];
					if (str.len < node.word.len) continue;
					if (str.StartsWith( node.word )) {
						candidate = node;
						str.ShiftStart( node.word.len );
						break;
					}
				}
				best.next = candidate;
				best = candidate ?? best;
			}
			// This is necessary because the last selected node for the 'best' spot will have next when
			// backspacing from the terrminator
			best.next = null;
			return best;
		}








		public override string ToString() {
			return word.AsString;
		}








		public string Preview() {
			if (word == "@") {
				WordNode<V> n = this;
				string r = word.AsString;
				while ((n = n.next) != null) r += "  →  " + n;
				return r;
			}
			else return $"\"{word}\"";
		}








		public unsafe static void BuildGraph( Dictionary<string, V> sentences, char separator, string terminator, WordNode<V> start ) {
			var _invokeStrings = sentences?.ToList().OrderBy( c => c.Key ).ToList() ?? new List<KeyValuePair<string, V>>();
			foreach (var term in sentences) {
				var keys = ((PString)term.Key).GetSlicer( separator, PString.Slicer.Mode.IncludeSeparatorAtEnd );
				var node = start;

				while (keys.HasNext) {
					var word = keys.Next();
					if (word.len == 0) continue;
					node = node.AddIfNotPresent( new string( word.ptr, 0, word.len ) );
					if (!keys.HasNext) node.AddIfNotPresent( terminator, term.Value );
				}
			}
		}








		public static void BuildGraph2( List<string> sentences, char separator, string terminator, WordNode<V> start ) {
			foreach (var term in sentences) {
				var keys = term.Split( separator );
				var node = start;
				for (int i = 0; i < keys.Length; i++) {
					// skips words that have length 0, cause when there are multiple spaces between the words.
					if (keys[i].Length == 0) continue;

					if (i < keys.Length - 1) {
						node = node.AddIfNotPresent( keys[i] + separator );
					}
					else {
						node = node.AddIfNotPresent( keys[i] );
						node.AddIfNotPresent( terminator );
					}
				}
			}
		}
	}

}
