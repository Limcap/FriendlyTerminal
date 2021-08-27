using Limcap.Dux;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.FTerminal {
	public abstract partial class ACommand : ICommand {

		public const string NEW_LINE = Terminal.NEW_LINE;

		//public const string DEFAULT_LOCALE = "enus";
		//public const string INVOKE_STRING = null;
		//public const int REQUIRED_PRIVILEGE = 0;

		public ACommand( string locale ) {
			Locale = locale;
			_translator = Translator.LoadTranslator( GetType(), locale );
			//if (locale != null && locale != GetType().GetConst( "DEFAULT_LOCALE" ) as string) {
			//	if (DefaultTextSource != null) {
			//		var keys = DefaultTextSource.Keys.ToList();
			//		for (var i = 0; i < DefaultTextSource.Keys.Count; i++) {
			//			var key = keys[i];
			//			DefaultTextSource[key] = _translator.Translate( key ) ?? DefaultTextSource[key];
			//		}
			//	}
			//	Info = InfoBuilder().Translate( _translator );
			//}
		}



		#region INFORMATION
		#endregion
		private Parameter[] _parameters;
		protected private abstract Parameter[] ParametersBuilder();
		public Parameter[] Parameters { get { _parameters = _parameters ?? ParametersBuilder(); return _parameters; } }


		private string _description;
		protected private abstract string DescriptionBuilder();
		public string Description { get { _description = _description ?? DescriptionBuilder(); return _description; } }


		public string Info {
			get {
				var t = Translator.LoadTranslator( typeof(Parameter), Locale );
				var word_description = t.Translate( "DESCRIPTION" );
				var word_parameters = t.Translate( "PARAMETERS" );
				var word_optional = t.Translate( "OPTIONAL" );
				var word_type = t.Translate( "TYPE" );
				string output = $"\n{word_description}\n{Description}\n\n";
				if (Parameters != null && Parameters.Length > 0) {
					output += word_parameters + "\n";
					foreach (var param in Parameters)
						output += $"{param.name}{(param.optional ? $" ({word_optional}) " : " ")}: {param.description} - {word_type}: {param.type};\n";
				}
				return output;
			}
		}




		#region TRANSLATION
		#endregion
		private Translator _translator;
		public string Locale { get; private set; }
		protected virtual TextSource DefaultTextSource { get; set; }
		protected string Txt( string key, string fallback = null ) {
			if (_translator is null) _translator = Translator.LoadTranslator( GetType(), Locale );
			string txt;
			//if( _defaultLocale is null || _defaultLocale == Locale && DefaultTextLibrary != null )
			//	txt = DefaultTextLibrary[key];
			txt = _translator.TranslateOrNull( key )
				?? (DefaultTextSource.ContainsKey( key ) ? DefaultTextSource[key] : null)
				?? fallback
				?? $"[missing_text:{key}]";
			return txt;
		}




		#region EXECUTION
		#endregion
		public abstract string MainFunction( Terminal t, Arg[] args );




		#region SUB-TYPES
		#endregion
		public class TextSource : Dictionary<string, string> { }

		//public static Translator commonTranslator = Translator.LoadTranslator(typeof(ACommand),)
	}
}