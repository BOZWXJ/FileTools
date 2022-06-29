using Livet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEraser.Models
{
	public class FileSelectorContents : NotificationObject, IFileSelector
	{
		public FileSelectorTypes SelectorType => FileSelectorTypes.Contents;

		public string Description
		{
			get => _Description;
			set => RaisePropertyChangedIfSet(ref _Description, value);
		}
		private string _Description;


		public string FilePath
		{
			get => _FilePath;
			set
			{
				Description = $"正規表現 /{value}/";
				_FilePath = value;
			}
		}
		private string _FilePath;

		public bool Check(string filePath)
		{
			// todo: ファイルの内容で判定
			throw new NotImplementedException();
		}

		public static FileSelectorContents FromString(string str)
		{
			string[] s = str.Split("\t");
			return new() { FilePath = s[1] };
		}

		public override string ToString()
		{
			return $"{SelectorType}\t{FilePath}";
		}

	}
}
