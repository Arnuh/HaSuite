/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.UndoRedo;

namespace HaCreator.GUI.InstanceEditor {
	public partial class MassZmEditor : EditorBase {
		public IContainsLayerInfo[] items;
		private Board board;

		public MassZmEditor(IContainsLayerInfo[] items, Board board, int zm) {
			InitializeComponent();
			this.items = items;
			this.board = board;
			zmInput.Value = zm;
		}

		protected override void cancelButton_Click(object sender, EventArgs e) {
			Close();
		}

		protected override void okButton_Click(object sender, EventArgs e) {
			lock (board.ParentControl) {
				var newZM = (int) zmInput.Value;
				var actions = new List<UndoRedoAction>();
				foreach (var item in items) {
					actions.Add(UndoRedoManager.zMChanged(item, item.PlatformNumber, newZM));
					item.PlatformNumber = newZM;
				}

				if (actions.Count > 0) {
					board.UndoRedoMan.AddUndoBatch(actions);
				}
			}

			Close();
		}
	}
}