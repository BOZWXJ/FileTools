﻿using FileOrganizer.Properties;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace FileOrganizer.Models
{
	internal class ZipCompress
	{
		public static string SelectedPath {
			get => Settings.Default.RenamePath;
			private set {
				Settings.Default.RenamePath = value;
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
			foreach (var (item, i) in items.Select((s, i) => (s, i))) {
				// 進行状況
				progress.Report(new(i, c, $"{i}/{c}:{Path.GetFileName(item)}"));
				// ファイル処理
				Log.Append($"ファイル作成 {Path.GetFileName(item)}.zip");
				ZipFile.CreateFromDirectory(item, $"{item}.zip", CompressionLevel.NoCompression, false);
				// 中断
				if (token.IsCancellationRequested) {
					return;
				}
			}
		}
	}
}
