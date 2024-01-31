/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using HaCreator.GUI;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Info.Default;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.Instance.Misc;
using HaCreator.MapEditor.Instance.Shapes;
using HaSharedLibrary.Render.DX;
using HaSharedLibrary.Wz;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure;
using MapleLib.WzLib.WzStructure.Data;

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

			image = new WzImage(name + ".img", Initialization.WzMapleVersion) {
				Parsed = true
			};
		}

		private void InsertImage() {
			if (board.MapInfo.mapType == MapType.RegularMap) {
				var mapId = image.Name.Replace(".img", string.Empty);

				WzDirectory parent;
				if (board.IsNewMapDesign) {
					parent = WzInfoTools.FindMapDirectoryParent(mapId, Program.WzManager);
				} else {
					WzObject mapImage = WzInfoTools.FindMapImage(mapId, Program.WzManager);
					if (mapImage == null) {
						throw new Exception("Could not find a suitable Map.wz to place the new map into.");
					}

					parent = (WzDirectory) mapImage.Parent;
					mapImage.Remove();
				}

				parent.AddImage(image);

				Program.WzManager.SetWzFileUpdated(parent.GetTopMostWzDirectory().Name /* "map" */, image);
			} else {
				var mapDir = Program.WzManager["ui"];
				mapDir[image.Name]?.Remove();

				mapDir.AddImage(image);
				Program.WzManager.SetWzFileUpdated("ui", image);
			}
		}

		private void SaveMapInfo() {
			board.MapInfo.Save(image,
				board.VRRectangle == null
					? (Rectangle?) null
					: new Rectangle(board.VRRectangle.X, board.VRRectangle.Y, board.VRRectangle.Width,
						board.VRRectangle.Height));
			if (board.MapInfo.mapType == MapType.RegularMap) {
				var strMapImg = (WzImage) Program.WzManager.FindWzImageByName("string", "Map.img");
				if (strMapImg == null) {
					throw new Exception("Map.img not found in string.wz");
				}

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
			if (board.MiniMap == null || board.MinimapRectangle == null) return;
			var miniMap = new WzSubProperty();
			var canvas = new WzCanvasProperty {
				PngProperty = new WzPngProperty()
			};
			canvas.PngProperty.PixFormat = (int) WzPngProperty.CanvasPixFormat.Argb4444;
			canvas.PngProperty.SetImage(board.MiniMap);
			miniMap["canvas"] = canvas;
			miniMap["width"] = InfoTool.SetInt(board.MinimapRectangle.Width);
			miniMap["height"] = InfoTool.SetInt(board.MinimapRectangle.Height);
			miniMap["centerX"] = InfoTool.SetInt(-board.MinimapPosition.X);
			miniMap["centerY"] = InfoTool.SetInt(-board.MinimapPosition.Y);
			miniMap["mag"] = InfoTool.SetInt(4);
			image["miniMap"] = miniMap;
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
				foreach (var item in l.Items) {
					if (item is ObjectInstance instance) {
						var obj = new WzSubProperty();
						var objInst = instance;
						var objInfo = (ObjectInfo) objInst.BaseInfo;

						obj["x"] = InfoTool.SetInt(objInst.UnflippedX);
						obj["y"] = InfoTool.SetInt(objInst.Y);
						obj["z"] = InfoTool.SetInt(objInst.Z);
						obj["oS"] = InfoTool.SetString(objInfo.oS);
						obj["l0"] = InfoTool.SetString(objInfo.l0);
						obj["l1"] = InfoTool.SetString(objInfo.l1);
						obj["l2"] = InfoTool.SetString(objInfo.l2);
						obj["f"] = InfoTool.SetBool(objInst.Flip);
						obj["zM"] = InfoTool.SetInt(objInst.PlatformNumber);
						obj["name"] = InfoTool.SetOptionalString(objInst.Name, Defaults.Object.Name);
						obj["r"] = objInst.r.SetOptionalBool(Defaults.Object.R);
						obj["hide"] = objInst.hide.SetOptionalBool(Defaults.Object.Hide);
						obj["reactor"] = objInst.reactor.SetOptionalBool(Defaults.Object.Reactor);
						obj["flow"] = objInst.flow.SetOptionalBool(Defaults.Object.Flow);
						obj["rx"] = objInst.rx.SetOptionalInt(Defaults.Object.RX);
						obj["ry"] = objInst.ry.SetOptionalInt(Defaults.Object.RY);
						obj["cx"] = objInst.cx.SetOptionalInt(Defaults.Object.CX);
						obj["cy"] = objInst.cy.SetOptionalInt(Defaults.Object.CY);
						obj["tags"] = InfoTool.SetOptionalString(objInst.tags, Defaults.Object.Tags);
						if (objInst.QuestInfo != null) {
							var questParent = new WzSubProperty();
							foreach (var objQuest in objInst.QuestInfo)
								questParent[objQuest.questId.ToString()] = InfoTool.SetInt((int) objQuest.state);

							obj["quest"] = questParent;
						}

						objParent[objIndex.ToString()] = obj;
						objIndex++;
					} else if (item is TileInstance instance1) {
						tiles.Add(instance1);
					} else {
						throw new Exception("Unknown type in layered lists");
					}
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
			var portalParent = new WzSubProperty();
			for (var i = 0; i < board.BoardItems.Portals.Count; i++) {
				var portalInst = board.BoardItems.Portals[i];
				var portal = new WzSubProperty();

				portal["x"] = InfoTool.SetInt(portalInst.X);
				portal["y"] = InfoTool.SetInt(portalInst.Y);
				portal["pt"] = InfoTool.SetInt(portalInst.pt);
				portal["tm"] = InfoTool.SetInt(portalInst.tm);
				portal["tn"] = InfoTool.SetString(portalInst.tn);
				portal["pn"] = InfoTool.SetString(portalInst.pn);
				portal["script"] = InfoTool.SetOptionalString(portalInst.script, Defaults.Portal.Script);
				portal["verticalImpact"] = portalInst.verticalImpact.SetOptionalInt(Defaults.Portal.VerticalImpact);
				portal["horizontalImpact"] = portalInst.horizontalImpact.SetOptionalInt(Defaults.Portal.HorizontalImpact);
				portal["hRange"] = portalInst.hRange.SetOptionalInt(Defaults.Portal.HRange);
				portal["vRange"] = portalInst.vRange.SetOptionalInt(Defaults.Portal.VRange);
				portal["delay"] = portalInst.delay.SetOptionalInt(Defaults.Portal.Delay);
				portal["hideTooltip"] = portalInst.hideTooltip.SetOptionalBool(Defaults.Portal.HideTooltip);
				portal["onlyOnce"] = portalInst.onlyOnce.SetOptionalBool(Defaults.Portal.OnlyOnce);
				portal["image"] = InfoTool.SetOptionalString(portalInst.image, Defaults.Portal.Image);

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
				reactor["name"] = InfoTool.SetOptionalString(reactorInst.Name, Defaults.Reactor.Name);
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
				if (strTooltipImg != null) {
					break; // found
				}
			}

			if (strTooltipImg == null) {
				throw new Exception("Unable to find ToolTipHelp.img in String.wz");
			}

			var strTooltipCat = (WzSubProperty) strTooltipImg["Mapobject"];
			var strTooltipParent = (WzSubProperty) strTooltipCat[board.MapInfo.id.ToString()];
			if (strTooltipParent == null) {
				strTooltipParent = new WzSubProperty();
				strTooltipCat[board.MapInfo.id.ToString()] = strTooltipParent;
				Program.WzManager.SetWzFileUpdated("string", strTooltipImg);
				retainTooltipStrings = false;
			}

			// Check if the tooltips' original numbers can still be used
			if (retainTooltipStrings) {
				for (var i = 0; i < board.BoardItems.ToolTips.Count; i++) {
					if (board.BoardItems.ToolTips[i].OriginalNumber == -1) {
						retainTooltipStrings = false;
						break;
					}
				}
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
				if (ttInst.CharacterToolTip != null) {
					tooltipParent[tooltipPropStr + "char"] = PackRectangle(ttInst.CharacterToolTip);
				}

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
					strTooltipProp["Title"] = InfoTool.SetString(ttInst.Title);
					strTooltipProp["Desc"] = InfoTool.SetOptionalString(ttInst.Desc, Defaults.ToolTip.Desc);
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
				bgProp["front"] = bgInst.front.SetOptionalBool(Defaults.Background.Front);
				if (bgInst.screenMode != (int) RenderResolution.Res_All) // 0
				{
					bgProp["screenMode"] = InfoTool.SetInt(bgInst.screenMode);
				}

				bgProp["spineAni"] = InfoTool.SetOptionalString(bgInst.SpineAni, Defaults.Background.SpineAni);
				bgProp["spineRandomStart"] = bgInst.SpineRandomStart.SetOptionalBool(Defaults.Background.SpineRandomStart);

				bgProp["f"] = bgInst.Flip.SetOptionalBool(Defaults.Background.Flip);
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

				var anchor1 = line.FirstDot;
				var anchor2 = line.SecondDot;
				line.next = ((FootholdAnchor) line.SecondDot).GetOtherLine(line)?.num ?? 0;
				line.prev = ((FootholdAnchor) line.FirstDot).GetOtherLine(line)?.num ?? 0;

				var fhProp = new WzSubProperty();
				fhProp["x1"] = InfoTool.SetInt(anchor1.X);
				fhProp["y1"] = InfoTool.SetInt(anchor1.Y);
				fhProp["x2"] = InfoTool.SetInt(anchor2.X);
				fhProp["y2"] = InfoTool.SetInt(anchor2.Y);
				fhProp["prev"] = InfoTool.SetInt(line.prev);
				fhProp["next"] = InfoTool.SetInt(line.next);
				fhProp["cantThrough"] = line.CantThrough.SetOptionalBool(Defaults.Foothold.CantThrough);
				fhProp["forbidFallDown"] = line.ForbidFallDown.SetOptionalBool(Defaults.Foothold.ForbidFalldown);
				fhProp["piece"] = line.Piece.SetOptionalInt(Defaults.Foothold.Piece);
				fhProp["force"] = line.Force.SetOptionalDouble(Defaults.Foothold.Force);
				prop[line.num.ToString()] = fhProp;

				line.saved = true;
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
					? board.BoardItems.Mobs[i]
					: (LifeInstance) board.BoardItems.NPCs[i - mobCount];
				var lifeProp = new WzSubProperty();

				lifeProp["id"] =
					InfoTool.SetString(mob ? ((MobInfo) lifeInst.BaseInfo).ID : ((NpcInfo) lifeInst.BaseInfo).ID);
				lifeProp["x"] = InfoTool.SetInt(lifeInst.UnflippedX);
				lifeProp["y"] = InfoTool.SetInt(lifeInst.Y - lifeInst.yShift);
				lifeProp["cy"] = InfoTool.SetInt(lifeInst.Y);
				lifeProp["mobTime"] = lifeInst.MobTime.SetOptionalInt(Defaults.Life.MobTime);
				lifeProp["info"] = lifeInst.Info.SetOptionalInt(Defaults.Life.Info);
				lifeProp["team"] = lifeInst.Team.SetOptionalInt(Defaults.Life.Team);
				lifeProp["rx0"] = InfoTool.SetInt(lifeInst.X - lifeInst.rx0Shift);
				lifeProp["rx1"] = InfoTool.SetInt(lifeInst.X + lifeInst.rx1Shift);
				lifeProp["f"] = lifeInst.Flip.SetOptionalBool(Defaults.Life.F);
				lifeProp["hide"] = lifeInst.Hide.SetOptionalBool(Defaults.Life.Hide);
				lifeProp["type"] = InfoTool.SetString(mob ? "m" : "n");
				lifeProp["limitedname"] = InfoTool.SetOptionalString(lifeInst.LimitedName, Defaults.Life.LimitedName);
				lifeProp["fh"] = InfoTool.SetInt(GetFootholdBelow(lifeInst.X, lifeInst.Y));
				lifeParent[i.ToString()] = lifeProp;
			}

			image["life"] = lifeParent;
		}

		public void SaveMisc() {
			var areaParent = new WzSubProperty();
			var buffParent = new WzSubProperty();
			var swimParent = new WzSubProperty();

			foreach (var item in board.BoardItems.MiscItems) {
				if (item is Clock clock) {
					var clockProp = new WzSubProperty();
					clockProp["x"] = InfoTool.SetInt(clock.Left);
					clockProp["y"] = InfoTool.SetInt(clock.Top);
					clockProp["width"] = InfoTool.SetInt(clock.Width);
					clockProp["height"] = InfoTool.SetInt(clock.Height);
					image["clock"] = clockProp;
				} else if (item is ShipObject ship) {
					var shipInfo = (ObjectInfo) ship.BaseInfo;
					var shipProp = new WzSubProperty();
					shipProp["shipObj"] = InfoTool.SetString("Map/Obj/" + shipInfo.oS + ".img/" + shipInfo.l0 + "/" +
					                                         shipInfo.l1 + "/" + shipInfo.l2);
					shipProp["x"] = InfoTool.SetInt(ship.UnflippedX);
					shipProp["y"] = InfoTool.SetInt(ship.Y);
					shipProp["z"] = ship.zValue.SetOptionalInt(Defaults.ShipObj.ZValue);
					shipProp["x0"] = ship.X0.SetOptionalInt(Defaults.ShipObj.X0);
					shipProp["tMove"] = InfoTool.SetInt(ship.TimeMove);
					shipProp["shipKind"] = InfoTool.SetInt(ship.ShipKind);
					shipProp["f"] = InfoTool.SetBool(ship.Flip);
					image["shipObj"] = shipProp;
				} else if (item is Area area) {
					areaParent[area.Identifier] = PackRectangle(area);
				} else if (item is Healer healer) {
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
				} else if (item is Pulley pulley) {
					var pulleyInfo = (ObjectInfo) pulley.BaseInfo;
					var pulleyProp = new WzSubProperty();
					pulleyProp["pulley"] = InfoTool.SetString("Map/Obj/" + pulleyInfo.oS + ".img/" + pulleyInfo.l0 +
					                                          "/" + pulleyInfo.l1 + "/" + pulleyInfo.l2);
					pulleyProp["x"] = InfoTool.SetInt(pulley.X);
					pulleyProp["y"] = InfoTool.SetInt(pulley.Y);
					image["pulley"] = pulleyProp;
				} else if (item is BuffZone buff) {
					var buffProp = PackRectangle(buff);
					buffProp["ItemID"] = InfoTool.SetInt(buff.ItemID);
					buffProp["Interval"] = InfoTool.SetInt(buff.Interval);
					buffProp["Duration"] = InfoTool.SetInt(buff.Duration);
					buffParent[buff.ZoneName] = buffProp;
				} else if (item is SwimArea swim) {
					swimParent[swim.Identifier] = PackRectangle(swim);
				}
			}

			if (areaParent.WzProperties.Count > 0) image["area"] = areaParent;

			if (buffParent.WzProperties.Count > 0) image["BuffZone"] = buffParent;

			if (swimParent.WzProperties.Count > 0) image["swimArea"] = swimParent;
		}

		public void SaveMirrorFieldData() {
			if (board.BoardItems.MirrorFieldDatas.Count == 0) {
				return;
			}

			var mirrorFieldDataParent = new WzSubProperty();

			var i = 0;
			foreach (MirrorFieldDataType dataType in
			         Enum.GetValues(typeof(MirrorFieldDataType))) // initial holder data, only run once
			{
				if (dataType == MirrorFieldDataType.NULL || dataType == MirrorFieldDataType.info) {
					continue;
				}

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
			foreach (BoardItem item in board.BoardItems.MirrorFieldDatas) {
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
						itemProp.SetLtRbRectangle(new Rectangle(rect.X, rect.Y, rect.Width,
								rect.Height) // convert Microsoft.Xna.Framework.Rectangle to System.Drawing.Rectangle
						);

						itemProp["offset"] = InfoTool.SetVector(mirrorFieldData.Offset.X, mirrorFieldData.Offset.Y);
						itemProp["gradient"] = InfoTool.SetInt(mirrorFieldData.ReflectionInfo.Gradient);
						itemProp["alpha"] = InfoTool.SetInt(mirrorFieldData.ReflectionInfo.Alpha);
						itemProp["objectForOverlay"] =
							InfoTool.SetOptionalString(mirrorFieldData.ReflectionInfo.ObjectForOverlay, Defaults.MirrorData.ObjectForOverlay);
						itemProp["reflection"] = mirrorFieldData.ReflectionInfo.Reflection.SetOptionalBool(Defaults.MirrorData.Reflection);
						itemProp["alphaTest"] = mirrorFieldData.ReflectionInfo.AlphaTest.SetOptionalBool(Defaults.MirrorData.AlphaTest);

						targetObjectWzProperty.WzProperties.Add(itemProp);
					} else {
						throw new Exception("Error saving mirror field data. Missing MirrorFieldDataType of " +
						                    forTargetObject);
					}
				}
			}

			if (mirrorFieldDataParent.WzProperties.Count > 0) image["MirrorFieldData"] = mirrorFieldDataParent;
		}

		/// <summary>
		/// Saves the additional unsupported properties read from the map image.
		/// </summary>
		private void SaveAdditionals() {
			foreach (var prop in board.MapInfo.additionalNonInfoProps) {
				image.AddProperty(prop);
			}
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
			var bestFoothold = -1;
			var y1 = int.MaxValue;
			foreach (var fh in board.BoardItems.FootholdLines) {
				if (fh.IsWall) continue;
				var x1 = fh.FirstDot.X;
				var x2 = fh.SecondDot.X;
				if (x1 >= x2) continue;
				if (x1 > x) continue;
				if (x2 < x) continue;
				var y2 = fh.FirstDot.Y + (x - x1) * (fh.SecondDot.Y - fh.FirstDot.Y) / (x2 - x1);
				if (y2 < y) continue;
				if (y2 >= y1) continue;
				y1 = y2;
				bestFoothold = fh.num;
			}

			if (bestFoothold == -1)
				// 0 stands in the game for flying or nonexistant foothold; I do not know what are the results of putting an NPC there,
				// however, if the user puts an NPC with no floor under it he should expect weird things to happen.
			{
				return 0;
			}

			return bestFoothold;
		}


		private FootholdAnchor FindOptimalContinuationAnchor(int y, int x0, int x1, int layer) {
			FootholdAnchor result = null;
			var distance = int.MaxValue;
			foreach (var anchor in board.BoardItems.FHAnchors) {
				// Find an anchor on the same layer, with 1 connected line, in the X range of our target line, whose line is not vertical
				if (anchor.LayerNumber != layer || anchor.connectedLines.Count != 1 || anchor.X < x0 || anchor.X > x1 ||
				    anchor.connectedLines[0].FirstDot.X == anchor.connectedLines[0].SecondDot.X) {
					continue;
				}

				var d = Math.Abs(anchor.Y - y);
				if (d < distance) {
					distance = d;
					result = anchor;
				}

				if (distance == 0)
					// Not going to find anything better
				{
					return result;
				}
			}

			return distance < 100 ? result : null;
		}

		public void ActualizeFootholds() {
			board.BoardItems.FHAnchors.Sort(FootholdAnchor.FHAnchorSorter);

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
					    tileInst.BoundItemsList[nitems - 1].X != tileInst.BoundItemsList[nitems - 2].X) {
						continue;
					}

					// Only work with snapped edU's
					if (tileInst.FindSnappableTiles(0,
						    x => ((TileInfo) x.BaseInfo).u == "enH0" || ((TileInfo) x.BaseInfo).u == "slLU" ||
						         ((TileInfo) x.BaseInfo).u == "slRU").Count == 0) {
						continue;
					}

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
			if (newType != MapType.RegularMap) {
				return;
			}

			var oldId = board.MapInfo.id;
			if (oldId == newId) return;

			board.MapInfo.id = newId;
			foreach (var portalInst in board.BoardItems.Portals) {
				if (portalInst.tm == oldId) {
					portalInst.tm = newId;
				}
			}
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
}