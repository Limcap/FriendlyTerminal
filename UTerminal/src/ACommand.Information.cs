namespace Limcap.UTerminal {
	public abstract partial class ACommand {
		public struct Information {
			public readonly Tstring description;
			public readonly Parameter[] parameters;

			//public const string invokeString = "help;PT_BR:ajuda";
			//public readonly int requiredPrivilege;
			//public Func<string, string> main;

			public Information( Tstring description, params Parameter[] parameters ) {
				this.description = description;
				this.parameters = parameters;
			}

			public override string ToString() {
				string output = "\nDESCRIPTION\n" + description + "\n\n";
				if (parameters.Length > 0) {
					output += "PARAMETERS\n";
					foreach (var param in parameters)
						output += $"{param.name}{(param.optional ? " (optional) " : " ")}- type: {param.type}; {param.description}\n";
				}
				return output;
			}
			public string ToString( Translator t ) {
				string output = "\nDESCRIPTION\n" + t.Translate( description ) + "\n\n";
				if (parameters.Length > 0) {
					output += "PARAMETERS\n";
					foreach (var param in parameters)
						output += $"{param.name}{(param.optional ? " (optional) " : " ")}- type: {param.type}; {t.Translate( param.description )}\n";
				}
				return output;
			}
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
