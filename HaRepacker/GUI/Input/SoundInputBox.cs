﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using System.Windows.Forms;
using HaRepacker.Properties;

namespace HaRepacker.GUI.Input {
	public partial class SoundInputBox : Form {
		public static bool Show(string title, out string name, out string path) {
			var form = new SoundInputBox(title);
			var result = form.ShowDialog() == DialogResult.OK;
			name = form.nameResult;
			path = form.soundResult;
			return result;
		}

		private string nameResult;
		private string soundResult;

		public SoundInputBox(string title) {
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
			if (pathBox.Text != "" && pathBox.Text != null && nameBox.Text != "" && nameBox.Text != null &&
			    File.Exists(pathBox.Text)) {
				nameResult = nameBox.Text;
				soundResult = pathBox.Text;
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

		private void browseButton_Click(object sender, EventArgs e) {
			var dialog = new OpenFileDialog {
				Title = Resources.SelectMp3,
				Filter = string.Format("{0}|*.mp3", Resources.Mp3Filter)
			};
			if (dialog.ShowDialog() == DialogResult.OK) pathBox.Text = dialog.FileName;
		}
	}
}