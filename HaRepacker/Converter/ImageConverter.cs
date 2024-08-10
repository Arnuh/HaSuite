using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace HaRepacker.Converters {
	public static class ImageConverter {
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