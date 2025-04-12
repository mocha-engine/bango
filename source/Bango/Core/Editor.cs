namespace Bango.Engine.Editor;

internal partial class EditorInstance
{
	internal static EditorInstance Instance { get; private set; }

	internal EditorInstance()
	{
		Event.Register( this );
		Instance = this;

		Graphics.Init();
	}

	private bool isChecked = false;
	internal void Render( Veldrid.CommandList commandList )
	{
		Graphics.PanelRenderer.NewFrame();
		// Graphics.DrawRect( new Rectangle( new Vector2( 0, 30 ), Screen.Size ), Theme.Default950, Vector4.Zero );
		ImDraw.NewFrame();
		ImDraw.TitleBar();

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
}
