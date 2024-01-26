﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Windows.Forms;
using HaRepacker.Properties;

namespace HaRepacker.GUI.Input {
	public partial class IntInputBox : Form {
		private bool bHideNameInputBox;

		public static bool Show(string title,
			string defaultName, int defaultValue,
			out string name, out int? integer, bool bHideNameInputBox = false) {
			var form = new IntInputBox(title);
			form.bHideNameInputBox = bHideNameInputBox;
			if (bHideNameInputBox) {
				form.nameBox.Visible = false;
				form.label_name.Visible = false;
			}

			// Set default value 
			if (defaultName != null) {
				form.nameBox.Text = defaultName;
			}

			if (defaultValue != 0) {
				form.valueBox.Value = defaultValue;
			}

			var result = form.ShowDialog() == DialogResult.OK;
			name = form.nameResult;
			integer = form.intResult;
			return result;
		}

		private string nameResult;
		private int? intResult;

		public IntInputBox(string title) {
			InitializeComponent();
			DialogResult = DialogResult.Cancel;
			Text = title;
		}

		private void nameBox_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == (char) 13) {
				okButton_Click(null, null);
			}
		}

		/// <summary>
		/// On ok clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void okButton_Click(object sender, EventArgs e) {
			if ((nameBox.Text != "" && nameBox.Text != null) || bHideNameInputBox) {
				nameResult = nameBox.Text;
				intResult = valueBox.Value;
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