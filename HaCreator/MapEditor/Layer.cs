/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.UndoRedo;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure;
using MapleLib.WzLib.WzStructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HaCreator.MapEditor {
	public class Layer {
		private List<LayeredItem> items = new List<LayeredItem>(); //needed?
		private readonly SortedSet<int> zms = new SortedSet<int>();
		private readonly int num;
		private readonly Board board;
		private string _tS = null;

		public Layer(Board board) {
			this.board = board;
			if (board.Layers.Count > MapConstants.MaxMapLayers)
				throw new NotSupportedException(
					"Cannot add more than 10 layers (why would you need that much anyway?)");

			num = board.Layers.Count;
		}

		public List<LayeredItem> Items {
			get => items;
			set => items = value;
		}

		public int LayerNumber => num;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public string tS {
			get => _tS;
			set {
				lock (board.ParentControl) {
					if (_tS != value) {
						_tS = value;
						if (!board.Loading) board.ParentControl.LayerTSChanged(this);
					}
				}
			}
		}

		public void ReplaceTS(string newTS) {
			lock (board.ParentControl) {
				foreach (var item in items)
					if (item is TileInstance) {
						var tile = (TileInstance) item;
						var tileBase = (TileInfo) tile.BaseInfo;
						var tileInfo = TileInfo.GetWithDefaultNo(newTS, tileBase.u, tileBase.no, "0");
						tile.SetBaseInfo(tileInfo);
					}
			}

			tS = newTS;
		}

		public static Layer GetLayerByNum(Board board, int num) {
			return board.Layers[num];
		}

		public void RecheckTileSet() {
			foreach (var item in items)
				if (item is TileInstance) {
					tS = ((TileInfo) item.BaseInfo).tS;
					return;
				}

			tS = null;
		}

		public void RecheckZM() {
			zMList.Clear();
			foreach (var li in items) zMList.Add(li.PlatformNumber);
		}

		/// <summary>
		/// zM
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public int zMDefault => board.SelectedPlatform == -1 ? zMList.ElementAt(0) : board.SelectedPlatform;


		/// <summary>
		/// zM List
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public SortedSet<int> zMList => zms;


		public override string ToString() {
			return LayerNumber.ToString() + (tS != null ? " - " + tS : "");
		}
	}
}