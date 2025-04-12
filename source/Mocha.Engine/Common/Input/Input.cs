using SDL2;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Bango.Common
{
	public static partial class Input
	{
		private static InputSnapshot PreviousSnapshot;
		private static InputSnapshot Snapshot;

		public static Vector2 MouseDelta => Snapshot.MouseDelta / Screen.DpiScale;
		public static Vector2 MousePosition => Snapshot.MousePosition / Screen.DpiScale;

		public static bool MouseLeftPressed => Snapshot.MouseLeft && !PreviousSnapshot.MouseLeft;
		public static bool MouseLeftDown => Snapshot.MouseLeft;
		public static bool MouseRightPressed => Snapshot.MouseRight && !PreviousSnapshot.MouseRight;
		public static bool MouseRightDown => Snapshot.MouseRight;

		private enum SDL_HitTestResult
		{
			Normal = 0,
			Draggable = 1,
			ResizeTopLeft,
			ResizeTop,
			ResizeTopRight,
			ResizeRight,
			ResizeBottomRight,
			ResizeBottom,
			ResizeBottomLeft,
			ResizeLeft
		}

		private delegate SDL_HitTestResult SDL_HitTestCallback( IntPtr window, IntPtr area, IntPtr data );

		private static SDL_HitTestCallback hitTestCallback = new SDL_HitTestCallback( HitTest );

		public static void SetHitTest( IntPtr sdlWindow )
		{
			if ( SDL_SetWindowHitTest( sdlWindow, hitTestCallback, IntPtr.Zero ) != 0 )
			{
				Console.WriteLine( "SDL_SetWindowHitTest failed: " + PtrToStringAnsi( SDL_GetError() ) );
			}
		}

		[DllImport( "SDL2", CallingConvention = CallingConvention.Cdecl )]
		private static extern int SDL_SetWindowHitTest( IntPtr window, SDL_HitTestCallback callback, IntPtr callbackData );

		[DllImport( "SDL2", CallingConvention = CallingConvention.Cdecl )]
		private static extern IntPtr SDL_GetError();

		private static string PtrToStringAnsi( IntPtr ptr )
		{
			return Marshal.PtrToStringAnsi( ptr );
		}

		struct SDL_Point
		{
			public int x;
			public int y;
		}

		private static SDL_HitTestResult HitTest( IntPtr window, IntPtr area, IntPtr data )
		{
			var point = Marshal.PtrToStructure<SDL_Point>( area );

			int width = (int)Screen.Size.X;
			int height = (int)Screen.Size.Y;
			int borderSize = 8;

			if ( point.y < borderSize )
			{
				if ( point.x < borderSize )
				{
					return SDL_HitTestResult.ResizeTopLeft;
				}
				else if ( point.x > width - borderSize )
				{
					return SDL_HitTestResult.ResizeTopRight;
				}
				else
				{
					return SDL_HitTestResult.ResizeTop;
				}
			}
			else if ( point.y > height - borderSize )
			{
				if ( point.x < borderSize )
				{
					return SDL_HitTestResult.ResizeBottomLeft;
				}
				else if ( point.x > width - borderSize )
				{
					return SDL_HitTestResult.ResizeBottomRight;
				}
				else
				{
					return SDL_HitTestResult.ResizeBottom;
				}
			}
			else if ( point.x < borderSize )
			{
				return SDL_HitTestResult.ResizeLeft;
			}
			else if ( point.x > width - borderSize )
			{
				return SDL_HitTestResult.ResizeRight;
			}
			else if ( point.y < 30 )
			{
				if ( point.x < Screen.Size.X - 143 )
				{
					return SDL_HitTestResult.Draggable;
				}
			}

			return SDL_HitTestResult.Normal;
		}

		public struct UserData
		{
			public Action RenderAction;
		}

		public static unsafe int ExposeEventWatcher( IntPtr pUserData, IntPtr pSdlEvent )
		{
			var ev = Marshal.PtrToStructure<SDL.SDL_Event>( pSdlEvent );
			var userData = Marshal.PtrToStructure<UserData>( pUserData );

			if ( ev.type == SDL.SDL_EventType.SDL_WINDOWEVENT )
			{
				SDL.SDL_WindowEvent we = Unsafe.Read<SDL.SDL_WindowEvent>( &ev );

				if ( we.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_EXPOSED )
				{
					userData.RenderAction.Invoke();
				}
			}

			return 0;
		}

		// Define a minimal MSG struct to work with Windows messages.
		[StructLayout( LayoutKind.Sequential )]
		public struct MSG
		{
			public IntPtr hwnd;
			public uint message;
			public UIntPtr wParam;
			public IntPtr lParam;
			public uint time;
			public POINT pt;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct POINT
		{
			public int x;
			public int y;
		}

		public static unsafe IntPtr WindowsMessageHook( IntPtr userdata, IntPtr hWnd, uint message, ulong wParam, long lParam )
		{
			const uint WM_NCPAINT = 0x0014;

			if ( message == WM_NCPAINT )
			{
				return IntPtr.Zero;
			}

			return (IntPtr)1;
		}

		public static unsafe void Update()
		{
			PreviousSnapshot = Snapshot;
			Snapshot.KeysDown = new List<SDL.SDL_Keysym>();
			Snapshot.MouseDelta = Vector2.Zero;
			Snapshot.WheelDelta = 0;

			SDL.SDL_Event e;
			while ( SDL.SDL_PollEvent( out e ) != 0 )
			{
				switch ( e.type )
				{
					case SDL.SDL_EventType.SDL_MOUSEMOTION:
						SDL.SDL_MouseMotionEvent mme = Unsafe.Read<SDL.SDL_MouseMotionEvent>( &e );
						Snapshot.MouseDelta = new Vector2( mme.xrel, mme.yrel );
						Snapshot.MousePosition = new Vector2( mme.x, mme.y );
						break;

					case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
					case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
						SDL.SDL_MouseButtonEvent mbe = Unsafe.Read<SDL.SDL_MouseButtonEvent>( &e );
						bool isButtonDown = (mbe.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN);

						switch ( mbe.button )
						{
							case (byte)SDL.SDL_BUTTON_LEFT:
								Snapshot.MouseLeft = isButtonDown;
								break;
							case (byte)SDL.SDL_BUTTON_RIGHT:
								Snapshot.MouseRight = isButtonDown;
								break;
							default:
								break;
						}

						break;

					case SDL.SDL_EventType.SDL_MOUSEWHEEL:
						SDL.SDL_MouseWheelEvent mwe = Unsafe.Read<SDL.SDL_MouseWheelEvent>( &e );
						Snapshot.WheelDelta = mwe.y;
						break;

					case SDL.SDL_EventType.SDL_KEYDOWN:
					case SDL.SDL_EventType.SDL_KEYUP:
						SDL.SDL_KeyboardEvent kbe = Unsafe.Read<SDL.SDL_KeyboardEvent>( &e );
						bool isKeyDown = (kbe.type == SDL.SDL_EventType.SDL_KEYDOWN);

						if ( isKeyDown )
						{
							if ( !Snapshot.KeysDown.Any( x => x.sym == kbe.keysym.sym ) )
								Snapshot.KeysDown.Add( kbe.keysym );
						}
						else
						{
							Snapshot.KeysDown.RemoveAll( x => x.sym == kbe.keysym.sym );
						}

						break;

					//case SDL_EventType.TextInput:
					//	SDL_TextInputEvent tie = Unsafe.Read<SDL_TextInputEvent>( &e );

					//	uint byteCount = 0;
					//	// Loop until the null terminator is found or maximum size is reached.
					//	while ( byteCount < SDL_TextInputEvent.MaxTextSize && tie.text[byteCount++] != 0 )
					//	{
					//	}

					//	if ( byteCount > 1 )
					//	{
					//		// Exclude the null terminator.
					//		byteCount -= 1;
					//		int charCount = Encoding.UTF8.GetCharCount( tie.text, (int)byteCount );
					//		char* charsPtr = stackalloc char[charCount];
					//		Encoding.UTF8.GetChars( tie.text, (int)byteCount, charsPtr, charCount );
					//		for ( int i = 0; i < charCount; i++ )
					//		{
					//			keyCharPresses.Add( charsPtr[i] );
					//		}
					//	}
					//	break;

					case SDL.SDL_EventType.SDL_WINDOWEVENT:
						SDL.SDL_WindowEvent we = Unsafe.Read<SDL.SDL_WindowEvent>( &e );
						switch ( we.windowEvent )
						{
							case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
							case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
								var newSize = new Point2( we.data1, we.data2 );
								Screen.UpdateFrom( newSize );
								Event.Run( Event.Window.ResizedAttribute.Name, newSize );
								break;

							case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
								// Handle a window close event.
								Environment.Exit( 0 );
								break;

							default:
								break;
						}
						break;

				}
			}
		}
	}
}
