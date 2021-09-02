using System;
using Stan = System.ReadOnlySpan<char>;

namespace Limcap.FTerminal {
	public unsafe struct SlicedChan {
		private char _sliceChar;
		private char* _txtPtr;
		private int _txtLen;
		private Range* _rangesPtr;
		private int _rangesLen;

		public int NumberOfSlices => _rangesLen;

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




		public unsafe void ProspectSlices( ref Stan sourceText, char sliceChar ) {
			_sliceChar = sliceChar;
			fixed (char* ptr = &sourceText.GetPinnableReference()) { _txtPtr = ptr; }
			_txtLen = sourceText.Length;
			_rangesLen = sourceText.Length > 0 ? 1 : 0;
			foreach (var c in sourceText) if (c == _sliceChar) _rangesLen++;
		}




		public unsafe void SetIndexMemory( Range* rangesPtr ) {
			_rangesPtr = rangesPtr;
		}




		public unsafe void Slice() {
			int rangeIdx = 0;
			int lastSliceCharIdx = -1;
			var currentRange = _rangesPtr[0];
			currentRange.start = 0;
			// find start, discarding spaces
			for (int i = 0; i < _txtLen; i++) {
				if (_txtPtr[i] != _sliceChar)
					continue;
				else {
					_rangesPtr[rangeIdx].length = i - _rangesPtr[rangeIdx].start;
					rangeIdx++;
					if (rangeIdx <= _rangesLen)
						_rangesPtr[rangeIdx].start = i + 1;
				}
			}
			_rangesPtr[rangeIdx].length = _txtLen - _rangesPtr[rangeIdx].start;
		}




		public void Trim( char c = ' ') {
			TrimStart();
			TrimEnd();
		}




		public void TrimStart( char c = ' ' ) {
			for (int r = 0; r < _rangesLen; r++) {
				while (_txtPtr[_rangesPtr[r].start] == c) {
					_rangesPtr[r].start++;
					_rangesPtr[r].length--;
				}
			}
		}




		public void TrimEnd( char c = ' ' ) {
			for (int r = 0; r < _rangesLen; r++) {
				if (_rangesPtr[r].length == 0) continue;
				int i; char cc;
				for(; ;) {
					i = _rangesPtr[r].start + _rangesPtr[r].length - 1;
					cc = _txtPtr[i];
					if (cc == c) _rangesPtr[r].length--;
					else break;
				}
				//while (_txtPtr[_rangesPtr[r].length - _rangesPtr[r].start] == c) _rangesPtr[r].length--;
			}
		}




		public unsafe void FindRange( Stan text, int textStartIndex, char splitterStartChar, char splitterChar, Range* rangePtr ) {
			var rangeToFill = rangePtr[0];

			// find start, discarding spaces
			rangeToFill.start = textStartIndex;
			int i = 0;
			while (i < text.Length && text[i] == ' ') i++;
			rangeToFill.start = i;

			// escpecial cases of length 0
			if (i == text.Length || text[i] == splitterChar) {
				rangeToFill.length = 0;
				return;
			}
			// find end, discarding spaces
			while (i < text.Length && text[i] != splitterChar) i++;
			i--;
			while (i > rangeToFill.start && text[i] == ' ') i--;
			rangeToFill.length = i + 1 - rangeToFill.start;

			return;
		}





		//public SlicedChan( char* textPtr, int textLen, Range* _rangeArrayPtr = null ) {
		//	_txtPtr = textPtr;
		//	_txtLen = textLen;
		//}
	}








	public static partial class Ext {
		public static bool Contains( ref this SlicedChan schan, string element ) {
			for (int i = 0; i < schan.NumberOfSlices; i++) {
				var c = schan[i];
				if (c.EqualsString( element )) return true;
			}
			return false;
		}


	}
}
