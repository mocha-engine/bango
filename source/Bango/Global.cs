global using Matrix4x4 = System.Numerics.Matrix4x4;
global using Vector4 = System.Numerics.Vector4;
global using Rectangle = Bango.Rectangle;

namespace Bango;
public static class Global
{
	public static Logger Log { get; } = new();
	public static Veldrid.GraphicsDevice Device { get; internal set; } = null!;
}

