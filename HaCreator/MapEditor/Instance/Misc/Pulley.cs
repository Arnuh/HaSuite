/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Drawing;
using HaCreator.CustomControls;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.MonoGame;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Misc {
	public class Pulley : BoardItem, INamedMisc, ISerializable {
		private ObjectInfo baseInfo;

		public Pulley(ObjectInfo baseInfo, Board board, int x, int y)
			: base(board, x, y, -1) {
			this.baseInfo = baseInfo;
		}

		public override ItemTypes Type => ItemTypes.Misc;

		public override MapleDrawableInfo BaseInfo => baseInfo;

		public override XNA.Color GetColor(SelectionInfo sel, bool selected) {
			var c = base.GetColor(sel, selected);
			return c;
		}

		public override void Draw(Renderer graphics, XNA.Color color, int xShift, int yShift) {
			var destinationRectangle =
				new XNA.Rectangle(X + xShift - Origin.X, Y + yShift - Origin.Y, Width, Height);
			graphics.Draw(baseInfo.GetTexture(graphics), destinationRectangle, null, color, 0f, new XNA.Vector2(0, 0),
				SpriteEffects.None, 0);
		}

		public override Bitmap Image => baseInfo.Image;

		public override int Width => baseInfo.Width;

		public override int Height => baseInfo.Height;

		public override Point Origin => baseInfo.Origin;

		public string Name => "Special: Pulley";

		public new class SerializationForm : BoardItem.SerializationForm {
			public string os, l0, l1, l2;
		}

		public override object Serialize() {
			var result = new SerializationForm();
			UpdateSerializedForm(result);
			return result;
		}

		protected void UpdateSerializedForm(SerializationForm result) {
			base.UpdateSerializedForm(result);
			result.os = baseInfo.oS;
			result.l0 = baseInfo.l0;
			result.l1 = baseInfo.l1;
			result.l2 = baseInfo.l2;
		}

		public Pulley(Board board, SerializationForm json)
			: base(board, json) {
			baseInfo = ObjectInfo.Get(json.os, json.l0, json.l1, json.l2);
		}
	}
}