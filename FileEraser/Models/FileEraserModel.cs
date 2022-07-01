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
			CanExecute.Value = false;

			List<string> files = new();
			files.AddRange(Directory.GetFiles(eraseFolder));
			if (subFolder) {
				Queue<string> queue = new(Directory.GetDirectories(eraseFolder));
				while (queue.Count > 0) {
					var dir = queue.Dequeue();
					files.AddRange(Directory.GetFiles(dir));
					foreach (var sub in Directory.GetDirectories(dir)) {
						queue.Enqueue(sub);
					}
				}
			}
			ProgressMax.Value = files.Count;
			Progress.Value = 0;
			foreach (var file in files) {
				StatusMessage.Value = $"{file.Substring(eraseFolder.Length)}";
				Progress.Value++;
				if (FileSelectorList.Any(p => p.Check(file))) {
					System.Diagnostics.Debug.WriteLine($"削除 {file}");
					// todo: ファイル削除処理

				}
			}
			StatusMessage.Value = "";

			CanExecute.Value = true;
		}

		public void SaveSettingList()
		{
			// todo: FileSelectorList を別ファイルに保存
			Settings.Default.SettingList.Clear();
			foreach (var item in FileSelectorList) {
				Settings.Default.SettingList.Add(item.ToString());
			}
		}

		public void LoadSettingList()
		{
			// todo: FileSelectorList を別ファイルから読込
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
			// todo: FileSelector 優先順位でソート
			System.Diagnostics.Debug.WriteLine($"AddFileSelectorItem {FileSelectorList.Count}");
		}

		public void EditFileSelectorItem(IFileSelector oldSelector, IFileSelector newSelector)
		{
			FileSelectorList.Remove(oldSelector);
			FileSelectorList.Add(newSelector);
			System.Diagnostics.Debug.WriteLine($"EditFileSelectorItem {FileSelectorList.Count}");
		}

		public void DeleteFileSelectorItem(IFileSelector selector)
		{
			FileSelectorList.Remove(selector);
			System.Diagnostics.Debug.WriteLine($"DeleteFileSelectorItem {FileSelectorList.Count}");
		}

	}
}
