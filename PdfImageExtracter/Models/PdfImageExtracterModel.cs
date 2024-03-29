﻿using BitMiracle.LibTiff.Classic;
using Livet;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Tokens;

namespace PdfImageExtracter.Models
{
	public class PdfImageExtracterModel : NotificationObject
	{
		public enum ResizeMode { None, Height, Width }

		public ReactivePropertySlim<bool> CanExecute { get; } = new(true);

		// StatusBar
		public ReactivePropertySlim<string> StatusMessage { get; } = new();
		public ReactivePropertySlim<int> ProgressMax { get; } = new();
		public ReactivePropertySlim<int> Progress { get; } = new();

		public void Method(string[] files, string dstPath, bool makeSubFolder, ResizeMode mode, int size, bool saveRaw)
		{
			CanExecute.Value = false;

			ProgressMax.Value = files.Length * 100;
			Progress.Value = 0;
			fileCount = 0;
			foreach (var file in files) {
				ExtractImage(file, dstPath, makeSubFolder, mode, size, saveRaw);
				fileCount++;
			}
			CanExecute.Value = true;
		}

		private int fileCount;
		private void SetProgress(string name, int page, int num)
		{
			StatusMessage.Value = $"{num}/{page} {name}";
			Progress.Value = fileCount * 100 + 100 * num / page;
		}

		private void ExtractImage(string srcFile, string dstFolder, bool makeSubFolder, ResizeMode mode, int pixel, bool saveRaw)
		{
			using (PdfDocument document = PdfDocument.Open(srcFile)) {
				string srcName = Path.GetFileNameWithoutExtension(srcFile);
				string folder = dstFolder;
				if (makeSubFolder) {
					folder = Path.Combine(dstFolder, srcName);
					Directory.CreateDirectory(folder);
				}
				foreach (Page page in document.GetPages()) {
					SetProgress(srcName, document.NumberOfPages, page.Number);
					int rotate = 0;
					if (page.Dictionary.TryGet(NameToken.Rotate, out NumericToken token)) {
						rotate = token.Int;
					}
					var images = page.GetImages();
					foreach ((IPdfImage image, int j) in images.Select((o, i) => (o, i))) {
						System.Diagnostics.Debug.WriteLine($"page={page.Number}-{j} Rotate={rotate},{image}");

						if (image.ImageDictionary.TryGet(NameToken.Filter, out NameToken imageToken)) {
							string name = $"{(makeSubFolder ? "" : $"{srcName} ")}{page.Number:d3}{(images.Count() > 1 ? $"_{j}" : "")}";
							string path = Path.Combine(folder, name);
							Size size;
							byte[] bytes;
							if (imageToken == NameToken.DctDecode) {
								image.ImageDictionary.TryGet(NameToken.Width, out IToken tok);
								int w = ((NumericToken)tok).Int;
								image.ImageDictionary.TryGet(NameToken.Height, out tok);
								int h = ((NumericToken)tok).Int;
								size = new(w, h);
								bytes = image.RawBytes.ToArray();
							} else if (imageToken == NameToken.CcittfaxDecode) {
								(size, bytes) = ConvertTiff(image);
							} else {
								continue;
							}
							if (saveRaw) {
								SaveRawData(bytes, folder, name);
							}
							BitmapImage bmp = MakeBitmap(size, bytes, mode, pixel, rotate);

							path = Path.Combine(folder, Path.ChangeExtension($"{name}", "jpg"));
							using FileStream fs = new(path, FileMode.Create);
							BitmapEncoder encoder = new JpegBitmapEncoder();
							encoder.Frames.Add(BitmapFrame.Create(bmp));
							encoder.Save(fs);
						}
					}
				}
			}
			StatusMessage.Value = "";
		}

		private static (Size, byte[]) ConvertTiff(IPdfImage image)
		{
			byte[] bytes;
			image.ImageDictionary.TryGet(NameToken.Width, out IToken tok);
			int width = ((NumericToken)tok).Int;
			image.ImageDictionary.TryGet(NameToken.Height, out tok);
			int height = ((NumericToken)tok).Int;
			int dpX = (int)Math.Round(width / image.Bounds.Width * 72);
			int dpY = (int)Math.Round(height / image.Bounds.Height * 72);
			image.ImageDictionary.TryGet(NameToken.BitsPerComponent, out tok);
			int bpc = ((NumericToken)tok).Int;
			Compression cmp = Compression.CCITTFAX3;
			if (image.ImageDictionary.TryGet(NameToken.DecodeParms, out tok)) {
				if (((DictionaryToken)tok).TryGet(NameToken.K, out tok)) {
					if (((NumericToken)tok).Int == -1) {
						cmp = Compression.CCITTFAX4;
					}
				}
			}
			string tmpPath = Path.GetTempFileName();
			byte[] tmp = image.RawBytes.ToArray();
			using (Tiff tiff = Tiff.Open(tmpPath, "w")) {
				tiff.SetField(TiffTag.IMAGEWIDTH, width);
				tiff.SetField(TiffTag.IMAGELENGTH, height);
				tiff.SetField(TiffTag.COMPRESSION, cmp);
				tiff.SetField(TiffTag.BITSPERSAMPLE, bpc);
				tiff.SetField(TiffTag.SAMPLESPERPIXEL, 1);
				tiff.SetField(TiffTag.XRESOLUTION, dpX);
				tiff.SetField(TiffTag.YRESOLUTION, dpY);
				tiff.WriteRawStrip(0, tmp, tmp.Length);
			}
			bytes = File.ReadAllBytes(tmpPath);
			File.Delete(tmpPath);
			return (new Size(width, height), bytes);
		}

		private static BitmapImage MakeBitmap(Size imageSize, byte[] bytes, ResizeMode mode, int pixel, int rotate)
		{
			BitmapImage bmp = new();
			using (MemoryStream ms = new(bytes)) {
				bmp.BeginInit();
				bmp.CacheOption = BitmapCacheOption.OnLoad;
				bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
				bmp.StreamSource = ms;

				if ((mode == ResizeMode.Height && (rotate == 0 || rotate == 180)) || (mode == ResizeMode.Width && (rotate == 90 || rotate == 270))) {
					// 縦かつ回転無し or 横かつ90度回転
					if (imageSize.Height > pixel) {
						bmp.DecodePixelHeight = pixel;
						bmp.DecodePixelWidth = (int)Math.Round(pixel / imageSize.Height * imageSize.Width);
					}
				} else if ((mode == ResizeMode.Height && (rotate == 90 || rotate == 270)) || (mode == ResizeMode.Width && (rotate == 0 || rotate == 180))) {
					// 縦かつ90度回転 or 横かつ回転無し
					if (imageSize.Width > pixel) {
						bmp.DecodePixelHeight = (int)Math.Round(pixel / imageSize.Width * imageSize.Height);
						bmp.DecodePixelWidth = pixel;
					}
				}

				bmp.Rotation = rotate switch {
					90 => Rotation.Rotate90,
					180 => Rotation.Rotate180,
					270 => Rotation.Rotate270,
					_ => Rotation.Rotate0,
				};

				bmp.EndInit();
				bmp.Freeze();
			}

			return bmp;
		}

		private void SaveRawData(byte[] bytes, string dstFolder, string fileName)
		{
			// png  89 50 4E 47
			// jpeg FF D8
			// gif  47 49 46
			// bmp  42 4D
			// tiff 49 49 2a 00 or 4d 4d 00 2a
			string ext = "bin";
			if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x50 && bytes[3] == 0x50) {
				ext = "png";
			} else if (bytes[0] == 0xff && bytes[1] == 0xd8) {
				ext = "jpg";
			} else if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46) {
				ext = "gif";
			} else if (bytes[0] == 0x42 && bytes[1] == 0x4d) {
				ext = "bmp";
			} else if ((bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x2a && bytes[3] == 0x00) || (bytes[0] == 0x4d && bytes[1] == 0x4d && bytes[2] == 0x00 && bytes[3] == 0x2a)) {
				ext = "tiff";
			}
			string path = Path.Combine(dstFolder, Path.ChangeExtension($"{Path.GetFileNameWithoutExtension(fileName)}_raw", ext));
			File.WriteAllBytes(path, bytes);
		}

	}
}
