using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Limcap.FriendlyTerminal.Cmds.Dev {
	public class InputTest : ACommand {

		public const string INVOKE_TEXT = "dev, input-test";
		public const int REQUIRED_PRIVILEGE = 0;
		


		
		protected override string DescriptionBuilder() => "Very long repetitive text.";




		protected override Parameter[] ParametersBuilder() => new[] {
			new Parameter("length", Parameter.Type.INTEGER, true, "Length of the text in characters.")
		};




		public InputTest( string locale ) : base( locale ) {}




		public override string MainFunction( Terminal t, Arg[] args ) {
			t.TypeText( "Input another text: " );
			t.ReadLine( Part2 );
			return null;
		}


		public string Part2( string input ) {
			return null;
		}
	}
}