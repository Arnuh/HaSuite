﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Windows.Forms;
using HaRepacker.Properties;

namespace HaRepacker.GUI.Input {
	public partial class FloatingPointInputBox : Form {
		public static bool Show(string title, out string name, out double? value) {
			var form = new FloatingPointInputBox(title);
			var result = form.ShowDialog() == DialogResult.OK;
			name = form.nameResult;
			value = form.doubleResult;
			return result;
		}

		private string nameResult;
		private double? doubleResult;

		public FloatingPointInputBox(string title) {
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
			if (nameBox.Text != "" && nameBox.Text != null) {
				nameResult = nameBox.Text;
				doubleResult = valueBox.Value;
				DialogResult = DialogResult.OK;
				Close();
			} else {
				MessageBox.Show(Resources.EnterValidInput, Resources.Warning,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void cancelButton_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}