namespace Bango;
public partial class Application
{
	internal static Application Instance { get; private set; }

	private float headerHeight = 0f;

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
		Graphics.DrawRect( new Rectangle( new Vector2( 0, 30 + headerHeight ), Screen.Size ), Theme.Default950, Vector4.Zero );
		ImDraw.NewFrame();
		ImDraw.TitleBar();

		OnRender();

		Graphics.PanelRenderer.Draw( commandList );
	}

	public virtual void OnRender()
	{

	}
}
