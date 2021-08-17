using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Chan = System.Span<char>;
using Stan = System.ReadOnlySpan<char>;

namespace Limcap.UTerminal {
	internal partial class ParameterTypeAssistant_optm2 {

		private readonly Dictionary<string, Type> _commandsSet;
		private readonly string _locale;
		private ACommand _currentCmd;
		private readonly List<ACommand.Parameter> _auxMissingParams_main = new List<ACommand.Parameter>( 8 );
		private readonly List<ACommand.Parameter> _auxMissingParams_temp = new List<ACommand.Parameter>( 8 );
		private readonly StringBuilder _autocompleteResult = new StringBuilder( 60 );




		public ParameterTypeAssistant_optm2( Dictionary<string, Type> commandsSet, string locale ) {
			_commandsSet = commandsSet;
			_locale = locale;
		}




		internal unsafe StringBuilder GetAutocompleteOtions( string fullInput ) {
			fullInput = "configurar terminal, tema: tamanho-da-fonte=14, cor-de-fundo=, terceiro= , ";

			var inputSolver = new InputSolver( fullInput.AsSpan() );

			inputSolver.SolveCommand( _commandsSet, ref _currentCmd, _locale );

			if(inputSolver.cmd is null)
				return _autocompleteResult.Reset( "Command not found" );

			int argsCount = inputSolver.CountArguments();
			var argsMem = stackalloc InputSolver.Arg[argsCount];
			inputSolver.SetMemoryForArgs( argsMem, argsCount );
			inputSolver.SolveArguments();

			if(argsCount == 0)
				GetFormmattedParamsNames( inputSolver.cmd.Parameters, _autocompleteResult );
			else {
				GetMissingParams( inputSolver.cmd, inputSolver.args, _auxMissingParams_temp );
				_auxMissingParams_main.Clear();
				_auxMissingParams_main.AddRange( _auxMissingParams_temp );
				GetFormmattedParamsNames( _auxMissingParams_main, _autocompleteResult );
			}

			return _autocompleteResult;
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




		private static unsafe void GetMissingParams( ACommand cmd, InputSolver.Arg.Array args, List<ACommand.Parameter> result ) {
			result.Clear();

			var lastArg = args[args.Length - 1];
			if (args.Length > 0) {
				cmd.Parameters.GetByNamePrefix( args.Last.name, result );
			}
			else {
				foreach (var p in cmd.Parameters) {
					bool isMissing = true;
					for (int i = 0; i < args.Length; i++) {
						if (!args[i].value.IsNull) continue;
						if (args[i].name == p.name)
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
