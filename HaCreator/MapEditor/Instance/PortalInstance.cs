/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using HaCreator.MapEditor.Info;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance {
	public class PortalInstance : BoardItem, ISerializable {
		private PortalInfo baseInfo;
		private string _pn;
		private int _pt;
		private string _tn;
		private int _tm;
		private string _script;
		private int _delay;
		private bool _hideTooltip;
		private bool _onlyOnce;
		private int _horizontalImpact;
		private int _verticalImpact;
		private string _image;
		private int _hRange;
		private int _vRange;

		public PortalInstance(PortalInfo baseInfo, Board board, int x, int y, string pn, int pt, string tn, int tm,
			string script, int delay, bool hideTooltip, bool onlyOnce, int horizontalImpact,
			int verticalImpact, string image, int hRange, int vRange)
			: base(board, x, y, -1) {
			this.baseInfo = baseInfo;
			_pn = pn;
			_pt = pt;
			_tn = tn;
			_tm = tm;
			_script = script;
			_delay = delay;
			_hideTooltip = hideTooltip;
			_onlyOnce = onlyOnce;
			_horizontalImpact = horizontalImpact;
			_verticalImpact = verticalImpact;
			_image = image;
			_hRange = hRange;
			_vRange = vRange;
		}

		public override void Draw(SpriteBatch sprite, XNA.Color color, int xShift, int yShift) {
			var destinationRectangle =
				new XNA.Rectangle((int) X + xShift - Origin.X, (int) Y + yShift - Origin.Y, Width, Height);
			sprite.Draw(baseInfo.GetTexture(sprite), destinationRectangle, null, color, 0f, new XNA.Vector2(0f, 0f),
				SpriteEffects.None, 1f);
			base.Draw(sprite, color, xShift, yShift);
		}

		public override MapleDrawableInfo BaseInfo => baseInfo;

		public override ItemTypes Type => ItemTypes.Portals;

		public override System.Drawing.Bitmap Image => baseInfo.Image;

		public override int Width => baseInfo.Width;

		public override int Height => baseInfo.Height;

		public override System.Drawing.Point Origin => baseInfo.Origin;

		/// <summary>
		/// The image number in Map.wz/MapHelper.img/portal/game/(portal type)/(image number)
		/// </summary>
		public string image {
			get => _image;
			set => _image = value;
		}

		public string pn {
			get => _pn;
			set => _pn = value;
		}

		public int pt {
			get => _pt;
			set {
				_pt = value;
				baseInfo = PortalInfo.GetPortalInfoByType(value);
			}
		}

		public string tn {
			get => _tn;
			set => _tn = value;
		}

		public int tm {
			get => _tm;
			set => _tm = value;
		}

		public string script {
			get => _script;
			set => _script = value;
		}

		public int delay {
			get => _delay;
			set => _delay = value;
		}

		public bool hideTooltip {
			get => _hideTooltip;
			set => _hideTooltip = value;
		}

		public bool onlyOnce {
			get => _onlyOnce;
			set => _onlyOnce = value;
		}

		public int horizontalImpact {
			get => _horizontalImpact;
			set => _horizontalImpact = value;
		}

		public int verticalImpact {
			get => _verticalImpact;
			set => _verticalImpact = value;
		}

		public int hRange {
			get => _hRange;
			set => _hRange = value;
		}

		public int vRange {
			get => _vRange;
			set => _vRange = value;
		}

		public new class SerializationForm : BoardItem.SerializationForm {
			public string pn, tn;
			public int pt;
			public int tm;
			public string script;
			public int delay;
			public bool hidett, onlyonce;
			public int himpact, vimpact;
			public string image;
			public int hrange, vrange;
		}

		public override object Serialize() {
			var result = new SerializationForm();
			UpdateSerializedForm(result);
			return result;
		}

		protected void UpdateSerializedForm(SerializationForm result) {
			base.UpdateSerializedForm(result);
			result.pn = _pn;
			result.pt = _pt;
			result.tn = _tn;
			result.tm = _tm;
			result.script = _script;
			result.delay = _delay;
			result.hidett = _hideTooltip;
			result.onlyonce = _onlyOnce;
			result.himpact = _horizontalImpact;
			result.vimpact = _verticalImpact;
			result.image = _image;
			result.hrange = _hRange;
			result.vrange = _vRange;
		}

		public PortalInstance(Board board, SerializationForm json)
			: base(board, json) {
			_pn = json.pn;
			_pt = json.pt;
			_tn = json.tn;
			_tm = json.tm;
			_script = json.script;
			_delay = json.delay;
			_hideTooltip = json.hidett;
			_onlyOnce = json.onlyonce;
			_horizontalImpact = json.himpact;
			_verticalImpact = json.vimpact;
			_image = json.image;
			_hRange = json.hrange;
			_vRange = json.vrange;
			baseInfo = PortalInfo.GetPortalInfoByType(pt);
		}
	}
}