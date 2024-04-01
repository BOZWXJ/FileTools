using System;
using System.Runtime.InteropServices;

namespace FileOrganizer.Utility
{
	internal class NativeMethods
	{
		public enum FOFunc : uint
		{
			FO_MOVE = 0x0001,
			FO_COPY = 0x0002,
			FO_DELETE = 0x0003,
			FO_RENAME = 0x0004
		}

		[Flags]
		public enum FOFlags : ushort
		{
			FOF_MULTIDESTFILES = 0x0001,
			FOF_CONFIRMMOUSE = 0x0002,
			FOF_SILENT = 0x0004,
			FOF_RENAMEONCOLLISION = 0x0008,
			FOF_NOCONFIRMATION = 0x0010,
			FOF_WANTMAPPINGHANDLE = 0x0020,
			FOF_ALLOWUNDO = 0x0040,
			FOF_FILESONLY = 0x0080,
			FOF_SIMPLEPROGRESS = 0x0100,
			FOF_NOCONFIRMMKDIR = 0x0200,
			FOF_NOERRORUI = 0x0400,
			FOF_NOCOPYSECURITYATTRIBS = 0x0800,
			FOF_NORECURSION = 0x1000,
			FOF_NO_CONNECTED_ELEMENTS = 0x2000,
			FOF_WANTNUKEWARNING = 0x4000,
			FOF_NORECURSEREPARSE = 0x8000
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct SHFILEOPSTRUCT
		{
			public IntPtr hwnd;
			public FOFunc wFunc;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pFrom;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pTo;
			public FOFlags fFlags;
			public bool fAnyOperationsAborted;
			public IntPtr hNameMappings;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string lpszProgressTitle;
		}

		[DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
		private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

		/// <summary>
		/// ゴミ箱に移動
		/// </summary>
		/// <param name="files"></param>
		/// <returns></returns>
		public static int MoveToTrash(string[] files)
		{
			SHFILEOPSTRUCT sh = new() {
				wFunc = FOFunc.FO_DELETE,
				pFrom = string.Join("\0", files) + "\0",
				pTo = null,
				fFlags = FOFlags.FOF_ALLOWUNDO,
				fAnyOperationsAborted = false,
				hNameMappings = IntPtr.Zero,
				lpszProgressTitle = "MDCAppMoveTrash"
			};
			return SHFileOperation(ref sh);
		}
	}
}
