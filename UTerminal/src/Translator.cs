
using Limcap.Dux;
using System.Collections.Generic;

namespace Limcap.UTerminal {
	public class Translator {
		public Translator( Dux.DuxNamedList translationDux, string locale = null ) : base() {
			CurrentLocale = locale;
			_t = translationDux;
		}
		public string CurrentLocale { get; set; }
		private Dux.DuxNamedList _t;
		public string Translate( Tstring tstrg ) => Translate( CurrentLocale, tstrg );
		public string Translate( string locale, Tstring tstrg ) => _t?[tstrg.id][locale].AsString(null) ?? tstrg.str;
	}








	//public class Translator2 : Dictionary<string, Dictionary<string, string>> {
	//	public Translator2( string locale = null ) : base() { CurrentLocale = locale; }
	//	public string Translate( string locale, Tstring tstrg ) {
	//		return (ContainsKey( tstrg.id ) && this[tstrg.id].ContainsKey( locale )) ? this[tstrg.id][locale] : tstrg.str;
	//	}
	//	public string Translate( Tstring tstrg ) {
	//		return Translate( CurrentLocale, tstrg );
	//	}
	//	public string CurrentLocale { get; set; }
	//}








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
