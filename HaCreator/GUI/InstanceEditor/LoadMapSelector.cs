﻿/* Copyright (C) 2020 lastbattle

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

//uncomment the line below to create a space-time tradeoff (saving RAM by wasting more CPU cycles)

#define SPACETIME

using System;
using System.Windows.Forms;

namespace HaCreator.GUI.InstanceEditor {
	public partial class LoadMapSelector : Form {
		/// <summary>
		/// The NumericUpDown text to set upon selection
		/// </summary>
		private NumericUpDown numericUpDown;

		/// <summary>
		/// Or the textbox
		/// </summary>
		private TextBox textBox;

		private bool showSpecialMaps;

		/// <summary>
		/// Load map selector
		/// </summary>
		/// <param name="numericUpDown"></param>
		public LoadMapSelector(NumericUpDown numericUpDown) {
			InitializeComponent();

			DialogResult = DialogResult.Cancel;

			this.numericUpDown = numericUpDown;

			searchBox.TextChanged += mapBrowser.searchBox_TextChanged;
		}

		public LoadMapSelector(TextBox textBox, bool showSpecialMaps = false) {
			InitializeComponent();

			DialogResult = DialogResult.Cancel;

			this.textBox = textBox;
			searchBox.TextChanged += mapBrowser.searchBox_TextChanged;

			this.showSpecialMaps = showSpecialMaps;
		}

		/// <summary>
		/// On load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Load_Load(object sender, EventArgs e) {
			mapBrowser.InitializeMaps(showSpecialMaps);
		}

		/// <summary>
		/// On load button clicked, selects that map and closes this dialog.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void loadButton_Click(object sender, EventArgs e) {
			var selectedEntry = mapBrowser.SelectedItem;
			// MapLogin and CashShopPreview wont contain an -, hacky check so I hope we don't break it later
			var mapId = selectedEntry.Contains("-") ? selectedEntry.Substring(0, 9) : selectedEntry;

			if (numericUpDown != null) {
				numericUpDown.Value = long.Parse(mapId);
			} else {
				textBox.Text = mapId;
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private void mapBrowser_SelectionChanged() {
		}

		private void Load_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Escape) {
				Close();
			} else if (e.KeyCode == Keys.Enter) {
				loadButton_Click(null, null);
			}
		}
	}
}