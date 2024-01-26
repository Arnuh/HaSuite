/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HaRepacker.GUI.Panels;
using HaRepacker.Properties;
using MapleLib.MapleCryptoLib;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;

namespace HaRepacker.GUI {
	public partial class SaveForm : Form {
		private readonly WzNode wzNode;

		private readonly WzFile wzf; // it can either be a WzImage or a WzFile only.
		private readonly WzImage wzImg; // it can either be a WzImage or a WzFile only.

		private readonly bool IsRegularWzFile; // or data.wz

		public string path;
		private readonly MainPanel _mainPanel;


		private bool bIsLoading;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="panel"></param>
		/// <param name="wzNode"></param>
		public SaveForm(MainPanel panel, WzNode wzNode) {
			InitializeComponent();

			MainForm.AddWzEncryptionTypesToComboBox(encryptionBox);

			this.wzNode = wzNode;
			if (wzNode.Tag is WzImage) // Data.wz hotfix file
			{
				wzImg = (WzImage) wzNode.Tag;
				IsRegularWzFile = false;

				versionBox.Enabled = false; // disable, not necessary
				checkBox_64BitFile.Enabled = false; // disable, not necessary
			} else {
				wzf = (WzFile) wzNode.Tag;
				IsRegularWzFile = true;
			}

			_mainPanel = panel;
		}

		/// <summary>
		/// On loading
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SaveForm_Load(object sender, EventArgs e) {
			bIsLoading = true;

			try {
				if (IsRegularWzFile) {
					encryptionBox.SelectedIndex = MainForm.GetIndexByWzMapleVersion(wzf.MapleVersion);
					versionBox.Value = wzf.Version;

					checkBox_64BitFile.Checked = wzf.Is64BitWzFile;
					versionBox.Enabled =
						wzf.Is64BitWzFile
							? false
							: true; // disable checkbox if its checked as 64-bit, since the version will always be 777
				} else { // Data.wz uses BMS encryption... no sepcific version indicated
					encryptionBox.SelectedIndex = MainForm.GetIndexByWzMapleVersion(WzMapleVersion.BMS);
				}
			} finally {
				bIsLoading = false;
			}
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


		/// <summary>
		/// On encryption box selection changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void encryptionBox_SelectedIndexChanged(object sender, EventArgs e) {
			if (bIsLoading) {
				return;
			}

			var selectedIndex = encryptionBox.SelectedIndex;
			var wzMapleVersion = MainForm.GetWzMapleVersionByWzEncryptionBoxSelection(selectedIndex);
			if (wzMapleVersion == WzMapleVersion.CUSTOM) {
				var customWzInputBox = new CustomWZEncryptionInputBox();
				customWzInputBox.ShowDialog();
			} else {
				MapleCryptoConstants.UserKey_WzLib = MapleCryptoConstants.MAPLESTORY_USERKEY_DEFAULT.ToArray();
			}
		}

		/// <summary>
		/// On save button clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SaveButton_Click(object sender, EventArgs e) {
			if (versionBox.Value < 0) {
				Warning.Error(Resources.SaveVersionError);
				return;
			}

			using (var dialog = new SaveFileDialog {
				       Title = Resources.SelectOutWz,
				       FileName = wzNode.Text,
				       Filter = string.Format("{0}|*.wz",
					       Resources.WzFilter)
			       }) {
				if (dialog.ShowDialog() != DialogResult.OK) {
					return;
				}

				var bSaveAs64BitWzFile = checkBox_64BitFile.Checked; // no version number
				var wzMapleVersionSelected =
					MainForm.GetWzMapleVersionByWzEncryptionBoxSelection(encryptionBox
						.SelectedIndex); // new encryption selected
				if (IsRegularWzFile) {
					if (wzf.MapleVersion != wzMapleVersionSelected) PrepareAllImgs(wzf.WzDirectory);

					wzf.Version = (short) versionBox.Value;
					wzf.MapleVersion = wzMapleVersionSelected;

					if (wzf.FilePath != null && wzf.FilePath.ToLower() == dialog.FileName.ToLower()) {
						wzf.SaveToDisk(dialog.FileName + "$tmp", bSaveAs64BitWzFile, wzMapleVersionSelected);
						_mainPanel.MainForm.UnloadWzFile(wzf);
						try {
							File.Delete(dialog.FileName);
							File.Move(dialog.FileName + "$tmp", dialog.FileName);
						} catch (IOException ex) {
							MessageBox.Show("Handle error overwriting WZ file", Resources.Error);
						}
					} else {
						wzf.SaveToDisk(dialog.FileName, bSaveAs64BitWzFile, wzMapleVersionSelected);
						_mainPanel.MainForm.UnloadWzFile(wzf);
					}

					// Reload the new file
					var loadedWzFile = Program.WzFileManager.LoadWzFile(dialog.FileName, wzMapleVersionSelected);
					if (loadedWzFile != null) _mainPanel.MainForm.AddLoadedWzObjectToMainPanel(loadedWzFile);
				} else {
					var WzIv = WzTool.GetIvByMapleVersion(wzMapleVersionSelected);

					// Save file
					var tmpFilePath = dialog.FileName + ".tmp";
					var targetFilePath = dialog.FileName;

					var error_noAdminPriviledge = false;
					try {
						using (var oldfs = File.Open(tmpFilePath, FileMode.OpenOrCreate)) {
							using (var wzWriter = new WzBinaryWriter(oldfs, WzIv)) {
								wzImg.SaveImage(wzWriter); // Write to temp folder
							}
						}

						try {
							File.Copy(tmpFilePath, targetFilePath, true);
							File.Delete(tmpFilePath);
						} catch (Exception exp) {
							Debug.WriteLine(exp); // nvm, dont show to user
						}

						wzNode.DeleteWzNode(); // this is a WzImage, and cannot be unloaded by _mainPanel.MainForm.UnloadWzFile
					} catch (UnauthorizedAccessException) {
						error_noAdminPriviledge = true;
					}

					// Reload the new file
					var img = Program.WzFileManager.LoadDataWzHotfixFile(dialog.FileName, wzMapleVersionSelected);
					if (img == null || error_noAdminPriviledge) {
						MessageBox.Show(Resources.MainFileOpenFail, Resources.Error);
					}

					_mainPanel.MainForm.AddLoadedWzObjectToMainPanel(img);
				}
			}

			Close();
		}


		private void PrepareAllImgs(WzDirectory dir) {
			foreach (var img in dir.WzImages) img.Changed = true;

			foreach (var subdir in dir.WzDirectories) PrepareAllImgs(subdir);
		}

		/// <summary>
		/// On checkBox_64BitFile checked changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void checkBox_64BitFile_CheckedChanged(object sender, EventArgs e) {
			if (bIsLoading) {
				return;
			}

			var checkbox_64 = (CheckBox) sender;
			versionBox.Enabled =
				checkbox_64.Checked !=
				true; // disable checkbox if its checked as 64-bit, since the version will always be 777
		}
	}
}