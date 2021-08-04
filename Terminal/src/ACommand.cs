using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.TextboxTerminal {
	public abstract class ACommand {

		public abstract string InvokeString { get; }
		public virtual int RequiredPrivilege { get => 0; }
		public abstract string HelpInfo { get; }
		public abstract string MainFunction( Terminal t, Args args );
	}
}
