using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApp1 {
	public class KeyGrabber {

		static byte[] _keyboardState = new byte[256];
		readonly static StringBuilder sb = new StringBuilder( 2 );


		public enum MapType : uint {
			MAPVK_VK_TO_VSC = 0x0,
			MAPVK_VSC_TO_VK = 0x1,
			MAPVK_VK_TO_CHAR = 0x2,
			MAPVK_VSC_TO_VK_EX = 0x3,
		}




		[DllImport( "user32.dll" )]
		public static extern int ToUnicode(
			 uint wVirtKey,
			 uint wScanCode,
			 byte[] lpKeyState,
			 [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
				StringBuilder pwszBuff,
			 int cchBuff,
			 uint wFlags
		);




		[DllImport( "user32.dll" )]
		public static extern bool GetKeyboardState( byte[] lpKeyState );




		[DllImport( "user32.dll" )]
		public static extern uint MapVirtualKey( uint uCode, MapType uMapType );




		public static char GetCharFromKey( Key key ) {
			char ch = (char)0;

			int virtualKey = KeyInterop.VirtualKeyFromKey( key );
			GetKeyboardState( _keyboardState );
			//if (key == Key.A) File.WriteAllBytes( "../bytesA.bin", _keyboardState );
			//if (key == Key.Oem7) File.WriteAllBytes( "../bytes'.bin", _keyboardState );			if (key == Key.A) File.WriteAllBytes( "../bytesA.bin", _keyboardState );
			//if (key == Key.A) _keyboardState = File.ReadAllBytes( "../o-bytesA.bin" );
			//if (key == Key.Oem7) _keyboardState = File.ReadAllBytes( "../o-bytes'.bin" );
			uint scanCode = MapVirtualKey( (uint)virtualKey, MapType.MAPVK_VK_TO_VSC );
			sb.Clear();

			int result = ToUnicode( (uint)virtualKey, scanCode, _keyboardState, sb, sb.Capacity, 0 );
			switch (result) {
				case -1:
					break;
				case 0:
					break;
				case 1: {
					ch = sb[0];
					break;
				}
				default: {
					ch = sb[0];
					break;
				}
			}
			return ch;
		}
	}
}
