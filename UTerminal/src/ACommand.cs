using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.UTerminal {
	public abstract class ACommand : ICommand {

		public static string INVOKE_STRING = null;
		public virtual int RequiredPrivilege { get => 0; }
		public static string HELP_INFO = "This command does not have help info.";
		public abstract string MainFunction( Terminal t, Args args );
	}

	public struct Param {
		public Param( string name, ParamType type, bool optional, string description ) {
			this.name = name;
			this.type = type;
			this.description = description;
			this.optional = optional;
		}
		public string name;
		public ParamType type;
		public string description;
		public bool optional;
	}

	public enum ParamType { ANY, ALPHANUMERIC, LETTERS, CHARACTER, NUMBER, INTEGER, DATE, TIME, DATETIME, EMAIL }

	public struct Meta {
		public readonly string locale;
		//public const string invokeString = "help;[PT_BR]:ajuda";
		//public readonly string description;
		//public readonly int requiredPrivilege;
		public readonly Param[] parameters;
		//public Func<string, string> main;
	}
}
