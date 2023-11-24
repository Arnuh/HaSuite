/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Input;
using HaCreator.MapEditor.UndoRedo;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Shapes {
	public abstract class MapleDot : BoardItem, ISnappable {
		public MapleDot(Board board, int x, int y)
			: base(board, x, y, -1) {
		}

		public List<MapleLine> connectedLines = new List<MapleLine>();

		public abstract XNA.Color Color { get; }
		public abstract XNA.Color InactiveColor { get; }

		private static Point origin = new Point(UserSettings.DotWidth, UserSettings.DotWidth);

		public static void OnDotWidthChanged() {
			origin = new Point(UserSettings.DotWidth, UserSettings.DotWidth);
		}

		public override bool IsPixelTransparent(int x, int y) {
			return false;
		}

		public override MapleDrawableInfo BaseInfo => null;

		public override void OnItemPlaced(List<UndoRedoAction> undoPipe) {
			lock (board.ParentControl) {
				base.OnItemPlaced(undoPipe);
				if (RemoveConnectedLines)
					foreach (var line in connectedLines)
						line.OnPlaced(undoPipe);
			}
		}

		protected abstract bool RemoveConnectedLines { get; }

		public override void RemoveItem(List<UndoRedoAction> undoPipe) {
			lock (board.ParentControl) {
				base.RemoveItem(undoPipe);
				if (RemoveConnectedLines)
					while (connectedLines.Count > 0)
						connectedLines[0].Remove(false, undoPipe);
			}
		}

		public override Bitmap Image => null;

		public override int Width => UserSettings.DotWidth * 2;

		public override int Height => UserSettings.DotWidth * 2;

		public override XNA.Color GetColor(SelectionInfo sel, bool selected) {
			if ((sel.editedTypes & Type) == Type && CheckIfLayerSelected(sel))
				return selected ? UserSettings.SelectedColor : Color;
			else return InactiveColor;
		}

		public override Point Origin => origin;

		public override void Draw(SpriteBatch sprite, XNA.Color color, int xShift, int yShift) {
			Board.ParentControl.FillRectangle(sprite,
				new XNA.Rectangle(X - UserSettings.DotWidth + xShift, Y - UserSettings.DotWidth + yShift,
					UserSettings.DotWidth * 2, UserSettings.DotWidth * 2), color);
		}

		public void DisconnectLine(MapleLine line) {
			connectedLines.Remove(line);
		}

		public bool IsMoveHandled => PointMoved != null;

		public override int X {
			get => base.X;
			set {
				base.X = value;
				if (PointMoved != null) PointMoved.Invoke();
			}
		}

		public override int Y {
			get => base.Y;
			set {
				base.Y = value;
				if (PointMoved != null) PointMoved.Invoke();
			}
		}

		public override void Move(int x, int y) {
			lock (board.ParentControl) {
				base.Move(x, y);
				if (PointMoved != null)
					PointMoved.Invoke();
			}
		}

		public override void SnapMove(int x, int y) {
			lock (board.ParentControl) {
				base.SnapMove(x, y);
				if (PointMoved != null)
					PointMoved.Invoke();
			}
		}

		public void MoveSilent(int x, int y) {
			base.Move(x, y);
		}

		public virtual void DoSnap() {
			if (InputHandler.IsKeyPushedDown(System.Windows.Forms.Keys.ShiftKey) && connectedLines.Count != 0 &&
			    connectedLines[0] is FootholdLine && board.SelectedItems.Count == 1 &&
			    board.SelectedItems[0].Equals(this)) {
				FootholdAnchor closestAnchor = null;
				var closestAngle = double.MaxValue;
				var xClosest = true;
				foreach (FootholdLine line in connectedLines) {
					var otherAnchor =
						(FootholdAnchor) (line.FirstDot == this ? line.SecondDot : line.FirstDot);
					var xAngle = Math.Abs(Math.Atan((double) (Y - otherAnchor.Y) / (double) (X - otherAnchor.X)));
					var yAngle = Math.Abs(Math.Atan((double) (X - otherAnchor.X) / (double) (Y - otherAnchor.Y)));
					double minAngle;
					var xSmaller = false;
					if (xAngle < yAngle) {
						xSmaller = true;
						minAngle = xAngle;
					}
					else {
						xSmaller = false;
						minAngle = yAngle;
					}

					if (minAngle < closestAngle) {
						xClosest = xSmaller;
						closestAnchor = otherAnchor;
						closestAngle = minAngle;
					}
				}

				if (closestAnchor != null) {
					if (xClosest)
						SnapMoveAllMouseBoundItems(new XNA.Point(Parent.X + Parent.BoundItems[this].X,
							closestAnchor.Y));
					else
						SnapMoveAllMouseBoundItems(new XNA.Point(closestAnchor.X,
							Parent.Y + Parent.BoundItems[this].Y));
				}
			}
		}

		public bool BetweenOrEquals(int value, int bounda, int boundb, int tolerance) {
			if (bounda < boundb)
				return bounda - tolerance <= value && value <= boundb + tolerance;
			else
				return boundb - tolerance <= value && value <= bounda + tolerance;
		}

		public MapleDot(Board board, SerializationForm json)
			: base(board, json) {
		}

		public delegate void OnPointMovedDelegate();

		public event OnPointMovedDelegate PointMoved;
	}
}