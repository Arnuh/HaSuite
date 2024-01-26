﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using HaCreator.MapEditor;

namespace HaCreator.GUI.InstanceEditor {
	public partial class TnSelector : EditorBase {
		public static string Show(Board board) {
			var ps = new TnSelector(board);
			ps.ShowDialog();
			return ps.result;
		}

		private string result;

		public TnSelector(Board board) {
			InitializeComponent();

			foreach (var pi in board.BoardItems.Portals) {
				if (pi.pn != null && pi.pn != "" && pi.pn != "sp" && pi.pn != "pt") {
					pnList.Items.Add(pi.pn);
				}
			}
		}

		protected override void cancelButton_Click(object sender, EventArgs e) {
			Close();
		}

		protected override void okButton_Click(object sender, EventArgs e) {
			result = (string) pnList.SelectedItem;
			Close();
		}
	}
}