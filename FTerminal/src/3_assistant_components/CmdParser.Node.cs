using Limcap.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Limcap.FTerminal {
	public partial class CmdParser {

		[Serializable]
		[DebuggerDisplay( "{Preview(),nq}" )]

		public class Node {

			[DebuggerDisplay( "{edges.Count} edges" )]
			public readonly KeyedSet<string, Node> edges = new KeyedSet<string, Node>( ( n ) => n.word );

			public Node prev;
			public Node next;
			public string word;
			public Type value;








			public bool IsLeafNode => edges.Count == 0;








			public bool FindNext( PString word ) {
				return (next = GetNext( word )) != null;
			}








			public Node GetNext( PString word ) {
				if (next != null && next.word == word) return next;
				return edges.FirstOrDefault( n => n.word == word );
			}








			public Node AddIfNotPresent( string word, Type cmd = null ) {
				var node = edges.FirstOrDefault( n => n.word == word );
				if (node is null) {
					node = new Node() { prev = this, word = word, value = cmd };
					edges.Add( node );
				}
				return node;
			}








			public unsafe Node Traverse( ref PString str ) {
				Node best = this;
				Node candidate = this;
				while (!str.IsEmpty && best.next != null && str.StartsWith( best.next.word )) {
					str.ShiftStart( best.next.word.Length );
					best = best.next;
				}
				while (!(str.IsEmpty || candidate is null)) {
					candidate = null;
					//foreach (var node in best.edges) {
					for (int i = 0; i < best.edges.Count; i++) {
						var node = best.edges[i];
						if (str.len < node.word.Length) continue;
						if (str.StartsWith( node.word )) {
							candidate = node;
							str.ShiftStart( node.word.Length );
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
				return word;
			}








			public string Preview() {
				if (word == "@") {
					Node n = this;
					string r = word;
					while ((n = n.next) != null) r += "  →  " + n;
					return r;
				}
				else return $"\"{word}\"";
			}








			public unsafe static void BuildGraph( Dictionary<string, Type> sentences, char separator, string terminator, Node start ) {
				var _invokeStrings = sentences?.ToList().OrderBy( c => c.Key ).ToList() ?? new List<KeyValuePair<string, Type>>();
				foreach (var term in sentences) {
					var words = ((PString)term.Key).GetSlicer( separator, PString.Slicer.Mode.IncludeSeparatorAtEnd );
					var node = start;

					while (words.HasNext) {
						var word = words.Next();
						if (word.len == 0) continue;
						node = node.AddIfNotPresent( new string( word.ptr, 0, word.len ) );
						if (!words.HasNext) node.AddIfNotPresent( terminator, term.Value );
					}
				}
			}








			public static void BuildGraph2( List<string> sentences, char separator, string terminator, Node start ) {
				foreach (var term in sentences) {
					var words = term.Split( separator );
					var node = start;
					for (int i = 0; i < words.Length; i++) {
						// skips words that have length 0, cause when there are multiple spaces between the words.
						if (words[i].Length == 0) continue;

						if (i < words.Length - 1) {
							node = node.AddIfNotPresent( words[i] + separator );
						}
						else {
							node = node.AddIfNotPresent( words[i] );
							node.AddIfNotPresent( terminator );
						}
					}
				}
			}
		}
	}
}
