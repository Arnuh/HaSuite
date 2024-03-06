/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using HaCreator.Exceptions;
using HaCreator.GUI;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Info.Default;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.Instance.Misc;
using HaCreator.MapEditor.Instance.Shapes;
using HaCreator.Properties;
using HaSharedLibrary.Render;
using HaSharedLibrary.Render.DX;
using HaSharedLibrary.Util;
using HaSharedLibrary.Wz;
using MapleLib.Helpers;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure;
using MapleLib.WzLib.WzStructure.Data;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Image = System.Windows.Controls.Image;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;
using TabControl = System.Windows.Controls.TabControl;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.Wz {
	public static class MapLoader {
		public static List<string> VerifyMapPropsKnown(WzImage mapImage, bool userless) {
			var copyPropNames = new List<string>();
			foreach (var prop in mapImage.WzProperties) {
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
						if (!userless) {
							MessageBox.Show("The map you are opening has the feature \"" + prop.Name +
							                "\", which is purposely not supported in the editor.\r\nTo get around this, HaCreator will copy the original feature's data byte-to-byte. This might cause the feature to stop working if it depends on map objects, such as footholds or mobs.");
						}

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
							mapImage /*overrides see WzImage.ToString()*/);

						ErrorLogger.Log(ErrorLevel.MissingFeature, error);
						copyPropNames.Add(prop.Name);
						break;
				}
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

		private static bool GetMapVR(WzImage mapImage, ref Rectangle? VR) {
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
			VR = new Rectangle(VRLeft, VRTop, VRRight - VRLeft, VRBottom - VRTop);
			return true;
		}

		public static void LoadLayers(WzImage mapImage, Board mapBoard) {
			for (var layer = 0; layer <= MapConstants.MaxMapLayers; layer++) {
				var layerProp = (WzSubProperty) mapImage[layer.ToString()];
				if (layerProp == null) {
					continue;
				}

				var tSprop = layerProp["info"]?["tS"];
				string tS = null;
				if (tSprop != null) {
					tS = InfoTool.GetString(tSprop);
				}

				// Load objects
				foreach (var obj in layerProp["obj"].WzProperties) {
					var x = InfoTool.GetInt(obj["x"]);
					var y = InfoTool.GetInt(obj["y"]);
					var z = InfoTool.GetInt(obj["z"]);
					var oS = InfoTool.GetString(obj["oS"]);
					var l0 = InfoTool.GetString(obj["l0"]);
					var l1 = InfoTool.GetString(obj["l1"]);
					var l2 = InfoTool.GetString(obj["l2"]);
					var flip = obj["f"].GetBool();
					var zM = InfoTool.GetInt(obj["zM"]);
					var name = obj["name"].GetOptionalString(Defaults.Object.Name);
					var r = obj["r"].GetOptionalBool(Defaults.Object.R);
					var hide = obj["hide"].GetOptionalBool(Defaults.Object.Hide);
					var reactor = obj["reactor"].GetOptionalBool(Defaults.Object.Reactor);
					var flow = obj["flow"].GetOptionalBool(Defaults.Object.Flow);
					var rx = obj["rx"].GetOptionalInt(Defaults.Object.RX);
					var ry = obj["ry"].GetOptionalInt(Defaults.Object.RY);
					var cx = obj["cx"].GetOptionalInt(Defaults.Object.CX);
					var cy = obj["cy"].GetOptionalInt(Defaults.Object.CY);
					var tags = obj["tags"].GetOptionalString(Defaults.Object.Tags);

					var questParent = obj["quest"];
					List<ObjectInstanceQuest> questInfo = null;
					if (questParent != null) {
						questInfo = new List<ObjectInstanceQuest>();
						foreach (WzIntProperty info in questParent.WzProperties)
							questInfo.Add(new ObjectInstanceQuest(int.Parse(info.Name), (QuestState) info.Value));
					}

					var objInfo = ObjectInfo.Get(oS, l0, l1, l2);
					if (objInfo == null) {
						continue;
					}

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
			if (lifeParent == null) {
				return;
			}

			if (lifeParent["isCategory"].GetOptionalBool(false)) // cant handle this for now.  Azwan 262021001.img TODO
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
			{
				return;
			}

			foreach (WzSubProperty life in lifeParent.WzProperties) {
				var id = InfoTool.GetString(life["id"]);
				var x = InfoTool.GetInt(life["x"]);
				var y = InfoTool.GetInt(life["y"]);
				var cy = InfoTool.GetInt(life["cy"]);
				var mobTime = life["mobTime"].GetOptionalInt(Defaults.Life.MobTime);
				var info = life["info"].GetOptionalInt(Defaults.Life.Info);
				var team = life["team"].GetOptionalInt(Defaults.Life.Team);
				var rx0 = InfoTool.GetInt(life["rx0"]);
				var rx1 = InfoTool.GetInt(life["rx1"]);
				var flip = life["f"].GetOptionalBool(Defaults.Life.F);
				var hide = life["hide"].GetOptionalBool(Defaults.Life.Hide);
				var type = InfoTool.GetString(life["type"]);
				var limitedname = life["limitedname"].GetOptionalString(Defaults.Life.LimitedName);

				switch (type) {
					case "m":
						var mobInfo = MobInfo.Get(id);
						if (mobInfo == null) {
							continue;
						}

						mapBoard.BoardItems.Mobs.Add((MobInstance) mobInfo.CreateInstance(mapBoard, x, cy, x - rx0,
							rx1 - x, cy - y, limitedname, mobTime, flip, hide, info, team));
						break;
					case "n":
						var npcInfo = NpcInfo.Get(id);
						if (npcInfo == null) {
							continue;
						}

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
				var name = reactor["name"].GetOptionalString(Defaults.Reactor.Name);
				var id = InfoTool.GetString(reactor["id"]);
				var flip = reactor["f"].GetBool();
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
					if (chairImage is WzVectorProperty chair) {
						mapBoard.BoardItems.Chairs.Add(new Chair(mapBoard, chair.X.Value, chair.Y.Value));
					}

					i++;
				}
				// Other WzSubProperty exist in maps like 330000100.img, FriendsStory
				// 'sitDir' 'offset'
			}

			mapBoard.BoardItems.Chairs.Sort(delegate(Chair a, Chair b) {
				if (a.X > b.X) {
					return 1;
				}

				if (a.X < b.X) {
					return -1;
				}

				if (a.Y > b.Y) {
					return 1;
				}

				if (a.Y < b.Y) {
					return -1;
				}

				return 0;
			});
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
				var uf = rope["uf"].GetBool();
				var page = InfoTool.GetInt(rope["page"]);
				var l = rope["l"].GetBool();
				mapBoard.BoardItems.Ropes.Add(new Rope(mapBoard, x, y1, y2, l, page, uf));
			}
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
						var cantThrough = fhProp["cantThrough"].GetOptionalBool(Defaults.Foothold.CantThrough);
						var forbidFallDown = fhProp["forbidFallDown"].GetOptionalBool(Defaults.Foothold.ForbidFalldown);
						var piece = fhProp["piece"].GetOptionalInt(Defaults.Foothold.Piece);
						var force = fhProp["force"].GetOptionalDouble(Defaults.Foothold.Force);
						if (a.X != b.X || a.Y != b.Y) {
							var fh = new FootholdLine(mapBoard, a, b, forbidFallDown, cantThrough, piece, force) {
								num = num,
								prev = prev,
								next = next
							};
							mapBoard.BoardItems.FootholdLines.Add(fh);
							fhs[num] = fh;
							anchors.Add(a);
							anchors.Add(b);
						}
					}
				}

				anchors.Sort(FootholdAnchor.FHAnchorSorter);
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
			{
				if (!allExistingZMs.Contains(zm_cand)) {
					mapBoard.Layers[i].zMList.Add(zm_cand);
					allExistingZMs.Add(zm_cand);
					break;
				}
			}
		}

		public static void LoadPortals(WzImage mapImage, Board mapBoard) {
			var portalParent = (WzSubProperty) mapImage["portal"];
			foreach (WzSubProperty portal in portalParent.WzProperties) {
				var x = InfoTool.GetInt(portal["x"]);
				var y = InfoTool.GetInt(portal["y"]);
				var pt = InfoTool.GetInt(portal["pt"]);
				var tm = InfoTool.GetInt(portal["tm"]);
				var tn = InfoTool.GetString(portal["tn"]);
				var pn = InfoTool.GetString(portal["pn"]);
				var image = portal["image"].GetOptionalString(Defaults.Portal.Image);
				var script = portal["script"].GetOptionalString(Defaults.Portal.Script);
				var verticalImpact = portal["verticalImpact"].GetOptionalInt(Defaults.Portal.VerticalImpact);
				var horizontalImpact = portal["horizontalImpact"].GetOptionalInt(Defaults.Portal.HorizontalImpact);
				var hRange = portal["hRange"].GetOptionalInt(Defaults.Portal.HRange);
				var vRange = portal["vRange"].GetOptionalInt(Defaults.Portal.VRange);
				var delay = portal["delay"].GetOptionalInt(Defaults.Portal.Delay);
				var hideTooltip = portal["hideTooltip"].GetOptionalBool(Defaults.Portal.HideTooltip);
				var onlyOnce = portal["onlyOnce"].GetOptionalBool(Defaults.Portal.OnlyOnce);

				mapBoard.BoardItems.Portals.Add(PortalInfo.GetPortalInfoByType(pt).CreateInstance(mapBoard, x, y, pn,
					tn, tm, script, delay, hideTooltip, onlyOnce, horizontalImpact, verticalImpact, image, hRange,
					vRange));
			}
		}

		public static void LoadToolTips(WzImage mapImage, Board mapBoard) {
			var tooltipsParent = (WzSubProperty) mapImage["ToolTip"];
			if (tooltipsParent == null) return;

			var tooltipsStringImage = (WzImage) Program.WzManager.FindWzImageByName("string", "ToolTipHelp.img");
			if (tooltipsStringImage == null) {
				throw new Exception("ToolTipHelp.img not found in string.wz");
			}

			if (!tooltipsStringImage.Parsed) {
				tooltipsStringImage.ParseImage();
			}

			var tooltipStrings =
				(WzSubProperty) tooltipsStringImage["Mapobject"][mapBoard.MapInfo.id.ToString()];
			if (tooltipStrings == null) {
				return;
			}

			for (var i = 0;; i++) {
				var num = i.ToString();
				var tooltipString = (WzSubProperty) tooltipStrings[num];
				var tooltipProp = (WzSubProperty) tooltipsParent[num];
				var tooltipChar = (WzSubProperty) tooltipsParent[num + "char"];

				if (tooltipString == null && tooltipProp == null) {
					break;
				}

				if ((tooltipString == null) ^ (tooltipProp == null)) {
					continue;
				}

				var title = InfoTool.GetString(tooltipString["Title"]);
				var desc = tooltipString["Desc"].GetOptionalString(Defaults.ToolTip.Desc);
				var x1 = InfoTool.GetInt(tooltipProp["x1"]);
				var x2 = InfoTool.GetInt(tooltipProp["x2"]);
				var y1 = InfoTool.GetInt(tooltipProp["y1"]);
				var y2 = InfoTool.GetInt(tooltipProp["y2"]);
				var tooltipPos =
					new XNA.Rectangle(x1, y1, x2 - x1, y2 - y1);
				var tt = new ToolTipInstance(mapBoard, tooltipPos, title, desc, i);
				mapBoard.BoardItems.ToolTips.Add(tt);
				if (tooltipChar != null) {
					x1 = InfoTool.GetInt(tooltipChar["x1"]);
					x2 = InfoTool.GetInt(tooltipChar["x2"]);
					y1 = InfoTool.GetInt(tooltipChar["y1"]);
					y2 = InfoTool.GetInt(tooltipChar["y2"]);
					tooltipPos = new XNA.Rectangle(x1, y1, x2 - x1, y2 - y1);

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
				var flip = bgProp["f"].GetOptionalBool(Defaults.Background.Flip);
				var bS = InfoTool.GetString(bgProp["bS"]);
				var ani = bgProp["ani"].GetOptionalBool(Defaults.Background.Ani);
				var no = InfoTool.GetInt(bgProp["no"]).ToString();
				var front = bgProp["front"].GetOptionalBool(Defaults.Background.Front);
				var screenMode = bgProp["screenMode"].GetOptionalInt((int) RenderResolution.Res_All);
				var spineAni = bgProp["spineAni"].GetOptionalString(Defaults.Background.SpineAni);
				var spineRandomStart = bgProp["spineRandomStart"].GetOptionalBool(Defaults.Background.SpineRandomStart);

				BackgroundInfoType infoType;
				if (!string.IsNullOrEmpty(spineAni)) {
					infoType = BackgroundInfoType.Spine;
				} else if (ani) {
					infoType = BackgroundInfoType.Animation;
				} else {
					infoType = BackgroundInfoType.Background;
				}

				var bgInfo = BackgroundInfo.Get(mapBoard.ParentControl.GraphicsDevice, bS, infoType, no);
				if (bgInfo == null) {
					continue;
				}

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
					new XNA.Rectangle(InfoTool.GetInt(clock["x"]), InfoTool.GetInt(clock["y"]),
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
					ship["z"].GetOptionalInt(Defaults.ShipObj.ZValue),
					ship["x0"].GetOptionalInt(Defaults.ShipObj.X0),
					InfoTool.GetInt(ship["tMove"]),
					InfoTool.GetInt(ship["shipKind"]),
					ship["f"].GetBool());
				mapBoard.BoardItems.Add(shipInstance, false);
			}

			if (area != null) {
				foreach (var prop in area.WzProperties) {
					var x1 = InfoTool.GetInt(prop["x1"]);
					var x2 = InfoTool.GetInt(prop["x2"]);
					var y1 = InfoTool.GetInt(prop["y1"]);
					var y2 = InfoTool.GetInt(prop["y2"]);
					var currArea = new Area(mapBoard,
						new XNA.Rectangle(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1)),
						prop.Name);
					mapBoard.BoardItems.Add(currArea, false);
				}
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

			if (BuffZone != null) {
				foreach (var zone in BuffZone.WzProperties) {
					var x1 = InfoTool.GetInt(zone["x1"]);
					var x2 = InfoTool.GetInt(zone["x2"]);
					var y1 = InfoTool.GetInt(zone["y1"]);
					var y2 = InfoTool.GetInt(zone["y2"]);
					var id = InfoTool.GetInt(zone["ItemID"]);
					var interval = InfoTool.GetInt(zone["Interval"]);
					var duration = InfoTool.GetInt(zone["Duration"]);

					var currZone = new BuffZone(mapBoard,
						new XNA.Rectangle(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1)), id,
						interval, duration, zone.Name);
					mapBoard.BoardItems.Add(currZone, false);
				}
			}

			if (swimArea != null) {
				foreach (var prop in swimArea.WzProperties) {
					var x1 = InfoTool.GetInt(prop["x1"]);
					var x2 = InfoTool.GetInt(prop["x2"]);
					var y1 = InfoTool.GetInt(prop["y1"]);
					var y2 = InfoTool.GetInt(prop["y2"]);

					var currArea = new SwimArea(mapBoard,
						new XNA.Rectangle(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1)),
						prop.Name);
					mapBoard.BoardItems.Add(currArea, false);
				}
			}

			if (mirrorFieldData != null) {
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
					    targetObjectReflectionType == MirrorFieldDataType.info) {
						continue;
					}

					foreach (var prop_items in prop_.WzProperties) {
						var rectBoundary = prop_items.GetLtRbRectangle();
						var offset = prop_items["offset"].GetVector();
						var gradient = (ushort) prop_items["gradient"].GetOptionalInt(0);
						var alpha = (ushort) prop_items["alpha"].GetOptionalInt(0);
						var objectForOverlay = prop_items["objectForOverlay"].GetOptionalString(Defaults.MirrorData.ObjectForOverlay);
						var reflection = prop_items["reflection"].GetOptionalBool(Defaults.MirrorData.Reflection);
						var alphaTest = prop_items["alphaTest"].GetOptionalBool(Defaults.MirrorData.AlphaTest);

						var rectangle = new XNA.Rectangle(
							rectBoundary.X - offset.X.Value,
							rectBoundary.Y - offset.Y.Value,
							rectBoundary.Width,
							rectBoundary.Height);

						var reflectionInfo =
							new ReflectionDrawableBoundary(gradient, alpha, objectForOverlay, reflection,
								alphaTest);

						var mirrorFieldDataItem = new MirrorFieldData(mapBoard, rectangle,
							new XNA.Vector2(offset.X.Value, offset.Y.Value), reflectionInfo,
							targetObjectReflectionType);
						mapBoard.BoardItems.MirrorFieldDatas.Add(mirrorFieldDataItem);
					}
				}
			}
			// Some misc items are not implemented here; these are copied byte-to-byte from the original. See VerifyMapPropsKnown for details.
		}

		public static ContextMenu CreateStandardMapMenu(
			RoutedEventHandler[] rightClickHandler) {
			var menu = new ContextMenu();

			var menuItem1 = new MenuItem {
				Header = "Edit map info..."
			};
			menuItem1.Click += rightClickHandler[0];
			menuItem1.Icon = new Image {
				Source = Resources.mapEditMenu.Convert(ImageFormat.Png)
			};

			var menuItem2 = new MenuItem {
				Header = "Add VR"
			};
			menuItem2.Click += rightClickHandler[1];
			menuItem2.Icon = new Image {
				Source = Resources.mapEditMenu.Convert(ImageFormat.Png)
			};

			var menuItem3 = new MenuItem {
				Header = "Add Minimap"
			};
			menuItem3.Click += rightClickHandler[2];
			menuItem3.Icon = new Image {
				Source = Resources.mapEditMenu.Convert(ImageFormat.Png)
			};

			var menuItem4 = new MenuItem {
				Header = "Reload Map"
			};
			menuItem4.Click += rightClickHandler[3];
			menuItem4.Icon = new Image {
				Source = Resources.mapEditMenu.Convert(ImageFormat.Png)
			};

			var menuItem5 = new MenuItem {
				Header = "Close"
			};
			menuItem5.Click += rightClickHandler[4];
			menuItem5.Icon = new Image {
				Source = Resources.mapEditMenu.Convert(ImageFormat.Png)
			};

			menu.Items.Add(menuItem1);
			menu.Items.Add(menuItem2);
			menu.Items.Add(menuItem3);
			menu.Items.Add(menuItem4);
			menu.Items.Add(menuItem5);

			return menu;
		}

		public static void GetMapDimensions(WzImage mapImage, out XNA.Rectangle VR, out XNA.Point mapCenter, out XNA.Point mapSize,
			out XNA.Point minimapCenter, out XNA.Point minimapSize, out bool hasVR, out bool hasMinimap) {
			var vr = MapInfo.GetVR(mapImage);
			hasVR = vr.HasValue;
			hasMinimap = mapImage["miniMap"] != null;
			if (!hasMinimap) {
				// No minimap, generate sizes from VR
				if (vr == null)
					// No minimap and no VR, our only chance of getting sizes is by generating a VR, if that fails we're screwed
				{
					if (!GetMapVR(mapImage, ref vr)) {
						throw new NoVRException();
					}
				}

				minimapSize = new XNA.Point(vr.Value.Width + 10, vr.Value.Height + 10); //leave 5 pixels on each side
				minimapCenter = new XNA.Point(5 - vr.Value.Left, 5 - vr.Value.Top);
				mapSize = new XNA.Point(minimapSize.X, minimapSize.Y);
				mapCenter = new XNA.Point(minimapCenter.X, minimapCenter.Y);
			} else {
				var miniMap = mapImage["miniMap"];
				minimapSize = new XNA.Point(InfoTool.GetInt(miniMap["width"]), InfoTool.GetInt(miniMap["height"]));
				minimapCenter = new XNA.Point(InfoTool.GetInt(miniMap["centerX"]), InfoTool.GetInt(miniMap["centerY"]));
				int topOffs = 0, botOffs = 0, leftOffs = 0, rightOffs = 0;
				int leftTarget = 69 - minimapCenter.X,
					topTarget = 86 - minimapCenter.Y,
					rightTarget = minimapSize.X - 69 - 69,
					botTarget = minimapSize.Y - 86 - 86;
				if (vr == null) {
					// We have no VR info, so set all VRs according to their target
					vr = new Rectangle(leftTarget, topTarget, rightTarget, botTarget);
				} else {
					if (vr.Value.Left < leftTarget) leftOffs = leftTarget - vr.Value.Left;

					if (vr.Value.Top < topTarget) topOffs = topTarget - vr.Value.Top;

					if (vr.Value.Right > rightTarget) rightOffs = vr.Value.Right - rightTarget;

					if (vr.Value.Bottom > botTarget) botOffs = vr.Value.Bottom - botTarget;
				}

				mapSize = new XNA.Point(minimapSize.X + leftOffs + rightOffs, minimapSize.Y + topOffs + botOffs);
				mapCenter = new XNA.Point(minimapCenter.X + leftOffs, minimapCenter.Y + topOffs);
			}

			VR = new XNA.Rectangle(vr.Value.X, vr.Value.Y, vr.Value.Width, vr.Value.Height);
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
			string categoryName, WzSubProperty strMapProp, TabControl Tabs,
			MultiBoard multiBoard, RoutedEventHandler[] rightClickHandler) {
			if (!mapImage.Parsed) {
				mapImage.ParseImage();
			}

			var copyPropNames = VerifyMapPropsKnown(mapImage, false);

			var info = new MapInfo(mapImage, mapName, streetName, categoryName);
			foreach (var copyPropName in copyPropNames) info.additionalNonInfoProps.Add(mapImage[copyPropName]);

			var type = GetMapType(mapImage);
			if (type == MapType.RegularMap) {
				info.id = int.Parse(WzInfoTools.RemoveLeadingZeros(WzInfoTools.RemoveExtension(mapImage.Name)));
			}

			info.mapType = type;

			var VR = new XNA.Rectangle();
			var center = new XNA.Point();
			var size = new XNA.Point();
			var minimapSize = new XNA.Point();
			var minimapCenter = new XNA.Point();
			var hasMinimap = false;
			var hasVR = false;

			try {
				GetMapDimensions(mapImage, out VR, out center, out size, out minimapCenter, out minimapSize, out hasVR,
					out hasMinimap);
			} catch (NoVRException) {
				MessageBox.Show(
					"Error - map does not contain size information and HaCreator was unable to generate it. An error has been logged.",
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				ErrorLogger.Log(ErrorLevel.IncorrectStructure, "no size @map " + info.id);
				return;
			}

			lock (multiBoard) {
				CreateMap(streetName, mapName, mapId, false,
					WzInfoTools.RemoveLeadingZeros(WzInfoTools.RemoveExtension(mapImage.Name)),
					CreateStandardMapMenu(rightClickHandler), size, center, Tabs, multiBoard);

				var mapBoard = multiBoard.SelectedBoard;
				mapBoard.Loading = true; // prevents TS Change callbacks
				mapBoard.MapInfo = info;
				if (hasMinimap) {
					mapBoard.MiniMap = ((WzCanvasProperty) mapImage["miniMap"]["canvas"]).GetLinkedWzCanvasBitmap();
					var mmPos = new Point(-minimapCenter.X, -minimapCenter.Y);
					mapBoard.MinimapPosition = mmPos;
					mapBoard.MinimapRectangle = new MinimapRectangle(mapBoard,
						new XNA.Rectangle(mmPos.X, mmPos.Y, minimapSize.X, minimapSize.Y));
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
		public static void CreateMap(string streetName, string mapName, int mapId, bool isNewMap, string tooltip,
			ContextMenu menu, XNA.Point size, XNA.Point center, TabControl Tabs,
			MultiBoard multiBoard) {
			lock (multiBoard) {
				var newBoard = multiBoard.CreateBoard(size, center, menu, isNewMap);
				GenerateDefaultZms(newBoard);

				var newTabPage = new TabItem {
					Header = GetFormattedMapNameForTabItem(mapId, streetName, mapName)
				};
				newTabPage.MouseRightButtonUp += (sender, e) => {
					var senderTab = (TabItem) sender;

					menu.PlacementTarget = senderTab;
					menu.IsOpen = true;
				};

				newBoard.TabPage = newTabPage;
				newTabPage.Tag = new TabItemContainer(mapName, multiBoard, tooltip, menu, newBoard); //newBoard;
				Tabs.Items.Add(newTabPage);
				Tabs.SelectedItem = newTabPage;

				multiBoard.SelectedBoard = newBoard;
				menu.Tag = newBoard;
				foreach (MenuItem item in menu.Items) item.Tag = newTabPage;

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

		public static void CreateMapFromHam(MultiBoard multiBoard, TabControl Tabs, string data,
			RoutedEventHandler[] rightClickHandler) {
			CreateMap("", "", -1, true, "", CreateStandardMapMenu(rightClickHandler), new XNA.Point(), new XNA.Point(), Tabs,
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