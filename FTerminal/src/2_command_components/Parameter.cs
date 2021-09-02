using System;
using System.Collections.Generic;
using System.Linq;

namespace Limcap.FriendlyTerminal {
	public class Parameter {
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







	public partial class Ext {
		public static List<Parameter> GetByNamePrefix(
			this Parameter[] paramArray,
			ReadOnlySpan<char> name,
			List<Parameter> listToFill = null
		) {
			if (listToFill is null) listToFill = new List<Parameter>( paramArray.Length );
			foreach (var p in paramArray) {
				if (p.name.StartsWith( name )) {
					listToFill.Add( p );
				}
			}
			return listToFill;
		}




		public static List<Parameter> GetByNamePrefix(
			this IEnumerable<Parameter> paramArray,
			PString name,
			List<Parameter> listToFill = null
		) {
			if (listToFill is null) listToFill = new List<Parameter>( paramArray.Count() );
			foreach (var p in paramArray) {
				if (p.name.StartsWith( name )) {
					listToFill.Add( p );
				}
			}
			return listToFill;
		}




		public static int GetIndexByNamePrefix( this IEnumerable<Parameter> paramArray, PString name, int startIndex = 0 ) {
			for (int i = startIndex; i < paramArray.Count(); i++)
				if (paramArray.ElementAt( i ).name.StartsWith( name ))
					return i;
			return -1;
		}





		public static Parameter GetByName(
			this Parameter[] paramArray,
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
			this IEnumerable<Parameter> array,
			PString pString
			) {
			for (int i = 0; i < array.Count(); i++) {
				if (array.ElementAt( i ).name == pString) {
					return i;
				}
			}
			return -1;
		}
	}
}
