/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Drawing;
using HaCreator.MapEditor.Info;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure.Data;

namespace HaCreator.Wz {
	public class WzInformationManager {
		public Dictionary<string, string> NPCs = new Dictionary<string, string>();
		public Dictionary<string, string> Mobs = new Dictionary<string, string>();
		public Dictionary<string, ReactorInfo> Reactors = new Dictionary<string, ReactorInfo>();
		public Dictionary<string, WzImage> TileSets = new Dictionary<string, WzImage>();
		public Dictionary<string, WzImage> ObjectSets = new Dictionary<string, WzImage>();

		public Dictionary<string, WzImage> BackgroundSets = new Dictionary<string, WzImage>();
		public Dictionary<string, WzBinaryProperty> BGMs = new Dictionary<string, WzBinaryProperty>();

		public Dictionary<string, Bitmap> MapMarks = new Dictionary<string, Bitmap>();
		public Dictionary<string, Tuple<string, string>> Maps = new Dictionary<string, Tuple<string, string>>();

		public Dictionary<int, PortalInfo> Portals = new Dictionary<int, PortalInfo>();

		public List<string> PortalTypeById = new List<string>();

		public Dictionary<string, int> PortalIdByType = new Dictionary<string, int> {
			{PortalType.Names.StartPoint, PortalType.StartPoint},
			{PortalType.Names.Invisible, PortalType.Invisible},
			{PortalType.Names.Visible, PortalType.Visible},
			{PortalType.Names.Collision, PortalType.Collision},
			{PortalType.Names.Changeable, PortalType.Changeable},
			{PortalType.Names.ChangeableInvisible, PortalType.ChangeableInvisible},
			{PortalType.Names.TownPortalPoint, PortalType.TownPortalPoint},
			{PortalType.Names.Script, PortalType.Script},
			{PortalType.Names.ScriptInvisible, PortalType.ScriptInvisible},
			{PortalType.Names.CollisionScript, PortalType.CollisionScript},
			{PortalType.Names.Hidden, PortalType.Hidden},
			{PortalType.Names.ScriptHidden, PortalType.ScriptHidden},
			{PortalType.Names.CollisionVerticalJump, PortalType.CollisionVerticalJump},
			{PortalType.Names.CollisionCustomImpact, PortalType.CollisionCustomImpact},
			{PortalType.Names.CollisionUnknownPcig, PortalType.CollisionUnknownPcig},
			{PortalType.Names.ScriptHiddenUng, PortalType.ScriptHiddenUng}
		};

		public Dictionary<string, PortalGameImageInfo> GamePortals = new Dictionary<string, PortalGameImageInfo>();

		/// <summary>
		/// Clears existing data loaded
		/// </summary>
		public void Clear() {
			NPCs.Clear();
			Mobs.Clear();
			Reactors.Clear();
			TileSets.Clear();
			ObjectSets.Clear();
			BackgroundSets.Clear();
			BGMs.Clear();
			MapMarks.Clear();
			Maps.Clear();
			Portals.Clear();
			PortalTypeById.Clear();
			//PortalIdByType.Clear();
			GamePortals.Clear();
		}
	}
}