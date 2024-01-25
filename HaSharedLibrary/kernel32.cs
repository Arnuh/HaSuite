using System;
using System.Runtime.InteropServices;

namespace HaSharedLibrary {
	public class kernel32 {
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool SetDllDirectory(string lpPathName);


		[DllImport("kernel32.dll")]
		public static extern IntPtr LoadLibrary(string dllToLoad);


		[DllImport("kernel32.dll")]
		public static extern bool FreeLibrary(IntPtr hModule);

		[DllImport("kernel32.dll")]
		public static extern uint GetLastError();
	}
}