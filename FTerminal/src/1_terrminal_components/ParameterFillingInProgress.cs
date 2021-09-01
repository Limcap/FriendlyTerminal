using System;
using System.Runtime.Serialization;

namespace Limcap.FTerminal {
	[Serializable]
	internal class ParameterFillingInProgress : Exception {
		public ParameterFillingInProgress() {
		}

		public ParameterFillingInProgress( string message ) : base( message ) {
		}

		public ParameterFillingInProgress( string message, Exception innerException ) : base( message, innerException ) {
		}

		protected ParameterFillingInProgress( SerializationInfo info, StreamingContext context ) : base( info, context ) {
		}
	}
}