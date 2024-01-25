/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Windows.Forms;
using HaCreator.CustomControls;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Info;
using MapleLib.WzLib.WzStructure.Data;

namespace HaCreator.GUI.EditorPanels {
	public partial class PortalPanel : UserControl {
		private HaCreatorStateManager hcsm;

		public PortalPanel() {
			InitializeComponent();
		}

		public void Initialize(HaCreatorStateManager hcsm) {
			this.hcsm = hcsm;

			foreach (var pt in Program.InfoManager.PortalTypeById) {
				var pInfo = PortalInfo.GetPortalInfoByType(Program.InfoManager.PortalIdByType[pt]);
				if (pInfo == null) continue;
				try {
					var item = portalImageContainer.Add(pInfo.Image, Tables.PortalTypeNames[pt], true);
					item.Tag = pInfo;
					item.MouseDown += portal_MouseDown;
					item.MouseUp += ImageViewer.item_MouseUp;
				} catch (KeyNotFoundException) {
				}
			}
		}

		private void portal_MouseDown(object sender, MouseEventArgs e) {
			lock (hcsm.MultiBoard) {
				hcsm.EnterEditMode(ItemTypes.Portals);
				hcsm.MultiBoard.SelectedBoard.Mouse.SetHeldInfo((PortalInfo) ((ImageViewer) sender).Tag);
				hcsm.MultiBoard.Focus();
				((ImageViewer) sender).IsActive = true;
			}
		}
	}
}