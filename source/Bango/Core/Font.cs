namespace Bango;
public sealed partial class Font
{
	private static Dictionary<string, Font> FontCache = new();

	public static Font Load( string fontFamily )
	{
		if ( FontCache.TryGetValue( fontFamily, out var val ) )
			return val;

		return new Font( fontFamily );
	}

	public Font.Data FontData { get; init; }
	public Texture FontTexture { get; init; }

	public Font( string fontFamily )
	{
		FontData = FileSystem.Game.Deserialize<Data>( $"core/fonts/baked/{fontFamily}.json" ) ?? throw new Exception( $"Failed to deserialize font data for '{fontFamily}'" );
		FontTexture = Texture.Builder.FromPath( $"core/fonts/baked/{fontFamily}.png" ).Build();

		FontCache.Add( fontFamily, this );
	}
}
