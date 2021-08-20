using Limcap.DataStructures;
using System;
using System.Diagnostics;
using System.Linq;

namespace Limcap.UTerminal {
	public partial class Assistant {
		//[DebuggerDisplay( "node: {value}  |  next: {next?.value}  |  edges: {edges.Count}" )]
		[Serializable]
		[DebuggerDisplay( "{Preview(),nq}" )]
		public class Node {
			[DebuggerDisplay( "{edges.Count} edges" )]
			public readonly KeyedSet<string, Node> edges = new KeyedSet<string, Node>( ( n ) => n.word );
			public Node prev;
			public Node next;
			public string word;
			public Type cmdType;


			public bool IsLeafNode => edges.Count == 0;


			public bool FindNext( PString word ) {
				return (next = GetNext( word )) != null;
			}


			public Node GetNext( PString word ) {
				if (next != null && next.word == word) return next;
				return edges.FirstOrDefault( n => n.word == word );
			}


			internal Node AddIfNotPresent( string word, Type cmd = null ) {
				var node = edges.FirstOrDefault( n => n.word == word );
				if (node is null) {
					node = new Node() { prev = this, word = word, cmdType = cmd };
					edges.Add( node );
				}
				return node;
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
		}








		public class Node<T> {
			[DebuggerDisplay( "{edges.Count} edges" )]
			public KeyedSet<string, Node<T>> edges = new KeyedSet<string, Node<T>>( ( n ) => n.value );
			public Node<T> prev;
			public Node<T> next;
			public string value;
			public T value2;
			public bool FindNext( PString value ) {
				if (next != null && next.value == value) return true;
				next = edges.FirstOrDefault( n => n.value == value );
				return next != null;
			}

			public Node<T> GetNext( PString value ) {
				if (next != null && next.value == value) return next;
				return next = edges.FirstOrDefault( n => n.value == value );
			}


			internal Node<T> AddIfNotPresent( string word, T value2 = default(T) ) {
				var node = edges.FirstOrDefault( n => n.value == word );
				if (node is null) {
					node = new Node<T>() { prev = this, value = word, value2 = value2 };
					edges.Add( node );
				}
				return node;
			}

			public override string ToString() {
				return value;
			}

			public string Preview() {
				if (value == "@") {
					Node<T> n = this;
					string r = value;
					while ((n = n.next) != null) r += "  →  " + n;
					return r;
				}
				else return $"\"{value}\"";
			}
		}
	}
}
