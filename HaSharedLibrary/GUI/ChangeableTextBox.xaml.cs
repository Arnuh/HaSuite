using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace HaSharedLibrary.GUI {
	/// <summary>
	/// Interaction logic for ChangeableTextBoxXAML.xaml
	/// </summary>
	public partial class ChangeableTextBox : UserControl, INotifyPropertyChanged {
		public ChangeableTextBox() {
			InitializeComponent();

			DataContext = this; // set data binding to self.
		}

		#region Exported Fields

		private string _Header = "";

		public string Header {
			get => _Header;
			set {
				_Header = value;
				OnPropertyChanged("Header");
			}
		}

		public string Text {
			get => textBox.Text;
			set {
				textBox.Text = value;
				OnPropertyChanged("Text");
			}
		}

		/// <summary>
		/// Apply button
		/// </summary>
		public bool ApplyButtonEnabled {
			get => applyButton.IsEnabled;
			set {
				applyButton.IsEnabled = value;
				OnPropertyChanged("ButtonEnabled");
			}
		}

		private TextWrapping _TextWrap;

		public TextWrapping TextWrap {
			get => _TextWrap;
			set {
				_TextWrap = value;
				OnPropertyChanged("TextWrap");
			}
		}

		private bool _AcceptsReturn;

		public bool AcceptsReturn {
			get => _AcceptsReturn;
			set {
				_AcceptsReturn = value;
				OnPropertyChanged("AcceptsReturn");
			}
		}

		#endregion

		public event EventHandler ButtonClicked;

		private void applyButton_Click(object sender, RoutedEventArgs e) {
			if (ButtonClicked != null) {
				ButtonClicked.Invoke(sender, e);
			}

			applyButton.IsEnabled = false;
		}

		/// <summary>
		/// On text changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void textBox_TextChanged(object sender, TextChangedEventArgs e) {
			applyButton.IsEnabled = true;
		}

		#region PropertyChanged

		/// <summary>
		/// Property changed event handler to trigger update UI
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}