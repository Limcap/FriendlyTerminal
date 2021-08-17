using System;
using System.Collections.Generic;
using System.Linq;

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


		//public partial struct Parameter {
		//	public static List<Parameter> GetByNamePrefix( Parameter[] source, ReadOnlySpan<char> name, List<Parameter> listToFill = null ) {
		//		if (listToFill is null) listToFill = new List<Parameter>( source. Length );
		//		foreach (var p in source) {
		//			if (p.name.StartsWith( name )) {
		//				listToFill.Add( p );
		//			}
		//		}
		//		return listToFill;
		//	}
		//}
	}








	public partial class Extensions {
		public static List<ACommand.Parameter> GetByNamePrefix(
			this ACommand.Parameter[] paramArray,
			ReadOnlySpan<char> name,
			List<ACommand.Parameter> listToFill = null
		) {
			if (listToFill is null) listToFill = new List<ACommand.Parameter>( paramArray.Length );
			foreach (var p in paramArray) {
				if (p.name.StartsWith( name )) {
					listToFill.Add( p );
				}
			}
			return listToFill;
		}




		public static List<ACommand.Parameter> GetByNamePrefix(
			this IEnumerable<ACommand.Parameter> paramArray,
			PString name,
			List<ACommand.Parameter> listToFill = null
		) {
			if (listToFill is null) listToFill = new List<ACommand.Parameter>( paramArray.Count() );
			foreach (var p in paramArray) {
				if (p.name.StartsWith( name )) {
					listToFill.Add( p );
				}
			}
			return listToFill;
		}




		public static ACommand.Parameter? GetByName(
			this ACommand.Parameter[] paramArray,
			ReadOnlySpan<char> name
		) {
			foreach (var p in paramArray) {
				if (p.name.AsSpan().Equals( name, StringComparison.OrdinalIgnoreCase )) {
					return p;
				}
			}
			return null;
		}

		public static int GetIndexByName(
			this List<ACommand.Parameter> array,
			PString pString
			) {
			for (int i=0; i<array.Count; i++) {
				if (array[i].name == pString ) {
					return i;
				}
			}
			return -1;
		}
	}
}
