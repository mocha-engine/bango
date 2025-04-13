using Veldrid;
using StbImageSharp;
using System.Runtime.InteropServices;

namespace Bango;
public partial class TextureBuilder
{
	private string type = "texture_diffuse";

	private byte[] data;
	private uint width;
	private uint height;

	private string path;

	private bool isRenderTarget;
	private TextureUsage textureUsage = TextureUsage.Sampled;

	private PixelFormat compressionFormat;

	private bool ignoreCache;

	public TextureBuilder()
	{
	}

	private static bool TryGetExistingTexture( string path, out Texture texture )
	{
		var existingTexture = Asset.All.OfType<Texture>().ToList().FirstOrDefault( t => t.Path == path );
		if ( existingTexture != null )
		{
			texture = existingTexture;
			return true;
		}

		texture = default;
		return false;
	}

	public Texture Build()
	{
		if ( TryGetExistingTexture( path, out var existingTexture ) && !ignoreCache )
			return existingTexture;

		var textureDescription = TextureDescription.Texture2D(
			width,
			height,
			1,
			1,
			compressionFormat,
			textureUsage
		);

		var texture = Device.ResourceFactory.CreateTexture( textureDescription );

		if ( !isRenderTarget )
		{
			var dataPtr = Marshal.AllocHGlobal( data.Length );

			Marshal.Copy( data, 0, dataPtr, data.Length );
			Device.UpdateTexture( texture,
						dataPtr,
						(uint)data.Length,
						0,
						0,
						0,
						width,
						height,
						1,
						0,
						0 );
			Marshal.FreeHGlobal( dataPtr );
		}

		var textureView = Device.ResourceFactory.CreateTextureView( texture );

		return new Texture( path, texture, textureView, type, (int)width, (int)height );
	}

	public TextureBuilder WithType( string type = "texture_diffuse" )
	{
		this.type = type;

		return this;
	}

	public TextureBuilder FromPath( string path )
	{
		if ( TryGetExistingTexture( path, out _ ) )
			return new TextureBuilder() { path = path };

		var fileData = FileSystem.Main.ReadAllBytes( path );
		var image = ImageResult.FromMemory( fileData, ColorComponents.RedGreenBlueAlpha );

		this.width = (uint)image.Width;
		this.height = (uint)image.Height;
		this.data = image.Data;
		this.compressionFormat = PixelFormat.R8_G8_B8_A8_UNorm;
		this.path = path;

		return this;
	}

	public TextureBuilder FromData( byte[] data, uint width, uint height )
	{
		this.data = data;
		this.width = width;
		this.height = height;

		return this;
	}

	public TextureBuilder FromEmpty( uint width, uint height )
	{
		var dataLength = (int)(width * height * 4);

		this.data = Enumerable.Repeat( (byte)0, dataLength ).ToArray();
		this.width = width;
		this.height = height;

		return this;
	}

	public TextureBuilder IgnoreCache( bool ignoreCache = true )
	{
		this.ignoreCache = ignoreCache;

		return this;
	}

	public TextureBuilder WithName( string name )
	{
		this.path = name;

		return this;
	}

	public TextureBuilder AsColorAttachment()
	{
		this.textureUsage |= TextureUsage.RenderTarget;
		this.isRenderTarget = true;

		return this;
	}

	public TextureBuilder AsDepthAttachment()
	{
		this.textureUsage |= TextureUsage.DepthStencil;
		this.compressionFormat = PixelFormat.D32_Float_S8_UInt;
		this.isRenderTarget = true;

		return this;
	}

	public TextureBuilder FromColor( Vector4 vector4 )
	{
		var data = new byte[4];

		data[0] = (byte)(vector4.X * 255).FloorToInt();
		data[1] = (byte)(vector4.Y * 255).FloorToInt();
		data[2] = (byte)(vector4.Z * 255).FloorToInt();
		data[3] = (byte)(vector4.W * 255).FloorToInt();

		return FromData( data, 1, 1 );
	}

	public TextureBuilder FromInternal( string name )
	{
		this.path = name;

		return this;
	}
}
