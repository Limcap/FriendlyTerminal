namespace Limcap.UTerminal {
	public abstract partial class ACommand {
		public struct Parameter {
			public Parameter( string name, Type type, bool optional, Tstring description ) {
				this.name = name;
				this.type = type;
				this.optional = optional;
				this.description = description;
			}
			public string name;
			public Type type;
			public bool optional;
			public Tstring description;








			public enum Type { ANY, ALPHANUMERIC, LETTERS, CHARACTER, NUMBER, INTEGER, DATE, TIME, DATETIME, EMAIL }
		}
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
