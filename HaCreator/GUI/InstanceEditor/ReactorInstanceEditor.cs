/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.UndoRedo;
using Microsoft.Xna.Framework;

namespace HaCreator.GUI.InstanceEditor {
	public partial class ReactorInstanceEditor : EditorBase {
		public ReactorInstance item;

		public ReactorInstanceEditor(ReactorInstance item) {
			InitializeComponent();
			this.item = item;
			xInput.Value = item.X;
			yInput.Value = item.Y;
			pathLabel.Text = HaCreatorStateManager.CreateItemDescription(item);
			if (item.Name == null) {
				useName.Checked = false;
			} else {
				nameBox.Text = item.Name;
			}

			timeBox.Value = item.ReactorTime;
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

				item.Name = useName.Checked ? nameBox.Text : null;
				item.ReactorTime = (int) timeBox.Value;
			}

			Close();
		}

		private void useName_CheckedChanged(object sender, EventArgs e) {
			nameBox.Enabled = useName.Checked;
		}
	}
}