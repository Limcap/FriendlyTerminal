using System;

namespace Limcap.UTerminal {
	public abstract partial class ACommand {
		public struct Information {

			public Tstring description;
			public Parameter[] parameters;
			private Words _words;

			public Information( string description, params Parameter[] parameters ) {
				this.description = description;
				this.parameters = parameters;
				_words = new Words() {
					Description = "1#DESCRIPTION",
					Parameters = "2#PARAMETERS",
					Optional = "3#optional",
					Type = "4#type"
				};
			}


			//public override string InformationBuilder() {
			//	string output = $"\n{words.Description}\n{description}\n\n";
			//	if (parameters.Length > 0) {
			//		output += words.Parameters + "\n";
			//		foreach (var param in parameters)
			//			output += $"{param.name}{(param.optional ? $" ({words.Optional}) " : " ")}: {param.description} - {words.Type}: {param.type};\n";
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
				var wt = Translator.LoadTranslator( typeof( Information ), t.CurrentLocale );
				_words.Description = wt.Translate( _words.Description );
				_words.Parameters = wt.Translate( _words.Parameters );
				_words.Optional = wt.Translate( _words.Optional );
				_words.Type = wt.Translate( _words.Type );
				return this;
			}


			private struct Words {				
				public Tstring Description;
				public Tstring Parameters;
				public Tstring Optional;
				public Tstring Type;
			}
		}
	}
}
