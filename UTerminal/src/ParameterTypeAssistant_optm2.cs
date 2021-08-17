﻿using System;
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
		private readonly List<ACommand.Parameter> _aux_possibleParams_main = new List<ACommand.Parameter>( 8 );
		private readonly List<ACommand.Parameter> _aux_possibleParams_temp = new List<ACommand.Parameter>( 8 );
		private readonly StringBuilder _aux_autocompleteResult = new StringBuilder( 60 );




		public ParameterTypeAssistant_optm2( Dictionary<string, Type> commandsSet, string locale ) {
			_commandsSet = commandsSet;
			_locale = locale;
		}




		internal unsafe StringBuilder GetAutocompleteOtions( string fullInput ) {
			//fullInput = "configurar terminal, tema: tamanho-da-fonte=14, cor-de-fundo=, terceiro= , ";

			var inputSolver = new InputSolver( fullInput );

			inputSolver.SolveCommand( _commandsSet, ref _currentCmd, _locale );

			if (inputSolver.cmd is null)
				return _aux_autocompleteResult.Reset( "Command not found" );

			int argsCount = inputSolver.CountArguments();
			var argsMem = stackalloc InputSolver.Arg[argsCount];
			inputSolver.SetMemoryForArgs( argsMem, argsCount );
			inputSolver.SolveArguments();

			//if (argsCount == 0)
			//	FormtParamsNames( inputSolver.cmd.Parameters, _aux_autocompleteResult );
			//else {
			FindPossibleParams( inputSolver.cmd, inputSolver.args, _aux_possibleParams_temp );
			_aux_possibleParams_main.Clear();
			_aux_possibleParams_main.AddRange( _aux_possibleParams_temp );
			FormatParamsNames( _aux_possibleParams_main, _aux_autocompleteResult );
			//}

			return _aux_autocompleteResult;
		}




		private StringBuilder FormatParamsNames( IEnumerable<ACommand.Parameter> parameters, StringBuilder sb ) {
			sb.Reset();
			if (parameters.IsNullOrEmpty()) return sb;
			foreach (var p in parameters) sb.Append( p.optional ? $"[{p.name}=]" : $"{p.name}=" ).Append( "     " );
			return sb;
		}




		private static unsafe void FindPossibleParams( ACommand cmd, InputSolver.Arg.Array args, List<ACommand.Parameter> result ) {
			result.Clear();

			//Initially we add to the result the possibilities, based on the name of the last argument.
			if (args.Last.name.IsNullOrEmty)
				result.AddRange( cmd.Parameters );
			else if (!args.Last.NameIsComplete || args.Last.ValueIsEmpty)
				cmd.Parameters.GetByNamePrefix( args.Last.name, result );

			// Then we remove from the result every parameter that has already been entered before.
			for (int i = 0; i < args.Length - 1; i++) {
				if (args[i].NameIsComplete) {
					var paramIndex = result.GetIndexByName( args[i].name );
					if (paramIndex > -1) result.RemoveAt( paramIndex );
				}
			}
		}




		//internal string AskForParameters( ACommand cmd ) {

		//}

	}
}
