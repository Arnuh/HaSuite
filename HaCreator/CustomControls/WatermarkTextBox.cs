using System;
using System.Drawing;
using System.Windows.Forms;

namespace HaCreator.CustomControls {
	public class WatermarkTextBox : TextBox {
		/// <summary>
		/// The text that will be presented as the watermak hint
		/// </summary>
		private string _watermarkText = "Type here";

		/// <summary>
		/// Gets or Sets the text that will be presented as the watermak hint
		/// </summary>
		public string WatermarkText {
			get => _watermarkText;
			set => _watermarkText = value;
		}

		/// <summary>
		/// Whether watermark effect is enabled or not
		/// </summary>
		private bool _watermarkActive = true;

		/// <summary>
		/// Gets or Sets whether watermark effect is enabled or not
		/// </summary>
		public bool WatermarkActive {
			get => _watermarkActive;
			set => _watermarkActive = value;
		}

		/// <summary>
		/// Create a new TextBox that supports watermak hint
		/// </summary>
		public WatermarkTextBox() {
			_watermarkActive = true;
			Text = _watermarkText;
			ForeColor = Color.Gray;

			GotFocus += (source, e) => { RemoveWatermak(); };

			LostFocus += (source, e) => { ApplyWatermark(); };
		}

		/// <summary>
		/// Remove watermark from the textbox
		/// </summary>
		public void RemoveWatermak() {
			if (_watermarkActive) {
				_watermarkActive = false;
				Text = "";
				ForeColor = Color.Black;
			}
		}

		/// <summary>
		/// Applywatermak immediately
		/// </summary>
		public void ApplyWatermark() {
			if ((!_watermarkActive && string.IsNullOrEmpty(Text))
			    || ForeColor == Color.Gray) {
				_watermarkActive = true;
				Text = _watermarkText;
				ForeColor = Color.Gray;
			}
		}

		/// <summary>
		/// Apply watermak to the textbox. 
		/// </summary>
		/// <param name="newText">Text to apply</param>
		public void ApplyWatermark(string newText) {
			WatermarkText = newText;
			ApplyWatermark();
		}

		protected override void OnTextChanged(EventArgs e) {
			if (WatermarkActive) return;

			base.OnTextChanged(e);
		}
	}
}