using BitMiracle.LibTiff.Classic;
using Livet;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Tokens;

namespace PdfImageExtracter.Models
{
	public class FileToolsModel : NotificationObject
	{
		public ReactivePropertySlim<string> StatusMessage { get; } = new();

		public void ExtractImage(string srcFile, string dstFolder, bool makeSubFolder, int resize)
		{
			using (PdfDocument document = PdfDocument.Open(srcFile)) {
				string srcName = Path.GetFileNameWithoutExtension(srcFile);
				string folder = dstFolder;
				if (makeSubFolder) {
					folder = Path.Combine(dstFolder, srcName);
					Directory.CreateDirectory(folder);
				}
				foreach (Page page in document.GetPages()) {
					int rotate = 0;
					if (page.Dictionary.TryGet(NameToken.Rotate, out NumericToken token)) {
						rotate = token.Int;
					}

					var images = page.GetImages();
					foreach (var (image, j) in images.Select((o, i) => (o, i))) {
						System.Diagnostics.Debug.WriteLine($"page={page.Number}-{j} Rotate={rotate},{image}");
						StatusMessage.Value = $"{page.Number}/{document.NumberOfPages} {srcName}";

						if (image.ImageDictionary.TryGet(NameToken.Filter, out NameToken imageToken)) {
							string name = $"{(makeSubFolder ? "" : $"{srcName} ")}{page.Number:d3}{(images.Count() > 1 ? $"_{j}" : "")}";
							string path = Path.Combine(folder, name);
							byte[] bytes;
							if (imageToken == NameToken.DctDecode) {
								bytes = image.RawBytes.ToArray();
							} else if (imageToken == NameToken.CcittfaxDecode) {
								bytes = ConvertTiff(folder, image);
							} else {
								continue;
							}

							BitmapImage bmp = new();
							using (MemoryStream ms = new(bytes)) {
								bmp.BeginInit();
								bmp.CacheOption = BitmapCacheOption.OnLoad;
								bmp.CreateOptions = BitmapCreateOptions.None;
								bmp.StreamSource = ms;
								if (resize != 0) {
									int px = Math.Abs(resize);
									if ((resize > 0 && (rotate == 0 || rotate == 180)) || (resize < 0 && (rotate == 90 || rotate == 270))) {
										// 縦かつ回転無し or 横かつ90度回転
										bmp.DecodePixelHeight = px;
										bmp.DecodePixelWidth = (int)Math.Round(px / image.Bounds.Height * image.Bounds.Width);
									} else {
										// 縦かつ90度回転 or 横かつ回転無し
										bmp.DecodePixelHeight = (int)Math.Round(px / image.Bounds.Width * image.Bounds.Height);
										bmp.DecodePixelWidth = px;
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
							path = Path.Combine(folder, Path.ChangeExtension($"{name}", "jpg"));
							using FileStream fs = new(path, FileMode.Create);
							BitmapEncoder encoder = new JpegBitmapEncoder();
							encoder.Frames.Add(BitmapFrame.Create(bmp));
							encoder.Save(fs);
						}
					}
				}
			}
			System.Diagnostics.Debug.WriteLine("ExtractImage");
			StatusMessage.Value = "";
		}

		private static byte[] ConvertTiff(string folder, IPdfImage image)
		{
			byte[] bytes;
			image.ImageDictionary.TryGet(NameToken.Width, out IToken tok);
			int wid = ((NumericToken)tok).Int;
			image.ImageDictionary.TryGet(NameToken.Height, out tok);
			int hei = ((NumericToken)tok).Int;
			int dpX = (int)Math.Round(wid / image.Bounds.Width * 72);
			int dpY = (int)Math.Round(hei / image.Bounds.Height * 72);
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
				tiff.SetField(TiffTag.IMAGEWIDTH, wid);
				tiff.SetField(TiffTag.IMAGELENGTH, hei);
				tiff.SetField(TiffTag.COMPRESSION, cmp);
				tiff.SetField(TiffTag.BITSPERSAMPLE, bpc);
				tiff.SetField(TiffTag.SAMPLESPERPIXEL, 1);
				tiff.SetField(TiffTag.XRESOLUTION, dpX);
				tiff.SetField(TiffTag.YRESOLUTION, dpY);
				tiff.WriteRawStrip(0, tmp, tmp.Length);
			}
			bytes = File.ReadAllBytes(tmpPath);
			File.Delete(tmpPath);
			return bytes;
		}

	}
}
