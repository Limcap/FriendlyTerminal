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
	internal partial class ParameterTypeAssistant_optm1 {

		public ParameterTypeAssistant_optm1( Dictionary<string, Type> commandsSet, string locale ) {
			_commandsSet = commandsSet;
			_locale = locale;
		}




		private readonly Dictionary<string, Type> _commandsSet;
		private readonly string _locale;
		private ACommand _currentCmd;
		private readonly List<ACommand.Parameter> _auxMissingParams_main = new List<ACommand.Parameter>( 8 );
		private readonly List<ACommand.Parameter> _auxMissingParams_temp = new List<ACommand.Parameter>( 8 );
		private readonly List<string> _auxMissingParamsNames = new List<string>( 8 );


		internal List<string> GetAutocompleteOtions( string fullInput ) {
			fullInput = "configurar terminal, tema: tamanho=,";
			var parts = SplitInput_pf2( ref fullInput );
			_currentCmd = GetCommand( parts.first );
			var result = GetAutocompleteOtions( _currentCmd, ref parts.second );
			return result;
			//return _commandsSet.Keys.ToList();
		}








		private List<string> GetAutocompleteOtions( ACommand cmd, ref Stan paramsInput ) {
			// If the input is empty, returns all parameters
			if (paramsInput.Trim().Length == 0) {
				var paramNames = GetParamsNames( cmd.Parameters );
				return paramNames;
			}

			// else select the parameters to return
			else {
				//paramsInput = "tamanho-da-fonte=, cor-d-fundo=,".AsSpan();
				//var paramsInputArr = SplitParamsInput( ref paramsInput );
				GetMissingParams( cmd, ref paramsInput, _auxMissingParams_temp );
				if (!_auxMissingParams_temp.All( _auxMissingParams_main.Contains )) {
					_auxMissingParams_main.Clear();
					_auxMissingParams_main.AddRange( _auxMissingParams_temp );
					_auxMissingParamsNames.Clear();
					foreach (var p in _auxMissingParams_main)
						_auxMissingParamsNames.Add( p.name );
					return _auxMissingParamsNames;
				}
				return null;
				

				/*
				// if the input ends right after a comma, shows all the missing params
				if (LastCharOfParamsInputIsCommaOrEmpty( ref paramsInput )) {
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
				*/
			}
		}








		private TwoStran SplitInput_pf2( ref string inputText ) {
			var inp = inputText.AsSpan();
			var cmdEndIndex = inputText.IndexOf( ':' );

			// In case there is no colon character, it is not time to assist with parameter typing since the command
			// hasn't been defined yey.
			if (cmdEndIndex == -1) return new TwoStran();

			Stan invokeText = inp.Slice( 0, cmdEndIndex );
			Stan paramsText = cmdEndIndex < inp.Length ? inp.Slice( cmdEndIndex + 1 ) : null;
			//var a = new TwoStrings() { cmdText = invokeText, paramsText = paramsText };
			var splittedInput = new TwoStran( ref invokeText, ref paramsText );
			return splittedInput;
		}
		private ValueTuple<string, string> SplitInput_pf1( string input ) {
			var inp = input.AsSpan();
			var cmdEndIndex = input.IndexOf( ':' );

			// In case there is no colon character, it is not time to assist with parameter typing since the command
			// hasn't been defined yey.
			if (cmdEndIndex == -1) return (null, null);

			string invokeText = inp.Slice( 0, cmdEndIndex ).ToString();
			string parameters = cmdEndIndex < inp.Length ? inp.Slice( cmdEndIndex + 1 ).ToString() : null;
			return (invokeText.ToString(), parameters.ToString());
		}
		private ValueTuple<string, string> SplitInput_pf0( string input ) {
			var inputParts = input.Split( ':' );

			// In case there is no colon character, it is not time to assist with parameter typing since the command
			// hasn't been defined yey.
			if (inputParts.Length == 1) return (null, null);

			var invokeText = inputParts[0];
			var parameters = inputParts.Length > 0 ? inputParts[1].Trim() : string.Empty;
			return (invokeText, parameters);
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
		//private ACommand GetCommand( string invokeText ) {
		//	if (!_commandsSet.ContainsKey( invokeText )) return null;
		//	var cmdType = _commandsSet[invokeText];
		//	//if (_currentCmd?.GetType() == cmdType) return _currentCmd;
		//	var instance = _currentCmd?.GetType() == cmdType ? _currentCmd
		//	: cmdType.IsSubclassOf( typeof( ACommand ) ) ? (ACommand)Activator.CreateInstance( cmdType, _locale )
		//	: null;
		//	return instance;
		//}








		private List<string> GetParamsNames( IEnumerable<ACommand.Parameter> parameters ) {
			return parameters?.Select( p => p.optional ? $"[{p.name}=]" : $"{p.name}=" ).ToList();
		}








		/// <summary>
		/// Splits a string that represents the parameters part of a command invoke expression (the part after the
		/// colon character) in an array of strings where the separator is the comma (,) character.
		/// </summary>
		/// <param name="paramsInput"></param>
		/// <returns></returns>
		//private string[] SplitParamsInput( ref Stran paramsInput ) {
		//	var paramsInputArr = paramsInput.Split( ',' ).Select( p => p.Trim() ).ToArray();
		//	return paramsInputArr;
		//}







		private static unsafe void GetMissingParams( ACommand cmd, ref Stan paramsInput, List<ACommand.Parameter> listToFill  ) {

			// Works and no allocations are made, all is on the stack!
			var charr = new SlicedChan();
			paramsInput = " cor-da-fonte= , cor-do-fundo ,  ".AsSpan();
			charr.SetupLength_1st( ref paramsInput, '=', ',' );
			var cva1_rangePtr = stackalloc Range[charr.NumberOfSlices];
			charr.SetupRanges_2nd( cva1_rangePtr );

			listToFill.Clear();

			//foreach (var p in cmd.Parameters) {
			//	bool isMissing = true;
			//	var name = p.name.AsSpan();
			//	for (int i = 0; i < charr.Length; i++) {
			//		if (charr[i].Equals( name, StringComparison.Ordinal ))
			//			isMissing = false;
			//		if (isMissing) tempList.Add( p );
			//	}
			//}
			
			foreach (var p in cmd.Parameters)
				if (!charr.Contains( p.name )) listToFill.Add( p );
		}




		private unsafe ACommand.Parameter[] GetMissingParams_old( ACommand cmd, ref Stan paramsInput ) {
			int idx = 0, commaIdx;
			Chan param;
			var missingParams = new Span<ACommand.Parameter>( cmd.Parameters );
			_auxMissingParams_main.Clear();
			_auxMissingParams_main.AddRange( cmd.Parameters );


			Chan inputParamNames = ReplaceRanges( ref paramsInput, ',', '=', ',' );

			// Does not work because the memory allocated inside is lost when the cnostructor returns.
			var cva1 = new ChanHoloArray1( ref paramsInput, '=', ',' );

			// Works but the cva2_ranges is allocated in the heap
			Span<Range> cva2_ranges = new Range[0];
			var cva2 = new ChanHoloArray2( ref paramsInput, ref cva2_ranges, '=', ',' );

			// Works and no allocations are made, all is on the stack!
			var cva3 = new ChanHoloArray3();
			cva3.SetupLength_1st( ref paramsInput, '=', ',' );
			var cva1_rangePtr = stackalloc Range[cva3.Length];
			cva3.SetupRanges_2nd( cva1_rangePtr );

			inputParamNames.IndexOf( ',', 1 );
			while (idx < inputParamNames.Length && (commaIdx = inputParamNames.IndexOf( ',', idx )) > -1) {
				//commaIdx = inputParamNames.IndexOf( ',', idx );
				//if (commaIdx == -1) commaIdx = inputParamNames.Length - 1;
				param = inputParamNames.Slice( idx, commaIdx );
				idx = commaIdx;

				foreach (var p in missingParams) {
					var n = p.name.AsSpan();
					//if (n.Equals( p )) missingParams.FindAndRemove( p => p.name, param )
				}
			}
			/*
			while ((commaIdx = paramsInput.IndexOf( ',', idx )) > -1) {
				param = paramsInput.Slice( idx, commaIdx - idx ).Trim().Trim( ',' );
				idx = commaIdx;
				missingParams.FindAndRemove( p => p.name,)
				for (int j =)
			}


			for (idx = 0; idx < paramsInput.Length; idx++) {
				if (paramsInput[idx] != ',') continue;
				commaIdx = paramsInput.IndexOf( ',' );
				if (commaIdx > -1) {
					param = paramsInput.Slice( idx, commaIdx - idx );
					int equalIdx = param.Trim().IndexOf( '=' );
				}
			}

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
			*/

			unsafe Chan ReplaceRanges( ref Stan stran, char start, char stop, char replace = ';' ) {
				int j = 0;
				char* newStran = stackalloc char[stran.Length];
				var o = stran.ToArray();
				bool shouldCopy = true;
				for (int i = 0; i < stran.Length; i++) {
					if (stran[i] == start) { shouldCopy = true; continue; }
					if (stran[i] == stop) { shouldCopy = false; newStran[j++] = replace; }
					if (shouldCopy) newStran[j++] = stran[i];
				}
				//var a = newStran.Slice( 0, j + 1 );
				//var a = new string( newStran );
				var b = new Chan( newStran, j );
				return new Chan( newStran, j );
			}

			return new ACommand.Parameter[2];
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


		public ref struct SplitedInput {
			public ReadOnlySpan<char> cmdText;
			public ReadOnlySpan<char> paramsText;
		}
		public ref struct TwoStran {
			public TwoStran( ref Stan first, ref Stan second ) {
				this.first = first;
				this.second = second;
			}
			public Stan first;
			public Stan second;
		}
	}

	public static class OptimizedExtensions {
		public static int IndexOf( this Chan stran, char searchedChar, int startIndex = 0 ) {
			for (int i = startIndex; i < stran.Length; i++)
				if (i == searchedChar) return i;
			return -1;
		}
		public static Span<T> FindAndRemove<T, K>( ref this Span<T> span, Func<T, K> identifier, Span<K> elements ) {
			Span<int> indexesFound = stackalloc int[span.Length];
			int count = 0;
			foreach (ref var i in indexesFound) i = -1;
			for (int i = 0; i < span.Length; i++) if (elements.Contains( identifier( span[i] ) )) indexesFound[count++] = i;
			Span<T> missing = span.Slice( 0, count );
			for (int i = 0; i < indexesFound.Length; i++) if (indexesFound[i] > -1) missing[count++] = span[indexesFound[i]];
			return missing;
		}
		//public static Span<T> FindAndRemove<T, K>( ref this Span<T> span, Func<T, K> identifier, Span<K> elements ) {
		//	Span<int> indexesFound = stackalloc int[span.Length];
		//	int count = 0;
		//	foreach (ref var i in indexesFound) i = -1;
		//	for (int i = 0; i < span.Length; i++) if (elements.Contains( identifier( span[i] ) )) indexesFound[count++] = i;
		//	Span<T> missing = span.Slice( 0, count );
		//	for (int i = 0; i < indexesFound.Length; i++) if (indexesFound[i] > -1) missing[count++] = span[indexesFound[i]];
		//	return missing;
		//}
		//public static unsafe bool FindAndRemove<K>( this Span<ACommand.Parameter> span, Func<ACommand.Parameter, K> identifier, Span<K> elements ) {
		//	Span<int> indexesFound = stackalloc int[span.Length];
		//	int count = 0;
		//	foreach (ref var i in indexesFound) i = -1;
		//	for (int i = 0; i < span.Length; i++) if (elements.Contains( identifier( span[i] ) )) indexesFound[count++] = i;
		//	Span<ACommand.Parameter> missing = stackalloc ACommand.Parameter[3];
		//	var b = typeof( T );
		//	var a = sizeof();
		//	foreach (ref var i in indexesFound) {
		//		if (i > -1)
		//	}

		//}
		//public static bool FindAndRemove( this Span<ACommand.Parameter> span, Func<ACommand.Parameter, bool> identifier ) {
		//	Span < ACommand.Pa >
		//	for (int i = 0; i < span.Length; i++) {
		//		if (identifier( span[i] ) == true) span[i].cle
		//	}
		//	var a = new Span<T>();
		//	var a = sizeof( Span<char> );
		//}
		public static bool Contains<T>( ref this Span<T> collection, T element ) {
			foreach (T item in collection) if (item.Equals( element )) return true;
			return false;
		}
		public static int CountChar( ref this Stan stran, char searchedChar ) {
			int count = 0;
			foreach (char c in stran) if (c == searchedChar) count++;
			return count;
		}
		public static bool EqualsString( ref this Stan stan, string other ) {
			if (stan.Length != other.Length) return false;
			for (int i = 0; i < stan.Length; i++) if (stan[i] != other[i]) return false;
			return true;
		}
		public static bool Contains( ref this SlicedChan charr, string element ) {
			for (int i = 0; i < charr.NumberOfSlices; i++) {
				var c = charr[i];
				if (c.EqualsString( element )) return true;
			}
			return false;
		}
	}
}
