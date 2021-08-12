using System;
using Stan = System.ReadOnlySpan<char>;

namespace Limcap.UTerminal {
	public unsafe ref struct ChanHoloArray3 {
		private char _gapStart;
		private char _gapEnd;
		private char* _txtPtr;
		private int _txtLen;
		private Range* _rangesPtr;
		private int _rangesLen;

		public int Length => _rangesLen;

#if DEBUG
		private Span<Range> Range => new Span<Range>( _rangesPtr, _rangesLen );

		private Stan Text => new Stan( _txtPtr, _txtLen );
#endif

		public string[] AsArray {
			get {
				var arr = new string[_rangesLen];
				for (int i = 0; i < _rangesLen; i++) arr[i] = this[i].ToString();
				return arr;
			}
		}

		public override string ToString() => string.Join( "; ", AsArray );

		public Stan this[int i] => new Stan( &_txtPtr[_rangesPtr[i].start], _rangesPtr[i].length );
		//public Span<char> this[int i] => fulltext.Slice( ranges[i].start, ranges[i].length );


		public unsafe void SetupLength_1st( ref Stan sourceText, char gapStart = '=', char gapEnd = ',' ) {
			_gapStart = gapStart;
			_gapEnd = gapEnd;
			fixed (char* ptr = &sourceText.GetPinnableReference()) { _txtPtr = ptr; }
			_txtLen = sourceText.Length;
			_rangesLen = sourceText.Length > 0 ? 1 : 0;
			foreach (var c in sourceText) if (c == _gapEnd) _rangesLen++;
		}


		public unsafe void SetupRanges_2nd( Range* rangesPtr ) {
			_rangesPtr = rangesPtr;
			bool isAtRangeBegining = true;
			int l = 0;
			for (int i = 0; i < _txtLen; i++) {
				if (isAtRangeBegining) {
					while (i < _txtLen && _txtPtr[i] == ' ') i++;
					if (i >= _txtLen) break;
					_rangesPtr[l].start = i;
					isAtRangeBegining = false;
				}
				var c = _txtPtr[i];
				if (c == _gapStart) {
					_rangesPtr[l].length = i - _rangesPtr[l].start; //(i-1+1)-(range.start) 
					l++;
				}
				else if (c == _gapEnd) {
					isAtRangeBegining = true;
					_rangesPtr[l].start = i == _txtLen - 1 ? 0 : i + 1;
				}
			}
			var lastRange = _rangesPtr[_rangesLen - 1];
			if (lastRange.start > 0) {
				for (int i = _txtLen - 1; i >= 0; i--) {
					if (_txtPtr[i] == ' ') continue;
					_rangesPtr[_rangesLen - 1].length = i + 1 - _rangesPtr[_rangesLen - 1].start;
					break;
				}
			}
		}
	}
}
