﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Instance;
using HaCreator.Wz;
using HaRepacker;
using HaSharedLibrary.Wz;
using MapleLib;
using MapleLib.Helpers;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure;
using Path = System.IO.Path;

namespace HaCreator.GUI {
	public partial class Initialization : Form {
		public HaEditor editor = null;


		private static WzMapleVersion
			_wzMapleVersion =
				WzMapleVersion.BMS; // Default to BMS, the enc version to use when decrypting the WZ files.

		public static WzMapleVersion WzMapleVersion {
			get => _wzMapleVersion;
			private set { }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public Initialization() {
			InitializeComponent();
		}

		private bool IsPathCommon(string path) {
			foreach (var commonPath in WzFileManager.COMMON_MAPLESTORY_DIRECTORY) {
				if (commonPath == path) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Initialise
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_initialise_Click(object sender, EventArgs e) {
			ApplicationSettings.MapleVersionIndex = versionBox.SelectedIndex;
			ApplicationSettings.MapleFolderIndex = pathBox.SelectedIndex;
			var wzPath = pathBox.Text;

			if (wzPath == "Select MapleStory Folder") {
				MessageBox.Show("Please select the MapleStory folder.", "Error", MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			if (!ApplicationSettings.MapleFoldersList.Contains(wzPath) && !IsPathCommon(wzPath)) {
				ApplicationSettings.MapleFoldersList = ApplicationSettings.MapleFoldersList == ""
					? wzPath
					: ApplicationSettings.MapleFoldersList + "," + wzPath;
			}

			WzMapleVersion fileVersion;
			short version = -1;
			if (versionBox.SelectedIndex == 3) {
				var testFile = File.Exists(Path.Combine(wzPath, "Data.wz")) ? "Data.wz" : "Item.wz";
				try {
					fileVersion = WzTool.DetectMapleVersion(Path.Combine(wzPath, testFile), out version);
				} catch (Exception ex) {
					Warning.Error("Error initializing " + testFile + " (" + ex.Message + ").\r\nCheck that the directory is valid and the file is not in use.");
					return;
				}
			} else {
				fileVersion = (WzMapleVersion) versionBox.SelectedIndex;
			}

			if (InitializeWzFiles(wzPath, fileVersion)) {
				Hide();
				Application.DoEvents();
				editor = new HaEditor();
				editor.ShowDialog();

				Application.Exit();
			}
		}

		/// <summary>
		/// Initialise the WZ files with the provided folder path
		/// </summary>
		/// <param name="wzPath"></param>
		/// <param name="fileVersion"></param>
		/// <returns></returns>
		private bool InitializeWzFiles(string wzPath, WzMapleVersion fileVersion) {
			// Check if directory exist
			if (!Directory.Exists(wzPath)) {
				MessageBox.Show(string.Format(Properties.Resources.Initialization_Error_MSDirectoryNotExist, wzPath),
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			if (Program.WzManager != null) {
				Program.WzManager.Dispose();
				Program.WzManager = null; // old loaded items
			}

			if (Program.InfoManager != null) Program.InfoManager.Clear();

			_wzMapleVersion = fileVersion; // set version to static vars

			var bIs64BitDirectoryWzFileFormat = WzFileManager.Detect64BitDirectoryWzFileFormat(wzPath); // set
			var bIsPreBBDataWzFormat = WzFileManager.DetectIsPreBBDataWZFormat(wzPath); // set

			Program.WzManager = new WzFileManager(wzPath, bIs64BitDirectoryWzFileFormat, bIsPreBBDataWzFormat);
			Program.WzManager.BuildWzFileList(); // builds the list of WZ files in the directories (for HaCreator)

			// for old maplestory with only Data.wz
			if (Program.WzManager.IsPreBBDataWzFormat) //currently always false
			{
				UpdateUI_CurrentLoadingWzFile("Data.wz");

				try {
					Program.WzManager.LoadLegacyDataWzFile("Data", _wzMapleVersion);
				} catch (Exception e) {
					MessageBox.Show("Error initializing data.wz (" + e.Message +
					                ").\r\nCheck that the directory is valid and the file is not in use.");
					return false;
				}

				ExtractStringWzMaps();
				//Program.WzManager.ExtractItems();

				ExtractMobFile();
				ExtractNpcFile();
				ExtractReactorFile();
				ExtractSoundFile();
				ExtractMapMarks();
				ExtractPortals();
				ExtractTileSets();
				ExtractObjSets();
				ExtractBackgroundSets();
			} else // for versions beyond v30x
			{
				// String.wz
				var stringWzFiles = Program.WzManager.GetWzFileNameListFromBase("string");
				foreach (var stringWzFileName in stringWzFiles) {
					UpdateUI_CurrentLoadingWzFile(stringWzFileName);

					Program.WzManager.LoadWzFile(stringWzFileName, _wzMapleVersion);
				}

				ExtractStringWzMaps();

				// Mob WZ
				var mobWzFiles = Program.WzManager.GetWzFileNameListFromBase("mob");
				foreach (var mobWZFile in mobWzFiles) {
					UpdateUI_CurrentLoadingWzFile(mobWZFile);

					Program.WzManager.LoadWzFile(mobWZFile, _wzMapleVersion);
				}

				ExtractMobFile();


				// Load Npc
				var npcWzFiles = Program.WzManager.GetWzFileNameListFromBase("npc");
				foreach (var npc in npcWzFiles) {
					UpdateUI_CurrentLoadingWzFile(npc);

					Program.WzManager.LoadWzFile(npc, _wzMapleVersion);
				}

				ExtractNpcFile();

				// Load reactor
				var reactorWzFiles = Program.WzManager.GetWzFileNameListFromBase("reactor");
				foreach (var reactor in reactorWzFiles) {
					UpdateUI_CurrentLoadingWzFile(reactor);

					Program.WzManager.LoadWzFile(reactor, _wzMapleVersion);
				}

				ExtractReactorFile();

				// Load sound
				var soundWzDirs = Program.WzManager.GetWzFileNameListFromBase("sound");
				foreach (var soundDirName in soundWzDirs) {
					UpdateUI_CurrentLoadingWzFile(soundDirName);

					Program.WzManager.LoadWzFile(soundDirName, _wzMapleVersion);
				}

				ExtractSoundFile();


				// Load maps
				var mapWzFiles = Program.WzManager.GetWzFileNameListFromBase("map");
				foreach (var mapWzFileName in mapWzFiles) {
					UpdateUI_CurrentLoadingWzFile(mapWzFileName);

					Program.WzManager.LoadWzFile(mapWzFileName, _wzMapleVersion);
				}

				for (var i_map = 0; i_map <= 9; i_map++) {
					var map_iWzFiles = Program.WzManager.GetWzFileNameListFromBase("map\\map\\map" + i_map);
					foreach (var map_iWzFileName in map_iWzFiles) {
						UpdateUI_CurrentLoadingWzFile(map_iWzFileName);

						Program.WzManager.LoadWzFile(map_iWzFileName, _wzMapleVersion);
					}
				}

				var
					tileWzFiles =
						Program.WzManager
							.GetWzFileNameListFromBase(
								"map\\tile"); // this doesnt exist before 64-bit client, and is kept in Map.wz
				foreach (var tileWzFileNames in tileWzFiles) {
					UpdateUI_CurrentLoadingWzFile(tileWzFileNames);

					Program.WzManager.LoadWzFile(tileWzFileNames, _wzMapleVersion);
				}

				var
					objWzFiles =
						Program.WzManager
							.GetWzFileNameListFromBase(
								"map\\obj"); // this doesnt exist before 64-bit client, and is kept in Map.wz
				foreach (var objWzFileName in objWzFiles) {
					UpdateUI_CurrentLoadingWzFile(objWzFileName);

					Program.WzManager.LoadWzFile(objWzFileName, _wzMapleVersion);
				}

				var
					backWzFiles =
						Program.WzManager
							.GetWzFileNameListFromBase(
								"map\\back"); // this doesnt exist before 64-bit client, and is kept in Map.wz
				foreach (var backWzFileName in backWzFiles) {
					UpdateUI_CurrentLoadingWzFile(backWzFileName);

					Program.WzManager.LoadWzFile(backWzFileName, _wzMapleVersion);
				}

				ExtractMapMarks();
				ExtractPortals();
				ExtractTileSets();
				ExtractObjSets();
				ExtractBackgroundSets();


				// UI.wz
				var uiWzFiles = Program.WzManager.GetWzFileNameListFromBase("ui");
				foreach (var uiWzFileNames in uiWzFiles) {
					UpdateUI_CurrentLoadingWzFile(uiWzFileNames);

					Program.WzManager.LoadWzFile(uiWzFileNames, _wzMapleVersion);
				}
			}

			return true;
		}

		private void UpdateUI_CurrentLoadingWzFile(string fileName) {
			textBox2.Text = string.Format("Initializing {0}.wz...", fileName);
			Application.DoEvents();
		}

		private void UpdateUI_CurrentLoading(string text) {
			textBox2.Text = text;
			Application.DoEvents();
		}

		/// <summary>
		/// On loading initialization.cs
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Initialization_Load(object sender, EventArgs e) {
			versionBox.SelectedIndex = 0;
			try {
				var paths =
					ApplicationSettings.MapleFoldersList.Split(",".ToCharArray(),
						StringSplitOptions.RemoveEmptyEntries);
				foreach (var path in paths) {
					if (!Directory.Exists(
						    path)) // check if the old path actually exist before adding it to the combobox
					{
						continue;
					}

					pathBox.Items.Add(path);
				}

				foreach (var path in WzFileManager.COMMON_MAPLESTORY_DIRECTORY) // default path list
				{
					if (Directory.Exists(path)) {
						pathBox.Items.Add(path);
					}
				}

				if (pathBox.Items.Count == 0) {
					pathBox.Items.Add("Select Maple Folder");
				}
			} catch {
			}

			versionBox.SelectedIndex = ApplicationSettings.MapleVersionIndex;
			if (pathBox.Items.Count < ApplicationSettings.MapleFolderIndex + 1) {
				pathBox.SelectedIndex = pathBox.Items.Count - 1;
			} else {
				pathBox.SelectedIndex = ApplicationSettings.MapleFolderIndex;
			}
		}

		private void button2_Click(object sender, EventArgs e) {
			using (var mapleSelect = new FolderBrowserDialog() {
				       ShowNewFolderButton = true,
				       //   RootFolder = Environment.SpecialFolder.ProgramFilesX86,
				       Description = "Select the MapleStory folder."
			       }) {
				if (mapleSelect.ShowDialog() != DialogResult.OK) {
					return;
				}

				pathBox.Items.Add(mapleSelect.SelectedPath);
				pathBox.SelectedIndex = pathBox.Items.Count - 1;
			}

			;
		}

		/// <summary>
		/// Debug button for check map errors
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void debugButton_Click(object sender, EventArgs e) {
			const string OUTPUT_ERROR_FILENAME = "Debug_errors.txt";

			// This function iterates over all maps in the game and verifies that we recognize all their props
			// It is meant to use by the developer(s) to speed up the process of adjusting this program for different MapleStory versions
			var wzPath = pathBox.Text;

			var fileVersion = (WzMapleVersion) versionBox.SelectedIndex;
			if (versionBox.SelectedIndex == 3) {
				var testFile = !File.Exists(Path.Combine(wzPath, "String.wz")) ? "Data.wz" : "String.wz";
				try {
					fileVersion = WzTool.DetectMapleVersion(Path.Combine(wzPath, testFile), out _);
				} catch (Exception ex) {
					Warning.Error($"Error initializing ${testFile} (" + ex.Message + ").\r\nCheck that the directory is valid and the file is not in use.");
					return;
				}
			}

			if (!InitializeWzFiles(wzPath, fileVersion)) return;

			var mb = new MultiBoard();
			var mapBoard = new Board(
				new Microsoft.Xna.Framework.Point(),
				new Microsoft.Xna.Framework.Point(),
				mb,
				false,
				null,
				MapleLib.WzLib.WzStructure.Data.ItemTypes.None,
				MapleLib.WzLib.WzStructure.Data.ItemTypes.None);

			foreach (var mapid in Program.InfoManager.Maps.Keys) {
				var mapImage = WzInfoTools.FindMapImage(mapid, Program.WzManager);
				if (mapImage == null) continue;
				UpdateUI_CurrentLoading(mapImage.Name);

				mapImage.ParseImage();
				if (mapImage["info"]["link"] != null) {
					mapImage.UnparseImage();
					continue;
				}

				MapLoader.VerifyMapPropsKnown(mapImage, true);
				var info = new MapInfo(mapImage, null, null, null);
				try {
					mapBoard.CreateMapLayers();

					MapLoader.LoadLayers(mapImage, mapBoard);
					MapLoader.LoadLife(mapImage, mapBoard);
					MapLoader.LoadFootholds(mapImage, mapBoard);
					MapLoader.GenerateDefaultZms(mapBoard);
					MapLoader.LoadRopes(mapImage, mapBoard);
					MapLoader.LoadChairs(mapImage, mapBoard);
					MapLoader.LoadPortals(mapImage, mapBoard);
					MapLoader.LoadReactors(mapImage, mapBoard);
					MapLoader.LoadToolTips(mapImage, mapBoard);
					MapLoader.LoadBackgrounds(mapImage, mapBoard);
					MapLoader.LoadMisc(mapImage, mapBoard);

					//MapLoader.LoadBackgrounds(mapImage, board);
					//MapLoader.LoadMisc(mapImage, board);

					// Check background to ensure that its correct
					var allBackgrounds = new List<BackgroundInstance>();
					allBackgrounds.AddRange(mapBoard.BoardItems.BackBackgrounds);
					allBackgrounds.AddRange(mapBoard.BoardItems.FrontBackgrounds);

					foreach (var bg in allBackgrounds) {
						if (bg.type != MapleLib.WzLib.WzStructure.Data.BackgroundType.Regular) {
							if (bg.cx < 0 || bg.cy < 0) {
								var error = string.Format(
									"Negative CX/ CY moving background object. CX='{0}', CY={1}, Type={2}, {3}{4}",
									bg.cx, bg.cy, bg.type.ToString(), Environment.NewLine,
									mapImage.ToString() /*overrides, see WzImage.ToString*/);
								ErrorLogger.Log(ErrorLevel.IncorrectStructure, error);
							}
						}
					}

					allBackgrounds.Clear();
				} catch (Exception exp) {
					var error = string.Format("Exception occured loading {0}{1}{2}{3}", Environment.NewLine,
						mapImage.ToString() /*overrides, see WzImage.ToString*/, Environment.NewLine, exp.ToString());
					ErrorLogger.Log(ErrorLevel.Crash, error);
				} finally {
					mapBoard.Dispose();

					mapBoard.BoardItems.BackBackgrounds.Clear();
					mapBoard.BoardItems.FrontBackgrounds.Clear();

					mapImage.UnparseImage(); // To preserve memory, since this is a very memory intensive test
				}

				if (ErrorLogger.NumberOfErrorsPresent() > 200) {
					ErrorLogger.SaveToFile(OUTPUT_ERROR_FILENAME);
				}
			}

			ErrorLogger.SaveToFile(OUTPUT_ERROR_FILENAME);


			MessageBox.Show(string.Format("Check for map errors completed. See '{0}' for more information.",
				OUTPUT_ERROR_FILENAME));
		}

		/// <summary>
		/// Keyboard navigation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Initialization_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Enter) {
				button_initialise_Click(null, null);
			} else if (e.KeyCode == Keys.Escape) Close();
		}


		#region Extractor

		/// <summary>
		/// 
		/// </summary>
		public void ExtractMobFile() {
			// Mob.wz
			var mobWzDirs = Program.WzManager.GetWzDirectoriesFromBase("mob");

			foreach (var mobWzDir in mobWzDirs) {
			}

			// String.wz
			var stringWzDirs = Program.WzManager.GetWzDirectoriesFromBase("string");
			foreach (var stringWzDir in stringWzDirs) {
				var mobStringImage = (WzImage) stringWzDir?["mob.img"];
				if (mobStringImage == null) {
					continue; // not in this wz
				}

				if (!mobStringImage.Parsed) {
					mobStringImage.ParseImage();
				}

				foreach (WzSubProperty mob in mobStringImage.WzProperties) {
					var nameProp = (WzStringProperty) mob["name"];
					var name = nameProp == null ? "" : nameProp.Value;
					var id = WzInfoTools.AddLeadingZeros(mob.Name, 7);
					if (Program.InfoManager.Mobs.ContainsKey(id)) continue;
					Program.InfoManager.Mobs.Add(id, name);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ExtractNpcFile() {
			// Npc.wz
			var npcWzDirs = Program.WzManager.GetWzDirectoriesFromBase("npc");

			foreach (var npcWzDir in npcWzDirs) {
			}

			// String.wz
			var stringWzDirs = Program.WzManager.GetWzDirectoriesFromBase("string");
			foreach (var stringWzDir in stringWzDirs) {
				var npcImage = (WzImage) stringWzDir?["Npc.img"];
				if (npcImage == null) {
					continue; // not in this wz
				}

				if (!npcImage.Parsed) {
					npcImage.ParseImage();
				}

				foreach (WzSubProperty npc in npcImage.WzProperties) {
					var nameProp = (WzStringProperty) npc["name"];
					var name = nameProp == null ? "" : nameProp.Value;
					var id = WzInfoTools.AddLeadingZeros(npc.Name, 7);
					if (Program.InfoManager.NPCs.ContainsKey(id)) continue;
					Program.InfoManager.NPCs.Add(id, name);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ExtractReactorFile() {
			var reactorWzDirs = Program.WzManager.GetWzDirectoriesFromBase("reactor");
			foreach (var reactorWzDir in reactorWzDirs)
			foreach (var reactorImage in reactorWzDir.WzImages) {
				var reactor = ReactorInfo.Load(reactorImage);
				Program.InfoManager.Reactors[reactor.ID] = reactor;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ExtractSoundFile() {
			var soundWzDirs = Program.WzManager.GetWzDirectoriesFromBase("sound");

			foreach (var soundWzDir in soundWzDirs) {
				if (Program.WzManager.IsPreBBDataWzFormat) {
					var x = (WzDirectory) soundWzDir["Sound"];
				}

				foreach (var soundImage in soundWzDir.WzImages) {
					if (!soundImage.Name.ToLower().Contains("bgm")) {
						continue;
					}

					if (!soundImage.Parsed) {
						soundImage.ParseImage();
					}

					try {
						foreach (var bgmImage in soundImage.WzProperties) {
							WzBinaryProperty binProperty = null;
							if (bgmImage is WzBinaryProperty bgm) {
								binProperty = bgm;
							} else if (bgmImage is WzUOLProperty uolBGM) // is UOL property
							{
								var linkVal = ((WzUOLProperty) bgmImage).LinkValue;
								if (linkVal is WzBinaryProperty linkCanvas) binProperty = linkCanvas;
							}

							if (binProperty != null) {
								Program.InfoManager.BGMs[
										WzInfoTools.RemoveExtension(soundImage.Name) + @"/" + binProperty.Name] =
									binProperty;
							}
						}
					} catch (Exception e) {
						var error = string.Format("[ExtractSoundFile] Error parsing {0}, {1} file.\r\nError: {2}",
							soundWzDir.Name, soundImage.Name, e.ToString());
						ErrorLogger.Log(ErrorLevel.IncorrectStructure, error);
						continue;
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ExtractMapMarks() {
			var mapWzImg = (WzImage) Program.WzManager.FindWzImageByName("map", "MapHelper.img");
			if (mapWzImg == null) {
				throw new Exception("MapHelper.img not found in map.wz.");
			}

			foreach (WzCanvasProperty mark in mapWzImg["mark"].WzProperties)
				Program.InfoManager.MapMarks[mark.Name] = mark.GetLinkedWzCanvasBitmap();
		}

		/// <summary>
		/// 
		/// </summary>
		public void ExtractTileSets() {
			var bLoadedInMap = false;

			var mapWzDirs = (WzDirectory) Program.WzManager.FindWzImageByName("map", "Tile");
			if (mapWzDirs != null) {
				foreach (var tileset in mapWzDirs.WzImages)
					Program.InfoManager.TileSets[WzInfoTools.RemoveExtension(tileset.Name)] = tileset;

				bLoadedInMap = true;
				return; // only needs to be loaded once
			}

			// Not loaded, try to find it in "tile.wz"
			// on 64-bit client it is stored in a different file apart from map
			if (!bLoadedInMap) {
				var tileWzDirs = Program.WzManager.GetWzDirectoriesFromBase("map\\tile");
				foreach (var tileWzDir in tileWzDirs)
				foreach (var tileset in tileWzDir.WzImages)
					Program.InfoManager.TileSets[WzInfoTools.RemoveExtension(tileset.Name)] = tileset;
			}
		}

		/// <summary>
		/// Handle various scenarios ie Map001.wz exists but may only contain Back or only Obj etc
		/// </summary>
		public void ExtractObjSets() {
			var bLoadedInMap = false;

			var mapWzDirs = (WzDirectory) Program.WzManager.FindWzImageByName("map", "Obj");
			if (mapWzDirs != null) {
				foreach (var objset in mapWzDirs.WzImages)
					Program.InfoManager.ObjectSets[WzInfoTools.RemoveExtension(objset.Name)] = objset;

				bLoadedInMap = true;
				return; // only needs to be loaded once
			}

			// Not loaded, try to find it in "tile.wz"
			// on 64-bit client it is stored in a different file apart from map
			if (!bLoadedInMap) {
				var objWzDirs = Program.WzManager.GetWzDirectoriesFromBase("map\\obj");
				foreach (var objWzDir in objWzDirs)
				foreach (var objset in objWzDir.WzImages)
					Program.InfoManager.ObjectSets[WzInfoTools.RemoveExtension(objset.Name)] = objset;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ExtractBackgroundSets() {
			var bLoadedInMap = false;

			var mapWzDirs = (WzDirectory) Program.WzManager.FindWzImageByName("map", "Back");
			if (mapWzDirs != null) {
				foreach (var bgset in mapWzDirs.WzImages)
					Program.InfoManager.BackgroundSets[WzInfoTools.RemoveExtension(bgset.Name)] = bgset;

				bLoadedInMap = true;
			}

			// Not loaded, try to find it in "tile.wz"
			// on 64-bit client it is stored in a different file apart from map
			if (!bLoadedInMap) {
				var backWzDirs = Program.WzManager.GetWzDirectoriesFromBase("map\\back");
				foreach (var backWzDir in backWzDirs)
				foreach (var bgset in backWzDir.WzImages)
					Program.InfoManager.BackgroundSets[WzInfoTools.RemoveExtension(bgset.Name)] = bgset;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ExtractStringWzMaps() {
			var stringWzImg = (WzImage) Program.WzManager.FindWzImageByName("string", "Map.img");

			if (!stringWzImg.Parsed) {
				stringWzImg.ParseImage();
			}

			foreach (WzSubProperty mapCat in stringWzImg.WzProperties)
			foreach (WzSubProperty map in mapCat.WzProperties) {
				var streetName = (WzStringProperty) map["streetName"];
				var mapName = (WzStringProperty) map["mapName"];
				string id;
				if (map.Name.Length == 9) {
					id = map.Name;
				} else {
					id = WzInfoTools.AddLeadingZeros(map.Name, 9);
				}

				if (mapName == null) {
					Program.InfoManager.Maps[id] = new Tuple<string, string>("", "");
				} else {
					Program.InfoManager.Maps[id] =
						new Tuple<string, string>(streetName?.Value == null ? string.Empty : streetName.Value,
							mapName.Value);
				}
			}
		}

		public void ExtractPortals() {
			var mapImg = (WzImage) Program.WzManager.FindWzImageByName("map", "MapHelper.img");
			if (mapImg == null) {
				throw new Exception("Couldnt extract portals. MapHelper.img not found.");
			}

			var portalParent = (WzSubProperty) mapImg["portal"];
			var editorParent = (WzSubProperty) portalParent["editor"];
			foreach (var key in Program.InfoManager.PortalIdByType.Keys) {
				if (editorParent[key] == null) continue;
				Program.InfoManager.PortalTypeById.Add(key);
			}

			foreach (var key in editorParent.WzProperties) {
				var portal = (WzCanvasProperty) key;
				PortalInfo.Load(portal);
			}

			var gameParent = (WzSubProperty) portalParent["game"]["pv"];
			foreach (var portal in gameParent.WzProperties) {
				if (portal.WzProperties[0] is WzSubProperty) {
					var images = new Dictionary<string, Bitmap>();
					Bitmap defaultImage = null;
					foreach (WzSubProperty image in portal.WzProperties) {
						//WzSubProperty portalContinue = (WzSubProperty)image["portalContinue"];
						//if (portalContinue == null) continue;
						var portalImage = image["0"].GetBitmap();
						if (image.Name == "default") {
							defaultImage = portalImage;
						} else {
							images.Add(image.Name, portalImage);
						}
					}

					Program.InfoManager.GamePortals.Add(portal.Name, new PortalGameImageInfo(defaultImage, images));
				} else if (portal.WzProperties[0] is WzCanvasProperty) {
					var images = new Dictionary<string, Bitmap>();
					Bitmap defaultImage = null;
					try {
						foreach (WzCanvasProperty image in portal.WzProperties) {
							//WzSubProperty portalContinue = (WzSubProperty)image["portalContinue"];
							//if (portalContinue == null) continue;
							var portalImage = image.GetLinkedWzCanvasBitmap();
							defaultImage = portalImage;
							images.Add(image.Name, portalImage);
						}

						Program.InfoManager.GamePortals.Add(portal.Name, new PortalGameImageInfo(defaultImage, images));
					} catch (InvalidCastException) {
						continue;
					} //nexon likes to toss ints in here zType etc
				}
			}
		}

		#endregion
	}
}