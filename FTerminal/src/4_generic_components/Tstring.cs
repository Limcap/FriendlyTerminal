namespace Limcap.FriendlyTerminal {
	public struct Tstring {
		public Tstring( string idstr ) {
			if (idstr is null) {
				id = null;
				str = null;
			}
			else {
				var separatorIndex = idstr.IndexOf( "#" );
				id = (separatorIndex == -1) ? idstr : idstr.Remove( separatorIndex );
				str = (separatorIndex == -1) ? idstr : idstr.Substring( separatorIndex + 1 );
			}
		}
		public readonly string id;
		public readonly string str;
		public static implicit operator string( Tstring idstr ) => idstr.str;
		public static implicit operator Tstring( string idstr ) => new Tstring( idstr );
		public override string ToString() => this;
	}
}
