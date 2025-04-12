namespace Bango.Renderer.UI;

public record RectangleInfo(
	Rectangle? TextureCoordinates = default,
	FillMode? FillMode = default,
	RenderMode Flags = RenderMode.None,
	float ScreenPxRange = 0f,
	Vector4? Rounding = default,
	Vector2? UnitRange = default
);
