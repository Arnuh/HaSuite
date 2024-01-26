﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows.Forms;
using HaRepacker.Properties;

namespace HaRepacker {
	public static class Warning {
		public static bool Warn(string text) {
			return Program.ConfigurationManager.UserSettings.SuppressWarnings || MessageBox.Show(text,
				Resources.Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
		}

		public static void Error(string text) {
			MessageBox.Show(text, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}