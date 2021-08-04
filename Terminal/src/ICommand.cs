using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.TextboxTerminal {
	public interface ICommand {

		// When creating a new comand class with this interface, the following fields are optional.
		// If INVOKE_STRING is not defined, it can be defined when registering the command class withing the Terminal instance,
		// or else it will be registered with the actual string that represents the class name.
		// If REQUIRED_PRIVILEGE is not define, the command can be executed by any privilege level of the Terminal instance.
		//public const string INVOKE_STRING;
		//public const int REQUIRED_PRIVILEGE;

		string HelpInfo { get; }

		string MainFunction( Terminal t, Args args );
	}
}
