/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Drawing;
using HaCreator.MapEditor.Info.Default;
using HaCreator.MapEditor.Instance;
using HaSharedLibrary.Wz;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure.Data;

namespace HaCreator.MapEditor.Info {
	public class PortalInfo : MapleDrawableInfo {
		private string type;

		public PortalInfo(string type, Bitmap image, Point origin, WzObject parentObject)
			: base(image, origin, parentObject) {
			this.type = type;
		}

		public static PortalInfo Load(WzCanvasProperty parentObject) {
			var portal = new PortalInfo(
				parentObject.Name,
				parentObject.GetLinkedWzCanvasBitmap(),
				WzInfoTools.PointFToSystemPoint(parentObject.GetCanvasOriginPosition()), parentObject);
			Program.InfoManager.Portals.Add(portal.type, portal);
			return portal;
		}

		public override BoardItem CreateInstance(Layer layer, Board board, int x, int y, int z, bool flip) {
			switch (type) {
				case PortalType.StartPoint: {
					return new PortalInstance(this, board, x, y, "sp", type, "", MapConstants.MaxMap, Defaults.Portal.Script,
						Defaults.Portal.Delay,
						Defaults.Portal.HideTooltip, Defaults.Portal.OnlyOnce, Defaults.Portal.HorizontalImpact,
						Defaults.Portal.VerticalImpact, Defaults.Portal.Image, Defaults.Portal.HRange,
						Defaults.Portal.VRange);
				}
				case PortalType.Invisible:
				case PortalType.Visible:
				case PortalType.Collision:
				case PortalType.Changeable:
				case PortalType.ChangeableInvisible:
				case PortalType.Hidden:
				case PortalType.CollisionVerticalJump:
				case PortalType.CollisionCustomImpact:
				case PortalType.CollisionUnknownPcig:
				case PortalType.ScriptHiddenUng: { // TODO
					return new PortalInstance(this, board, x, y, "portal", type, "", MapConstants.MaxMap, Defaults.Portal.Script,
						Defaults.Portal.Delay,
						Defaults.Portal.HideTooltip, Defaults.Portal.OnlyOnce, Defaults.Portal.HorizontalImpact,
						Defaults.Portal.VerticalImpact, Defaults.Portal.Image, Defaults.Portal.HRange,
						Defaults.Portal.VRange);
				}
				case PortalType.TownPortalPoint: {
					return new PortalInstance(this, board, x, y, "tp", type, "", MapConstants.MaxMap, Defaults.Portal.Script,
						Defaults.Portal.Delay,
						Defaults.Portal.HideTooltip, Defaults.Portal.OnlyOnce, Defaults.Portal.HorizontalImpact,
						Defaults.Portal.VerticalImpact, Defaults.Portal.Image, Defaults.Portal.HRange,
						Defaults.Portal.VRange);
				}
				case PortalType.Script:
				case PortalType.ScriptInvisible:
				case PortalType.CollisionScript:
				case PortalType.ScriptHidden: {
					return new PortalInstance(this, board, x, y, "portal", type, "", MapConstants.MaxMap, "script",
						Defaults.Portal.Delay, Defaults.Portal.HideTooltip, Defaults.Portal.OnlyOnce,
						Defaults.Portal.HorizontalImpact,
						Defaults.Portal.VerticalImpact, Defaults.Portal.Image,
						Defaults.Portal.HRange, Defaults.Portal.VRange);
				}
				default:
					throw new Exception("unknown pt @ CreateInstance, type: " + type);
			}
		}

		public PortalInstance CreateInstance(Board board, int x, int y, string pn, string tn, int tm, string script,
			int delay, bool hideTooltip, bool onlyOnce, int horizontalImpact, int verticalImpact,
			string image, int hRange, int vRange) {
			return new PortalInstance(this, board, x, y, pn, type, tn, tm, script, delay, hideTooltip, onlyOnce,
				horizontalImpact, verticalImpact, image, hRange, vRange);
		}

		public string Type => type;

		public static PortalInfo GetPortalInfoByType(string type) {
			return Program.InfoManager.Portals[type];
		}
	}
}