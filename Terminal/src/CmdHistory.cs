using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.TextboxTerminal {
	public class CmdHistory {
		private readonly List<string> _cmdHistory = new List<string>();
		public int Index { get; private set; } = int.MaxValue - 1;
		public string TempText { get; set; }

		public void Add( string value ) {
			if (_cmdHistory.Count > 0 && value != _cmdHistory[_cmdHistory.Count-1] || _cmdHistory.Count == 0)
				_cmdHistory.Add( value );
			Reset();
		}

		public string Prev() {
			if (_cmdHistory.Count == 0) return string.Empty;
			Index = (Index - 1).MinMax( 0, _cmdHistory.Count - 1 ); //Math.Min( Math.Max( Index - 1, -1 ), _cmdHistory.Count );
																					  //return Index < 0 ? string.Empty : _cmdHistory[Index];
			return _cmdHistory[Index];
		}

		public string Down() {
			if (_cmdHistory.Count == 0) return string.Empty;
			Index = (Index + 1).MinMax( 0, _cmdHistory.Count );// Math.Max( Index + 1, _cmdHistory.Count - 1 );
			return Index >= _cmdHistory.Count ? string.Empty : _cmdHistory[Index];
		}

		public void Reset() {
			Index = int.MaxValue - 1;
			TempText = null;
		}

	}
}
