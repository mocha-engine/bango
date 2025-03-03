﻿global using Bango.Common;
global using Veldrid;
global using Matrix4x4 = System.Numerics.Matrix4x4;
global using Vector4 = System.Numerics.Vector4;

namespace Bango.Renderer;

public static class Global
{
	public static Logger Log { get; } = new();
	public static GraphicsDevice Device { get; internal set; }
}

