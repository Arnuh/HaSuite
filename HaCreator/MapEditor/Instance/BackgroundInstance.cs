/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using HaCreator.MapEditor.Info;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance {
	public class BackgroundInstance : BoardItem, IFlippable, ISerializable {
		private readonly BackgroundInfo baseInfo;
		private bool flip;
		private int _a; //alpha
		private int _cx; //copy x
		private int _cy; //copy y
		private int _rx;
		private int _ry;
		private bool _front;
		private int _screenMode;
		private string _spineAni;
		private bool _spineRandomStart;
		private BackgroundType _type;

		public BackgroundInstance(BackgroundInfo baseInfo, Board board, int x, int y, int z, int rx, int ry, int cx,
			int cy, BackgroundType type, int a, bool front, bool flip, int _screenMode,
			string _spineAni, bool _spineRandomStart)
			: base(board, x, y, z) {
			this.baseInfo = baseInfo;
			this.flip = flip;
			_rx = rx;
			_ry = ry;
			_cx = cx;
			_cy = cy;
			_a = a;
			_type = type;
			_front = front;
			this._screenMode = _screenMode;
			this._spineAni = _spineAni;
			this._spineRandomStart = _spineRandomStart;

			if (flip) {
				BaseX -= Width - 2 * Origin.X;
			}
		}

		public override ItemTypes Type => ItemTypes.Backgrounds;

		public bool Flip {
			get => flip;
			set {
				if (flip == value) return;
				flip = value;
				var xFlipShift = Width - 2 * Origin.X;
				if (flip) {
					BaseX -= xFlipShift;
				} else {
					BaseX += xFlipShift;
				}
			}
		}

		public int UnflippedX => flip ? BaseX + Width - 2 * Origin.X : BaseX;

		public override void Draw(SpriteBatch sprite, XNA.Color color, int xShift, int yShift) {
			if (sprite == null || baseInfo.GetTexture(sprite) == null) {
				return;
			}

			var destinationRectangle =
				new XNA.Rectangle(X + xShift - Origin.X, Y + yShift - Origin.Y, Width, Height);
			sprite.Draw(baseInfo.GetTexture(sprite), destinationRectangle, null, color, 0f, new XNA.Vector2(0f, 0f),
				Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);

			base.Draw(sprite, color, xShift, yShift);
		}

		public override MapleDrawableInfo BaseInfo => baseInfo;

		public override Bitmap Image => baseInfo.Image;

		public override int Width => baseInfo.Width;

		public override int Height => baseInfo.Height;

		public override Point Origin => baseInfo.Origin;

		// Parallax + Undo\Redo is troublesome. I don't like this way either.
		public int BaseX {
			get => (int) position.X;
			set => position.X = value;
		}

		public int BaseY {
			get => (int) position.Y;
			set => position.Y = value;
		}

		public int rx {
			get => _rx;
			set => _rx = value;
		}

		public int ry {
			get => _ry;
			set => _ry = value;
		}

		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public int cx {
			get => _cx;
			set => _cx = value;
		}

		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public int cy {
			get => _cy;
			set => _cy = value;
		}

		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public int a {
			get => _a;
			set => _a = value;
		}

		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public BackgroundType type {
			get => _type;
			set => _type = value;
		}

		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public bool front {
			get => _front;
			set => _front = value;
		}

		/// <summary>
		/// The screen resolution to display this background object. (0 = all res)
		/// </summary>
		public int screenMode {
			get => _screenMode;
			set => _screenMode = value;
		}

		/// <summary>
		/// Spine animation path 
		/// </summary>
		public string SpineAni {
			get => _spineAni;
			set => _spineAni = value;
		}

		public bool SpineRandomStart {
			get => _spineRandomStart;
			set => _spineRandomStart = value;
		}

		public int CalculateBackgroundPosX() {
			//double dpi = ScreenDPIUtil.GetScreenScaleFactor(); // dpi affected via window.. does not have to be calculated manually
			double dpi = 1;
			var width = (int) (Board.ParentControl.CurrentDXWindowSize.Width / 2 / dpi); // 400;

			return rx * (Board.hScroll - Board.CenterPoint.X + width) / 100 + base.X /*- Origin.X*/ + width -
				Board.CenterPoint.X + Board.hScroll;
		}

		public int CalculateBackgroundPosY() {
			//double dpi = ScreenDPIUtil.GetScreenScaleFactor(); // dpi affected via window.. does not have to be calculated manually
			double dpi = 1;
			var height = (int) (Board.ParentControl.CurrentDXWindowSize.Height / 2 / dpi); // 300;

			return ry * (Board.vScroll - Board.CenterPoint.Y + height) / 100 + base.Y /*- Origin.X*/ + height -
				Board.CenterPoint.Y + Board.vScroll;
		}

		public int ReverseBackgroundPosX(int bgPos) {
			//double dpi = ScreenDPIUtil.GetScreenScaleFactor(); // dpi affected via window.. does not have to be calculated manually
			double dpi = 1;
			var width = (int) (Board.ParentControl.CurrentDXWindowSize.Width / 2 / dpi); // 400;

			return bgPos - Board.hScroll + Board.CenterPoint.X - width -
			       rx * (Board.hScroll - Board.CenterPoint.X + width) / 100;
		}

		public int ReverseBackgroundPosY(int bgPos) {
			//double dpi = ScreenDPIUtil.GetScreenScaleFactor(); // dpi affected via window.. does not have to be calculated manually
			double dpi = 1;
			var height = (int) (Board.ParentControl.CurrentDXWindowSize.Height / 2 / dpi); // 300;

			return bgPos - Board.vScroll + Board.CenterPoint.Y - height -
			       ry * (Board.vScroll - Board.CenterPoint.Y + height) / 100;
		}

		public override int X {
			get {
				if (UserSettings.emulateParallax) {
					return CalculateBackgroundPosX();
				}

				return base.X;
			}
			set {
				int newX;
				if (UserSettings.emulateParallax) {
					newX = ReverseBackgroundPosX(value);
				} else {
					newX = value;
				}

				base.Move(newX, base.Y);
			}
		}

		public override int Y {
			get {
				if (UserSettings.emulateParallax) {
					return CalculateBackgroundPosY();
				}

				return base.Y;
			}
			set {
				int newY;
				if (UserSettings.emulateParallax) {
					newY = ReverseBackgroundPosY(value);
				} else {
					newY = value;
				}

				base.Move(base.X, newY);
			}
		}

		public override void Move(int x, int y) {
			X = x;
			Y = y;
		}

		public void MoveBase(int x, int y) {
			BaseX = x;
			BaseY = y;
		}

		public new class SerializationForm : BoardItem.SerializationForm {
			public bool flip;
			public int a, cx, cy, rx, ry;
			public bool front;
			public int screenMode;
			public BackgroundType type;
			public string bs;
			public BackgroundInfoType backgroundInfoType;
			public string no;
			public string spineAni;
			public bool spineRandomStart;
		}

		public override object Serialize() {
			var result = new SerializationForm();
			UpdateSerializedForm(result);
			return result;
		}

		protected void UpdateSerializedForm(SerializationForm result) {
			base.UpdateSerializedForm(result);
			result.flip = flip;
			result.a = _a;
			result.cx = _cx;
			result.cy = _cy;
			result.rx = _rx;
			result.ry = _ry;
			result.front = _front;
			result.screenMode = _screenMode;
			result.spineAni = _spineAni;
			result.spineRandomStart = _spineRandomStart;
			result.type = _type;
			result.bs = baseInfo.bS;
			result.backgroundInfoType = baseInfo.Type;
			result.no = baseInfo.no;
		}

		public BackgroundInstance(Board board, SerializationForm json)
			: base(board, json) {
			flip = json.flip;
			_a = json.a;
			_cx = json.cx;
			_cy = json.cy;
			_rx = json.rx;
			_ry = json.ry;
			_front = json.front;
			_type = json.type;
			_screenMode = json.screenMode;
			_spineAni = json.spineAni;
			_spineRandomStart = json.spineRandomStart;

			baseInfo = BackgroundInfo.Get(board.ParentControl.GraphicsDevice, json.bs, json.backgroundInfoType,
				json.no);
		}

		public override void PostDeserializationActions(bool? selected, XNA.Point? offset) {
			if (selected.HasValue) Selected = selected.Value;

			if (offset.HasValue) Move(X + offset.Value.X, Y + offset.Value.Y);
		}
	}
}