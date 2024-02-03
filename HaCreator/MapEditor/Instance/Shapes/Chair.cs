/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using MapleLib.WzLib.WzStructure.Data;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Shapes {
	public class Chair : MapleDot, ISerializable {
		public Chair(Board board, int x, int y)
			: base(board, x, y) {
		}

		public override bool CheckIfLayerSelected(SelectionInfo sel) {
			return true;
		}

		public override void DoSnap() {
			SnapHelper.SnapToFootholdLine(this, 1);
		}

		public override XNA.Color Color => UserSettings.ChairColor;

		public override XNA.Color InactiveColor => MultiBoard.ChairInactiveColor;

		public override ItemTypes Type => ItemTypes.Chairs;

		protected override bool RemoveConnectedLines => true;

		public Chair(Board board, SerializationForm json)
			: base(board, json) {
		}
	}
}