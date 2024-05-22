/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Input;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance {
	public class ObjectInstance : LayeredItem, IFlippable, ISnappable {
		private ObjectInfo baseInfo;
		private bool flip;
		private bool _r;
		private string name;
		private bool _hide;
		private bool _reactor;
		private bool _flow;
		private int _rx, _ry, _cx, _cy;
		private string _tags;
		private List<ObjectInstanceQuest> questInfo;

		public ObjectInstance(ObjectInfo baseInfo, Layer layer, Board board, int x, int y, int z, int zM, bool r,
			bool hide, bool reactor, bool flow, int rx, int ry, int cx, int cy, string name,
			string tags, List<ObjectInstanceQuest> questInfo, bool flip)
			: base(board, layer, zM, x, y, z) {
			this.baseInfo = baseInfo;
			this.flip = flip;
			_r = r;
			this.name = name;
			_hide = hide;
			_reactor = reactor;
			_flow = flow;
			_rx = rx;
			_ry = ry;
			_cx = cx;
			_cy = cy;
			_tags = tags;
			this.questInfo = questInfo;
			if (flip) {
				X -= Width - 2 * Origin.X;
			}
		}

		public override ItemTypes Type => ItemTypes.Objects;

		public override MapleDrawableInfo BaseInfo => baseInfo;

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

		private void DrawOffsetMap(SpriteBatch sprite, List<List<XNA.Point>> offsetMap, int xBase, int yBase) {
			foreach (var offsetList in offsetMap)
			foreach (var offset in offsetList) {
				Board.ParentControl.DrawDot(sprite, xBase + offset.X, yBase + offset.Y,
					MultiBoard.RopeInactiveColor, 1);
			}
		}

		public override void Draw(SpriteBatch sprite, XNA.Color color, int xShift, int yShift) {
			var destinationRectangle =
				new XNA.Rectangle(X + xShift - Origin.X, Y + yShift - Origin.Y, Width, Height);

			sprite.Draw(baseInfo.GetTexture(sprite), destinationRectangle, null, color, 0f, new XNA.Vector2(0, 0),
				Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0 /*Layer.LayerNumber / 10f + Z / 1000f*/);
			if (ApplicationSettings.InfoMode) {
				var xBase = X + xShift;
				var yBase = Y + yShift;
				var oi = baseInfo;
				if (oi.RopeOffsets != null) {
					DrawOffsetMap(sprite, oi.RopeOffsets, xBase, yBase);
				}

				if (oi.LadderOffsets != null) {
					DrawOffsetMap(sprite, oi.LadderOffsets, xBase, yBase);
				}
			}

			base.Draw(sprite, color, xShift, yShift);
		}

		public override Bitmap Image => baseInfo.Image;

		public override int Width => baseInfo.Width;

		public override int Height => baseInfo.Height;

		public override Point Origin => baseInfo.Origin;

		public void DoSnap() {
			if (!baseInfo.Connect) {
				return;
			}

			XNA.Point? closestDestPoint = null;
			var closestDistance = double.MaxValue;
			foreach (var li in Board.BoardItems.TileObjs) {
				// Trying to snap to other selected items can mess up some of the mouse bindings
				if (!(li is ObjectInstance) || li.Selected || li.Equals(this)) {
					continue;
				}

				var objInst = (ObjectInstance) li;
				var objInfo = (ObjectInfo) objInst.BaseInfo;
				if (!objInfo.Connect) {
					continue;
				}

				var snapPoint = new XNA.Point(objInst.X,
					objInst.Y - objInst.Origin.Y + objInst.Height + Origin.Y);
				double dx = snapPoint.X - X;
				double dy = snapPoint.Y - Y;
				if (dx > UserSettings.SnapDistance || dy > UserSettings.SnapDistance) {
					continue;
				}

				var distance = InputHandler.Distance(dx, dy);
				if (distance > UserSettings.SnapDistance) {
					continue;
				}

				if (closestDistance > distance) {
					closestDistance = distance;
					closestDestPoint = snapPoint;
				}
			}

			if (closestDestPoint.HasValue) {
				SnapMoveAllMouseBoundItems(new XNA.Point(closestDestPoint.Value.X, closestDestPoint.Value.Y));
			}
		}

		public string Name {
			get => name;
			set => name = value;
		}

		public string tags {
			get => _tags;
			set => _tags = value;
		}

		public bool r {
			get => _r;
			set => _r = value;
		}

		public bool hide {
			get => _hide;
			set => _hide = value;
		}

		public bool flow {
			get => _flow;
			set => _flow = value;
		}

		public bool reactor {
			get => _reactor;
			set => _reactor = value;
		}

		public int rx {
			get => _rx;
			set => _rx = value;
		}

		public int ry {
			get => _ry;
			set => _ry = value;
		}

		public int cx {
			get => _cx;
			set => _cx = value;
		}

		public int cy {
			get => _cy;
			set => _cy = value;
		}

		public List<ObjectInstanceQuest> QuestInfo {
			get => questInfo;
			set => questInfo = value;
		}

		public new class SerializationForm : LayeredItem.SerializationForm {
			public string os, l0, l1, l2;
			public bool flip;
			public bool r;
			public string name;
			public bool hide, reactor, flow;
			public int rx, ry, cx, cy;
			public string tags;
			public ObjectInstanceQuest[] quest;
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
			result.flip = flip;
			result.r = _r;
			result.name = name;
			result.hide = _hide;
			result.reactor = _reactor;
			result.flow = _flow;
			result.rx = _rx;
			result.ry = _ry;
			result.cx = _cx;
			result.cy = _cy;
			result.tags = tags;
			result.quest = questInfo == null ? null : questInfo.ToArray();
		}

		public ObjectInstance(Board board, SerializationForm json)
			: base(board, json) {
			baseInfo = ObjectInfo.Get(json.os, json.l0, json.l1, json.l2);
			flip = json.flip;
			_r = json.r;
			name = json.name;
			_hide = json.hide;
			_reactor = json.reactor;
			_flow = json.flow;
			_rx = json.rx;
			_ry = json.ry;
			_cx = json.cx;
			_cy = json.cy;
			tags = json.tags;
			if (json.quest != null) {
				questInfo = json.quest.ToList();
			}
		}
	}
}