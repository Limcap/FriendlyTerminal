using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace Limcap.UTerminal {
	public partial class Assistant {

		protected readonly List<string> _invokeStrings;

		// USED BYT ARGUMENT PROCESSING
		protected readonly Dictionary<string, Type> _commandsSet;
		protected readonly string _locale;

		protected readonly StringBuilder _predictionResult = new StringBuilder( 60 );
		protected readonly StringBuilder _autocompleteResult = new StringBuilder( 30 );

		protected ArgParser _argsParser;
		protected CmdParser _cmdParser;

		public ACommand ParsedCommand { get => _cmdParser?.parsedCmd; }
		public PString RawArgs { get; protected set; }
		public Arg[] ParsedArgs { get => _argsParser.Elements; }



		public int Index { get; protected set; }
		public bool AutocompleteParams { get; protected set; } //false is command prediction, true is parameter prediction








		#region SETUP
		#endregion




		public Assistant( Dictionary<string, Type> commandsSet, string locale ) {

			_cmdParser = new CmdParser( commandsSet, locale );
			_argsParser = new ArgParser( 8 );

			_locale = locale;
			_commandsSet = commandsSet;
		}








		#region PREDICTION
		#endregion




		public void ProcessInput( string input ) {
			var (inpCmd, inpArgs) = SplitInput( input );
			RawArgs = inpArgs;

			_cmdParser.Parse( inpCmd, _locale );

			if (_cmdParser.parsedCmd == null) {
				AutocompleteParams = false;
				_argsParser.Reset();
			}

			else {
				_argsParser.Parse( inpArgs, _cmdParser.parsedCmd?.Parameters );
				AutocompleteParams = true;
			}

			// Resets the autocomplete index, because new prediction options have been generated.
			Index = -1;
		}








		public StringBuilder GetPredictions( string input ) {
			var (inpCmd, inpArgs) = SplitInput( input );
			RawArgs = inpArgs;

			//_cmdParser = new CmdParser( commandsSet, locale );
			//_argsParser = new ArgParser( 8 );
			//GC.Collect();
			//GC.WaitForPendingFinalizers();
			//GC.Collect();
			_cmdParser.Parse( inpCmd, _locale );
	
			if (_cmdParser.parsedCmd == null) {
				_cmdParser.GetPredictionPossibilitiesText( _predictionResult );
				AutocompleteParams = false;
				_argsParser.Reset();
			}

			else {
				_argsParser.Parse( inpArgs, _cmdParser.parsedCmd?.Parameters );
				_argsParser.GetPredictionPossibilities( _predictionResult );
				AutocompleteParams = true;
			}

			// Resets the autocomplete index, because new prediction options have been generated.
			Index = -1;

			return _predictionResult;
		}








		protected (PString inpCmd, PString inpArgs) SplitInput( string input ) {
			var slicer = ((PString)input).GetSlicer( CmdParser.CMD_TERMINATOR, PString.Slicer.Mode.IncludeSeparatorAtEnd );
			var inpCmd = slicer.Next();
			var inpArgs = slicer.Remaining();
			inpArgs.Trim();
			return (inpCmd, inpArgs);
		}








		#region AUTOCOMPLETE
		#endregion




		public StringBuilder GetNextAutocompleteEntry() {
			// Adjust Index
			 Index++;
			//if (AutocompleteParams == false && Index >= _cmdParser.predictedNodes.Count ||
			//	 AutocompleteParams == true && Index >= _argsParser.possible.Count )
			//	Index = -1;

			_autocompleteResult.Clear();
			
			if (!AutocompleteParams) {
				if (Index >= _cmdParser.predictedNodes.Count) Index = -1;
				_cmdParser.GetConfirmedText( _autocompleteResult );
				_cmdParser.GetPredictionText( Index, _autocompleteResult );
			}
			else {
				if (Index >= _argsParser.possible.Count) Index = -1;
				_cmdParser.GetConfirmedText( _autocompleteResult );
				_argsParser.GetConfirmedText( _autocompleteResult );
				_argsParser.GetPredictionText( Index, _autocompleteResult );
			}

			return _autocompleteResult;
		}







		public void Reset() {
			RawArgs = null;
			_cmdParser.Reset();
			_argsParser.Reset();
		}
	}
}
