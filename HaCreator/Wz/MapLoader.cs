/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HaCreator.MapEditor;
using Microsoft.Xna.Framework;
using MapleLib.WzLib;
using MapleLib.WzLib.WzStructure;
using MapleLib.WzLib.WzStructure.Data;
using MapleLib.Helpers;
using MapleLib.WzLib.WzProperties;
using System.Collections;
using HaCreator.MapEditor.Instance.Shapes;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Instance.Misc;
using XNA = Microsoft.Xna.Framework;
using System.Runtime.Remoting.Channels;
using System.Windows.Media;
using HaSharedLibrary.Util;
using HaCreator.GUI;
using HaCreator.MapSimulator;
using HaCreator.Exceptions;
using HaSharedLibrary.Render.DX;
using HaSharedLibrary.Render;
using HaSharedLibrary.Wz;

namespace HaCreator.Wz {
	public static class MapLoader {
		public static List<string> VerifyMapPropsKnown(WzImage mapImage, bool userless) {
			var copyPropNames = new List<string>();
			foreach (var prop in mapImage.WzProperties)
				switch (prop.Name) {
					case "0":
					case "1":
					case "2":
					case "3":
					case "4":
					case "5":
					case "6":
					case "7":
					case "8": // what? 749080500.img
					case "info":
					case "life":
					case "ladderRope":
					case "reactor":
					case "back":
					case "foothold":
					case "miniMap":
					case "portal":
					case "seat":
					case "ToolTip":
					case "clock":
					case "shipObj":
					case "area":
					case "healer":
					case "pulley":
					case "BuffZone":
					case "swimArea":
						continue;
					case "coconut"
						: // The coconut event. Prop is copied but not edit-supported, we don't need to notify the user since it has no stateful objects. (e.g. 109080002)
					case "user"
						: // A map prop that dresses the user with predefined items according to his job. No stateful objects. (e.g. 930000010)
					case "noSkill"
						: // Preset in Monster Carnival maps, can only guess by its name that it blocks skills. Nothing stateful. (e.g. 980031100)
					case "snowMan"
						: // I don't even know what is this for; it seems to only have 1 prop with a path to the snowman, which points to a nonexistant image. (e.g. 889100001)
					case "weather"
						: // This has something to do with cash weather items, and exists in some nautlius maps (e.g. 108000500)
					case "mobMassacre": // This is the Mu Lung Dojo header property (e.g. 926021200)
					case "battleField": // The sheep vs wolf event and other useless maps (e.g. 109090300, 910040100)
						copyPropNames.Add(prop.Name);
						continue;
					case "snowBall"
						: // The snowball/snowman event. It has the snowman itself, which is a stateful object (somewhat of a mob), but we do not support it.
					case "monsterCarnival"
						: // The Monster Carnival. It has an immense amount of info and stateful objects, including the mobs and guardians. We do not support it. (e.g. 980000201)
						copyPropNames.Add(prop.Name);
						if (!userless)
							MessageBox.Show("The map you are opening has the feature \"" + prop.Name +
							                "\", which is purposely not supported in the editor.\r\nTo get around this, HaCreator will copy the original feature's data byte-to-byte. This might cause the feature to stop working if it depends on map objects, such as footholds or mobs.");

						continue;
					case "tokyoBossParty": // Neo Tokyo 802000801.img
					case "skyWhale":
					case "rectInfo":
					case "directionInfo":
					case "particle":
					case "respawn":
					case "enterUI":
					case "mobTeleport":
					case "climbArea":
					case "stigma":
					case "monsterDefense":
					case "oxQuiz":
					case "nodeInfo":
					case "onlyUseSkill":
					case "replaceUI":
					case "rapidStream":
					case "areaCtrl":
					case "swimArea_Moment":
					case "reactorRemove":
					case "objectVisibleLevel":
					case "bonusRewards":
					case "incHealRate":
					case "triggersTW":
					case "climbArea_Moment":
					case "crawlArea":
					case "checkPoint":
					case "mobKillCountExp":
					case "ghostPark":
					case "courtshipDance":
					case "fishingZone":
					case "remoteCharacterEffect":
					case "publicTaggedObjectVisible":
					case "MirrorFieldData":
					case "defenseMob":
					case "randomMobGen":
					case "unusableSkillArea":
					case "flyingAreaData":
					case "extinctMO":
					case "permittedSkill":
					case "WindArea":
					case "pocketdrop":
					case "footprintData":
					case "illuminantCluster": // 450016030.img
					case "property": // 450016110.img
					case "languageSchool": // 702090101.img
					case "languageSchoolQuizTime":
					case "languageSchoolMobSummonItemID":
						continue;

					default:
						var error = string.Format("[MapLoader] Unknown field property '{0}', {1}", prop.Name,
							mapImage.ToString() /*overrides see WzImage.ToString()*/);

						ErrorLogger.Log(ErrorLevel.MissingFeature, error);
						copyPropNames.Add(prop.Name);
						break;
				}

			return copyPropNames;
		}

		public static MapType GetMapType(WzImage mapImage) {
			switch (mapImage.Name) {
				case "MapLogin.img":
				case "MapLogin1.img":
				case "MapLogin2.img":
				case "MapLogin3.img":
					return MapType.MapLogin;
				case "CashShopPreview.img":
					return MapType.CashShopPreview;
				default:
					return MapType.RegularMap;
			}
		}

		private static bool GetMapVR(WzImage mapImage, ref System.Drawing.Rectangle? VR) {
			var fhParent = (WzSubProperty) mapImage["foothold"];
			if (fhParent == null) {
				VR = null;
				return false;
			}

			int mostRight = int.MinValue, mostLeft = int.MaxValue, mostTop = int.MaxValue, mostBottom = int.MinValue;
			foreach (WzSubProperty fhLayer in fhParent.WzProperties)
			foreach (WzSubProperty fhCat in fhLayer.WzProperties)
			foreach (WzSubProperty fh in fhCat.WzProperties) {
				var x1 = InfoTool.GetInt(fh["x1"]);
				var x2 = InfoTool.GetInt(fh["x2"]);
				var y1 = InfoTool.GetInt(fh["y1"]);
				var y2 = InfoTool.GetInt(fh["y2"]);

				if (x1 > mostRight) mostRight = x1;
				if (x1 < mostLeft) mostLeft = x1;
				if (x2 > mostRight) mostRight = x2;
				if (x2 < mostLeft) mostLeft = x2;
				if (y1 > mostBottom) mostBottom = y1;
				if (y1 < mostTop) mostTop = y1;
				if (y2 > mostBottom) mostBottom = y2;
				if (y2 < mostTop) mostTop = y2;
			}

			if (mostRight == int.MinValue || mostLeft == int.MaxValue || mostTop == int.MaxValue ||
			    mostBottom == int.MinValue) {
				VR = null;
				return false;
			}

			var VRLeft = mostLeft - 10;
			var VRRight = mostRight + 10;
			var VRBottom = mostBottom + 110;
			var VRTop = Math.Min(mostBottom - 600, mostTop - 360);
			VR = new System.Drawing.Rectangle(VRLeft, VRTop, VRRight - VRLeft, VRBottom - VRTop);
			return true;
		}

		public static void LoadLayers(WzImage mapImage, Board mapBoard) {
			for (var layer = 0; layer <= MapConstants.MaxMapLayers; layer++) {
				var layerProp = (WzSubProperty) mapImage[layer.ToString()];
				if (layerProp == null)
					continue; // most maps only have 7 layers.

				var tSprop = layerProp["info"]?["tS"];
				string tS = null;
				if (tSprop != null)
					tS = InfoTool.GetString(tSprop);

				// Load objects
				foreach (var obj in layerProp["obj"].WzProperties) {
					var x = InfoTool.GetInt(obj["x"]);
					var y = InfoTool.GetInt(obj["y"]);
					var z = InfoTool.GetInt(obj["z"]);
					var zM = InfoTool.GetInt(obj["zM"]);
					var oS = InfoTool.GetString(obj["oS"]);
					var l0 = InfoTool.GetString(obj["l0"]);
					var l1 = InfoTool.GetString(obj["l1"]);
					var l2 = InfoTool.GetString(obj["l2"]);
					var name = InfoTool.GetOptionalString(obj["name"]);
					var r = InfoTool.GetOptionalBool(obj["r"]);
					var hide = InfoTool.GetOptionalBool(obj["hide"]);
					var reactor = InfoTool.GetOptionalBool(obj["reactor"]);
					var flow = InfoTool.GetOptionalBool(obj["flow"]);
					var rx = InfoTool.GetOptionalTranslatedInt(obj["rx"]);
					var ry = InfoTool.GetOptionalTranslatedInt(obj["ry"]);
					var cx = InfoTool.GetOptionalTranslatedInt(obj["cx"]);
					var cy = InfoTool.GetOptionalTranslatedInt(obj["cy"]);
					var tags = InfoTool.GetOptionalString(obj["tags"]);

					var questParent = obj["quest"];
					List<ObjectInstanceQuest> questInfo = null;
					if (questParent != null) {
						questInfo = new List<ObjectInstanceQuest>();
						foreach (WzIntProperty info in questParent.WzProperties)
							questInfo.Add(new ObjectInstanceQuest(int.Parse(info.Name), (QuestState) info.Value));
					}

					var flip = InfoTool.GetBool(obj["f"]);
					var objInfo = ObjectInfo.Get(oS, l0, l1, l2);
					if (objInfo == null)
						continue;

					var l = mapBoard.Layers[layer];
					mapBoard.BoardItems.TileObjs.Add((LayeredItem) objInfo.CreateInstance(l, mapBoard, x, y, z, zM, r,
						hide, reactor, flow, rx, ry, cx, cy, name, tags, questInfo, flip, false));
					l.zMList.Add(zM);
				}

				// Load tiles
				var tileParent = layerProp["tile"];
				foreach (var tile in tileParent.WzProperties) {
					var x = InfoTool.GetInt(tile["x"]);
					var y = InfoTool.GetInt(tile["y"]);
					var zM = InfoTool.GetInt(tile["zM"]);
					var u = InfoTool.GetString(tile["u"]);
					var no = InfoTool.GetInt(tile["no"]);
					var l = mapBoard.Layers[layer];

					var tileInfo = TileInfo.Get(tS, u, no.ToString());
					mapBoard.BoardItems.TileObjs.Add((LayeredItem) tileInfo.CreateInstance(l, mapBoard, x, y,
						int.Parse(tile.Name), zM, false, false));
					l.zMList.Add(zM);
				}
			}
		}

		public static void LoadLife(WzImage mapImage, Board mapBoard) {
			var lifeParent = mapImage["life"];
			if (lifeParent == null)
				return;

			if (InfoTool.GetOptionalBool(lifeParent["isCategory"]) ==
			    true) // cant handle this for now.  Azwan 262021001.img TODO
				// - 170
				// -- 5
				// -- 4
				// -- 3
				// -- 2
				// -- 1
				// -- 0
				// - 130
				// - 85
				// - 45
				return;

			foreach (WzSubProperty life in lifeParent.WzProperties) {
				var id = InfoTool.GetString(life["id"]);
				var x = InfoTool.GetInt(life["x"]);
				var y = InfoTool.GetInt(life["y"]);
				var cy = InfoTool.GetInt(life["cy"]);
				var mobTime = InfoTool.GetOptionalInt(life["mobTime"]);
				var info = InfoTool.GetOptionalInt(life["info"]);
				var team = InfoTool.GetOptionalInt(life["team"]);
				var rx0 = InfoTool.GetInt(life["rx0"]);
				var rx1 = InfoTool.GetInt(life["rx1"]);
				var flip = InfoTool.GetOptionalBool(life["f"]);
				var hide = InfoTool.GetOptionalBool(life["hide"]);
				var type = InfoTool.GetString(life["type"]);
				var limitedname = InfoTool.GetOptionalString(life["limitedname"]);

				switch (type) {
					case "m":
						var mobInfo = MobInfo.Get(id);
						if (mobInfo == null)
							continue;
						mapBoard.BoardItems.Mobs.Add((MobInstance) mobInfo.CreateInstance(mapBoard, x, cy, x - rx0,
							rx1 - x, cy - y, limitedname, mobTime, flip, hide, info, team));
						break;
					case "n":
						var npcInfo = NpcInfo.Get(id);
						if (npcInfo == null)
							continue;
						mapBoard.BoardItems.NPCs.Add((NpcInstance) npcInfo.CreateInstance(mapBoard, x, cy, x - rx0,
							rx1 - x, cy - y, limitedname, mobTime, flip, hide, info, team));
						break;
					default:
						throw new Exception("invalid life type " + type);
				}
			}
		}

		public static void LoadReactors(WzImage mapImage, Board mapBoard) {
			var reactorParent = (WzSubProperty) mapImage["reactor"];
			if (reactorParent == null) return;
			foreach (WzSubProperty reactor in reactorParent.WzProperties) {
				var x = InfoTool.GetInt(reactor["x"]);
				var y = InfoTool.GetInt(reactor["y"]);
				var reactorTime = InfoTool.GetInt(reactor["reactorTime"]);
				var name = InfoTool.GetOptionalString(reactor["name"]);
				var id = InfoTool.GetString(reactor["id"]);
				var flip = InfoTool.GetBool(reactor["f"]);
				mapBoard.BoardItems.Reactors.Add((ReactorInstance) Program.InfoManager.Reactors[id]
					.CreateInstance(mapBoard, x, y, reactorTime, name, flip));
			}
		}

		public static void LoadChairs(WzImage mapImage, Board mapBoard) {
			var chairParent = (WzSubProperty) mapImage["seat"];
			if (chairParent != null) {
				var i = 0;
				WzImageProperty chairImage;
				while ((chairImage = chairParent[i.ToString()]) != null) {
					if (chairImage is WzVectorProperty chair)
						mapBoard.BoardItems.Chairs.Add(new Chair(mapBoard, chair.X.Value, chair.Y.Value));

					i++;
				}
				// Other WzSubProperty exist in maps like 330000100.img, FriendsStory
				// 'sitDir' 'offset'
			}

			mapBoard.BoardItems.Chairs.Sort(new Comparison<Chair>(
				delegate(Chair a, Chair b) {
					if (a.X > b.X) {
						return 1;
					} else if (a.X < b.X) {
						return -1;
					} else {
						if (a.Y > b.Y)
							return 1;
						else if (a.Y < b.Y)
							return -1;
						else return 0;
					}
				}));
			for (var i = 0; i < mapBoard.BoardItems.Chairs.Count - 1; i++) {
				var a = mapBoard.BoardItems.Chairs[i];
				var b = mapBoard.BoardItems.Chairs[i + 1];
				if (a.Y == b.Y &&
				    a.X == b.X) //removing b is more comfortable because that way we don't need to decrease i
				{
					if (a.Parent == null && b.Parent != null) {
						mapBoard.BoardItems.Chairs.Remove(a);
						i--;
					} else {
						mapBoard.BoardItems.Chairs.Remove(b);
					}
				}
			}
		}

		public static void LoadRopes(WzImage mapImage, Board mapBoard) {
			var ropeParent = (WzSubProperty) mapImage["ladderRope"];
			foreach (WzSubProperty rope in ropeParent.WzProperties) {
				var x = InfoTool.GetInt(rope["x"]);
				var y1 = InfoTool.GetInt(rope["y1"]);
				var y2 = InfoTool.GetInt(rope["y2"]);
				var uf = InfoTool.GetBool(rope["uf"]);
				var page = InfoTool.GetInt(rope["page"]);
				var l = InfoTool.GetBool(rope["l"]);
				mapBoard.BoardItems.Ropes.Add(new Rope(mapBoard, x, y1, y2, l, page, uf));
			}
		}

		private static bool IsAnchorPrevOfFoothold(FootholdAnchor a, FootholdLine x) {
			var prevnum = x.prev;
			var nextnum = x.next;

			foreach (FootholdLine l in a.connectedLines)
				if (l.num == prevnum)
					return true;
				else if (l.num == nextnum) return false;

			throw new Exception("Could not match anchor to foothold");
		}

		public static void LoadFootholds(WzImage mapImage, Board mapBoard) {
			var anchors = new List<FootholdAnchor>();
			var footholdParent = (WzSubProperty) mapImage["foothold"];
			int layer;
			FootholdAnchor a;
			FootholdAnchor b;
			var fhs = new Dictionary<int, FootholdLine>();
			foreach (WzSubProperty layerProp in footholdParent.WzProperties) {
				layer = int.Parse(layerProp.Name);
				var l = mapBoard.Layers[layer];
				foreach (WzSubProperty platProp in layerProp.WzProperties) {
					var zM = int.Parse(platProp.Name);
					l.zMList.Add(zM);
					foreach (WzSubProperty fhProp in platProp.WzProperties) {
						a = new FootholdAnchor(mapBoard, InfoTool.GetInt(fhProp["x1"]), InfoTool.GetInt(fhProp["y1"]),
							layer, zM, false);
						b = new FootholdAnchor(mapBoard, InfoTool.GetInt(fhProp["x2"]), InfoTool.GetInt(fhProp["y2"]),
							layer, zM, false);
						var num = int.Parse(fhProp.Name);
						var next = InfoTool.GetInt(fhProp["next"]);
						var prev = InfoTool.GetInt(fhProp["prev"]);
						var cantThrough = InfoTool.GetOptionalBool(fhProp["cantThrough"]);
						var forbidFallDown = InfoTool.GetOptionalBool(fhProp["forbidFallDown"]);
						var piece = InfoTool.GetOptionalInt(fhProp["piece"]);
						var force = InfoTool.GetOptionalInt(fhProp["force"]);
						if (a.X != b.X || a.Y != b.Y) {
							var fh = new FootholdLine(mapBoard, a, b, forbidFallDown, cantThrough, piece, force);
							fh.num = num;
							fh.prev = prev;
							fh.next = next;
							mapBoard.BoardItems.FootholdLines.Add(fh);
							fhs[num] = fh;
							anchors.Add(a);
							anchors.Add(b);
						}
					}
				}

				anchors.Sort(new Comparison<FootholdAnchor>(FootholdAnchor.FHAnchorSorter));
				for (var i = 0; i < anchors.Count - 1; i++) {
					a = anchors[i];
					b = anchors[i + 1];
					if (a.X == b.X && a.Y == b.Y) {
						FootholdAnchor.MergeAnchors(a, b); // Transfer lines from b to a
						anchors.RemoveAt(i + 1); // Remove b
						i--; // Fix index after we removed b
					}
				}

				foreach (var anchor in anchors) {
					if (anchor.connectedLines.Count > 2)
						foreach (FootholdLine line in anchor.connectedLines)
							if (IsAnchorPrevOfFoothold(anchor, line)) {
								if (fhs.ContainsKey(line.prev)) line.prevOverride = fhs[line.prev];
							} else {
								if (fhs.ContainsKey(line.next)) line.nextOverride = fhs[line.next];
							}

					mapBoard.BoardItems.FHAnchors.Add(anchor);
				}

				anchors.Clear();
			}
		}

		public static void GenerateDefaultZms(Board mapBoard) {
			// generate default zM's
			var allExistingZMs = new HashSet<int>();
			foreach (var l in mapBoard.Layers) {
				l.RecheckTileSet();
				l.RecheckZM();
				l.zMList.ToList().ForEach(y => allExistingZMs.Add(y));
			}

			for (var i = 0; i < mapBoard.Layers.Count; i++)
			for (var zm_cand = 0; mapBoard.Layers[i].zMList.Count == 0; zm_cand++)
				// Choose a zM that is free
				if (!allExistingZMs.Contains(zm_cand)) {
					mapBoard.Layers[i].zMList.Add(zm_cand);
					allExistingZMs.Add(zm_cand);
					break;
				}
		}

		public static void LoadPortals(WzImage mapImage, Board mapBoard) {
			var portalParent = (WzSubProperty) mapImage["portal"];
			foreach (WzSubProperty portal in portalParent.WzProperties) {
				var x = InfoTool.GetInt(portal["x"]);
				var y = InfoTool.GetInt(portal["y"]);
				var pt = Program.InfoManager.PortalTypeById[InfoTool.GetInt(portal["pt"])];
				var tm = InfoTool.GetInt(portal["tm"]);
				var tn = InfoTool.GetString(portal["tn"]);
				var pn = InfoTool.GetString(portal["pn"]);
				var image = InfoTool.GetOptionalString(portal["image"]);
				var script = InfoTool.GetOptionalString(portal["script"]);
				var verticalImpact = InfoTool.GetOptionalInt(portal["verticalImpact"]);
				var horizontalImpact = InfoTool.GetOptionalInt(portal["horizontalImpact"]);
				var hRange = InfoTool.GetOptionalInt(portal["hRange"]);
				var vRange = InfoTool.GetOptionalInt(portal["vRange"]);
				var delay = InfoTool.GetOptionalInt(portal["delay"]);
				var hideTooltip = InfoTool.GetOptionalBool(portal["hideTooltip"]);
				var onlyOnce = InfoTool.GetOptionalBool(portal["onlyOnce"]);

				mapBoard.BoardItems.Portals.Add(PortalInfo.GetPortalInfoByType(pt).CreateInstance(mapBoard, x, y, pn,
					tn, tm, script, delay, hideTooltip, onlyOnce, horizontalImpact, verticalImpact, image, hRange,
					vRange));
			}
		}

		public static void LoadToolTips(WzImage mapImage, Board mapBoard) {
			var tooltipsParent = (WzSubProperty) mapImage["ToolTip"];
			if (tooltipsParent == null) return;

			var tooltipsStringImage = (WzImage) Program.WzManager.FindWzImageByName("string", "ToolTipHelp.img");
			if (tooltipsStringImage == null)
				throw new Exception("ToolTipHelp.img not found in string.wz");

			if (!tooltipsStringImage.Parsed)
				tooltipsStringImage.ParseImage();

			var tooltipStrings =
				(WzSubProperty) tooltipsStringImage["Mapobject"][mapBoard.MapInfo.id.ToString()];
			if (tooltipStrings == null)
				return;

			for (var i = 0; true; i++) {
				var num = i.ToString();
				var tooltipString = (WzSubProperty) tooltipStrings[num];
				var tooltipProp = (WzSubProperty) tooltipsParent[num];
				var tooltipChar = (WzSubProperty) tooltipsParent[num + "char"];

				if (tooltipString == null && tooltipProp == null)
					break;
				if ((tooltipString == null) ^ (tooltipProp == null))
					continue;

				var title = InfoTool.GetOptionalString(tooltipString["Title"]);
				var desc = InfoTool.GetOptionalString(tooltipString["Desc"]);
				var x1 = InfoTool.GetInt(tooltipProp["x1"]);
				var x2 = InfoTool.GetInt(tooltipProp["x2"]);
				var y1 = InfoTool.GetInt(tooltipProp["y1"]);
				var y2 = InfoTool.GetInt(tooltipProp["y2"]);
				var tooltipPos =
					new Rectangle(x1, y1, x2 - x1, y2 - y1);
				var tt = new ToolTipInstance(mapBoard, tooltipPos, title, desc, i);
				mapBoard.BoardItems.ToolTips.Add(tt);
				if (tooltipChar != null) {
					x1 = InfoTool.GetInt(tooltipChar["x1"]);
					x2 = InfoTool.GetInt(tooltipChar["x2"]);
					y1 = InfoTool.GetInt(tooltipChar["y1"]);
					y2 = InfoTool.GetInt(tooltipChar["y2"]);
					tooltipPos = new Rectangle(x1, y1, x2 - x1, y2 - y1);

					var ttc = new ToolTipChar(mapBoard, tooltipPos, tt);
					mapBoard.BoardItems.CharacterToolTips.Add(ttc);
				}
			}
		}

		public static void LoadBackgrounds(WzImage mapImage, Board mapBoard) {
			var bgParent = (WzSubProperty) mapImage["back"];
			WzSubProperty bgProp;
			var i = 0;
			while ((bgProp = (WzSubProperty) bgParent[(i++).ToString()]) != null) {
				var x = InfoTool.GetInt(bgProp["x"]);
				var y = InfoTool.GetInt(bgProp["y"]);
				var rx = InfoTool.GetInt(bgProp["rx"]);
				var ry = InfoTool.GetInt(bgProp["ry"]);
				var cx = InfoTool.GetInt(bgProp["cx"]);
				var cy = InfoTool.GetInt(bgProp["cy"]);
				var a = InfoTool.GetInt(bgProp["a"]);
				var type = (BackgroundType) InfoTool.GetInt(bgProp["type"]);
				var front = InfoTool.GetBool(bgProp["front"]);
				var screenMode = InfoTool.GetInt(bgProp["screenMode"], (int) RenderResolution.Res_All);
				var spineAni = InfoTool.GetString(bgProp["spineAni"]);
				var spineRandomStart = InfoTool.GetBool(bgProp["spineRandomStart"]);
				bool? flip_t = InfoTool.GetOptionalBool(bgProp["f"]);
				var flip = flip_t.HasValue ? flip_t.Value : false;
				var bS = InfoTool.GetString(bgProp["bS"]);
				var ani = InfoTool.GetBool(bgProp["ani"]);
				var no = InfoTool.GetInt(bgProp["no"]).ToString();

				BackgroundInfoType infoType;
				if (spineAni != null)
					infoType = BackgroundInfoType.Spine;
				else if (ani)
					infoType = BackgroundInfoType.Animation;
				else
					infoType = BackgroundInfoType.Background;

				var bgInfo = BackgroundInfo.Get(mapBoard.ParentControl.GraphicsDevice, bS, infoType, no);
				if (bgInfo == null)
					continue;

				IList list = front ? mapBoard.BoardItems.FrontBackgrounds : mapBoard.BoardItems.BackBackgrounds;
				list.Add((BackgroundInstance) bgInfo.CreateInstance(mapBoard, x, y, i, rx, ry, cx, cy, type, a, front,
					flip, screenMode,
					spineAni, spineRandomStart));
			}
		}

		public static void LoadMisc(WzImage mapImage, Board mapBoard) {
			// All of the following properties are extremely esoteric features that only appear in a handful of maps. 
			// They are implemented here for the sake of completeness, and being able to repack their maps without corruption.

			var clock = mapImage["clock"];
			var ship = mapImage["shipObj"];
			var area = mapImage["area"];
			var healer = mapImage["healer"];
			var pulley = mapImage["pulley"];
			var BuffZone = mapImage["BuffZone"];
			var swimArea = mapImage["swimArea"];
			var
				mirrorFieldData = mapImage["MirrorFieldData"]; // on 5th job maps like Estera, nameless village

			if (clock != null) {
				var clockInstance = new Clock(mapBoard,
					new Rectangle(InfoTool.GetInt(clock["x"]), InfoTool.GetInt(clock["y"]),
						InfoTool.GetInt(clock["width"]), InfoTool.GetInt(clock["height"])));
				mapBoard.BoardItems.Add(clockInstance, false);
			}

			if (ship != null) {
				var objPath = InfoTool.GetString(ship["shipObj"]);
				var objPathParts = objPath.Split("/".ToCharArray());
				var oS = WzInfoTools.RemoveExtension(objPathParts[objPathParts.Length - 4]);
				var l0 = objPathParts[objPathParts.Length - 3];
				var l1 = objPathParts[objPathParts.Length - 2];
				var l2 = objPathParts[objPathParts.Length - 1];

				var objInfo = ObjectInfo.Get(oS, l0, l1, l2);
				var shipInstance = new ShipObject(objInfo, mapBoard,
					InfoTool.GetInt(ship["x"]),
					InfoTool.GetInt(ship["y"]),
					InfoTool.GetOptionalInt(ship["z"]),
					InfoTool.GetOptionalInt(ship["x0"]),
					InfoTool.GetInt(ship["tMove"]),
					InfoTool.GetInt(ship["shipKind"]),
					InfoTool.GetBool(ship["f"]));
				mapBoard.BoardItems.Add(shipInstance, false);
			}

			if (area != null)
				foreach (var prop in area.WzProperties) {
					var x1 = InfoTool.GetInt(prop["x1"]);
					var x2 = InfoTool.GetInt(prop["x2"]);
					var y1 = InfoTool.GetInt(prop["y1"]);
					var y2 = InfoTool.GetInt(prop["y2"]);
					var currArea = new Area(mapBoard,
						new Rectangle(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1)),
						prop.Name);
					mapBoard.BoardItems.Add(currArea, false);
				}

			if (healer != null) {
				var objPath = InfoTool.GetString(healer["healer"]);
				var objPathParts = objPath.Split("/".ToCharArray());
				var oS = WzInfoTools.RemoveExtension(objPathParts[objPathParts.Length - 4]);
				var l0 = objPathParts[objPathParts.Length - 3];
				var l1 = objPathParts[objPathParts.Length - 2];
				var l2 = objPathParts[objPathParts.Length - 1];

				var objInfo = ObjectInfo.Get(oS, l0, l1, l2);
				var healerInstance = new Healer(objInfo, mapBoard,
					InfoTool.GetInt(healer["x"]),
					InfoTool.GetInt(healer["yMin"]),
					InfoTool.GetInt(healer["yMax"]),
					InfoTool.GetInt(healer["healMin"]),
					InfoTool.GetInt(healer["healMax"]),
					InfoTool.GetInt(healer["fall"]),
					InfoTool.GetInt(healer["rise"]));
				mapBoard.BoardItems.Add(healerInstance, false);
			}

			if (pulley != null) {
				var objPath = InfoTool.GetString(pulley["pulley"]);
				var objPathParts = objPath.Split("/".ToCharArray());
				var oS = WzInfoTools.RemoveExtension(objPathParts[objPathParts.Length - 4]);
				var l0 = objPathParts[objPathParts.Length - 3];
				var l1 = objPathParts[objPathParts.Length - 2];
				var l2 = objPathParts[objPathParts.Length - 1];

				var objInfo = ObjectInfo.Get(oS, l0, l1, l2);
				var pulleyInstance = new Pulley(objInfo, mapBoard,
					InfoTool.GetInt(pulley["x"]),
					InfoTool.GetInt(pulley["y"]));
				mapBoard.BoardItems.Add(pulleyInstance, false);
			}

			if (BuffZone != null)
				foreach (var zone in BuffZone.WzProperties) {
					var x1 = InfoTool.GetInt(zone["x1"]);
					var x2 = InfoTool.GetInt(zone["x2"]);
					var y1 = InfoTool.GetInt(zone["y1"]);
					var y2 = InfoTool.GetInt(zone["y2"]);
					var id = InfoTool.GetInt(zone["ItemID"]);
					var interval = InfoTool.GetInt(zone["Interval"]);
					var duration = InfoTool.GetInt(zone["Duration"]);

					var currZone = new BuffZone(mapBoard,
						new Rectangle(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1)), id,
						interval, duration, zone.Name);
					mapBoard.BoardItems.Add(currZone, false);
				}

			if (swimArea != null)
				foreach (var prop in swimArea.WzProperties) {
					var x1 = InfoTool.GetInt(prop["x1"]);
					var x2 = InfoTool.GetInt(prop["x2"]);
					var y1 = InfoTool.GetInt(prop["y1"]);
					var y2 = InfoTool.GetInt(prop["y2"]);

					var currArea = new SwimArea(mapBoard,
						new Rectangle(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1)),
						prop.Name);
					mapBoard.BoardItems.Add(currArea, false);
				}

			if (mirrorFieldData != null)
				foreach (var prop in mirrorFieldData.WzProperties)
				foreach (var prop_ in prop.WzProperties) // mob, user
				{
					var targetObjectReflectionType = MirrorFieldDataType.NULL;
					if (!Enum.TryParse(prop_.Name, out targetObjectReflectionType)) {
						var error = string.Format("New MirrorFieldData type object detected. prop name = '{0}",
							prop_.Name);
						ErrorLogger.Log(ErrorLevel.MissingFeature, error);
					}

					if (targetObjectReflectionType == MirrorFieldDataType.NULL ||
					    targetObjectReflectionType == MirrorFieldDataType.info)
						continue;

					foreach (var prop_items in prop_.WzProperties) {
						var rectBoundary = InfoTool.GetLtRbRectangle(prop_items);
						var offset = InfoTool.GetVector(prop_items["offset"]);
						var gradient = (ushort) InfoTool.GetOptionalInt(prop_items["gradient"], 0);
						var alpha = (ushort) InfoTool.GetOptionalInt(prop_items["alpha"], 0);
						var objectForOverlay = InfoTool.GetOptionalString(prop_items["objectForOverlay"]);
						bool reflection = InfoTool.GetOptionalBool(prop_items["reflection"]);
						bool alphaTest = InfoTool.GetOptionalBool(prop_items["alphaTest"]);

						var rectangle = new Rectangle(
							rectBoundary.X - offset.X.Value,
							rectBoundary.Y - offset.Y.Value,
							rectBoundary.Width,
							rectBoundary.Height);

						var reflectionInfo =
							new ReflectionDrawableBoundary(gradient, alpha, objectForOverlay, reflection,
								alphaTest);

						var mirrorFieldDataItem = new MirrorFieldData(mapBoard, rectangle,
							new Vector2(offset.X.Value, offset.Y.Value), reflectionInfo,
							targetObjectReflectionType);
						mapBoard.BoardItems.MirrorFieldDatas.Add(mirrorFieldDataItem);
					}
				}
			// Some misc items are not implemented here; these are copied byte-to-byte from the original. See VerifyMapPropsKnown for details.
		}

		public static System.Windows.Controls.ContextMenu CreateStandardMapMenu(
			System.Windows.RoutedEventHandler[] rightClickHandler) {
			var menu = new System.Windows.Controls.ContextMenu();

			var menuItem1 = new System.Windows.Controls.MenuItem {
				Header = "Edit map info..."
			};
			menuItem1.Click += rightClickHandler[0];
			menuItem1.Icon = new System.Windows.Controls.Image {
				Source = BitmapHelper.Convert(Properties.Resources.mapEditMenu, System.Drawing.Imaging.ImageFormat.Png)
			};

			var menuItem2 = new System.Windows.Controls.MenuItem {
				Header = "Add VR"
			};
			menuItem2.Click += rightClickHandler[1];
			menuItem2.Icon = new System.Windows.Controls.Image {
				Source = BitmapHelper.Convert(Properties.Resources.mapEditMenu, System.Drawing.Imaging.ImageFormat.Png)
			};

			var menuItem3 = new System.Windows.Controls.MenuItem {
				Header = "Add Minimap"
			};
			menuItem3.Click += rightClickHandler[2];
			menuItem3.Icon = new System.Windows.Controls.Image {
				Source = BitmapHelper.Convert(Properties.Resources.mapEditMenu, System.Drawing.Imaging.ImageFormat.Png)
			};

			var menuItem4 = new System.Windows.Controls.MenuItem {
				Header = "Reload Map"
			};
			menuItem4.Click += rightClickHandler[3];
			menuItem4.Icon = new System.Windows.Controls.Image {
				Source = BitmapHelper.Convert(Properties.Resources.mapEditMenu, System.Drawing.Imaging.ImageFormat.Png)
			};

			var menuItem5 = new System.Windows.Controls.MenuItem {
				Header = "Close"
			};
			menuItem5.Click += rightClickHandler[4];
			menuItem5.Icon = new System.Windows.Controls.Image {
				Source = BitmapHelper.Convert(Properties.Resources.mapEditMenu, System.Drawing.Imaging.ImageFormat.Png)
			};

			menu.Items.Add(menuItem1);
			menu.Items.Add(menuItem2);
			menu.Items.Add(menuItem3);
			menu.Items.Add(menuItem4);
			menu.Items.Add(menuItem5);

			return menu;
		}

		public static void GetMapDimensions(WzImage mapImage, out Rectangle VR, out Point mapCenter, out Point mapSize,
			out Point minimapCenter, out Point minimapSize, out bool hasVR, out bool hasMinimap) {
			var vr = MapInfo.GetVR(mapImage);
			hasVR = vr.HasValue;
			hasMinimap = mapImage["miniMap"] != null;
			if (!hasMinimap) {
				// No minimap, generate sizes from VR
				if (vr == null)
					// No minimap and no VR, our only chance of getting sizes is by generating a VR, if that fails we're screwed
					if (!GetMapVR(mapImage, ref vr))
						throw new NoVRException();

				minimapSize = new Point(vr.Value.Width + 10, vr.Value.Height + 10); //leave 5 pixels on each side
				minimapCenter = new Point(5 - vr.Value.Left, 5 - vr.Value.Top);
				mapSize = new Point(minimapSize.X, minimapSize.Y);
				mapCenter = new Point(minimapCenter.X, minimapCenter.Y);
			} else {
				var miniMap = mapImage["miniMap"];
				minimapSize = new Point(InfoTool.GetInt(miniMap["width"]), InfoTool.GetInt(miniMap["height"]));
				minimapCenter = new Point(InfoTool.GetInt(miniMap["centerX"]), InfoTool.GetInt(miniMap["centerY"]));
				int topOffs = 0, botOffs = 0, leftOffs = 0, rightOffs = 0;
				int leftTarget = 69 - minimapCenter.X,
					topTarget = 86 - minimapCenter.Y,
					rightTarget = minimapSize.X - 69 - 69,
					botTarget = minimapSize.Y - 86 - 86;
				if (vr == null) {
					// We have no VR info, so set all VRs according to their target
					vr = new System.Drawing.Rectangle(leftTarget, topTarget, rightTarget, botTarget);
				} else {
					if (vr.Value.Left < leftTarget) leftOffs = leftTarget - vr.Value.Left;

					if (vr.Value.Top < topTarget) topOffs = topTarget - vr.Value.Top;

					if (vr.Value.Right > rightTarget) rightOffs = vr.Value.Right - rightTarget;

					if (vr.Value.Bottom > botTarget) botOffs = vr.Value.Bottom - botTarget;
				}

				mapSize = new Point(minimapSize.X + leftOffs + rightOffs, minimapSize.Y + topOffs + botOffs);
				mapCenter = new Point(minimapCenter.X + leftOffs, minimapCenter.Y + topOffs);
			}

			VR = new Rectangle(vr.Value.X, vr.Value.Y, vr.Value.Width, vr.Value.Height);
		}

		/// <summary>
		/// Creates the map object from WzImage.
		/// </summary>
		/// <param name="mapId">May be -1 if none. If mapid == -1, it suggest a map that is cloned from a source or loaded from .ham file.</param>
		/// <param name="mapImage"></param>
		/// <param name="mapName"></param>
		/// <param name="streetName"></param>
		/// <param name="categoryName"></param>
		/// <param name="strMapProp"></param>
		/// <param name="Tabs"></param>
		/// <param name="multiBoard"></param>
		/// <param name="rightClickHandler"></param>
		public static void CreateMapFromImage(int mapId, WzImage mapImage, string mapName, string streetName,
			string categoryName, WzSubProperty strMapProp, System.Windows.Controls.TabControl Tabs,
			MultiBoard multiBoard, System.Windows.RoutedEventHandler[] rightClickHandler) {
			if (!mapImage.Parsed)
				mapImage.ParseImage();

			var copyPropNames = VerifyMapPropsKnown(mapImage, false);

			var info = new MapInfo(mapImage, mapName, streetName, categoryName);
			foreach (var copyPropName in copyPropNames) info.additionalNonInfoProps.Add(mapImage[copyPropName]);

			var type = GetMapType(mapImage);
			if (type == MapType.RegularMap)
				info.id = int.Parse(WzInfoTools.RemoveLeadingZeros(WzInfoTools.RemoveExtension(mapImage.Name)));
			info.mapType = type;

			var VR = new Rectangle();
			var center = new Point();
			var size = new Point();
			var minimapSize = new Point();
			var minimapCenter = new Point();
			var hasMinimap = false;
			var hasVR = false;

			try {
				GetMapDimensions(mapImage, out VR, out center, out size, out minimapCenter, out minimapSize, out hasVR,
					out hasMinimap);
			} catch (NoVRException) {
				MessageBox.Show(
					"Error - map does not contain size information and HaCreator was unable to generate it. An error has been logged.",
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				ErrorLogger.Log(ErrorLevel.IncorrectStructure, "no size @map " + info.id.ToString());
				return;
			}

			lock (multiBoard) {
				CreateMap(streetName, mapName, mapId,
					WzInfoTools.RemoveLeadingZeros(WzInfoTools.RemoveExtension(mapImage.Name)),
					CreateStandardMapMenu(rightClickHandler), size, center, Tabs, multiBoard);

				var mapBoard = multiBoard.SelectedBoard;
				mapBoard.Loading = true; // prevents TS Change callbacks
				mapBoard.MapInfo = info;
				if (hasMinimap) {
					mapBoard.MiniMap = ((WzCanvasProperty) mapImage["miniMap"]["canvas"]).GetLinkedWzCanvasBitmap();
					var mmPos = new System.Drawing.Point(-minimapCenter.X, -minimapCenter.Y);
					mapBoard.MinimapPosition = mmPos;
					mapBoard.MinimapRectangle = new MinimapRectangle(mapBoard,
						new Rectangle(mmPos.X, mmPos.Y, minimapSize.X, minimapSize.Y));
				}

				if (hasVR) mapBoard.VRRectangle = new VRRectangle(mapBoard, VR);
				// ensure that the MultiBoard.GraphicDevice is loaded at this point before loading images

				LoadLayers(mapImage, mapBoard);
				LoadLife(mapImage, mapBoard);
				LoadFootholds(mapImage, mapBoard);
				GenerateDefaultZms(mapBoard);
				LoadRopes(mapImage, mapBoard);
				LoadChairs(mapImage, mapBoard);
				LoadPortals(mapImage, mapBoard);
				LoadReactors(mapImage, mapBoard);
				LoadToolTips(mapImage, mapBoard);
				LoadBackgrounds(mapImage, mapBoard);
				LoadMisc(mapImage, mapBoard);

				mapBoard.BoardItems.Sort();
				mapBoard.Loading = false;
			}

			if (ErrorLogger.ErrorsPresent()) {
				ErrorLogger.SaveToFile("errors.txt");
				if (UserSettings.ShowErrorsMessage) {
					// MessageBox.Show("Errors were encountered during the loading process. These errors were saved to \"errors.txt\". Please send this file to the author, either via mail (" + ApplicationSettings.AuthorEmail + ") or from the site you got this software from.\n\n(In the case that this program was not updated in so long that this message is now thrown on every map load, you may cancel this message from the settings)", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}

				ErrorLogger.ClearErrors();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="streetName"></param>
		/// <param name="mapName"></param>
		/// <param name="mapId">May be -1 if none. If mapid == -1, it suggest a map that is cloned from a source or loaded from .ham file.</param>
		/// <param name="tooltip"></param>
		/// <param name="menu"></param>
		/// <param name="size"></param>
		/// <param name="center"></param>
		/// <param name="layers"></param>
		/// <param name="Tabs"></param>
		/// <param name="multiBoard"></param>
		public static void CreateMap(string streetName, string mapName, int mapId, string tooltip,
			System.Windows.Controls.ContextMenu menu, Point size, Point center, System.Windows.Controls.TabControl Tabs,
			MultiBoard multiBoard) {
			lock (multiBoard) {
				var bIsNewMapDesign = mapId == -1;

				var newBoard = multiBoard.CreateBoard(size, center, menu, bIsNewMapDesign);
				GenerateDefaultZms(newBoard);

				var newTabPage = new System.Windows.Controls.TabItem {
					Header = GetFormattedMapNameForTabItem(mapId, streetName, mapName)
				};
				newTabPage.MouseRightButtonUp += (sender, e) => {
					var senderTab = (System.Windows.Controls.TabItem) sender;

					menu.PlacementTarget = senderTab;
					menu.IsOpen = true;
				};

				newBoard.TabPage = newTabPage;
				newTabPage.Tag = new TabItemContainer(mapName, multiBoard, tooltip, menu, newBoard); //newBoard;
				Tabs.Items.Add(newTabPage);
				Tabs.SelectedItem = newTabPage;

				multiBoard.SelectedBoard = newBoard;
				menu.Tag = newBoard;
				foreach (System.Windows.Controls.MenuItem item in menu.Items) item.Tag = newTabPage;

				multiBoard.HaCreatorStateManager.UpdateEditorPanelVisibility();
			}
		}

		/// <summary>
		/// Gets the formatted text of the TabItem (mapid, street name, mapName)
		/// </summary>
		/// <param name="mapId"></param>
		/// <param name="streetName"></param>
		/// <param name="mapName"></param>
		/// <returns></returns>
		public static string GetFormattedMapNameForTabItem(int mapId, string streetName, string mapName) {
			return $"[{(mapId == -1 ? "<NEW>" : mapId.ToString())}] {streetName}: {mapName}"; // Header of the tab
		}

		public static void CreateMapFromHam(MultiBoard multiBoard, System.Windows.Controls.TabControl Tabs, string data,
			System.Windows.RoutedEventHandler[] rightClickHandler) {
			CreateMap("", "", -1, "", CreateStandardMapMenu(rightClickHandler), new Point(), new Point(), Tabs,
				multiBoard);
			multiBoard.SelectedBoard.Loading = true; // Prevent TS Change callbacks while were loading
			lock (multiBoard) {
				multiBoard.SelectedBoard.SerializationManager.DeserializeBoard(data);
				multiBoard.AdjustScrollBars();
			}

			multiBoard.SelectedBoard.Loading = false;
		}
	}
}