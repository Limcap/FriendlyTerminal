using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Chan = System.Span<char>;
using Stan = System.ReadOnlySpan<char>;
using TwoStrings = System.ValueTuple<string, string>;

namespace Limcap.UTerminal {
	internal partial class ParameterTypeAssistant_optm2 {

		public ParameterTypeAssistant_optm2( Dictionary<string, Type> commandsSet, string locale ) {
			_commandsSet = commandsSet;
			_locale = locale;
		}




		private readonly Dictionary<string, Type> _commandsSet;
		private readonly string _locale;
		private ACommand _currentCmd;
		private readonly List<ACommand.Parameter> _auxMissingParams_main = new List<ACommand.Parameter>( 8 );
		private readonly List<ACommand.Parameter> _auxMissingParams_temp = new List<ACommand.Parameter>( 8 );
		private readonly StringBuilder _autocompleteResult = new StringBuilder( 60 );


		internal unsafe StringBuilder GetAutocompleteOtions( string fullInput ) {
			fullInput = "configurar terminal, tema: tamanho-da-fonte=, cor-de-fundo=,";

			var sorttedInput = new SorttedInput( fullInput.AsSpan() );
			sorttedInput.SortCommand( _commandsSet, ref _currentCmd, _locale );
			int argsCount = sorttedInput.ProspectAmountOfArguments();
			var argsMem = stackalloc Argument[argsCount];
			sorttedInput.SetSorttedArgsMem( argsMem, argsCount );
			sorttedInput.SortArguments();

			var parts = SplitInput( ref fullInput );
			_currentCmd = GetCommand( parts.item1 );
			if (_currentCmd is null) return _autocompleteResult.Reset( "Command not found" );
			ProcessAutocompleteOtions( _currentCmd, ref parts.item2, result: _autocompleteResult );
			return _autocompleteResult;
		}








		private void ProcessAutocompleteOtions( ACommand cmd, ref Stan paramsInput, StringBuilder result ) {
			// If the input is empty, returns all parameters
			result.Length = 0;
			if (paramsInput.Trim().Length == 0) {
				GetFormmattedParamsNames( cmd.Parameters, result );
			}

			// else select the parameters to return
			else {
				//!DEBUG
				//paramsInput = "tamanho-da-fonte=, cor-d-fundo=,".AsSpan();
				GetMissingParams( cmd, ref paramsInput, _auxMissingParams_temp );
				_auxMissingParams_main.Clear();
				_auxMissingParams_main.AddRange( _auxMissingParams_temp );
				GetFormmattedParamsNames( _auxMissingParams_main, result );
			}
		}








		private StanTuple SplitInput( ref string inputText ) {
			var inp = inputText.AsSpan();
			var cmdEndIndex = inputText.IndexOf( ':' );

			// In case there is no colon character, it is not time to assist with parameter typing since the command
			// hasn't been defined yey.
			if (cmdEndIndex == -1) return new StanTuple();

			var splittedInput = new StanTuple();
			// command name part
			splittedInput.item1 = inp.Slice( 0, cmdEndIndex );
			// parameters part
			splittedInput.item2 = cmdEndIndex < inp.Length ? inp.Slice( cmdEndIndex + 1 ) : null;

			return splittedInput;
		}








		private ACommand GetCommand( Stan invokeText ) {
			if (!_commandsSet.ContainsKey( invokeText.ToString() )) return null;
			var cmdType = _commandsSet[invokeText.ToString()];
			//if (_currentCmd?.GetType() == cmdType) return _currentCmd;
			var instance = _currentCmd?.GetType() == cmdType ? _currentCmd
			: cmdType.IsSubclassOf( typeof( ACommand ) ) ? (ACommand)Activator.CreateInstance( cmdType, _locale )
			: null;
			return instance;
		}








		private StringBuilder GetFormmattedParamsNames( IEnumerable<ACommand.Parameter> parameters, StringBuilder sb ) {
			if (parameters.IsNullOrEmpty()) return sb;
			sb.Reset();
			foreach (var p in parameters) sb.Append( p.optional ? $"[{p.name}=]" : $"{p.name}=" ).Append( "     " );
			return sb;
		}
		private StringBuilder GetFormmattedParamsNames( ACommand.Parameter p, StringBuilder sb ) {
			return sb.Reset( p.optional ? $"[{p.name}=]" : $"{p.name}=" ).Append( "     " );
		}







		private static unsafe void GetMissingParams( ACommand cmd, ref Stan paramsInput, List<ACommand.Parameter> result ) {
			//!DEBUG
			//paramsInput = " cor-da-fonte= , cor-do-fundo ,  ".AsSpan();

			var slicedInput = new SlicedChan();
			slicedInput.ProspectSlices( ref paramsInput, ',' );
			var schanRangePtr = stackalloc Range[slicedInput.NumberOfSlices];
			slicedInput.SetIndexMemory( schanRangePtr );
			slicedInput.Slice();
			slicedInput.Trim();

			result.Clear();

			Stan lastPart = slicedInput[slicedInput.NumberOfSlices - 1];
			if (lastPart.Length > 0) {
				var inputtedName = lastPart.Trim( '=' );
				//if (inputtedName == null) inputtedName = lastPart;
				cmd.Parameters.GetByNamePrefix( inputtedName, result );
			}
			else {
				foreach (var p in cmd.Parameters) {
					bool isMissing = true;
					var paramName = p.name.AsSpan();
					for (int i = 0; i < slicedInput.NumberOfSlices; i++) {
						if (!slicedInput[i].Contains( '=' )) continue;
						Stan inputtedName = slicedInput[i].SliceAtChar( '=' );
						if (inputtedName.Equals( paramName, StringComparison.Ordinal ))
							isMissing = false;
					}
					if (isMissing) result.Add( p );
				}
			}
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

	}

}
