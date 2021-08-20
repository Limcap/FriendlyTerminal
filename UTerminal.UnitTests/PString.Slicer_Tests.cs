using Microsoft.VisualStudio.TestTools.UnitTesting;
using Limcap.UTerminal;

namespace Limcap.UTerminal.UnitTests {
	[TestClass]
	public class PString_Slicer_Tests {

		[TestMethod]
		[DataRow( 1, "",    false, true,   "",   null )]
		[DataRow( 2, " ",   false, false,  " ",  null )]
		[DataRow( 3, ",",   false, true,   "",   ""   )]
		[DataRow( 4, ", ",  false, true,   "",   " "  )]
		[DataRow( 5, " ,",  false, false,  " ",  ""   )]
		[DataRow( 6, "a,",  false, false,  "a",  ""   )]
		[DataRow( 7, "a,b", false, false,  "a",  "b"  )]
		[DataRow( 8, ",b",  false, true,   "",   "b"  )]
		[DataRow( 9, " , ", false, false,  " ",  " "  )]
		public void NextSlice_ExcludeSeparator( int index, string input, bool aIsNull, bool aIsEmpty, string xa, string xb ) {
			var slicer = ((PString)input).GetSlicer( ',' );
			var a = slicer .Next( PString.Slicer.Mode.ExcludeSeparator );
			var b = slicer.Next( PString.Slicer.Mode.ExcludeSeparator );
			var c = slicer.Next( PString.Slicer.Mode.ExcludeSeparator );

			Assert.IsTrue( a.IsNull == aIsNull, $"IsNull => expected: {aIsNull}, actual: {a.IsNull}" );
			Assert.IsTrue( a.IsEmpty == aIsEmpty, $"IsEmpty => expected: {aIsEmpty}, actual: {a.IsEmpty}" );
			Assert.AreEqual( a, xa );
			Assert.AreEqual( b, xb );
		}




		[TestMethod]
		[DataRow( 1, "",    false, true,   "",   null )]
		[DataRow( 2, " ",   false, false,  " ",  null )]
		[DataRow( 3, ",",   false, true,   "",   ","  )]
		[DataRow( 4, ", ",  false, true,   "",   ", " )]
		[DataRow( 5, " ,",  false, false,  " ",  ","  )]
		[DataRow( 6, "a,",  false, false,  "a",  ","  )]
		[DataRow( 7, "a,b", false, false,  "a",  ",b" )]
		[DataRow( 8, ",b",  false, true,   "",   ",b" )]
		[DataRow( 9, " , ", false, false,  " ",  ", " )]
		public void NextSlice_IncludeSeparatorAtStart( int index, string input, bool aIsNull, bool aIsEmpty, string xa, string xb ) {
			var slicer = ((PString)input).GetSlicer( ',' );
			var a = slicer .Next( PString.Slicer.Mode.IncludeSeparatorAtStart );
			var b = slicer.Next( PString.Slicer.Mode.IncludeSeparatorAtStart );
			var c = slicer.Next( PString.Slicer.Mode.IncludeSeparatorAtStart );

			Assert.IsTrue( a.IsNull == aIsNull, $"IsNull => expected: {aIsNull}, actual: {a.IsNull}" );
			Assert.IsTrue( a.IsEmpty == aIsEmpty, $"IsEmpty => expected: {aIsEmpty}, actual: {a.IsEmpty}" );
			Assert.AreEqual( a, xa );
			Assert.AreEqual( b, xb );
		}




		[TestMethod]
		[DataRow( 1, "",    false, true,   "",    null )]
		[DataRow( 2, " ",   false, false,  " ",   null )]
		[DataRow( 3, ",",   false, false,  ",",   ""   )]
		[DataRow( 4, ", ",  false, false,  ",",   " "  )]
		[DataRow( 5, " ,",  false, false,  " ,",  ""   )]
		[DataRow( 6, "a,",  false, false,  "a,",  ""   )]
		[DataRow( 7, "a,b", false, false,  "a,",  "b"  )]
		[DataRow( 8, ",b",  false, false,  ",",   "b"  )]
		[DataRow( 9, " , ", false, false,  " ,",  " "  )]
		public void NextSlice_IncludeSeparatorAtEnd( int index, string input, bool aIsNull, bool aIsEmpty, string xa, string xb ) {
			var slicer = ((PString)input).GetSlicer( ',' );
			var a = slicer.Next( PString.Slicer.Mode.IncludeSeparatorAtEnd );
			var b = slicer.Next( PString.Slicer.Mode.IncludeSeparatorAtEnd );
			var c = slicer.Next( PString.Slicer.Mode.IncludeSeparatorAtEnd );

			Assert.IsTrue( a.IsNull == aIsNull, $"IsNull => expected: {aIsNull}, actual: {a.IsNull}" );
			Assert.IsTrue( a.IsEmpty == aIsEmpty, $"IsEmpty => expected: {aIsEmpty}, actual: {a.IsEmpty}" );
			Assert.AreEqual( a, xa );
			Assert.AreEqual( b, xb );
		}



		/*
		[TestMethod]
		public void NextSlice_SeparatorAtMiddle() {
			PString input = "string1,string2";
			var slicer = input.GetSlicer( ',' );
			var a = slicer.Next();
			var b = slicer.Next();
			var c = slicer.Next();

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
			
			var a = slicer.Next();
			var b = slicer.Next();
			var c = slicer.Next();

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

			var a = slicer.Next();
			var b = slicer.Next();
			var c = slicer.Next();
			
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

			var a = slicer.Next( PString.Slicer.Mode.IncludeSeparatorAtEnd );
			var b = slicer.Next( PString.Slicer.Mode.IncludeSeparatorAtEnd );
			var c = slicer.Next( PString.Slicer.Mode.IncludeSeparatorAtEnd );

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

			var a = slicer.Next( PString.Slicer.Mode.IncludeSeparatorAtStart );
			var b = slicer.Next( PString.Slicer.Mode.IncludeSeparatorAtStart );
			var c = slicer.Next( PString.Slicer.Mode.IncludeSeparatorAtStart );

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
		*/
	}
}
