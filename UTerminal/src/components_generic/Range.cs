using System;

namespace Limcap.UTerminal {
	public struct Range {
		public int start { get; set; }
		public int length { get; set; }
	}

	public ref struct MySpan<T> {
		public int start { get; set; }
		public int length { get; set; }

		public Span<T> s;

	}
}
