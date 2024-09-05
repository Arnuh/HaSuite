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
		public Dictionary<string, string> NPCs = new();
		public Dictionary<string, string> Mobs = new();
		public Dictionary<string, ReactorInfo> Reactors = new();
		public Dictionary<string, WzImage> TileSets = new();
		public Dictionary<string, WzImage> ObjectSets = new();

		public Dictionary<string, WzImage> BackgroundSets = new();
		public Dictionary<string, WzSoundProperty> BGMs = new();

		public Dictionary<string, Bitmap> MapMarks = new();
		public Dictionary<string, Tuple<string, string>> Maps = new();

		public Dictionary<int, PortalInfo> Portals = new();

		public List<string> PortalTypeById = new();

		public Dictionary<string, int> PortalIdByType = new() {
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

		public Dictionary<string, PortalGameImageInfo> GamePortals = new();

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