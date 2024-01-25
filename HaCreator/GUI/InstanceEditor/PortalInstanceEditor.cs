/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HaCreator.MapEditor.Info.Default;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.UndoRedo;
using MapleLib.WzLib.WzStructure.Data;
using static HaCreator.GUI.InstanceEditor.EditorTools;

namespace HaCreator.GUI.InstanceEditor {
	public partial class PortalInstanceEditor : EditorBase {
		public PortalInstance item;
		private ControlRowManager rowMan;

		public PortalInstanceEditor(PortalInstance item) {
			InitializeComponent();
			var portalTypes = Program.InfoManager.PortalTypeById.Count;
			var portals = new ArrayList();
			for (var i = 0; i < portalTypes; i++) {
				try {
					portals.Add(Tables.PortalTypeNames[Program.InfoManager.PortalTypeById[i]]);
				} catch (KeyNotFoundException) {
					continue;
				}
			}

			ptComboBox.Items.AddRange(portals.ToArray());
			this.item = item;

			rowMan = new ControlRowManager(new ControlRow[] {
				new ControlRow(new Control[] {pnLabel, pnBox}, 26, "pn"),
				new ControlRow(new Control[] {tmLabel, tmBox, btnBrowseMap, thisMap}, 26, "tm"),
				new ControlRow(new Control[] {tnLabel, tnBox, btnBrowseTn, leftBlankLabel}, 26, "tn"),
				new ControlRow(new Control[] {scriptLabel, scriptBox}, 26, "script"),
				new ControlRow(new Control[] {delayEnable, delayBox}, 26, "delay"),
				new ControlRow(new Control[] {rangeEnable, xRangeLabel, hRangeBox, yRangeLabel, vRangeBox}, 26,
					"range"),
				new ControlRow(new Control[] {impactLabel, hImpactEnable, hImpactBox, vImpactEnable, vImpactBox}, 26,
					"impact"),
				new ControlRow(new Control[] {hideTooltip, onlyOnce}, 26, "bool"),
				new ControlRow(new Control[] {imageLabel, portalImageList, portalImageBox},
					okButton.Top - portalImageList.Top, "image"),
				new ControlRow(new Control[] {okButton, cancelButton}, 26, "buttons")
			}, this);

			delayEnable.Tag = delayBox;
			hImpactEnable.Tag = hImpactBox;
			vImpactEnable.Tag = vImpactBox;

			xInput.Value = item.X;
			yInput.Value = item.Y;
			ptComboBox.SelectedIndex = Program.InfoManager.PortalIdByType[item.pt];
			pnBox.Text = item.pn;
			if (item.tm == item.Board.MapInfo.id) {
				thisMap.Checked = true;
			} else {
				tmBox.Value = item.tm;
			}

			tnBox.Text = item.tn;
			if (item.script != null) scriptBox.Text = item.script;
			LoadOptionalInt(item.delay, delayBox, delayEnable, Defaults.Portal.Delay);
			LoadOptionalInt(item.hRange, hRangeBox, rangeEnable, Defaults.Portal.HRange);
			LoadOptionalInt(item.vRange, vRangeBox, rangeEnable, Defaults.Portal.VRange);
			LoadOptionalInt(item.horizontalImpact, hImpactBox, hImpactEnable, Defaults.Portal.HorizontalImpact);
			LoadOptionalInt(item.verticalImpact, vImpactBox, vImpactEnable, Defaults.Portal.VerticalImpact);
			onlyOnce.Checked = item.onlyOnce;
			hideTooltip.Checked = item.hideTooltip;
			if (item.image != null) portalImageList.SelectedItem = item.image;
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

				if (actions.Count > 0) {
					item.Board.UndoRedoMan.AddUndoBatch(actions);
				}

				item.pt = Program.InfoManager.PortalTypeById[ptComboBox.SelectedIndex];

				item.hRange = Defaults.Portal.HRange;
				item.vRange = Defaults.Portal.VRange;
				item.horizontalImpact = Defaults.Portal.HorizontalImpact;
				item.verticalImpact = Defaults.Portal.VerticalImpact;
				item.delay = Defaults.Portal.Delay;
				// What portals actually use these 2?
				item.onlyOnce = onlyOnce.Checked;
				item.hideTooltip = hideTooltip.Checked;
				if (item.pt == PortalType.Script || item.pt == PortalType.ScriptHidden || item.pt == PortalType.ScriptInvisible ||
				    item.pt == PortalType.ScriptHiddenUng || item.pt == PortalType.CollisionScript) {
					item.script = scriptBox.Text;
				} else {
					item.script = Defaults.Portal.Script;
				}

				switch (item.pt) {
					case PortalType.StartPoint:
						item.pn = "sp";
						item.tm = 999999999;
						item.tn = "";
						break;
					case PortalType.Invisible:
						item.pn = pnBox.Text;
						item.tm = thisMap.Checked ? item.Board.MapInfo.id : (int) tmBox.Value;
						item.tn = tnBox.Text;
						item.delay = GetOptionalInt(delayBox, delayEnable, Defaults.Portal.Delay);
						break;
					case PortalType.Visible:
						item.pn = pnBox.Text;
						item.tm = thisMap.Checked ? item.Board.MapInfo.id : (int) tmBox.Value;
						item.tn = tnBox.Text;
						item.delay = GetOptionalInt(delayBox, delayEnable, Defaults.Portal.Delay);
						break;
					case PortalType.Collision:
						item.pn = pnBox.Text;
						item.tm = thisMap.Checked ? item.Board.MapInfo.id : (int) tmBox.Value;
						item.tn = tnBox.Text;
						item.hRange = GetOptionalInt(hRangeBox, rangeEnable, Defaults.Portal.HRange);
						item.vRange = GetOptionalInt(vRangeBox, rangeEnable, Defaults.Portal.VRange);
						item.delay = GetOptionalInt(delayBox, delayEnable, Defaults.Portal.Delay);
						break;
					case PortalType.Changeable:
						item.pn = pnBox.Text;
						item.tm = thisMap.Checked ? item.Board.MapInfo.id : (int) tmBox.Value;
						item.tn = tnBox.Text;
						item.delay = GetOptionalInt(delayBox, delayEnable, Defaults.Portal.Delay);
						break;
					case PortalType.ChangeableInvisible:
						item.pn = pnBox.Text;
						item.tm = thisMap.Checked ? item.Board.MapInfo.id : (int) tmBox.Value;
						item.tn = tnBox.Text;
						item.delay = GetOptionalInt(delayBox, delayEnable, Defaults.Portal.Delay);
						break;
					case PortalType.TownPortalPoint:
						item.pn = "tp";
						item.tm = 999999999;
						item.tn = "";
						break;
					case PortalType.Script:
						item.pn = pnBox.Text;
						item.tm = 999999999;
						item.tn = "";
						item.script = scriptBox.Text;
						break;
					case PortalType.ScriptInvisible:
						item.pn = pnBox.Text;
						item.tm = 999999999;
						item.tn = "";
						break;
					case PortalType.CollisionScript:
						item.pn = pnBox.Text;
						item.tm = 999999999;
						item.tn = "";
						item.delay = GetOptionalInt(delayBox, delayEnable, Defaults.Portal.Delay);
						break;
					case PortalType.Hidden:
						item.pn = pnBox.Text;
						item.tm = thisMap.Checked ? item.Board.MapInfo.id : (int) tmBox.Value;
						item.tn = tnBox.Text;
						item.delay = GetOptionalInt(delayBox, delayEnable, Defaults.Portal.Delay);
						break;
					case PortalType.ScriptHidden:
						item.pn = pnBox.Text;
						item.tm = 999999999;
						item.tn = "";
						item.delay = GetOptionalInt(delayBox, delayEnable, Defaults.Portal.Delay);
						break;
					case PortalType.CollisionVerticalJump:
						item.pn = pnBox.Text;
						item.tm = 999999999;
						item.tn = tnBox.Text;
						item.delay = GetOptionalInt(delayBox, delayEnable, Defaults.Portal.Delay);
						break;
					case PortalType.CollisionCustomImpact:
						item.pn = pnBox.Text;
						item.tm = 999999999;
						item.tn = "";
						item.horizontalImpact = GetOptionalInt(hImpactBox, hImpactEnable, Defaults.Portal.HorizontalImpact);
						item.verticalImpact = GetOptionalInt(vImpactBox, vImpactEnable, Defaults.Portal.VerticalImpact);
						item.delay = GetOptionalInt(delayBox, delayEnable, Defaults.Portal.Delay);
						break;
					case PortalType.CollisionUnknownPcig:
						item.pn = pnBox.Text;
						item.tm = thisMap.Checked ? item.Board.MapInfo.id : (int) tmBox.Value;
						item.tn = tnBox.Text;
						item.horizontalImpact = GetOptionalInt(hImpactBox, hImpactEnable, Defaults.Portal.HorizontalImpact);
						item.verticalImpact = GetOptionalInt(vImpactBox, vImpactEnable, Defaults.Portal.VerticalImpact);
						item.delay = GetOptionalInt(delayBox, delayEnable, Defaults.Portal.Delay);
						break;
				}

				if (portalImageList.SelectedItem != null &&
				    Program.InfoManager.GamePortals.ContainsKey(
					    Program.InfoManager.PortalTypeById[ptComboBox.SelectedIndex])) {
					item.image = (string) portalImageList.SelectedItem;
				}
			}

			Close();
		}

		private void thisMap_CheckedChanged(object sender, EventArgs e) {
			tmBox.Enabled = !thisMap.Checked;
			btnBrowseMap.Enabled = !thisMap.Checked;
			btnBrowseTn.Enabled = thisMap.Checked;
		}

		private void EnablingCheckBoxCheckChanged(object sender, EventArgs e) {
			((Control) ((CheckBox) sender).Tag).Enabled = ((CheckBox) sender).Checked;
		}

		private void ptComboBox_SelectedIndexChanged(object sender, EventArgs e) {
			btnBrowseTn.Enabled = thisMap.Checked;
			var script = false;
			switch (Program.InfoManager.PortalTypeById[ptComboBox.SelectedIndex]) {
				case PortalType.StartPoint:
					rowMan.SetInvisible("pn");
					rowMan.SetInvisible("tm");
					rowMan.SetInvisible("tn");
					rowMan.SetInvisible("delay");
					rowMan.SetInvisible("range");
					rowMan.SetInvisible("impact");
					rowMan.SetInvisible("bool");
					break;
				case PortalType.Invisible:
					rowMan.SetVisible("pn");
					rowMan.SetVisible("tm");
					rowMan.SetVisible("tn");
					rowMan.SetVisible("delay");
					rowMan.SetInvisible("range");
					rowMan.SetInvisible("impact");
					rowMan.SetVisible("bool");
					break;
				case PortalType.Visible:
					rowMan.SetVisible("pn");
					rowMan.SetVisible("tm");
					rowMan.SetVisible("tn");
					rowMan.SetVisible("delay");
					rowMan.SetInvisible("range");
					rowMan.SetInvisible("impact");
					rowMan.SetVisible("bool");
					break;
				case PortalType.Collision:
					rowMan.SetVisible("pn");
					rowMan.SetVisible("tm");
					rowMan.SetVisible("tn");
					rowMan.SetVisible("delay");
					rowMan.SetVisible("range");
					rowMan.SetInvisible("impact");
					rowMan.SetVisible("bool");
					break;
				case PortalType.Changeable:
					rowMan.SetVisible("pn");
					rowMan.SetVisible("tm");
					rowMan.SetVisible("tn");
					rowMan.SetVisible("delay");
					rowMan.SetInvisible("range");
					rowMan.SetInvisible("impact");
					rowMan.SetVisible("bool");
					break;
				case PortalType.ChangeableInvisible:
					rowMan.SetVisible("pn");
					rowMan.SetVisible("tm");
					rowMan.SetVisible("tn");
					rowMan.SetVisible("delay");
					rowMan.SetInvisible("range");
					rowMan.SetInvisible("impact");
					rowMan.SetVisible("bool");
					break;
				case PortalType.TownPortalPoint:
					rowMan.SetInvisible("pn");
					rowMan.SetInvisible("tm");
					rowMan.SetInvisible("tn");
					rowMan.SetInvisible("delay");
					rowMan.SetInvisible("range");
					rowMan.SetInvisible("impact");
					rowMan.SetInvisible("bool");
					break;
				case PortalType.Script:
				case PortalType.ScriptHidden:
				case PortalType.ScriptInvisible:
					rowMan.SetVisible("pn");
					rowMan.SetInvisible("tm");
					rowMan.SetInvisible("tn");
					rowMan.SetVisible("delay");
					rowMan.SetInvisible("range");
					rowMan.SetInvisible("impact");
					rowMan.SetVisible("bool");
					script = true;
					break;
				case PortalType.CollisionScript:
					rowMan.SetVisible("pn");
					rowMan.SetInvisible("tm");
					rowMan.SetInvisible("tn");
					rowMan.SetVisible("delay");
					rowMan.SetVisible("range");
					rowMan.SetInvisible("impact");
					rowMan.SetVisible("bool");
					script = true;
					break;
				case PortalType.Hidden:
					rowMan.SetVisible("pn");
					rowMan.SetVisible("tm");
					rowMan.SetVisible("tn");
					rowMan.SetVisible("delay");
					rowMan.SetInvisible("range");
					rowMan.SetInvisible("impact");
					rowMan.SetVisible("bool");
					break;
				case PortalType.CollisionVerticalJump:
					rowMan.SetVisible("pn");
					rowMan.SetInvisible("tm");
					rowMan.SetVisible("tn");
					rowMan.SetVisible("delay");
					rowMan.SetInvisible("range");
					rowMan.SetInvisible("impact");
					rowMan.SetVisible("bool");
					break;
				case PortalType.CollisionCustomImpact:
					rowMan.SetVisible("pn");
					rowMan.SetInvisible("tm");
					rowMan.SetVisible("tn");
					rowMan.SetVisible("delay");
					rowMan.SetInvisible("range");
					rowMan.SetVisible("impact");
					rowMan.SetVisible("bool");
					break;
				case PortalType.CollisionUnknownPcig:
					rowMan.SetVisible("pn");
					rowMan.SetVisible("tm");
					rowMan.SetVisible("tn");
					rowMan.SetVisible("delay");
					rowMan.SetInvisible("range");
					rowMan.SetVisible("impact");
					rowMan.SetVisible("bool");
					break;
			}

			if (script) {
				rowMan.SetVisible("script");
			} else {
				rowMan.SetInvisible("script");
			}

			var pt = Program.InfoManager.PortalTypeById[ptComboBox.SelectedIndex];
			leftBlankLabel.Visible = pt == PortalType.CollisionVerticalJump;
			if (pt == PortalType.CollisionVerticalJump) {
				btnBrowseTn.Enabled = true;
			}

			if (!Program.InfoManager.GamePortals.ContainsKey(pt)) {
				rowMan.SetInvisible("image");
			} else {
				portalImageList.Items.Clear();
				portalImageList.Items.Add("default");
				portalImageBox.Image = null;
				rowMan.SetVisible("image");
				foreach (DictionaryEntry image in Program.InfoManager.GamePortals[pt])
					portalImageList.Items.Add(image.Key);
				portalImageList.SelectedIndex = 0;
			}
		}

		private void portalImageList_SelectedIndexChanged(object sender, EventArgs e) {
			lock (item.Board.ParentControl) {
				if (portalImageList.SelectedItem == null) {
					return;
				} else if ((string) portalImageList.SelectedItem == "default") {
					return;
				}
				//portalImageBox.Image = new Bitmap(Program.InfoManager.GamePortals[Program.InfoManager.PortalTypeById[ptComboBox.SelectedIndex]].DefaultImage);
				else {
					portalImageBox.Image =
						new Bitmap(Program.InfoManager.GamePortals[
							Program.InfoManager.PortalTypeById[ptComboBox.SelectedIndex]][
							(string) portalImageList.SelectedItem]);
				}
			}
		}

		private void rangeEnable_CheckedChanged(object sender, EventArgs e) {
			hRangeBox.Enabled = rangeEnable.Checked;
			vRangeBox.Enabled = rangeEnable.Checked;
		}

		private void btnBrowseMap_Click(object sender, EventArgs e) {
			var selector = new LoadMapSelector(tmBox);
			selector.ShowDialog();
		}

		private void btnBrowseTn_Click(object sender, EventArgs e) {
			var tn = TnSelector.Show(item.Board);
			if (tn != null) {
				tnBox.Text = tn;
			}
		}
	}

	public class ControlRow {
		public Control[] controls;
		public bool invisible = false;
		public int rowSize;
		public string rowName;

		public ControlRow(Control[] controls, int rowSize, string rowName) {
			this.controls = controls;
			this.rowSize = rowSize;
			this.rowName = rowName;
		}
	}

	public class ControlRowManager {
		private ControlRow[] rows;
		private Hashtable names = new Hashtable();
		private Form form;

		public ControlRowManager(ControlRow[] rows, Form form) {
			this.form = form;
			this.rows = rows;
			var index = 0;
			foreach (var row in rows)
				names[row.rowName] = index++;
		}

		public void SetInvisible(string name) {
			SetInvisible((int) names[name]);
		}

		public void SetInvisible(int index) {
			var row = rows[index];
			if (row.invisible) return;
			row.invisible = true;
			foreach (var c in row.controls)
				c.Visible = false;
			var size = row.rowSize;
			for (var i = index + 1; i < rows.Length; i++) {
				row = rows[i];
				foreach (var c in row.controls)
					c.Location = new Point(c.Location.X, c.Location.Y - size);
			}

			form.Height -= size;
		}

		public void SetVisible(string name) {
			SetVisible((int) names[name]);
		}

		public void SetVisible(int index) {
			var row = rows[index];
			if (!row.invisible) return;
			row.invisible = false;
			foreach (var c in row.controls)
				c.Visible = true;
			var size = row.rowSize;
			for (var i = index + 1; i < rows.Length; i++) {
				row = rows[i];
				foreach (var c in row.controls)
					c.Location = new Point(c.Location.X, c.Location.Y + size);
			}

			form.Height += size;
		}
	}
}