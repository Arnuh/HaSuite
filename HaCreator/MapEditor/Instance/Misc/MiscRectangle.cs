﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Drawing;
using HaCreator.CustomControls;
using HaCreator.MapEditor.Instance.Shapes;
using HaCreator.MapEditor.MonoGame;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Misc {
	public abstract class MiscRectangle : MapleRectangle, INamedMisc {
		public abstract string Name { get; }

		public MiscRectangle(Board board, XNA.Rectangle rect)
			: base(board, rect) {
		}

		public override MapleDot CreateDot(int x, int y) {
			return new MiscDot(this, board, x, y);
		}

		public override MapleLine CreateLine(MapleDot a, MapleDot b) {
			return new MiscLine(board, a, b);
		}

		public override XNA.Color Color => Selected ? UserSettings.ToolTipSelectedFill : UserSettings.ToolTipFill;

		public override ItemTypes Type => ItemTypes.Misc;

		public override void Draw(Renderer graphics, XNA.Color dotColor, int xShift, int yShift) {
			base.Draw(graphics, dotColor, xShift, yShift);
			graphics.DrawString(new Point(X + xShift + 2, Y + yShift + 2),
				XNA.Color.Black, Name, Width);
		}

		public MiscRectangle(Board board, SerializationForm json)
			: base(board, json) {
		}
	}
}