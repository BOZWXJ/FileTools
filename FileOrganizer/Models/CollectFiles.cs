using FileOrganizer.Properties;
using FileOrganizer.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileOrganizer.Models
{
	internal class CollectFiles
	{
		public static string SelectedPath {
			get => Settings.Default.CollectFilesPath;
			private set {
				Settings.Default.CollectFilesPath = value;
				Settings.Default.Save();
			}
		}

		public static void Method(string path, IProgress<ProgressInfo> progress, CancellationToken token)
		{
			if (!Directory.Exists(path)) {
				Log.Append("フォルダを選択して下さい");
				return;
			}
			SelectedPath = path;
			Log.Append($"処理フォルダ {SelectedPath}");
			var items = Directory.GetDirectories(SelectedPath);
			var c = items.Length;
			var queue = new Queue<string>();
			foreach (var (item, i) in items.OrderBy(p => p, new LogicalCompare()).Select((s, i) => (s, i))) {
				// 進行状況
				progress.Report(new(i + 1, c, $"{i + 1}/{c}:{Path.GetFileName(item)}"));
				// ファイル処理
				queue.Enqueue(item);
				// ファイル移動
				while (queue.Count > 0) {
					string dir = queue.Dequeue();
					// フォルダ
					foreach (string d in Directory.GetDirectories(dir)) {
						// 不要
						if (Path.GetFileName(d).Equals("__MACOSX", StringComparison.OrdinalIgnoreCase)) {
							continue;
						}
						queue.Enqueue(d);
					}
					// ファイル
					foreach (string file in Directory.GetFiles(dir)) {
						string name = Path.GetFileName(file);
						string dest = Path.Combine(item, name);
						if (name.Equals("hentaicore.jpg", StringComparison.OrdinalIgnoreCase)
							|| name.Equals("hc.jpg", StringComparison.OrdinalIgnoreCase)
							|| name.Equals("Thumbs.db", StringComparison.OrdinalIgnoreCase)
							|| name.Equals(".DS_Store", StringComparison.OrdinalIgnoreCase)
							|| name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
							|| name.EndsWith(".url", StringComparison.OrdinalIgnoreCase)) {
							// 不要	hentaicore.jpg, hc.jpg, Thumbs.db, .DS_Store, *.txt, *.url
							Log.Append($"ファイル削除 {file}");
							// next: File.Delete(file);
						} else if (!file.Equals(dest, StringComparison.OrdinalIgnoreCase) && !File.Exists(dest)) {
							// 移動	その他
							Log.Append($"ファイル移動 {dest}");
							// next: File.Move(file, dest);
						}
					}
					// 中断
					if (token.IsCancellationRequested) {
						return;
					}
				}
				// サブフォルダ削除
				foreach (string dir in Directory.GetDirectories(item)) {
					Log.Append($"サブフォルダ削除 {dir}");
					// next: Directory.Delete(dir, true);
				}
				// 中断
				if (token.IsCancellationRequested) {
					return;
				}
			}
		}
	}
}
