using System;
using System.Windows.Forms;

namespace HaCreator.GUI.InstanceEditor {
	public class EditorTools {
		public static void LoadOptionalInt(int source, NumericUpDown target, CheckBox checkBox, int def) {
			checkBox.Checked = source != def;
			if (checkBox.Checked) target.Value = source;
		}

		public static int GetOptionalInt(NumericUpDown textbox, CheckBox checkBox, int def) {
			return checkBox.Checked ? decimal.ToInt32(textbox.Value) : def;
		}

		public static void LoadOptionalFloat(float source, NumericUpDown target, CheckBox checkBox, float def) {
			checkBox.Checked = Math.Abs(source - def) > float.Epsilon;
			if (checkBox.Checked) target.Value = (decimal) source;
		}

		public static float GetOptionalFloat(NumericUpDown textbox, CheckBox checkBox, float def) {
			return checkBox.Checked ? (float) textbox.Value : def;
		}

		public static void LoadOptionalString(string source, TextBox target, CheckBox checkBox, string def) {
			checkBox.Checked = !string.Equals(source, def);
			if (checkBox.Checked) target.Text = source;
		}

		public static string GetOptionalString(TextBox textbox, CheckBox checkBox, string def) {
			return checkBox.Checked ? textbox.Text : def;
		}
	}
}