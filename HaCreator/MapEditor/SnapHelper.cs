using System;
using HaCreator.MapEditor.Instance.Shapes;
using Microsoft.Xna.Framework;

namespace HaCreator.MapEditor {
	public class SnapHelper {
		public static bool BetweenOrEquals(int value, int bounda, int boundb, int tolerance) {
			if (bounda < boundb) {
				return bounda - tolerance <= value && value <= boundb + tolerance;
			}

			return boundb - tolerance <= value && value <= bounda + tolerance;
		}

		public static void SnapToFootholdLine(BoardItem boardItem, int xOffset) {
			FootholdLine closestLine = null;
			var closestDistance = double.MaxValue;
			foreach (var fh in boardItem.Board.BoardItems.FootholdLines) {
				// Trying to snap to other selected items can mess up some of the mouse bindings
				if (fh.FirstDot.Selected || fh.SecondDot.Selected) {
					continue;
				}

				if (fh.IsWall || !BetweenOrEquals(boardItem.X, fh.FirstDot.X, fh.SecondDot.X, (int) UserSettings.SnapDistance) ||
				    !BetweenOrEquals(boardItem.Y, fh.FirstDot.Y, fh.SecondDot.Y, (int) UserSettings.SnapDistance)) {
					continue;
				}

				var targetY = fh.CalculateY(boardItem.X) - xOffset;
				var distance = Math.Abs(targetY - boardItem.Y);
				if (!(closestDistance > distance)) continue;
				closestDistance = distance;
				closestLine = fh;
			}

			if (closestLine != null) {
				boardItem.SnapMoveAllMouseBoundItems(new Point(boardItem.Parent.X + boardItem.Parent.BoundItems[boardItem].X,
					(int) closestLine.CalculateY(boardItem.X) - xOffset));
			}
		}
	}
}