using Microsoft.VisualStudio.TestTools.UnitTesting;
using Limcap.UTerminal;

namespace Limcap.UTerminal.UnitTests {
	[TestClass]
	public class PString_Slicer_Tests {
		

		[TestMethod]
		public void NextSlice_SeparatorAtMiddle() {
			PString input = "string1,string2";
			var slicer = input.GetSlicer( ',' );
			var a = slicer.NextSlice();
			var b = slicer.NextSlice();
			var c = slicer.NextSlice();

			Assert.IsTrue( !a.IsNull );
			Assert.IsTrue( !a.IsNullOrEmpty );
			Assert.IsTrue( !a.IsEmpty );
			Assert.IsTrue( a == "string1" );

			Assert.IsTrue( !b.IsNull );
			Assert.IsTrue( !b.IsNullOrEmpty );
			Assert.IsTrue( !b.IsEmpty );
			Assert.IsTrue( b == "string2" );
			
			Assert.IsTrue( c.IsNull );
			Assert.IsTrue( c.IsNullOrEmpty );
			Assert.IsTrue( !c.IsEmpty );
			Assert.IsTrue( c == null );
		}

		[TestMethod]
		public void NextSlice_SeparatorAtEnd() {
			PString input = "string1,";
			var slicer = input.GetSlicer( ',' );
			
			var a = slicer.NextSlice();
			var b = slicer.NextSlice();
			var c = slicer.NextSlice();

			Assert.IsTrue( !a.IsNull );
			Assert.IsTrue( !a.IsNullOrEmpty );
			Assert.IsTrue( !a.IsEmpty );
			Assert.IsTrue( a == "string1" );
			
			Assert.IsTrue( !b.IsNull );
			Assert.IsTrue( b.IsNullOrEmpty );
			Assert.IsTrue( b.IsEmpty );
			Assert.IsTrue( b == string.Empty );
			Assert.IsTrue( b == "" );

			Assert.IsTrue( c.IsNull );
			Assert.IsTrue( c.IsNullOrEmpty );
			Assert.IsTrue( !c.IsEmpty );
			Assert.IsTrue( c == null );
		}

		[TestMethod]
		public void NextSlice_SeparatorAtStart() {
			PString input = ",string1";
			var slicer = input.GetSlicer( ',' );

			var a = slicer.NextSlice();
			var b = slicer.NextSlice();
			var c = slicer.NextSlice();
			
			Assert.IsTrue( !a.IsNull );
			Assert.IsTrue( a.IsNullOrEmpty );
			Assert.IsTrue( a.IsEmpty );
			Assert.IsTrue( a == string.Empty );
			Assert.IsTrue( a == "" );

			Assert.IsTrue( !b.IsNull );
			Assert.IsTrue( !b.IsNullOrEmpty );
			Assert.IsTrue( !b.IsEmpty );
			Assert.IsTrue( b == "string1" );
			
			Assert.IsTrue( c.IsNull );
			Assert.IsTrue( c.IsNullOrEmpty );
			Assert.IsTrue( !c.IsEmpty );
			Assert.IsTrue( c == null );
		}

		[TestMethod]
		public void NextSlice_SeparatorAtEnd_IncludeSeparatorAtEnd() {
			PString input = "string1,";
			var slicer = input.GetSlicer( ',' );

			var a = slicer.NextSlice( PString.Slicer.Mode.IncludeSeparatorAtEnd );
			var b = slicer.NextSlice( PString.Slicer.Mode.IncludeSeparatorAtEnd );
			var c = slicer.NextSlice( PString.Slicer.Mode.IncludeSeparatorAtEnd );

			Assert.IsTrue( !a.IsNull );
			Assert.IsTrue( !a.IsNullOrEmpty );
			Assert.IsTrue( !a.IsEmpty );
			Assert.IsTrue( a == "string1," );

			Assert.IsTrue( !b.IsNull );
			Assert.IsTrue( b.IsNullOrEmpty );
			Assert.IsTrue( b.IsEmpty );
			Assert.IsTrue( b == string.Empty );
			Assert.IsTrue( b == "" );

			Assert.IsTrue( c.IsNull );
			Assert.IsTrue( c.IsNullOrEmpty );
			Assert.IsTrue( !c.IsEmpty );
			Assert.IsTrue( c == null );
		}

		[TestMethod]
		public void NextSlice_SeparatorAtStart_IncludeSeparatorAtStart() {
			PString input = ",string1";
			var slicer = input.GetSlicer( ',' );

			var a = slicer.NextSlice( PString.Slicer.Mode.IncludeSeparatorAtStart );
			var b = slicer.NextSlice( PString.Slicer.Mode.IncludeSeparatorAtStart );
			var c = slicer.NextSlice( PString.Slicer.Mode.IncludeSeparatorAtStart );

			Assert.IsTrue( !a.IsNull );
			Assert.IsTrue( a.IsNullOrEmpty );
			Assert.IsTrue( a.IsEmpty );
			Assert.IsTrue( a == string.Empty );
			Assert.IsTrue( a == "" );

			Assert.IsTrue( !b.IsNull );
			Assert.IsTrue( !b.IsNullOrEmpty );
			Assert.IsTrue( !b.IsEmpty );
			Assert.IsTrue( b == ",string1" );

			Assert.IsTrue( c.IsNull );
			Assert.IsTrue( c.IsNullOrEmpty );
			Assert.IsTrue( !c.IsEmpty );
			Assert.IsTrue( c == null );
		}
	}
}
