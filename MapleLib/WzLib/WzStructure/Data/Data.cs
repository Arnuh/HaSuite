/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2009, 2010, 2015 Snow and haha01haha01

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

using System.Collections.Generic;

namespace MapleLib.WzLib.WzStructure.Data {
	public static class Tables {
		public static Dictionary<string, string> PortalTypeNames = new Dictionary<string, string> {
			{PortalType.Names.StartPoint, "Start Point"},
			{PortalType.Names.Invisible, "Invisible"},
			{PortalType.Names.Visible, "Visible"},
			{PortalType.Names.Collision, "Collision"},
			{PortalType.Names.Changeable, "Changeable"},
			{PortalType.Names.ChangeableInvisible, "Changeable Invisible"},
			{PortalType.Names.TownPortalPoint, "Town Portal"},
			{PortalType.Names.Script, "Script"},
			{PortalType.Names.ScriptInvisible, "Script Invisible"},
			{PortalType.Names.CollisionScript, "Script Collision"},
			{PortalType.Names.Hidden, "Hidden"},
			{PortalType.Names.ScriptHidden, "Script Hidden"},
			{PortalType.Names.CollisionVerticalJump, "Vertical Spring"},
			{PortalType.Names.CollisionCustomImpact, "Custom Impact Spring"},
			{PortalType.Names.CollisionUnknownPcig, "Unknown (PCIG)"},
			{PortalType.Names.ScriptHiddenUng, "Unknown Script Hidden"}
		};

		public static string[] BackgroundTypeNames = {
			"Regular",
			"Horizontal Copies",
			"Vertical Copies",
			"H+V Copies",
			"Horizontal Moving+Copies",
			"Vertical Moving+Copies",
			"H+V Copies, Horizontal Moving",
			"H+V Copies, Vertical Moving"
		};
	}
}