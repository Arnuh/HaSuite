/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using HaCreator.MapEditor;
using HaCreator.MapEditor.UndoRedo;
using Microsoft.Xna.Framework;
using Point = Microsoft.Xna.Framework.Point;

namespace HaCreator.GUI.InstanceEditor {
	public partial class GeneralInstanceEditor : EditorBase {
		public BoardItem item;

		public GeneralInstanceEditor(BoardItem item) {
			InitializeComponent();
			this.item = item;
			xInput.Value = item.X;
			yInput.Value = item.Y;
			if (item.Z == -1) {
				zInput.Enabled = false;
			} else {
				zInput.Value = item.Z;
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

				if (zInput.Enabled && item.Z != zInput.Value) {
					actions.Add(UndoRedoManager.ItemZChanged(item, item.Z, (int) zInput.Value));
					item.Z = (int) zInput.Value;
					item.Board.BoardItems.Sort();
				}

				if (actions.Count > 0) {
					item.Board.UndoRedoMan.AddUndoBatch(actions);
				}
			}

			Close();
		}
	}
}