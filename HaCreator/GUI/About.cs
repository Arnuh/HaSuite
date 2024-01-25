﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Diagnostics;
using System.Windows.Forms;

namespace HaCreator.GUI {
	public partial class About : Form {
		public About() {
			InitializeComponent();
		}

		private void About_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Escape) {
				e.Handled = true;
				Close();
			}
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			Process.Start("https://github.com/lastbattle/Harepacker-resurrected");
		}
	}
}