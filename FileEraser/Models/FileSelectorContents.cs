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
			private set => RaisePropertyChangedIfSet(ref _Description, value);
		}
		private string _Description;

		public string FilePath
		{
			get => _FilePath;
			set
			{
				Description = $"ファイルパス {value}";
				_FilePath = value;
			}
		}
		private string _FilePath;

		private byte[] Contents;

		public bool Check(string path)
		{
			try {
				if (Contents == null) {
					Contents = File.ReadAllBytes(FilePath);
				}
				byte[] file = File.ReadAllBytes(path);
				if (Contents.Length != file.Length) {
					return false;
				}
				for (int i = 0; i < Contents.Length; i++) {
					if (Contents[i] != file[i]) {
						return false;
					}
				}
			} catch {
				return false;
			}
			return true;
		}

		public FileSelectorContents(string filePath)
		{
			FilePath = filePath;
		}

		public static FileSelectorContents FromString(string str)
		{
			string[] s = str.Split("\t");
			return new(s[1]);
		}

		public override string ToString()
		{
			return $"{SelectorType}\t{FilePath}";
		}

	}
}
