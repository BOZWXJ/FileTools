using FileOrganizer.Properties;
using FileOrganizer.Utility;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace FileOrganizer.Models
{
	internal static class Rename
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
				max += Directory.GetFiles(item).Length;
			}
			foreach (var folder in path) {
				Log.Append($"処理フォルダ {folder}");
				var items = Directory.GetDirectories(folder).Concat(Directory.GetFiles(folder));
				foreach (var item in items.OrderBy(p => p, new LogicalCompare())) {
					// 進行状況
					cnt++;
					progress.Report(new(cnt, max, $"{cnt}/{max}:{Path.GetFileName(item)}"));
					// ファイル処理
					string name = Path.GetFileNameWithoutExtension(item);
					string ext = Path.GetExtension(item);
					string newName;
					if (ext.Equals(".opdownload", StringComparison.OrdinalIgnoreCase)) {
						continue;
					} else if (ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase)) {
						newName = PdfName(name);
					} else {
						newName = CheckName(name);
					}
					if (!name.Equals(newName, StringComparison.OrdinalIgnoreCase)) {
						string dest = Path.Combine(folder, newName + ext);
						if (!Directory.Exists(dest) && !File.Exists(dest)) {
							Log.Append($"名前変更 {name} -> {newName}");
							Directory.Move(item, dest);
						}
					}
					// 中断
					if (token.IsCancellationRequested) {
						return;
					}
				}
			}
			Log.Append("完了");
		}
		#region
		private static string CheckName(string name)
		{
			name = name.Normalize();
			// ～
			name = Regex.Replace(name, @"〜", "～");
			// ∼
			name = Regex.Replace(name, @"∼", "～");
			// 削除
			name = Regex.Replace(name, @"[♥♡]", "");
			// 全角
			name = Regex.Replace(name, @"!", "！");
			name = Regex.Replace(name, @"~", "～");
			// 半角
			name = Regex.Replace(name, @"　", " ");
			name = Regex.Replace(name, @"［", "[");
			name = Regex.Replace(name, @"］", "]");
			// [アンソロジー]
			name = Regex.Replace(name, @"^\[アンソロジー]", "(アンソロジー)");
			// ANGEL倶楽部
			name = Regex.Replace(name, @"^ANGEL club", "ANGEL倶楽部", RegexOptions.IgnoreCase);
			name = Regex.Replace(name, @"^ANGEL 倶楽部", "ANGEL倶楽部", RegexOptions.IgnoreCase);
			// LO
			name = Regex.Replace(name, @"^コミックエルオー\s*", "COMIC LO ");
			// COMIC
			name = Regex.Replace(name, @"^コミック\s*", "COMIC ");
			// 年-月
			name = Regex.Replace(name, @"(\d{4})-(\d{1,2})", "$1年$2月");
			// 月１桁
			name = Regex.Replace(name, @"年(\d)月", "年0$1月");
			// ] の次にスペース挿入
			name = Regex.Replace(name, @"]", "] ");
			// ( の前にスペース挿入
			name = Regex.Replace(name, @"\(", " (");
			// 複数のスペースをまとめる
			name = Regex.Replace(name, @" +", " ");
			// 前後のスペースを削除
			name = name.Trim();
			return name;
		}
		private static string PdfName(string name)
		{
			name = Regex.Replace(name, @"^(.*)【(.*)】(.*)", "[$2] $1 $3");
			return name;
		}
		#endregion
	}
}
