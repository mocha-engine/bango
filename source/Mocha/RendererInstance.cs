using Mocha.Common.World;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Mocha.Renderer;

public class RendererInstance
{
	public Window window;

	private DateTime lastFrame;

	private CommandList commandList;

	private Material gbufferCombineMaterial;

	public Action PreUpdate;
	public Action OnUpdate;
	public Action PostUpdate;

	public Action<CommandList> RenderOverlays;

	public RendererInstance()
	{
		Event.Register( this );

		Init();
		lastFrame = DateTime.Now;
	}

	private void Init()
	{
		window = new();

		CreateGraphicsDevice();
		// Swap the buffers so that the screen isn't a mangled mess
		Device.SwapBuffers();

		commandList = Device.ResourceFactory.CreateCommandList();
	}

	public void Run()
	{
		OnWindowResized( window.Size );

		while ( window.SdlWindow.Exists )
		{
			Update();

			PreRender();

			PostRender();

			//
			// Set window title
			//
			var framerate = 1.000f / Time.AverageDelta;
			var windowTitle = $"Mocha.UI | {Device.BackendType} | {framerate.CeilToInt()}fps";
			Window.Current.Title = windowTitle;
		}
	}

	private void PreRender()
	{
		// TODO: Make this nicer
		// Check each shader, if it's dirty then recompile it
		foreach ( var shader in Asset.All.OfType<Shader>().Where( x => x.IsDirty ) )
		{
			shader.Recompile();
		}

		commandList.Begin();
	}

	private void PostRender()
	{
		commandList.SetFramebuffer( Device.SwapchainFramebuffer );
		commandList.SetViewport( 0, new Viewport( 0, 0, Device.SwapchainFramebuffer.Width, Device.SwapchainFramebuffer.Height, 0, 1 ) );
		commandList.SetFullViewports();
		commandList.SetFullScissorRects();
		commandList.ClearColorTarget( 0, RgbaFloat.Black );
		commandList.ClearDepthStencil( 1 );

		commandList.PushDebugGroup( "UI Render" );
		RenderOverlays?.Invoke( commandList );
		commandList.PopDebugGroup();

		commandList.End();

		Device.SubmitCommands( commandList );
		Device.SwapBuffers();
	}

	private void Update()
	{
		float deltaTime = (float)(DateTime.Now - lastFrame).TotalSeconds;
		lastFrame = DateTime.Now;

		Time.UpdateFrom( deltaTime );

		PreUpdate?.Invoke();
		OnUpdate?.Invoke();
		PostUpdate?.Invoke();
	}

	private void CreateGraphicsDevice()
	{
		var options = new GraphicsDeviceOptions()
		{
			PreferStandardClipSpaceYDirection = true,
			PreferDepthRangeZeroToOne = true,
			SwapchainDepthFormat = PixelFormat.D24_UNorm_S8_UInt,
			SwapchainSrgbFormat = false,
			SyncToVerticalBlank = true
		};

		Device = VeldridStartup.CreateGraphicsDevice( Window.Current.SdlWindow, options );
	}

	[Event.Window.Resized]
	public void OnWindowResized( Point2 newSize )
	{
		var dpiScale = 1.0f;

		Device.MainSwapchain.Resize( (uint)(newSize.X * dpiScale), (uint)(newSize.Y * dpiScale) );
	}
}
