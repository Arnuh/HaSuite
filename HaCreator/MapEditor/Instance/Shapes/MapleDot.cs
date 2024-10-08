﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HaCreator.CustomControls;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Input;
using HaCreator.MapEditor.MonoGame;
using HaCreator.MapEditor.UndoRedo;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Shapes {
	public abstract class MapleDot : BoardItem, ISnappable {
		public MapleDot(Board board, int x, int y)
			: base(board, x, y, -1) {
		}

		public List<MapleLine> connectedLines = new();

		public abstract XNA.Color Color { get; }
		public abstract XNA.Color InactiveColor { get; }

		private static Point origin = new(UserSettings.DotWidth, UserSettings.DotWidth);

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
				if (RemoveConnectedLines) {
					foreach (var line in connectedLines) {
						line.OnPlaced(undoPipe);
					}
				}
			}
		}

		protected abstract bool RemoveConnectedLines { get; }

		public override void RemoveItem(List<UndoRedoAction> undoPipe) {
			lock (board.ParentControl) {
				base.RemoveItem(undoPipe);
				if (RemoveConnectedLines) {
					while (connectedLines.Count > 0) {
						connectedLines[0].Remove(false, undoPipe);
					}
				}
			}
		}

		public override Bitmap Image => null;

		public override int Width => UserSettings.DotWidth * 2;

		public override int Height => UserSettings.DotWidth * 2;

		public override XNA.Color GetColor(SelectionInfo sel, bool selected) {
			if ((sel.editedTypes & Type) == Type && CheckIfLayerSelected(sel)) {
				return selected ? UserSettings.SelectedColor : Color;
			}

			return InactiveColor;
		}

		public override Point Origin => origin;

		public override void Draw(Renderer graphics, XNA.Color color, int xShift, int yShift) {
			graphics.FillRectangle(
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
				if (PointMoved != null) {
					PointMoved.Invoke();
				}
			}
		}

		public override int Y {
			get => base.Y;
			set {
				base.Y = value;
				if (PointMoved != null) {
					PointMoved.Invoke();
				}
			}
		}

		public override void Move(int x, int y) {
			lock (board.ParentControl) {
				base.Move(x, y);
				if (PointMoved != null) {
					PointMoved.Invoke();
				}
			}
		}

		public override void SnapMove(int x, int y) {
			lock (board.ParentControl) {
				base.SnapMove(x, y);
				if (PointMoved != null) {
					PointMoved.Invoke();
				}
			}
		}

		public void MoveSilent(int x, int y) {
			base.Move(x, y);
		}

		public virtual void DoSnap() {
			if (!InputHandler.IsKeyPushedDown(Keys.ShiftKey)) {
				return;
			}

			if (connectedLines.Count == 0 || !(connectedLines[0] is FootholdLine)) {
				return;
			}

			// this is mouse when we have the 2nd dot held on the mouse but not placed yet.
			// Selected item check is after it has been placed.
			if (!(this is Mouse) && (board.SelectedItems.Count != 1 || !board.SelectedItems[0].Equals(this))) {
				return;
			}

			FootholdAnchor closestAnchor = null;
			var closestAngle = double.MaxValue;
			var xClosest = true;
			foreach (var line in connectedLines) {
				var otherAnchor = (FootholdAnchor) (line.FirstDot == this ? line.SecondDot : line.FirstDot);
				var xAngle = Math.Abs(Math.Atan((Y - otherAnchor.Y) / (double) (X - otherAnchor.X)));
				var yAngle = Math.Abs(Math.Atan((X - otherAnchor.X) / (double) (Y - otherAnchor.Y)));
				double minAngle;
				bool xSmaller;
				if (xAngle < yAngle) {
					xSmaller = true;
					minAngle = xAngle;
				} else {
					xSmaller = false;
					minAngle = yAngle;
				}

				if (minAngle < closestAngle) {
					xClosest = xSmaller;
					closestAnchor = otherAnchor;
					closestAngle = minAngle;
				}
			}

			if (closestAnchor == null) {
				return;
			}

			if (this is Mouse) {
				if (xClosest) {
					SnapMove(X, closestAnchor.Y);
				} else {
					SnapMove(closestAnchor.X, Y);
				}
			} else {
				if (xClosest) {
					SnapMoveAllMouseBoundItems(new XNA.Point(Parent.X + Parent.BoundItems[this].X, closestAnchor.Y));
				} else {
					SnapMoveAllMouseBoundItems(new XNA.Point(closestAnchor.X, Parent.Y + Parent.BoundItems[this].Y));
				}
			}
		}

		public MapleDot(Board board, SerializationForm json)
			: base(board, json) {
		}

		public delegate void OnPointMovedDelegate();

		public event OnPointMovedDelegate PointMoved;
	}
}