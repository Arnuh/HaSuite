/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

//uncomment the line below to create a space-time tradeoff (saving RAM by wasting more CPU cycles)

#define SPACETIME

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using HaCreator.MapEditor;
using HaCreator.Wz;
using HaSharedLibrary.Wz;
using MapleLib.Helpers;
using MapleLib.WzLib;
using MapleLib.WzLib.Serialization;
using MapleLib.WzLib.WzProperties;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using TabControl = System.Windows.Controls.TabControl;

namespace HaCreator.GUI {
	public partial class FieldSelector : Form {
		private readonly MultiBoard multiBoard;
		private readonly TabControl Tabs;
		private readonly RoutedEventHandler[] rightClickHandler;

		private readonly string defaultMapNameFilter;

		private bool _bAutoCloseUponSelection;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="board"></param>
		/// <param name="Tabs"></param>
		/// <param name="rightClickHandler"></param>
		/// <param name="defaultMapNameFilter">The default text to set for the map name filter</param>
		public FieldSelector(MultiBoard board, TabControl Tabs,
			RoutedEventHandler[] rightClickHandler, bool bAutoCloseUponSelection,
			string defaultMapNameFilter = null) {
			InitializeComponent();

			DialogResult = DialogResult.Cancel;
			multiBoard = board;
			this.Tabs = Tabs;
			this.rightClickHandler = rightClickHandler;
			_bAutoCloseUponSelection = bAutoCloseUponSelection;
			this.defaultMapNameFilter = defaultMapNameFilter;

			searchBox.TextChanged += mapBrowser.searchBox_TextChanged;
		}

		private void Load_Load(object sender, EventArgs e) {
			switch (ApplicationSettings.lastRadioIndex) {
				case 0:
					IMGSelect.Checked = true;
					IMGBox.Text = ApplicationSettings.LastImgPath;
					loadButton.Enabled = IMGBox.Text != "";
					break;
				case 1:
					XMLSelect.Checked = true;
					XMLBox.Text = ApplicationSettings.LastXmlPath;
					loadButton.Enabled = XMLBox.Text != "";
					break;
				case 2:
					WZSelect.Checked = true;
					break;
			}

			mapBrowser.InitializeMaps(true);

			// after loading
			if (defaultMapNameFilter != null) {
				searchBox.Focus();
				searchBox.Text = defaultMapNameFilter;

				mapBrowser.searchBox_TextChanged(searchBox, null);
			}
		}

		private void SelectionChanged(object sender, EventArgs e) {
			if (IMGSelect.Checked) {
				ApplicationSettings.lastRadioIndex = 0;
				IMGBox.Enabled = true;
				XMLBox.Enabled = false;
				searchBox.Enabled = false;
				mapBrowser.IsEnabled = false;
				loadButton.Enabled = true;
			} else if (XMLSelect.Checked) {
				ApplicationSettings.lastRadioIndex = 1;
				IMGBox.Enabled = false;
				XMLBox.Enabled = true;
				searchBox.Enabled = false;
				mapBrowser.IsEnabled = false;
				loadButton.Enabled = XMLBox.Text != "";
			} else if (WZSelect.Checked) {
				ApplicationSettings.lastRadioIndex = 2;
				IMGBox.Enabled = false;
				XMLBox.Enabled = false;
				searchBox.Enabled = true;
				mapBrowser.IsEnabled = true;
				loadButton.Enabled = mapBrowser.LoadAvailable;
			}
		}

		private void BrowseXML_Click(object sender, EventArgs e) {
			var dialog = new OpenFileDialog();
			dialog.Title = "Select XML to load...";
			dialog.Filter = "eXtensible Markup Language file (*.xml)|*.xml";
			if (dialog.ShowDialog() != DialogResult.OK) {
				return;
			}

			XMLBox.Text = dialog.FileName;
			loadButton.Enabled = true;
		}

		private void BrowseIMG_Click(object sender, EventArgs e) {
			var dialog = new OpenFileDialog();
			dialog.Title = "Select Map to load...";
			dialog.Filter = "WZ Image Files (*.img)|*.img";
			if (dialog.ShowDialog() != DialogResult.OK) {
				return;
			}

			IMGBox.Text = dialog.FileName;
			loadButton.Enabled = true;
		}

		/// <summary>
		/// Load map
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LoadButton_Click(object sender, EventArgs e) {
			var ww = new WaitWindow("Loading...");
			ww.Show();
			Application.DoEvents();

			WzImage mapImage = null;
			var mapid = -1;
			string mapName = null, streetName = "", categoryName = "";
			WzSubProperty strMapProp = null;

			if (IMGSelect.Checked) {
				var version = Initialization.WzMapleVersion;
				var deserializer = new WzImgDeserializer(false, version);
				var fileName = Path.GetFileName(IMGBox.Text);
				mapImage = deserializer.WzImageFromIMGFile(IMGBox.Text, fileName, out var successfullyParsedImage);
				if (!successfullyParsedImage) {
					MessageBox.Show("Error while loading IMG. Aborted.");
					ww.EndWait();
					Show();
					return;
				}

				var mapid_str = mapImage.Name.Replace(".img", "").Replace(".xml", "");
				int.TryParse(mapid_str, out mapid);
			} else if (XMLSelect.Checked) {
				try {
					var version = Initialization.WzMapleVersion;
					mapImage = (WzImage) new WzXmlDeserializer(false, version).ParseXML(XMLBox.Text)[0];
					var mapid_str = mapImage.Name.Replace(".img", "").Replace(".xml", "");
					int.TryParse(mapid_str, out mapid);
				} catch (Exception ex) {
					Debug.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
					ErrorLogger.Log(ErrorLevel.Save, $"{ex.Message}\r\n{ex.StackTrace}");
					MessageBox.Show("Error while loading XML. Aborted.");
					ww.EndWait();
					Show();
					return;
				}
			} else if (WZSelect.Checked) {
				if (mapBrowser.SelectedItem == null) {
					return; // racing event
				}

				var selectedName = mapBrowser.SelectedItem;

				if (selectedName.StartsWith("MapLogin")) { // MapLogin, MapLogin1, MapLogin2, MapLogin3
					var uiWzDirs = Program.WzManager.GetWzDirectoriesFromBase("ui");
					foreach (var uiWzDir in uiWzDirs) {
						mapImage = (WzImage) uiWzDir?[selectedName + ".img"];
						if (mapImage != null) {
							break;
						}
					}

					mapName = streetName = categoryName = selectedName;
				} else if (selectedName == "CashShopPreview") {
					var uiWzDirs = Program.WzManager.GetWzDirectoriesFromBase("ui");
					foreach (var uiWzDir in uiWzDirs) {
						mapImage = (WzImage) uiWzDir?["CashShopPreview.img"];
						if (mapImage != null) {
							break;
						}
					}

					mapName = streetName = categoryName = "CashShopPreview";
				} else if (selectedName == "ITCPreview") {
					var uiWzDirs = Program.WzManager.GetWzDirectoriesFromBase("ui");
					foreach (var uiWzDir in uiWzDirs) {
						mapImage = (WzImage) uiWzDir?["ITCPreview.img"];
						if (mapImage != null) {
							break;
						}
					}

					mapName = streetName = categoryName = "ITCPreview";
				} else {
					var mapid_str = mapBrowser.SelectedItem.Substring(0, 9);
					int.TryParse(mapid_str, out mapid);

					mapImage = GetMapLoadData(mapid, out strMapProp, out mapName, out streetName, out categoryName);
				}
			}

			MapLoader.CreateMapFromImage(CreateReason.Load, mapid, mapImage, mapName, streetName, categoryName, strMapProp, Tabs,
				multiBoard, rightClickHandler);

			DialogResult = DialogResult.OK;
			ww.EndWait();

			if (_bAutoCloseUponSelection) {
				Close();
			}
		}

		public static WzImage GetMapLoadData(int mapId, out WzSubProperty strMapProp, out string mapName, out string streetName, out string categoryName) {
			var mapImage = WzInfoTools.FindMapImage(mapId.ToString(), Program.WzManager);

			strMapProp = WzInfoTools.GetMapStringProp(mapId, Program.WzManager);
			if (strMapProp == null && Program.WzManager.IsKMSBWzFormat) {
				strMapProp = (WzSubProperty) mapImage["info"];
			}

			mapName = WzInfoTools.GetMapName(strMapProp);
			streetName = WzInfoTools.GetMapStreetName(strMapProp);
			categoryName = WzInfoTools.GetMapCategoryName(strMapProp);
			return mapImage;
		}

		private void MapBrowser_SelectionChanged() {
			loadButton.Enabled = mapBrowser.LoadAvailable;
		}

		private void Load_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Escape) {
				Close();
			} else if (e.KeyCode == Keys.Enter) {
				LoadButton_Click(null, null);
			}
		}

		private void IMGBox_TextChanged(object sender, EventArgs e) {
			ApplicationSettings.LastImgPath = IMGBox.Text;
		}

		private void XMLBox_TextChanged(object sender, EventArgs e) {
			ApplicationSettings.LastXmlPath = XMLBox.Text;
		}
	}
}