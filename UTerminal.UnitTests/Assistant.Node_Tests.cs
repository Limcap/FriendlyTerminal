using Limcap.UTerminal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Limcap.UTerminal.UnitTests {
	[TestClass]
	public class Assistant_Node_Tests {

		static Assistant.Node start = new Assistant.Node() { word = "@" };
		static Dictionary<string, Type> source;




		public Assistant_Node_Tests() {
			source = new Dictionary<string, Type>() {
				["it's my house"] = null,
				["it's my book"] = null,
				["it's not his car"] = null,
				["it's not her shoe"] = null,
				["that's my bread"] = null,
				["that's your lunch "] = null,
				["that's my brother"] = null,

			};
			start = new Assistant.Node() { word = "@" };

			Stopwatch sw = new Stopwatch();
			sw.Start();
			var memoryBefore = GC.GetTotalMemory( true );
			Assistant.Node.BuildTree( source.Keys.ToList(), ' ', ":", start );
			var memoryAfter = GC.GetTotalMemory( true );
			var memoryOccupied = memoryAfter - memoryBefore;
			sw.Stop();
			Trace.WriteLine( "memory occupied by tree: " + memoryOccupied );
			Trace.WriteLine( "time taken to build tree: " + sw.ElapsedMilliseconds );
		}




		//public static void BuildTree( List<string> sentences, char separator, string terminator ) {
		//	foreach (var term in sentences) {
		//		var words = term.Split( separator );
		//		var node = start;
		//		for (int i = 0; i < words.Length; i++) {
		//			// skips words that have length 0, cause when there are multiple spaces between the words.
		//			if (words[i].Length == 0) continue;

		//			if (i < words.Length - 1) {
		//				node = node.AddIfNotPresent( words[i] + separator );
		//			}
		//			else {
		//				node = node.AddIfNotPresent( words[i] );
		//				node.AddIfNotPresent( terminator );
		//			}
		//		}
		//	}
		//}




		//public unsafe static void BuildTree2( List<string> sentences, char separator, string terminator ) {
		//	foreach (var term in sentences) {
		//		var words = ((PString)term).GetSlicer( ' ', PString.Slicer.Mode.IncludeSeparatorAtEnd );
		//		var node = start;

		//		while (words.HasNext) {
		//			var word = words.Next();
		//			if (word.len == 0) continue;
		//			node = node.AddIfNotPresent( new string(word.ptr,0,word.len) );
		//			if (!words.HasNext) node.AddIfNotPresent( terminator );
		//		}
		//	}
		//}


		public void FixTrailingSpace( ref PString ps ) {
			if (ps.len > 1 && ps[ps.len - 1] == ':') {
				while (ps[ps.len - 2] == ' ') {
					ps[ps.len - 2] = ':';
					ps[ps.len - 1] = ' ';
					ps.len--;
				}
			}
		}




		[TestInitialize]
		public void Initialize() {
		}
		
		
		
		
		#region TESTS
		#endregion




		[TestMethod]
		[DataRow( 11, "", "@" )]
		[DataRow( 12, "it", "@" )]
		[DataRow( 13, "it's", "@" )]
		[DataRow( 14, "it's ", "it's " )]
		[DataRow( 15, "it's m", "it's " )]
		[DataRow( 16, "it's m ", "it's " )]
		[DataRow( 17, "it's my", "it's " )]
		[DataRow( 18, "it's my ", "my " )]
		[DataRow( 19, "it's my bo", "my " )]
		[DataRow( 20, "it's my book", "book" )]
		[DataRow( 21, "it's my book:", ":" )]
		[DataRow( 22, "it's my book ", "book" )]
		[DataRow( 23, "it's my book :", ":" )]
		public void Traverse_ReachCorrectNode( int index, string input, string expected ) {
			var inp = (PString)input;
			Assistant_TestInterface.FixCommandPartTermination( ref inp );
			var n = start.Traverse( ref inp );
			Assert.AreEqual( expected, n.word );
		}
	}
}
