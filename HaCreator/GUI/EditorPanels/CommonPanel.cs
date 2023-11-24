﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using HaCreator.CustomControls;
using HaCreator.MapEditor;
using HaCreator.Wz;
using HaSharedLibrary.Wz;
using MapleLib.WzLib.WzStructure.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace HaCreator.GUI.EditorPanels {
	public partial class CommonPanel : UserControl {
		private HaCreatorStateManager hcsm;

		public CommonPanel() {
			InitializeComponent();
		}

		public void Initialize(HaCreatorStateManager hcsm) {
			this.hcsm = hcsm;

			var commonItems = new ImageViewer[] {
				miscItemsContainer.Add(CreateColoredBitmap(WzInfoTools.XNAToDrawingColor(UserSettings.FootholdColor)),
					"Foothold", true),
				miscItemsContainer.Add(CreateColoredBitmap(WzInfoTools.XNAToDrawingColor(UserSettings.RopeColor)),
					"Rope", true),
				miscItemsContainer.Add(CreateColoredBitmap(WzInfoTools.XNAToDrawingColor(UserSettings.ChairColor)),
					"Chair", true),
				miscItemsContainer.Add(CreateColoredBitmap(WzInfoTools.XNAToDrawingColor(UserSettings.ToolTipColor)),
					"Tooltip", true),
				miscItemsContainer.Add(CreateColoredBitmap(WzInfoTools.XNAToDrawingColor(UserSettings.MiscColor)),
					"Clock", true)
			};
			foreach (var item in commonItems) {
				item.MouseDown += new MouseEventHandler(commonItem_Click);
				item.MouseUp += new MouseEventHandler(ImageViewer.item_MouseUp);
			}
		}

		private Bitmap CreateColoredBitmap(Color color) {
			var containerSize = UserSettings.dotDescriptionBoxSize;
			var DotWidth = Math.Min(UserSettings.DotWidth, containerSize);
			var result = new Bitmap(containerSize, containerSize);
			using (var g = Graphics.FromImage(result)) {
				g.FillRectangle(new SolidBrush(color),
					new Rectangle(containerSize / 2 - DotWidth / 2, containerSize / 2 - DotWidth / 2, DotWidth,
						DotWidth));
			}

			return result;
		}

		private void commonItem_Click(object sender, MouseEventArgs e) {
			lock (hcsm.MultiBoard) {
				var item = (ImageViewer) sender;
				switch (item.Name) {
					case "Foothold":
						if (!hcsm.MultiBoard.AssertLayerSelected()) return;

						hcsm.EnterEditMode(ItemTypes.Footholds);
						hcsm.MultiBoard.SelectedBoard.Mouse.SetFootholdMode();
						hcsm.MultiBoard.Focus();
						break;
					case "Rope":
						if (!hcsm.MultiBoard.AssertLayerSelected()) return;

						hcsm.EnterEditMode(ItemTypes.Ropes);
						hcsm.MultiBoard.SelectedBoard.Mouse.SetRopeMode();
						hcsm.MultiBoard.Focus();
						break;
					case "Chair":
						hcsm.EnterEditMode(ItemTypes.Chairs);
						hcsm.MultiBoard.SelectedBoard.Mouse.SetChairMode();
						hcsm.MultiBoard.Focus();
						break;
					case "Tooltip":
						hcsm.EnterEditMode(ItemTypes.Footholds);
						hcsm.MultiBoard.SelectedBoard.Mouse.SetTooltipMode();
						hcsm.MultiBoard.Focus();
						break;
					case "Clock":
						hcsm.EnterEditMode(ItemTypes.Misc);
						hcsm.MultiBoard.SelectedBoard.Mouse.SetClockMode();
						hcsm.MultiBoard.Focus();
						break;
				}

				item.IsActive = true;
			}
		}
	}
}