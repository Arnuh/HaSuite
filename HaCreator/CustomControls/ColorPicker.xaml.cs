/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SD = System.Drawing;

namespace WPFColorPickerLib {
	/// <summary>
	/// A simple WPF color picker.  The basic idea is to use a Color swatch image and then pick out a single
	/// pixel and use that pixel's RGB values along with the Alpha slider to form a SelectedColor.
	/// 
	/// This class is from Sacha Barber at http://sachabarber.net/?p=424 and http://www.codeproject.com/KB/WPF/WPFColorPicker.aspx.
	/// 
	/// This class borrows an idea or two from the following sources:
	///  - AlphaSlider and Preview box; Based on an article by ShawnVN's Blog; 
	///    http://weblogs.asp.net/savanness/archive/2006/12/05/colorcomb-yet-another-color-picker-dialog-for-wpf.aspx.
	///  - 1*1 pixel copy; Based on an article by Lee Brimelow; http://thewpfblog.com/?p=62.
	/// 
	/// Enhanced by Mark Treadwell (1/2/10):
	///  - Left click to select the color with no mouse move
	///  - Set tab behavior
	///  - Set an initial color (note that the search to set the cursor ellipse delays the initial display)
	///  - Fix single digit hex displays
	///  - Add Mouse Wheel support to change the Alpha value
	///  - Modify color select dragging behavior
	/// </summary>
	public partial class ColorPicker : UserControl {
		#region Data

		private DrawingAttributes drawingAttributes = new DrawingAttributes();
		private Color selectedColor = Colors.Transparent;
		private bool IsMouseDown = false;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor that initializes the ColorPicker to Black.
		/// </summary>
		public ColorPicker()
			: this(Colors.Black) {
		}

		/// <summary>
		/// Constructor that initializes to ColorPicker to the specified color.
		/// </summary>
		/// <param name="initialColor"></param>
		public ColorPicker(Color initialColor) {
			InitializeComponent();
			selectedColor = initialColor;
			ColorImage.Source = ImgSqaure1.Source;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or privately sets the Selected Color.
		/// </summary>
		public Color SelectedColor {
			get => selectedColor;
			private set {
				if (selectedColor != value) {
					selectedColor = value;
					CreateAlphaLinearBrush();
					UpdateTextBoxes(value);
					UpdateInk();
				}
			}
		}

		/// <summary>
		/// Sets the initial Selected Color.
		/// </summary>
		public Color InitialColor {
			set {
				SelectedColor = value;
				CreateAlphaLinearBrush();
				AlphaSlider.Value = value.A;
				UpdateCursorEllipse(value);
			}
		}

		#endregion

		#region Control Events

		/// <summary>
		/// 
		/// </summary>
		private void AlphaSlider_MouseWheel(object sender, MouseWheelEventArgs e) {
			var change = e.Delta / Math.Abs(e.Delta);
			AlphaSlider.Value = AlphaSlider.Value + (double) change;
		}

		/// <summary>
		/// Update SelectedColor Alpha based on Slider value.
		/// </summary>
		private void AlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			SelectedColor = Color.FromArgb((byte) AlphaSlider.Value, SelectedColor.R, SelectedColor.G, SelectedColor.B);
		}

		/// <summary>
		/// Update the SelectedColor if moving the mouse with the left button down.
		/// </summary>
		private void CanvasImage_MouseMove(object sender, MouseEventArgs e) {
			if (IsMouseDown)
				UpdateColor();
		}

		/// <summary>
		/// Handle MouseDown event.
		/// </summary>
		private void CanvasImage_MouseDown(object sender, MouseButtonEventArgs e) {
			IsMouseDown = true;
			UpdateColor();
		}

		/// <summary>
		/// Handle MouseUp event.
		/// </summary>
		private void CanvasImage_MouseUp(object sender, MouseButtonEventArgs e) {
			IsMouseDown = false;
			//UpdateColor();
		}

		/// <summary>
		/// Apply the new Swatch image based on user requested swatch.
		/// </summary>
		private void Swatch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			var img = sender as Image;
			ColorImage.Source = img.Source;
			UpdateCursorEllipse(SelectedColor);
		}

		#endregion // Control Events

		#region Private Methods

		/// <summary>
		/// Creates a new LinearGradientBrush background for the Alpha area slider.  This is based on the current color.
		/// </summary>
		private void CreateAlphaLinearBrush() {
			var startColor = Color.FromArgb((byte) 0, SelectedColor.R, SelectedColor.G, SelectedColor.B);
			var endColor = Color.FromArgb((byte) 255, SelectedColor.R, SelectedColor.G, SelectedColor.B);
			var alphaBrush =
				new LinearGradientBrush(startColor, endColor, new Point(0, 0), new Point(1, 0));
			AlphaBorder.Background = alphaBrush;
		}

		/// <summary>
		/// Sets a new Selected Color based on the color of the pixel under the mouse pointer.
		/// </summary>
		private void UpdateColor() {
			// Test to ensure we do not get bad mouse positions along the edges
			var imageX = (int) Mouse.GetPosition(canvasImage).X;
			var imageY = (int) Mouse.GetPosition(canvasImage).Y;
			if (imageX < 0 || imageY < 0 || imageX > ColorImage.Width - 1 ||
			    imageY > ColorImage.Height - 1) return;
			// Get the single pixel under the mouse into a bitmap and copy it to a byte array
			var cb = new CroppedBitmap(ColorImage.Source as BitmapSource, new Int32Rect(imageX, imageY, 1, 1));
			var pixels = new byte[4];
			cb.CopyPixels(pixels, 4, 0);
			// Update the mouse cursor position and the Selected Color
			ellipsePixel.SetValue(Canvas.LeftProperty,
				(double) (Mouse.GetPosition(canvasImage).X - ellipsePixel.Width / 2.0));
			ellipsePixel.SetValue(Canvas.TopProperty,
				(double) (Mouse.GetPosition(canvasImage).Y - ellipsePixel.Width / 2.0));
			canvasImage.InvalidateVisual();
			// Set the Selected Color based on the cursor pixel and Alpha Slider value
			SelectedColor = Color.FromArgb((byte) AlphaSlider.Value, pixels[2], pixels[1], pixels[0]);
			ellipsePixel.SetValue(Canvas.LeftProperty, (double) imageX - ellipsePixel.Width / 2.0);
			ellipsePixel.SetValue(Canvas.TopProperty, (double) imageY - ellipsePixel.Width / 2.0);
		}

		/// <summary>
		/// Update the mouse cursor ellipse position.
		/// </summary>
		private void UpdateCursorEllipse(Color searchColor) {
			// Scan the canvas image for a color which matches the search color
			CroppedBitmap cb;
			var pixels = new byte[4];
			int searchY;
			int searchX;
			var colorSwatch = ColorImage.Source as BitmapSource;
			if (colorSwatch.Format != PixelFormats.Bgra32) {
				colorSwatch = new FormatConvertedBitmap(colorSwatch, PixelFormats.Bgra32, null, 0);
			}

			var bytesPerPixel = (colorSwatch.Format.BitsPerPixel + 7) / 8;
			var stride = colorSwatch.PixelWidth * bytesPerPixel;
			var bufferSize = colorSwatch.PixelHeight * stride;
			var bytes = new byte[bufferSize];

			colorSwatch.CopyPixels(new Int32Rect(0, 0, colorSwatch.PixelWidth, colorSwatch.PixelHeight), bytes, stride, 0);
			for (var y = 0; y < colorSwatch.PixelHeight; y++) {
				for (var x = 0; x < colorSwatch.PixelWidth; x++) {
					if (bytes[y * bytesPerPixel + x] == searchColor.B &&
					    bytes[y * bytesPerPixel + x + 1] == searchColor.G &&
					    bytes[y * bytesPerPixel + x + 2] == searchColor.R) {
			            searchX = x;
			            searchY = y;
			            goto end;
			        }
			    }
			}

			for (searchY = 0; searchY <= canvasImage.Width - 1; searchY++) {
				for (searchX = 0; searchX <= canvasImage.Height - 1; searchX++) {
					cb = new CroppedBitmap(ColorImage.Source as BitmapSource, new Int32Rect(searchX, searchY, 1, 1));
					cb.CopyPixels(pixels, 4, 0);
					if (pixels[2] == searchColor.R && pixels[1] == searchColor.G && pixels[0] == searchColor.B) {
						goto end;
					}
				}
			}
			// Default to the top left if no match is found
			searchX = 0;
			searchY = 0;

			end:
			// Update the mouse cursor ellipse position
			ellipsePixel.SetValue(Canvas.LeftProperty, (double) searchX - ellipsePixel.Width / 2.0);
			ellipsePixel.SetValue(Canvas.TopProperty, (double) searchY - ellipsePixel.Width / 2.0);
		}

		/// <summary>
		/// Update text box values based on the Selected Color.
		/// </summary>
		private void UpdateTextBoxes(Color color) {
			txtAlpha.Text = color.A.ToString();
			txtAlphaHex.Text = color.A.ToString("X2");
			txtRed.Text = color.R.ToString();
			txtRedHex.Text = color.R.ToString("X2");
			txtGreen.Text = color.G.ToString();
			txtGreenHex.Text = color.G.ToString("X2");
			txtBlue.Text = color.B.ToString();
			txtBlueHex.Text = color.B.ToString("X2");
			txtAll.Text = string.Format("#{0}{1}{2}{3}", txtAlphaHex.Text, txtRedHex.Text, txtGreenHex.Text,
				txtBlueHex.Text);
		}

		/// <summary>
		/// Updates the Ink strokes based on the Selected Color.
		/// </summary>
		private void UpdateInk() {
			drawingAttributes.Color = SelectedColor;
			drawingAttributes.StylusTip = StylusTip.Ellipse;
			drawingAttributes.Width = 5;
			// Update drawing attributes on previewPresenter
			foreach (var s in previewPresenter.Strokes) s.DrawingAttributes = drawingAttributes;
		}

		#endregion // Update Methods

		private void txtAlpha_TextChanged(object sender, TextChangedEventArgs e) {
			byte a, r, g, b;
			var success = byte.TryParse(txtAlpha.Text, out a) & byte.TryParse(txtRed.Text, out r) &
			              byte.TryParse(txtGreen.Text, out g) & byte.TryParse(txtBlue.Text, out b);
			if (success) {
				selectedColor = Color.FromArgb(a, r, g, b);
				CreateAlphaLinearBrush();
				UpdateInk();
				AlphaSlider.Value = a;
			}
		}
	}
}