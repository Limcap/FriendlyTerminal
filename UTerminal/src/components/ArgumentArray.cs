using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.UTerminal {
	public unsafe ref partial struct InputSolver {
		public unsafe struct ArgumentArray {
			private PtrArray<Arg> _rawArr;

			//private Argument[] Ee {
			//	get {
			//		var aarr = new Argument[_rawArr.len];
			//		for (var i = 0; i < _rawArr.len; i++) aarr[i] = ((Argument*)_rawArr.ptr)[i];
			//		return aarr;
			//	}
			//}

			public ArgumentArray( void* ptr, int len ) {
				_rawArr.ptr = ptr;
				_rawArr.len = len;
			}

			public Arg[] Elements => _rawArr.AsArray;

			public Arg this[int i] {
				get => ((Arg*)_rawArr.ptr)[i];
				set => ((Arg*)_rawArr.ptr)[i] = value;
			}
		}
	}
}
