using Livet;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FileEraser.Models
{
	public class FileSelectorName : NotificationObject, IFileSelector
	{
		public FileSelectorTypes SelectorType => FileSelectorTypes.Name;

		public string Description
		{
			get => _Description;
			set => RaisePropertyChangedIfSet(ref _Description, value);
		}
		private string _Description;

		public string Pattern
		{
			get => _Pattern;
			set
			{
				Description = $"正規表現 /{value}/";
				_Pattern = value;
			}
		}
		private string _Pattern;

		public bool Check(string filePath)
		{
			return Regex.IsMatch(Path.GetFileName(filePath), Pattern);
		}

		public static FileSelectorName FromString(string str)
		{
			string[] s = str.Split("\t");
			return new() { Pattern = s[1] };
		}

		public override string ToString()
		{
			return $"{SelectorType}\t{Pattern}";
		}

	}
}
