using Livet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileEraser.Models
{
	public class FileSelectorDate : NotificationObject, IFileSelector
	{
		public FileSelectorTypes SelectorType => FileSelectorTypes.Date;

		public string Description
		{
			get => _Description;
			set => RaisePropertyChangedIfSet(ref _Description, value);
		}
		private string _Description;

		// < > <= >=

		public DateTime Date
		{
			get => _Date;
			set
			{
				Description = $"{Date}";
				_Date = value;
			}
		}
		private DateTime _Date;

		public bool Check(string filePath)
		{
			// todo: 更新日時で判定
			throw new NotImplementedException();
		}

		public static FileSelectorDate FromString(string str)
		{
			string[] s = str.Split("\t");
			return new() { Date = DateTime.Parse(s[1]) };
		}

		public override string ToString()
		{
			return $"{SelectorType}\t{Date}";
		}

	}
}
