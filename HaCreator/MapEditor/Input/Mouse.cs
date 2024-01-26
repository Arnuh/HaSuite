/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.Instance.Misc;
using HaCreator.MapEditor.Instance.Shapes;
using HaCreator.MapEditor.UndoRedo;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework.Graphics;
using Xna = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Input {
	public class Mouse : MapleDot //inheriting mapledot to make it easier to attach maplelines to it
	{
		private Bitmap placeholder = new Bitmap(1, 1);
		private Point origin = new Point(0, 0);
		private bool isDown;
		private bool minimapBrowseOngoing;
		private bool cameraPanning;
		private Xna.Point cameraPanningStart;
		private bool multiSelectOngoing;
		private Xna.Point multiSelectStart;
		private bool singleSelectStarting;
		private Xna.Point singleSelectStart;
		private MouseState state;
		private MapleDrawableInfo currAddedInfo;
		private BoardItem currAddedObj;
		private TileInfo[] tileRandomList;

		public Mouse(Board board)
			: base(board, 0, 0) {
			IsDown = false;
		}

		public static int NextInt32(int max) {
			var bytes = new byte[sizeof(int)];
			var Gen = new RNGCryptoServiceProvider();
			Gen.GetBytes(bytes);
			return Math.Abs(BitConverter.ToInt32(bytes, 0) % max);
		}

		public void PlaceObject() {
			lock (Board.ParentControl) {
				if (state == MouseState.StaticObjectAdding || state == MouseState.RandomTiles) // tiles, obj
				{
					var undoPipe = new List<UndoRedoAction>();
					currAddedObj.OnItemPlaced(undoPipe);
					Board.UndoRedoMan.AddUndoBatch(undoPipe);
					ReleaseItem(currAddedObj);
					if (currAddedObj is LayeredItem) {
						var highestZ = 0;
						foreach (var item in Board.BoardItems.TileObjs) {
							if (item.Z > highestZ) {
								highestZ = item.Z;
							}
						}

						currAddedObj.Z = highestZ;
						Board.BoardItems.Sort();
					}

					if (state == MouseState.StaticObjectAdding) {
						var boardItem = currAddedInfo.CreateInstance(Board.SelectedLayer, Board,
							X + currAddedInfo.Origin.X - currAddedInfo.Image.Width / 2,
							Y + currAddedInfo.Origin.Y - currAddedInfo.Image.Height / 2, 50, false);
						currAddedObj = boardItem;
					} else {
						var boardItem = tileRandomList[NextInt32(tileRandomList.Length)]
							.CreateInstance(Board.SelectedLayer, Board,
								X + currAddedInfo.Origin.X - currAddedInfo.Image.Width / 2,
								Y + currAddedInfo.Origin.Y - currAddedInfo.Image.Height / 2, 50, false);
						currAddedObj = boardItem;
					}

					Board.BoardItems.Add(currAddedObj, false);
					BindItem(currAddedObj,
						new Xna.Point(currAddedInfo.Origin.X - currAddedInfo.Image.Width / 2,
							currAddedInfo.Origin.Y - currAddedInfo.Image.Height / 2));
				} else if (state == MouseState.Chairs) // Chair
				{
					Board.UndoRedoMan.AddUndoBatch(new List<UndoRedoAction> {UndoRedoManager.ItemAdded(currAddedObj)});
					ReleaseItem(currAddedObj);
					currAddedObj = new Chair(Board, X, Y);
					Board.BoardItems.Add(currAddedObj, false);
					BindItem(currAddedObj, new Xna.Point());
				} else if (state == MouseState.Ropes) // Ropes
				{
					var count = BoundItems.Count;
					var anchor = (RopeAnchor) BoundItems.Keys.ElementAt(0);
					ReleaseItem(anchor);
					if (count == 1) {
						Board.UndoRedoMan.AddUndoBatch(new List<UndoRedoAction>
							{UndoRedoManager.RopeAdded(anchor.ParentRope)});
						CreateRope();
					}
				} else if (state == MouseState.Tooltip) // Tooltip
				{
					var count = BoundItems.Count;
					var dot = (ToolTipDot) BoundItems.Keys.ElementAt(0);
					ReleaseItem(dot);
					if (count == 1) {
						var undoPipe = new List<UndoRedoAction>();
						dot.ParentTooltip.OnItemPlaced(undoPipe);
						Board.UndoRedoMan.AddUndoBatch(undoPipe);
						CreateTooltip();
					}
				} else if (state == MouseState.Clock) // Clock
				{
					var count = BoundItems.Count;
					var items = BoundItems.Keys.ToList();
					Clock clock = null;
					foreach (var item in items) {
						if (item is Clock) {
							clock = (Clock) item;
						}
					}

					foreach (var item in items) ReleaseItem(item);

					var undoPipe = new List<UndoRedoAction>();
					clock.OnItemPlaced(undoPipe);
					Board.UndoRedoMan.AddUndoBatch(undoPipe);
					CreateClock();
				}
			}
		}

		private void CreateRope() {
			lock (Board.ParentControl) {
				var rope = new Rope(Board, X, Y, Y, false, Board.SelectedLayerIndex, true);
				Board.BoardItems.Ropes.Add(rope);
				BindItem(rope.FirstAnchor, new Xna.Point());
				BindItem(rope.SecondAnchor, new Xna.Point());
			}
		}

		private void CreateTooltip() {
			lock (Board.ParentControl) {
				var tt = new ToolTipInstance(Board, new Xna.Rectangle(X, Y, 0, 0), "Title", "Description");
				Board.BoardItems.ToolTips.Add(tt);
				BindItem(tt.PointA, new Xna.Point());
				BindItem(tt.PointC, new Xna.Point());
			}
		}

		private void CreateClock() {
			lock (Board.ParentControl) {
				var clock = new Clock(Board, new Xna.Rectangle(X - 100, Y - 100, 200, 200));
				Board.BoardItems.MiscItems.Add(clock);
				BindItem(clock, new Xna.Point(clock.Width / 2, clock.Height / 2));
				BindItem(clock.PointA, new Xna.Point(-clock.Width / 2, -clock.Height / 2));
				BindItem(clock.PointB, new Xna.Point(clock.Width / 2, -clock.Height / 2));
				BindItem(clock.PointC, new Xna.Point(clock.Width / 2, clock.Height / 2));
				BindItem(clock.PointD, new Xna.Point(-clock.Width / 2, clock.Height / 2));
			}
		}


		public void CreateFhAnchor() {
			lock (Board.ParentControl) {
				var fhAnchor =
					new FootholdAnchor(Board, X, Y, Board.SelectedLayerIndex, Board.SelectedPlatform, true);
				Board.BoardItems.FHAnchors.Add(fhAnchor);
				Board.UndoRedoMan.AddUndoBatch(new List<UndoRedoAction> {UndoRedoManager.ItemAdded(fhAnchor)});
				if (connectedLines.Count == 0) {
					Board.BoardItems.FootholdLines.Add(new FootholdLine(Board, fhAnchor));
				} else {
					connectedLines[0].ConnectSecondDot(fhAnchor);
					Board.BoardItems.FootholdLines.Add(new FootholdLine(Board, fhAnchor));
				}
			}
		}

		public void TryConnectFoothold() {
			lock (Board.ParentControl) {
				var pos = new Xna.Point(X, Y);
				var sel = board.GetUserSelectionInfo();
				foreach (var anchor in Board.BoardItems.FHAnchors) {
					if (MultiBoard.IsPointInsideRectangle(pos, anchor.Left, anchor.Top, anchor.Right, anchor.Bottom) &&
					    anchor.CheckIfLayerSelected(sel)) {
						if (anchor.connectedLines.Count > 1) continue;

						if (connectedLines.Count > 0) // Are we already holding a foothold?
						{
							// We are, so connect the two ends
							// Check that we are not connecting a foothold to itself, or creating duplicate footholds
							if (connectedLines[0].FirstDot != anchor && !FootholdLine.Exists(anchor.X, anchor.Y,
								    connectedLines[0].FirstDot.X, connectedLines[0].FirstDot.Y, Board)) {
								Board.UndoRedoMan.AddUndoBatch(new List<UndoRedoAction>
									{UndoRedoManager.LineAdded(connectedLines[0], connectedLines[0].FirstDot, anchor)});
								connectedLines[0].ConnectSecondDot(anchor);
								// Now that we finished the previous foothold, create a new one between the anchor and the mouse
								var fh = new FootholdLine(Board, anchor);
								Board.BoardItems.FootholdLines.Add(fh);
							}
						} else // Construct a footholdline between the anchor and the mouse
						{
							Board.BoardItems.FootholdLines.Add(new FootholdLine(Board, anchor));
						}
					}
				}
			}
		}

		public void Clear() {
			lock (Board.ParentControl) {
				if (currAddedObj != null) {
					currAddedObj.RemoveItem(null);
					currAddedObj = null;
				}

				if (state == MouseState.Ropes || state == MouseState.Tooltip) {
					if (state == MouseState.Ropes) {
						((RopeAnchor) BoundItems.Keys.ElementAt(0)).RemoveItem(null);
					} else {
						((ToolTipDot) BoundItems.Keys.ElementAt(0)).ParentTooltip.RemoveItem(null);
					}
				} else if (state == MouseState.Footholds && connectedLines.Count > 0) {
					var fh = (FootholdLine) connectedLines[0];
					fh.Remove(false, null);
					Board.BoardItems.FootholdLines.Remove(fh);
				} else if (state == MouseState.Clock) {
					var items = BoundItems.Keys.ToList();
					foreach (var item in items) item.RemoveItem(null);
				}

				InputHandler.ClearBoundItems(Board);
				InputHandler.ClearSelectedItems(Board);
				IsDown = false;
			}
		}

		public void SelectionMode() {
			lock (Board.ParentControl) {
				Clear();
				currAddedInfo = null;
				tileRandomList = null;
				state = MouseState.Selection;
			}
		}

		public void SetHeldInfo(MapleDrawableInfo newInfo) {
			lock (Board.ParentControl) {
				Clear();
				if (newInfo.Image == null) {
					((MapleExtractableInfo) newInfo).ParseImage();
				}

				currAddedInfo = newInfo;

				currAddedObj = newInfo.CreateInstance(Board.SelectedLayer, Board,
					X + currAddedInfo.Origin.X - newInfo.Image.Width / 2,
					Y + currAddedInfo.Origin.Y - newInfo.Image.Height / 2, 50, false);
				Board.BoardItems.Add(currAddedObj, false);

				BindItem(currAddedObj,
					new Xna.Point(newInfo.Origin.X - newInfo.Image.Width / 2,
						newInfo.Origin.Y - newInfo.Image.Height / 2));
				/*    if (currAddedInfo.Image.Width == 1 && currAddedInfo.Image.Height == 1)
				    {
				        //case occurs for things like life/mob objects, //tbd investigate how wzR2 handles
				        currAddedInfo.Image = global::HaCreator.Properties.Resources.placeholder;
				    }*/
				state = MouseState.StaticObjectAdding;
			}
		}

		public void SetRandomTilesMode(TileInfo[] tileList) {
			lock (Board.ParentControl) {
				Clear();
				tileRandomList = tileList;
				SetHeldInfo(tileRandomList[NextInt32(tileRandomList.Length)]);
				state = MouseState.RandomTiles;
			}
		}

		public void SetFootholdMode() {
			lock (Board.ParentControl) {
				Clear();
				state = MouseState.Footholds;
			}
		}

		public void SetRopeMode() {
			lock (Board.ParentControl) {
				Clear();
				state = MouseState.Ropes;
				CreateRope();
			}
		}

		public void SetChairMode() {
			lock (Board.ParentControl) {
				Clear();
				currAddedObj = new Chair(Board, X, Y);
				Board.BoardItems.Add(currAddedObj, false);
				BindItem(currAddedObj, new Xna.Point());
				state = MouseState.Chairs;
			}
		}

		public void SetTooltipMode() {
			lock (Board.ParentControl) {
				Clear();
				state = MouseState.Tooltip;
				CreateTooltip();
			}
		}

		public void SetClockMode() {
			lock (Board.ParentControl) {
				Clear();
				state = MouseState.Clock;
				CreateClock();
			}
		}

		#region Properties

		public bool IsDown {
			get => isDown;
			set {
				isDown = value;
				if (!isDown) {
					multiSelectOngoing = false;
					multiSelectStart = new Xna.Point();
					minimapBrowseOngoing = false;
					singleSelectStarting = false;
					singleSelectStart = new Xna.Point();
				}
			}
		}

		public bool MinimapBrowseOngoing {
			get => minimapBrowseOngoing;
			set => minimapBrowseOngoing = value;
		}

		public bool CameraPanning {
			get => cameraPanning;
			set => cameraPanning = value;
		}

		public Xna.Point CameraPanningStart {
			get => cameraPanningStart;
			set => cameraPanningStart = value;
		}

		public bool MultiSelectOngoing {
			get => multiSelectOngoing;
			set => multiSelectOngoing = value;
		}

		public Xna.Point MultiSelectStart {
			get => multiSelectStart;
			set => multiSelectStart = value;
		}

		public bool SingleSelectStarting {
			get => singleSelectStarting;
			set => singleSelectStarting = value;
		}

		public Xna.Point SingleSelectStart {
			get => singleSelectStart;
			set => singleSelectStart = value;
		}

		public MouseState State => state;

		#endregion

		#region Overrides

		protected override bool RemoveConnectedLines => false;

		public override MapleDrawableInfo BaseInfo => null;

		public override void Draw(SpriteBatch sprite,
			Xna.Color color, int xShift, int yShift) {
		}

		public override Bitmap Image => placeholder;

		public override Point Origin => origin;

		public override ItemTypes Type => ItemTypes.None;

		public override Xna.Color Color => Xna.Color.White;

		public override Xna.Color InactiveColor => Xna.Color.White;

		public override void BindItem(BoardItem item, Xna.Point distance) {
			lock (Board.ParentControl) {
				if (BoundItems.ContainsKey(item)) {
					return;
				}

				BoundItems[item] = distance;
				item.tempParent = item.Parent;
				item.Parent = this;
			}
		}

		public override void ReleaseItem(BoardItem item) {
			lock (Board.ParentControl) {
				if (BoundItems.ContainsKey(item)) {
					BoundItems.Remove(item);
					item.Parent = item.tempParent;
					item.tempParent = null;
				}
			}
		}

		public override void RemoveItem(List<UndoRedoAction> undoPipe) {
		}

		#endregion
	}
}