using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Bango.Renderer;

public class RendererInstance
{
	public Window Window;

	private DateTime _lastFrame;

	private CommandList _commandList;

	public Action PreUpdate;
	public Action OnUpdate;
	public Action PostUpdate;

	public Action<CommandList> RenderOverlays;

	public static RendererInstance Current { get; private set; } = null!;

	public RendererInstance()
	{
		Current = this;
		Event.Register( this );

		Init();
		_lastFrame = DateTime.Now;
	}

	private void Init()
	{
		Window = new();

		CreateGraphicsDevice();
		// Swap the buffers so that the screen isn't a mangled mess
		Device.SwapBuffers();
		CreateMultisampledFramebuffer();

		_commandList = Device.ResourceFactory.CreateCommandList();
	}

	private void CreateMultisampledFramebuffer()
	{
		var colorTextureInfo = TextureDescription.Texture2D(
			(uint)(Screen.RawSize.X),
			(uint)(Screen.RawSize.Y),
			1,
			1,
			PixelFormat.B8_G8_R8_A8_UNorm,
			TextureUsage.RenderTarget,
			TextureSampleCount.Count4
		);

		var colorTexture = Device.ResourceFactory.CreateTexture( colorTextureInfo );

		colorTextureInfo.SampleCount = TextureSampleCount.Count1;
		colorTextureInfo.Usage = TextureUsage.Sampled;

		ResolveTexture = Device.ResourceFactory.CreateTexture( colorTextureInfo );

		var framebufferAttachmentInfo = new FramebufferAttachmentDescription( colorTexture, 0 );
		var framebufferDescription = new FramebufferDescription()
		{
			ColorTargets = [framebufferAttachmentInfo]
		};

		MultisampledFramebuffer = Device.ResourceFactory.CreateFramebuffer( framebufferDescription );
	}

	public Framebuffer MultisampledFramebuffer;
	public Veldrid.Texture ResolveTexture;

	private Pipeline _blitPipeline;
	private ResourceSet _blitResourceSet;
	private ResourceLayout _blitResourceLayout;

	public void Run()
	{
		var layoutDescription = new ResourceLayoutDescription(
			new ResourceLayoutElementDescription( "g_tInput", ResourceKind.TextureReadOnly, ShaderStages.Fragment ),
			new ResourceLayoutElementDescription( "g_sSampler", ResourceKind.Sampler, ShaderStages.Fragment )
		);

		_blitResourceLayout = Device.ResourceFactory.CreateResourceLayout( layoutDescription );


		// Create shader
		var shader = ShaderBuilder.Default.FromPath( "core/shaders/blit.mshdr" )
										.WithFramebuffer( MultisampledFramebuffer )
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
			new[] { _blitResourceLayout },
			Device.MainSwapchain.Framebuffer.OutputDescription
		);

		_blitPipeline = Device.ResourceFactory.CreateGraphicsPipeline( pipelineDescription );

		_blitResourceSet = Device.ResourceFactory.CreateResourceSet( new ResourceSetDescription(
			_blitResourceLayout,
			ResolveTexture,
			Device.LinearSampler
		) );

		OnWindowResized( Window.Size );

		while ( Window.SdlWindow.Exists )
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

		_commandList.Begin();
	}

	private void PostRender()
	{
		_commandList.SetFramebuffer( MultisampledFramebuffer ); // Use MSAA framebuffer
		_commandList.SetViewport( 0, new Viewport( 0, 0, MultisampledFramebuffer.Width, MultisampledFramebuffer.Height, 0, 1 ) );
		_commandList.SetFullViewports();
		_commandList.SetFullScissorRects();
		_commandList.ClearColorTarget( 0, RgbaFloat.Black );

		// Render UI to MSAA buffer
		_commandList.PushDebugGroup( "UI Render" );
		RenderOverlays?.Invoke( _commandList );
		_commandList.PopDebugGroup();

		// Resolve MSAA to non-MSAA texture
		_commandList.ResolveTexture( MultisampledFramebuffer.ColorTargets[0].Target, ResolveTexture );

		// Blit to screen
		_commandList.SetFramebuffer( Device.MainSwapchain.Framebuffer );
		_commandList.SetViewport( 0, new Viewport( 0, 0, Device.MainSwapchain.Framebuffer.Width, Device.MainSwapchain.Framebuffer.Height, 0, 1 ) );

		_commandList.SetPipeline( _blitPipeline );
		_commandList.SetGraphicsResourceSet( 0, _blitResourceSet );
		_commandList.Draw( 3, 1, 0, 0 );

		_commandList.End();

		Device.SubmitCommands( _commandList );
		Device.SwapBuffers();
	}

	private void Update()
	{
		float deltaTime = (float)(DateTime.Now - _lastFrame).TotalSeconds;
		_lastFrame = DateTime.Now;

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

		var swapchainSource = VeldridStartup.GetSwapchainSource( Window.SdlWindow );
		Device = GraphicsDevice.CreateD3D11( swapchainDescription: new SwapchainDescription( swapchainSource, (uint)(Window.Size.X), (uint)(Window.Size.Y), options.SwapchainDepthFormat, options.SyncToVerticalBlank, options.SwapchainSrgbFormat ), options: options );
	}

	[Event.Window.Resized]
	public void OnWindowResized( Point2 newSize )
	{
		var dpiScale = 1.0f;
		Device.MainSwapchain.Resize( (uint)(newSize.X * dpiScale), (uint)(newSize.Y * dpiScale) );

		// Cleanup old MSAA resources
		MultisampledFramebuffer?.Dispose();
		ResolveTexture?.Dispose();

		// Recreate MSAA resources
		CreateMultisampledFramebuffer();

		// Recreate blit resources since they depend on the framebuffer
		_blitResourceSet?.Dispose();
		_blitResourceSet = Device.ResourceFactory.CreateResourceSet( new ResourceSetDescription(
			_blitResourceLayout,
			ResolveTexture,
			Device.LinearSampler
		) );
	}
}
