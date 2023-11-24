using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

		private bool _AcceptsReturn = false;

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
			if (ButtonClicked != null)
				ButtonClicked.Invoke(sender, e);

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