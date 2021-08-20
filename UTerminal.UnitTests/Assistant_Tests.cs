using Limcap.UTerminal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Limcap.UTerminal.UnitTests {
	[TestClass]
	public class Assistant_Tests {

		static Assistant_TestInterface a;
		static Dictionary<string, Type> source;



		static Assistant_Tests() {
			source = new Dictionary<string, Type>() {
				["help"] = null,
				["hello world"] = null,
			};
			a = new Assistant_TestInterface( source, "ptbr" );
			a._startNode.word = "@";
		}




		#region TESTS
		#endregion
		[TestInitialize]
		public void Initialize() {
			a.Reset();
		}




		[TestMethod]
		[DataRow( 1, "",          "",       null )]
		[DataRow( 2, "h",         "h",      null )]
		[DataRow( 3, "he",        "he",     null )]
		[DataRow( 4, "he ",       "he ",    null )]
		[DataRow( 5, "help",      "help",   null )]
		[DataRow( 6, "help:",     "help:",  ""   )]
		[DataRow( 7, "help  ",    "help  ", null )]
		[DataRow( 8, "help  :",   "help:",  ""   )]
		[DataRow( 9, "help:   ",  "help:",  ""   )]
		[DataRow( 10, "help :  ", "help:",  ""   )]
		public void SplitInput_FixInput( int index, string input, string x_cmd, string x_args ) {
			var (cmd,args) = a.SplitInput( input );
			a.FixInput( ref cmd, ref args );
			Assert.AreEqual( x_cmd, cmd );
			Assert.AreEqual( x_args, args );
		}




		[TestMethod]
		[DataRow( 1, "",          "@"     )]
		[DataRow( 2, "h",         "@"     )]
		[DataRow( 3, "he",        "@"     )]
		[DataRow( 4, "he ",       "@"     )]
		[DataRow( 5, "help",      "@"     )]
		[DataRow( 6, "help:",     "help:" )]
		[DataRow( 7, "help  ",    "@"     )]
		[DataRow( 8, "help  :",   "help:" )]
		[DataRow( 9, "help:   ",  "help:" )]
		[DataRow( 10, "help :  ", "help:" )]
		// Cases 9 and 10 would not pass the test if input did not get pre-processed by
		// the methods SpliInput and FixInpup.
		public void ProcessCommandInput_CorrectPredictedNodes( int index, string input, string expected ) {
			var _confirmedNode = a._confirmedNode;
			var _predictedNodes = a._predictedNodes;
			var (cmd, args) = a.SplitInput( input );
			a.FixInput( ref cmd, ref args );
			a.ProcessCommandInput( cmd, a._startNode, ref _confirmedNode, ref _predictedNodes );
			Assert.AreEqual( expected, _confirmedNode.word );
		}
	}
}
