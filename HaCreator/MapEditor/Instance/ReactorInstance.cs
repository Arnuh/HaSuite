/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using HaCreator.MapEditor.Info;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance {
	public class ReactorInstance : BoardItem, IFlippable, ISerializable {
		private readonly ReactorInfo reactorInfo;

		public ReactorInfo ReactorInfo => reactorInfo;

		private int reactorTime;
		private bool flip;
		private string name;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="baseInfo"></param>
		/// <param name="board"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="reactorTime"></param>
		/// <param name="name"></param>
		/// <param name="flip"></param>
		public ReactorInstance(ReactorInfo baseInfo, Board board, int x, int y, int reactorTime, string name, bool flip)
			: base(board, x, y, -1) {
			reactorInfo = baseInfo;
			this.reactorTime = reactorTime;
			this.flip = flip;
			this.name = name;
			if (flip) {
				X -= Width - 2 * Origin.X;
			}
		}

		public override void Draw(SpriteBatch sprite, XNA.Color color, int xShift, int yShift) {
			var destinationRectangle =
				new XNA.Rectangle((int) X + xShift - Origin.X, (int) Y + yShift - Origin.Y, Width, Height);
			//if (baseInfo.Texture == null) baseInfo.CreateTexture(sprite.GraphicsDevice);
			sprite.Draw(reactorInfo.GetTexture(sprite), destinationRectangle, null, color, 0f, new XNA.Vector2(0f, 0f),
				Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1f);
			base.Draw(sprite, color, xShift, yShift);
		}

		public override MapleDrawableInfo BaseInfo => reactorInfo;

		public bool Flip {
			get => flip;
			set {
				if (flip == value) return;
				flip = value;
				var xFlipShift = Width - 2 * Origin.X;
				if (flip) {
					X -= xFlipShift;
				} else {
					X += xFlipShift;
				}
			}
		}

		public int UnflippedX => flip ? X + Width - 2 * Origin.X : X;

		public override ItemTypes Type => ItemTypes.Reactors;

		public override System.Drawing.Bitmap Image => reactorInfo.Image;

		public override int Width => reactorInfo.Width;

		public override int Height => reactorInfo.Height;

		public override System.Drawing.Point Origin => reactorInfo.Origin;

		public int ReactorTime {
			get => reactorTime;
			set => reactorTime = value;
		}

		public string Name {
			get => name;
			set => name = value;
		}

		public new class SerializationForm : BoardItem.SerializationForm {
			public string id;
			public int reactortime;
			public bool flip;
			public string name;
		}

		public override object Serialize() {
			var result = new SerializationForm();
			UpdateSerializedForm(result);
			return result;
		}

		protected void UpdateSerializedForm(SerializationForm result) {
			base.UpdateSerializedForm(result);
			result.id = reactorInfo.ID;
			result.reactortime = reactorTime;
			result.flip = flip;
			result.name = name;
		}

		public ReactorInstance(Board board, SerializationForm json)
			: base(board, json) {
			reactorInfo = ReactorInfo.Get(json.id);
		}
	}
}