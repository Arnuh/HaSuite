/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.UndoRedo;
using MapleLib.WzLib.WzStructure.Data;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Shapes {
	public class FootholdAnchor : MapleDot, IContainsLayerInfo, ISerializable {
		private int layer;
		private int zm;

		public bool user;

		public FootholdAnchor(Board board, int x, int y, int layer, int zm, bool user)
			: base(board, x, y) {
			this.layer = layer;
			this.zm = zm;
			this.user = user;
		}

		public override bool CheckIfLayerSelected(SelectionInfo sel) {
			return (sel.selectedLayer == -1 || sel.selectedLayer == layer) &&
			       (sel.selectedPlatform == -1 || sel.selectedPlatform == zm);
		}

		public override XNA.Color Color => UserSettings.FootholdColor;

		public override XNA.Color InactiveColor => MultiBoard.FootholdInactiveColor;

		public override ItemTypes Type => ItemTypes.Footholds;

		protected override bool RemoveConnectedLines => true;

		public static int FHAnchorSorter(FootholdAnchor c, FootholdAnchor d) {
			if (c.X > d.X) {
				return 1;
			}

			if (c.X < d.X) {
				return -1;
			}

			if (c.Y > d.Y) {
				return 1;
			}

			if (c.Y < d.Y) {
				return -1;
			}

			if (c.LayerNumber > d.LayerNumber) {
				return 1;
			}

			if (c.LayerNumber < d.LayerNumber) {
				return -1;
			}

			if (c.PlatformNumber > d.PlatformNumber) {
				return 1;
			}

			if (c.PlatformNumber < d.PlatformNumber) {
				return -1;
			}

			if (c.Parent != null && c.Parent is TileInstance &&
			    ((TileInfo) c.Parent.BaseInfo).u == "edU") {
				return -1;
			}

			if (d.Parent != null && d.Parent is TileInstance &&
			    ((TileInfo) d.Parent.BaseInfo).u == "edU") {
				return 1;
			}

			return 0;
		}

		public static void MergeAnchors(FootholdAnchor a, FootholdAnchor b) {
			foreach (FootholdLine line in b.connectedLines) {
				if (line.FirstDot == b) {
					line.FirstDot = a;
				} else if (line.SecondDot == b) {
					line.SecondDot = a;
				} else {
					throw new Exception("No anchor matches foothold");
				}

				a.connectedLines.Add(line);
			}

			b.connectedLines.Clear();
		}

		public bool AllConnectedLinesVertical() {
			foreach (var line in connectedLines) {
				if (line.FirstDot.X != line.SecondDot.X) {
					return false;
				}
			}

			return true;
		}

		public bool AllConnectedLinesHorizontal() {
			foreach (var line in connectedLines) {
				if (line.FirstDot.Y != line.SecondDot.Y) {
					return false;
				}
			}

			return true;
		}

		public int LayerNumber {
			get => layer;
			set => layer = value;
		}

		public int PlatformNumber {
			get => zm;
			set => zm = value;
		}

		public FootholdLine GetOtherLine(FootholdLine line) {
			foreach (FootholdLine currLine in connectedLines) {
				if (line != currLine) {
					return currLine;
				}
			}

			return null;
		}

		public FootholdLine GetLineWith(FootholdAnchor anchor) {
			foreach (FootholdLine line in connectedLines) {
				if (line.FirstDot == anchor || line.SecondDot == anchor) {
					return line;
				}
			}

			return null;
		}

		public new class SerializationForm : BoardItem.SerializationForm {
			public int layer, zm;
			public bool user;
		}

		public override bool ShouldSelectSerialized => true;

		public override List<ISerializableSelector> SelectSerialized(HashSet<ISerializableSelector> serializedItems) {
			var serList = new List<ISerializableSelector>();
			foreach (FootholdLine fh in connectedLines) {
				if (serializedItems.Contains(fh.GetOtherAnchor(this))) {
					serList.Add(fh);
				}
			}

			return serList;
		}

		public override object Serialize() {
			var result = new SerializationForm();
			UpdateSerializedForm(result);
			return result;
		}

		protected void UpdateSerializedForm(SerializationForm result) {
			base.UpdateSerializedForm(result);
			result.layer = layer;
			result.zm = zm;
			result.user = user;
		}

		public override IDictionary<string, object> SerializeBindings(Dictionary<ISerializable, long> refDict) {
			// No bindings
			return null;
		}

		public FootholdAnchor(Board board, SerializationForm json)
			: base(board, json) {
			layer = json.layer;
			zm = json.zm;
			user = json.user; // Will be reset to true on AddToBoard if we are copypasting
		}

		public override void DeserializeBindings(IDictionary<string, object> bindSer,
			Dictionary<long, ISerializable> refDict) {
			// No bindings
		}

		public override void AddToBoard(List<UndoRedoAction> undoPipe) {
			if (undoPipe != null) {
				user = true;
				layer = board.SelectedLayerIndex;
				zm = board.SelectedPlatform;
			}

			base.AddToBoard(undoPipe);
		}
	}
}