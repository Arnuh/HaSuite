﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.UndoRedo;

namespace HaCreator.GUI.InstanceEditor {
	public partial class TileInstanceEditor : EditorBase {
		public TileInstance item;

		public TileInstanceEditor(TileInstance item) {
			InitializeComponent();
			this.item = item;
			xInput.Value = item.X;
			yInput.Value = item.Y;
			zInput.Value = item.Z;
			pathLabel.Text = HaCreatorStateManager.CreateItemDescription(item);
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

				if (item.Z != zInput.Value) {
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