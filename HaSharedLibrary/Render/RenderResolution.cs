﻿/* Copyright (C) 2020 lastbattle

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace HaSharedLibrary.Render.DX {
	public enum RenderResolution {
		Res_All = 0,

		Res_800x600 = 0x1, // 800x600 4:3

		Res_1024x768 = 0x2, // 1024x768 4:3

		Res_1280x720 = 0x200, // 1280x720 16:9

		Res_1366x768 = 0x4, // 1366x768 16:9

		Res_1920x1080 = 0x8, // 1920x1080 16:9
		Res_1920x1200 = 0x10, // 1920x1200 16:9

		Res_1920x1080_120PercScaled = 0x20, // 1920x1080 16:9 150% scale
		Res_1920x1080_150PercScaled = 0x40, // 1920x1080 16:9 150% scale

		Res_1920x1200_120PercScaled = 0x80, // 1920x1200 16:9 120% scale
		Res_1920x1200_150PercScaled = 0x100 // 1920x1200 16:9 150% scale
	}

	public static class RenderResolutionExtensions {
		/// <summary>
		/// Converts RenderResolution name to human readable text
		/// </summary>
		/// <param name="rr"></param>
		/// <returns></returns>
		public static string ToReadableString(this RenderResolution rr) {
			return rr.ToString().Replace("Res_", "").Replace("_", " ").Replace("PercScaled", "% scale");
		}
	}
}