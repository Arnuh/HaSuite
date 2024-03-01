﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Windows.Forms;
using HaRepacker.GUI.Panels;
using HaSharedLibrary.Wz;
using MapleLib.WzLib;

namespace HaRepacker.GUI {
	public partial class NewForm : Form {
		private MainPanel panel;

		private bool bIsLoading;

		public NewForm(MainPanel panel) {
			this.panel = panel;
			InitializeComponent();

			Load += NewForm_Load;
		}

		/// <summary>
		/// Process command key on the form
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="keyData"></param>
		/// <returns></returns>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			// ...
			if (keyData == Keys.Escape) {
				Close(); // exit window
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}


		private void NewForm_Load(object sender, EventArgs e) {
			bIsLoading = true;
			try {
				WzEncryptionTypeHelper.Setup(encryptionBox, Program.ConfigurationManager.ApplicationSettings.MapleVersion);
				versionBox.Value = 1;
			} finally {
				bIsLoading = false;
			}
		}

		/// <summary>
		/// On combobox selection changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EncryptionBox_SelectionChanged(object sender, EventArgs e) {
			if (bIsLoading) {
				return;
			}

			var selectedIndex = encryptionBox.SelectedIndex;
			var wzMapleVersion = WzEncryptionTypeHelper.GetWzMapleVersionByWzEncryptionBoxSelection(selectedIndex);
			if (wzMapleVersion == WzMapleVersion.CUSTOM) {
				var customWzInputBox = new CustomWZEncryptionInputBox(Program.ConfigurationManager);
				customWzInputBox.ShowDialog();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Listwz_CheckedChanged(object sender, EventArgs e) {
			copyrightBox.Enabled = true;
			versionBox.Enabled = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DataWZ_CheckedChanged(object sender, EventArgs e) {
			copyrightBox.Enabled = false;
			versionBox.Enabled = false;
		}

		/// <summary>
		/// Selecting regular WZ checkbox
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void regBox_CheckedChanged(object sender, EventArgs e) {
			copyrightBox.Enabled = regBox.Checked;
			versionBox.Enabled = regBox.Checked;

			copyrightBox.Enabled = true;
			versionBox.Enabled = true;
		}

		private void cancelButton_Click(object sender, EventArgs e) {
			Close();
		}

		private void okButton_Click(object sender, EventArgs e) {
			var name = nameBox.Text;

			var wzMapleVersion = WzEncryptionTypeHelper.GetWzMapleVersionByWzEncryptionBoxSelection(encryptionBox.SelectedIndex);
			if (regBox.Checked) {
				var file = new WzFile((short) versionBox.Value, wzMapleVersion);
				file.Header.Copyright = copyrightBox.Text;
				file.Header.RecalculateFileStart();
				file.Name = name + ".wz";
				file.WzDirectory.Name = name + ".wz";
				panel.DataTree.Nodes.Add(new WzNode(file));
			} else if (listBox.Checked) {
				new ListEditor(null, wzMapleVersion).Show();
			} else if (radioButton_hotfix.Checked) {
				var img = new WzImage(name + ".wz");
				img.MarkWzImageAsParsed();

				var node = new WzNode(img);
				panel.DataTree.Nodes.Add(node);
			}

			Close();
		}
	}
}