using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.UTerminal {

	/// <summary>
	/// Interface for commands to be registered withing a Terminal.
	/// The class impementing this interface should also implement the following fields:
	/// <br/>public const string INVOKE_STRING: the string that when typed in the terminal will invoke this command;
	/// <br/>public const string HELP_INFO: help information for this command;
	/// <br/>public const int REQUIRED_PRIVILEGE: the level of privilege needed to invoke this command;
	/// </summary>
	public interface ICommand {

		// When creating a new comand class with this interface, the following fields are optional.

		// If INVOKE_STRING is not defined, it can be defined when registering the command class withing the Terminal instance,
		// or else it will be registered with the actual string that represents the class name.
		//public const string INVOKE_STRING;

		// If REQUIRED_PRIVILEGE is not define, the command can be executed by any privilege level of the Terminal instance.
		//public const int REQUIRED_PRIVILEGE;

		// If HELP_INFO is not defined, the command will not have help information for when the command is called with the '?' argument.
		//public const string HELP_INFO;

		string MainFunction( Terminal t, Arg[] args );
	}
}
