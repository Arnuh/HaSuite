/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Instance;
using MapleLib.WzLib.WzStructure.Data;

namespace HaCreator.MapEditor {
	public class Layer {
		private List<LayeredItem> items = new(); //needed?
		private readonly SortedSet<int> zms = new();
		private readonly int num;
		private readonly Board board;
		private string _tS;

		public Layer(Board board, int index) {
			this.board = board;
			num = index;
		}

		public List<LayeredItem> Items {
			get => items;
			set => items = value;
		}

		public int LayerNumber => num;

		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public string tS {
			get => _tS;
			set {
				lock (board.ParentControl) {
					if (_tS != value) {
						_tS = value;
						if (!board.Loading) {
							board.ParentControl.LayerTSChanged(this);
						}
					}
				}
			}
		}

		public void ReplaceTS(string newTS) {
			lock (board.ParentControl) {
				foreach (var item in items) {
					if (item is TileInstance) {
						var tile = (TileInstance) item;
						var tileBase = (TileInfo) tile.BaseInfo;
						var tileInfo = TileInfo.GetWithDefaultNo(newTS, tileBase.u, tileBase.no, "0");
						tile.SetBaseInfo(tileInfo);
					}
				}
			}

			tS = newTS;
		}

		public void RecheckTileSet() {
			foreach (var item in items) {
				if (item is TileInstance) {
					tS = ((TileInfo) item.BaseInfo).tS;
					return;
				}
			}

			tS = null;
		}

		public void RecheckZM() {
			zMList.Clear();
			foreach (var li in items) {
				zMList.Add(li.PlatformNumber);
			}
		}

		/// <summary>
		/// zM
		/// </summary>
		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public int zMDefault => board.SelectedPlatform == -1 ? zMList.ElementAt(0) : board.SelectedPlatform;


		/// <summary>
		/// zM List
		/// </summary>
		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public SortedSet<int> zMList => zms;


		public override string ToString() {
			return LayerNumber + (tS != null ? " - " + tS : "");
		}
	}
}