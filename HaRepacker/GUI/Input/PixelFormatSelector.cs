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
		private readonly int _defaultPixFormat;

		private PixelFormatSelector(int defaultPixFormat) {
			InitializeComponent();
			StartPosition = FormStartPosition.CenterParent;
			DialogResult = DialogResult.Cancel;
			_defaultPixFormat = defaultPixFormat;
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
					_pixFormatResult = _defaultPixFormat;
					break;
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private void cancelButton_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}