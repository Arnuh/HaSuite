namespace MapleLib.WzLib.WzStructure.Data {
	public static class PortalType {
		public const int
			StartPoint = 0,
			Invisible = 1,
			Visible = 2,
			Collision = 3,
			Changeable = 4,
			ChangeableInvisible = 5,
			TownPortalPoint = 6,
			Script = 7,
			ScriptInvisible = 8,
			CollisionScript = 9,
			Hidden = 10,
			ScriptHidden = 11,
			CollisionVerticalJump = 12,
			CollisionCustomImpact = 13,
			CollisionUnknownPcig = 14,
			ScriptHiddenUng = 15;

		public static class Names {
			public const string
				StartPoint = "sp",
				Invisible = "pi",
				Visible = "pv",
				Collision = "pc",
				Changeable = "pg",
				ChangeableInvisible = "pgi",
				TownPortalPoint = "tp",
				Script = "ps",
				ScriptInvisible = "psi",
				CollisionScript = "pcs",
				Hidden = "ph",
				ScriptHidden = "psh",
				CollisionVerticalJump = "pcj",
				CollisionCustomImpact = "pci",
				CollisionUnknownPcig = "pcig",
				ScriptHiddenUng = "pshg";
		}
	}
}