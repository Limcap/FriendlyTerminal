namespace Limcap.UTerminal {
	public abstract partial class ACommand {
		public struct Parameter {
			public Parameter( Tstring name, Type type, bool optional, Tstring description ) {
				this.name = name;
				this.type = type;
				this.optional = optional;
				this.description = description;
			}
			public Tstring name;
			public Type type;
			public bool optional;
			public Tstring description;








			public enum Type { ANY, ALPHANUMERIC, LETTERS, CHARACTER, NUMBER, INTEGER, DATE, TIME, DATETIME, EMAIL }
		}
	}
}
