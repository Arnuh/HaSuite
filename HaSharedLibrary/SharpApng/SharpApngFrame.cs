/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Drawing;

namespace HaSharedLibrary.SharpApng {
	public class SharpApngFrame : IDisposable {
		public void Dispose() {
			Bitmap.Dispose();
		}

		public SharpApngFrame(Bitmap bmp, int num, int den) {
			DelayNum = num;
			DelayDen = den;
			Bitmap = bmp;
		}

		public int DelayNum { get; set; }

		public int DelayDen { get; set; }

		public Bitmap Bitmap { get; set; }
	}
}