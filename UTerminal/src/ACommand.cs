
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




		public string TranslatedInfo( string locale ) {
			var translator = LoadTranslator( locale );
			return Info.ToString( translator );
		}




		public Translator LoadTranslator( string locale = null ) {
			//var resourcesNames = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
			var resourcesNames = GetType().Assembly.GetManifestResourceNames();
			var assemblyName = GetType().Name;
			var translationSheetName = resourcesNames.FirstOrDefault( c => c.Contains( assemblyName ) );
			using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream( translationSheetName )) {
				using (StreamReader reader = new StreamReader( stream )) {
					var translationJson = reader.ReadToEnd();
					var translationDux = Dux.Dux.Import.FromJson( translationJson );
					return new Translator( translationDux as DuxNamedList, locale );
				}
			}
		}
	}
}
