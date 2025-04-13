using Bango;

namespace Example;

public class ExampleApplication : Application
{
	public ExampleApplication() : base( appTitle: "Example Application", headerHeight: 88 )
	{

	}

	private bool isChecked = false;

	public override void OnRender()
	{
		//
		// Header
		//
		{
			ImDraw.Text( "Header Test" );
			ImDraw.Spacing();

			ImDraw.Button( "Item 1" );
			ImDraw.Inline();
			ImDraw.Button( "Item 2" );
			ImDraw.Inline();
			ImDraw.Button( "Item 3" );
		}

		ImDraw.Spacing( 32.0f );

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

			ImDraw.Spacing( 32.0f );
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

			var isDark = Theme.IsDark;
			if ( ImDraw.Checkbox( "Dark mode", ref isDark ) )
			{
				Theme.IsDark = isDark;
			}
		}
	}
}
