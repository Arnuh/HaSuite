/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Info.Default;
using HaCreator.MapEditor.Instance.Shapes;
using HaCreator.MapEditor.UndoRedo;
using Microsoft.Xna.Framework;

namespace HaCreator.GUI.InstanceEditor {
	public partial class TooltipInstanceEditor : EditorBase {
		public ToolTipInstance item;

		public TooltipInstanceEditor(ToolTipInstance item) {
			InitializeComponent();
			this.item = item;
			xInput.Value = item.X;
			yInput.Value = item.Y;
			pathLabel.Text = HaCreatorStateManager.CreateItemDescription(item);
			
			titleBox.Text = item.Title;

			if (!item.Desc.Equals(Defaults.ToolTip.Desc)) {
				useDescBox.Checked = true;
				descBox.Text = item.Desc;
			}
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

				item.Title = titleBox.Text;
				item.Desc = useDescBox.Checked ? descBox.Text : Defaults.ToolTip.Desc;
			}

			Close();
		}

		private void useDescBox_CheckedChanged(object sender, EventArgs e) {
			descBox.Enabled = useDescBox.Checked;
		}
	}
}