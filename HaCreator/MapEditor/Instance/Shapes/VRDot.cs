﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using MapleLib.WzLib.WzStructure.Data;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Shapes {
	public class VRDot : MapleDot {
		private MapleEmptyRectangle rect;

		public VRDot(MapleEmptyRectangle rect, Board board, int x, int y)
			: base(board, x, y) {
			this.rect = rect;
		}

		public override XNA.Color Color => UserSettings.VRColor;

		public override XNA.Color InactiveColor => MultiBoard.VRInactiveColor;

		public override ItemTypes Type => ItemTypes.Misc;

		protected override bool RemoveConnectedLines => false;
	}
}