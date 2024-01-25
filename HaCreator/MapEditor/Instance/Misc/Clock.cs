/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using XNA = Microsoft.Xna.Framework;

namespace HaCreator.MapEditor.Instance.Misc {
	public class Clock : MiscRectangle, ISerializable {
		public Clock(Board board, XNA.Rectangle rect)
			: base(board, rect) {
		}

		public override string Name => "Clock";

		public Clock(Board board, SerializationForm json)
			: base(board, json) {
		}
	}
}