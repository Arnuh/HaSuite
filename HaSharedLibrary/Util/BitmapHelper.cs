﻿using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace HaSharedLibrary.Util {
	public static class BitmapHelper {
		/// <summary>
		/// Takes a bitmap and converts it to an image that can be handled by WPF ImageBrush
		/// </summary>
		/// <param name="src">A bitmap image</param>
		/// <param name="format"></param>
		/// <returns>The image as a BitmapImage for WPF</returns>
		public static BitmapImage Convert(this Bitmap src, System.Drawing.Imaging.ImageFormat format) {
			using (var ms = new MemoryStream()) {
				src.Save(ms, format);
				var image = new BitmapImage();
				image.BeginInit();
				ms.Seek(0, SeekOrigin.Begin);
				image.StreamSource = ms;
				image.EndInit();

				return image;
			}
		}

		public static Texture2D ToTexture2D(this Bitmap bitmap, GraphicsDevice device) {
			if (bitmap == null) return null; //todo handle this in a useful way

			using (var s = new MemoryStream()) {
				bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Png);
				s.Seek(0, SeekOrigin.Begin);
				return Texture2D.FromStream(device, s);
			}
		}
	}
}