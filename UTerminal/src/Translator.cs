
using Limcap.Dux;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Limcap.UTerminal {
	public class Translator {

		#region Static members
		#endregion

		private static readonly Assembly executingAssembly;
		private static readonly string[] resourceList;
		
		
		static Translator() {
			executingAssembly = Assembly.GetExecutingAssembly();
			resourceList = executingAssembly.GetManifestResourceNames();
		}


		public static Translator LoadTranslator( Type type, string locale ) {
			var translationSheetName = resourceList.FirstOrDefault( c => c.Contains( type.Name ) );
			if (translationSheetName is null) return new Translator();
			using (Stream stream = executingAssembly.GetManifestResourceStream( translationSheetName )) {
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
			dux = translationDux;
		}


		public string CurrentLocale { get; set; }
		private readonly Dux.DuxNamedList dux;


		public string Translate( Tstring tstrg ) => Translate( tstrg, CurrentLocale );
	
		
		public string Translate( Tstring tstrg, string locale ) => dux?[tstrg.id][locale].AsString( null ) ?? tstrg.str;
	}
}
