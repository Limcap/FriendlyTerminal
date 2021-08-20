using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.DataStructures {
	[Serializable]
	[DebuggerDisplay( "{Preview(), nq}" )]
	public class KeyedSet<K,V> : IEnumerable<V> {

		public KeyedSet( Func<V, K> keyDefinition ) {
			_keyDef = keyDefinition;
			_dict = new Dictionary<K, V>();
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private readonly Func<V, K> _keyDef;

		[DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
		private readonly Dictionary<K, V> _dict;

		public int Count => _dict.Count;
		//public V Current => throw new NotImplementedException();

		public void Add( V value ) {
			_dict.Add( _keyDef( value ), value );
		}

		public void Update( V value ) {
			var key = _keyDef( value );
			if (_dict.ContainsKey( key ))
				_dict[key] = value;
			else
				Add( value );
		}

		public IEnumerator<V> GetEnumerator() {
			return _dict.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return _dict.Values.GetEnumerator();
		}


		public V Get( V value ) {
			var key = _keyDef( value );
			if (_dict.ContainsKey( key )) return _dict[key];
			else return default( V );
		}


		public V Get( K key ) {
			if (_dict.ContainsKey( key )) return _dict[key];
			else return default( V );
		}


		public bool HasValue( V value ) {
			var key = _keyDef( value );
			return _dict.ContainsKey( key );
		}


		public bool HasKey( K key ) {
			return _dict.ContainsKey( key );
		}


		private string Preview() {
			return "Dictionary";
		}

		////private enumerator class
		//private class Enumerator<V> : IEnumerator {
		//	public Dictionary<K, V> _dict;
		//	int position = -1;
		//	IEnumerator enumerator;

		//	//constructor
		//	public Enumerator( Dictionary<K, V> dict ) {
		//		_dict = dict;
		//		_dict.Values.GetEnumerator();
		//	}
		//	private IEnumerator GetEnumerator() {
		//		return (IEnumerator)this;
		//	}
		//	//IEnumerator
		//	public bool MoveNext() {
		//		return _dict.GetEnumerator().MoveNext();
		//	}

		//	//IEnumerator
		//	public object Current {
		//		get {
		//			try {
		//				return _dict
		//			}
		//			catch (IndexOutOfRangeException) {
		//				throw new InvalidOperationException();
		//			}
		//		}
		//	}
		//}  //end nested class
	}
}
