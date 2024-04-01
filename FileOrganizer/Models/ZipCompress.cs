using FileOrganizer.Properties;
using FileOrganizer.Utility;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace FileOrganizer.Models
{
	internal class ZipCompress
	{
		public static string SelectedPath
		{
			get => Settings.Default.RenamePath;
			private set
			{
				Settings.Default.RenamePath = value;
				Settings.Default.Save();
			}
		}

		public static void Method(string[] path, IProgress<ProgressInfo> progress, CancellationToken token)
		{
			if (path == null || !path.Any(Directory.Exists)) {
				Log.Append("フォルダを選択して下さい");
				return;
			}
			if (path.Length == 1) {
				SelectedPath = path[0];
			} else {
				SelectedPath = Path.GetDirectoryName(path[0]);
			}
			int max = 0, cnt = 0;
			foreach (var item in path) {
				max += Directory.GetDirectories(item).Length;
			}
			foreach (var folder in path) {
				Log.Append($"処理フォルダ {folder}");
				var items = Directory.GetDirectories(folder);
				foreach (var item in items.OrderBy(p => p, new LogicalCompare())) {
					// 進行状況
					cnt++;
					progress.Report(new(cnt, max, $"{cnt}/{max}:{Path.GetFileName(item)}"));
					// ファイル処理
					if (!File.Exists($"{item}.zip")) {
						Log.Append($"ファイル作成 {Path.GetFileName(item)}.zip");
						ZipFile.CreateFromDirectory(item, $"{item}.zip", CompressionLevel.NoCompression, false);
					}
					// 中断
					if (token.IsCancellationRequested) {
						return;
					}
				}
			}
			Log.Append("完了");
		}
	}
}
