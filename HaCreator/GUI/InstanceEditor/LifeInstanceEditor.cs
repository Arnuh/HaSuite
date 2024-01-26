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
using Microsoft.Xna.Framework;
using static HaCreator.GUI.InstanceEditor.EditorTools;

namespace HaCreator.GUI.InstanceEditor {
	public partial class LifeInstanceEditor : EditorBase {
		public LifeInstance item;

		public LifeInstanceEditor(LifeInstance item) {
			InitializeComponent();
			this.item = item;
			infoEnable.Tag = infoBox;
			limitedNameEnable.Tag = limitedNameBox;
			mobTimeEnable.Tag = mobTimeBox;
			teamEnable.Tag = teamBox;

			xInput.Value = item.X;
			yInput.Value = item.Y;
			rx0Box.Value = item.rx0Shift;
			rx1Box.Value = item.rx1Shift;
			yShiftBox.Value = item.yShift;

			LoadOptionalInt(item.Info, infoBox, infoEnable, Defaults.Life.Info);
			LoadOptionalInt(item.Team, teamBox, teamEnable, Defaults.Life.Team);
			LoadOptionalInt(item.MobTime, mobTimeBox, mobTimeEnable, Defaults.Life.MobTime);
			LoadOptionalString(item.LimitedName, limitedNameBox, limitedNameEnable, Defaults.Life.LimitedName);

			hideBox.Checked = item.Hide;
			flipBox.Checked = item.Flip;

			pathLabel.Text = HaCreatorStateManager.CreateItemDescription(item);
		}

		protected override void cancelButton_Click(object sender, EventArgs e) {
			Close();
		}

		protected override void okButton_Click(object sender, EventArgs e) {
			lock (item.Board.ParentControl) {
				var actions = new List<UndoRedoAction>();
				if (xInput.Value != item.X || yInput.Value != item.Y) {
					actions.Add(UndoRedoManager.ItemMoved(item, new Point(item.X, item.Y),
						new Point((int) xInput.Value, (int) yInput.Value)));
					item.Move((int) xInput.Value, (int) yInput.Value);
				}

				if (actions.Count > 0) {
					item.Board.UndoRedoMan.AddUndoBatch(actions);
				}

				item.rx0Shift = (int) rx0Box.Value;
				item.rx1Shift = (int) rx1Box.Value;
				item.yShift = (int) yShiftBox.Value;
				item.MobTime = GetOptionalInt(mobTimeBox, mobTimeEnable, Defaults.Life.MobTime);
				item.Info = GetOptionalInt(infoBox, infoEnable, Defaults.Life.Info);
				item.Team = GetOptionalInt(teamBox, teamEnable, Defaults.Life.Team);
				//item.TypeStr = GetOptionalStr(typeEnable, typeBox);
				item.LimitedName = GetOptionalString(limitedNameBox, limitedNameEnable, Defaults.Life.LimitedName);

				item.Hide = hideBox.Checked;
				item.Flip = flipBox.Checked;
			}

			Close();
		}

		private void enablingCheckBoxCheckChanged(object sender, EventArgs e) {
			var cbx = (CheckBox) sender;
			((Control) cbx.Tag).Enabled = cbx.Checked;
		}
	}
}