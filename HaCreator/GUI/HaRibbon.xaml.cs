/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using HaCreator.CustomControls;
using HaCreator.MapEditor;
using HaSharedLibrary.Render.DX;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace HaCreator.GUI {
	/// <summary>
	/// Interaction logic for HaRibbon.xaml
	/// </summary>
	public partial class HaRibbon : UserControl {
		public HaRibbon() {
			InitializeComponent();

#if DEBUG
			debugTab.Visibility = Visibility.Visible;
#else
			debugTab.Visibility = Visibility.Collapsed;
#endif

			PreviewMouseWheel += HaRibbon_PreviewMouseWheel;
			altBackgroundToggle.IsChecked = UserSettings.altBackground;
			fhSideToggle.IsChecked = UserSettings.displayFHSide;
		}

		private void HaRibbon_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
			e.Handled = true;
			if (ribbon.IsMouseOver) {
				var platformBox = (HaList) FindName("platformBox");
				if (platformBox.IsMouseOver) {
					platformBox.Scroll(e.Delta);
				} else if (layerBox.IsMouseOver) {
					layerBox.Scroll(e.Delta);
				}
			}
		}

		public int reducedHeight;
		private int actualLayerIndex;
		private int actualPlatform;
		private int changingIndexCnt;
		private Layer[] layers;
		private bool hasMinimap;

		private void Ribbon_Loaded(object sender, RoutedEventArgs e) {
			var child = VisualTreeHelper.GetChild((DependencyObject) sender, 0) as Grid;
			if (child != null) {
				reducedHeight = (int) child.RowDefinitions[0].ActualHeight;
				child.RowDefinitions[0].Height = new GridLength(0);
			}

			// Load map simulator resolutions
			foreach (RenderResolution val in Enum.GetValues(typeof(RenderResolution))) {
				var comboBoxItem = new ComboBoxItem {
					Tag = val,
					Content = val.ToReadableString()
				};

				comboBox_Resolution.Items.Add(comboBoxItem);
			}
			//comboBox_Resolution.DisplayMemberPath = "Content";

			var i = 0;
			foreach (ComboBoxItem item in comboBox_Resolution.Items) {
				if ((RenderResolution) item.Tag == UserSettings.SimulateResolution) {
					comboBox_Resolution.SelectedIndex = i;
					break;
				}

				i++;
			}
		}

		public static readonly RoutedUICommand New = new("New", "New", typeof(HaRibbon),
			new InputGestureCollection {new KeyGesture(Key.N, ModifierKeys.Control)});

		public static readonly RoutedUICommand Open = new("Open", "Open", typeof(HaRibbon),
			new InputGestureCollection {new KeyGesture(Key.O, ModifierKeys.Control)});

		public static readonly RoutedUICommand Save = new("Save", "Save", typeof(HaRibbon),
			new InputGestureCollection {new KeyGesture(Key.S, ModifierKeys.Control)});

		public static readonly RoutedUICommand Repack = new("Repack", "Repack", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand About = new("About", "About", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand Help = new("Help", "Help", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand Settings = new("Settings", "Settings", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand Exit = new("Exit", "Exit", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand ViewBoxes = new("ViewBoxes", "ViewBoxes",
			typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand Minimap = new("Minimap", "Minimap", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand Parallax = new("Parallax", "Parallax", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand Finalize = new("Finalize", "Finalize", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand AllLayerView = new("AllLayerView", "AllLayerView",
			typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand MapSim = new("MapSim", "MapSim", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand RegenMinimap = new("RegenMinimap", "RegenMinimap",
			typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand Snapping = new("Snapping", "Snapping", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand Random = new("Random", "Random", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand InfoMode = new("InfoMode", "InfoMode", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand HaRepacker = new("PheRepacker", "PheRepacker",
			typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand LayerUp = new("LayerUp", "LayerUp", typeof(HaRibbon),
			new InputGestureCollection {new KeyGesture(Key.OemPlus, ModifierKeys.Control)});

		public static readonly RoutedUICommand LayerDown = new("LayerDown", "LayerDown",
			typeof(HaRibbon),
			new InputGestureCollection {new KeyGesture(Key.OemMinus, ModifierKeys.Control)});

		public static readonly RoutedUICommand AllPlatformView = new("AllPlatformView",
			"AllPlatformView", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand PlatformUp = new("PlatformUp", "PlatformUp",
			typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand PlatformDown = new("PlatformDown", "PlatformDown",
			typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand NewPlatform = new("NewPlatform", "NewPlatform",
			typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand UserObjs = new("UserObjs", "UserObjs", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand Export = new("Export", "Export", typeof(HaRibbon),
			new InputGestureCollection());

		public static readonly RoutedUICommand PhysicsEdit = new("PhysicsEdit", "PhysicsEdit",
			typeof(HaRibbon),
			new InputGestureCollection());

		#region Debug Items

		public static readonly RoutedUICommand ShowMapProperties = new("ShowMapProperties",
			"ShowMapProperties", typeof(HaRibbon),
			new InputGestureCollection());

		#endregion

		private void AlwaysExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void HasMinimap(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = hasMinimap;
		}

		private void New_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (NewClicked != null) {
				NewClicked.Invoke();
			}
		}

		private void Open_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (OpenClicked != null) {
				OpenClicked.Invoke();
			}
		}

		private void Save_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (SaveClicked != null) {
				SaveClicked.Invoke();
			}
		}

		private void Repack_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (RepackClicked != null) {
				RepackClicked.Invoke();
			}
		}

		private void About_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (AboutClicked != null) {
				AboutClicked.Invoke();
			}
		}

		private void Help_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (HelpClicked != null) {
				HelpClicked.Invoke();
			}
		}

		private void Settings_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (SettingsClicked != null) {
				SettingsClicked.Invoke();
			}
		}

		private void Exit_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (ExitClicked != null) {
				ExitClicked.Invoke();
			}
		}

		private void ViewBoxes_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (ViewToggled != null) {
				ViewToggled.Invoke(tilesCheck.IsChecked, objsCheck.IsChecked, npcsCheck.IsChecked, mobsCheck.IsChecked,
					reactCheck.IsChecked, portalCheck.IsChecked, fhCheck.IsChecked, ropeCheck.IsChecked,
					chairCheck.IsChecked, tooltipCheck.IsChecked, bgCheck.IsChecked, miscCheck.IsChecked,
					mirrorFieldDataCheck.IsChecked);
			}
		}

		private void Minimap_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (ShowMinimapToggled != null) {
				ShowMinimapToggled.Invoke(((RibbonToggleButton) e.OriginalSource).IsChecked.Value);
			}
		}

		private void Parallax_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (ParallaxToggled != null) {
				ParallaxToggled.Invoke(((RibbonToggleButton) e.OriginalSource).IsChecked.Value);
			}
		}

		private void Finalize_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (FinalizeClicked != null) {
				FinalizeClicked.Invoke();
			}
		}

		private void MapSim_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (MapSimulationClicked != null) {
				MapSimulationClicked.Invoke();
			}
		}

		private void RegenMinimap_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (RegenerateMinimapClicked != null) {
				RegenerateMinimapClicked.Invoke();
			}
		}

		private void Snapping_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (SnappingToggled != null) {
				SnappingToggled.Invoke(((RibbonToggleButton) e.OriginalSource).IsChecked.Value);
			}
		}

		private void Random_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (RandomTilesToggled != null) {
				RandomTilesToggled.Invoke(((RibbonToggleButton) e.OriginalSource).IsChecked.Value);
			}
		}

		private void InfoMode_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (InfoModeToggled != null) {
				InfoModeToggled.Invoke(((RibbonToggleButton) e.OriginalSource).IsChecked.Value);
			}
		}

		private void HaRepacker_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (HaRepackerClicked != null) {
				HaRepackerClicked.Invoke();
			}
		}

		private void Export_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (ExportClicked != null) {
				ExportClicked.Invoke();
			}
		}

		/// <summary>
		/// Edit map physics
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PhysicsEdit_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (MapPhysicsClicked != null) {
				MapPhysicsClicked.Invoke();
			}
		}


		/// <summary>
		/// Show map 'info' properties clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowMapProperties_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (ShowMapPropertiesClicked != null) {
				ShowMapPropertiesClicked.Invoke();
			}
		}


		#region Layer UI

		private void LayerUp_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (UserSettings.InverseUpDown && sender != null) {
				LayerDown_Executed(null, null);
			} else if (layerBox.IsEnabled && layerBox.SelectedIndex != layerBox.Items.Count - 1) {
				layerBox.SelectedIndex++;
			}
		}

		private void LayerDown_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (UserSettings.InverseUpDown && sender != null) {
				LayerUp_Executed(null, null);
			} else if (layerBox.IsEnabled && layerBox.SelectedIndex != 0) {
				layerBox.SelectedIndex--;
			}
		}

		private void PlatformUp_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (UserSettings.InverseUpDown && sender != null) {
				PlatformDown_Executed(null, null);
			} else if (platformBox.IsEnabled && platformBox.SelectedIndex != platformBox.Items.Count - 1) {
				platformBox.SelectedIndex++;
			}
		}

		private void PlatformDown_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (UserSettings.InverseUpDown && sender != null) {
				PlatformUp_Executed(null, null);
			} else if (platformBox.IsEnabled && platformBox.SelectedIndex != 0) {
				platformBox.SelectedIndex--;
			}
		}

		private void NewPlatform_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (NewPlatformClicked != null) {
				NewPlatformClicked();
			}
		}

		private void UserObjs_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (UserObjsClicked != null) {
				UserObjsClicked();
			}
		}

		private void beginInternalEditing() {
			changingIndexCnt++;
		}

		private void endInternalEditing() {
			changingIndexCnt--;
		}

		private bool isInternal => changingIndexCnt > 0;

		private void UpdateLocalLayerInfo() {
			actualLayerIndex = layerBox.SelectedIndex;
			actualPlatform = platformBox.SelectedItem == null ? 0 : (int) platformBox.SelectedItem;
		}

		private void UpdateRemoteLayerInfo() {
			if (LayerViewChanged != null) {
				LayerViewChanged.Invoke(actualLayerIndex, actualPlatform, layerCheckbox.IsChecked.Value,
					layerCheckbox.IsChecked.Value || platformCheckbox.IsChecked.Value);
			}
		}

		private void AllLayerView_Executed(object sender, ExecutedRoutedEventArgs e) {
			UpdateRemoteLayerInfo();
		}

		private void AllPlatformView_Executed(object sender, ExecutedRoutedEventArgs e) {
			UpdateRemoteLayerInfo();
		}

		private void LoadPlatformsForLayer(SortedSet<int> zms) {
			beginInternalEditing();

			platformBox.ClearItems();
			foreach (var zm in zms) {
				platformBox.Items.Add(new HaListItem(zm.ToString(), zm));
			}

			platformBox.SelectedIndex = 0;

			endInternalEditing();
		}

		private void layerBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!isInternal) {
				LoadPlatformsForLayer(layers[layerBox.SelectedIndex].zMList);
				UpdateLocalLayerInfo();
				UpdateRemoteLayerInfo();
			}
		}

		private void platformBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!isInternal) {
				UpdateLocalLayerInfo();
				UpdateRemoteLayerInfo();
			}
		}

		public void SetSelectedLayer(int layer, int platform, bool allLayers, bool allPlats) {
			// Disable callbacks
			beginInternalEditing();

			// Set layer info
			layerCheckbox.IsChecked = allLayers;
			layerBox.SelectedIndex = layer;
			LoadPlatformsForLayer(layers[layer].zMList);

			// Set platform info
			platformCheckbox.IsChecked = allPlats;
			platformBox.SelectedIndex = layers[layer].zMList.ToList().IndexOf(platform);
			actualPlatform = platform;

			// Update stuff
			UpdateLocalLayerInfo();

			// Re-enable callbacks
			endInternalEditing();
		}

		public void SetLayers(Layer[] layers) {
			beginInternalEditing();

			this.layers = layers;
			layerBox.ClearItems();
			for (var i = 0; i < layers.Length; i++) {
				layerBox.Items.Add(new HaListItem(layers[i].ToString(), i));
			}

			endInternalEditing();
		}

		public void SetLayer(Layer layer) {
			beginInternalEditing();

			var oldIdx = layerBox.SelectedIndex;
			var i = layer.LayerNumber;
			layerBox.Items[i].Text = layer.ToString();
			layerBox.SelectedIndex = oldIdx;

			endInternalEditing();
		}

		#endregion

		public delegate void EmptyEvent();

		public delegate void ViewToggleEvent(bool? tiles, bool? objs, bool? npcs, bool? mobs, bool? reactors,
			bool? portals, bool? footholds, bool? ropes, bool? chairs, bool? tooltips, bool? backgrounds, bool? misc,
			bool? mirrorField);

		public delegate void ToggleEvent(bool pressed);

		public delegate void LayerViewChangedEvent(int layer, int platform, bool allLayers, bool allPlats);

		public event EmptyEvent NewClicked;
		public event EmptyEvent OpenClicked;
		public event EmptyEvent SaveClicked;
		public event EmptyEvent RepackClicked;
		public event EmptyEvent AboutClicked;
		public event EmptyEvent HelpClicked;
		public event EmptyEvent SettingsClicked;
		public event EmptyEvent ExitClicked;
		public event EmptyEvent FinalizeClicked;
		public event ViewToggleEvent ViewToggled;
		public event ToggleEvent ShowMinimapToggled;
		public event ToggleEvent ParallaxToggled;
		public event LayerViewChangedEvent LayerViewChanged;
		public event EmptyEvent MapSimulationClicked;
		public event EmptyEvent RegenerateMinimapClicked;
		public event ToggleEvent SnappingToggled;
		public event ToggleEvent RandomTilesToggled;
		public event ToggleEvent InfoModeToggled;
		public event EmptyEvent HaRepackerClicked;
		public event EmptyEvent ExportClicked;
		public event EmptyEvent NewPlatformClicked;
		public event EmptyEvent UserObjsClicked;
		public event EmptyEvent MapPhysicsClicked;
		public event EmptyEvent ShowMapPropertiesClicked;
		public event EventHandler<KeyEventArgs> RibbonKeyDown;
		public event ToggleEvent AltBackgroundToggled;
		public event ToggleEvent FhSideToggled;

		public void SetVisibilityCheckboxes(bool? tiles, bool? objs, bool? npcs, bool? mobs, bool? reactors,
			bool? portals, bool? footholds, bool? ropes, bool? chairs, bool? tooltips, bool? backgrounds, bool? misc,
			bool? mirrorField) {
			tilesCheck.IsChecked = tiles;
			objsCheck.IsChecked = objs;
			npcsCheck.IsChecked = npcs;
			mobsCheck.IsChecked = mobs;
			reactCheck.IsChecked = reactors;
			portalCheck.IsChecked = portals;
			fhCheck.IsChecked = footholds;
			ropeCheck.IsChecked = ropes;
			chairCheck.IsChecked = chairs;
			tooltipCheck.IsChecked = tooltips;
			bgCheck.IsChecked = backgrounds;
			miscCheck.IsChecked = misc;
			mirrorFieldDataCheck.IsChecked = mirrorField;
		}

		public void SetEnabled(bool enabled) {
			viewTab.IsEnabled = enabled;
			toolsTab.IsEnabled = enabled;
			saveBtn.IsEnabled = enabled;
			exportBtn.IsEnabled = enabled;
			//resetLayerBoxIfNeeded();
		}

		public void SetOptions(bool minimap, bool parallax, bool snap, bool random, bool infomode) {
			minimapBtn.IsChecked = minimap;
			parallaxBtn.IsChecked = parallax;
			snapBtn.IsChecked = snap;
			randomBtn.IsChecked = random;
			infomodeBtn.IsChecked = infomode;
		}

		public void SetHasMinimap(bool hasMinimap) {
			this.hasMinimap = hasMinimap;
			CommandManager.InvalidateRequerySuggested();
		}

		private void ChangeAllCheckboxes(bool? state) {
			foreach (var cb in new[] {
				         tilesCheck, objsCheck, npcsCheck, mobsCheck, reactCheck, portalCheck, fhCheck, ropeCheck,
				         chairCheck, tooltipCheck, bgCheck, miscCheck, mirrorFieldDataCheck
			         }) {
				cb.IsChecked = state;
			}

			ViewBoxes_Executed(null, null);
		}

		private void allFullCheck_Click(object sender, RoutedEventArgs e) {
			ChangeAllCheckboxes(true);
		}

		private void allHalfCheck_Click(object sender, RoutedEventArgs e) {
			ChangeAllCheckboxes(null);
		}

		private void allClearCheck_Click(object sender, RoutedEventArgs e) {
			ChangeAllCheckboxes(false);
		}

		protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e) {
			base.OnPreviewKeyDown(e);
			if (e.Key != Key.Down && e.Key != Key.Up && RibbonKeyDown != null) {
				RibbonKeyDown.Invoke(this, e);
			}
		}

		/// <summary>
		/// On simulator preview resolution changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void comboBox_Resolution_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (comboBox_Resolution.SelectedItem == null) {
				return;
			}

			var selectedItem = (RenderResolution) (comboBox_Resolution.SelectedItem as ComboBoxItem).Tag;
			UserSettings.SimulateResolution =
				selectedItem; // combo box selection. 800x600, 1024x768, 1280x720, 1920x1080
		}

		private void AltBackgroundToggle_OnClick(object sender, RoutedEventArgs e) {
			AltBackgroundToggled?.Invoke(((CheckBox) e.OriginalSource).IsChecked.Value);
		}

		private void FhSideToggle_OnClick(object sender, RoutedEventArgs e) {
			FhSideToggled?.Invoke(((CheckBox) e.OriginalSource).IsChecked.Value);
		}
	}
}