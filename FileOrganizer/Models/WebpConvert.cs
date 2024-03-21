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

		public static void Method(string path, IProgress<ProgressInfo> progress, CancellationToken token)
		{
			if (!Directory.Exists(path)) {
				Log.Append("フォルダを選択して下さい");
				return;
			}
			SelectedPath = path;
			Log.Append($"処理フォルダ {SelectedPath}");
			var files = Directory.GetFiles(SelectedPath);
			var c = files.Length;
			foreach (var (file, i) in files.OrderBy(p => p, new LogicalCompare()).Select((s, i) => (s, i))) {
				// 進行状況
				progress.Report(new(i + 1, c, $"{i + 1}/{c}:{Path.GetFileName(file)}"));
				// ファイル処理
				string dir = Path.GetDirectoryName(file);
				string name = Path.GetFileNameWithoutExtension(file);
				string ext = Path.GetExtension(file);
				if (ext.Equals(".zip", StringComparison.OrdinalIgnoreCase)) {
					bool flg = false;
					string tmp = Path.Combine(dir, Path.GetRandomFileName());
					using (ZipArchive srcArchive = ZipFile.Open(file, ZipArchiveMode.Read, Encoding.GetEncoding(932)))
					using (ZipArchive dstArchive = ZipFile.Open(tmp, ZipArchiveMode.Create)) {
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
						// next: ゴミ箱
						File.Delete(file);
						File.Move(tmp, file);
						Log.Append($"{Path.GetFileName(file)}");
					} else {
						// 変換無し
						File.Delete(tmp);
					}
				} else if (ext.Equals(".webp", StringComparison.OrdinalIgnoreCase)) {
					using Image image = Image.Load(file);
					if (image.Metadata.DecodedImageFormat is SixLabors.ImageSharp.Formats.Webp.WebpFormat) {
						image.SaveAsJpeg(Path.ChangeExtension(file, ".jpg"));
						// next: ゴミ箱
						File.Delete(file);
						Log.Append($"{Path.GetFileName(file)}");
					}
				}
				// 中断
				if (token.IsCancellationRequested) {
					return;
				}
			}
			Log.Append("完了");
		}
	}
}
