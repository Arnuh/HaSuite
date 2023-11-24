/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using MapleLib.WzLib.WzStructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Shapes {
	public class ToolTipDot : MapleDot {
		private MapleRectangle parentTooltip;

		public ToolTipDot(MapleRectangle parentTooltip, Board board, int x, int y)
			: base(board, x, y) {
			this.parentTooltip = parentTooltip;
		}

		public override XNA.Color Color => UserSettings.ToolTipColor;

		public override XNA.Color InactiveColor => MultiBoard.ToolTipInactiveColor;

		public override ItemTypes Type => ItemTypes.ToolTips;

		public MapleRectangle ParentTooltip {
			get => parentTooltip;
			set => parentTooltip = value;
		}

		protected override bool RemoveConnectedLines => false;

		public override bool ShouldSelectSerialized => true;

		public override List<ISerializableSelector> SelectSerialized(HashSet<ISerializableSelector> serializedItems) {
			return new List<ISerializableSelector> {parentTooltip};
		}
	}
}