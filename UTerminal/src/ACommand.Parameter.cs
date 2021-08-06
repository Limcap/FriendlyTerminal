namespace Limcap.UTerminal {
	public abstract partial class ACommand {
		public struct Parameter {
			public Parameter( string name, Type type, bool optional, string description ) {
				this.name = name;
				this.type = type;
				this.optional = optional;
				this.description = description;
			}
			public string name;
			public Type type;
			public bool optional;
			public string description;








			public enum Type { ANY, ALPHANUMERIC, LETTERS, CHARACTER, NUMBER, INTEGER, DATE, TIME, DATETIME, EMAIL, BOOLEAN }
		}
	}
}
