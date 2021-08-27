
using Limcap.Dux;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Limcap.FTerminal {
	public class Translator {

		#region Static members
		#endregion

		private static readonly Assembly _executingAssembly;
		private static readonly string[] _resourceList;
		
		
		static Translator() {
			_executingAssembly = Assembly.GetExecutingAssembly();
			_resourceList = _executingAssembly.GetManifestResourceNames();
		}


		public static Translator LoadTranslator( Type type, string locale ) {
			var translationSheetName = _resourceList.FirstOrDefault( c => c.Contains( type.Name ) );
			if (translationSheetName is null) return new Translator();
			using (Stream stream = _executingAssembly.GetManifestResourceStream( translationSheetName )) {
				using (StreamReader reader = new StreamReader( stream, System.Text.Encoding.GetEncoding(1252) )) {
					var translationJson = reader.ReadToEnd();
					return new Translator( translationJson, locale );
				}
			}
		}




		#region Instance members
		#endregion

		public Translator( string translationJson = "{}", string locale = null ) : base() {
			CurrentLocale = locale;
			var translationDux = Dux.Dux.Import.FromJson( translationJson ) as Dux.DuxNamedList ?? new DuxNamedList();
			_dux = translationDux;
		}


		public string CurrentLocale { get; set; }
		private readonly Dux.DuxNamedList _dux;


		public string Translate( Tstring tstrg ) => Translate( tstrg, CurrentLocale );
	
		
		public string Translate( Tstring tstrg, string locale ) => _dux?[tstrg.id][locale].AsString( null ) ?? tstrg.str;

		public string TranslateOrNull( string key ) => _dux?[key][CurrentLocale].AsString( null );
		public string TranslateOrNull( string key, string locale ) => _dux?[key][locale].AsString( null );
	}
}
