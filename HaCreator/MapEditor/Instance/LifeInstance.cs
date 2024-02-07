/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Drawing;
using HaCreator.MapEditor.Info;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance {
	public abstract class LifeInstance : BoardItem, IFlippable, ISerializable, ISnappable {
		private int _rx0Shift;
		private int _rx1Shift;
		private int _yShift;
		private int mobTime;
		private string limitedname;
		private bool flip;
		private bool hide;
		private int info; //no idea
		private int team;

		public LifeInstance(MapleDrawableInfo baseInfo, Board board, int x, int y, int rx0Shift, int rx1Shift,
			int yShift, string limitedname, int mobTime, bool flip, bool hide, int info, int team)
			: base(board, x, y, -1) {
			this.limitedname = limitedname;
			_rx0Shift = rx0Shift;
			_rx1Shift = rx1Shift;
			_yShift = yShift;
			this.mobTime = mobTime;
			this.info = info;
			this.team = team;
			this.flip = flip;
			if (flip)
				// We need to use the data from baseInfo directly because BaseInfo property is only instantiated in the child ctor,
				// which will execute after we are finished.
			{
				X -= baseInfo.Width - 2 * baseInfo.Origin.X;
			}

			this.hide = hide;
		}

		public override void Draw(SpriteBatch sprite, XNA.Color color, int xShift, int yShift) {
			var destinationRectangle =
				new XNA.Rectangle(X + xShift - Origin.X, Y + yShift - Origin.Y, Width, Height);
			//if (baseInfo.Texture == null) baseInfo.CreateTexture(sprite.GraphicsDevice);
			sprite.Draw(BaseInfo.GetTexture(sprite), destinationRectangle, null, color, 0f, new XNA.Vector2(0f, 0f),
				Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1f);
			base.Draw(sprite, color, xShift, yShift);
		}

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

		public string LimitedName {
			get => limitedname;
			set => limitedname = value;
		}

		public override XNA.Color GetColor(SelectionInfo sel, bool selected) {
			var c = base.GetColor(sel, selected);
			if (hide) c.R = (byte) UserSettings.HiddenLifeR;
			return c;
		}

		public bool Hide {
			get => hide;
			set => hide = value;
		}

		public override Bitmap Image => BaseInfo.Image;

		public override int Width => BaseInfo.Width;

		public override int Height => BaseInfo.Height;

		public override Point Origin => BaseInfo.Origin;

		public int rx0Shift {
			get => _rx0Shift;
			set => _rx0Shift = value;
		}

		public int rx1Shift {
			get => _rx1Shift;
			set => _rx1Shift = value;
		}

		public int yShift {
			get => _yShift;
			set => _yShift = value;
		}

		public int MobTime {
			get => mobTime;
			set => mobTime = value;
		}

		public int Info {
			get => info;
			set => info = value;
		}

		public int Team {
			get => team;
			set => team = value;
		}

		public new class SerializationForm : BoardItem.SerializationForm {
			public int rx0, rx1, yshift;
			public int mobtime;
			public string limitedname;
			public bool flip, hide;
			public int info, team;
		}

		public override object Serialize() {
			var result = new SerializationForm();
			UpdateSerializedForm(result);
			return result;
		}

		protected void UpdateSerializedForm(SerializationForm result) {
			base.UpdateSerializedForm(result);
			result.rx0 = _rx0Shift;
			result.rx1 = _rx1Shift;
			result.yshift = _yShift;
			result.mobtime = mobTime;
			result.limitedname = limitedname;
			result.flip = flip;
			result.hide = hide;
			result.info = info;
			result.team = team;
		}

		public LifeInstance(Board board, SerializationForm json)
			: base(board, json) {
			_rx0Shift = json.rx0;
			_rx1Shift = json.rx1;
			_yShift = json.yshift;
			mobTime = json.mobtime;
			limitedname = json.limitedname;
			flip = json.flip;
			hide = json.hide;
			info = json.info;
			team = json.team;
		}

		public void DoSnap() {
			SnapHelper.SnapToFootholdLine(this, 0);
		}
	}
}