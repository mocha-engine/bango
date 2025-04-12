using System;
using System.Runtime.InteropServices;

public static class WinApi
{
	// Import the DwmExtendFrameIntoClientArea function from dwmapi.dll.
	[DllImport( "dwmapi.dll", PreserveSig = false )]
	private static extern void DwmExtendFrameIntoClientArea( IntPtr hWnd, ref MARGINS pMarInset );

	// Structure to define the margins (in pixels).
	// Setting a margin to -1 extends the glass effect over the whole window on that side.
	[StructLayout( LayoutKind.Sequential )]
	private struct MARGINS
	{
		public int cxLeftWidth;   // width of the left border that retains the glass effect
		public int cxRightWidth;  // width of the right border that retains the glass effect
		public int cyTopHeight;   // height of the top border that retains the glass effect
		public int cyBottomHeight;// height of the bottom border that retains the glass effect
	}

	/// <summary>
	/// Extends the window frame into the client area.
	/// </summary>
	/// <param name="hWnd">The handle to the target window (HWND).</param>
	/// <param name="left">Left margin. Use -1 to extend to the left edge.</param>
	/// <param name="right">Right margin. Use -1 to extend to the right edge.</param>
	/// <param name="top">Top margin. Use -1 to extend to the top edge.</param>
	/// <param name="bottom">Bottom margin. Use -1 to extend to the bottom edge.</param>
	public static void ExtendFrame( IntPtr hWnd, int left, int right, int top, int bottom )
	{
		MARGINS margins = new MARGINS
		{
			cxLeftWidth = left,
			cxRightWidth = right,
			cyTopHeight = top,
			cyBottomHeight = bottom
		};

		// Call the DWM API. If desktop composition is not enabled, this may throw an exception.
		DwmExtendFrameIntoClientArea( hWnd, ref margins );
	}
}
