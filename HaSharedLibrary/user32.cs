using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HaSharedLibrary {
	public class user32 {
		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
			int Y, int cx, int cy, uint uFlags);

		public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
		public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
		public static readonly IntPtr HWND_TOP = new IntPtr(0);

		public const uint SWP_NOSIZE = 0x0001;
		public const uint SWP_NOMOVE = 0x0002;
		public const uint SWP_NOZORDER = 0x0004;
		public const uint SWP_NOREDRAW = 0x0008;
		public const uint SWP_NOACTIVATE = 0x0010;
		public const uint SWP_FRAMECHANGED = 0x0020; /* The frame changed: send WM_NCCALCSIZE */
		public const uint SWP_SHOWWINDOW = 0x0040;
		public const uint SWP_HIDEWINDOW = 0x0080;
		public const uint SWP_NOCOPYBITS = 0x0100;
		public const uint SWP_NOOWNERZORDER = 0x0200; /* Don't do owner Z ordering */
		public const uint SWP_NOSENDCHANGING = 0x0400; /* Don't send WM_WINDOWPOSCHANGING */
	}
}