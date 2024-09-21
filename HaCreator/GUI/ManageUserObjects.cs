/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Instance;
using HaSharedLibrary.Wz;
using MapleLib.WzLib;
using MapleLib.WzLib.WzStructure;

namespace HaCreator.GUI {
	public partial class ManageUserObjects : Form {
		private UserObjectsManager userObjs;

		public ManageUserObjects(UserObjectsManager userObjs) {
			this.userObjs = userObjs;
			InitializeComponent();
		}

		private void ManageUserObjects_KeyDown(object sender, KeyEventArgs e) {
			e.Handled = true;
			if (e.KeyCode == Keys.Escape) {
				Close();
			}
		}

		private void ManageUserObjects_Load(object sender, EventArgs e) {
			var oldProps = new List<WzImageProperty>();
			var newProps = new List<WzImageProperty>();
			foreach (var prop in userObjs.L1Property.WzProperties) {
				(userObjs.NewObjects.Any(x => x.l2 == prop.Name) ? newProps : oldProps).Add(prop);
			}

			oldProps.ForEach(x => objsList.Items.Add(new UserObject(x, false)));
			newProps.ForEach(x => objsList.Items.Add(new UserObject(x, true)));
		}

		private void objsList_DrawItem(object sender, DrawItemEventArgs e) {
			if (e.Index == -1) {
				return;
			}

			e.DrawBackground();
			var g = e.Graphics;

			// Background
			g.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);
			// Foreground
			var obj = (UserObject) objsList.Items[e.Index];
			g.DrawString(obj.ToString(), e.Font, new SolidBrush(obj.newObj ? Color.Green : e.ForeColor),
				new PointF(e.Bounds.X, e.Bounds.Y));

			e.DrawFocusRectangle();
		}

		private void removeBtn_Click(object sender, EventArgs e) {
			if (MessageBox.Show("This action CANNOT BE UNDONE, are you sure you want to remove this object?",
				    "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) {
				return;
			}

			var index = objsList.SelectedIndex;
			var obj = (UserObject) objsList.SelectedItem;
			userObjs.Remove(obj.ToString());
			objsList.Items.Remove(obj);
			objsList.SelectedIndex = Math.Min(index, objsList.Items.Count - 1);
		}

		private void objsList_SelectedIndexChanged(object sender, EventArgs e) {
			removeBtn.Enabled = searchBtn.Enabled = objsList.SelectedItem != null;
		}

		private void searchBtn_Click(object sender, EventArgs e) {
			var uobj = (UserObject) objsList.SelectedItem;
			var l2 = uobj.ToString();
			CancelableWaitWindow cww = null;
			if (!uobj.newObj) {
				if (MessageBox.Show(
					    "This will search the entire Map.wz for usages of this object. It may take a while. Proceed?",
					    "Search", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes) {
					return;
				}

				cww = new CancelableWaitWindow("Searching...",
					() => SearchMapWzForObj(l2).Concat(SearchEditorForObj(l2).Select(x => x + " (In Editor)")));
			} else {
				cww = new CancelableWaitWindow("Searching...",
					() => SearchEditorForObj(l2).Select(x => x + " (In Editor)"));
			}

			cww.ShowDialog();
			if (cww.result == null) {
				return;
			}

			var result = ((IEnumerable<string>) cww.result).ToList();
			if (result.Count > 0) {
				MessageBox.Show(
					"The object is used in the following maps:\r\n\r\n" + result.Aggregate((x, y) => x + ", " + y),
					"Search Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
			} else {
				MessageBox.Show("The object is not used in any maps.", "Search Results", MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}

		private List<string> SearchEditorForObj(string l2) {
			var result = new List<string>();
			foreach (var board in userObjs.MultiBoard.Boards)
			foreach (var li in board.BoardItems.TileObjs) {
				if (li is ObjectInstance) {
					var oi = (ObjectInfo) li.BaseInfo;
					if (oi.oS == UserObjectsManager.oS &&
					    oi.l0 == Program.APP_NAME &&
					    oi.l1 == UserObjectsManager.l1 &&
					    oi.l2 == l2) {
						result.Add(board.MapInfo.id.ToString());
						break;
					}
				}
			}

			return result;
		}

		private List<string> SearchMapWzForObj(string l2) {
			var result = new List<string>();
			foreach (var mapDir in ((WzDirectory) Program.WzManager["map"]["Map"]).WzDirectories)
			foreach (var mapImg in mapDir.WzImages) {
				var fastForwardToNext = false;
				var parsed = mapImg.Parsed;
				if (!parsed) {
					mapImg.ParseImage();
				}

				foreach (var layer in mapImg.WzProperties) {
					if (layer.Name.Length != 1 || !char.IsDigit(layer.Name[0])) {
						continue;
					}

					var prop = layer["obj"];
					if (prop == null) {
						continue;
					}

					foreach (var obj in prop.WzProperties) {
						if (InfoTool.GetString(obj["oS"]) == UserObjectsManager.oS &&
						    InfoTool.GetString(obj["l0"]) == Program.APP_NAME &&
						    InfoTool.GetString(obj["l1"]) == UserObjectsManager.l1 &&
						    InfoTool.GetString(obj["l2"]) == l2) {
							result.Add(WzInfoTools.RemoveExtension(mapImg.Name));
							fastForwardToNext = true;
							break;
						}
					}

					if (fastForwardToNext) {
						break;
					}
				}

				if (!parsed) {
					mapImg.UnparseImage();
				}
			}

			return result;
		}
	}

	public struct UserObject {
		public bool newObj;
		public WzImageProperty prop;

		public UserObject(WzImageProperty prop, bool newObj) {
			this.prop = prop;
			this.newObj = newObj;
		}

		public override string ToString() {
			return prop.Name;
		}
	}
}