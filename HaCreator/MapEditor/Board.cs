/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading;
using HaCreator.Collections;
using HaCreator.MapEditor.Input;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.Instance.Shapes;
using HaCreator.MapEditor.UndoRedo;
using HaSharedLibrary.Util;
using MapleLib.WzLib.WzStructure;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace HaCreator.MapEditor {
	public class Board {
		private Point mapSize;

		private Rectangle minimapArea;

		//private Point maxMapSize;
		private Point centerPoint;
		private readonly BoardItemsManager boardItems;
		private readonly List<Layer> mapLayers = new List<Layer>();
		private readonly List<BoardItem> selected = new List<BoardItem>();
		private MultiBoard parent;
		private readonly Mouse mouse;
		private MapInfo mapInfo = new MapInfo();
		private bool bIsNewMapDesign = false; // determines if this board is a new map design or editing an existing map.
		private System.Drawing.Bitmap miniMap;
		private System.Drawing.Point miniMapPos;
		private Texture2D miniMapTexture;

		// App settings
		private int selectedLayerIndex = ApplicationSettings.lastDefaultLayer;
		private int selectedPlatform = 0;
		private bool selectedAllLayers = ApplicationSettings.lastAllLayers;
		private bool selectedAllPlats = true;
		private int _hScroll = 0;
		private int _vScroll = 0;
		private int _mag = 16;
		private readonly UndoRedoManager undoRedoMan;
		private ItemTypes visibleTypes;
		private ItemTypes editedTypes;
		private bool loading = false;
		private VRRectangle vrRect = null;
		private MinimapRectangle mmRect = null;
		private System.Windows.Controls.ContextMenu menu = null;
		private readonly SerializationManager serMan = null;
		private System.Windows.Controls.TabItem page = null;
		private bool dirty;
		private readonly int uid;

		private static int uidCounter = 0;

		public ItemTypes VisibleTypes {
			get => visibleTypes;
			set => visibleTypes = value;
		}

		public ItemTypes EditedTypes {
			get => editedTypes;
			set => editedTypes = value;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="mapSize"></param>
		/// <param name="centerPoint"></param>
		/// <param name="parent"></param>
		/// <param name="bIsNewMapDesign">Determines if this board is a new map design or editing an existing map.</param>
		/// <param name="menu"></param>
		/// <param name="visibleTypes"></param>
		/// <param name="editedTypes"></param>
		public Board(Point mapSize, Point centerPoint, MultiBoard parent, bool bIsNewMapDesign, System.Windows.Controls.ContextMenu menu,
			ItemTypes visibleTypes, ItemTypes editedTypes) {
			uid = Interlocked.Increment(ref uidCounter);
			MapSize = mapSize;
			this.centerPoint = centerPoint;
			this.parent = parent;
			this.bIsNewMapDesign = bIsNewMapDesign;
			this.visibleTypes = visibleTypes;
			this.editedTypes = editedTypes;
			this.menu = menu;

			boardItems = new BoardItemsManager(this);
			undoRedoMan = new UndoRedoManager(this);
			mouse = new Mouse(this);
			serMan = new SerializationManager(this);
		}

		public void RenderList(IMapleList list, SpriteBatch sprite, int xShift, int yShift, SelectionInfo sel) {
			if (list.ListType == ItemTypes.None) {
				foreach (BoardItem item in list) {
					if (parent.IsItemInRange(item.X, item.Y, item.Width, item.Height, xShift - item.Origin.X,
						    yShift - item.Origin.Y) && (sel.visibleTypes & item.Type) != 0) {
						item.Draw(sprite, item.GetColor(sel, item.Selected), xShift, yShift);
					}
				}
			} else if ((sel.visibleTypes & list.ListType) != 0) {
				if (list.IsItem) {
					foreach (BoardItem item in list) {
						if (parent.IsItemInRange(item.X, item.Y, item.Width, item.Height, xShift - item.Origin.X,
							    yShift - item.Origin.Y)) {
							item.Draw(sprite, item.GetColor(sel, item.Selected), xShift, yShift);
						}
					}
				} else {
					foreach (MapleLine line in list) {
						if (parent.IsItemInRange(Math.Min(line.FirstDot.X, line.SecondDot.X),
							    Math.Min(line.FirstDot.Y, line.SecondDot.Y),
							    Math.Abs(line.FirstDot.X - line.SecondDot.X),
							    Math.Abs(line.FirstDot.Y - line.SecondDot.Y), xShift, yShift)) {
							line.Draw(sprite, line.GetColor(sel), xShift, yShift);
						}
					}
				}
			}
		}

		public static System.Drawing.Bitmap ResizeImage(System.Drawing.Bitmap FullsizeImage, float coeff) {
			return (System.Drawing.Bitmap) FullsizeImage.GetThumbnailImage(
				(int) Math.Round(FullsizeImage.Width / coeff), (int) Math.Round(FullsizeImage.Height / coeff), null,
				IntPtr.Zero);
		}

		public static System.Drawing.Bitmap CropImage(System.Drawing.Bitmap img, System.Drawing.Rectangle selection) {
			var result = new System.Drawing.Bitmap(selection.Width, selection.Height);
			using (var g = System.Drawing.Graphics.FromImage(result)) {
				g.DrawImage(img, new System.Drawing.Rectangle(0, 0, selection.Width, selection.Height), selection,
					System.Drawing.GraphicsUnit.Pixel);
			}

			return result;
		}


		public bool RegenerateMinimap() {
			try {
				lock (parent) {
					if (MinimapRectangle == null) {
						MiniMap = null;
					} else {
						var bmp = new System.Drawing.Bitmap(mapSize.X, mapSize.Y);
						var processor = System.Drawing.Graphics.FromImage(bmp);
						foreach (var item in BoardItems.TileObjs) {
							var flip = false;
							if (item is IFlippable flippable) {
								flip = flippable.Flip;
							}

							var image = item.Image;
							if (flip) {
								// Clone since I assume this modifies the original image
								image = (Bitmap) image.Clone();
								image.RotateFlip(RotateFlipType.RotateNoneFlipX);
							}

							processor.DrawImage(image,
								new System.Drawing.Point(item.X + centerPoint.X - item.Origin.X,
									item.Y + centerPoint.Y - item.Origin.Y));
						}

						bmp = CropImage(bmp,
							new System.Drawing.Rectangle(MinimapRectangle.X + centerPoint.X,
								MinimapRectangle.Y + centerPoint.Y, MinimapRectangle.Width, MinimapRectangle.Height));
						MiniMap = ResizeImage(bmp, (float) _mag);
						MinimapPosition = new System.Drawing.Point(MinimapRectangle.X, MinimapRectangle.Y);
					}
				}

				return true;
			} catch {
				return false;
			}
		}

		public void RenderBoard(SpriteBatch sprite) {
			if (mapInfo == null) {
				return;
			}

			var xShift = centerPoint.X - hScroll;
			var yShift = centerPoint.Y - vScroll;
			var sel = GetUserSelectionInfo();

			// Render the object lists
			foreach (var list in boardItems.AllItemLists) RenderList(list, sprite, xShift, yShift, sel);

			// Render the user's selection square
			if (mouse.MultiSelectOngoing) {
				var selectionRect = InputHandler.CreateRectangle(
					new Point(MultiBoard.VirtualToPhysical(mouse.MultiSelectStart.X, centerPoint.X, hScroll, 0),
						MultiBoard.VirtualToPhysical(mouse.MultiSelectStart.Y, centerPoint.Y, vScroll, 0)),
					new Point(MultiBoard.VirtualToPhysical(mouse.X, centerPoint.X, hScroll, 0),
						MultiBoard.VirtualToPhysical(mouse.Y, centerPoint.Y, vScroll, 0)));
				parent.DrawRectangle(sprite, selectionRect, UserSettings.SelectSquare);
				selectionRect.X++;
				selectionRect.Y++;
				selectionRect.Width--;
				selectionRect.Height--;
				parent.FillRectangle(sprite, selectionRect, UserSettings.SelectSquareFill);
			}

			// Render VR if it exists
			if (VRRectangle != null && (sel.visibleTypes & VRRectangle.Type) != 0) {
				VRRectangle.Draw(sprite, xShift, yShift, sel);
			}

			// Render minimap rectangle
			if (MinimapRectangle != null && (sel.visibleTypes & MinimapRectangle.Type) != 0) {
				MinimapRectangle.Draw(sprite, xShift, yShift, sel);
			}

			// Render the minimap itself
			if (miniMap != null && UserSettings.useMiniMap) {
				var scale = parent.Scale;
				// Area for the image itself
				var minimapImageArea = new Rectangle((int) ((miniMapPos.X + (double) centerPoint.X) / _mag / scale),
					(int) ((miniMapPos.Y + (double) centerPoint.Y) / _mag / scale), (int) (miniMap.Width / scale), (int) (miniMap.Height / scale));

				// Render gray area
				var mapArea = new Rectangle(minimapArea.X, minimapArea.Y, (int) (minimapArea.Width / scale), (int) (minimapArea.Height / scale));
				parent.FillRectangle(sprite, mapArea, Color.Gray);
				// Render minimap
				if (miniMapTexture == null) {
					miniMapTexture = miniMap.ToTexture2D(parent.GraphicsDevice);
				}

				sprite.Draw(miniMapTexture, minimapImageArea, null, Color.White, 0, new Vector2(0, 0),
					SpriteEffects.None, 0.99999f);
				// Render current location on minimap
				parent.DrawRectangle(sprite,
					new Rectangle((int) (hScroll / (double) _mag / scale), (int) (vScroll / (double) _mag / scale),
						(int) (parent.CurrentDXWindowSize.Width / (double) _mag / scale),
						(int) (parent.CurrentDXWindowSize.Height / (double) _mag / scale)), Color.Blue);

				// Render minimap borders
				parent.DrawRectangle(sprite, minimapImageArea, Color.Black);
			}

			// Render center point if InfoMode on
			if (ApplicationSettings.InfoMode) {
				parent.FillRectangle(sprite,
					new Rectangle(MultiBoard.VirtualToPhysical(-5, centerPoint.X, hScroll, 0),
						MultiBoard.VirtualToPhysical(-5, centerPoint.Y, vScroll, 0), 10, 10), Color.DarkRed);
			}
		}

		public void Dispose() {
			lock (parent) {
				parent.Boards.Remove(this);
				boardItems.Clear();
				selected.Clear();
				mapLayers.Clear();
			}

			// This must be called when MultiBoard is unlocked, to prevent BackupManager deadlocking
			parent.OnBoardRemoved(this);
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		#region Properties

		public int UniqueID => uid;

		public bool Dirty {
			get => dirty;
			set => dirty = value;
		}

		public UndoRedoManager UndoRedoMan => undoRedoMan;

		public int mag {
			get => _mag;
			set {
				lock (parent) {
					_mag = value;
				}
			}
		}

		public MapInfo MapInfo {
			get => mapInfo;
			set {
				lock (parent) {
					mapInfo = value;
				}
			}
		}

		/// <summary>
		/// Determines if this board is a new map design or editing an existing map.
		/// </summary>
		public bool IsNewMapDesign => bIsNewMapDesign;

		public System.Drawing.Bitmap MiniMap {
			get => miniMap;
			set {
				lock (parent) {
					miniMap = value;
					miniMapTexture = null;
				}
			}
		}

		public System.Drawing.Point MinimapPosition {
			get => miniMapPos;
			set => miniMapPos = value;
		}

		public int hScroll {
			get => _hScroll;
			set {
				lock (parent) {
					_hScroll = value;
					parent.SetHScrollbarValue(_hScroll);
				}
			}
		}

		public Point CenterPoint {
			get => centerPoint;
			internal set => centerPoint = value;
		}

		public int vScroll {
			get => _vScroll;
			set {
				lock (parent) {
					_vScroll = value;
					parent.SetVScrollbarValue(_vScroll);
				}
			}
		}

		public MultiBoard ParentControl {
			get => parent;
			internal set => parent = value;
		}

		public Mouse Mouse => mouse;

		public Point MapSize {
			get => mapSize;
			set {
				mapSize = value;
				minimapArea = new Rectangle(0, 0, mapSize.X / _mag, mapSize.Y / _mag);
			}
		}

		public Rectangle MinimapArea => minimapArea;

		public VRRectangle VRRectangle {
			get => vrRect;
			set {
				vrRect = value;
				((System.Windows.Controls.MenuItem) menu.Items[1]).IsEnabled = value == null;
			}
		}

		public MinimapRectangle MinimapRectangle {
			get => mmRect;
			set {
				mmRect = value;
				((System.Windows.Controls.MenuItem) menu.Items[2]).IsEnabled = value == null;
				parent.OnMinimapStateChanged(this, mmRect != null);
			}
		}

		public BoardItemsManager BoardItems => boardItems;

		public List<BoardItem> SelectedItems => selected;

		/// <summary>
		/// Map layers
		/// </summary>
		public void CreateMapLayers() {
			for (var i = 0; i <= MapConstants.MaxMapLayers; i++) AddMapLayer(new Layer(this));
		}

		public void AddMapLayer(Layer layer) {
			lock (parent) {
				mapLayers.Add(layer);
			}
		}

		/// <summary>
		/// Gets the map layers
		/// </summary>
		public ReadOnlyCollection<Layer> Layers => mapLayers.AsReadOnly();

		public int SelectedLayerIndex {
			get => selectedLayerIndex;
			set {
				lock (parent) {
					selectedLayerIndex = value;
				}
			}
		}

		public bool SelectedAllLayers {
			get => selectedAllLayers;
			set => selectedAllLayers = value;
		}

		public System.Windows.Controls.ContextMenu Menu => menu;

		public Layer SelectedLayer => Layers[SelectedLayerIndex];

		public int SelectedPlatform {
			get => selectedPlatform;
			set => selectedPlatform = value;
		}

		public bool SelectedAllPlatforms {
			get => selectedAllPlats;
			set => selectedAllPlats = value;
		}

		public SelectionInfo GetUserSelectionInfo() {
			return new SelectionInfo(selectedAllLayers ? -1 : selectedLayerIndex,
				selectedAllPlats ? -1 : selectedPlatform, visibleTypes, editedTypes);
		}

		public bool Loading {
			get => loading;
			set => loading = value;
		}

		public SerializationManager SerializationManager => serMan;

		public System.Windows.Controls.TabItem TabPage {
			get => page;
			set => page = value;
		}

		#endregion
	}
}