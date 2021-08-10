using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.UTerminal {
	internal class ParameterTypeAssistant {

		public ParameterTypeAssistant( Dictionary<string, Type> commandsSet, string locale ) {
			_commandsSet = commandsSet;
			_locale = locale;
		}




		private readonly Dictionary<string, Type> _commandsSet;
		private readonly string _locale;
		private ACommand _currentCmd;




		internal List<string> GetAutocompleteOtions( string fullInput ) {
			var (invokeText, parameters) = SplitInput( fullInput );
			_currentCmd = GetCommand( invokeText );
			if (_currentCmd == null) return null;
			//return GetAutocompleteOtions( _currentCmd, parameters );
			return _commandsSet.Keys.ToList();
		}








		private List<string> GetAutocompleteOtions( ACommand cmd, string paramsInput ) {
			// If the input is empty, returns all parameters
			if (paramsInput.Trim().Length == 0) {
				var paramNames = GetParamsNames( cmd.Parameters );
				return paramNames;
			}

			// else select the parameters to return
			else {
				var paramsInputArr = SplitParamsInput( paramsInput );
				var missingParams = GetMissingParams( cmd, paramsInputArr );

				// if the input ends right after a comma, shows all the missing params
				if (LastCharOfParamsInputIsCommaOrEmpty( paramsInput )) {
					if (missingParams.IsNullOrEmpty()) return new List<string>();
					else return GetParamsNames( missingParams );
				}

				// else show only the predicted params
				else {
					var lastParamInput = paramsInputArr.Length == 0 ? paramsInputArr[0] : paramsInputArr[paramsInputArr.Length - 1];
					var predictedParams = GetPossibleParameters( lastParamInput, missingParams );
					if (predictedParams.IsNullOrEmpty()) return new List<string>();
					return GetParamsNames( predictedParams );
				}
			}
		}








		private Tuple<string, string> SplitInput( string input ) {
			var inputParts = input.Split( ':' );

			// In case there is no colon character, it is not time to assist with parameter typing since the command
			// hasn't been defined yey.
			if (inputParts.Length == 1) return null;

			var invokeText = inputParts[0];
			var parameters = inputParts.Length > 0 ? inputParts[1].Trim() : string.Empty;
			return Tuple.Create( invokeText, parameters );
		}








		private ACommand GetCommand( string invokeText ) {
			if (!_commandsSet.ContainsKey( invokeText )) return null;
			var cmdType = _commandsSet[invokeText];
			//if (_currentCmd?.GetType() == cmdType) return _currentCmd;
			var instance = _currentCmd?.GetType() == cmdType ? _currentCmd
			: cmdType.IsSubclassOf( typeof( ACommand ) ) ? (ACommand)Activator.CreateInstance( cmdType, _locale )
			: null;
			return instance;
		}








		private List<string> GetParamsNames( IEnumerable<ACommand.Parameter> parameters ) {
			return parameters?.Select( p => p.optional ? $"[{p.name}=]" : $"{p.name}=" ).ToList();
		}








		/// <summary>
		/// Splits a string that represents the parameters part of a command invoke expression (the part after the
		/// colon character) in an array of strings where the separator is the comma (,) character.
		/// </summary>
		/// <param name="paramsInput"></param>
		/// <returns></returns>
		private string[] SplitParamsInput( string paramsInput ) {
			var paramsInputArr = paramsInput.Split( ',' ).Select( p => p.Trim() ).ToArray();
			return paramsInputArr;
		}








		private ACommand.Parameter[] GetMissingParams( ACommand cmd, string[] paramsInputArr ) {
			var missingParams = new List<ACommand.Parameter>( cmd.Parameters );
			for (int i = 0; i < paramsInputArr.Length; i++) {
				var paramArr = paramsInputArr[i].Split( '=' );
				var paramName = paramArr[0];
				// if the param input arr is length 1, it means the input does not have the equal sign,
				// wich means the parameter is not yet completed, so we won't consider it for removing from
				// the missing params list.
				if (paramArr.Length == 1) continue;
				if (cmd.Parameters.Where( p => p.name == paramName ).Count() > 0)
					missingParams.RemoveAll( p => p.name == paramName );
			}
			return missingParams.ToArray();
		}







		private bool LastCharOfParamsInputIsCommaOrEmpty( string input ) {
			return input.Trim().Length == 0 || input.Trim().Last() == ',';
		}








		private ACommand.Parameter[] GetPossibleParameters( string aParamInput, IEnumerable<ACommand.Parameter> parameters ) {
			if (aParamInput.Length == 0) return null;
			var aParamInputArr = aParamInput.Split( '=' );

			// if the last param input has the equal sign then there's no need for autocomplete at this time.
			if (aParamInputArr.Length > 1) return null;

			// else try to find the corresponding parameter.
			return parameters.Where( p => p.name.StartsWith( aParamInputArr[0] ) ).ToArray();
		}








		//internal string AskForParameters( ACommand cmd ) {

		//}


		//internal class CommandNotFoundException : Exception {
		//	public CommandNotFoundException( string invokeText ) : base( message ) {
		//	}
		//	public static string Msg(string invokeText ) {
		//		return "The command was not found"
		//	}
		//}
	}
}
