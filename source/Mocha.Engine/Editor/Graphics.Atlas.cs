using Bango.Renderer.UI;

namespace Bango.Engine.Editor;
partial class Graphics
{
	internal static Texture UITexture { get; set; }
	private static List<(Texture Texture, Vector2 Position)> TextureCache { get; } = new();

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
		UITexture = Texture.Builder.FromData( whiteSpriteData, 1, 1 ).WithName( "internal:editor_white_box" ).Build();

		PanelRenderer.AtlasBuilder.AddOrGetTexture( UITexture );
	}
}
