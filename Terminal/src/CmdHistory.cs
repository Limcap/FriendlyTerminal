using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.TextboxTerminal {
	public class CmdHistory {
		private readonly List<string> _history = new List<string>();

		private int _index = -1;

		public int Index {
			get => _index;
			private set => _index = _history.Count == 0 ? -1 : value.MinMax( MinIndex - 1, MaxIndex );
		}

		public int InversedIndex => Index == -1 ? -1 : _history.Count - (Index + 1);

		private int MaxIndex => _history.Count == 0 ? 0 : _history.Count - 1;

		private int MinIndex => 0;

		public bool IsSelected => Index > -1;

		public string Current() {
			return Index == -1 ? string.Empty : _history[InversedIndex];
		}

		public string Prev() {
			Index++;
			return Current();
		}

		public string Next() {
			Index--;
			return Current();
		}

		public void Deselect() {
			Index = -1;
		}

		public void Reset() {
			_history.Clear();
		}

		public void Add( string value ) {
			if (_history.Count == 0 || value != _history[MaxIndex])
				_history.Add( value );
			Deselect();
		}
	}
}
