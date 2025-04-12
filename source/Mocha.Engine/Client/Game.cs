namespace Bango.Engine;

/// <summary>
/// Handles the creation of various game systems.
/// </summary>
internal class Game
{
	private RendererInstance renderer;
	private EditorInstance editor;

	internal Game()
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
