﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using HaCreator.MapEditor;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure.Data;
using System.Collections;
using HaCreator.GUI;
using MapleLib.WzLib.WzStructure;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.UndoRedo;
using HaCreator.CustomControls;

namespace HaCreator.GUI.EditorPanels {
	public partial class TilePanel : UserControl {
		private HaCreatorStateManager hcsm;

		public TilePanel() {
			InitializeComponent();
		}

		public void Initialize(HaCreatorStateManager hcsm) {
			this.hcsm = hcsm;
			hcsm.SetTilePanel(this);

			var sortedTileSets = new List<string>();
			foreach (var tS in Program.InfoManager.TileSets)
				sortedTileSets.Add(tS.Key);
			sortedTileSets.Sort();
			foreach (var tS in sortedTileSets)
				tileSetList.Items.Add(tS);
		}

		private void searchResultsBox_SelectedIndexChanged(object sender, EventArgs e) {
			SelectedIndexChanged.Invoke(sender, e);
		}

		public event EventHandler SelectedIndexChanged;

		private void tileBrowse_Click(object sender, EventArgs e) {
			lock (hcsm.MultiBoard) {
				new TileSetBrowser(tileSetList).ShowDialog();
			}
		}

		private void tileSetList_SelectedIndexChanged(object sender, EventArgs e) {
			LoadTileSetList();
		}

		public void LoadTileSetList() {
			lock (hcsm.MultiBoard) {
				if (tileSetList.SelectedItem == null)
					return;
				tileImagesContainer.Controls.Clear();
				var selectedSetName = (string) tileSetList.SelectedItem;
				if (!Program.InfoManager.TileSets.ContainsKey(selectedSetName))
					return;
				var tileSetImage = Program.InfoManager.TileSets[selectedSetName];
				var mag = InfoTool.GetOptionalInt(tileSetImage["info"]["mag"]);
				foreach (WzSubProperty tCat in tileSetImage.WzProperties) {
					if (tCat.Name == "info")
						continue;
					if (ApplicationSettings.randomTiles) {
						var canvasProp = (WzCanvasProperty) tCat["0"];
						if (canvasProp == null)
							continue;
						var item =
							tileImagesContainer.Add(canvasProp.GetLinkedWzCanvasBitmap(), tCat.Name, true);
						var randomInfos = new TileInfo[tCat.WzProperties.Count];
						for (var i = 0; i < randomInfos.Length; i++)
							randomInfos[i] = TileInfo.Get((string) tileSetList.SelectedItem, tCat.Name,
								tCat.WzProperties[i].Name, mag);

						item.Tag = randomInfos;
						item.MouseDown += new MouseEventHandler(tileItem_Click);
						item.MouseUp += new MouseEventHandler(ImageViewer.item_MouseUp);
					}
					else {
						foreach (WzCanvasProperty tile in tCat.WzProperties) {
							var item = tileImagesContainer.Add(tile.GetLinkedWzCanvasBitmap(),
								tCat.Name + "/" + tile.Name, true);
							item.Tag = TileInfo.Get((string) tileSetList.SelectedItem, tCat.Name, tile.Name, mag);
							item.MouseDown += new MouseEventHandler(tileItem_Click);
							item.MouseUp += new MouseEventHandler(ImageViewer.item_MouseUp);
						}
					}
				}
			}
		}

		private void tileItem_Click(object sender, MouseEventArgs e) {
			lock (hcsm.MultiBoard) {
				var item = (ImageViewer) sender;
				if (!hcsm.MultiBoard.AssertLayerSelected()) return;

				var layer = hcsm.MultiBoard.SelectedBoard.SelectedLayer;
				if (layer.tS != null) {
					TileInfo infoToAdd = null;
					if (ApplicationSettings.randomTiles)
						infoToAdd = ((TileInfo[]) item.Tag)[0];
					else
						infoToAdd = (TileInfo) item.Tag;
					if (infoToAdd.tS != layer.tS) {
						if (MessageBox.Show("This action will change the layer's tS. Proceed?", "Layer tS Change",
							    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) !=
						    DialogResult.Yes)
							return;
						var actions = new List<UndoRedoAction>();
						actions.Add(UndoRedoManager.LayerTSChanged(layer, layer.tS, infoToAdd.tS));
						layer.ReplaceTS(infoToAdd.tS);
						hcsm.MultiBoard.SelectedBoard.UndoRedoMan.AddUndoBatch(actions);
					}
				}

				hcsm.EnterEditMode(ItemTypes.Tiles);
				if (ApplicationSettings.randomTiles)
					hcsm.MultiBoard.SelectedBoard.Mouse.SetRandomTilesMode((TileInfo[]) item.Tag);
				else
					hcsm.MultiBoard.SelectedBoard.Mouse.SetHeldInfo((TileInfo) item.Tag);
				hcsm.MultiBoard.Focus();
				item.IsActive = true;
			}
		}
	}
}