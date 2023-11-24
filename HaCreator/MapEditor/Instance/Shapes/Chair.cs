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
	public class Chair : MapleDot, ISnappable, ISerializable {
		public Chair(Board board, int x, int y)
			: base(board, x, y) {
		}

		public override bool CheckIfLayerSelected(SelectionInfo sel) {
			return true;
		}

		public override void DoSnap() {
			FootholdLine closestLine = null;
			var closestDistance = double.MaxValue;
			foreach (var fh in Board.BoardItems.FootholdLines) {
				// Trying to snap to other selected items can mess up some of the mouse bindings
				if (fh.FirstDot.Selected || fh.SecondDot.Selected)
					continue;
				if (!fh.IsWall && BetweenOrEquals(X, fh.FirstDot.X, fh.SecondDot.X, (int) UserSettings.SnapDistance) &&
				    BetweenOrEquals(Y, fh.FirstDot.Y, fh.SecondDot.Y, (int) UserSettings.SnapDistance)) {
					var targetY = fh.CalculateY(X) - 1;
					var distance = Math.Abs(targetY - Y);
					if (closestDistance > distance) {
						closestDistance = distance;
						closestLine = fh;
					}
				}
			}

			if (closestLine != null)
				SnapMoveAllMouseBoundItems(new XNA.Point(Parent.X + Parent.BoundItems[this].X,
					(int) closestLine.CalculateY(X) - 1));
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