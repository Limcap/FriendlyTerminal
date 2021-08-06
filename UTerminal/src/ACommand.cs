
using Limcap.Dux;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Limcap.UTerminal.ACommand;

namespace Limcap.UTerminal {
	public abstract partial class ACommand : ICommand {

		public ACommand( string locale ) {
			Locale = locale;
			Translator = LoadTranslator( locale );
			Info = Translator.Translate( GetInfo() );
		}




		//public const string DEFAULT_LOCALE = "enus";
		//public const string INVOKE_STRING = null;
		//public const int REQUIRED_PRIVILEGE = 0;




		protected Translator Translator { get; private set; }
		public string Locale { get; private set; }
		public Information Info { get; private set; }




		public abstract Information GetInfo();




		//public Information TranslateInfo( string locale ) {
		//	var translator = LoadTranslator( locale );
		//	var translatedInfo = translator.Translate( Info );
		//	return translatedInfo;
		//}



		public abstract string MainFunction( Terminal t, Args args );




		public Translator LoadTranslator( string locale = null ) {
			//var resourcesNames = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
			var resourcesNames = GetType().Assembly.GetManifestResourceNames();
			var assemblyName = GetType().Name;
			var translationSheetName = resourcesNames.FirstOrDefault( c => c.Contains( assemblyName ) );
			if (translationSheetName is null) return new Translator();
			using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream( translationSheetName )) {
				using (StreamReader reader = new StreamReader( stream )) {
					var translationJson = reader.ReadToEnd();
					return new Translator( translationJson, locale );
				}
			}
		}




		//public static Translator LoadTranslator( string locale, Type t ) {
		//	var resourcesNames = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
		//	var assemblyName = t.Name;
		//	var translationSheetName = resourcesNames.FirstOrDefault( c => c.Contains( assemblyName ) );
		//	if (translationSheetName is null) return new Translator();
		//	using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream( translationSheetName )) {
		//		using (StreamReader reader = new StreamReader( stream )) {
		//			var translationJson = reader.ReadToEnd();
		//			return new Translator( translationJson, locale );
		//		}
		//	}
		//}



		//public static string GetTranslatedInvokeString(string locale, Type type = null ) {
		//	type = type ?? MethodBase.GetCurrentMethod().DeclaringType;
		//	var translator = LoadTranslator( locale, type );
		//	var invokeText = type.GetConst( "INVOKE_STRING" ) as string;
		//	return translator.Translate( invokeText );
		//}
	}
}
