using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEraser.Models
{
	// ファイル名が正規表現と一致
	// 拡張子が一致
	// 更新日時
	// 指定ファイルと一致
	public enum FileSelectorTypes { Name, Ext, Date, Contents }

	public interface IFileSelector : INotifyPropertyChanged
	{
		public FileSelectorTypes SelectorType { get; }
		public string Description { get; set; }

		public bool Check(string filePath);
	}
}
