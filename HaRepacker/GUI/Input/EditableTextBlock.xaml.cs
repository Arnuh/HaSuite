using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HaRepacker.GUI.Input {
	public partial class EditableTextBlock : IComparable, IComparable<EditableTextBlock> {
		#region Constructor

		public EditableTextBlock() {
			InitializeComponent();
			Focusable = true;
			FocusVisualStyle = null;
		}

		#endregion Constructor

		#region Member Variables

		// We keep the old text when we go into editmode
		// in case the user aborts with the escape key
		private string oldText;

		#endregion Member Variables

		#region Properties

		public string Text {
			get => (string) GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register(
				"Text",
				typeof(string),
				typeof(EditableTextBlock),
				new PropertyMetadata(""));

		public bool IsEditable {
			get => (bool) GetValue(IsEditableProperty);
			set => SetValue(IsEditableProperty, value);
		}

		public static readonly DependencyProperty IsEditableProperty =
			DependencyProperty.Register(
				"IsEditable",
				typeof(bool),
				typeof(EditableTextBlock),
				new PropertyMetadata(true));

		public bool IsInEditMode {
			get {
				if (IsEditable) {
					return (bool) GetValue(IsInEditModeProperty);
				} else {
					return false;
				}
			}
			set {
				if (IsEditable) {
					if (value) {
						oldText = Text;
					}

					SetValue(IsInEditModeProperty, value);
				}
			}
		}

		public static readonly DependencyProperty IsInEditModeProperty =
			DependencyProperty.Register(
				"IsInEditMode",
				typeof(bool),
				typeof(EditableTextBlock),
				new PropertyMetadata(false));

		public string TextFormat {
			get => (string) GetValue(TextFormatProperty);
			set {
				if (value == "") {
					value = "{0}";
				}

				SetValue(TextFormatProperty, value);
			}
		}

		public static readonly DependencyProperty TextFormatProperty =
			DependencyProperty.Register(
				"TextFormat",
				typeof(string),
				typeof(EditableTextBlock),
				new PropertyMetadata("{0}"));

		public string FormattedText => string.Format(TextFormat, Text);

		#endregion Properties

		#region Event Handlers

		// Invoked when we enter edit mode.
		private void TextBox_Loaded(object sender, RoutedEventArgs e) {
			var txt = sender as TextBox;

			// Give the TextBox input focus
			txt.Focus();

			txt.SelectAll();
		}

		// Invoked when we exit edit mode.
		private void TextBox_LostFocus(object sender, RoutedEventArgs e) {
			IsInEditMode = false;
			EditListener?.Invoke(this, e);
		}

		// Invoked when the user edits the annotation.
		private void TextBox_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				IsInEditMode = false;
				e.Handled = true;
				EditListener?.Invoke(this, e);
			} else if (e.Key == Key.Escape) {
				IsInEditMode = false;
				Text = oldText;
				e.Handled = true;
			}
		}

		#endregion Event Handlers

		public int CompareTo(object value) {
			if (value == null) {
				return 1;
			}

			return value is EditableTextBlock other ? string.Compare(Text, other.Text, StringComparison.CurrentCulture) : throw new ArgumentException("Other is not an EditableTextBlock");
		}

		public int CompareTo(EditableTextBlock value) {
			return value == null ? 1 : CultureInfo.CurrentCulture.CompareInfo.Compare(Text, value.Text, CompareOptions.None);
		}

		public event Action<object, object> EditListener;
	}
}