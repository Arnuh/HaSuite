using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MapleLib.Converters {
	public static class ImageConverter {
		#region Texture2D

		/// <summary>
		///  Converts Microsoft.Xna.Framework.Graphics.Texture2D to PNG MemoryStream
		/// </summary>
		/// <param name="texture2D"></param>
		/// <returns></returns>
		public static MemoryStream Texture2DToPng(this Texture2D texture2D) {
			var memoryStream = new MemoryStream();
			texture2D.SaveAsPng(memoryStream, texture2D.Width, texture2D.Height);
			memoryStream.Seek(0, SeekOrigin.Begin);

			return memoryStream;
		}

		/// <summary>
		/// Converts Microsoft.Xna.Framework.Graphics.Texture2D to JPG MemoryStream
		/// </summary>
		/// <param name="texture2D"></param>
		/// <returns></returns>
		public static MemoryStream Texture2DToJpg(this Texture2D texture2D) {
			var memoryStream = new MemoryStream();
			texture2D.SaveAsJpeg(memoryStream, texture2D.Width, texture2D.Height);
			memoryStream.Seek(0, SeekOrigin.Begin);

			return memoryStream;
		}

		#endregion

		public static Bitmap ToWinFormsBitmap(this BitmapSource bitmapsource) {
			using (var stream = new MemoryStream()) {
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(bitmapsource));
				enc.Save(stream);

				using (var tempBitmap = new Bitmap(stream)) {
					// According to MSDN, one "must keep the stream open for the lifetime of the Bitmap."
					// So we return a copy of the new bitmap, allowing us to dispose both the bitmap and the stream.
					return new Bitmap(tempBitmap);
				}
			}
		}

		/// <summary>
		/// System.Drawing.Bitmap to System.Drawing.Image
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		public static Image ToImage(this Bitmap bitmap) {
			return (Image) bitmap;
		}

		/// <summary>
		/// Converts System.Drawing.Bitmap to byte[]
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte[] BitmapToBytes(this Bitmap bitmap) {
			BitmapData bmpdata = null;
			try {
				bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
					bitmap.PixelFormat);
				var numbytes = bmpdata.Stride * bitmap.Height;
				var bytedata = new byte[numbytes];
				var ptr = bmpdata.Scan0;

				Marshal.Copy(ptr, bytedata, 0, numbytes);

				return bytedata;
			}
			finally {
				if (bmpdata != null)
					bitmap.UnlockBits(bmpdata);
			}
		}

		public static BitmapSource ToWpfBitmap(this Bitmap bitmap) {
			using (var stream = new MemoryStream()) {
				bitmap.Save(stream, ImageFormat.Png);
				stream.Position = 0;
				var result = new BitmapImage();
				result.BeginInit();
				// According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
				// Force the bitmap to load right now so we can dispose the stream.
				result.CacheOption = BitmapCacheOption.OnLoad;
				result.StreamSource = stream;
				result.EndInit();
				result.Freeze();
				return result;
			}
		}
	}
}