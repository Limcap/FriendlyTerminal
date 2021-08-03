using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.TextboxTerminal {
	public interface ICommand {
		//private Terminal terminal { get; set; }
		string HelpInfo { get; }
		string MainFunction( Terminal t, Args args );
	}
}
