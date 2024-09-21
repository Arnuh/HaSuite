/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Drawing;
using HaCreator.CustomControls;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.MonoGame;
using HaCreator.MapEditor.UndoRedo;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Shapes {
	public abstract class MapleRectangle : BoardItem {
		//clockwise, beginning in upper-left
		private MapleDot a;
		private MapleDot b;
		private MapleDot c;
		private MapleDot d;

		private MapleLine ab;
		private MapleLine bc;
		private MapleLine cd;
		private MapleLine da;

		protected XNA.Rectangle rect;

		public XNA.Rectangle Rectangle {
			get => rect;
			private set { }
		}

		public MapleRectangle(Board board, XNA.Rectangle rect)
			: base(board, 0, 0, 0) // BoardItem position doesn't do anything in rectangles
		{
			this.rect = rect;

			lock (board.ParentControl) {
				// Make dots
				a = CreateDot(rect.Left, rect.Top);
				b = CreateDot(rect.Right, rect.Top);
				c = CreateDot(rect.Right, rect.Bottom);
				d = CreateDot(rect.Left, rect.Bottom);
				PlaceDots();

				// Make lines
				ab = CreateLine(a, b);
				bc = CreateLine(b, c);
				cd = CreateLine(c, d);
				da = CreateLine(d, a);
				ab.yBind = true;
				bc.xBind = true;
				cd.yBind = true;
				da.xBind = true;
			}
		}

		protected void PlaceDots() {
			board.BoardItems.Add(a, false);
			board.BoardItems.Add(b, false);
			board.BoardItems.Add(c, false);
			board.BoardItems.Add(d, false);
		}

		public abstract XNA.Color Color { get; }

		public abstract MapleDot CreateDot(int x, int y);
		public abstract MapleLine CreateLine(MapleDot a, MapleDot b);

		public MapleDot PointA {
			get => a;
			set => a = value;
		}

		public MapleDot PointB {
			get => b;
			set => b = value;
		}

		public MapleDot PointC {
			get => c;
			set => c = value;
		}

		public MapleDot PointD {
			get => d;
			set => d = value;
		}

		public MapleLine LineAB {
			get => ab;
			set => ab = value;
		}

		public MapleLine LineBC {
			get => bc;
			set => bc = value;
		}

		public MapleLine LineCD {
			get => cd;
			set => cd = value;
		}

		public MapleLine LineDA {
			get => da;
			set => da = value;
		}

		public override bool CheckIfLayerSelected(SelectionInfo sel) {
			return true;
		}

		public override void Draw(Renderer graphics, XNA.Color dotColor, int xShift, int yShift) {
			var lineColor = ab.Color;
			if (Selected) {
				lineColor = dotColor;
			}

			int x, y;
			if (a.X < b.X) {
				x = a.X + xShift;
			} else {
				x = b.X + xShift;
			}

			if (b.Y < c.Y) {
				y = b.Y + yShift;
			} else {
				y = c.Y + yShift;
			}

			graphics.FillRectangle(new XNA.Rectangle(x, y, Width, Height), Color);
			ab.Draw(graphics, lineColor, xShift, yShift);
			bc.Draw(graphics, lineColor, xShift, yShift);
			cd.Draw(graphics, lineColor, xShift, yShift);
			da.Draw(graphics, lineColor, xShift, yShift);
		}

		public override MapleDrawableInfo BaseInfo => null;

		public override Bitmap Image => throw new NotImplementedException();

		public override Point Origin => Point.Empty;

		public override bool IsPixelTransparent(int x, int y) {
			return false;
		}

		public override int Width => a.X < b.X ? b.X - a.X : a.X - b.X;

		public override int Height => b.Y < c.Y ? c.Y - b.Y : b.Y - c.Y;

		public override int X {
			get => Math.Min(a.X, b.X);
			set {
				// Not doing anything since move is handled in the dots
			}
		}

		public override int Y {
			get => Math.Min(b.Y, c.Y);
			set {
				// Not doing anything since move is handled in the dots
			}
		}

		public override void Move(int x, int y) {
			// Not doing anything since move is handled in the dots
		}

		public override void SnapMove(int x, int y) {
			// Not doing anything since move is handled in the dots
		}

		public override int Left => Math.Min(a.X, b.X);

		public override int Top => Math.Min(b.Y, c.Y);

		public override int Bottom => Math.Max(b.Y, c.Y);

		public override int Right => Math.Max(a.X, b.X);

		public override bool Selected {
			get => base.Selected;
			set {
				base.Selected = value;
				a.Selected = value;
				b.Selected = value;
				c.Selected = value;
				d.Selected = value;
			}
		}

		public override void RemoveItem(List<UndoRedoAction> undoPipe) {
			lock (board.ParentControl) {
				base.RemoveItem(undoPipe);
				PointA.RemoveItem(undoPipe);
				PointB.RemoveItem(undoPipe);
				PointC.RemoveItem(undoPipe);
				PointD.RemoveItem(undoPipe);
			}
		}

		public override void OnItemPlaced(List<UndoRedoAction> undoPipe) {
			lock (board.ParentControl) {
				base.OnItemPlaced(undoPipe);
				PointA.OnItemPlaced(undoPipe);
				PointB.OnItemPlaced(undoPipe);
				PointC.OnItemPlaced(undoPipe);
				PointD.OnItemPlaced(undoPipe);
			}
		}

		#region ISerializable Implementation

		public new class SerializationForm {
			public int x1, y1, x2, y2;
		}

		public override object Serialize() {
			var result = new SerializationForm();
			UpdateSerializedForm(result);
			return result;
		}

		protected void UpdateSerializedForm(SerializationForm result) {
			result.x1 = Left;
			result.x2 = Right;
			result.y1 = Top;
			result.y2 = Bottom;
		}

		public MapleRectangle(Board board, SerializationForm json)
			: base(board, 0, 0, 0) {
			// Make dots
			a = CreateDot(json.x1, json.y1);
			b = CreateDot(json.x2, json.y1);
			c = CreateDot(json.x2, json.y2);
			d = CreateDot(json.x1, json.y2);

			// Make lines
			ab = CreateLine(a, b);
			bc = CreateLine(b, c);
			cd = CreateLine(c, d);
			da = CreateLine(d, a);
			ab.yBind = true;
			bc.xBind = true;
			cd.yBind = true;
			da.xBind = true;
		}

		public override void AddToBoard(List<UndoRedoAction> undoPipe) {
			PlaceDots();
			board.BoardItems.Add(this, false);
			OnItemPlaced(undoPipe);
		}

		public override void PostDeserializationActions(bool? selected, XNA.Point? offset) {
			if (selected.HasValue) {
				Selected = selected.Value;
			}

			if (offset.HasValue) {
				foreach (var dot in new[] {a, b, c, d}) {
					dot.MoveSilent(dot.X + offset.Value.X, dot.Y + offset.Value.Y);
				}
			}
		}

		#endregion
	}
}