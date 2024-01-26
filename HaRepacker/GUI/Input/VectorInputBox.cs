/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Drawing;
using System.Windows.Forms;

namespace HaRepacker.GUI.Input {
	public partial class VectorInputBox : Form {
		public static bool Show(string title, out string name, out Point? pt) {
			var form = new VectorInputBox(title);
			var result = form.ShowDialog() == DialogResult.OK;
			name = form.nameResult;
			pt = form.pointResult;
			return result;
		}

		private string nameResult;
		private Point? pointResult;

		public VectorInputBox(string title) {
			InitializeComponent();
			DialogResult = DialogResult.Cancel;
			Text = title;
		}

		private void nameBox_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == (char) 13) {
				okButton_Click(null, null);
			}
		}

		private void okButton_Click(object sender, EventArgs e) {
			var x = xBox.Value;
			var y = yBox.Value;
			nameResult = resultBox.Text;
			pointResult = new Point(x, y);
			DialogResult = DialogResult.OK;
			Close();
		}

		private void cancelButton_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}