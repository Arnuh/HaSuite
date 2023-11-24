/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using HaCreator.MapEditor.UndoRedo;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Shapes {
	public abstract class MapleEmptyRectangle {
		//clockwise, beginning in upper-left
		private MapleDot a;
		private MapleDot b;
		private MapleDot c;
		private MapleDot d;

		private MapleLine ab;
		private MapleLine bc;
		private MapleLine cd;
		private MapleLine da;

		protected Board board;

		public MapleEmptyRectangle(Board board, XNA.Rectangle rect) {
			this.board = board;

			lock (board.ParentControl) {
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

		public int Width => a.X < b.X ? b.X - a.X : a.X - b.X;

		public int Height => b.Y < c.Y ? c.Y - b.Y : b.Y - c.Y;

		public int X => Math.Min(a.X, b.X);

		public int Y => Math.Min(b.Y, c.Y);

		public int Left => Math.Min(a.X, b.X);

		public int Top => Math.Min(b.Y, c.Y);

		public int Bottom => Math.Max(b.Y, c.Y);

		public int Right => Math.Max(a.X, b.X);

		public virtual void RemoveItem(List<UndoRedoAction> undoPipe) {
			lock (board.ParentControl) {
				PointA.RemoveItem(undoPipe);
				PointB.RemoveItem(undoPipe);
				PointC.RemoveItem(undoPipe);
				PointD.RemoveItem(undoPipe);
			}
		}

		public virtual void Draw(SpriteBatch sprite, int xShift, int yShift, SelectionInfo sel) {
			var lineColor = ab.GetColor(sel);
			int x, y;
			if (a.X < b.X) x = a.X + xShift;
			else x = b.X + xShift;
			if (b.Y < c.Y) y = b.Y + yShift;
			else y = c.Y + yShift;
			ab.Draw(sprite, lineColor, xShift, yShift);
			bc.Draw(sprite, lineColor, xShift, yShift);
			cd.Draw(sprite, lineColor, xShift, yShift);
			da.Draw(sprite, lineColor, xShift, yShift);
		}

		public ItemTypes Type => ItemTypes.Misc;

		public class SerializationForm {
			public int x0, x1, y0, y1;
		}

		public object Serialize() {
			var result = new SerializationForm();
			result.x0 = Left;
			result.y0 = Top;
			result.x1 = Right;
			result.y1 = Bottom;
			return result;
		}
	}
}