/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Windows.Forms;
using HaCreator.MapEditor.Instance.Shapes;

namespace HaCreator.GUI.InstanceEditor {
	public partial class FootholdEditor : EditorBase {
		private FootholdLine[] footholds;

		public FootholdEditor(FootholdLine[] footholds) {
			this.footholds = footholds;
			InitializeComponent();

			var force = footholds[0].Force;
			var piece = footholds[0].Piece;
			var cantThrough = footholds[0].CantThrough;
			var forbidFallDown = footholds[0].ForbidFallDown;
			var indeterminate = false;
			for (var i = 1; i < footholds.Length; i++) {
				if (footholds[i].Force != force) {
					indeterminate = true;
					break;
				}
			}

			if (indeterminate) {
				forceEnable.CheckState = CheckState.Indeterminate;
			} else {
				forceEnable.Checked = force != 0;
				if (forceEnable.Checked) forceInt.Value = (int) force;
			}

			indeterminate = false;
			for (var i = 1; i < footholds.Length; i++) {
				if (footholds[i].Piece != piece) {
					indeterminate = true;
					break;
				}
			}

			if (indeterminate) {
				pieceEnable.CheckState = CheckState.Indeterminate;
			} else {
				pieceEnable.Checked = force != 0;
				if (pieceEnable.Checked) pieceInt.Value = piece;
			}

			indeterminate = false;
			for (var i = 1; i < footholds.Length; i++) {
				if (footholds[i].CantThrough != cantThrough) {
					indeterminate = true;
					break;
				}
			}

			if (indeterminate) {
				cantThroughBox.CheckState = CheckState.Indeterminate;
			} else {
				cantThroughBox.Checked = cantThrough;
			}

			indeterminate = false;
			for (var i = 1; i < footholds.Length; i++) {
				if (footholds[i].ForbidFallDown != forbidFallDown) {
					indeterminate = true;
					break;
				}
			}

			if (indeterminate) {
				forbidFallDownBox.CheckState = CheckState.Indeterminate;
			} else {
				forbidFallDownBox.Checked = forbidFallDown;
			}
		}

		protected override void cancelButton_Click(object sender, EventArgs e) {
			Close();
		}

		private void forceEnable_CheckedChanged(object sender, EventArgs e) {
			forceInt.Enabled = forceEnable.CheckState == CheckState.Checked;
		}

		protected override void okButton_Click(object sender, EventArgs e) {
			if (footholds.Length == 0) return;

			lock (footholds[0].Board.ParentControl) {
				if (forceEnable.CheckState != CheckState.Indeterminate) {
					var force = forceEnable.Checked ? decimal.ToDouble(forceInt.Value) : 0.0;
					foreach (var line in footholds) line.Force = force;
				}

				if (pieceEnable.CheckState != CheckState.Indeterminate) {
					var piece = pieceEnable.Checked ? (int) pieceInt.Value : 0;
					foreach (var line in footholds) line.Piece = piece;
				}

				if (cantThroughBox.CheckState != CheckState.Indeterminate) {
					foreach (var line in footholds)
						line.CantThrough = cantThroughBox.Checked;
				}

				if (forbidFallDownBox.CheckState != CheckState.Indeterminate) {
					foreach (var line in footholds)
						line.ForbidFallDown = forbidFallDownBox.Checked;
				}
			}

			Close();
		}
	}
}