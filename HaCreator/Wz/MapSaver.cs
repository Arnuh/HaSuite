﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HaCreator.MapEditor;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure.Data;
using MapleLib.WzLib.WzStructure;
using HaCreator.MapEditor.TilesDesign;
using HaCreator.MapEditor.Instance.Shapes;
using HaCreator.MapEditor.Instance.Misc;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Instance;
using HaCreator.Collections;
using HaCreator.MapSimulator;
using HaSharedLibrary.Render.DX;
using HaSharedLibrary.Wz;

namespace HaCreator.Wz {
	public class MapSaver {
		private Board board;
		private WzImage image;

		public MapSaver(Board board) {
			this.board = board;
		}

		private void CreateImage() {
			string name;
			switch (board.MapInfo.mapType) {
				case MapType.RegularMap:
					name = WzInfoTools.AddLeadingZeros(board.MapInfo.id.ToString(), 9);
					break;
				case MapType.MapLogin:
				case MapType.CashShopPreview:
					name = board.MapInfo.strMapName;
					break;
				default:
					throw new Exception("Unknown map type");
			}

			image = new WzImage(name + ".img") {
				Parsed = true
			};
		}

		private void InsertImage() {
			if (board.MapInfo.mapType == MapType.RegularMap) {
				var mapId = image.Name.Replace(".img", string.Empty);

				WzDirectory parent;
				if (board.IsNewMapDesign) {
					parent = (WzDirectory) WzInfoTools.FindMapDirectoryParent(mapId, Program.WzManager);
				} else {
					WzObject mapImage = WzInfoTools.FindMapImage(mapId, Program.WzManager);
					if (mapImage == null)
						throw new Exception("Could not find a suitable Map.wz to place the new map into.");

					parent = (WzDirectory) mapImage.Parent;
					mapImage.Remove();
				}

				parent.AddImage(image);

				Program.WzManager.SetWzFileUpdated(parent.GetTopMostWzDirectory().Name /* "map" */, image);
			} else {
				var mapDir = (WzDirectory) Program.WzManager["ui"];
				var mapImg = (WzImage) mapDir[image.Name];
				if (mapImg != null) mapImg.Remove();

				mapDir.AddImage(image);
				Program.WzManager.SetWzFileUpdated("ui", image);
			}
		}

		private void SaveMapInfo() {
			board.MapInfo.Save(image,
				board.VRRectangle == null
					? (System.Drawing.Rectangle?) null
					: new System.Drawing.Rectangle(board.VRRectangle.X, board.VRRectangle.Y, board.VRRectangle.Width,
						board.VRRectangle.Height));
			if (board.MapInfo.mapType == MapType.RegularMap) {
				var strMapImg = (WzImage) Program.WzManager.FindWzImageByName("string", "Map.img");
				if (strMapImg == null)
					throw new Exception("Map.img not found in string.wz");

				var strCatProp = (WzSubProperty) strMapImg[board.MapInfo.strCategoryName];
				if (strCatProp == null) {
					strCatProp = new WzSubProperty();
					strMapImg[board.MapInfo.strCategoryName] = strCatProp;
					Program.WzManager.SetWzFileUpdated("string", strMapImg);
				}

				var strMapProp = (WzSubProperty) strCatProp[board.MapInfo.id.ToString()];
				if (strMapProp == null) {
					strMapProp = new WzSubProperty();
					strCatProp[board.MapInfo.id.ToString()] = strMapProp;
					Program.WzManager.SetWzFileUpdated("string", strMapImg);
				}

				var strMapName = (WzStringProperty) strMapProp["mapName"];
				if (strMapName == null) {
					strMapName = new WzStringProperty();
					strMapProp["mapName"] = strMapName;
					Program.WzManager.SetWzFileUpdated("string", strMapImg);
				}

				var strStreetName = (WzStringProperty) strMapProp["streetName"];
				if (strStreetName == null) {
					strStreetName = new WzStringProperty();
					strMapProp["streetName"] = strStreetName;
					Program.WzManager.SetWzFileUpdated("string", strMapImg);
				}

				UpdateString(strMapName, board.MapInfo.strMapName, strMapImg);
				UpdateString(strStreetName, board.MapInfo.strStreetName, strMapImg);
			}
		}

		private void UpdateString(WzStringProperty strProp, string val, WzImage img) {
			if (strProp.Value != val) {
				strProp.Value = val;
				Program.WzManager.SetWzFileUpdated("string", img);
			}
		}

		private void SaveMiniMap() {
			if (board.MiniMap != null && board.MinimapRectangle != null) {
				var miniMap = new WzSubProperty();
				var canvas = new WzCanvasProperty {
					PngProperty = new WzPngProperty()
				};
				canvas.PngProperty.PixFormat = (int) WzPngProperty.WzPixelFormat.B4G4R4A4;
				canvas.PngProperty.SetImage(board.MiniMap);
				miniMap["canvas"] = canvas;
				miniMap["width"] = InfoTool.SetInt(board.MinimapRectangle.Width);
				miniMap["height"] = InfoTool.SetInt(board.MinimapRectangle.Height);
				miniMap["centerX"] = InfoTool.SetInt(-board.MinimapPosition.X);
				miniMap["centerY"] = InfoTool.SetInt(-board.MinimapPosition.Y);
				miniMap["mag"] = InfoTool.SetInt(4);
				image["miniMap"] = miniMap;
			}
		}

		public void SaveLayers() {
			for (var layer = 0; layer <= MapConstants.MaxMapLayers; layer++) {
				var layerProp = new WzSubProperty();
				var infoProp = new WzSubProperty();

				// Info
				var l = board.Layers[layer];
				if (l.tS != null) infoProp["tS"] = InfoTool.SetString(l.tS);

				layerProp["info"] = infoProp;

				// Organize items and save objects
				var tiles = new List<TileInstance>();
				var objParent = new WzSubProperty();
				var objIndex = 0;
				foreach (var item in l.Items)
					if (item is ObjectInstance instance) {
						var obj = new WzSubProperty();
						var objInst = instance;
						var objInfo = (ObjectInfo) objInst.BaseInfo;

						obj["x"] = InfoTool.SetInt(objInst.UnflippedX);
						obj["y"] = InfoTool.SetInt(objInst.Y);
						obj["z"] = InfoTool.SetInt(objInst.Z);
						obj["zM"] = InfoTool.SetInt(objInst.PlatformNumber);
						obj["oS"] = InfoTool.SetString(objInfo.oS);
						obj["l0"] = InfoTool.SetString(objInfo.l0);
						obj["l1"] = InfoTool.SetString(objInfo.l1);
						obj["l2"] = InfoTool.SetString(objInfo.l2);
						obj["name"] = InfoTool.SetOptionalString(objInst.Name);
						obj["r"] = InfoTool.SetOptionalBool(objInst.r);
						obj["hide"] = InfoTool.SetOptionalBool(objInst.hide);
						obj["reactor"] = InfoTool.SetOptionalBool(objInst.reactor);
						obj["flow"] = InfoTool.SetOptionalBool(objInst.flow);
						obj["rx"] = InfoTool.SetOptionalTranslatedInt(objInst.rx);
						obj["ry"] = InfoTool.SetOptionalTranslatedInt(objInst.ry);
						obj["cx"] = InfoTool.SetOptionalTranslatedInt(objInst.cx);
						obj["cy"] = InfoTool.SetOptionalTranslatedInt(objInst.cy);
						obj["tags"] = InfoTool.SetOptionalString(objInst.tags);
						if (objInst.QuestInfo != null) {
							var questParent = new WzSubProperty();
							foreach (var objQuest in objInst.QuestInfo)
								questParent[objQuest.questId.ToString()] = InfoTool.SetInt((int) objQuest.state);

							obj["quest"] = questParent;
						}

						obj["f"] = InfoTool.SetBool(objInst.Flip);

						objParent[objIndex.ToString()] = obj;
						objIndex++;
					} else if (item is TileInstance instance1) {
						tiles.Add(instance1);
					} else {
						throw new Exception("Unkown type in layered lists");
					}

				layerProp["obj"] = objParent;

				// Save tiles
				tiles.Sort((a, b) => a.Z.CompareTo(b.Z));
				var tileParent = new WzSubProperty();
				for (var j = 0; j < tiles.Count; j++) {
					var tileInst = tiles[j];
					var tileInfo = (TileInfo) tileInst.BaseInfo;
					var tile = new WzSubProperty();

					tile["x"] = InfoTool.SetInt(tileInst.X);
					tile["y"] = InfoTool.SetInt(tileInst.Y);
					tile["zM"] = InfoTool.SetInt(tileInst.PlatformNumber);
					tile["u"] = InfoTool.SetString(tileInfo.u);
					tile["no"] = InfoTool.SetInt(int.Parse(tileInfo.no));

					tileParent[j.ToString()] = tile;
				}

				layerProp["tile"] = tileParent;

				image[layer.ToString()] = layerProp;
			}
		}

		public void SaveRopes() {
			var ropeParent = new WzSubProperty();
			for (var i = 0; i < board.BoardItems.Ropes.Count; i++) {
				var ropeInst = board.BoardItems.Ropes[i];
				var rope = new WzSubProperty();

				rope["x"] = InfoTool.SetInt(ropeInst.FirstAnchor.X);
				rope["y1"] = InfoTool.SetInt(Math.Min(ropeInst.FirstAnchor.Y, ropeInst.SecondAnchor.Y));
				rope["y2"] = InfoTool.SetInt(Math.Max(ropeInst.FirstAnchor.Y, ropeInst.SecondAnchor.Y));
				rope["uf"] = InfoTool.SetBool(ropeInst.uf);
				rope["page"] = InfoTool.SetInt(ropeInst.LayerNumber);
				rope["l"] = InfoTool.SetBool(ropeInst.ladder);

				ropeParent[(i + 1).ToString()] = rope;
			}

			image["ladderRope"] = ropeParent;
		}

		public void SaveChairs() {
			if (board.BoardItems.Chairs.Count == 0) return;

			var chairParent = new WzSubProperty();
			for (var i = 0; i < board.BoardItems.Chairs.Count; i++) {
				var chairInst = board.BoardItems.Chairs[i];
				var chair = new WzVectorProperty();
				chair.X = new WzIntProperty("X", chairInst.X);
				chair.Y = new WzIntProperty("Y", chairInst.Y);
				chairParent[i.ToString()] = chair;
			}

			image["seat"] = chairParent;
		}

		public void SavePortals() {
			var portalInstanceSorted = new List<PortalInstance>(board.BoardItems.Portals);


			var portalParent = new WzSubProperty();
			for (var i = 0; i < board.BoardItems.Portals.Count; i++) {
				var portalInst = board.BoardItems.Portals[i];
				var portal = new WzSubProperty();

				portal["x"] = InfoTool.SetInt(portalInst.X);
				portal["y"] = InfoTool.SetInt(portalInst.Y);
				portal["pt"] = InfoTool.SetInt(Program.InfoManager.PortalIdByType[portalInst.pt]);
				portal["tm"] = InfoTool.SetInt(portalInst.tm);
				portal["tn"] = InfoTool.SetString(portalInst.tn);
				portal["pn"] = InfoTool.SetString(portalInst.pn);
				portal["image"] = InfoTool.SetOptionalString(portalInst.image);
				portal["script"] = InfoTool.SetOptionalString(portalInst.script);
				portal["verticalImpact"] = InfoTool.SetOptionalInt(portalInst.verticalImpact);
				portal["horizontalImpact"] = InfoTool.SetOptionalInt(portalInst.horizontalImpact);
				portal["hRange"] = InfoTool.SetOptionalInt(portalInst.hRange);
				portal["vRange"] = InfoTool.SetOptionalInt(portalInst.vRange);
				portal["delay"] = InfoTool.SetOptionalInt(portalInst.delay);
				portal["hideTooltip"] = InfoTool.SetOptionalBool(portalInst.hideTooltip);
				portal["onlyOnce"] = InfoTool.SetOptionalBool(portalInst.onlyOnce);

				portalParent[i.ToString()] = portal;
			}

			image["portal"] = portalParent;
		}

		public void SaveReactors() {
			var reactorParent = new WzSubProperty();
			for (var i = 0; i < board.BoardItems.Reactors.Count; i++) {
				var reactorInst = board.BoardItems.Reactors[i];
				var reactor = new WzSubProperty();

				reactor["x"] = InfoTool.SetInt(reactorInst.UnflippedX);
				reactor["y"] = InfoTool.SetInt(reactorInst.Y);
				reactor["reactorTime"] = InfoTool.SetInt(reactorInst.ReactorTime);
				reactor["name"] = InfoTool.SetOptionalString(reactorInst.Name);
				reactor["id"] = InfoTool.SetString(((ReactorInfo) reactorInst.BaseInfo).ID);
				reactor["f"] = InfoTool.SetBool(reactorInst.Flip);

				reactorParent[i.ToString()] = reactor;
			}

			image["reactor"] = reactorParent;
		}

		public void SaveTooltips() {
			if (board.BoardItems.ToolTips.Count == 0) return;

			var retainTooltipStrings = true;
			var tooltipParent = new WzSubProperty();

			WzImage strTooltipImg = null;

			// Find the string.wz file
			var stringWzDirs = Program.WzManager.GetWzDirectoriesFromBase("string");
			foreach (var stringWzDir in stringWzDirs) {
				strTooltipImg = (WzImage) stringWzDir?["ToolTipHelp.img"];
				if (strTooltipImg != null)
					break; // found
			}

			if (strTooltipImg == null)
				throw new Exception("Unable to find ToolTipHelp.img in String.wz");

			var strTooltipCat = (WzSubProperty) strTooltipImg["Mapobject"];
			var strTooltipParent = (WzSubProperty) strTooltipCat[board.MapInfo.id.ToString()];
			if (strTooltipParent == null) {
				strTooltipParent = new WzSubProperty();
				strTooltipCat[board.MapInfo.id.ToString()] = strTooltipParent;
				Program.WzManager.SetWzFileUpdated("string", strTooltipImg);
				retainTooltipStrings = false;
			}

			var caughtNumbers = new HashSet<int>();

			// Check if the tooltips' original numbers can still be used
			if (retainTooltipStrings)
				for (var i = 0; i < board.BoardItems.ToolTips.Count; i++)
					if (board.BoardItems.ToolTips[i].OriginalNumber == -1) {
						retainTooltipStrings = false;
						break;
					}

			// If they do not, we need to update string.wz and rebuild the string tooltip props
			if (!retainTooltipStrings) {
				Program.WzManager.SetWzFileUpdated("string", strTooltipImg);
				strTooltipParent.ClearProperties();
			}

			for (var i = 0; i < board.BoardItems.ToolTips.Count; i++) {
				var ttInst = board.BoardItems.ToolTips[i];
				var tooltipPropStr = retainTooltipStrings ? ttInst.OriginalNumber.ToString() : i.ToString();
				tooltipParent[tooltipPropStr] = PackRectangle(ttInst);
				if (ttInst.CharacterToolTip != null)
					tooltipParent[tooltipPropStr + "char"] = PackRectangle(ttInst.CharacterToolTip);

				if (retainTooltipStrings) {
					// This prop must exist if we are retaining, otherwise the map would not load
					var strTooltipProp = (WzSubProperty) strTooltipParent[tooltipPropStr];

					if (ttInst.Title != null) {
						var titleProp = (WzStringProperty) strTooltipProp["Title"];
						if (titleProp == null) {
							titleProp = new WzStringProperty();
							Program.WzManager.SetWzFileUpdated("string", strTooltipImg);
						}

						UpdateString(titleProp, ttInst.Title, strTooltipImg);
					}

					if (ttInst.Desc != null) {
						var descProp = (WzStringProperty) strTooltipProp["Desc"];
						if (descProp == null) {
							descProp = new WzStringProperty();
							Program.WzManager.SetWzFileUpdated("string", strTooltipImg);
						}

						UpdateString(descProp, ttInst.Desc, strTooltipImg);
					}
				} else {
					var strTooltipProp = new WzSubProperty();
					strTooltipProp["Title"] = InfoTool.SetOptionalString(ttInst.Title);
					strTooltipProp["Desc"] = InfoTool.SetOptionalString(ttInst.Desc);
					strTooltipParent[tooltipPropStr] = strTooltipProp;
				}
			}

			image["ToolTip"] = tooltipParent;
		}

		private static WzSubProperty PackRectangle(MapleRectangle rect) {
			var prop = new WzSubProperty();
			prop["x1"] = InfoTool.SetInt(rect.Left);
			prop["x2"] = InfoTool.SetInt(rect.Right);
			prop["y1"] = InfoTool.SetInt(rect.Top);
			prop["y2"] = InfoTool.SetInt(rect.Bottom);
			return prop;
		}

		public void SaveBackgrounds() {
			var bgParent = new WzSubProperty();
			var backCount = board.BoardItems.BackBackgrounds.Count;
			var frontCount = board.BoardItems.FrontBackgrounds.Count;
			for (var i = 0; i < backCount + frontCount; i++) {
				var bgInst = i < backCount
					? board.BoardItems.BackBackgrounds[i]
					: board.BoardItems.FrontBackgrounds[i - backCount];
				var bgInfo = (BackgroundInfo) bgInst.BaseInfo;
				var bgProp = new WzSubProperty();
				bgProp["x"] = InfoTool.SetInt(bgInst.UnflippedX);
				bgProp["y"] = InfoTool.SetInt(bgInst.BaseY);
				bgProp["rx"] = InfoTool.SetInt(bgInst.rx);
				bgProp["ry"] = InfoTool.SetInt(bgInst.ry);
				bgProp["cx"] = InfoTool.SetInt(bgInst.cx);
				bgProp["cy"] = InfoTool.SetInt(bgInst.cy);
				bgProp["a"] = InfoTool.SetInt(bgInst.a);
				bgProp["type"] = InfoTool.SetInt((int) bgInst.type);
				bgProp["front"] = InfoTool.SetOptionalBool(bgInst.front);
				if (bgInst.screenMode != (int) RenderResolution.Res_All) // 0
					bgProp["screenMode"] = InfoTool.SetInt(bgInst.screenMode);

				if (bgInst.SpineAni != null) // dont put anything if null
					bgProp["spineAni"] = InfoTool.SetOptionalString(bgInst.SpineAni); // dont put anything if null
				if (bgInst.SpineRandomStart) // dont put anything if false
					bgProp["spineRandomStart"] =
						InfoTool.SetOptionalBool(bgInst.SpineRandomStart); // dont put anything if false

				bgProp["f"] = InfoTool.SetOptionalBool(bgInst.Flip);
				bgProp["bS"] = InfoTool.SetString(bgInfo.bS);
				bgProp["ani"] = InfoTool.SetBool(bgInfo.Type == BackgroundInfoType.Animation);
				bgProp["no"] = InfoTool.SetInt(int.Parse(bgInfo.no));
				bgParent[i.ToString()] = bgProp;
			}

			image["back"] = bgParent;
		}

		private void SavePlatform(int layer, int zM, WzSubProperty prop) {
			foreach (var line in board.BoardItems.FootholdLines) {
				// Save all footholds in the platform (same layer and zM)
				if (line.LayerNumber != layer || line.PlatformNumber != zM) continue;

				var orientation = GetFootholdOrientation(line);
				var prev = GetFootholdPrevNext(line, orientation, FootholdDirection.Prev);
				var next = GetFootholdPrevNext(line, orientation, FootholdDirection.Next);
				var anchor1 = (FootholdAnchor) (orientation == FootholdOrientation.PrevFirstNextSecond
					? line.FirstDot
					: line.SecondDot);
				var anchor2 = (FootholdAnchor) (orientation == FootholdOrientation.PrevFirstNextSecond
					? line.SecondDot
					: line.FirstDot);

				var fhProp = new WzSubProperty();
				fhProp["x1"] = InfoTool.SetInt(anchor1.X);
				fhProp["y1"] = InfoTool.SetInt(anchor1.Y);
				fhProp["x2"] = InfoTool.SetInt(anchor2.X);
				fhProp["y2"] = InfoTool.SetInt(anchor2.Y);
				fhProp["prev"] = InfoTool.SetInt(prev);
				fhProp["next"] = InfoTool.SetInt(next);
				fhProp["cantThrough"] = InfoTool.SetOptionalBool(line.CantThrough);
				fhProp["forbidFallDown"] = InfoTool.SetOptionalBool(line.ForbidFallDown);
				fhProp["piece"] = InfoTool.SetOptionalInt(line.Piece);
				fhProp["force"] = InfoTool.SetOptionalInt(line.Force);
				prop[line.num.ToString()] = fhProp;

				line.saved = true;
			}
		}

		private FootholdOrientation GetFootholdOrientation(FootholdLine line) {
			FootholdOrientation result;
			if (TryGetSimpleFootholdOrientation(line, out result)) {
				return result;
			} else {
				// Vertical foothold, search for near nonvertical foothold as orientation reference

				// Obtain vertical orientation of the foothold
				FootholdAnchor top, bottom;
				if (line.FirstDot.Y < line.SecondDot.Y) {
					top = (FootholdAnchor) line.FirstDot;
					bottom = (FootholdAnchor) line.SecondDot;
				} else if (line.FirstDot.Y > line.SecondDot.Y) {
					bottom = (FootholdAnchor) line.FirstDot;
					top = (FootholdAnchor) line.SecondDot;
				} else {
					throw new Exception("Zero length foothold in saving");
				}

				// For starters, we search the footholds linking downwards from us.
				// This is because we are looking for the conventional foothold U scheme:
				//
				// |     |
				// |     |
				// |_ _ _|
				//
				// or the Z/S schemes:
				//_ _ _                _ _ _
				//     |              |
				//     |              |
				//     |_ _ _ or _ _ _|
				var referenceEnumerator = new FootholdEnumerator(line, top);
				foreach (var reference in referenceEnumerator)
					if (!reference.IsWall)
						// We found a suiting foothold reference, find what is our orientation
						return GetVerticalFootholdOrientationByReference(line, top, reference,
							referenceEnumerator.CurrentAnchor);

				// If downard-search failed, search upwards, to resolve the n scheme:
				//  _ _ _
				// |     |
				// |     |
				// |     |
				//
				// Note that the order of searches is important; we MUST search downwards before upwards, to resolve schemes such as the tower scheme:
				//
				// |           |
				// |           |
				// |_ _     _ _|
				// |           |
				// |           |
				// |_ _ _ _ _ _|
				//
				// If we searched upwards-first, the footholds between the two tower "floors" would be treated as n scheme footholds, while they should be U schemed.

				referenceEnumerator = new FootholdEnumerator(line, bottom);
				foreach (var reference in referenceEnumerator)
					if (!reference.IsWall)
						// We found a suiting foothold reference, find what is our orientation
						return GetVerticalFootholdOrientationByReference(line, bottom, reference,
							referenceEnumerator.CurrentAnchor);

				// If all else failed, we are dealing with a pure-wall foothold platform (i.e. a foothold graph consisting only of vertical footholds)
				// In this case, we arbitrarily select the Normal orientation, since there's no more actual logic we can perform to know what is the correct orientation.
				return FootholdOrientation.PrevFirstNextSecond;
			}
		}

		private FootholdOrientation GetNonverticalFootholdOrientation(FootholdLine line) {
			if (line.FirstDot.X < line.SecondDot.X)
				// Normal foothold orientation
				return FootholdOrientation.PrevFirstNextSecond;
			else // (line.FirstDot.X > line.SecondDot.X)
				// Inverted foothold orientation
				return FootholdOrientation.NextFirstPrevSecond;
		}

		private bool TryGetSimpleFootholdOrientation(FootholdLine line, out FootholdOrientation result) {
			if (line.prevOverride != null && line.FirstDot.connectedLines.Contains(line.prevOverride)) {
				result = FootholdOrientation.PrevFirstNextSecond;
				return true;
			} else if (line.prevOverride != null && line.SecondDot.connectedLines.Contains(line.prevOverride)) {
				result = FootholdOrientation.NextFirstPrevSecond;
				return true;
			} else if (line.nextOverride != null && line.FirstDot.connectedLines.Contains(line.nextOverride)) {
				result = FootholdOrientation.NextFirstPrevSecond;
				return true;
			} else if (line.nextOverride != null && line.SecondDot.connectedLines.Contains(line.nextOverride)) {
				result = FootholdOrientation.PrevFirstNextSecond;
				return true;
			} else if (!line.IsWall) {
				result = GetNonverticalFootholdOrientation(line);
				return true;
			} else {
				// Result doesn't really matter here since we're returning false
				result = FootholdOrientation.PrevFirstNextSecond;
				return false;
			}
		}

		private FootholdOrientation GetVerticalFootholdOrientationByReference(FootholdLine line, FootholdAnchor anchor,
			FootholdLine reference, FootholdAnchor referenceAnchor) {
			var referenceOrientation = GetNonverticalFootholdOrientation(reference);
			var leadingAnchorIsFirst = referenceAnchor == reference.FirstDot;
			var firstIsPrev = referenceOrientation == FootholdOrientation.PrevFirstNextSecond;
			var startAnchorIsFirst = anchor == line.FirstDot;

			// LAIF | FIP | RESULT (leadingAnchorIsPrev "LAIP")
			//  1   |  1  |   1
			//  1   |  0  |   0
			//  0   |  1  |   0
			//  0   |  0  |   1

			// LAIP | SAIF | RESULT (Orientation: 0 normal, 1 inverted)
			//  1   |  1   |   0
			//  1   |  0   |   1
			//  0   |  1   |   1
			//  0   |  0   |   0

			return !(leadingAnchorIsFirst ^ firstIsPrev) ^ startAnchorIsFirst
				? FootholdOrientation.NextFirstPrevSecond
				: FootholdOrientation.PrevFirstNextSecond;
		}

		private int GetFootholdPrevNext(FootholdLine line, FootholdOrientation orientation, FootholdDirection dir) {
			var overrideLine = dir == FootholdDirection.Prev ? line.prevOverride : line.nextOverride;
			if (overrideLine != null && (line.FirstDot.connectedLines.Contains(overrideLine) ||
			                             line.SecondDot.connectedLines.Contains(overrideLine))) {
				return overrideLine.num;
			} else {
				var anchor =
					(FootholdAnchor) ((orientation == FootholdOrientation.PrevFirstNextSecond) ^
					                  (dir == FootholdDirection.Next)
						? line.FirstDot
						: line.SecondDot);
				if (anchor.connectedLines.Count < 2)
					return 0;
				else
					return anchor.GetOtherLine(line).num;
			}
		}

		public void SaveFootholds() {
			var fhParent = new WzSubProperty();
			board.BoardItems.FootholdLines.ForEach(x => x.saved = false);
			board.BoardItems.FootholdLines.Sort(FootholdLine.FHSorter);
			var fhIndex = 1;
			foreach (var line in board.BoardItems.FootholdLines) line.num = fhIndex++;

			for (var layer = 0; layer <= 7; layer++) {
				var fhLayerProp = new WzSubProperty();
				foreach (var fhInst in board.BoardItems.FootholdLines) {
					// Search only footholds in our layer, that weren't already saved
					if (fhInst.LayerNumber != layer || fhInst.saved) continue;

					var zM = fhInst.PlatformNumber;
					var fhPlatProp = new WzSubProperty();
					SavePlatform(layer, zM, fhPlatProp);
					fhLayerProp[zM.ToString()] = fhPlatProp;
				}

				if (fhLayerProp.WzProperties.Count > 0) fhParent[layer.ToString()] = fhLayerProp;
			}

			image["foothold"] = fhParent;
		}

		public void SaveLife() {
			var lifeParent = new WzSubProperty();
			var mobCount = board.BoardItems.Mobs.Count;
			var npcCount = board.BoardItems.NPCs.Count;
			for (var i = 0; i < mobCount + npcCount; i++) {
				var mob = i < mobCount;
				var lifeInst = mob
					? (LifeInstance) board.BoardItems.Mobs[i]
					: (LifeInstance) board.BoardItems.NPCs[i - mobCount];
				var lifeProp = new WzSubProperty();

				lifeProp["id"] =
					InfoTool.SetString(mob ? ((MobInfo) lifeInst.BaseInfo).ID : ((NpcInfo) lifeInst.BaseInfo).ID);
				lifeProp["x"] = InfoTool.SetInt(lifeInst.UnflippedX);
				lifeProp["y"] = InfoTool.SetInt(lifeInst.Y - lifeInst.yShift);
				lifeProp["cy"] = InfoTool.SetInt(lifeInst.Y);
				lifeProp["mobTime"] = InfoTool.SetOptionalInt(lifeInst.MobTime);
				lifeProp["info"] = InfoTool.SetOptionalInt(lifeInst.Info);
				lifeProp["team"] = InfoTool.SetOptionalInt(lifeInst.Team);
				lifeProp["rx0"] = InfoTool.SetInt(lifeInst.X - lifeInst.rx0Shift);
				lifeProp["rx1"] = InfoTool.SetInt(lifeInst.X + lifeInst.rx1Shift);
				lifeProp["f"] = InfoTool.SetOptionalBool(lifeInst.Flip);
				lifeProp["hide"] = InfoTool.SetOptionalBool(lifeInst.Hide);
				lifeProp["type"] = InfoTool.SetString(mob ? "m" : "n");
				lifeProp["limitedname"] = InfoTool.SetOptionalString(lifeInst.LimitedName);
				lifeProp["fh"] = InfoTool.SetInt(GetFootholdBelow(lifeInst.X, lifeInst.Y));
				lifeParent[i.ToString()] = lifeProp;
			}

			image["life"] = lifeParent;
		}

		public void SaveMisc() {
			var areaParent = new WzSubProperty();
			var buffParent = new WzSubProperty();
			var swimParent = new WzSubProperty();

			foreach (var item in board.BoardItems.MiscItems)
				if (item is Clock) {
					var clock = (Clock) item;
					var clockProp = new WzSubProperty();
					clockProp["x"] = InfoTool.SetInt(item.Left);
					clockProp["y"] = InfoTool.SetInt(item.Top);
					clockProp["width"] = InfoTool.SetInt(item.Width);
					clockProp["height"] = InfoTool.SetInt(item.Height);
					image["clock"] = clockProp;
				} else if (item is ShipObject) {
					var ship = (ShipObject) item;
					var shipInfo = (ObjectInfo) ship.BaseInfo;
					var shipProp = new WzSubProperty();
					shipProp["shipObj"] = InfoTool.SetString("Map/Obj/" + shipInfo.oS + ".img/" + shipInfo.l0 + "/" +
					                                         shipInfo.l1 + "/" + shipInfo.l2);
					shipProp["x"] = InfoTool.SetInt(ship.UnflippedX);
					shipProp["y"] = InfoTool.SetInt(ship.Y);
					shipProp["z"] = InfoTool.SetOptionalInt(ship.zValue);
					shipProp["x0"] = InfoTool.SetOptionalInt(ship.X0);
					shipProp["tMove"] = InfoTool.SetInt(ship.TimeMove);
					shipProp["shipKind"] = InfoTool.SetInt(ship.ShipKind);
					shipProp["f"] = InfoTool.SetBool(ship.Flip);
					image["shipObj"] = shipProp;
				} else if (item is Area) {
					var area = (Area) item;
					areaParent[area.Identifier] = PackRectangle(area);
				} else if (item is Healer) {
					var healer = (Healer) item;
					var healerInfo = (ObjectInfo) healer.BaseInfo;
					var healerProp = new WzSubProperty();
					healerProp["healer"] = InfoTool.SetString("Map/Obj/" + healerInfo.oS + ".img/" + healerInfo.l0 +
					                                          "/" + healerInfo.l1 + "/" + healerInfo.l2);
					healerProp["x"] = InfoTool.SetInt(healer.X);
					healerProp["yMin"] = InfoTool.SetInt(healer.yMin);
					healerProp["yMax"] = InfoTool.SetInt(healer.yMax);
					healerProp["healMin"] = InfoTool.SetInt(healer.healMin);
					healerProp["healMax"] = InfoTool.SetInt(healer.healMax);
					healerProp["fall"] = InfoTool.SetInt(healer.fall);
					healerProp["rise"] = InfoTool.SetInt(healer.rise);
					image["healer"] = healerProp;
				} else if (item is Pulley) {
					var pulley = (Pulley) item;
					var pulleyInfo = (ObjectInfo) pulley.BaseInfo;
					var pulleyProp = new WzSubProperty();
					pulleyProp["pulley"] = InfoTool.SetString("Map/Obj/" + pulleyInfo.oS + ".img/" + pulleyInfo.l0 +
					                                          "/" + pulleyInfo.l1 + "/" + pulleyInfo.l2);
					pulleyProp["x"] = InfoTool.SetInt(pulley.X);
					pulleyProp["y"] = InfoTool.SetInt(pulley.Y);
					image["pulley"] = pulleyProp;
				} else if (item is BuffZone) {
					var buff = (BuffZone) item;
					var buffProp = PackRectangle(buff);
					buffProp["ItemID"] = InfoTool.SetInt(buff.ItemID);
					buffProp["Interval"] = InfoTool.SetInt(buff.Interval);
					buffProp["Duration"] = InfoTool.SetInt(buff.Duration);
					buffParent[buff.ZoneName] = buffProp;
				} else if (item is SwimArea) {
					var swim = (SwimArea) item;
					swimParent[swim.Identifier] = PackRectangle(swim);
				}

			if (areaParent.WzProperties.Count > 0) image["area"] = areaParent;

			if (buffParent.WzProperties.Count > 0) image["BuffZone"] = buffParent;

			if (swimParent.WzProperties.Count > 0) image["swimArea"] = swimParent;
		}

		public void SaveMirrorFieldData() {
			if (board.BoardItems.MirrorFieldDatas.Count == 0)
				return;

			var mirrorFieldDataParent = new WzSubProperty();

			var i = 0;
			foreach (MirrorFieldDataType dataType in
			         Enum.GetValues(typeof(MirrorFieldDataType))) // initial holder data, only run once
			{
				if (dataType == MirrorFieldDataType.NULL || dataType == MirrorFieldDataType.info)
					continue;

				var holderProp = new WzSubProperty(); // <imgdir name="MirrorFieldData"><imgdir name="0">
				holderProp.Name = i.ToString(); // "0"

				// Subproperty of holderProp
				var
					InfoProp =
						new WzSubProperty(); // <imgdir name="MirrorFieldData"><imgdir name="0"><imgdir name="info">
				InfoProp.Name = "info"; // unused for now
				holderProp.AddProperty(InfoProp);

				var
					dataTypeProp =
						new WzSubProperty(); // <imgdir name="MirrorFieldData"><imgdir name="0"><imgdir name="mob">
				dataTypeProp.Name = dataType.ToString(); // unused for now
				holderProp.AddProperty(dataTypeProp);

				// add to parent
				mirrorFieldDataParent.AddProperty(holderProp);

				i++;
			}

			// dumpppp
			foreach (BoardItem item in board.BoardItems.MirrorFieldDatas)
				if (item is MirrorFieldData) {
					var mirrorFieldData = (MirrorFieldData) item;
					var forTargetObject =
						mirrorFieldData.MirrorFieldDataType
							.ToString(); // <imgdir name="MirrorFieldData"><imgdir name="0">< imgdir name="info"/><imgdir name="mob" >

					var containsWzSubPropertyForTargetObj =
						mirrorFieldDataParent.WzProperties.FirstOrDefault(wzImg => {
							return wzImg.WzProperties.FirstOrDefault(x => x.Name == forTargetObject) !=
							       null; // mob, user
						});

					if (containsWzSubPropertyForTargetObj != null) {
						var targetObjectWzProperty = containsWzSubPropertyForTargetObj[forTargetObject];

						var itemProp = new WzSubProperty(); // <imgdir name="0">
						itemProp.Name = targetObjectWzProperty.WzProperties.Count.ToString(); // "0"

						var rect = mirrorFieldData.Rectangle;

						/*
						    int width = rb.X.Value - lt.X.Value;
						    int height = rb.Y.Value - lt.Y.Value;
						    Rectangle rectangle = new Rectangle(
						        lt.X.Value - offset.X.Value,
						        lt.Y.Value - offset.Y.Value,
						        width,
						        height);*/
						InfoTool.SetLtRbRectangle(itemProp,
							new System.Drawing.Rectangle(rect.X, rect.Y, rect.Width,
								rect.Height) // convert Microsoft.Xna.Framework.Rectangle to System.Drawing.Rectangle
						);

						itemProp["offset"] = InfoTool.SetVector(mirrorFieldData.Offset.X, mirrorFieldData.Offset.Y);
						itemProp["gradient"] = InfoTool.SetInt(mirrorFieldData.ReflectionInfo.Gradient);
						itemProp["alpha"] = InfoTool.SetInt(mirrorFieldData.ReflectionInfo.Alpha);
						itemProp["objectForOverlay"] =
							InfoTool.SetString(mirrorFieldData.ReflectionInfo.ObjectForOverlay);
						itemProp["reflection"] = InfoTool.SetBool(mirrorFieldData.ReflectionInfo.Reflection);
						itemProp["alphaTest"] = InfoTool.SetOptionalBool(mirrorFieldData.ReflectionInfo.AlphaTest);

						targetObjectWzProperty.WzProperties.Add(itemProp);
					} else {
						throw new Exception("Error saving mirror field data. Missing MirrorFieldDataType of " +
						                    forTargetObject);
					}
				}

			if (mirrorFieldDataParent.WzProperties.Count > 0) image["MirrorFieldData"] = mirrorFieldDataParent;
		}

		/// <summary>
		/// Saves the additional unsupported properties read from the map image.
		/// </summary>
		private void SaveAdditionals() {
			foreach (var prop in board.MapInfo.additionalNonInfoProps) image.AddProperty(prop);
		}

		public void SaveMapImage() {
			CreateImage();
			SaveMapInfo();
			SaveMiniMap();
			SaveLayers();
			SaveRopes();
			SaveChairs();
			SavePortals();
			SaveReactors();
			SaveTooltips();
			SaveBackgrounds();
			SaveFootholds();
			SaveLife();
			SaveMisc();
			SaveMirrorFieldData();
			SaveAdditionals();
			InsertImage();
		}

		public WzImage MapImage => image;

		private int GetFootholdBelow(int x, int y) {
			var bestDistance = double.MaxValue;
			var bestFoothold = -1;
			foreach (var fh in board.BoardItems.FootholdLines)
				if (Math.Min(fh.FirstDot.X, fh.SecondDot.X) <= x && Math.Max(fh.FirstDot.X, fh.SecondDot.X) >= x) {
					var fhY = fh.CalculateY(x);
					if (fhY >= y && fhY - y < bestDistance) {
						bestDistance = fhY - y;
						bestFoothold = fh.num;
						if (bestDistance == 0)
							// Not going to find anything better than 0
							return bestFoothold;
					}
				}

			if (bestFoothold == -1)
				// 0 stands in the game for flying or nonexistant foothold; I do not know what are the results of putting an NPC there,
				// however, if the user puts an NPC with no floor under it he should expect weird things to happen.
				return 0;

			return bestFoothold;
		}


		private FootholdAnchor FindOptimalContinuationAnchor(int y, int x0, int x1, int layer) {
			FootholdAnchor result = null;
			var distance = int.MaxValue;
			foreach (var anchor in board.BoardItems.FHAnchors) {
				// Find an anchor on the same layer, with 1 connected line, in the X range of our target line, whose line is not vertical
				if (anchor.LayerNumber != layer || anchor.connectedLines.Count != 1 || anchor.X < x0 || anchor.X > x1 ||
				    anchor.connectedLines[0].FirstDot.X == anchor.connectedLines[0].SecondDot.X)
					continue;

				var d = Math.Abs(anchor.Y - y);
				if (d < distance) {
					distance = d;
					result = anchor;
				}

				if (distance == 0)
					// Not going to find anything better
					return result;
			}

			return distance < 100 ? result : null;
		}

		public void ActualizeFootholds() {
			board.BoardItems.FHAnchors.Sort(new Comparison<FootholdAnchor>(FootholdAnchor.FHAnchorSorter));

			// Merge foothold anchors
			// This sorts out all foothold inconsistencies in all non-edU tiles
			for (var i = 0; i < board.BoardItems.FHAnchors.Count - 1; i++) {
				var a = board.BoardItems.FHAnchors[i];
				var b = board.BoardItems.FHAnchors[i + 1];
				if (a.X == b.X && a.Y == b.Y && a.LayerNumber == b.LayerNumber && (a.user || b.user)) {
					if (a.user != b.user) a.user = false;

					FootholdAnchor.MergeAnchors(a, b); // Transfer lines from b to a
					b.RemoveItem(null); // Remove b
					i--; // Fix index after we removed b
					// Note: We are unlinking b from its parent. If b's parent is an edU tile, this will cause the edU to be irregular
					// and thus it will not get fixed in the next step. To counter this, FHAnchorSorter makes sure edU-children always come first.
				}
			}

			// Organize edU tiles
			foreach (var li in board.BoardItems.TileObjs) {
				if (!(li is TileInstance)) continue;

				var tileInst = (TileInstance) li;
				var tileInfo = (TileInfo) li.BaseInfo;
				// Ensure that the tile is an edU, that it was created by the user in this session, and that it doesnt have some messed up foothold structure we can't deal with
				if (tileInfo.u == "edU" && tileInst.BoundItemsList.Count >= 4) {
					var nitems = tileInst.BoundItemsList.Count;
					if (tileInst.BoundItemsList[0].Y != tileInst.BoundItemsList[nitems - 1].Y ||
					    tileInst.BoundItemsList[0].X != tileInst.BoundItemsList[1].X ||
					    tileInst.BoundItemsList[nitems - 1].X != tileInst.BoundItemsList[nitems - 2].X)
						continue;

					// Only work with snapped edU's
					if (tileInst.FindSnappableTiles(0,
						    x => ((TileInfo) x.BaseInfo).u == "enH0" || ((TileInfo) x.BaseInfo).u == "slLU" ||
						         ((TileInfo) x.BaseInfo).u == "slRU").Count == 0)
						continue;

					/*FootholdLine surfaceLine = GetConnectingLine((FootholdAnchor)tileInst.BoundItemsList[1], (FootholdAnchor)tileInst.BoundItemsList[2]);
					if (surfaceLine == null)
					{
					    continue;
					}*/

					var contAnchor = FindOptimalContinuationAnchor(
						(tileInst.BoundItemsList[1].Y + tileInst.BoundItemsList[nitems - 2].Y) / 2,
						tileInst.BoundItemsList[1].X, tileInst.BoundItemsList[nitems - 2].X, tileInst.LayerNumber);
					if (contAnchor == null) continue;

					// The anchor is guaranteed to have exactly 1 line
					var anchorLine = (FootholdLine) contAnchor.connectedLines[0];
					// The line is guaranteed to be non-vertical
					var direction = anchorLine.GetOtherAnchor(contAnchor).X > contAnchor.X
						? Direction.Right
						: Direction.Left;
					FootholdAnchor remainingAnchor = null;
					var remainingIndex = -1;

					// Remove the rightmost/leftmost footholds
					for (var i = direction == Direction.Right ? 0 : nitems - 1;
					     direction == Direction.Right ? i < nitems : i > 0;
					     i += direction == Direction.Right ? 1 : -1) {
						var anchor = (FootholdAnchor) tileInst.BoundItemsList[i];
						if (direction == Direction.Right ? anchor.X >= contAnchor.X : anchor.X <= contAnchor.X) break;

						remainingIndex = i;
					}

					if (remainingIndex == -1) continue;

					remainingAnchor = (FootholdAnchor) tileInst.BoundItemsList[remainingIndex];
					var deleteStart = direction == Direction.Right ? remainingIndex + 1 : 0;
					var deleteEnd = direction == Direction.Right ? nitems : remainingIndex;

					for (var i = deleteStart; i < deleteEnd; i++)
						((FootholdAnchor) tileInst.BoundItemsList[deleteStart]).RemoveItem(null);

					board.BoardItems.FootholdLines.Add(new FootholdLine(board, remainingAnchor, contAnchor,
						anchorLine.ForbidFallDown, anchorLine.CantThrough, anchorLine.Piece, anchorLine.Force));
				}
			}

			// Remove all Tile-FH bindings since they have no meaning now
			foreach (var li in board.BoardItems.TileObjs) {
				if (!(li is TileInstance)) continue;

				var tileInst = (TileInstance) li;

				while (tileInst.BoundItemsList.Count > 0) tileInst.ReleaseItem(tileInst.BoundItemsList[0]);
			}

			board.UndoRedoMan.UndoList.Clear();
			board.UndoRedoMan.RedoList.Clear();

			// Break foothold lines
			/*for (int i = 0; i < board.BoardItems.FootholdLines.Count; i++)
			{
			    FootholdLine line = board.BoardItems.FootholdLines[i];
			    if (line.FirstDot.X == line.SecondDot.X || line.FirstDot.Y == line.SecondDot.Y)
			    {
			        foreach (FootholdAnchor anchor in board.BoardItems.FHAnchors)
			        {
			            if ((anchor.X == line.FirstDot.X && anchor.X == line.SecondDot.X && Math.Min(line.FirstDot.Y, line.SecondDot.Y) < anchor.Y && Math.Max(line.FirstDot.Y, line.SecondDot.Y) > anchor.Y && anchor.AllConnectedLinesVertical())
			             || (anchor.Y == line.FirstDot.Y && anchor.Y == line.SecondDot.Y && Math.Min(line.FirstDot.X, line.SecondDot.X) < anchor.X && Math.Max(line.FirstDot.X, line.SecondDot.X) > anchor.X && anchor.AllConnectedLinesHorizontal()))
			            {
			                // Create first line
			                if (!FootholdExists((FootholdAnchor)line.FirstDot, anchor))
			                {
			                    board.BoardItems.FootholdLines.Add(new FootholdLine(board, line.FirstDot, anchor, line.ForbidFallDown, line.CantThrough, line.Piece, line.Force, true));
			                }

			                // Create second line
			                if (!FootholdExists((FootholdAnchor)line.SecondDot, anchor))
			                {
			                    board.BoardItems.FootholdLines.Add(new FootholdLine(board, line.SecondDot, anchor, line.ForbidFallDown, line.CantThrough, line.Piece, line.Force, true));
			                }

			                // Remove long line
			                line.FirstDot.DisconnectLine(line);
			                line.SecondDot.DisconnectLine(line);
			                board.BoardItems.FootholdLines.RemoveAt(i);
			                i--; // To conserve the current loop position
			            }
			        }
			    }
			}

			// Special tile snapping cases
			MapleTable<LayeredItem, bool> leftSnapping = new MapleTable<LayeredItem,bool>();
			MapleTable<LayeredItem, bool> rightSnapping = new MapleTable<LayeredItem,bool>();
			foreach (LayeredItem li in board.BoardItems.TileObjs) {
			    if (!(li is TileInstance))
			        continue;
			    TileInstance tileInst = (TileInstance)li;
			    TileInfo tileInfo = (TileInfo)li.BaseInfo;
			    if (tileInst.BoundItems.Count > 0) // This if statement ensures in one check: 1.that the tile is foothold-containing type and 2.that it was created by the user in this session
			    {
			        Tuple<TileInstance, TileInstance> sideSnaps = tileInst.FindExactSideSnaps();
			        TileInstance prev = sideSnaps.Item1;
			        TileInstance next = sideSnaps.Item2;
			    }
			}*/
		}

		public void ChangeMapTypeAndID(int newId, MapType newType) {
			board.MapInfo.mapType = newType;
			if (newType != MapType.RegularMap)
				return;
			var oldId = board.MapInfo.id;
			if (oldId == newId) return;

			board.MapInfo.id = newId;
			foreach (var portalInst in board.BoardItems.Portals)
				if (portalInst.tm == oldId)
					portalInst.tm = newId;
		}

		public void UpdateMapLists() {
			Program.InfoManager.Maps[WzInfoTools.AddLeadingZeros(board.MapInfo.id.ToString(), 9)] =
				new Tuple<string, string>(board.MapInfo.strStreetName, board.MapInfo.strMapName);
		}
	}

	internal enum Direction {
		Left,
		Right
	}

	internal enum Dots {
		FirstDot,
		SecondDot
	}

	internal enum FootholdDirection {
		Prev,
		Next
	}

	internal enum FootholdOrientation {
		PrevFirstNextSecond = 0, // "Normal"
		NextFirstPrevSecond = 1 // "Inverted"
	}
}