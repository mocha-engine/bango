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
		{
			var color = Theme.IsDark ? Theme.Default800.WithAlpha( 0.15f ) : Theme.Default500.WithAlpha( 0.5f );
			Graphics.DrawRect( new Rectangle( new Vector2( 0, 50 + headerHeight ), Screen.Size ), color, Vector4.Zero );

			var borderColor = Theme.IsDark ? Theme.Default50.WithAlpha( 0.25f ) : Theme.Default950.WithAlpha( 0.25f );
			Graphics.DrawRect( new Rectangle( new Vector2( 0, 50 + headerHeight ), Screen.Size.WithY( 1.0f ) ), borderColor, Vector4.Zero );
		}

		ImDraw.NewFrame();
		ImDraw.TitleBar();

		OnRender();

		Graphics.PanelRenderer.Draw( commandList );
	}

	public virtual void OnRender()
	{

	}
}
