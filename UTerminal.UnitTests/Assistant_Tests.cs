using Limcap.UTerminal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Limcap.UTerminal.UnitTests {
	[TestClass]
	public class Assistant_Tests {

		public Assistant_Publinator a;

		#region TESTS
		#endregion
		[TestInitialize]
		public void Initialize() {
			var source = new Dictionary<string, Type>() {
				["ajuda"] = null,
				["hello world"] = null,
			};
			a = new Assistant_Publinator( source, "ptbr" );
		}




		[TestMethod]
		[DataRow( "", 1, "@" )]
		[DataRow( "a", 2, "@" )]
		[DataRow( "aju", 3, "@" )]
		[DataRow( "aju ", 4, "@" )]
		[DataRow( "ajuda", 5, "@" )]
		[DataRow( "ajuda ", 6, "ajuda" )]
		[DataRow( "ajuda :", 7, "ajuda" )]
		[DataRow( "ajuda:  ", 8, "ajuda" )]
		public void ProcessCommandInput_CorrectPredictedNodes( string input, int index, string result1 ) {
			var _confirmedNode = a._confirmedNode;
			var _predictedNodes = a._predictedNodes;
			a.ProcessCommandInput( input, a._startNode, ref _confirmedNode, ref _predictedNodes );
			Assert.AreEqual( _confirmedNode.word, result1 );
		}
	}





#nullable enable
	public static class ExtensionsForTest {
		public static MethodInfo? NonPublicMethod<T>( this T me, string name, params Type[] args ) {
			return me.GetType().GetMethod( name, BindingFlags.Instance | BindingFlags.NonPublic, null, args, null );
		}
		public static MethodInfo? NonPublicStaticMethod<T>( this T me, string name, params Type[] args ) {
			return me.GetType().GetMethod( name, BindingFlags.Static | BindingFlags.NonPublic, null, args, null );
		}
		public static object NonPublicField<T>( this T me, string name ) {
			var field = me.GetType().GetField( name, BindingFlags.Instance | BindingFlags.NonPublic );
			var value0 = field?.GetValue( me );
			dynamic value1;
			if (value0 is null)
				value1 = default( dynamic );
			else
				value1 = value0;
			//var value1 = value0 is null ? default( R ) : ((R)value0);
			return value1;
		}
		public static R NonPublicStaticField<T,R>( this T me, string name ) {
			var field = me.GetType().GetField( name, BindingFlags.Static | BindingFlags.NonPublic );
			var value = (R) field?.GetValue(me) ?? default(R);
			return value;
		}
	}
}
