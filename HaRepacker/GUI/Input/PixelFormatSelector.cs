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
			switch ((WzPngProperty.WzPixelFormat) defaultPixFormat) {
				case WzPngProperty.WzPixelFormat.Unknown:
				case WzPngProperty.WzPixelFormat.B4G4R4A4:
					formatSelector.SelectedIndex = 0;
					break;
				case WzPngProperty.WzPixelFormat.B8G8R8A8:
					formatSelector.SelectedIndex = 1;
					break;
				case WzPngProperty.WzPixelFormat.R5G6B5:
					formatSelector.SelectedIndex = 2;
					break;
				default:
					throw new ArgumentException("Invalid pixel format used.");
			}
		}

		private void okButton_Click(object sender, EventArgs e) {
			switch (formatSelector.SelectedIndex) {
				case 0:
					_pixFormatResult = (int) WzPngProperty.WzPixelFormat.B4G4R4A4;
					break;
				case 1:
					_pixFormatResult = (int) WzPngProperty.WzPixelFormat.B8G8R8A8;
					break;
				case 2:
					_pixFormatResult = (int) WzPngProperty.WzPixelFormat.R5G6B5;
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