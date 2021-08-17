using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Limcap.UTerminal {

	[DebuggerDisplay( "{len} elements <{typeof(T).Name,nq}>" )]
	public unsafe struct PArray<T> where T : struct {
		public void* ptr;
		public int len;
		



		public PArray( void* ptr, int len ) {
			this.ptr = ptr;
			this.len = len;
		}




		//public T this[int i] {
		//	get => Marshal.PtrToStructure<T>( new IntPtr( (byte*)ptr + i * Marshal.SizeOf( typeof( T ) ) ) );
		//	set {
		//		var size = Marshal.SizeOf( typeof( T ) );
		//		IntPtr p = new IntPtr( (byte*)ptr + i * size );
		//		var f = new Span<byte>( (byte*)ptr + i * size, 1 );
		//		void* g = Unsafe.AsPointer( ref value );
		//		f[0] = value;
		//		fixed (byte* g = &value) { }
		//	}
		//}
		//public void* this[int i] => (byte*)ptr + i * Marshal.SizeOf( typeof( T ) );




		public Span<T> AsSpan => new Span<T>( ptr, len );
		



		public T[] AsArray {
			get {
				var result = new T[len];
				var size = Marshal.SizeOf( typeof( T ) );
				IntPtr p = new IntPtr( (byte*)ptr );
				for (int i=0; i<len; i++) {
					//IntPtr p = new IntPtr( (byte*)ptr + (i * size) );
					result[i] = Marshal.PtrToStructure<T>( p );
					p += size;
				}
				return result;
			}
		}

		//public T[] AsArray1 => AsSpan.ToArray();

		//public T[] AsArray2 => ByteArrayToObject( ToByteArray() );




		public byte[] ToByteArray() {
			var size = Marshal.SizeOf( typeof( T ) );
			var bytePtr = (byte*)ptr;
			var byteArr = new byte[size];
			for (int i = 0; i < len * size; i++) byteArr[i] = bytePtr[i];
			return byteArr;
		}




		public T[] ByteArrayToObject( byte[] _ByteArray ) {
			try {
				// convert byte array to memory stream
				System.IO.MemoryStream _MemoryStream = new System.IO.MemoryStream( _ByteArray );
				// create new BinaryFormatter
				System.Runtime.Serialization.Formatters.Binary.BinaryFormatter _BinaryFormatter
							  = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				// set memory stream position to starting point
				_MemoryStream.Position = 0;
				// Deserializes a stream into an object graph and return as a object.
				return (T[]) _BinaryFormatter.Deserialize( _MemoryStream );
			}
			catch (Exception _Exception) {
				// Error
				Console.WriteLine( "Exception caught in process: {0}", _Exception.ToString() );
			}
			// Error occured, return null
			return default(T[]);
		}
	}
}
