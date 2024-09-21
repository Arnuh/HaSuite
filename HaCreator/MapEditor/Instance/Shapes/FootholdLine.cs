/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using HaCreator.CustomControls;
using HaCreator.MapEditor.MonoGame;
using HaCreator.MapEditor.UndoRedo;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework.Graphics;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Shapes {
	public class FootholdLine : MapleLine, IContainsLayerInfo, ISerializable {
		private bool _cantThrough;
		private bool _forbidFallDown;
		private int _piece;
		private double _force;

		// internal use variables
		public int prev = 0;
		public int next = 0;
		public int num;
		public bool saved;

		public FootholdLine(Board board, MapleDot firstDot, MapleDot secondDot)
			: base(board, firstDot, secondDot) {
			_cantThrough = false;
			_forbidFallDown = false;
			_piece = 0;
			_force = 0;
		}

		public FootholdLine(Board board, MapleDot firstDot)
			: base(board, firstDot) {
			_cantThrough = false;
			_forbidFallDown = false;
			_piece = 0;
			_force = 0;
		}

		public FootholdLine(Board board, MapleDot firstDot, MapleDot secondDot, bool forbidFallDown,
			bool cantThrough, int piece, double force)
			: base(board, firstDot, secondDot) {
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
			}

			return MultiBoard.FootholdSideInactiveColor;
		}

		public override void Draw(Renderer graphics, XNA.Color color, int xShift, int yShift) {
			base.Draw(graphics, color, xShift, yShift);

#if DEBUG
			graphics.DrawString($"{num}, P: {prev}, N: {next}",
				MultiBoard.VirtualToPhysical(firstDot.X, board.CenterPoint.X, board.hScroll, 0),
				MultiBoard.VirtualToPhysical(firstDot.Y - 20, board.CenterPoint.Y, board.vScroll, 0));
#endif
			if (!UserSettings.displayFHSide) {
				return;
			}

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
			graphics.DrawLine(new XNA.Vector2(firstDot.X + xShift + xOffset, firstDot.Y + yShift + yOffset),
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
			foreach (var fh in board.BoardItems.FootholdLines) {
				if ((fh.FirstDot.X == x1 && fh.FirstDot.Y == y1 &&
				     fh.SecondDot.X == x2 && fh.SecondDot.Y == y2) ||
				    (fh.FirstDot.X == x2 && fh.FirstDot.Y == y2 &&
				     fh.SecondDot.X == x1 && fh.SecondDot.Y == y1)) {
					return true;
				}
			}

			return false;
		}

		public void Flip() {
			var temp = FirstDot;
			var tempLines = FirstDot == null ? new List<MapleLine>() : new List<MapleLine>(FirstDot.connectedLines);
			var tempLines2 = SecondDot == null ? new List<MapleLine>() : new List<MapleLine>(SecondDot.connectedLines);
			FirstDot = SecondDot;
			SecondDot = temp;

			FirstDot?.connectedLines.Clear();
			FirstDot?.connectedLines.AddRange(tempLines2);
			SecondDot?.connectedLines.Clear();
			SecondDot?.connectedLines.AddRange(tempLines);
		}

		public static FootholdLine[] GetSelectedFootholds(Board board) {
			var length = 0;
			foreach (var line in board.BoardItems.FootholdLines) {
				if (line.Selected) {
					length++;
				}
			}

			var result = new FootholdLine[length];
			var index = 0;
			foreach (var line in board.BoardItems.FootholdLines) {
				if (line.Selected) {
					result[index] = line;
					index++;
				}
			}

			return result;
		}

		public bool IsWall => FirstDot.X == SecondDot.X;

		public double Force {
			get => _force;
			set => _force = value;
		}

		public int Piece {
			get => _piece;
			set => _piece = value;
		}

		public bool ForbidFallDown {
			get => _forbidFallDown;
			set => _forbidFallDown = value;
		}

		public bool CantThrough {
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
			if (a.num == 0 && b.num != 0) {
				return 1;
			}

			if (a.num != 0 && b.num == 0) {
				return -1;
			}

			if (a.num > b.num) {
				return 1;
			}

			if (a.num < b.num) {
				return -1;
			}

			return 0;
		}

		public FootholdAnchor GetOtherAnchor(FootholdAnchor first) {
			if (FirstDot == first) {
				return (FootholdAnchor) SecondDot;
			}

			if (SecondDot == first) {
				return (FootholdAnchor) FirstDot;
			}

			throw new Exception("GetOtherAnchor: line is not properly connected");
		}

		#region ISerializable Implementation

		public class SerializationForm {
			public bool cantthrough, forbidfalldown;
			public int piece;
			public double force;
		}

		public bool ShouldSelectSerialized => true;

		public List<ISerializableSelector> SelectSerialized(HashSet<ISerializableSelector> serializedItems) {
			var result = new List<ISerializableSelector>();
			// We add the dots to make sure they are serialized (we might have been added as a prev/next override of another line)
			result.Add(FirstDot);
			result.Add(SecondDot);
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

		public IDictionary<string, object> SerializeBindings(Dictionary<ISerializable, long> refDict) {
			var result = new Dictionary<string, object>();
			result[FIRSTDOT_KEY] = refDict[(FootholdAnchor) FirstDot];
			result[SECONDDOT_KEY] = refDict[(FootholdAnchor) SecondDot];
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