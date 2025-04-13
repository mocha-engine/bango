namespace Bango;
public partial class Application
{
	internal static Application Instance { get; private set; }

	private float headerHeight = 0f;

	public bool DrawBackground { get; set; } = true;

	protected Application( string appTitle = "Bango Window", float headerHeight = 0 )
	{		
		var bootstrap = new Bootstrap( this );

		this.headerHeight = headerHeight;
		SdlWindow.Current.Title = appTitle;

		Event.Register( this );
		Instance = this;

		OnStart();

		bootstrap.Run();
	}

	public virtual void OnStart()
	{

	}

	internal void Render( Veldrid.CommandList commandList )
	{
		Graphics.PanelRenderer.NewFrame();

		if ( DrawBackground )
			Graphics.DrawRect( new Rectangle( new Vector2( 0, 50 + headerHeight ), Screen.Size ), Theme.Default800.WithAlpha( 0.15f ), Vector4.Zero );

		ImDraw.NewFrame();
		ImDraw.TitleBar();

		OnRender();

		Graphics.PanelRenderer.Draw( commandList );
	}

	public virtual void OnRender()
	{

	}
}
