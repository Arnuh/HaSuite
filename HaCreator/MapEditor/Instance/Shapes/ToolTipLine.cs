/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using HaCreator.MapEditor.UndoRedo;
using MapleLib.WzLib.WzStructure.Data;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Shapes {
	public class ToolTipLine : MapleLine {
		public ToolTipLine(Board board, MapleDot firstDot, MapleDot secondDot)
			: base(board, firstDot, secondDot) {
		}

		public override XNA.Color Color => UserSettings.ToolTipColor;

		public override XNA.Color InactiveColor => MultiBoard.ToolTipInactiveColor;

		public override ItemTypes Type => ItemTypes.ToolTips;

		public override void Remove(bool removeDots, List<UndoRedoAction> undoPipe) {
		}
	}
}