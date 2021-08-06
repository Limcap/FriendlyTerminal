using System;

namespace Limcap.UTerminal {
	public abstract partial class ACommand {
		public struct Information {

			public Tstring description;
			public Parameter[] parameters;
			private Words words;


			public Information( Tstring description, params Parameter[] parameters ) {
				this.description = description;
				this.parameters = parameters;
				words = new Words() {
					Description = "1#DESCRIPTION",
					Parameters = "2#PARAMETERS",
					Optional = "3#optional",
					Type = "4#type"
				};
			}


			public override string ToString() {
				string output = $"\n{words.Description}\n{description}\n\n";
				if (parameters.Length > 0) {
					output += words.Parameters + "\n";
					foreach (var param in parameters)
						output += $"{param.name}{(param.optional ? $" ({words.Optional}) " : " ")}- {words.Type}: {param.type}; {param.description}\n";
				}
				return output;
			}


			//internal string TranslateInfo( Translator cmdTranslator ) {
			//	var ut = cmdTranslator;
			//	var wt = new Words( cmdTranslator.CurrentLocale );
			//	string output = $"\n{wt.Description}\n{ut.Translate( description )}\n\n";
			//	if (parameters.Length > 0) {
			//		output += "PARAMETERS\n";
			//		foreach (var param in parameters)
			//			output += $"{param.name}{(param.optional ? " (optional) " : " ")}- type: {param.type}; {t.Translate( param.description )}\n";
			//	}
			//	return output;
			//}


			public Information Translate( Translator t ) {
				// Traduz os termos específicos
				description = t.Translate( description );
				for (int i = 0; i < parameters.Length; i++) {
					parameters[i].name = t.Translate( parameters[i].name );
					parameters[i].description = t.Translate( parameters[i].description );
				}
				// Traduz os termos genéricos.
				//words = words.Translate( t.CurrentLocale );
				var wt = Translator.LoadTranslator( typeof( Information ), t.CurrentLocale );
				words.Description = wt.Translate( words.Description );
				words.Parameters = wt.Translate( words.Parameters );
				words.Optional = wt.Translate( words.Optional );
				words.Type = wt.Translate( words.Type );
				return this;
			}


			private struct Words {
				// C# não permite structs com construtores vazios, por causa de umas coisas técnincas. mas nao tem
				// problema, colocamos um parametro não-usado só para poder usar o construtor.
				//public Words(string none=null) {
				//	Description = "1#DESCRIPTION";
				//	Parameters = "2#PARAMETERS";
				//	Optional = "2#optional";
				//	Type = "4#type";
				//}
				
				public Tstring Description;
				public Tstring Parameters;
				public Tstring Optional;
				public Tstring Type;

				//internal Words Translate( string locale ) {
				//	var t = Translator.LoadTranslator( typeof( Information ), locale );
				//	Description = t.Translate( Description );
				//	Parameters = t.Translate( Parameters );
				//	Optional = t.Translate( Optional );
				//	Type = t.Translate( Type );
				//	return this;
				//}
			}
			//internal class TitleTranslator {
			//	internal TitleTranslator( string locale ) {
			//		translator = Translator.LoadTranslator( typeof( Information ), locale );
			//	}
			//	internal Translator translator;
			//	internal string Description => translator.Translate( "1#DESCRIPTION" );
			//	internal string Parameters => translator.Translate( "2#PARAMETERS" );
			//	internal string Optional => translator.Translate( "2#optional" );
			//	internal string Type => translator.Translate( "4#type" );
			//}
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
