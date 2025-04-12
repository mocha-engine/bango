namespace Bango;
public struct InputSnapshot
{
	public System.Numerics.Vector2 MouseDelta { get; internal set; } = new System.Numerics.Vector2();
	public System.Numerics.Vector2 MousePosition { get; internal set; } = new System.Numerics.Vector2();
	public bool MouseLeft { get; set; } = false;
	public bool MouseRight { get; set; } = false;
	public float WheelDelta { get; internal set; } = 0f;
	public List<SDL2.SDL.SDL_Keysym> KeysDown { get; set; } = new();

	public InputSnapshot()
	{

	}
}
