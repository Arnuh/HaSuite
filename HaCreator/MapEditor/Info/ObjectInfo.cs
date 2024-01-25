/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Drawing;
using HaCreator.MapEditor.Info.Default;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.Instance.Shapes;
using HaSharedLibrary.Wz;
using MapleLib.Helpers;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Info {
	public class ObjectInfo : MapleDrawableInfo {
		private string _oS;
		private string _l0;
		private string _l1;
		private string _l2;
		private List<List<XNA.Point>> footholdOffsets = null;
		private List<List<XNA.Point>> ropeOffsets = null;
		private List<List<XNA.Point>> ladderOffsets = null;
		private List<XNA.Point> chairOffsets = null;
		private bool connect;

		public ObjectInfo(Bitmap image, Point origin, string oS, string l0, string l1, string l2,
			WzObject parentObject)
			: base(image, origin, parentObject) {
			_oS = oS;
			_l0 = l0;
			_l1 = l1;
			_l2 = l2;
			connect = oS.StartsWith("connect");
		}

		public static ObjectInfo Get(string oS, string l0, string l1, string l2) {
			if (!Program.InfoManager.ObjectSets.ContainsKey(oS)) {
				var logError = string.Format("Background object Map.wz/Obj/{0}/{1}/{2}/{3} not found.", oS, l0, l1,
					l2);
				ErrorLogger.Log(ErrorLevel.IncorrectStructure, logError);
				return null;
			}

			var objInfoProp = Program.InfoManager.ObjectSets[oS]?[l0]?[l1]?[l2];
			if (objInfoProp == null) {
				var logError = string.Format("Background object Map.wz/Obj/{0}/{1}/{2}/{3} not found.", oS, l0, l1,
					l2);
				ErrorLogger.Log(ErrorLevel.IncorrectStructure, logError);
				return null;
			}

			if (objInfoProp.HCTag == null) {
				try {
					objInfoProp.HCTag = Load((WzSubProperty) objInfoProp, oS, l0, l1, l2);
				} catch (KeyNotFoundException) {
					return null;
				}
			}

			return (ObjectInfo) objInfoProp.HCTag;
		}

		private static List<XNA.Point> ParsePropToOffsetList(WzImageProperty prop) {
			var result = new List<XNA.Point>();
			foreach (WzVectorProperty point in prop.WzProperties) result.Add(WzInfoTools.VectorToXNAPoint(point));

			return result;
		}

		private static List<List<XNA.Point>> ParsePropToOffsetMap(WzImageProperty prop) {
			if (prop == null) {
				return null;
			}

			var result = new List<List<XNA.Point>>();
			if (prop is WzConvexProperty) {
				result.Add(ParsePropToOffsetList((WzConvexProperty) prop));
			} else if (prop is WzSubProperty) {
				try {
					foreach (WzConvexProperty offsetSet in prop.WzProperties)
						result.Add(ParsePropToOffsetList(offsetSet));
				} catch (InvalidCastException) {
				}
			} else {
				result = null;
			}

			return result;
		}

		private static ObjectInfo Load(WzSubProperty parentObject, string oS, string l0, string l1, string l2) {
			var frame1 = (WzCanvasProperty) WzInfoTools.GetRealProperty(parentObject["0"]);
			var result = new ObjectInfo(
				frame1.GetLinkedWzCanvasBitmap(),
				WzInfoTools.PointFToSystemPoint(frame1.GetCanvasOriginPosition()),
				oS,
				l0,
				l1,
				l2,
				parentObject);
			var chairs = parentObject["seat"];
			var ropes = frame1["rope"];
			var ladders = frame1["ladder"];
			var footholds = frame1["foothold"];
			result.footholdOffsets = ParsePropToOffsetMap(footholds);
			result.ropeOffsets = ParsePropToOffsetMap(ropes);
			result.ladderOffsets = ParsePropToOffsetMap(ladders);
			if (chairs != null) {
				result.chairOffsets = ParsePropToOffsetList(chairs);
			}

			return result;
		}

		private void CreateFootholdsFromAnchorList(Board board, List<FootholdAnchor> anchors) {
			for (var i = 0; i < anchors.Count - 1; i++) {
				var fh = new FootholdLine(board, anchors[i], anchors[i + 1]);
				board.BoardItems.FootholdLines.Add(fh);
			}
		}

		public void ParseOffsets(ObjectInstance instance, Board board, int x, int y) {
			var ladder = l0 == "ladder";
			if (footholdOffsets != null) {
				foreach (var anchorList in footholdOffsets) {
					var anchors = new List<FootholdAnchor>();
					foreach (var foothold in anchorList) {
						var anchor = new FootholdAnchor(board, x + foothold.X, y + foothold.Y,
							instance.LayerNumber, instance.PlatformNumber, true);
						board.BoardItems.FHAnchors.Add(anchor);
						instance.BindItem(anchor, foothold);
						anchors.Add(anchor);
					}

					CreateFootholdsFromAnchorList(board, anchors);
				}
			}

			if (chairOffsets != null) {
				foreach (var chairPos in chairOffsets) {
					var chair = new Chair(board, x + chairPos.X, y + chairPos.Y);
					board.BoardItems.Chairs.Add(chair);
					instance.BindItem(chair, chairPos);
				}
			}
		}

		public override BoardItem CreateInstance(Layer layer, Board board, int x, int y, int z, bool flip) {
			var instance = new ObjectInstance(this, layer, board, x, y, z, layer.zMDefault, Defaults.Object.R, Defaults.Object.Hide,
				Defaults.Object.Reactor, Defaults.Object.Flow, Defaults.Object.RX, Defaults.Object.RY, Defaults.Object.CX,
				Defaults.Object.CY, Defaults.Object.Name, Defaults.Object.Tags, null, flip);
			ParseOffsets(instance, board, x, y);
			return instance;
		}

		public BoardItem CreateInstance(Layer layer, Board board, int x, int y, int z, int zM, bool r,
			bool hide, bool reactor, bool flow, int rx, int ry, int cx, int cy, string name,
			string tags, List<ObjectInstanceQuest> questInfo, bool flip, bool parseOffsets) {
			var instance = new ObjectInstance(this, layer, board, x, y, z, zM, r, hide, reactor, flow, rx,
				ry, cx, cy, name, tags, questInfo, flip);
			if (parseOffsets) ParseOffsets(instance, board, x, y);
			return instance;
		}

		public string oS {
			get => _oS;
			set => _oS = value;
		}

		public string l0 {
			get => _l0;
			set => _l0 = value;
		}

		public string l1 {
			get => _l1;
			set => _l1 = value;
		}

		public string l2 {
			get => _l2;
			set => _l2 = value;
		}

		public List<List<XNA.Point>> FootholdOffsets => footholdOffsets;

		public List<XNA.Point> ChairOffsets => chairOffsets;

		public List<List<XNA.Point>> RopeOffsets => ropeOffsets;

		public List<List<XNA.Point>> LadderOffsets => ladderOffsets;

		public bool Connect => connect;
	}
}