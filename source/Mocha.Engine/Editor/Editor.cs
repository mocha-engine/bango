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
		foreach ( var item in Widget.All.ToArray() )
		{
			item.Delete();
		}

		//
		// Everything has to go inside a layout otherwise they'll go in funky places
		//
		RootLayout = new VerticalLayout
		{
			Size = new Vector2( -1, -1 )
		};

		RootLayout.Spacing = 8;
		RootLayout.Margin = new( 16, 16 );

		RootLayout.Add( new Button( "OK" ), false );
		RootLayout.Add( new Button( "I am a long button! Hello!" ), false );
	}

	private string testString = "";
	private bool isChecked = false;
	internal void Render( Veldrid.CommandList commandList )
	{
		Graphics.PanelRenderer.NewFrame();
		Graphics.DrawRect( new Rectangle( 0, (Vector2)Screen.Size ), Theme.Default50, Vector4.Zero );
		ImDraw.NewFrame();
		ImDraw.TitleBar();

		RenderWidgets();

		//
		// Labels
		//
		{
			ImDraw.Text( "Labels" );
			ImDraw.Spacing();

			ImDraw.SetFontDefault( 32.0f );
			ImDraw.Text( "Extra Large" );
			ImDraw.Inline();

			ImDraw.SetFontDefault( 24.0f );
			ImDraw.Text( "Large" );
			ImDraw.Inline();

			ImDraw.SetFontDefault( 16.0f );
			ImDraw.Text( "Medium" );
			ImDraw.Inline();

			ImDraw.SetFontDefault();
			ImDraw.Text( "Small" );

			ImDraw.Spacing( 16.0f );
		}

		ImDraw.Separator();

		//
		// Buttons
		//
		{
			ImDraw.Text( "Buttons" );
			ImDraw.Spacing();

			ImDraw.Button( "Default" );
			ImDraw.Inline();
			ImDraw.ButtonLight( "Light" );
			ImDraw.Inline();
			ImDraw.ButtonDelete( "Danger" );
			ImDraw.Inline();
			ImDraw.ButtonDisabled( "Disabled" );
		}

		ImDraw.Separator();

		//
		// Checkbox
		//
		{
			ImDraw.Text( "Checkboxes" );
			ImDraw.Spacing();

			ImDraw.Checkbox( "Enabled", ref isChecked );
		}

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

			if ( Input.MouseLeftPressed )
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
