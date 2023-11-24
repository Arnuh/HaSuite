﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using HaCreator.CustomControls;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Info;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure.Data;
using System;
using System.Collections;
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
	public partial class ObjPanel : UserControl {
		private HaCreatorStateManager hcsm;

		public ObjPanel() {
			InitializeComponent();
		}

		public void Initialize(HaCreatorStateManager hcsm) {
			this.hcsm = hcsm;
			hcsm.SetObjPanel(this);

			var sortedObjSets = new List<string>();
			foreach (var oS in Program.InfoManager.ObjectSets)
				sortedObjSets.Add(oS.Key);
			sortedObjSets.Sort();
			foreach (var oS in sortedObjSets)
				objSetListBox.Items.Add(oS);
		}

		private void objSetListBox_SelectedIndexChanged(object sender, EventArgs e) {
			if (objSetListBox.SelectedItem == null)
				return;

			objL0ListBox.Items.Clear();
			objL1ListBox.Items.Clear();
			objImagesContainer.Controls.Clear();
			var oSImage = Program.InfoManager.ObjectSets[(string) objSetListBox.SelectedItem];
			if (!oSImage.Parsed) oSImage.ParseImage();

			foreach (var l0Prop in oSImage.WzProperties) objL0ListBox.Items.Add(l0Prop.Name);

			// select the first item automatically
			if (objL0ListBox.Items.Count > 0) objL0ListBox.SelectedIndex = 0;
		}

		private void objL0ListBox_SelectedIndexChanged(object sender, EventArgs e) {
			if (objL0ListBox.SelectedItem == null)
				return;

			objL1ListBox.Items.Clear();
			objImagesContainer.Controls.Clear();
			var l0Prop =
				Program.InfoManager.ObjectSets[(string) objSetListBox.SelectedItem][(string) objL0ListBox.SelectedItem];
			foreach (var l1Prop in l0Prop.WzProperties) objL1ListBox.Items.Add(l1Prop.Name);

			// select the first item automatically
			if (objL1ListBox.Items.Count > 0) objL1ListBox.SelectedIndex = 0;
		}

		private void objL1ListBox_SelectedIndexChanged(object sender, EventArgs e) {
			lock (hcsm.MultiBoard) {
				if (objL1ListBox.SelectedItem == null) return;
				objImagesContainer.Controls.Clear();
				var l1Prop =
					Program.InfoManager.ObjectSets[(string) objSetListBox.SelectedItem][
						(string) objL0ListBox.SelectedItem][(string) objL1ListBox.SelectedItem];
				try {
					foreach (WzSubProperty l2Prop in l1Prop.WzProperties) {
						var info = ObjectInfo.Get((string) objSetListBox.SelectedItem,
							(string) objL0ListBox.SelectedItem, (string) objL1ListBox.SelectedItem, l2Prop.Name);
						var item = objImagesContainer.Add(info.Image, l2Prop.Name, true);
						item.Tag = info;
						item.MouseDown += new MouseEventHandler(objItem_Click);
						item.MouseUp += new MouseEventHandler(ImageViewer.item_MouseUp);
						item.MaxHeight = UserSettings.ImageViewerHeight;
						item.MaxWidth = UserSettings.ImageViewerWidth;
					}
				}
				catch (InvalidCastException) {
					return;
				}
			}
		}

		public void OnL1Changed(string l1) {
			if ((string) objL1ListBox.SelectedItem == l1)
				objL1ListBox_SelectedIndexChanged(null, null);
		}

		private void objItem_Click(object sender, MouseEventArgs e) {
			lock (hcsm.MultiBoard) {
				if (!hcsm.MultiBoard.AssertLayerSelected()) return;

				hcsm.EnterEditMode(ItemTypes.Objects);
				hcsm.MultiBoard.SelectedBoard.Mouse.SetHeldInfo((ObjectInfo) ((ImageViewer) sender).Tag);
				hcsm.MultiBoard.Focus();
				((ImageViewer) sender).IsActive = true;
			}
		}
	}
}