using Limcap.Dux;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.UTerminal {
	public abstract partial class ACommand : ICommand {

		public const string NEW_LINE = Terminal.NEW_LINE;

		//public const string DEFAULT_LOCALE = "enus";
		//public const string INVOKE_STRING = null;
		//public const int REQUIRED_PRIVILEGE = 0;


		public ACommand( string locale ) {
			Locale = locale;
			if (locale != null && locale != GetType().GetConst("DEFAULT_LOCALE") as string) {
				CmdTranslator = Translator.LoadTranslator( GetType(), locale );
				if (Txt != null) {
					var keys = Txt.Keys.ToList();
					for (var i=0; i<Txt.Keys.Count; i++ ) {
						var key = keys[i];
						Txt[key] = CmdTranslator.Translate( key ) ?? Txt[key];
					}
				}
				Info = GetInfo().Translate( CmdTranslator );
			}
		}


		protected Translator CmdTranslator { get; private set; }
		public string Locale { get; private set; }
		public Information Info { get; private set; }


		public abstract Information GetInfo();


		public abstract string MainFunction( Terminal t, Args args );

		protected virtual TextLibrary Txt { get; set; }
		//protected virtual string[] Txt { get; set; } = { };
	}

	public class TextLibrary : Dictionary<string,string> { }
}
