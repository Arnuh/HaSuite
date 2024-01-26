/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Instance.Shapes;
using HaCreator.MapEditor.UndoRedo;
using Microsoft.Xna.Framework;

namespace HaCreator.GUI.InstanceEditor {
	public partial class RopeInstanceEditor : EditorBase {
		public RopeAnchor item;

		public RopeInstanceEditor(RopeAnchor item) {
			InitializeComponent();
			this.item = item;
			xInput.Value = item.X;
			yInput.Value = item.Y;
			ufBox.Checked = item.ParentRope.uf;
			if (item.ParentRope.ladder) {
				ladderBox.Checked = true;
			} else {
				ladderBox.Checked = false;
			}

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

				item.ParentRope.uf = ufBox.Checked;
				if (item.ParentRope.ladder != ladderBox.Checked) {
					item.ParentRope.OnUserTouchedLadder();
					item.ParentRope.ladder = ladderBox.Checked;
				}
			}

			Close();
		}
	}
}