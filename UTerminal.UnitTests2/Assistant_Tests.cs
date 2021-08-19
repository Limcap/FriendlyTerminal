using Limcap.UTerminal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Limcap.UTerminal.UnitTests {
	[TestClass]
	public class Assistant_Tests {

		public Assistant assistant;
		public string[] inputs;
		public Type type;


		#region SIMULATE ASSISTANT CLASS MEMBERS
		#endregion
		//Node _invalidCmdNode => type.GetField( "_invalidCmdNode", BindingFlags.Static | BindingFlags.NonPublic ).As( f => f.GetValue( null ) as Node );
		dynamic _invalidCmdNode => assistant.NonPublicStaticField<Assistant,Node>("_invalidCmdNode");
		
		
		//Node _startNode => type.GetField( "_startNode", BindingFlags.Static | BindingFlags.NonPublic ).GetValue( assistant ) as Node;
		dynamic _startNode => assistant.NonPublicField("_startNode");
		
		
		//Node _confirmedNode => type.GetField( "_confirmedNode", BindingFlags.Static | BindingFlags.NonPublic ).GetValue( assistant ) as Node;
		public Node _confirmedNode = new Node();
	
		
		//List<Node> _predictedNodes => type.GetField( "_predictedNodes", BindingFlags.Static | BindingFlags.NonPublic ).GetValue( assistant ) as List<Node>;
		public List<Node> _predictedNodes = new List<Node>();
	
		
		public void AddCommand( string invokeTerm ) =>
			type.GetMethod( "AddCommand", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof( string ) }, null )
			.Invoke( assistant, new object[] { invokeTerm } );
		
		
		public void ProcessCommandInput( dynamic inpCmd, dynamic initialNode, dynamic result1, dynamic result2 ) {
			var method = assistant.NonPublicStaticMethod( "ProcessCommandInput", typeof( PString ), typeof( Node ), typeof( Node ).MakeByRefType(), typeof( List<Node> ).MakeByRefType() );
			method.Invoke( assistant, new object[] { inpCmd, initialNode, result1, result2 } );
		}
		//void ProcessCommandInput( PString inpCmd, Node initialNode, ref Node result1, ref List<Node> result2 ) =>
		//	type.GetMethod( "ProcessCommandInput", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof( PString ), typeof( Node ), typeof( Node ).MakeByRefType(), typeof( List<Node> ).MakeByRefType() }, null )
		//	.Invoke( assistant, new object[] { inpCmd, initialNode, result1, result2 } );


		#region TESTS
		#endregion
		//[TestInitialize]
		//public void Initialize() {
		//	//var a = new PrivateObject( assistant, "ProcessCommandInput" );
		//	type = typeof( Assistant );
		//	var source = new Dictionary<string, Type>() {
		//		["ajuda"] = null,
		//		["hello world"] = null,
		//	};
		//	assistant = new Assistant( source, "ptbr" );
		//	//AddCommand( "Hello World" );
		//	//AddCommand( "ajuda" );
		//}




		[TestMethod]
		[DataRow( "", 1, "@" )]
		[DataRow( "a", 2, "@" )]
		[DataRow( "aju", 3, "@" )]
		[DataRow( "aju ", 4, "@" )]
		[DataRow( "ajuda", 5, "@" )]
		[DataRow( "ajuda ", 6, "ajuda" )]
		[DataRow( "ajuda :", 7, "ajuda" )]
		[DataRow( "ajuda:  ", 8, "ajuda" )]
		public void ProcessCommandInput_CorrectPredictedNodes( PString input, int index, string result1 ) {
			var source = new Dictionary<string, Type>() {
				["ajuda"] = null,
				["hello world"] = null,
			};
			assistant = new Assistant( source, "ptbr" );
			ProcessCommandInput( input, _startNode, _confirmedNode, _predictedNodes );
			Assert.AreEqual( _confirmedNode.word, result1 );
		}
	}





	public static class ExtensionsForTest {
		public static MethodInfo NonPublicMethod<T>( this T me, string name, params Type[] args ) {
			return me.GetType().GetMethod( name, BindingFlags.Instance | BindingFlags.NonPublic, null, args, null );
		}
		
		public static MethodInfo NonPublicStaticMethod<T>( this T me, string name, params Type[] args ) {
			var ms = me.GetType().GetMethods( BindingFlags.Static | BindingFlags.NonPublic );
			foreach (var m in ms) { if (m.Name == name) return m; }
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
			var value = field?.GetValue( me );
			if (value is null) return default( R ); else return (R)value;
		}
	}
}
