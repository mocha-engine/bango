namespace Bango;
internal class Bootstrap
{
	private RendererInstance renderer;

	internal Bootstrap( Application app )
	{
		if ( Veldrid.RenderDoc.Load( out var renderDoc ) )
		{
			renderDoc.OverlayEnabled = false;
			Log.Trace( "Loaded RenderDoc" );
		}

		renderer = new();

		// Must be called before everything else
		renderer.PreUpdate += Input.Update;
		renderer.RenderOverlays += app.Render;

		Graphics.Init();
	}

	internal void Run()
	{
		renderer.Run();
	}
}
