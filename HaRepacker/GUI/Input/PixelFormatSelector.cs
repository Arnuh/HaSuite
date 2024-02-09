using System;
using System.Windows.Forms;
using MapleLib.WzLib.WzProperties;

namespace HaRepacker.GUI.Input {
	public partial class PixelFormatSelector : Form {
		public static bool Show(int defaultPixFormat, out int pixFormat) {
			var form = new PixelFormatSelector(defaultPixFormat);
			var result = form.ShowDialog() == DialogResult.OK;
			pixFormat = form._pixFormatResult;
			return result;
		}

		private int _pixFormatResult;

		private PixelFormatSelector(int defaultPixFormat) {
			InitializeComponent();
			StartPosition = FormStartPosition.CenterParent;
			DialogResult = DialogResult.Cancel;
			switch ((WzPngProperty.CanvasPixFormat) defaultPixFormat) {
				case WzPngProperty.CanvasPixFormat.Unknown:
				case WzPngProperty.CanvasPixFormat.Argb4444:
					formatSelector.SelectedIndex = 0;
					break;
				case WzPngProperty.CanvasPixFormat.Argb8888:
					formatSelector.SelectedIndex = 1;
					break;
				case WzPngProperty.CanvasPixFormat.Argb1555:
					formatSelector.SelectedIndex = 3;
					break;
				case WzPngProperty.CanvasPixFormat.Rgb565:
					formatSelector.SelectedIndex = 3;
					break;
				case WzPngProperty.CanvasPixFormat.DXT3:
					formatSelector.SelectedIndex = 4;
					break;
				case WzPngProperty.CanvasPixFormat.DXT5:
					formatSelector.SelectedIndex = 5;
					break;
				default:
					throw new ArgumentException("Invalid pixel format used.");
			}
		}

		private void okButton_Click(object sender, EventArgs e) {
			switch (formatSelector.SelectedIndex) {
				case 0:
					_pixFormatResult = (int) WzPngProperty.CanvasPixFormat.Argb4444;
					break;
				case 1:
					_pixFormatResult = (int) WzPngProperty.CanvasPixFormat.Argb8888;
					break;
				case 2:
					_pixFormatResult = (int) WzPngProperty.CanvasPixFormat.Argb1555;
					break;
				case 3:
					_pixFormatResult = (int) WzPngProperty.CanvasPixFormat.Rgb565;
					break;
				case 4:
					_pixFormatResult = (int) WzPngProperty.CanvasPixFormat.DXT3;
					break;
				case 5:
					_pixFormatResult = (int) WzPngProperty.CanvasPixFormat.DXT5;
					break;
				default:
					throw new ArgumentException("Invalid pixel format selected.");
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private void cancelButton_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			Close();
		}


		private void format_OnKeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == 13) {
				okButton_Click(sender, e);
			}
		}
	}
}