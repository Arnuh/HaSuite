/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using HaCreator.MapEditor.Input;
using HaCreator.Wz;
using TabControl = System.Windows.Controls.TabControl;

namespace HaCreator.MapEditor {
	public class BackupManager {
		private const string userObjsFileName = "userobjs.ham";

		private InputHandler input;
		private MultiBoard multiBoard;
		private readonly HaCreatorStateManager hcsm;
		private TabControl tabs;
		private bool enabled;

		public BackupManager(MultiBoard multiBoard, InputHandler input, HaCreatorStateManager hcsm,
			TabControl tabs) {
			this.input = input;
			this.multiBoard = multiBoard;
			this.hcsm = hcsm;
			this.tabs = tabs;
		}

		private string GetBasePath() {
			return Path.Combine(Program.GetLocalSettingsFolder(), "Backups");
		}

		public void Start() {
			enabled = true;
			// Initialize timers
			input.OnBackup();
			input.OnUserInteraction();
		}

		public void BackupCheck() {
			if (!enabled || !UserSettings.BackupEnabled) {
				return;
			}

			if (input.IsUserIdleFor(UserSettings.BackupIdleTime) ||
			    input.IsBackupDelayedFor(UserSettings.BackupMaxTime)) {
				lock (multiBoard) {
					if (multiBoard.SelectedBoard == null ||
					    multiBoard.SelectedBoard.Mouse == null ||
					    multiBoard.SelectedBoard.Mouse.State != MouseState.Selection ||
					    multiBoard.SelectedBoard.Mouse.BoundItems.Count > 0) {
						return;
					}
				}

				input.OnBackup();

				// We really don't want to hang on IO while multiBoard is locked, so we queue it
				// This also prevents from having to lock both BackupManager and MultiBoard, which might deadlock
				var ioQueue = new Dictionary<string, string>();

				// We also don't want to serialize while locked, since not all of the serialization process requires locking
				var serQueue = new Dictionary<string, SerializationManager>();

				// BackupManager serves as the backup file access lock, to avoid conflicts when writing files
				lock (this) {
					lock (multiBoard) {
						if (multiBoard.UserObjects.Dirty) {
							multiBoard.UserObjects.Dirty = false;
							ioQueue.Add(userObjsFileName, multiBoard.UserObjects.SerializedForm);
						}

						foreach (var board in multiBoard.Boards) {
							if (board.Dirty) {
								board.Dirty = false;
								serQueue.Add(board.UniqueID + ".ham", board.SerializationManager);
							}
						}
					}

					// Execute the serialization queue
					if (serQueue.Count > 0) {
						foreach (var serReq in serQueue)
							ioQueue.Add(serReq.Key, serReq.Value.SerializeBoard(false));
					}


					// Execute the IO queue
					if (ioQueue.Count > 0) {
						var basePath = GetBasePath();
						if (!Directory.Exists(basePath)) {
							Directory.CreateDirectory(basePath);
						}

						foreach (var ioRequest in ioQueue)
							File.WriteAllText(Path.Combine(basePath, ioRequest.Key), ioRequest.Value);
					}
				}
			}
		}

		public void DeleteBackup(int uid) {
			lock (this) {
				var basePath = GetBasePath();
				var backup = Path.Combine(basePath, uid + ".ham");
				if (File.Exists(backup)) {
					File.Delete(backup);
				}
			}
		}

		public void ClearBackups() {
			lock (this) {
				var basePath = GetBasePath();
				if (Directory.Exists(basePath)) {
					Directory.Delete(basePath, true);
				}
			}
		}

		public bool AttemptRestore() {
			var loadedFiles = new Dictionary<string, string>();
			lock (this) {
				var basePath = GetBasePath();
				if (!Directory.Exists(basePath)) {
					return false;
				}

				if (MessageBox.Show(
					    "PheCreator was shut down unexpectedly, and can attempt to recover from a backed up state automatically. Proceed?\r\n\r\n(To start from scratch, press \"No\")",
					    "Recovery", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No) {
					ClearBackups();
					return false;
				}

				foreach (var file in new DirectoryInfo(basePath).GetFiles())
					loadedFiles.Add(file.Name.ToLower(), File.ReadAllText(file.FullName));
			}

			if (loadedFiles.Count == 0) {
				return false;
			}

			lock (multiBoard) {
				if (loadedFiles.ContainsKey(userObjsFileName)) {
					multiBoard.UserObjects.DeserializeObjects(loadedFiles[userObjsFileName]);
					loadedFiles.Remove(userObjsFileName);
				}

				foreach (var file in loadedFiles) {
					if (Path.GetExtension(file.Key) != ".ham") {
						continue;
					}

					//MapLoader.CreateMapFromHam(multiBoard, tabs, file.Value, hcsm.MakeRightClickHandler());
				}
			}

			ClearBackups();
			hcsm.LoadMap();
			return true;
		}
	}
}