namespace Bango.Engine.Editor;

internal partial class EditorInstance
{
	private BaseLayout RootLayout { get; set; }
	internal static EditorInstance Instance { get; private set; }

	internal EditorInstance()
	{
		Event.Register( this );
		Instance = this;

		Graphics.Init();

		CreateUI();
	}

	public virtual void CreateUI()
	{
		using var _ = new Stopwatch( "CreateUI" );

		foreach ( var item in Widget.All.ToArray() )
		{
			item.Delete();
		}

		//
		// Everything has to go inside a layout otherwise they'll go in funky places
		//
		RootLayout = new HorizontalLayout
		{
			Size = new Vector2( -1, -1 )
		};

		RootLayout.Spacing = 8;
		RootLayout.Margin = new( 16, 16 );

		RootLayout.Add( new Button( "OK" ), false );
		RootLayout.Add( new Button( "I am a really long button with some really long text inside it" ), false );
	}

	internal void Render( Veldrid.CommandList commandList )
	{
		Graphics.PanelRenderer.NewFrame();
		Graphics.DrawRect( new Rectangle( 0, (Vector2)Screen.Size ), Vector4.One, Vector4.Zero );

		RenderWidgets();

		Graphics.PanelRenderer.Draw( commandList );
	}

	internal void RenderWidgets()
	{
		var widgets = Widget.All.Where( x => x.Visible ).OrderBy( x => x.ZIndex ).ToList();
		var mouseOverWidgets = widgets.Where( x => x.Bounds.Contains( Input.MousePosition ) );

		foreach ( var widget in widgets )
		{
			widget.InputFlags = PanelInputFlags.None;
		}

		if ( mouseOverWidgets.Any() )
		{
			var focusedWidget = mouseOverWidgets.Last();
			focusedWidget.InputFlags |= PanelInputFlags.MouseOver;

			if ( Input.MouseLeft )
			{
				focusedWidget.InputFlags |= PanelInputFlags.MouseDown;
			}
		}

		foreach ( var widget in widgets )
		{
			widget.Render();
		}
	}

	[Event.Window.Resized]
	public void OnWindowResized( Point2 newSize )
	{
		CreateUI();
	}
}
