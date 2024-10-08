﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HaRepacker.Converters;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using static MapleLib.Configuration.UserSettings;
using CheckBox = System.Windows.Controls.CheckBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace HaRepacker.GUI.Panels.SubPanels {
	/// <summary>
	/// Interaction logic for ImageRenderViewer.xaml
	/// </summary>
	public partial class ImageRenderViewer : UserControl, INotifyPropertyChanged {
		private bool isLoading;

		public ImageRenderViewer() {
			isLoading = true; // set isloading 

			InitializeComponent();

			// Set theme color
			if (Program.ConfigurationManager.UserSettings.ThemeColor == (int) UserSettingsThemeColor.Dark) {
				VisualStateManager.GoToState(this, "BlackTheme", false);
			}

			DataContext = this; // set data binding to self.

			Loaded += ImageRenderViewer_Loaded;
		}

		/// <summary>
		/// When the page loads
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ImageRenderViewer_Loaded(object sender, RoutedEventArgs e) {
			try {
				// Set via app settings
				checkbox_crosshair.IsChecked =
					Program.ConfigurationManager.UserSettings.EnableCrossHairDebugInformation;
				checkbox_border.IsChecked = Program.ConfigurationManager.UserSettings.EnableBorderDebugInformation;

				ZoomSlider.Value = Program.ConfigurationManager.UserSettings.ImageZoomLevel;
				ZoomSliderText.Text = ZoomSlider.Value.ToString();
			} finally {
				isLoading = false;
			}
		}

		#region Exported Fields

		private WzNode _ParentWzNode;

		/// <summary>
		/// The parent WZCanvasProperty to display from
		/// </summary>
		public WzNode ParentWzNode {
			get => _ParentWzNode;
			set => _ParentWzNode = value;
		}

		private WzCanvasProperty _ParentWzCanvasProperty;

		/// <summary>
		/// The parent WZCanvasProperty to display from
		/// </summary>
		public WzCanvasProperty ParentWzCanvasProperty {
			get => _ParentWzCanvasProperty;
			set => _ParentWzCanvasProperty = value;
		}

		private ImageSource _Image;

		/// <summary>
		/// The image to display on the canvas
		/// </summary>
		public ImageSource Image {
			get => _Image;
			private set {
				_Image = value;
				OnPropertyChanged("Image");
			}
		}

		private int _ImageWidth, _ImageHeight;

		public void SetImage(Bitmap bmp) {
			Image = bmp.ToWpfBitmap();
			_ImageWidth = bmp.Width;
			_ImageHeight = bmp.Height;
			CalculateDisplaySize();
		}

		private int _Delay;

		/// <summary>
		/// Delay of the image
		/// </summary>
		public int Delay {
			get => _Delay;
			set {
				_Delay = value;
				OnPropertyChanged("Delay");

				textbox_delay.Text = _Delay.ToString();
			}
		}

		private PointF _CanvasVectorOrigin = new PointF(0, 0);

		/// <summary>
		/// 
		/// </summary>
		public PointF CanvasVectorOrigin {
			get => _CanvasVectorOrigin;
			set {
				_CanvasVectorOrigin = value;
				OnPropertyChanged("CanvasVectorOrigin");

				textbox_originX.Text = _CanvasVectorOrigin.X.ToString();
				textbox_originY.Text = _CanvasVectorOrigin.Y.ToString();
			}
		}

		private PointF? _CanvasVectorHead;

		/// <summary>
		/// Head vector 
		/// </summary>
		public PointF? CanvasVectorHead {
			get => _CanvasVectorHead;
			set {
				_CanvasVectorHead = value;
				OnPropertyChanged("CanvasVectorHead");
				OnPropertyChanged("CanvasVectorHeadOffset");

				textbox_headX.Text = _CanvasVectorHead?.X.ToString() ?? "0";
				textbox_headY.Text = _CanvasVectorHead?.Y.ToString() ?? "0";
			}
		}

		public PointF? CanvasVectorHeadOffset =>
			CanvasVectorHead is PointF head ? (PointF?) new PointF(head.X + CanvasVectorOrigin.X, head.Y + CanvasVectorOrigin.Y) : null;

		private PointF? _CanvasVectorLt;

		/// <summary>
		/// lt vector
		/// </summary>
		public PointF? CanvasVectorLt {
			get => _CanvasVectorLt;
			set {
				_CanvasVectorLt = value;
				OnPropertyChanged("CanvasVectorLt");
				OnPropertyChanged("CanvasVectorLtOffset");

				textbox_ltX.Text = _CanvasVectorLt?.X.ToString() ?? "0";
				textbox_ltY.Text = _CanvasVectorLt?.Y.ToString() ?? "0";
			}
		}

		public PointF? CanvasVectorLtOffset =>
			CanvasVectorLt is PointF lt ? (PointF?) new PointF(lt.X + CanvasVectorOrigin.X, lt.Y + CanvasVectorOrigin.Y) : null;

		private PointF? _CanvasVectorRb;

		/// <summary>
		/// rb vector
		/// </summary>
		public PointF? CanvasVectorRb {
			get => _CanvasVectorRb;
			set {
				_CanvasVectorRb = value;
				OnPropertyChanged("CanvasVectorRb");
				OnPropertyChanged("CanvasVectorRbOffset");

				textbox_rbX.Text = _CanvasVectorRb?.X.ToString() ?? "0";
				textbox_rbY.Text = _CanvasVectorRb?.Y.ToString() ?? "0";
			}
		}

		public PointF? CanvasVectorRbOffset =>
			CanvasVectorRb is PointF rb ? (PointF?) new PointF(rb.X + CanvasVectorOrigin.X, rb.Y + CanvasVectorOrigin.Y) : null;

		private int _DisplayWidth;

		/// <summary>
		/// The width of the canvas
		/// </summary>
		public int DisplayWidth {
			get => _DisplayWidth;
			set {
				_DisplayWidth = value;
				OnPropertyChanged("DisplayWidth");
			}
		}

		private int _DisplayHeight;

		/// <summary>
		/// The Height of the image currently displayed on the canvas
		/// </summary>
		public int DisplayHeight {
			get => _DisplayHeight;
			set {
				_DisplayHeight = value;
				OnPropertyChanged("DisplayHeight");
			}
		}

		#endregion

		#region Property Changed

		/// <summary>
		/// Property changed event handler to trigger update UI
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region UI Events

		/// <summary>
		/// Checkbox for crosshair
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void checkbox_crosshair_Checked(object sender, RoutedEventArgs e) {
			if (isLoading) {
				return;
			}

			var checkbox = (CheckBox) sender;
			if (checkbox.IsChecked == true) {
				Program.ConfigurationManager.UserSettings.EnableCrossHairDebugInformation = true;
			} else {
				Program.ConfigurationManager.UserSettings.EnableCrossHairDebugInformation = false;
			}
		}

		/// <summary>
		/// Checkbox for Border
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void checkbox_border_Checked(object sender, RoutedEventArgs e) {
			if (isLoading) {
				return;
			}

			var checkbox = (CheckBox) sender;
			if (checkbox.IsChecked == true) {
				Program.ConfigurationManager.UserSettings.EnableBorderDebugInformation = true;
			} else {
				Program.ConfigurationManager.UserSettings.EnableBorderDebugInformation = false;
			}
		}

		/// <summary>
		/// 'lt' value changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void textbox_lt_TextChanged(object sender, TextChangedEventArgs e) {
			if (isLoading) {
				return;
			}

			button_ltEdit.IsEnabled = true;
		}

		/// <summary>
		/// 'rb' value changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void textbox_rb_TextChanged(object sender, TextChangedEventArgs e) {
			if (isLoading) {
				return;
			}

			button_rbEdit.IsEnabled = true;
		}

		/// <summary>
		/// 'head' value changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void textbox_head_TextChanged(object sender, TextChangedEventArgs e) {
			if (isLoading) {
				return;
			}

			button_headEdit.IsEnabled = true;
		}

		/// <summary>
		///  'vector' value changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void textbox_origin_TextChanged(object sender, TextChangedEventArgs e) {
			if (isLoading) {
				return;
			}

			button_originEdit.IsEnabled = true;
		}

		/// <summary>
		/// 'delay' valeu changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void textbox_delay_TextChanged(object sender, TextChangedEventArgs e) {
			if (isLoading) {
				return;
			}

			button_delayEdit.IsEnabled = true;
		}

		/// <summary>
		/// Easy access to editing image 'lt' properties 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_ltEdit_Click(object sender, RoutedEventArgs e) {
			if (isLoading) {
				return;
			}

			if (!int.TryParse(textbox_ltX.Text, out var newX) || !int.TryParse(textbox_ltY.Text, out var newY)) return;
			UpdatePoint(WzCanvasProperty.LtPropertyName, newX, newY);

			// Update local UI
			CanvasVectorLt = new PointF(newX, newY);

			button_ltEdit.IsEnabled = false;
		}

		private void button_rbEdit_Click(object sender, RoutedEventArgs e) {
			if (isLoading) {
				return;
			}

			if (!int.TryParse(textbox_rbX.Text, out var newX) || !int.TryParse(textbox_rbY.Text, out var newY)) return;
			UpdatePoint(WzCanvasProperty.RbPropertyName, newX, newY);

			// Update local UI
			CanvasVectorRb = new PointF(newX, newY);

			button_rbEdit.IsEnabled = false;
		}

		/// <summary>
		/// Easy access to editing image 'head' properties 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_headEdit_Click(object sender, RoutedEventArgs e) {
			if (isLoading) {
				return;
			}

			if (!int.TryParse(textbox_headX.Text, out var newX) || !int.TryParse(textbox_headY.Text, out var newY)) return;
			UpdatePoint(WzCanvasProperty.HeadPropertyName, newX, newY);

			// Update local UI
			CanvasVectorHead = new PointF(newX, newY);

			button_headEdit.IsEnabled = false;
		}

		/// <summary>
		/// Easy access to editing image 'delay' properties 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_delayEdit_Click(object sender, RoutedEventArgs e) {
			if (isLoading) {
				return;
			}

			if (!int.TryParse(textbox_delay.Text, out var newdelay)) return;
			if (!(_ParentWzCanvasProperty[WzCanvasProperty.AnimationDelayPropertyName] is WzIntProperty intProperty)) {
				var prop = new WzIntProperty(WzCanvasProperty.AnimationDelayPropertyName, newdelay);
				AddNode(prop);
			} else {
				intProperty.Value = newdelay;
			}

			// Update local UI
			Delay = newdelay;

			button_delayEdit.IsEnabled = false;
		}

		/// <summary>
		/// Easy access to editing image 'origin' properties 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_originEdit_Click(object sender, RoutedEventArgs e) {
			if (isLoading) {
				return;
			}

			if (!int.TryParse(textbox_originX.Text, out var newX) || !int.TryParse(textbox_originY.Text, out var newY)) return;
			var xDiff = newX - CanvasVectorOrigin.X;
			var yDiff = newY - CanvasVectorOrigin.Y;
			UpdatePoint(WzCanvasProperty.OriginPropertyName, newX, newY);

			// Update local UI
			CanvasVectorOrigin = new PointF(newX, newY);

			UpdateOthers(xDiff, yDiff);

			button_originEdit.IsEnabled = false;
		}

		private bool ProgramZoomEdit = false;

		/// <summary>
		/// Image zoom level on value changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (isLoading) {
				return;
			}

			if (ProgramZoomEdit) {
				return;
			}

			var zoomSlider = (Slider) sender;
			ProgramZoomEdit = true;
			lock (ZoomSlider) {
				Program.ConfigurationManager.UserSettings.ImageZoomLevel = zoomSlider.Value;
				ZoomSliderText.Text = zoomSlider.Value.ToString();
			}

			ProgramZoomEdit = false;
		}

		private void ZoomSliderText_OnTextChanged(object sender, TextChangedEventArgs e) {
			if (isLoading) {
				return;
			}

			if (ProgramZoomEdit) {
				return;
			}

			var zoom = (TextBox) sender;
			ProgramZoomEdit = true;
			lock (ZoomSlider) {
				if (double.TryParse(zoom.Text, out var newZoom)) {
					Program.ConfigurationManager.UserSettings.ImageZoomLevel = newZoom;
					ZoomSlider.Value = newZoom;
				}
			}

			ProgramZoomEdit = false;
		}

		private void ZoomSliderReset_OnClick(object sender, RoutedEventArgs e) {
			ZoomSlider.Value = 3;
		}

		private Point _positionInBlock;
		private PointF _startPosition;

		private void Head_OnMouseDown(object sender, MouseButtonEventArgs e) {
			if (!(sender is Grid uiElement)) return;
			_positionInBlock = Mouse.GetPosition(VisualTreeHelper.GetParent(uiElement) as UIElement);
			PointF? point;
			switch (uiElement.Name) {
				case "OriginCrosshair":
					point = CanvasVectorOrigin;
					break;
				case "HeadCrosshair":
					point = CanvasVectorHead;
					break;
				case "LtCrosshair":
					point = CanvasVectorLt;
					break;
				case "RbCrosshair":
					point = CanvasVectorRb;
					break;
				default:
					throw new ArgumentException();
			}

			_startPosition = new PointF(point?.X ?? 0, point?.Y ?? 0);
			uiElement.CaptureMouse();
		}

		private void Head_OnMouseUp(object sender, MouseButtonEventArgs e) {
			if (!(sender is UIElement uiElement)) return;
			uiElement.ReleaseMouseCapture();
		}

		private void Head_OnMouseMove(object sender, MouseEventArgs e) {
			if (!(sender is Grid uiElement)) return;
			if (!uiElement.IsMouseCaptured) return;
			e.Handled = true;

			var mousePosition = Mouse.GetPosition(VisualTreeHelper.GetParent(uiElement) as UIElement);
			var x = (int) Math.Round(_startPosition.X + (mousePosition.X - _positionInBlock.X));
			var y = (int) Math.Round(_startPosition.Y + (mousePosition.Y - _positionInBlock.Y));
			var point = new PointF(x, y);
			switch (uiElement.Name) {
				case "OriginCrosshair":
					var xDiff = x - CanvasVectorOrigin.X;
					var yDiff = y - CanvasVectorOrigin.Y;
					CanvasVectorOrigin = point;
					UpdatePoint(WzCanvasProperty.OriginPropertyName, x, y);
					UpdateOthers(xDiff, yDiff);
					button_originEdit.IsEnabled = false;
					break;
				case "HeadCrosshair":
					CanvasVectorHead = point;
					UpdatePoint(WzCanvasProperty.HeadPropertyName, x, y);
					button_headEdit.IsEnabled = false;
					break;
				case "LtCrosshair":
					CanvasVectorLt = point;
					UpdatePoint(WzCanvasProperty.LtPropertyName, x, y);
					button_ltEdit.IsEnabled = false;
					break;
				case "RbCrosshair":
					CanvasVectorRb = point;
					UpdatePoint(WzCanvasProperty.RbPropertyName, x, y);
					button_rbEdit.IsEnabled = false;
					break;
				default:
					throw new ArgumentException();
			}
		}

		#endregion

		private void AddNode(WzImageProperty prop) {
			_ParentWzNode.AddNode(new WzNode(prop, true), false);
		}

		private void UpdatePoint(string propName, int x, int y) {
			if (!(_ParentWzCanvasProperty[propName] is WzVectorProperty vectorProp)) {
				var prop = new WzVectorProperty(propName, x, y);
				AddNode(prop);
			} else {
				vectorProp.X.Value = x;
				vectorProp.Y.Value = y;
			}

			CalculateDisplaySize();
		}

		private void UpdateOthers(float xDiff, float yDiff) {
			if (CanvasVectorHead != null) {
				var x = (int) (CanvasVectorHead?.X - xDiff ?? 0);
				var y = (int) (CanvasVectorHead?.Y - yDiff ?? 0);
				CanvasVectorHead = new PointF(x, y);
				UpdatePoint(WzCanvasProperty.HeadPropertyName, x, y);
				button_headEdit.IsEnabled = false;
			}

			if (CanvasVectorLt != null) {
				var x = (int) (CanvasVectorLt?.X - xDiff ?? 0);
				var y = (int) (CanvasVectorLt?.Y - yDiff ?? 0);
				CanvasVectorLt = new PointF(x, y);
				UpdatePoint(WzCanvasProperty.LtPropertyName, x, y);
				button_ltEdit.IsEnabled = false;
			}

			if (CanvasVectorRb != null) {
				var x = (int) (CanvasVectorRb?.X - xDiff ?? 0);
				var y = (int) (CanvasVectorRb?.Y - yDiff ?? 0);
				CanvasVectorRb = new PointF(x, y);
				UpdatePoint(WzCanvasProperty.RbPropertyName, x, y);
				button_rbEdit.IsEnabled = false;
			}

			CalculateDisplaySize();
		}

		private void CalculateDisplaySize() {
			// starts at 0? idk
			// + 1 is needed in some cases for border
			// otherwise border overlaps the image
			DisplayWidth = Math.Max(_ImageWidth + 1, (int) CanvasVectorOrigin.X + 11);
			DisplayHeight = Math.Max(_ImageHeight + 1, (int) CanvasVectorOrigin.Y + 9);

			if (CanvasVectorHead != null) {
				DisplayWidth = Math.Max(DisplayWidth, (int) (CanvasVectorHeadOffset?.X + 11 ?? 0));
				DisplayHeight = Math.Max(DisplayHeight, (int) (CanvasVectorHeadOffset?.Y + 9 ?? 0));
			}

			if (CanvasVectorRb != null) {
				DisplayWidth = Math.Max(DisplayWidth, (int) (CanvasVectorRbOffset?.X + 11 ?? 0));
				DisplayHeight = Math.Max(DisplayHeight, (int) (CanvasVectorRbOffset?.Y + 9 ?? 0));
			}
		}

		public void PreLoad() {
			isLoading = true;
		}

		public void PostLoad() {
			isLoading = false;
			// Lets you click save to create a default crosshair and then drag it around
			button_headEdit.IsEnabled = CanvasVectorHead == null;
			button_ltEdit.IsEnabled = CanvasVectorLt == null;
			button_rbEdit.IsEnabled = CanvasVectorRb == null;
			CalculateDisplaySize();
		}
	}
}