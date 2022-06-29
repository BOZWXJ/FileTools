using FileEraser.Properties;
using Livet;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace FileEraser.Models
{
	public class FileEraserModel : NotificationObject
	{
		public ReactivePropertySlim<bool> CanExecute { get; } = new(true);

		// 削除条件リスト
		public ReactiveCollection<IFileSelector> FileSelectorList { get; } = new();

		// StatusBar
		public ReactivePropertySlim<string> StatusMessage { get; } = new();
		public ReactivePropertySlim<int> ProgressMax { get; } = new();
		public ReactivePropertySlim<int> Progress { get; } = new();

		public void Method(string eraseFolder, bool subFolder)
		{
			List<string> files = new();
			files.AddRange(Directory.GetFiles(eraseFolder));
			if (subFolder) {
				// todo: サブフォルダ
			}
			foreach (var file in files) {
				if (FileSelectorList.Any(p => p.Check(file))) {
					System.Diagnostics.Debug.WriteLine($"削除 {file}");
					// todo: ファイル削除処理
				}
			}
		}

		public void SaveSettingList()
		{
			Settings.Default.SettingList.Clear();
			foreach (var item in FileSelectorList) {
				Settings.Default.SettingList.Add(item.ToString());
			}
		}

		public void LoadSettingList()
		{
			FileSelectorList.Clear();
			foreach (var item in Settings.Default.SettingList) {
				IFileSelector selector = null;
				if (item.StartsWith(FileSelectorTypes.Name.ToString())) {
					selector = FileSelectorName.FromString(item);
				} else if (item.StartsWith(FileSelectorTypes.Ext.ToString())) {
					selector = FileSelectorExt.FromString(item);
				} else if (item.StartsWith(FileSelectorTypes.Date.ToString())) {
					selector = FileSelectorDate.FromString(item);
				} else if (item.StartsWith(FileSelectorTypes.Contents.ToString())) {
					selector = FileSelectorContents.FromString(item);
				}
				if (selector != null) {
					FileSelectorList.Add(selector);
				}
			}
		}

		public void AddFileSelectorItem(IFileSelector selector)
		{
			FileSelectorList.Add(selector);
			System.Diagnostics.Debug.WriteLine($"AddFileSelectorItem {FileSelectorList.Count}");
		}

		public void EditFileSelectorItem(IFileSelector oldSelector, IFileSelector newSelector)
		{
			FileSelectorList.Remove(oldSelector);
			FileSelectorList.Add(newSelector);
			System.Diagnostics.Debug.WriteLine($"EditFileSelectorItem {FileSelectorList.Count}");
		}

		public void DeleteFileSelectorItem(int index)
		{
			FileSelectorList.RemoveAt(index);
			System.Diagnostics.Debug.WriteLine($"DeleteFileSelectorItem {FileSelectorList.Count}");
		}

	}
}
