using Limcap.Duxtools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.FriendlyTerminal {
	public abstract class ACommand : ICommand {

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
		protected abstract Parameter[] ParametersBuilder();
		public Parameter[] Parameters { get { _parameters = _parameters ?? ParametersBuilder(); return _parameters; } }


		private string _description;
		protected abstract string DescriptionBuilder();
		public string Description { get { _description = _description ?? DescriptionBuilder(); return _description; } }


		public string Info {
			get {
				string bullet = "● ";//▪
				var t = Translator.LoadTranslator( typeof(Parameter), Locale );
				var title = t.Translate( "TITLE" );
				var word_description = t.Translate( "DESCRIPTION" );
				var word_parameters = t.Translate( "PARAMETERS" );
				var word_optional = t.Translate( "OPTIONAL" );
				var word_type = t.Translate( "TYPE" );
				var lines = new List<string>( Parameters?.Length??0 );
				string output = $"{title}\n\n═══════════════ {word_description} ═══════════════\n{Description}";
				if (Parameters != null && Parameters.Length > 0) {
					output += $"\n\n═══════════════ {word_parameters} ═══════════════";
					foreach (var param in Parameters) {
						var longDesc = param.longDescription != null ? $"\n{param.longDescription}" : "";
						lines.Add( $"\n{bullet}{param.name} ({(param.optional ? $"{word_optional}; " : "")}{word_type}: {param.type})\n{param.shortDescription}{longDesc}" );
					}
					output += string.Join( NEW_LINE, lines );
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
				?? (DefaultTextSource is null
					? null : ( DefaultTextSource.ContainsKey( key )
						? DefaultTextSource[key] : null))
				?? fallback
				?? $"[missing_text:{key}]";
			return txt;
		}




		public string InvokeText {
			get {
				var invokeText = GetType().GetConst( "INVOKE_TEXT" ) as string;
				var translated = Txt( "INVOKE_TEXT", invokeText );
				return translated;
			}
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