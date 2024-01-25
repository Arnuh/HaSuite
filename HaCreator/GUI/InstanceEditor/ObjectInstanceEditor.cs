/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Info.Default;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.UndoRedo;
using static HaCreator.GUI.InstanceEditor.EditorTools;

namespace HaCreator.GUI.InstanceEditor {
	public partial class ObjectInstanceEditor : EditorBase {
		public ObjectInstance item;

		public ObjectInstanceEditor(ObjectInstance item) {
			InitializeComponent();
			cxBox.Tag = cxInt;
			cyBox.Tag = cyInt;
			rxBox.Tag = rxInt;
			ryBox.Tag = ryInt;
			nameEnable.Tag = nameBox;
			questEnable.Tag = new Control[] {questAdd, questRemove, questList};
			tagsEnable.Tag = tagsBox;

			this.item = item;
			xInput.Value = item.X;
			yInput.Value = item.Y;
			zInput.Value = item.Z;
			rBox.Checked = item.r;
			flipBox.Checked = item.Flip;
			hideBox.Checked = item.hide != Defaults.Object.Hide;
			pathLabel.Text = HaCreatorStateManager.CreateItemDescription(item);
			if (Defaults.Object.Name.Equals(item.Name)) {
				nameEnable.Checked = true;
				nameBox.Text = item.Name;
			}

			flowBox.Checked = item.flow;
			LoadOptionalInt(item.rx, rxInt, rxBox, Defaults.Object.RX);
			LoadOptionalInt(item.ry, ryInt, ryBox, Defaults.Object.RY);
			LoadOptionalInt(item.cx, cxInt, cxBox, Defaults.Object.CX);
			LoadOptionalInt(item.cy, cyInt, cyBox, Defaults.Object.CY);
			if (!Defaults.Object.Tags.Equals(item.tags)) {
				tagsEnable.Checked = false;
			} else {
				tagsEnable.Checked = true;
				tagsBox.Text = item.tags;
			}

			if (item.QuestInfo != null) {
				questEnable.Checked = true;
				foreach (var info in item.QuestInfo)
					questList.Items.Add(info);
			}
		}

		protected override void cancelButton_Click(object sender, EventArgs e) {
			Close();
		}

		protected override void okButton_Click(object sender, EventArgs e) {
			lock (item.Board.ParentControl) {
				var actions = new List<UndoRedoAction>();
				if (xInput.Value != item.X || yInput.Value != item.Y) {
					actions.Add(UndoRedoManager.ItemMoved(item, new Microsoft.Xna.Framework.Point(item.X, item.Y),
						new Microsoft.Xna.Framework.Point((int) xInput.Value, (int) yInput.Value)));
					item.Move((int) xInput.Value, (int) yInput.Value);
				}

				if (zInput.Enabled && item.Z != zInput.Value) {
					actions.Add(UndoRedoManager.ItemZChanged(item, item.Z, (int) zInput.Value));
					item.Z = (int) zInput.Value;
					item.Board.BoardItems.Sort();
				}

				if (actions.Count > 0) {
					item.Board.UndoRedoMan.AddUndoBatch(actions);
				}

				item.Name = nameEnable.Checked ? nameBox.Text : Defaults.Object.Name;
				item.flow = flowBox.Checked;
				item.reactor = reactorBox.Checked;
				item.r = rBox.Checked;
				item.Flip = flipBox.Checked;
				item.hide = hideBox.Checked;
				item.rx = GetOptionalInt(rxInt, rxBox, Defaults.Object.RX);
				item.ry = GetOptionalInt(ryInt, ryBox, Defaults.Object.RY);
				item.cx = GetOptionalInt(cxInt, cxBox, Defaults.Object.CX);
				item.cy = GetOptionalInt(cyInt, cyBox, Defaults.Object.CY);
				item.tags = tagsEnable.Checked ? tagsBox.Text : Defaults.Object.Tags;
				if (questEnable.Checked) {
					var questInfo = new List<ObjectInstanceQuest>();
					foreach (ObjectInstanceQuest info in questList.Items) questInfo.Add(info);
					item.QuestInfo = questInfo;
				} else {
					item.QuestInfo = null;
				}
			}

			Close();
		}

		private void enablingCheckBox_CheckChanged(object sender, EventArgs e) {
			var cbx = (CheckBox) sender;
			var featureActivated = cbx.Checked && cbx.Enabled;
			if (cbx.Tag is Control) {
				((Control) cbx.Tag).Enabled = featureActivated;
			} else {
				foreach (var control in (Control[]) cbx.Tag) control.Enabled = featureActivated;
				foreach (var control in (Control[]) cbx.Tag) {
					if (control is CheckBox) {
						enablingCheckBox_CheckChanged(control, e);
					}
				}
			}
		}

		private void questRemove_Click(object sender, EventArgs e) {
			if (questList.SelectedIndex != -1) questList.Items.RemoveAt(questList.SelectedIndex);
		}

		private void questAdd_Click(object sender, EventArgs e) {
			var input = new ObjQuestInput();
			if (input.ShowDialog() == DialogResult.OK) {
				questList.Items.Add(input.result);
			}
		}
	}
}