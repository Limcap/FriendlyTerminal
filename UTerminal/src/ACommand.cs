
using Limcap.Dux;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Limcap.UTerminal.ACommand;

namespace Limcap.UTerminal {
	public abstract partial class ACommand : ICommand {

		//public const string INVOKE_STRING = null;
		//public const int REQUIRED_PRIVILEGE = 0;

		public abstract Information Info { get; }
		public abstract string MainFunction( Terminal t, Args args );




		public Information TranslateInfo( string locale ) {
			var translator = LoadTranslator( locale );
			var translatedInfo = translator.Translate( Info );
			return translatedInfo;
		}




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
	}
}
