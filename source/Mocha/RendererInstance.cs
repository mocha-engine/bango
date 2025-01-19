using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Mocha.Renderer;

public class RendererInstance
{
	public Window window;

	private DateTime lastFrame;

	private CommandList commandList;

	public Action PreUpdate;
	public Action OnUpdate;
	public Action PostUpdate;

	public Action<CommandList> RenderOverlays;

	public static RendererInstance Current;

	public RendererInstance()
	{
		Current = this;
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
		CreateFb();

		commandList = Device.ResourceFactory.CreateCommandList();
	}

	private void CreateFb()
	{
		var ctd = TextureDescription.Texture2D(
			(uint)(Screen.RawSize.X),
			(uint)(Screen.RawSize.Y),
			1,
			1,
			PixelFormat.B8_G8_R8_A8_UNorm,
			TextureUsage.RenderTarget,
			TextureSampleCount.Count4
		);

		var ct = Device.ResourceFactory.CreateTexture( ctd );

		ctd.SampleCount = TextureSampleCount.Count1;
		ctd.Usage = TextureUsage.Sampled;
		resolve = Device.ResourceFactory.CreateTexture( ctd );

		var fbad = new FramebufferAttachmentDescription( ct, 0 );
		var fbd = new FramebufferDescription()
		{
			ColorTargets = [fbad]
		};

		fb = Device.ResourceFactory.CreateFramebuffer( fbd );
	}

	public Framebuffer fb;
	public Veldrid.Texture resolve;

	private Pipeline blitPipeline;
	private ResourceSet blitResourceSet;
	private ResourceLayout blitResourceLayout;

	public void Run()
	{
		var layoutDescription = new ResourceLayoutDescription(
			new ResourceLayoutElementDescription( "g_tInput", ResourceKind.TextureReadOnly, ShaderStages.Fragment ),
			new ResourceLayoutElementDescription( "g_sSampler", ResourceKind.Sampler, ShaderStages.Fragment )
		);

		blitResourceLayout = Device.ResourceFactory.CreateResourceLayout( layoutDescription );


		// Create shader
		var shader = ShaderBuilder.Default.FromPath( "core/shaders/blit.mshdr" )
										.WithFramebuffer( fb )
										.WithFaceCullMode( FaceCullMode.None )
										.Build();

		var blitShader = shader.ShaderProgram;

		var pipelineDescription = new GraphicsPipelineDescription(
			BlendStateDescription.SingleAlphaBlend,
			DepthStencilStateDescription.Disabled,
			RasterizerStateDescription.CullNone,
			PrimitiveTopology.TriangleList,
			new ShaderSetDescription(
				Array.Empty<VertexLayoutDescription>(),
				shader.ShaderProgram
			),
			new[] { blitResourceLayout },
			Device.MainSwapchain.Framebuffer.OutputDescription
		);

		blitPipeline = Device.ResourceFactory.CreateGraphicsPipeline( pipelineDescription );

		blitResourceSet = Device.ResourceFactory.CreateResourceSet( new ResourceSetDescription(
			blitResourceLayout,
			resolve,
			Device.LinearSampler
		) );

		OnWindowResized( window.Size );

		while ( window.SdlWindow.Exists )
		{
			Update();
			PreRender();
			PostRender();
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
		commandList.SetFramebuffer( fb ); // Use MSAA framebuffer
		commandList.SetViewport( 0, new Viewport( 0, 0, fb.Width, fb.Height, 0, 1 ) );
		commandList.SetFullViewports();
		commandList.SetFullScissorRects();
		commandList.ClearColorTarget( 0, RgbaFloat.Black );

		// Render UI to MSAA buffer
		commandList.PushDebugGroup( "UI Render" );
		RenderOverlays?.Invoke( commandList );
		commandList.PopDebugGroup();

		// Resolve MSAA to non-MSAA texture
		commandList.ResolveTexture( fb.ColorTargets[0].Target, resolve );

		// Blit to screen
		commandList.SetFramebuffer( Device.MainSwapchain.Framebuffer );
		commandList.SetViewport( 0, new Viewport( 0, 0, Device.MainSwapchain.Framebuffer.Width, Device.MainSwapchain.Framebuffer.Height, 0, 1 ) );

		commandList.SetPipeline( blitPipeline );
		commandList.SetGraphicsResourceSet( 0, blitResourceSet );
		commandList.Draw( 3, 1, 0, 0 );

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
			SyncToVerticalBlank = true,
			HasMainSwapchain = true
		};

		var swapchainSource = VeldridStartup.GetSwapchainSource( window.SdlWindow );
		Device = GraphicsDevice.CreateD3D11( swapchainDescription: new SwapchainDescription( swapchainSource, (uint)(window.Size.X), (uint)(window.Size.Y), options.SwapchainDepthFormat, options.SyncToVerticalBlank, options.SwapchainSrgbFormat ), options: options );
	}

	[Event.Window.Resized]
	public void OnWindowResized( Point2 newSize )
	{
		var dpiScale = 1.0f;
		Device.MainSwapchain.Resize( (uint)(newSize.X * dpiScale), (uint)(newSize.Y * dpiScale) );

		uint width = (uint)(newSize.X * dpiScale);
		uint height = (uint)(newSize.Y * dpiScale);

		// Cleanup old MSAA resources
		fb?.Dispose();
		resolve?.Dispose();

		// Recreate MSAA resources
		CreateFb();

		// Recreate blit resources since they depend on the framebuffer
		blitResourceSet?.Dispose();
		blitResourceSet = Device.ResourceFactory.CreateResourceSet( new ResourceSetDescription(
			blitResourceLayout,
			resolve,
			Device.LinearSampler
		) );
	}
}
