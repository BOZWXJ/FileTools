using Livet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEraser.Models
{
	public class FileSelectorExt : NotificationObject, IFileSelector
	{
		public FileSelectorTypes SelectorType => FileSelectorTypes.Ext;

		public string Description
		{
			get => _Description;
			set => RaisePropertyChangedIfSet(ref _Description, value);
		}
		private string _Description;

		public string Extension
		{
			get => _Extension;
			set
			{
				Description = $"拡張子 {value}";
				_Extension = value;
			}
		}
		private string _Extension;

		public bool Check(string filePath)
		{
			return Path.GetExtension(filePath).TrimStart('.').Equals(Extension, StringComparison.OrdinalIgnoreCase);
		}

		public static FileSelectorExt FromString(string str)
		{
			string[] s = str.Split("\t");
			return new() { Extension = s[1] };
		}

		public override string ToString()
		{
			return $"{SelectorType}\t{Extension}";
		}

	}
}
