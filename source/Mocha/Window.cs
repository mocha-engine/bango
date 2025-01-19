using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using Veldrid.Sdl2;

namespace Mocha.Renderer;

public class Window
{
	public static Window Current { get; set; }
	public Sdl2Window SdlWindow { get; private set; }
	public Point2 Size
	{
		get => new Point2( SdlWindow.Width, SdlWindow.Height );
		set
		{
			SdlWindow.Width = value.X;
			SdlWindow.Height = value.Y + 24;
		}
	}

	public string Title
	{
		set => Sdl2Native.SDL_SetWindowTitle( SdlWindow.SdlWindowHandle, value );
	}

	public Window()
	{
		Current ??= this;

		var windowFlags = SDL_WindowFlags.OpenGL | SDL_WindowFlags.AllowHighDpi | SDL_WindowFlags.Shown | SDL_WindowFlags.Resizable;
		SdlWindow = new Sdl2Window( "Mocha", 128, 128, 1280, 720, windowFlags, threadedProcessing: false );

		Screen.UpdateFrom( Size );
	}
}
