namespace Bango.Engine;

internal class Application
{
	private RendererInstance renderer;
	private EditorInstance editor;

	internal Application()
	{
		if ( Veldrid.RenderDoc.Load( out var renderDoc ) )
		{
			renderDoc.OverlayEnabled = false;
			Log.Trace( "Loaded RenderDoc" );
		}

		using ( _ = new Stopwatch( "Full init" ) )
		{
			using ( _ = new Stopwatch( "Renderer init" ) )
			{
				renderer = new();
			}

			using ( _ = new Stopwatch( "Editor init" ) )
			{
				editor = new();
			}
		}

		// Must be called before everything else
		renderer.PreUpdate += Input.Update;
		renderer.RenderOverlays += editor.Render;

		renderer.Run();
	}
}
