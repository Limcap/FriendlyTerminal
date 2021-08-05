namespace Limcap.UTerminal {
	public struct Tstring {
		public Tstring( string idstr ) {
			var separatorIndex = idstr.IndexOf( "#" );
			id = (separatorIndex == -1) ? null : idstr.Remove( separatorIndex );
			str = (separatorIndex == -1) ? idstr : idstr.Substring( separatorIndex + 1 );
		}
		public readonly string id;
		public readonly string str;
		public static implicit operator string( Tstring idstr ) => idstr.str;
		public static implicit operator Tstring( string idstr ) => new Tstring( idstr );
		public override string ToString() => this;
		public string Translate( string locale, Translator t ) => t.Translate( locale, this );
		public string Translate( Translator t ) => t.Translate( this );
	}









	//public struct LText {
	//	public LText( params LString[] entries ) {
	//		this.entries = entries;
	//	}
	//	public readonly LString[] entries;
	//}

	//public struct LString {
	//	public LString( string locale, string text ) {
	//		this.locale = locale;
	//		this.text = text;
	//	}
	//	public readonly string locale;
	//	public readonly string text;
	//}

	//public struct LS {
	//	private readonly Entry[] entries;
	//	public LS( params string[] texts ) {
	//		entries = new Entry[texts.Length];
	//		for (int i = 0; i < texts.Length; i++) {
	//			string text = texts[i];
	//			var separatorIndex = text.IndexOf( "#" );
	//			if (separatorIndex == -1) entries[i].text = text;
	//			entries[i].locale = text.Remove( separatorIndex );
	//			entries[i].text = text.Substring( separatorIndex + 1 );
	//		}
	//	}
	//	private struct Entry {
	//		public string locale;
	//		public string text;
	//	}
	//}
}
