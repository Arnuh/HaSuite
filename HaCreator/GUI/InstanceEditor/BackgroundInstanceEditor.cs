﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HaCreator.MapEditor;
using MapleLib.WzLib.WzStructure.Data;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.UndoRedo;
using HaCreator.MapSimulator;
using System.Windows.Controls;
using HaCreator.MapEditor.Info;
using Spine;
using HaSharedLibrary.Render.DX;

namespace HaCreator.GUI.InstanceEditor {
	public partial class BackgroundInstanceEditor : EditorBase {
		public BackgroundInstance item;

		public BackgroundInstanceEditor(BackgroundInstance item) {
			InitializeComponent();

			this.item = item;
			xInput.Value = item.BaseX;
			yInput.Value = item.BaseY;
			if (item.Z == -1)
				zInput.Enabled = false;
			else
				zInput.Value = item.Z;

			pathLabel.Text = HaCreatorStateManager.CreateItemDescription(item);
			typeBox.Items.AddRange((object[]) Tables.BackgroundTypeNames.Cast<object>());
			typeBox.SelectedIndex = (int) item.type;
			alphaBox.Value = item.a;
			front.Checked = item.front;

			rxBox.Value = item.rx;
			trackBar_parallaxX.Value = item.rx;
			ryBox.Value = item.ry;
			trackBar_parallaxY.Value = item.ry;

			cxBox.Value = item.cx;
			cyBox.Value = item.cy;

			// Resolutions
			foreach (RenderResolution val in Enum.GetValues(typeof(RenderResolution))) {
				var comboBoxItem = new ComboBoxItem {
					Tag = val,
					Content = RenderResolutionExtensions.ToReadableString(val)
				};

				comboBox_screenMode.Items.Add(comboBoxItem);
			}

			comboBox_screenMode.DisplayMember = "Content";

			var i = 0;
			foreach (ComboBoxItem citem in comboBox_screenMode.Items) {
				if ((int) (RenderResolution) citem.Tag == item.screenMode) {
					comboBox_screenMode.SelectedIndex = i;
					break;
				}

				i++;
			}

			if (item.screenMode < 0)
				comboBox_screenMode.SelectedIndex = 0;

			// Spine
			var baseInfo = (BackgroundInfo) item.BaseInfo;
			if (baseInfo.WzSpineAnimationItem == null) {
				groupBox_spine.Enabled = false; // disable editing
			}
			else {
				groupBox_spine.Enabled = true; // editing

				foreach (var ani in baseInfo.WzSpineAnimationItem.SkeletonData.Animations) {
					var comboBoxItem = new ComboBoxItem();
					comboBoxItem.Tag = ani;
					comboBoxItem.Content = ani.Name;

					comboBox_spineAnimation.Items.Add(comboBoxItem);
				}

				comboBox_spineAnimation.DisplayMember = "Content";

				var i_animation = 0;
				foreach (ComboBoxItem citem in comboBox_spineAnimation.Items) {
					if (((Animation) citem.Tag).Name == item.SpineAni) {
						comboBox_spineAnimation.SelectedIndex = i_animation;
						break;
					}

					i_animation++;
				}

				// spineRandomStart checkbox
				checkBox_spineRandomStart.Checked = item.SpineRandomStart;
			}
		}

		protected override void cancelButton_Click(object sender, EventArgs e) {
			Close();
		}

		protected override void okButton_Click(object sender, EventArgs e) {
			var bgType = (BackgroundType) typeBox.SelectedIndex;
			if ((cyBox.Value < 0 && bgType != BackgroundType.Regular) ||
			    (cxBox.Value < 0 && bgType == BackgroundType.Regular)) {
				MessageBox.Show(
					"You may not select a negative CX or CY value while selecting a non-regular background type.",
					"Error", MessageBoxButtons.OK);
				return;
			}

			lock (item.Board.ParentControl) {
				var actions = new List<UndoRedoAction>();
				var sort = false;
				if (xInput.Value != item.BaseX || yInput.Value != item.BaseY) {
					actions.Add(UndoRedoManager.BackgroundMoved(item,
						new Microsoft.Xna.Framework.Point(item.BaseX, item.BaseY),
						new Microsoft.Xna.Framework.Point((int) xInput.Value, (int) yInput.Value)));
					item.MoveBase((int) xInput.Value, (int) yInput.Value);
				}

				if (zInput.Enabled && item.Z != zInput.Value) {
					actions.Add(UndoRedoManager.ItemZChanged(item, item.Z, (int) zInput.Value));
					item.Z = (int) zInput.Value;
					sort = true;
				}

				if (front.Checked != item.front) {
					(item.front ? item.Board.BoardItems.FrontBackgrounds : item.Board.BoardItems.BackBackgrounds)
						.Remove(item);
					(item.front ? item.Board.BoardItems.BackBackgrounds : item.Board.BoardItems.FrontBackgrounds)
						.Add(item);
					item.front = front.Checked;
					sort = true;
				}

				if (sort) item.Board.BoardItems.Sort();
				if (actions.Count > 0)
					item.Board.UndoRedoMan.AddUndoBatch(actions);

				item.type = (BackgroundType) typeBox.SelectedIndex;
				item.a = (int) alphaBox.Value;
				item.rx = (int) rxBox.Value;
				item.ry = (int) ryBox.Value;
				item.cx = (int) cxBox.Value;
				item.cy = (int) cyBox.Value;
				item.screenMode =
					(int) (RenderResolution) ((ComboBoxItem) comboBox_screenMode.SelectedItem)
					.Tag; // combo box selection. 800x600, 1024x768, 1280x720, 1920x1080

				// Spine
				if (!groupBox_spine.Enabled) {
					item.SpineRandomStart = false;
					item.SpineAni = null;
				}
				else {
					item.SpineRandomStart = checkBox_spineRandomStart.Checked;

					if (comboBox_spineAnimation.SelectedItem != null)
						item.SpineAni = ((comboBox_spineAnimation.SelectedItem as ComboBoxItem).Tag as Animation).Name;
					else
						item.SpineAni = null;
				}
			}

			Close();
		}

		/// <summary>
		/// TrackBar for parallaxY
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void trackBar_parallaxY_Scroll(object sender, EventArgs e) {
			var trackBar = sender as TrackBar;

			ryBox.Value = trackBar.Value;
		}

		/// <summary>
		/// TrackBar for parallax X
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void trackBar_parallaxX_Scroll(object sender, EventArgs e) {
			var trackBar = sender as TrackBar;

			rxBox.Value = trackBar.Value;
		}

		/// <summary>
		/// cx changed
		/// Disables the 'ok' button if the user selects a moving type background AND a negative cx or cy value.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cxBox_ValueChanged(object sender, EventArgs e) {
			var bDisableSaveButton = false;

			var bgType = (BackgroundType) typeBox.SelectedIndex;
			if (bgType != BackgroundType.Regular)
				if (cxBox.Value < 0)
					bDisableSaveButton = true;

			okButton.Enabled = !bDisableSaveButton;
		}

		/// <summary>
		/// cy changed
		/// Disables the 'ok' button if the user selects a moving type background AND a negative cx or cy value.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cyBox_ValueChanged(object sender, EventArgs e) {
			var bDisableSaveButton = false;

			var bgType = (BackgroundType) typeBox.SelectedIndex;
			if (bgType != BackgroundType.Regular)
				if (cyBox.Value < 0)
					bDisableSaveButton = true;

			okButton.Enabled = !bDisableSaveButton;
		}

		/// <summary>
		/// Background type changed
		/// Disables the 'ok' button if the user selects a moving type background AND a negative cx or cy value.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void typeBox_SelectedIndexChanged(object sender, EventArgs e) {
			var bDisableSaveButton = false;

			var bgType = (BackgroundType) typeBox.SelectedIndex;
			if (bgType != BackgroundType.Regular) {
				cxBox.Minimum = 0;
				cyBox.Minimum = 0;

				if (cyBox.Value < 0 || cxBox.Value < 0)
					bDisableSaveButton = true;
			}
			else {
				cxBox.Minimum = int.MaxValue * -1;
				cyBox.Minimum = int.MaxValue * -1;
			}

			okButton.Enabled = !bDisableSaveButton;
		}
	}
}