﻿namespace MapleLib.WzLib.WzStructure.Data.MapStructure {
	public struct TimeMob {
		public int startHour, endHour;
		public int id;
		public string message;

		public TimeMob(int startHour, int endHour, int id, string message) {
			this.startHour = startHour;
			this.endHour = endHour;
			this.id = id;
			this.message = message;
		}
	}
}