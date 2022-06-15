using BitMiracle.LibTiff.Classic;
using Livet;
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
		public void ExtractImage()
		{
			string srcFile = @"D:\Users\mitamura\Pictures\tmp\test.pdf";
			string dstFolder = @"D:\Users\mitamura\Pictures\tmp";

			using (PdfDocument document = PdfDocument.Open(srcFile)) {
				string folder = Path.Combine(dstFolder, Path.GetFileNameWithoutExtension(srcFile));
				Directory.CreateDirectory(folder);
				foreach (Page page in document.GetPages()) {
					int rotate = 0;
					if (page.Dictionary.TryGet(NameToken.Rotate, out NumericToken token)) {
						rotate = token.Int;
					}

					var images = page.GetImages();
					foreach (var (image, j) in images.Select((o, i) => (o, i))) {
						System.Diagnostics.Debug.WriteLine($"page={page.Number} Rotate={rotate} {j},{image}");

						if (image.ImageDictionary.TryGet(NameToken.Filter, out NameToken imageToken)) {
							string name = $"{page.Number:d3}{(images.Count() > 1 ? $"_{j}" : "")}";
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
								if (rotate == 90  || rotate == 270) {
									bmp.DecodePixelHeight = (int)Math.Round(1400.0 / image.Bounds.Width * image.Bounds.Height);
									bmp.DecodePixelWidth = 1400;
								} else {
									bmp.DecodePixelHeight = 1400;
									bmp.DecodePixelWidth = (int)Math.Round(1400.0 / image.Bounds.Height * image.Bounds.Width);
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
							using (FileStream fs = new(path, FileMode.Create)) {
								BitmapEncoder encoder = new JpegBitmapEncoder();
								encoder.Frames.Add(BitmapFrame.Create(bmp));
								encoder.Save(fs);
							}

						}
					}
				}
			}
			System.Diagnostics.Debug.WriteLine("ExtractImage");
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
			string tmpPath = Path.Combine(folder, Path.GetTempFileName());
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
