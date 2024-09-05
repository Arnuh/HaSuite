/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Windows.Forms;
using HaCreator.CustomControls;
using MapleLib.WzLib.WzProperties;

namespace HaCreator.GUI {
	public partial class TileSetBrowser : Form {
		private ListBox targetListBox;
		public ImageViewer selectedItem;

		public TileSetBrowser(ListBox target) {
			InitializeComponent();
			targetListBox = target;
			var sortedTileSets = new List<string>();
			foreach (var tS in Program.InfoManager.TileSets) {
				sortedTileSets.Add(tS.Key);
			}

			sortedTileSets.Sort();
			foreach (var tS in sortedTileSets) {
				var tSImage = Program.InfoManager.TileSets[tS];
				if (!tSImage.Parsed) {
					tSImage.ParseImage();
				}

				var enh0 = tSImage["enH0"];
				if (enh0 == null) {
					continue;
				}

				var image = (WzCanvasProperty) enh0["0"];
				if (image == null) {
					continue;
				}

				var item = koolkLVContainer.Add(image.GetLinkedWzCanvasBitmap(), tS, true);
				item.MouseDown += item_Click;
				item.MouseDoubleClick += item_DoubleClick;
			}
		}

		private void item_DoubleClick(object sender, MouseEventArgs e) {
			if (selectedItem == null) {
				return;
			}

			targetListBox.SelectedItem = selectedItem.Name;
			Close();
		}

		private void item_Click(object sender, MouseEventArgs e) {
			if (selectedItem != null) {
				selectedItem.IsActive = false;
			}

			selectedItem = (ImageViewer) sender;
			selectedItem.IsActive = true;
		}

		private void TileSetBrowser_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Escape) {
				e.Handled = true;
				Close();
			}
		}
	}
}