﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.Instance.Misc;
using HaCreator.MapEditor.Instance.Shapes;
using MapleLib.WzLib.WzStructure;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework;
using Point = Microsoft.Xna.Framework.Point;

namespace HaCreator.Collections {
	public class BoardItemsManager {
		public MapleList<BackgroundInstance> BackBackgrounds = new(ItemTypes.Backgrounds, true);

		public MapleList<LayeredItem> TileObjs = new(ItemTypes.None, true);
		public MapleList<MobInstance> Mobs = new(ItemTypes.Mobs, true);
		public MapleList<NpcInstance> NPCs = new(ItemTypes.NPCs, true);
		public MapleList<ReactorInstance> Reactors = new(ItemTypes.Reactors, true);
		public MapleList<PortalInstance> Portals = new(ItemTypes.Portals, true);

		public MapleList<BackgroundInstance> FrontBackgrounds = new(ItemTypes.Backgrounds, true);

		public MapleList<FootholdLine> FootholdLines = new(ItemTypes.Footholds, false);
		public MapleList<RopeLine> RopeLines = new(ItemTypes.Ropes, false);
		public MapleList<FootholdAnchor> FHAnchors = new(ItemTypes.Footholds, true);
		public MapleList<RopeAnchor> RopeAnchors = new(ItemTypes.Ropes, true);
		public MapleList<Chair> Chairs = new(ItemTypes.Chairs, true);
		public MapleList<ToolTipChar> CharacterToolTips = new(ItemTypes.ToolTips, true);
		public MapleList<ToolTipInstance> ToolTips = new(ItemTypes.ToolTips, true);
		public MapleList<ToolTipDot> ToolTipDots = new(ItemTypes.ToolTips, true);
		public MapleList<BoardItem> MiscItems = new(ItemTypes.Misc, true);
		public MapleList<MapleDot> SpecialDots = new(ItemTypes.Misc, true);

		public MapleList<MirrorFieldData> MirrorFieldDatas = new(ItemTypes.MirrorFieldData, true);

		/// <summary>
		/// Checks if a BaseDXDrawableItem is within any of the MirrorFieldData in the field.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="filter_objectForOverlay"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MirrorFieldData CheckObjectWithinMirrorFieldDataBoundary(int x, int y,
			MirrorFieldDataType filter_objectForOverlay) {
			if (MirrorFieldDatas.Count == 0) {
				return null;
			}

			var pos = new Point(x, y - 60);
			var reflectionBoundaryWithin = MirrorFieldDatas.FirstOrDefault(
				mirror => { return mirror.Rectangle.Contains(pos) && mirror.MirrorFieldDataType == filter_objectForOverlay; });

			if (reflectionBoundaryWithin != null) {
				return reflectionBoundaryWithin;
			}

			return null;
		}

		/// <summary>
		/// Map info
		/// </summary>
		public MapInfo MapInfo => board.MapInfo;

		public List<Rope> Ropes = new();
		public IMapleList[] AllItemLists;
		public BoardItemsCollection Items;
		public MapleLinesCollection Lines;

		private readonly Board board;

		public BoardItemsManager(Board board) {
			AllItemLists = new IMapleList[] {
				BackBackgrounds, TileObjs, Mobs, NPCs, Reactors, Portals, FrontBackgrounds, FootholdLines, RopeLines,
				FHAnchors,
				RopeAnchors, Chairs, CharacterToolTips, ToolTips, ToolTipDots, MiscItems, SpecialDots, MirrorFieldDatas
			};
			this.board = board;
			Items = new BoardItemsCollection(this, true);
			Lines = new MapleLinesCollection(this, false);
		}

		public void Clear() {
			foreach (var itemList in AllItemLists) {
				itemList.Clear();
			}
		}

		public void Remove(BoardItem item) {
			lock (board.ParentControl) {
				if (item is TileInstance || item is ObjectInstance) {
					TileObjs.Remove((LayeredItem) item);
				} else if (item is BackgroundInstance) {
					if (((BackgroundInstance) item).front) {
						FrontBackgrounds.Remove((BackgroundInstance) item);
					} else {
						BackBackgrounds.Remove((BackgroundInstance) item);
					}
				} else if (item.Type == ItemTypes.Misc) {
					if (item is VRDot || item is MinimapDot) {
						SpecialDots.Remove((MapleDot) item);
					} else {
						MiscItems.Remove(item);
					}
				} else {
					var itemType = item.GetType();
					foreach (var itemList in AllItemLists) {
						var listType = itemList.GetType().GetGenericArguments()[0];
						if (listType.FullName == itemType.FullName) {
							itemList.Remove(item);
							return;
						}
					}

					throw new Exception("unknown type at boarditems.remove");
				}
			}
		}

		public void Add(BoardItem item, bool sort) {
			lock (board.ParentControl) {
				if (item is TileInstance || item is ObjectInstance) {
					TileObjs.Add((LayeredItem) item);
					if (sort) {
						Sort();
					}
				} else if (item is BackgroundInstance instance) {
					if (instance.front) {
						FrontBackgrounds.Add(instance);
					} else {
						BackBackgrounds.Add(instance);
					}

					if (sort) {
						Sort();
					}
				} else if (item.Type == ItemTypes.Misc) {
					if (item is VRDot || item is MinimapDot) {
						SpecialDots.Add((MapleDot) item);
					} else {
						MiscItems.Add(item);
					}
				} else {
					var itemType = item.GetType();
					foreach (var itemList in AllItemLists) {
						var listType = itemList.GetType().GetGenericArguments()[0];
						if (listType.FullName == itemType.FullName) {
							itemList.Add(item);
							return;
						}
					}

					throw new Exception("unknown type at boarditems.add");
				}
			}
		}

		public void Sort() {
			SortLayers();
			SortBackBackgrounds();
			SortFrontBackgrounds();
		}

		private void SortLayers() {
			lock (board.ParentControl) {
				for (var i = 0; i < 2; i++) {
					TileObjs.Sort(
						delegate(LayeredItem a, LayeredItem b) {
							if (a.Layer.LayerNumber > b.Layer.LayerNumber) {
								return 1;
							}

							if (a.Layer.LayerNumber < b.Layer.LayerNumber) {
								return -1;
							}

							if (a is TileInstance && b is TileInstance) {
								var ai = (TileInfo) a.BaseInfo;
								var bi = (TileInfo) b.BaseInfo;
								if (ai.z > bi.z) {
									return 1;
								}

								if (ai.z < bi.z) {
									return -1;
								}

								if (a.Z > b.Z) {
									return 1;
								}

								if (a.Z < b.Z) {
									return -1;
								}

								return 0;
							}

							if (a is ObjectInstance && b is ObjectInstance) {
								if (a.Z > b.Z) {
									return 1;
								}

								if (a.Z < b.Z) {
									return -1;
								}

								return 0;
							}

							if (a is TileInstance && b is ObjectInstance) {
								return 1;
							}

							return -1;
						}
					);
				}
			}
		}

		public int Count {
			get {
				var total = 0;
				foreach (IList itemList in AllItemLists) {
					total += itemList.Count;
				}

				return total;
			}
		}

		public BoardItem this[int index] {
			get {
				if (index < 0) {
					throw new Exception("invalid index");
				}

				foreach (IList list in AllItemLists) {
					if (index < list.Count) {
						return (BoardItem) list[index];
					}

					index -= list.Count;
				}

				throw new Exception("invalid index");
			}
		}

		private void SortBackBackgrounds() {
			lock (board.ParentControl) {
				BackBackgrounds.Sort(
					delegate(BackgroundInstance a, BackgroundInstance b) {
						if (a.Z > b.Z) {
							return 1;
						}

						if (a.Z < b.Z) {
							return -1;
						}

						return 0;
					}
				);
			}
		}

		private void SortFrontBackgrounds() {
			lock (board.ParentControl) {
				FrontBackgrounds.Sort(
					delegate(BackgroundInstance a, BackgroundInstance b) {
						if (a.Z > b.Z) {
							return 1;
						}

						if (a.Z < b.Z) {
							return -1;
						}

						return 0;
					}
				);
			}
		}
	}
}