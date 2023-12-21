﻿using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FileOrganizer.Utility
{
	class LogicalCompare : IComparer<string> 
	{
		[DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
		private static extern int StrCmpLogicalW(string psz1, string psz2);
		public int Compare(string x, string y)
		{
			return StrCmpLogicalW(x , y);
		}
	}
}
