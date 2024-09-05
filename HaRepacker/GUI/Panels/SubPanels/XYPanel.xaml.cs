using System;
using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace HaRepacker.GUI.Panels.SubPanels {
	/// <summary>
	/// Interaction logic for XYPanelXAML.xaml
	/// </summary>
	public partial class XYPanel : UserControl {
		public XYPanel() {
			InitializeComponent();

			button_ApplyChanges.IsEnabled = false;
		}


		public int X {
			get {
				var val = 0;
				int.TryParse(xBox.Text, out val);
				return val;
			}
			set => xBox.Text = value.ToString();
		}

		public int Y {
			get {
				var val = 0;
				int.TryParse(yBox.Text, out val);
				return val;
			}
			set => yBox.Text = value.ToString();
		}

		public event EventHandler ButtonClicked;

		private void button_ApplyChanges_Click(object sender, RoutedEventArgs e) {
			// Update X Y & check for number formatting
			var val = 0;
			int.TryParse(yBox.Text, out val);
			yBox.Text = val.ToString();


			int.TryParse(xBox.Text, out val);
			xBox.Text = val.ToString();

			// event
			if (ButtonClicked != null) ButtonClicked.Invoke(sender, e);

			button_ApplyChanges.IsEnabled = false;
		}

		private void yBox_TextChanged(object sender, TextChangedEventArgs e) {
			if (button_ApplyChanges != null) {
				button_ApplyChanges.IsEnabled = true;
			}
		}

		private void xBox_TextChanged(object sender, TextChangedEventArgs e) {
			if (button_ApplyChanges != null) {
				button_ApplyChanges.IsEnabled = true;
			}
		}
	}
}