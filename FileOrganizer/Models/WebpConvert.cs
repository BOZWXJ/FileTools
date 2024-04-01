using FileOrganizer.Properties;
using FileOrganizer.Utility;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace FileOrganizer.Models
{
	internal class WebpConvert
	{
		public static string SelectedPath
		{
			get => Settings.Default.WebpConvertPath;
			private set
			{
				Settings.Default.WebpConvertPath = value;
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
				max += Directory.GetFiles(item).Length;
			}
			foreach (var folder in path) {
				Log.Append($"処理フォルダ {folder}");
				var files = Directory.GetFiles(folder);
				foreach (var file in files.OrderBy(p => p, new LogicalCompare())) {
					// 進行状況
					cnt++;
					progress.Report(new(cnt, max, $"{cnt}/{max}:{Path.GetFileName(file)}"));
					// ファイル処理
					string dir = Path.GetDirectoryName(file);
					string name = Path.GetFileNameWithoutExtension(file);
					string ext = Path.GetExtension(file);
					if (ext.Equals(".zip", StringComparison.OrdinalIgnoreCase)) {
						bool flg = false;
						using MemoryStream ms = new();
						using (ZipArchive srcArchive = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.GetEncoding(932)))
						using (ZipArchive dstArchive = new(ms, ZipArchiveMode.Create, true)) {
							foreach (var srcEntry in srcArchive.Entries) {
								if (Path.GetExtension(srcEntry.FullName).Equals(".webp", StringComparison.OrdinalIgnoreCase)) {
									ZipArchiveEntry dstEntry = dstArchive.CreateEntry(Path.ChangeExtension(srcEntry.FullName, "jpg"), CompressionLevel.NoCompression);
									using Stream src = srcEntry.Open();
									using Stream dst = dstEntry.Open();
									using Image image = Image.Load(src);
									image.SaveAsJpeg(dst);
									flg = true;
								} else {
									ZipArchiveEntry dstEntry = dstArchive.CreateEntry(srcEntry.FullName, CompressionLevel.NoCompression);
									using Stream src = srcEntry.Open();
									using Stream dst = dstEntry.Open();
									src.CopyTo(dst);
								}
							}
						}
						if (flg) {
							// 変換有り
							NativeMethods.MoveToTrash(new string[] { file });
							using (FileStream fs = new(file, FileMode.Create, FileAccess.Write)) {
								ms.WriteTo(fs);
							}
							Log.Append($"{Path.GetFileName(file)}");
						}
					} else if (ext.Equals(".webp", StringComparison.OrdinalIgnoreCase)) {
						using Image image = Image.Load(file);
						if (image.Metadata.DecodedImageFormat is SixLabors.ImageSharp.Formats.Webp.WebpFormat) {
							image.SaveAsJpeg(Path.ChangeExtension(file, ".jpg"));
							NativeMethods.MoveToTrash(new string[] { file });
							Log.Append($"{Path.GetFileName(file)}");
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
	}
}
