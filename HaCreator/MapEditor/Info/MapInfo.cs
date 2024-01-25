/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C)
 * 2009, 2010, 2015 Snow and haha01haha01
 * 2020 lastbattle

 * This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using System;
using System.Collections.Generic;
using System.Drawing;
using HaCreator.MapEditor.Info.Default;
using MapleLib.Helpers;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure.Data;
using MapleLib.WzLib.WzStructure.Data.MapStructure;

namespace MapleLib.WzLib.WzStructure {
	public class MapInfo //Credits to Bui for some of the info
	{
		public static MapInfo Default = new MapInfo();

		private WzImage image = null;

		//Editor related, not actual properties
		public MapType mapType = MapType.RegularMap;


		#region Wz related Properties

		//Cannot change
		public int version = 10;

		// Other properties that is not supported by HaCreator yet, but still gets dumped back when saving
		public readonly List<WzImageProperty> unsupportedInfoProperties = new List<WzImageProperty>();

		//Must have
		public string bgm = "Bgm00/GoPicnic";
		public string mapMark = "None";
		public long fieldLimit = 0; // FieldLimitType a | FieldLimitType b | etc
		public int returnMap = Defaults.Info.InvalidMap;
		public int forcedReturn = Defaults.Info.InvalidMap;
		public bool cloud = false;
		public bool swim = false;
		public bool hideMinimap = false;
		public bool town = false;
		public float mobRate = Defaults.Info.MobRate;

		//Optional
		public bool VRLimit = Defaults.Info.VRLimit; //use vr's as limits?
		public int VRTop = 0, VRBottom = 0, VRLeft = 0, VRRight = 0;
		public int timeLimit = Defaults.Info.TimeLimit;
		public int lvLimit = Defaults.Info.LvLimit;
		public FieldType fieldType = Defaults.Info.FieldTypeDefault;
		public string onFirstUserEnter = Defaults.Info.OnFirstUserEnter;
		public string onUserEnter = Defaults.Info.OnUserEnter;
		public bool fly = Defaults.Info.Fly;
		public bool noMapCmd = Defaults.Info.NoMapCmd;
		public bool partyOnly = Defaults.Info.PartyOnly;
		public bool reactorShuffle = Defaults.Info.ReactorShuffle;
		public string reactorShuffleName = Defaults.Info.ReactorShuffleName;
		public bool personalShop = Defaults.Info.PersonalShop;
		public bool entrustedShop = Defaults.Info.EntrustedShop;
		public string effect = Defaults.Info.Effect; //Bubbling; 610030550 and many others
		public int lvForceMove = Defaults.Info.LvForceMove; //limit FROM value
		public TimeMob? timeMob;
		public string help = Defaults.Info.Help; //help string
		public bool snow = Defaults.Info.Snow;
		public bool rain = Defaults.Info.Rain;
		public int dropExpire = Defaults.Info.DropExpire; //in seconds
		public int decHP = Defaults.Info.DecHP;
		public int decInterval = Defaults.Info.DecInterval;
		public AutoLieDetector autoLieDetector;
		public bool expeditionOnly = Defaults.Info.ExpeditionOnly;
		public float fs = Defaults.Info.FS; //slip on ice speed, default 0.2
		public int protectItem = Defaults.Info.ProtectItem; //ID, item protecting from cold
		public int createMobInterval = Defaults.Info.CreateMobInterval; //used for massacre pqs
		public int fixedMobCapacity = Defaults.Info.FixedMobCapacity; //mob capacity to target (used for massacre pqs)
		public bool miniMapOnOff = Defaults.Info.MiniMapOnOff;
		public bool noRegenMap = Defaults.Info.NoRegenMap; //610030400
		public List<int> allowedItem = null;
		public float recovery = Defaults.Info.Recovery; //recovery rate, like in sauna (3)
		public bool blockPBossChange = Defaults.Info.BlockPBossChange; //something with monster carnival
		public bool everlast = Defaults.Info.Everlast; //something with bonus stages of PQs
		public bool damageCheckFree = Defaults.Info.DamageCheckFree; //something with fishing event
		public float dropRate = Defaults.Info.DropRate;
		public bool scrollDisable = Defaults.Info.ScrollDisable;
		public bool needSkillForFly = Defaults.Info.NeedSkillForFly;
		public bool zakum2Hack = Defaults.Info.Zakum2Hack; //JQ hack protection
		public bool allMoveCheck = Defaults.Info.AllMoveCheck; //another JQ hack protection
		public bool consumeItemCoolTime = Defaults.Info.ConsumeItemCoolTime; //cool time of consume item
		public bool zeroSideOnly = Defaults.Info.ZeroSideOnly; // true if its zero's temple map
		public int moveLimit = Defaults.Info.MoveLimit;

		public bool mirror_Bottom = Defaults.Info.MirrorBottom; // Mirror Bottom (Reflection for objects near VRBottom of the field, arcane river maps)

		//Unknown optional
		public string mapDesc = "";
		public string mapName = "";
		public string streetName = "";

		//Special
		public List<WzImageProperty> additionalProps = new List<WzImageProperty>();
		public List<WzImageProperty> additionalNonInfoProps = new List<WzImageProperty>();
		public string strMapName = "<Untitled>";
		public string strStreetName = "<Untitled>";
		public string strCategoryName = "HaCreator";
		public int id = 0;

		#endregion


		/// <summary>
		/// Empty Constructor
		/// </summary>
		public MapInfo() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="image"></param>
		/// <param name="strMapName"></param>
		/// <param name="strStreetName"></param>
		/// <param name="strCategoryName"></param>
		public MapInfo(WzImage image, string strMapName, string strStreetName, string strCategoryName) {
			this.image = image;
			int startHour;
			int endHour;
			this.strMapName = strMapName;
			this.strStreetName = strStreetName;
			this.strCategoryName = strCategoryName;
			var file = image.WzFileParent;
			var loggerSuffix = ", map " + image.Name + (file != null
				? " of version " + Enum.GetName(typeof(WzMapleVersion), file.MapleVersion) + ", v" +
				  file.Version
				: "");

			foreach (var prop in image["info"].WzProperties) {
				switch (prop.Name) {
					case "bgm":
						bgm = InfoTool.GetString(prop);
						break;
					case "cloud":
						cloud = prop.GetBool();
						break;
					case "swim":
						swim = prop.GetBool();
						break;
					case "forcedReturn":
						forcedReturn = prop.GetInt();
						break;
					case "hideMinimap":
						hideMinimap = prop.GetBool();
						break;
					case "mapDesc":
						mapDesc = InfoTool.GetString(prop);
						break;
					case "mapName":
						mapName = InfoTool.GetString(prop);
						break;
					case "mapMark":
						mapMark = InfoTool.GetString(prop);
						break;
					case "mobRate":
						mobRate = InfoTool.GetFloat(prop);
						break;
					case "moveLimit":
						moveLimit = InfoTool.GetInt(prop);
						break;
					case "returnMap":
						returnMap = InfoTool.GetInt(prop);
						break;
					case "town":
						town = InfoTool.GetBool(prop);
						break;
					case "version":
						version = InfoTool.GetInt(prop);
						break;
					case "fieldLimit":
						var fl = InfoTool.GetLong(prop);
						fieldLimit = fl;
						break;
					case "VRTop":
						VRTop = InfoTool.GetOptionalInt(prop, 0);
						break;
					case "VRBottom":
						VRBottom = InfoTool.GetOptionalInt(prop, 0);
						break;
					case "VRLeft":
						VRLeft = InfoTool.GetOptionalInt(prop, 0);
						break;
					case "VRRight":
						VRRight = InfoTool.GetOptionalInt(prop, 0);
						break;
					case "link":
						//link = InfoTool.GetInt(prop);
						break;
					case "timeLimit":
						timeLimit = InfoTool.GetInt(prop);
						break;
					case "lvLimit":
						lvLimit = InfoTool.GetInt(prop);
						break;
					case "onFirstUserEnter":
						onFirstUserEnter = InfoTool.GetString(prop);
						break;
					case "onUserEnter":
						onUserEnter = InfoTool.GetString(prop);
						break;
					case "fly":
						fly = InfoTool.GetBool(prop);
						break;
					case "noMapCmd":
						noMapCmd = InfoTool.GetBool(prop);
						break;
					case "partyOnly":
						partyOnly = InfoTool.GetBool(prop);
						break;
					case "fieldType":
						var ft = InfoTool.GetInt(prop);
						if (!Enum.IsDefined(typeof(FieldType), ft)) {
							ErrorLogger.Log(ErrorLevel.IncorrectStructure,
								"Invalid fieldType " + ft + loggerSuffix);
							ft = 0;
						}

						fieldType = (FieldType) ft;
						break;
					case "miniMapOnOff":
						miniMapOnOff = InfoTool.GetBool(prop);
						break;
					case "reactorShuffle":
						reactorShuffle = InfoTool.GetBool(prop);
						break;
					case "reactorShuffleName":
						reactorShuffleName = InfoTool.GetString(prop);
						break;
					case "personalShop":
						personalShop = InfoTool.GetBool(prop);
						break;
					case "entrustedShop":
						entrustedShop = InfoTool.GetBool(prop);
						break;
					case "effect":
						effect = InfoTool.GetString(prop);
						break;
					case "lvForceMove":
						lvForceMove = InfoTool.GetInt(prop);
						break;
					case "timeMob":
						startHour = prop["startHour"].GetOptionalInt(Defaults.TimeMob.StartHour);
						endHour = prop["endHour"].GetOptionalInt(Defaults.TimeMob.EndHour);
						var id = prop["id"].GetInt();
						var message = prop["message"].GetOptionalString(Defaults.TimeMob.Message);
						if (message == null) {
							ErrorLogger.Log(ErrorLevel.IncorrectStructure, "timeMob" + loggerSuffix);
						} else {
							timeMob = new TimeMob(startHour, endHour, id, message);
						}

						break;
					case "help":
						help = InfoTool.GetString(prop);
						break;
					case "snow":
						snow = InfoTool.GetBool(prop);
						break;
					case "rain":
						rain = InfoTool.GetBool(prop);
						break;
					case "dropExpire":
						dropExpire = InfoTool.GetInt(prop);
						break;
					case "decHP":
						decHP = InfoTool.GetInt(prop);
						break;
					case "decInterval":
						decInterval = InfoTool.GetInt(prop);
						break;
					case "autoLieDetector":
						startHour = InfoTool.GetInt(prop["startHour"]);
						endHour = InfoTool.GetInt(prop["endHour"]);
						var interval = InfoTool.GetInt(prop["interval"]);
						var propInt = InfoTool.GetInt(prop["prop"]);
						autoLieDetector = new AutoLieDetector(startHour, endHour, interval, propInt);
						break;
					case "expeditionOnly":
						expeditionOnly = InfoTool.GetBool(prop);
						break;
					case "fs":
						fs = InfoTool.GetFloat(prop);
						break;
					case "protectItem":
						protectItem =
							InfoTool.GetInt(
								prop); // could also be a WzSubProperty in later versions.  "Map002.wz\\Map\\Map2\\211000200.img\\info\\protectItem"
						break;
					case "createMobInterval":
						createMobInterval = InfoTool.GetInt(prop);
						break;
					case "fixedMobCapacity":
						fixedMobCapacity = InfoTool.GetInt(prop);
						break;
					case "streetName":
						streetName = InfoTool.GetString(prop);
						break;
					case "noRegenMap":
						noRegenMap = InfoTool.GetBool(prop);
						break;
					case "allowedItem":
						allowedItem = new List<int>();
						if (prop.WzProperties != null && prop.WzProperties.Count > 0) {
							foreach (var item in prop.WzProperties)
								allowedItem.Add(item.GetInt());
						}

						break;
					case "recovery":
						recovery = InfoTool.GetFloat(prop);
						break;
					case "blockPBossChange":
						blockPBossChange = InfoTool.GetBool(prop);
						break;
					case "everlast":
						everlast = InfoTool.GetBool(prop);
						break;
					case "damageCheckFree":
						damageCheckFree = InfoTool.GetBool(prop);
						break;
					case "dropRate":
						dropRate = InfoTool.GetFloat(prop);
						break;
					case "scrollDisable":
						scrollDisable = InfoTool.GetBool(prop);
						break;
					case "needSkillForFly":
						needSkillForFly = InfoTool.GetBool(prop);
						break;
					case "zakum2Hack":
						zakum2Hack = InfoTool.GetBool(prop);
						break;
					case "allMoveCheck":
						allMoveCheck = InfoTool.GetBool(prop);
						break;
					case "VRLimit":
						VRLimit = InfoTool.GetBool(prop);
						break;
					case "consumeItemCoolTime":
						consumeItemCoolTime = InfoTool.GetBool(prop);
						break;
					case "zeroSideOnly":
						zeroSideOnly = InfoTool.GetBool(prop);
						break;
					case "mirror_Bottom":
						mirror_Bottom = InfoTool.GetBool(prop);
						break;
					case "AmbientBGM":
					case "AmbientBGMv":
					case "areaCtrl":
					case "onlyUseSkill":
					case "limitUseShop":
					case "limitUseTrunk":
					case "freeFallingVX":
					case "midAirAccelVX":
					case "midAirDecelVX":
					case "jumpSpeedR":
					case "jumpAccUpKey":
					case "jumpAccDownKey":
					case "jumpApplyVX":
					case "dashSkill":
					case "speedMaxOver":
					case "speedMaxOver ": // with a space, stupid nexon
					case "isSpecialMoveCheck":
					case "forceSpeed":
					case "forceJump":
					case "forceUseIndie":
					case "vanishPet":
					case "vanishAndroid":
					case "vanishDragon":
					case "limitUI":
					case "largeSplit":
					case "qrLimit":
					case "noChair":
					case "fieldScript":
					case "standAlone":
					case "partyStandAlone":
					case "HobbangKing":
					case "quarterView":
					case "MRLeft":
					case "MRTop":
					case "MRRight":
					case "MRBottom":
					case "limitSpeedAndJump":
					case "directionInfo":
					case "LBSide":
					case "LBTop":
					case "LBBottom":
					case "bgmSub":
					case "fieldLimit2":
					case "fieldLimit_tw":
					case "particle":
					case "qrLimitJob":
					case "individualMobPool":
					case "barrier":
					case "remoteEffect":
					case "isFixDec":
					case "decHPr":
					case "isDecView":
					case "forceFadeInTime":
					case "forceFadeOutTime":
					case "qrLimitState":
					case "qrLimitState2":
					case "difficulty":
					case "alarm":
					case "bossMobID":
					case "reviveCurField":
					case "ReviveCurFieldOfNoTransfer":
					case "ReviveCurFieldOfNoTransferPoint":
					case "limitUserEffectField":
					case "nofollowCharacter":
					case "functionObjInfo":
					case "quest":
					case "ridingMove":
					case "ridingField":
					case "noLanding":
					case "noCancelSkill":
					case "defaultAvatarDir":
					case "footStepSound":
					case "taggedObjRegenInfo":
					case "offSoulAbsorption":
					case "canPartyStatChangeIgnoreParty":
					case "canPartyStatChangeIgnoreParty ": // with a space
					case "forceReturnOnDead":
					case "zoomOutField":
					case "EscortMinTime":
					case "deathCount":
					case "onEnterResetFifthSkill":
					case "potionLimit":
					case "lifeCount":
					case "respawnCooltimeField":
					case "scale":
					case "skills":
					case "noResurection":
					case "incInterval":
					case "incMPr":
					case "bonusExpPerUserHPRate":
					case "barrierArc":
					case "noBackOverlapped":
					case "partyBonusR":
					case "specialSound":
					case "inFieldsetForcedReturn":
					case "chaser":
					case "chaserEndTime":
					case "chaserEffect":
					case "cagePotal":
					case "cageLT":
					case "cageRB":
					case "chaserHoldTime":
					case "phase":
					case "boss":
					case "actionBarIdx":
					case "shadowzone":
					case "towerChairEnable":
					case "DiceMasterUpIndex":
					case "DiceMasterDownIndex":
					case "ChangeName":
					case "entrustedFishing":
					case "forcedScreenMode":
					case "isHideUI":
					case "resetRidingField":
					case "PL_Gunman":
					case "noHekatonEffect":
					case "mode":
					case "skill":
					case "MR":
					case "ratemob":
					case "bonusStageNoChangeBack":
					case "questAmbience":
					case "blockScriptItem":
					case "remoteTranslucenceView":
					case "rewardContentName":
					case "specialDeadAction":
					case "FriendsStoryBossDelay":
					case "soulFieldType":
					case "cameraMoveY":
					case "subType":
					case "playTime":
					case "noEvent":
					case "forParty":
					case "whiteOut":
					case "UserInvisible":
					case "OnFirstUserEnter": // the same as onFirstUserEnter
					case "teamPortal":
					case "typingGame":
					case "lt":
					case "rb":
					case "fieldAttackObj":
					case "timeLeft":
					case "timeLeft_d":
					case "eventSummon":
					case "rankCheckMob":
					case "questCheckMob":
					case "bindSkillLimit":
					case "NoMobCapacityLimit":
					case "userTimer":
					case "eventChairIndex":
					case "onlyEventChair":
					case "waitReviveTime":
					case "RidingTop":
					case "temporarySkill":
					case "largeSplt":
					case "blockTakeOffItem":
					case "PartyOnly":
					case "climb":
					case "bulletConsume":
					case "gaugeDelay":
					case "individualPet":
					case "level":
					case "hungryMuto":
					case "property": //  map 921172300.img
					case "spiritSavior":
					case "standAlonePermitUpgrade": //  993059600.img
					case "limitHeadAlarmField": // 993180000.img
					case "noPsychicGrab": // 993194700.img
					case "offSoulEffect": // 993194700.img
					case "offActiveEffectItem": // 993194700.img
					case "forceDamageSkinID": // 993194700.img
					case "minimapFullSizeViewBlock": // 993194700.img
					case "limitUIContextMenu": // 993210400.img  993220101.img
					case "vanishHaku": // 993194700.img
					case "pulbicTaggedObjectVisible": // 993210000.img 
					{
						var cloneProperty = prop.DeepClone();

						//cloneProperty.Parent = prop.Parent;
						unsupportedInfoProperties.Add(cloneProperty);
						break;
					}
					default:
						ErrorLogger.Log(ErrorLevel.MissingFeature,
							string.Format(
								"[MapInfo] Unknown field info/ property: '{0}'. {1}. Please fix it at MapInfo.cs",
								prop.Name, loggerSuffix));
						additionalProps.Add(prop.DeepClone());
						break;
				}
			}
		}

		public static Rectangle? GetVR(WzImage image) {
			Rectangle? result = null;
			if (image["info"]["VRLeft"] != null) {
				var info = image["info"];
				var left = InfoTool.GetInt(info["VRLeft"]);
				var right = InfoTool.GetInt(info["VRRight"]);
				var top = InfoTool.GetInt(info["VRTop"]);
				var bottom = InfoTool.GetInt(info["VRBottom"]);
				result = new Rectangle(left, top, right - left, bottom - top);
			}

			return result;
		}

		/// <summary>
		/// Save MapInfo variables to WzImage
		/// </summary>
		/// <param name="dest"></param>
		/// <param name="VR"></param>
		public void Save(WzImage dest, Rectangle? VR) {
			var info = new WzSubProperty();
			info["version"] = InfoTool.SetInt(version);
			info["cloud"] = InfoTool.SetBool(cloud);
			info["town"] = InfoTool.SetBool(town);
			info["mobRate"] = InfoTool.SetFloat(mobRate);
			info["bgm"] = InfoTool.SetString(bgm);
			info["returnMap"] = InfoTool.SetInt(returnMap);
			info["forcedReturn"] = InfoTool.SetInt(forcedReturn);
			info["hideMinimap"] = InfoTool.SetBool(hideMinimap);
			info["moveLimit"] = moveLimit.SetOptionalInt(Defaults.Info.MoveLimit);
			info["mapMark"] = InfoTool.SetString(mapMark);
			info["fieldLimit"] = InfoTool.SetInt((int) fieldLimit);
			info["swim"] = swim.SetOptionalBool(Defaults.Info.Swim);

			if (VR.HasValue) {
				info["VRLeft"] = InfoTool.SetInt(VR.Value.Left);
				info["VRRight"] = InfoTool.SetInt(VR.Value.Right);
				info["VRTop"] = InfoTool.SetInt(VR.Value.Top);
				info["VRBottom"] = InfoTool.SetInt(VR.Value.Bottom);
			} else {
				// ?
				info["VRTop"] = VRTop.SetOptionalInt(0);
				info["VRBottom"] = VRBottom.SetOptionalInt(0);
				info["VRLeft"] = VRLeft.SetOptionalInt(0);
				info["VRRight"] = VRRight.SetOptionalInt(0);
			}

			info["VRLimit"] = VRLimit.SetOptionalBool(Defaults.Info.VRLimit);

			info["mapDesc"] = InfoTool.SetOptionalString(mapDesc, Defaults.Info.MapDesc);
			info["mapName"] = InfoTool.SetOptionalString(mapName, Defaults.Info.MapName);
			info["streetName"] = InfoTool.SetOptionalString(streetName, Defaults.Info.StreetName);
			info["timeLimit"] = timeLimit.SetOptionalInt(Defaults.Info.TimeLimit);
			info["lvLimit"] = lvLimit.SetOptionalInt(Defaults.Info.LvLimit);
			info["onFirstUserEnter"] = InfoTool.SetOptionalString(onFirstUserEnter, Defaults.Info.OnFirstUserEnter);
			info["onUserEnter"] = InfoTool.SetOptionalString(onUserEnter, Defaults.Info.OnUserEnter);
			info["fly"] = fly.SetOptionalBool(Defaults.Info.Fly);
			info["noMapCmd"] = noMapCmd.SetOptionalBool(Defaults.Info.NoMapCmd);
			info["partyOnly"] = partyOnly.SetOptionalBool(Defaults.Info.PartyOnly);
			info["fieldType"] = ((int) fieldType).SetOptionalInt((int) Defaults.Info.FieldTypeDefault);
			info["miniMapOnOff"] = miniMapOnOff.SetOptionalBool(Defaults.Info.MiniMapOnOff);
			info["reactorShuffle"] = reactorShuffle.SetOptionalBool(Defaults.Info.ReactorShuffle);
			info["reactorShuffleName"] = InfoTool.SetOptionalString(reactorShuffleName, Defaults.Info.ReactorShuffleName);
			info["personalShop"] = personalShop.SetOptionalBool(Defaults.Info.PersonalShop);
			info["entrustedShop"] = entrustedShop.SetOptionalBool(Defaults.Info.EntrustedShop);
			info["effect"] = InfoTool.SetOptionalString(effect, Defaults.Info.Effect);
			info["lvForceMove"] = lvForceMove.SetOptionalInt(Defaults.Info.LvForceMove);
			info["mirror_Bottom"] = mirror_Bottom.SetOptionalBool(Defaults.Info.MirrorBottom);

			// Time mob
			if (timeMob != null) {
				var prop = new WzSubProperty();
				prop["startHour"] = timeMob.Value.startHour.SetOptionalInt(Defaults.TimeMob.StartHour);
				prop["endHour"] = timeMob.Value.endHour.SetOptionalInt(Defaults.TimeMob.EndHour);
				prop["id"] = InfoTool.SetInt(timeMob.Value.id);
				prop["message"] = InfoTool.SetOptionalString(timeMob.Value.message, Defaults.TimeMob.Message);
				info["timeMob"] = prop;
			}

			info["help"] = InfoTool.SetOptionalString(help, Defaults.Info.Help);
			info["snow"] = snow.SetOptionalBool(Defaults.Info.Snow);
			info["rain"] = rain.SetOptionalBool(Defaults.Info.Rain);
			info["dropExpire"] = dropExpire.SetOptionalInt(Defaults.Info.DropExpire);
			info["decHP"] = decHP.SetOptionalInt(Defaults.Info.DecHP);
			info["decInterval"] = decInterval.SetOptionalInt(Defaults.Info.DecInterval);

			// Lie detector
			if (autoLieDetector != null) {
				var prop = new WzSubProperty();
				prop["startHour"] = InfoTool.SetInt(autoLieDetector.startHour);
				prop["endHour"] = InfoTool.SetInt(autoLieDetector.endHour);
				prop["interval"] = InfoTool.SetInt(autoLieDetector.interval);
				prop["prop"] = InfoTool.SetInt(autoLieDetector.prop);
				info["autoLieDetector"] = prop;
			}

			info["expeditionOnly"] = expeditionOnly.SetOptionalBool(Defaults.Info.ExpeditionOnly);
			info["fs"] = InfoTool.SetOptionalFloat(fs, Defaults.Info.FS);
			info["protectItem"] = protectItem.SetOptionalInt(Defaults.Info.ProtectItem);
			info["createMobInterval"] = createMobInterval.SetOptionalInt(Defaults.Info.CreateMobInterval);
			info["fixedMobCapacity"] = fixedMobCapacity.SetOptionalInt(Defaults.Info.FixedMobCapacity);
			info["noRegenMap"] = noRegenMap.SetOptionalBool(Defaults.Info.NoRegenMap);
			if (allowedItem != null) {
				var prop = new WzSubProperty();
				for (var i = 0; i < allowedItem.Count; i++) prop[i.ToString()] = InfoTool.SetInt(allowedItem[i]);

				info["allowedItem"] = prop;
			}

			info["recovery"] = InfoTool.SetOptionalFloat(recovery, Defaults.Info.Recovery);
			info["blockPBossChange"] = blockPBossChange.SetOptionalBool(Defaults.Info.BlockPBossChange);
			info["everlast"] = everlast.SetOptionalBool(Defaults.Info.Everlast);
			info["damageCheckFree"] = damageCheckFree.SetOptionalBool(Defaults.Info.DamageCheckFree);
			info["dropRate"] = InfoTool.SetOptionalFloat(dropRate, Defaults.Info.DropRate);
			info["scrollDisable"] = scrollDisable.SetOptionalBool(Defaults.Info.ScrollDisable);
			info["needSkillForFly"] = needSkillForFly.SetOptionalBool(Defaults.Info.NeedSkillForFly);
			info["zakum2Hack"] = zakum2Hack.SetOptionalBool(Defaults.Info.Zakum2Hack);
			info["allMoveCheck"] = allMoveCheck.SetOptionalBool(Defaults.Info.AllMoveCheck);
			info["consumeItemCoolTime"] = consumeItemCoolTime.SetOptionalBool(Defaults.Info.ConsumeItemCoolTime);
			info["zeroSideOnly"] = zeroSideOnly.SetOptionalBool(Defaults.Info.ZeroSideOnly);
			foreach (var prop in additionalProps) info.AddProperty(prop);

			// Add back all unsupported properties
			info.AddProperties(unsupportedInfoProperties);

			//
			dest["info"] = info;
		}

		public WzImage Image {
			get => image;
			set => image = value;
		}

		public bool ShouldSerializeImage() {
			// To keep JSON.NET from serializing this
			return false;
		}
	}
}