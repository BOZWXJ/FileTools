using Livet;
using System;
using System.Collections.Generic;
using System.IO;
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
			private set => RaisePropertyChangedIfSet(ref _Description, value);
		}
		private string _Description;

		public int Days
		{
			get => _Days;
			set
			{
				Description = $"更新日時 {Comparison} {Days}";
				RaisePropertyChangedIfSet(ref _Days, value);
			}
		}
		private int _Days;

		/// <summary>
		/// 以内 or 以降
		/// </summary>
		public string Comparison
		{
			get => _Comparison;
			set
			{
				Description = $"更新日時 {Comparison} {Days}";
				RaisePropertyChangedIfSet(ref _Comparison, value);
			}
		}
		private string _Comparison;

		public bool Check(string filePath)
		{
			var t = DateTime.Now.Date - File.GetLastWriteTime(filePath).Date;
			return Comparison switch {
				"<" => t.TotalDays < Days,
				">" => t.TotalDays > Days,
				_ => false,
			};
		}

		public static FileSelectorDate FromString(string str)
		{
			string[] s = str.Split("\t");
			return new() { Days = int.Parse(s[1]), Comparison = s[2] };
		}

		public override string ToString()
		{
			return $"{SelectorType}\t{Days}\t{Comparison}";
		}

	}
}
