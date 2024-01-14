using MapleLib.WzLib.WzStructure.Data;

namespace HaCreator.MapEditor.Info.Default {
	public class Defaults {
		public static class Info {
			public const int InvalidMap = 999999999;
			public const float MobRate = 1f; // Unknown

			public const string MapName = "";
			public const string MapDesc = "";
			public const string StreetName = "";

			public const bool VRLimit = false;
			public const int TimeLimit = 0;
			public const int LvLimit = 0;
			public const FieldType FieldTypeDefault = FieldType.FIELDTYPE_DEFAULT;
			public const string OnFirstUserEnter = "";
			public const string OnUserEnter = "";
			public const bool Fly = false;
			public const bool NoMapCmd = false;
			public const bool PartyOnly = false;
			public const bool ReactorShuffle = false;
			public const string ReactorShuffleName = "";
			public const bool PersonalShop = false;
			public const bool EntrustedShop = false;
			public const string Effect = "";
			public const int LvForceMove = 0;
			public const string Help = "";
			public const bool Snow = false;
			public const bool Rain = false;
			public const bool Swim = false;
			public const int DropExpire = 0;
			public const int DecHP = 0;
			public const int DecInterval = 0;
			public const bool ExpeditionOnly = false;
			public const float FS = 1f;
			public const int ProtectItem = 0;
			public const int CreateMobInterval = 0;
			public const int FixedMobCapacity = 0;
			public const bool MiniMapOnOff = false;
			public const bool NoRegenMap = false;
			public const float Recovery = 1f;
			public const bool BlockPBossChange = false;
			public const bool Everlast = false;
			public const bool DamageCheckFree = false;
			public const float DropRate = 1f; // Unknown
			public const bool ScrollDisable = false;
			public const bool NeedSkillForFly = false; // Unknown
			public const bool Zakum2Hack = false;
			public const bool AllMoveCheck = false;
			public const bool ConsumeItemCoolTime = false; // Unknown
			public const bool ZeroSideOnly = false; // Unknown
			public const int MoveLimit = 0;
			public const bool MirrorBottom = false;
		}

		public static class Object {
			public const string Name = "";
			public const bool R = false;
			public const bool Hide = false;
			public const bool Reactor = false;
			public const bool Flow = false;
			public const int RX = 0, RY = 0, CX = 0, CY = 0;
			public const string Tags = "";
		}

		public static class Tile {
			public const int Mag = 1;
			public const int Z = 0;
		}

		public static class Background {
			public const bool Flip = false;

			public const bool Front = false;

			public const string SpineAni = "";
			public const bool SpineRandomStart = false;
		}

		public static class Foothold {
			public const bool CantThrough = false;
			public const bool ForbidFalldown = false;
			public const int Piece = 0;
			public const double Force = 0;
		}

		public static class Life {
			public const int MobTime = 0;
			public const int Team = -1;
			public const int Info = 0; // Unknown
			public const string LimitedName = "";
			public const bool F = false;
			public const bool Hide = false;
		}

		public static class Portal {
			public const string Script = "";
			public const bool HideTooltip = false;
			public const bool OnlyOnce = false;
			public const string Image = "";

			public const int VerticalImpact = 0; // Unknown
			public const int HorizontalImpact = 0; // Unknown
			public const int HRange = 100;
			public const int VRange = 100;
			public const int Delay = 0;
		}

		public static class Reactor {
			public const string Name = "";
		}

		public static class ShipObj {
			public const int ZValue = 0;
			public const int X0 = 0;
		}

		public static class MirrorData {
			public const string ObjectForOverlay = "";
			public const bool Reflection = false;
			public const bool AlphaTest = false;
		}

		public static class TimeMob {
			public const int StartHour = 0; // Unknown
			public const int EndHour = 0; // Unknown
			public const string Message = "";
		}

		public static class ToolTip {
			public const string Desc = "";
		}
	}
}