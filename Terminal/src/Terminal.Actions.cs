using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.TextboxTerminal {
	public partial class Terminal {
		public void TypeText( string text ) {
			_mainArea.Text += text;
			_mainArea.CaretIndex = _mainArea.Text.Length;
		}



		public void Clear() {
			Text = _introText + PromptLine;
		}




		public void Exit() {
			_onExit();
		}




		public void ReadLine( Func<string, string> inputHandler ) {
			_inputHandler = inputHandler;
		}
	}
}
