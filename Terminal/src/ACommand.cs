using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.TextboxTerminal {
	public abstract class ACommand : ICommand {

		public static string INVOKE_STRING = null;
		public static int REQUIRED_PRIVILEGE = 0;
		public static string HELP_INFO = "This command does not have help info.";
		public abstract string MainFunction( Terminal t, Args args );
	}
}
