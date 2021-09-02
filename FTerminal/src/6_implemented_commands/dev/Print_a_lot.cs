using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Limcap.FriendlyTerminal.Cmds.Dev {
	public class Print_a_lot : ACommand {

		public const string INVOKE_TEXT = "dev, print-a-lot";
		public const int REQUIRED_PRIVILEGE = 0;
		


		
		protected override string DescriptionBuilder() => "Very long repetitive text.";




		protected override Parameter[] ParametersBuilder() => new[] {
			new Parameter("length", Parameter.Type.INTEGER, true, "Length of the text in characters.")
		};




		public Print_a_lot( string locale ) : base( locale ) {}




		public override string MainFunction( Terminal t, Arg[] args ) {
			int length = args.Get( Parameters[0] ).SafeParseInt() ?? 1000;
			//var sb = new StringBuilder();
			for (int i = 0; i < length; i = i + loopText2.Length) {
				t.TypeText( loopText2 );
				//sb.Append( loopText2 );
			}
			return null;
			//return sb.ToString();
		}




		public const string loopText1 = "\nthis is a hundred characters long string, with the solo purpose of testing long strings. the end...";
		public const string loopText2 = "\nthe quick brown fox jumped over the lazy dog while chasing a shining dragonfly";
	}
}