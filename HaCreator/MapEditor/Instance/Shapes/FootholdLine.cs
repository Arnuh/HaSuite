/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using HaCreator.MapEditor.UndoRedo;
using MapleLib.WzLib.WzStructure;
using MapleLib.WzLib.WzStructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Shapes {
	public class FootholdLine : MapleLine, IContainsLayerInfo, ISerializable {
		private MapleBool _cantThrough;
		private MapleBool _forbidFallDown;
		private int? _piece;
		private int? _force;

		// internal use variables
		public int prev = 0;
		public int next = 0;
		public FootholdLine prevOverride = null;
		public FootholdLine nextOverride = null;
		public int num;
		public bool saved;

		public FootholdLine(Board board, MapleDot firstDot, MapleDot secondDot)
			: base(board, firstDot, secondDot) {
			_cantThrough = null;
			_forbidFallDown = null;
			_piece = null;
			_force = null;
		}

		public FootholdLine(Board board, MapleDot firstDot)
			: base(board, firstDot) {
			_cantThrough = null;
			_forbidFallDown = null;
			_piece = null;
			_force = null;
		}

		public FootholdLine(Board board, MapleDot firstDot, MapleDot secondDot, MapleBool forbidFallDown,
			MapleBool cantThrough, int? piece, int? force)
			: base(board, firstDot, secondDot) {
			_cantThrough = cantThrough;
			_forbidFallDown = forbidFallDown;
			_piece = piece;
			_force = force;
		}

		public FootholdLine(Board board, MapleDot firstDot, MapleBool forbidFallDown, MapleBool cantThrough, int? piece,
			int? force)
			: base(board, firstDot) {
			_cantThrough = cantThrough;
			_forbidFallDown = forbidFallDown;
			_piece = piece;
			_force = force;
		}

		public override XNA.Color Color => UserSettings.FootholdColor;

		public override XNA.Color InactiveColor => MultiBoard.FootholdInactiveColor;

		public override ItemTypes Type => ItemTypes.Footholds;

		public virtual XNA.Color GetColorSide(SelectionInfo sel) {
			if ((sel.editedTypes & Type) == Type && firstDot.CheckIfLayerSelected(sel)) {
				return Selected ? UserSettings.SelectedColor : UserSettings.FootholdSideColor;
			} else {
				return MultiBoard.FootholdSideInactiveColor;
			}
		}

		public override void Draw(SpriteBatch sprite, XNA.Color color, int xShift, int yShift) {
			base.Draw(sprite, color, xShift, yShift);

#if DEBUG
			board.ParentControl.DrawString(sprite, $"{num}, P: {prev}, N: {next}",
				MultiBoard.VirtualToPhysical(firstDot.X, board.CenterPoint.X, board.hScroll, 0),
				MultiBoard.VirtualToPhysical(firstDot.Y - 20, board.CenterPoint.Y, board.vScroll, 0));
#endif
			if (!UserSettings.displayFHSide) return;
			var xOffset = 1;
			var yOffset = 1;

			if (firstDot.X < secondDot.X) {
				xOffset *= -1;
				yOffset *= -1;
			} else if (firstDot.Y > secondDot.Y) {
				xOffset *= -1;
				yOffset *= -1;
			}

			// The side the line is on means it blocks you from that direction.
			board.ParentControl.DrawLine(sprite, new XNA.Vector2(firstDot.X + xShift + xOffset, firstDot.Y + yShift + yOffset),
				new XNA.Vector2(secondDot.X + xShift + xOffset, secondDot.Y + yShift + yOffset), GetColorSide(board.GetUserSelectionInfo()));
		}

		public bool FhEquals(FootholdLine obj) {
			return (obj.FirstDot.X == FirstDot.X && obj.SecondDot.X == SecondDot.X
			                                     && obj.FirstDot.Y == FirstDot.Y &&
			                                     obj.SecondDot.Y == SecondDot.Y)
			       || (obj.FirstDot.X == SecondDot.X &&
			           obj.SecondDot.X == FirstDot.X
			           && obj.FirstDot.Y == SecondDot.Y &&
			           obj.SecondDot.Y == FirstDot.Y);
		}

		public static bool Exists(int x1, int y1, int x2, int y2, Board board) {
			foreach (var fh in board.BoardItems.FootholdLines)
				if ((fh.FirstDot.X == x1 && fh.FirstDot.Y == y1 &&
				     fh.SecondDot.X == x2 && fh.SecondDot.Y == y2) ||
				    (fh.FirstDot.X == x2 && fh.FirstDot.Y == y2 &&
				     fh.SecondDot.X == x1 && fh.SecondDot.Y == y1))
					return true;

			return false;
		}

		public static FootholdLine[] GetSelectedFootholds(Board board) {
			var length = 0;
			foreach (var line in board.BoardItems.FootholdLines)
				if (line.Selected)
					length++;
			var result = new FootholdLine[length];
			var index = 0;
			foreach (var line in board.BoardItems.FootholdLines)
				if (line.Selected) {
					result[index] = line;
					index++;
				}

			return result;
		}

		public bool IsWall => FirstDot.X == SecondDot.X;

		public int? Force {
			get => _force;
			set => _force = value;
		}

		public int? Piece {
			get => _piece;
			set => _piece = value;
		}

		public MapleBool ForbidFallDown {
			get => _forbidFallDown;
			set => _forbidFallDown = value;
		}

		public MapleBool CantThrough {
			get => _cantThrough;
			set => _cantThrough = value;
		}

		public int LayerNumber {
			get => ((FootholdAnchor) FirstDot).LayerNumber;
			set => throw new NotImplementedException();
		}

		public int PlatformNumber {
			get => ((FootholdAnchor) FirstDot).PlatformNumber;
			set => throw new NotImplementedException();
		}

		public static int FHSorter(FootholdLine a, FootholdLine b) {
			if (a.num == 0 && b.num != 0) return 1;
			if (a.num != 0 && b.num == 0) return -1;
			if (a.num > b.num) return 1;
			if (a.num < b.num) return -1;
			return 0;
		}

		public FootholdAnchor GetOtherAnchor(FootholdAnchor first) {
			if (FirstDot == first)
				return (FootholdAnchor) SecondDot;
			else if (SecondDot == first)
				return (FootholdAnchor) FirstDot;
			else
				throw new Exception("GetOtherAnchor: line is not properly connected");
		}

		#region ISerializable Implementation

		public class SerializationForm {
			public MapleBool cantthrough, forbidfalldown;
			public int? piece, force;
		}

		public bool ShouldSelectSerialized => true;

		public List<ISerializableSelector> SelectSerialized(HashSet<ISerializableSelector> serializedItems) {
			var result = new List<ISerializableSelector>();
			// We add the dots to make sure they are serialized (we might have been added as a prev/next override of another line)
			result.Add(FirstDot);
			result.Add(SecondDot);
			if (prevOverride != null)
				result.Add(prevOverride);
			if (nextOverride != null)
				result.Add(nextOverride);
			return result;
		}

		public object Serialize() {
			var result = new SerializationForm();
			result.cantthrough = _cantThrough;
			result.forbidfalldown = _forbidFallDown;
			result.piece = _piece;
			result.force = _force;
			return result;
		}

		private const string FIRSTDOT_KEY = "dot1";
		private const string SECONDDOT_KEY = "dot2";
		private const string PREVOVERRIDE_KEY = "prevOverride";
		private const string NEXTOVERRIDE_KEY = "nextOverride";

		public IDictionary<string, object> SerializeBindings(Dictionary<ISerializable, long> refDict) {
			var result = new Dictionary<string, object>();
			result[FIRSTDOT_KEY] = refDict[(FootholdAnchor) FirstDot];
			result[SECONDDOT_KEY] = refDict[(FootholdAnchor) SecondDot];
			if (prevOverride != null)
				result[PREVOVERRIDE_KEY] = refDict[prevOverride];
			if (nextOverride != null)
				result[NEXTOVERRIDE_KEY] = refDict[nextOverride];
			return result;
		}

		public FootholdLine(Board board, SerializationForm json)
			: base(board) {
			_cantThrough = json.cantthrough;
			_forbidFallDown = json.forbidfalldown;
			_piece = json.piece;
			_force = json.force;
		}

		public void DeserializeBindings(IDictionary<string, object> bindSer, Dictionary<long, ISerializable> refDict) {
			firstDot = (FootholdAnchor) refDict[(long) bindSer[FIRSTDOT_KEY]];
			secondDot = (FootholdAnchor) refDict[(long) bindSer[SECONDDOT_KEY]];
			if (bindSer.ContainsKey(PREVOVERRIDE_KEY))
				prevOverride = (FootholdLine) refDict[(long) bindSer[PREVOVERRIDE_KEY]];
			if (bindSer.ContainsKey(NEXTOVERRIDE_KEY))
				prevOverride = (FootholdLine) refDict[(long) bindSer[NEXTOVERRIDE_KEY]];
			firstDot.connectedLines.Add(this);
			secondDot.connectedLines.Add(this);
			firstDot.PointMoved += OnFirstDotMoved;
			secondDot.PointMoved += OnSecondDotMoved;
		}

		public void AddToBoard(List<UndoRedoAction> undoPipe) {
			base.OnPlaced(undoPipe);
			Board.BoardItems.FootholdLines.Add(this);
		}

		public void PostDeserializationActions(bool? selected, XNA.Point? offset) {
			// Nothing to do here, we cant be offset nor selected.
		}

		#endregion
	}
}