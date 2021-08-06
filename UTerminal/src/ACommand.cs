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

		public ACommand( string locale ) {
			Locale = locale;
			CmdTranslator = Translator.LoadTranslator( GetType(), locale );
			Info = GetInfo().Translate( CmdTranslator );
		}


		//public const string DEFAULT_LOCALE = "enus";
		//public const string INVOKE_STRING = null;
		//public const int REQUIRED_PRIVILEGE = 0;


		protected Translator CmdTranslator { get; private set; }
		public string Locale { get; private set; }
		public Information Info { get; private set; }


		public abstract Information GetInfo();


		public abstract string MainFunction( Terminal t, Args args );
	}
}
