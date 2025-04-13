using SDL2;
using System.Runtime.InteropServices;

namespace Bango;
public class SdlWindow
{
	public static SdlWindow Current { get; set; }
	public IntPtr SdlHandle { get; private set; }
	public IntPtr WindowHandle { get; private set; }
	public IntPtr InstanceHandle { get; private set; }
	public Point2 Size
	{
		get
		{
			SDL.SDL_GetWindowSize( SdlHandle, out var w, out var h );
			return new Point2( w, h );
		}
		set
		{
			SDL.SDL_SetWindowSize( SdlHandle, value.X, value.Y );
		}
	}

	public string Title
	{
		set
		{
			SDL.SDL_SetWindowTitle( SdlHandle, value );
		}
		get
		{
			return SDL.SDL_GetWindowTitle( SdlHandle );
		}
	}

	public bool Visible
	{
		set
		{
			if ( value )
			{
				SDL.SDL_ShowWindow( SdlHandle );
			}
			else
			{
				SDL.SDL_HideWindow( SdlHandle );
			}
		}
		get
		{
			return ((SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags( SdlHandle )).HasFlag( SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN );
		}
	}
	public bool Active { get; private set; } = true;
	public int Width => Size.X;
	public int Height => Size.Y;

	private IntPtr pUserData = IntPtr.Zero;
	private Input.UserData userData = new();

	public SdlWindow()
	{
		Current ??= this;

		var windowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
			| SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI
			| SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
			| SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;

		SdlHandle = SDL.SDL_CreateWindow( "Bango Demo", 128, 128, 400, 300, windowFlags );

		SDL.SDL_SetWindowResizable( SdlHandle, SDL.SDL_bool.SDL_TRUE );

		var wmInfo = new SDL.SDL_SysWMinfo();
		SDL.SDL_GetVersion( out wmInfo.version );
		SDL.SDL_GetWindowWMInfo( SdlHandle, ref wmInfo );

		userData = new Input.UserData
		{
			RenderAction = RendererInstance.Current.Render
		};

		pUserData = Marshal.AllocHGlobal( Marshal.SizeOf( userData ) );
		Marshal.StructureToPtr( userData, pUserData, false );
		SDL.SDL_AddEventWatch( Input.ExposeEventWatcher, pUserData );
		SDL.SDL_SetWindowsMessageHook( Input.WindowsMessageHook, IntPtr.Zero );
		SDL.SDL_SetHint( "SDL_BORDERLESS_RESIZABLE_STYLE", "1" );
		SDL.SDL_SetWindowBordered( SdlHandle, SDL.SDL_bool.SDL_FALSE );
		SDL.SDL_SetWindowMinimumSize( SdlHandle, 300, 30 );

		WindowHandle = wmInfo.info.win.window;
		InstanceHandle = wmInfo.info.win.hinstance;

		WinApi.ExtendFrame( WindowHandle, -1, 0, 0, 0 );
		Input.SetHitTest( SdlHandle );
		SetMica( WindowHandle );

		Screen.UpdateFrom( Size );
	}

	public void Minimize()
	{
		SDL.SDL_MinimizeWindow( SdlHandle );
	}

	public void Maximize()
	{
		var windowFlags = (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags( SdlHandle );

		if ( windowFlags.HasFlag( SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED ) )
		{
			SDL.SDL_RestoreWindow( SdlHandle );
		}
		else
		{
			SDL.SDL_MaximizeWindow( SdlHandle );
		}
	}

	public void Close()
	{
		SDL.SDL_Quit();
		Active = false;
	}

	// The DWMWA_USE_IMMERSIVE_DARK_MODE attribute.
	// Some sources use 20 for Windows 10 v1809 and later. (On some systems you may try 19.)
	private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

	// P/Invoke declaration for DwmSetWindowAttribute from dwmapi.dll.
	[DllImport( "dwmapi.dll", SetLastError = true )]
	private static extern int DwmSetWindowAttribute( IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute );

	/// <summary>
	/// Enables or disables immersive dark mode for the specified window.
	/// </summary>
	/// <param name="hwnd">Handle to the target window.</param>
	/// <param name="enable">True to enable dark mode; false to disable.</param>
	/// <returns>True if the attribute was successfully set; otherwise, false.</returns>
	public static bool SetImmersiveDarkMode( IntPtr hwnd, bool enable )
	{
		if ( hwnd == IntPtr.Zero )
		{
			throw new ArgumentException( "Invalid window handle.", nameof( hwnd ) );
		}

		int attributeValue = enable ? 1 : 0;
		int result = DwmSetWindowAttribute( hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref attributeValue, sizeof( int ) );

		// A result of 0 indicates success.
		return result == 0;
	}

	private const int DWMSBT_MAINWINDOW = 2;
	private const int DWMSBT_TRANSIENTWINDOW = 3;
	private const int DWMSBT_TABBEDWINDOW = 4;
	private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

	public static bool SetMica( IntPtr hwnd )
	{
		if ( hwnd == IntPtr.Zero )
		{
			throw new ArgumentException( "Invalid window handle.", nameof( hwnd ) );
		}

		int attributeValue = DWMSBT_MAINWINDOW;
		int result = DwmSetWindowAttribute( hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref attributeValue, sizeof( int ) );

		// A result of 0 indicates success.
		return result == 0;
	}
}
