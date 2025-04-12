using Bango.Renderer.UI;

namespace Bango.Engine.Editor;

partial class Graphics
{
	internal static Texture UITexture { get; set; }

	public static void Init()
	{
		PanelRenderer = new();
		InitializeAtlas();
	}

	private static void InitializeAtlas()
	{
		//
		// White box
		//
		var whiteSpriteData = new byte[1 * 1 * 4];
		Array.Fill( whiteSpriteData, (byte)255 );
		UITexture = Texture.Builder.FromData( whiteSpriteData, 1, 1 ).WithName( "internal:blank" ).Build();

		PanelRenderer.AtlasBuilder.AddOrGetTexture( UITexture );
	}
}
