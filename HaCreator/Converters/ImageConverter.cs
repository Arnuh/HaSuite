using System.Drawing;

namespace HaCreator.Converters {
	public static class ImageConverter {
		/// <summary>
		/// System.Drawing.Bitmap to System.Drawing.Image
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		public static Image ToImage(this Bitmap bitmap) {
			return bitmap;
		}
	}
}