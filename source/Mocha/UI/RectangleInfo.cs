namespace Bango.Renderer.UI;

public record RectangleInfo(
	Common.Rectangle? TextureCoordinates = default,
	FillMode? FillMode = default,
	RenderMode Flags = RenderMode.None,
	float ScreenPxRange = 0f,
	Vector4? Rounding = default
);
