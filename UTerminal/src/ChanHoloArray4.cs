using System;
using Stan = System.ReadOnlySpan<char>;

namespace Limcap.UTerminal {
	public unsafe struct SlicedChan {
		private char _sliceChar;
		private char _gapStart;
		private char _gapEnd;
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
			_sliceChar = ',';
			SetArrayMemory( rangesPtr );
			Slice();
			FindRange( new Stan( _txtPtr, _txtLen ), 0, '=', ',', rangesPtr );
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



		public unsafe void FindNumberOfSlices( ref Stan sourceText, char sliceChar ) {
			_gapStart = default;
			_gapEnd = default;
			_sliceChar = sliceChar;
			fixed (char* ptr = &sourceText.GetPinnableReference()) { _txtPtr = ptr; }
			_txtLen = sourceText.Length;
			_rangesLen = sourceText.Length > 0 ? 1 : 0;
			foreach (var c in sourceText) if (c == _sliceChar) _rangesLen++;
		}

		public unsafe void SetArrayMemory( Range* rangesPtr ) {
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
			_rangesPtr[rangeIdx].length = _txtLen - 1 - _rangesPtr[rangeIdx].start;
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

		public ref struct SlicedParamInput {
			public SlicedParamInput( ref Stan name, ref Stan value ) {
				this.name = name;
				this.value = value;
			}
			public Stan name;
			public Stan value;
		}







		//public SlicedChan( char* textPtr, int textLen, Range* _rangeArrayPtr = null ) {
		//	_txtPtr = textPtr;
		//	_txtLen = textLen;
		//}
	}
}
