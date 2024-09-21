/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using HaCreator.CustomControls;
using HaCreator.MapEditor.MonoGame;
using HaCreator.MapEditor.UndoRedo;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Shapes {
	/// <summary>
	/// The boundary, that shows the tooltip when the player gets close to it.
	/// </summary>
	public class ToolTipChar : MapleRectangle, ISerializable {
		private ToolTipInstance boundTooltip;

		public ToolTipChar(Board board, XNA.Rectangle rect, ToolTipInstance boundTooltip)
			: base(board, rect) {
			BoundTooltip = boundTooltip;
		}

		public override MapleDot CreateDot(int x, int y) {
			return new ToolTipDot(this, board, x, y);
		}

		public override MapleLine CreateLine(MapleDot a, MapleDot b) {
			return new ToolTipLine(board, a, b);
		}

		public ToolTipInstance BoundTooltip {
			get => boundTooltip;
			set {
				boundTooltip = value;
				if (value != null) {
					value.CharacterToolTip = this;
				}
			}
		}

		public override XNA.Color Color =>
			Selected ? UserSettings.ToolTipCharSelectedFill : UserSettings.ToolTipCharFill;

		public override ItemTypes Type => ItemTypes.ToolTips;

		public override void Draw(Renderer graphics, XNA.Color dotColor, int xShift, int yShift) {
			base.Draw(graphics, dotColor, xShift, yShift);

			if (boundTooltip != null) {
				graphics.DrawLine(new XNA.Vector2(X + Width / 2 + xShift, Y + Height / 2 + yShift),
					new XNA.Vector2(boundTooltip.X + boundTooltip.Width / 2 + xShift,
						boundTooltip.Y + boundTooltip.Height / 2 + yShift), UserSettings.ToolTipBindingLine);
			}
		}

		public override void OnItemPlaced(List<UndoRedoAction> undoPipe) {
			lock (board.ParentControl) {
				base.OnItemPlaced(undoPipe);
				if (undoPipe != null) {
					undoPipe.Add(UndoRedoManager.ToolTipLinked(BoundTooltip, this));
				}
			}
		}

		public override void RemoveItem(List<UndoRedoAction> undoPipe) {
			lock (board.ParentControl) {
				if (boundTooltip == null) {
					return; //already removed via the parent tooltip
				}

				base.RemoveItem(undoPipe);
				if (undoPipe != null) {
					undoPipe.Add(UndoRedoManager.ToolTipUnlinked(boundTooltip, this));
				}

				boundTooltip.CharacterToolTip = null;
				boundTooltip = null;
			}
		}

		public override bool ShouldSelectSerialized => true;

		public override List<ISerializableSelector> SelectSerialized(HashSet<ISerializableSelector> serializedItems) {
			return new List<ISerializableSelector> {boundTooltip};
		}

		public ToolTipChar(Board board, SerializationForm json)
			: base(board, json) {
		}
	}
}