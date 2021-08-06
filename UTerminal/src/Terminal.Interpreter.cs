using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.UTerminal {
	public partial class Terminal {

		//private List<ICommand> _cmds = new List<ICommand>();
		private Dictionary<string, Type> _cmdList;




		public void RegisterCommand<T>( string invokeString = null ) where T : ICommand, new() {
			RegisterCommand( typeof( T ), invokeString );
		}




		public void RegisterCommandsInNamespaces( params string[] nspaces ) {
			var typeICommand = typeof( ICommand );
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var nspace in nspaces) {
				//var resourceStream = executingAssembly.GetManifestResourceStream( translationSheetName )
				var types = assemblies
				.SelectMany( s => s.GetTypes() )
				.Where( p => typeICommand.IsAssignableFrom( p ) && p.Namespace == nspace );
				foreach (var t in types) RegisterCommand( t );
			}
		}




		void RegisterCommand( Type type, string customInvokeText = null ) {
			string defaultLocale = (string)(type.GetConst( "DEFAULT_LOCALE" ) ?? Locale);
			if (customInvokeText != null) _cmdList.Add( customInvokeText, type );
			else {
				Tstring embeddedInvokeText = type.GetConst( "INVOKE_STRING" ) as string;
				if (type.IsSubclassOf( typeof( ACommand ) ) && defaultLocale != Locale)
					embeddedInvokeText = GetTranslatedInvokeText( type, Locale );
				_cmdList.Add( embeddedInvokeText, type );
			}
		}




		string GetTranslatedInvokeText( Type type, string locale ) {
			var translator = Translator.LoadTranslator( type, locale );
			var invokeText = type.GetConst( "INVOKE_STRING" ) as string;
			return translator.Translate( invokeText );
		}




		//Translator LoadTranslator( Type type, string locale ) {
		//	//var resourcesNames = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
		//	var executingAssembly = GetType().Assembly;
		//	var resourcesNames = executingAssembly.GetManifestResourceNames();
		//	var assemblyName = type.Name;
		//	var translationSheetName = resourcesNames.FirstOrDefault( c => c.Contains( assemblyName ) );
		//	if (translationSheetName is null) return new Translator();
		//	using (Stream stream = executingAssembly.GetManifestResourceStream( translationSheetName )) {
		//		using (StreamReader reader = new StreamReader( stream )) {
		//			var translationJson = reader.ReadToEnd();
		//			return new Translator( translationJson, locale );
		//		}
		//	}
		//}




		public string ProcessInput( string input ) {
			int splitter = Math.Max( 0, input.IndexOf( ':' ) );
			var cmd = splitter == 0 ? input : input.Remove( splitter ).Trim();
			var arg = splitter == 0 ? string.Empty : input.Substring( splitter + 1 ).Trim();

			if (cmd == "exit") {
				Clear();
				onExit?.Invoke();
				return string.Empty;
			}
			if (cmd == "clear") {
				Clear();
				return null;
			}
			if (!_cmdList.ContainsKey( cmd )) {
				return "Comando não reconhecido.";
			}
			else {
				Type cmdType = _cmdList[cmd];
				int requiredPrivilege = (int)(cmdType.GetConst( "REQUIRED_PRIVILEGE" ) ?? 0);
				if (arg == "?")
					return GetCommandInfo( cmdType );
				//return cmdType.GetConst("HELP_INFO") as string ?? "Este comando não possui informação de ajuda.";
				if (CurrentPrivilege < requiredPrivilege)
					return INSUFICIENT_PRIVILEGE_MESSAGE;
				var instance = cmdType.IsSubclassOf( typeof( ACommand ) )
					? (ICommand)Activator.CreateInstance( cmdType, Locale )
					: (ICommand)Activator.CreateInstance( cmdType );
				try {
					return instance.MainFunction( this, arg );
				}
				catch (Exception ex) {
					return ex.ToString();
				}
			}
		}




		public string GetCommandInfo( Type t ) {
			if (t.IsSubclassOf( typeof( ACommand ) )) {
				var instance = Activator.CreateInstance( t, Locale ) as ACommand;
				//var translator = instance.LoadTranslator( Locale );
				//var translatedInfo = translator.Translate( instance.Info );
				return instance.Info.ToString();
			}
			else if (t.IsAssignableFrom( typeof( ICommand ) ))
				return t.GetConst( "HELP_INFO" ) as string ?? "Este comando não possui informação de ajuda.";
			return null;
		}
	}
}
