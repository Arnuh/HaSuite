/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using HaSharedLibrary.Wz;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace HaCreator.CustomControls {
	public partial class MapBrowser : UserControl {
		private bool loadMapAvailable;
		private readonly List<string> maps = new();

		public MapBrowser() {
			InitializeComponent();

			minimapBox.SizeMode = PictureBoxSizeMode.Zoom;
		}

		public bool LoadAvailable => loadMapAvailable;

		public string SelectedItem => (string) mapNamesBox.SelectedItem;

		public bool IsEnabled {
			set {
				mapNamesBox.Enabled = value;
				minimapBox.Visible = value;
			}
		}

		public delegate void MapSelectChangedDelegate();

		public event MapSelectChangedDelegate SelectionChanged;

		/// <summary>
		/// Initialise
		/// </summary>
		/// <param name="special">True to include cash shop and login.</param>
		public void InitializeMaps(bool special) {
			// Logins
			var mapLogins = new List<string>();
			for (var i = 0; i < 20; i++) // Not exceeding 20 logins yet.
			{
				var imageName = "MapLogin" + (i == 0 ? "" : i.ToString()) + ".img";

				WzObject mapLogin = null;

				var uiWzFiles = Program.WzManager.GetWzDirectoriesFromBase("ui");
				foreach (var uiWzFile in uiWzFiles) {
					mapLogin = uiWzFile?[imageName];
					if (mapLogin != null) {
						break;
					}
				}

				if (mapLogin == null) {
					break;
				}

				mapLogins.Add(imageName);
			}

			// Maps
			foreach (var map in Program.InfoManager.Maps) {
				maps.Add(string.Format("{0} - {1} : {2}", map.Key, map.Value.Item1, map.Value.Item2));
			}

			maps.Sort();

			if (special) {
				maps.Insert(0, "CashShopPreview");
				maps.Insert(1, "ITCPreview");

				foreach (var mapLogin in mapLogins) {
					maps.Insert(0, mapLogin.Replace(".img", ""));
				}
			}

			var mapsObjs = maps.Cast<object>().ToArray();
			mapNamesBox.Items.AddRange(mapsObjs);
		}

		private string _previousSeachText = string.Empty;
		private CancellationTokenSource _existingSearchTaskToken;

		/// <summary>
		/// On search box text changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">May be null</param>
		public void searchBox_TextChanged(object sender, EventArgs e) {
			var searchBox = (TextBox) sender;
			var tosearch = searchBox.Text.ToLower();

			if (_previousSeachText == tosearch) {
				return;
			}

			_previousSeachText = tosearch; // set


			// Cancel existing task if any
			if (_existingSearchTaskToken != null && !_existingSearchTaskToken.IsCancellationRequested) {
				_existingSearchTaskToken.Cancel();
			}

			// Clear 
			mapNamesBox.Items.Clear();
			// AddRange does Begin/End Update for a smooth UI experience.
			if (tosearch == string.Empty) {
				mapNamesBox.Items.AddRange(maps.ToArray<object>());

				mapNamesBox_SelectedIndexChanged(null, null);
			} else {
				var currentDispatcher = Dispatcher.CurrentDispatcher;

				_existingSearchTaskToken = new CancellationTokenSource();
				var cancellationToken = _existingSearchTaskToken.Token;

				var t = Task.Run(() => {
					var mapsFiltered = new List<string>();
					foreach (var map in maps) {
						if (_existingSearchTaskToken.IsCancellationRequested) {
							return;
						}

						if (map.ToLower().Contains(tosearch)) {
							mapsFiltered.Add(map);
						}
					}

					currentDispatcher.BeginInvoke(new Action(() => {
						if (_existingSearchTaskToken.IsCancellationRequested) {
							return;
						}

						mapNamesBox.Items.AddRange(mapsFiltered.ToArray<object>());
						if (mapNamesBox.Items.Count > 0) {
							mapNamesBox.SelectedIndex = 0; // set default selection to reduce clicks
						}
					}));
				}, cancellationToken);
			}
		}

		/// <summary>
		/// On map selection changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mapNamesBox_SelectedIndexChanged(object sender, EventArgs e) {
			var selectedName = (string) mapNamesBox.SelectedItem;

			if (selectedName == "MapLogin" ||
			    selectedName == "MapLogin1" ||
			    selectedName == "MapLogin2" ||
			    selectedName == "MapLogin3" ||
			    selectedName == "MapLogin4" ||
			    selectedName == "MapLogin5" ||
			    selectedName == "CashShopPreview" ||
			    selectedName == "ITCPreview" ||
			    selectedName == null) {
				panel_linkWarning.Visible = false;
				panel_mapExistWarning.Visible = false;

				minimapBox.Image = new Bitmap(1, 1);
				loadMapAvailable = mapNamesBox.SelectedItem != null;
			} else {
				var mapid = selectedName.Substring(0, 9);

				var mapImage = WzInfoTools.FindMapImage(mapid, Program.WzManager);
				if (mapImage == null) {
					panel_linkWarning.Visible = false;
					panel_mapExistWarning.Visible = true;

					minimapBox.Image = new Bitmap(1, 1);
					loadMapAvailable = false;
				} else {
					using (var rsrc = new WzImageResource(mapImage)) {
						if (mapImage["info"]["link"] != null) {
							panel_linkWarning.Visible = true;
							panel_mapExistWarning.Visible = false;
							label_linkMapId.Text = mapImage["info"]["link"].ToString();

							minimapBox.Image = new Bitmap(1, 1);
							loadMapAvailable = false;
						} else {
							panel_linkWarning.Visible = false;
							panel_mapExistWarning.Visible = false;

							loadMapAvailable = true;
							var minimap = (WzCanvasProperty) mapImage.GetFromPath("miniMap/canvas");
							if (minimap != null) {
								minimapBox.Image = minimap.GetLinkedWzCanvasBitmap();
							} else {
								minimapBox.Image = new Bitmap(1, 1);
							}

							loadMapAvailable = true;
						}
					}
				}
			}

			SelectionChanged.Invoke();
		}
	}
}