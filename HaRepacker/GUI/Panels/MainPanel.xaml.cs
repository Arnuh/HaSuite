﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using HaRepacker.Comparer;
using HaRepacker.GUI.Input;
using HaSharedLibrary.GUI;
using MapleLib.Helpers;
using MapleLib.WzLib;
using MapleLib.WzLib.Serialization;
using MapleLib.WzLib.Spine;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure.Data;
using static MapleLib.Configuration.UserSettings;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Clipboard;
using Control = System.Windows.Forms.Control;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using Image = System.Drawing.Image;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using Point = System.Drawing.Point;
using TextDataFormat = System.Windows.TextDataFormat;
using UserControl = System.Windows.Controls.UserControl;

namespace HaRepacker.GUI.Panels {
	/// <summary>
	/// Interaction logic for MainPanelXAML.xaml
	/// </summary>
	public partial class MainPanel : UserControl {
		// Constants
		private const string FIELD_LIMIT_OBJ_NAME = "fieldLimit";
		private const string FIELD_TYPE_OBJ_NAME = "fieldType";
		private const string PORTAL_NAME_OBJ_NAME = "pn";

		private readonly MainForm _mainForm;

		public MainForm MainForm {
			get => _mainForm;
			private set { }
		}

		public TreeViewMS DataTree => _dataTree;

		// Etc
		private readonly UndoRedoManager undoRedoMan;

		private bool isSelectingWzMapFieldLimit;
		private bool isLoading;

		/// <summary>
		/// Constructor
		/// </summary>
		public MainPanel(MainForm mainForm) {
			InitializeComponent();

			isLoading = true;

			_mainForm = mainForm;
			_dataTree.MainPanel = this;

			// Events
#if DEBUG
			toolStripStatusLabel_debugMode.Visibility = Visibility.Visible;
#else
			toolStripStatusLabel_debugMode.Visibility = Visibility.Collapsed;
#endif

			// undo redo
			undoRedoMan = new UndoRedoManager(this);

			// Set theme color
			if (Program.ConfigurationManager.UserSettings.ThemeColor == (int) UserSettingsThemeColor.Dark) {
				VisualStateManager.GoToState(this, "BlackTheme", false);
				DataTree.Background = Brushes.Black;
				DataTree.Foreground = Brushes.White;
			}

			nameBox.Header = "Name";
			textPropBox.Header = "Value";
			textPropBox.ButtonClicked += applyChangesButton_Click;

			vectorPanel.ButtonClicked += VectorPanel_ButtonClicked;

			textPropBox.Visibility = Visibility.Collapsed;
			//nameBox.Visibility = Visibility.Collapsed;

			// Storyboard
			var sbb =
				(Storyboard) FindResource("Storyboard_Find_FadeIn");
			sbb.Completed += Storyboard_Find_FadeIn_Completed;


			// buttons
			menuItem_Animate.Visibility = Visibility.Collapsed;
			menuItem_changeImage.Visibility = Visibility.Collapsed;
			menuItem_changeSound.Visibility = Visibility.Collapsed;
			menuItem_saveSound.Visibility = Visibility.Collapsed;
			menuItem_saveImage.Visibility = Visibility.Collapsed;
			menuItem_ChangePixelFormat.Visibility = Visibility.Collapsed;

			textEditor.SaveButtonClicked += TextEditor_SaveButtonClicked;
			Loaded += MainPanelXAML_Loaded;

			isLoading = false;
		}

		private void MainPanelXAML_Loaded(object sender, RoutedEventArgs e) {
			fieldLimitPanel1.SetTextboxOnFieldLimitChange(textPropBox);
			fieldTypePanel.SetTextboxOnFieldTypeChange(textPropBox);
		}

		#region Exported Fields

		public UndoRedoManager UndoRedoMan => undoRedoMan;

		#endregion

		#region Image directory add

		/// <summary>
		/// WzDirectory
		/// </summary>
		/// <param name="target"></param>
		public void AddWzDirectoryToSelectedNode(WzNode target) {
			if (!(target.Tag is WzDirectory) && !(target.Tag is WzFile)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!NameInputBox.Show(Properties.Resources.MainAddDir, 0, out var name)) {
				return;
			}

			var obj = (WzObject) target.Tag;
			// Context menu allows this so we'll allow it here
			// since I'm not sure what restrictions a WzDirectory might have.
			if (obj is WzFile || obj is WzDirectory) {
				var topMostWzFileParent = obj.WzFileParent;
				if (topMostWzFileParent != null) {
					((WzNode) target).AddObject(new WzDirectory(name, topMostWzFileParent), UndoRedoMan);
					return;
				}
			}

			MessageBox.Show(Properties.Resources.MainTreeAddDirError);
		}

		/// <summary>
		/// WzDirectory
		/// </summary>
		/// <param name="target"></param>
		public void AddWzImageToSelectedNode(WzNode target) {
			string name;
			if (!(target.Tag is WzDirectory) && !(target.Tag is WzFile)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!NameInputBox.Show(Properties.Resources.MainAddImg, 0, out name)) {
				return;
			}

			((WzNode) target).AddObject(new WzImage(name) {Changed = true}, UndoRedoMan);
		}

		/// <summary>
		/// WzByteProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzByteFloatToSelectedNode(WzNode target) {
			string name;
			double? d;
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!FloatingPointInputBox.Show(Properties.Resources.MainAddFloat, out name, out d)) {
				return;
			}

			((WzNode) target).AddObject(new WzFloatProperty(name, (float) d), UndoRedoMan);
		}

		/// <summary>
		/// WzCanvasProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzCanvasToSelectedNode(WzNode target) {
			string name;
			var bitmaps = new List<Bitmap>();
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!BitmapInputBox.Show(Properties.Resources.MainAddCanvas, out name, out bitmaps)) {
				return;
			}

			if (!PixelFormatSelector.Show((int) WzPngProperty.CanvasPixFormat.Argb8888, out var pixelFormat)) {
				return;
			}

			var wzNode = (WzNode) target;

			var i = 0;
			foreach (var bmp in bitmaps) {
				var canvas = new WzCanvasProperty(bitmaps.Count == 1 ? name : name + i);
				var pngProperty = new WzPngProperty();
				pngProperty.PixFormat = pixelFormat;
				pngProperty.SetImage(bmp);
				canvas.PngProperty = pngProperty;

				var newInsertedNode = wzNode.AddObject(canvas, UndoRedoMan);
				// Add an additional WzVectorProperty with X Y of 0,0
				newInsertedNode.AddObject(
					new WzVectorProperty(WzCanvasProperty.OriginPropertyName, new WzIntProperty("X", 0),
						new WzIntProperty("Y", 0)), UndoRedoMan);

				i++;
			}
		}

		/// <summary>
		/// WzCompressedInt
		/// </summary>
		/// <param name="target"></param>
		public void AddWzCompressedIntToSelectedNode(WzNode target) {
			string name;
			int? value;
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!IntInputBox.Show(
				    Properties.Resources.MainAddInt,
				    "", 0,
				    out name, out value)) {
				return;
			}

			((WzNode) target).AddObject(new WzIntProperty(name, (int) value), UndoRedoMan);
		}

		/// <summary>
		/// WzLongProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzLongToSelectedNode(WzNode target) {
			string name;
			long? value;
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!LongInputBox.Show(Properties.Resources.MainAddInt, out name, out value)) {
				return;
			}

			((WzNode) target).AddObject(new WzLongProperty(name, (long) value), UndoRedoMan);
		}

		/// <summary>
		/// WzConvexProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzConvexPropertyToSelectedNode(WzNode target) {
			string name;
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!NameInputBox.Show(Properties.Resources.MainAddConvex, 0, out name)) {
				return;
			}

			((WzNode) target).AddObject(new WzConvexProperty(name), UndoRedoMan);
		}

		/// <summary>
		/// WzNullProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzDoublePropertyToSelectedNode(WzNode target) {
			string name;
			double? d;
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!FloatingPointInputBox.Show(Properties.Resources.MainAddDouble, out name, out d)) {
				return;
			}

			((WzNode) target).AddObject(new WzDoubleProperty(name, (double) d), UndoRedoMan);
		}

		/// <summary>
		/// WzNullProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzNullPropertyToSelectedNode(WzNode target) {
			string name;
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!NameInputBox.Show(Properties.Resources.MainAddNull, 0, out name)) {
				return;
			}

			((WzNode) target).AddObject(new WzNullProperty(name), UndoRedoMan);
		}

		/// <summary>
		/// WzSoundProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzSoundPropertyToSelectedNode(WzNode target) {
			string name;
			string path;
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!SoundInputBox.Show(Properties.Resources.MainAddSound, out name, out path)) {
				return;
			}

			((WzNode) target).AddObject(new WzSoundProperty(name, path), UndoRedoMan);
		}

		/// <summary>
		/// WzStringProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzStringPropertyToSelectedIndex(WzNode target) {
			string name;
			string value;
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!NameValueInputBox.Show(Properties.Resources.MainAddString, out name, out value)) {
				return;
			}

			((WzNode) target).AddObject(new WzStringProperty(name, value), UndoRedoMan);
		}

		/// <summary>
		/// WzSubProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzSubPropertyToSelectedIndex(WzNode target) {
			string name;
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!NameInputBox.Show(Properties.Resources.MainAddSub, 0, out name)) {
				return;
			}

			((WzNode) target).AddObject(new WzSubProperty(name), UndoRedoMan);
		}

		/// <summary>
		/// WzUnsignedShortProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzUnsignedShortPropertyToSelectedIndex(WzNode target) {
			string name;
			int? value;
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!IntInputBox.Show(Properties.Resources.MainAddShort,
				    "", 0,
				    out name, out value)) {
				return;
			}

			((WzNode) target).AddObject(new WzShortProperty(name, (short) value), UndoRedoMan);
		}

		/// <summary>
		/// WzUOLProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzUOLPropertyToSelectedIndex(WzNode target) {
			string name;
			string value;
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!NameValueInputBox.Show(Properties.Resources.MainAddLink, out name, out value)) {
				return;
			}

			((WzNode) target).AddObject(new WzUOLProperty(name, value), UndoRedoMan);
		}

		/// <summary>
		/// WzVectorProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzVectorPropertyToSelectedIndex(WzNode target) {
			string name;
			Point? pt;
			if (!(target.Tag is IPropertyContainer)) {
				Warning.Error(Properties.Resources.MainCannotInsertToNode);
				return;
			}

			if (!VectorInputBox.Show(Properties.Resources.MainAddVec, out name, out pt)) {
				return;
			}

			((WzNode) target).AddObject(
				new WzVectorProperty(name, new WzIntProperty("X", ((Point) pt).X),
					new WzIntProperty("Y", ((Point) pt).Y)), UndoRedoMan);
		}

		/// <summary>
		/// WzLuaProperty
		/// </summary>
		/// <param name="target"></param>
		public void AddWzLuaPropertyToSelectedIndex(WzNode target) {
			/*           string name;
			           string value;
			           if (!(target.Tag is WzDirectory) && !(target.Tag is WzFile))
			           {
			               Warning.Error(Properties.Resources.MainCannotInsertToNode);
			               return;
			           }
			           else if (!NameValueInputBox.Show(Properties.Resources.MainAddString, out name, out value))
			               return;

			           string propertyName = name;
			           if (!propertyName.EndsWith(".lua"))
			           {
			               propertyName += ".lua"; // it must end with .lua regardless
			           }
			           ((WzNode)target).AddObject(new WzImage(propertyName), UndoRedoMan);*/
		}

		/// <summary>
		/// Remove selected nodes
		/// </summary>
		public void PromptRemoveSelectedTreeNodes() {
			if (!Warning.Warn(Properties.Resources.MainConfirmRemove)) return;

			var actions = new List<UndoRedoAction>();

			var nodeArr = new WzNode[DataTree.SelectedNodes.Count];
			DataTree.SelectedNodes.CopyTo(nodeArr, 0);

			foreach (WzNode node in nodeArr) {
				// No parent is considered an unload
				// Don't support redo
				if (node.Parent is WzNode parentNode) {
					actions.Add(UndoRedoManager.ObjectRemoved(parentNode, node));
				}

				node.DeleteWzNode();
			}

			UndoRedoMan.AddUndoBatch(actions);
		}

		/// <summary>
		/// Rename an individual node
		/// </summary>
		public void PromptRenameWzTreeNode(WzNode node) {
			if (node == null) {
				return;
			}

			var newName = "";
			var wzNode = node;
			if (RenameInputBox.Show(Properties.Resources.MainConfirmRename, wzNode.Text, out newName)) {
				wzNode.ChangeName(newName);
			}
		}

		#endregion

		#region Panel Loading Events

		/// <summary>
		/// Set panel loading splash screen from MainForm.cs
		/// <paramref name="currentDispatcher"/>
		/// </summary>
		public void OnSetPanelLoading(Dispatcher currentDispatcher = null) {
			Action action = () => {
				loadingPanel.OnStartAnimate();
				grid_LoadingPanel.Visibility = Visibility.Visible;
				DataTree.Visibility = Visibility.Collapsed;
			};
			if (currentDispatcher != null) {
				currentDispatcher.BeginInvoke(action);
			} else {
				grid_LoadingPanel.Dispatcher.BeginInvoke(action);
			}
		}

		/// <summary>
		/// Remove panel loading splash screen from MainForm.cs
		/// <paramref name="currentDispatcher"/>
		/// </summary>
		public void OnSetPanelLoadingCompleted(Dispatcher currentDispatcher = null) {
			Action action = () => {
				loadingPanel.OnPauseAnimate();
				grid_LoadingPanel.Visibility = Visibility.Collapsed;
				DataTree.Visibility = Visibility.Visible;
			};
			if (currentDispatcher != null) {
				currentDispatcher.BeginInvoke(action);
			} else {
				grid_LoadingPanel.Dispatcher.BeginInvoke(action);
			}
		}

		#endregion

		#region Animate

		/// <summary>
		/// Animate the list of selected canvases
		/// </summary>
		public void StartAnimateSelectedCanvas() {
			if (DataTree.SelectedNodes.Count == 0) {
				MessageBox.Show("Please select at least one or more canvas node.");
				return;
			}

			var selectedNodes = new List<WzObject>();
			if (DataTree.SelectedNodes.Count == 1 && !int.TryParse(DataTree.SelectedNode.Name, out _)) {
				var obj = (WzObject) DataTree.SelectedNode.Tag;
				for (var i = 0;; i++) {
					var child = obj[i.ToString()];
					if (child == null) break;
					selectedNodes.Add(child);
				}
			} else {
				foreach (WzNode node in DataTree.SelectedNodes) selectedNodes.Add((WzObject) node.Tag);
			}

			var path_title = "Animate";
			if (DataTree.SelectedNodes[0] != null && DataTree.SelectedNode.Parent != null) {
				path_title = (DataTree.SelectedNode.Parent as WzNode).GetFullPath();
			}

			var thread = new Thread(() => {
				try {
					var previewWnd = new ImageAnimationPreviewWindow(selectedNodes, path_title);
					previewWnd.Run();
				} catch (Exception ex) {
					MessageBox.Show("Error previewing animation. " + ex);
				}
			});
			thread.Start();
			// thread.Join();
		}

		private void nextLoopTime_comboBox_SelectedIndexChanged(object sender, EventArgs e) {
			/* if (nextLoopTime_comboBox == null)
			      return;

			  switch (nextLoopTime_comboBox.SelectedIndex)
			  {
			      case 1:
			          Program.ConfigurationManager.UserSettings.DelayNextLoop = 1000;
			          break;
			      case 2:
			          Program.ConfigurationManager.UserSettings.DelayNextLoop = 2000;
			          break;
			      case 3:
			          Program.ConfigurationManager.UserSettings.DelayNextLoop = 5000;
			          break;
			      case 4:
			          Program.ConfigurationManager.UserSettings.DelayNextLoop = 10000;
			          break;
			      default:
			          Program.ConfigurationManager.UserSettings.DelayNextLoop = Program.TimeStartAnimateDefault;
			          break;
			  }*/
		}

		#endregion

		#region Buttons

		private void nameBox_ButtonClicked(object sender, EventArgs e) {
			if (DataTree.SelectedNode == null) return;
			if (DataTree.SelectedNode.Tag is WzFile) {
				((WzFile) DataTree.SelectedNode.Tag).Header.Copyright = nameBox.Text;
				((WzFile) DataTree.SelectedNode.Tag).Header.RecalculateFileStart();
			} else if (WzNode.CanNodeBeInserted((WzNode) DataTree.SelectedNode.Parent, nameBox.Text)) {
				var text = nameBox.Text;
				((WzNode) DataTree.SelectedNode).ChangeName(text);
				nameBox.Text = text;
				nameBox.ApplyButtonEnabled = false;
			} else {
				Warning.Error(Properties.Resources.MainNodeExists);
			}
		}

		/// <summary>
		/// On vector panel 'apply' button clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void VectorPanel_ButtonClicked(object sender, EventArgs e) {
			applyChangesButton_Click(null, null);
		}

		private void applyChangesButton_Click(object sender, EventArgs e) {
			if (DataTree.SelectedNode == null) {
				return;
			}

			var setText = textPropBox.Text;

			var obj = (WzObject) DataTree.SelectedNode.Tag;
			if (obj is WzImageProperty imageProperty) imageProperty.ParentImage.Changed = true;

			if (obj is WzVectorProperty vectorProperty) {
				vectorProperty.X.Value = vectorPanel.X;
				vectorProperty.Y.Value = vectorPanel.Y;
			} else if (obj is WzStringProperty stringProperty) {
				if (!stringProperty.IsSpineAtlasResources) {
					stringProperty.Value = setText;
				} else {
					throw new NotSupportedException("Usage of textBoxProp for spine WzStringProperty.");
				}
			} else if (obj is WzFloatProperty floatProperty) {
				float val;
				if (!float.TryParse(setText, out val)) {
					Warning.Error(string.Format(Properties.Resources.MainConversionError, setText));
					return;
				}

				floatProperty.Value = val;
			} else if (obj is WzIntProperty intProperty) {
				int val;
				if (!int.TryParse(setText, out val)) {
					Warning.Error(string.Format(Properties.Resources.MainConversionError, setText));
					return;
				}

				intProperty.Value = val;
			} else if (obj is WzLongProperty longProperty) {
				long val;
				if (!long.TryParse(setText, out val)) {
					Warning.Error(string.Format(Properties.Resources.MainConversionError, setText));
					return;
				}

				longProperty.Value = val;
			} else if (obj is WzDoubleProperty doubleProperty) {
				double val;
				if (!double.TryParse(setText, out val)) {
					Warning.Error(string.Format(Properties.Resources.MainConversionError, setText));
					return;
				}

				doubleProperty.Value = val;
			} else if (obj is WzShortProperty shortProperty) {
				short val;
				if (!short.TryParse(setText, out val)) {
					Warning.Error(string.Format(Properties.Resources.MainConversionError, setText));
					return;
				}

				shortProperty.Value = val;
			} else if (obj is WzUOLProperty UOLProperty) {
				UOLProperty.Value = setText;
			} else if (obj is WzLuaProperty) {
				throw new NotSupportedException("Moved to TextEditor_SaveButtonClicked()");
			}
		}

		/// <summary>
		/// On texteditor save button clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TextEditor_SaveButtonClicked(object sender, EventArgs e) {
			if (DataTree.SelectedNode == null) {
				return;
			}

			var obj = (WzObject) DataTree.SelectedNode.Tag;
			if (obj is WzLuaProperty luaProp) {
				var setText = textEditor.textEditor.Text;
				var encBytes = luaProp.EncodeDecode(Encoding.ASCII.GetBytes(setText));
				luaProp.Value = encBytes;
			} else if (obj is WzStringProperty stringProp) {
				//if (stringProp.IsSpineAtlasResources)
				// {
				var setText = textEditor.textEditor.Text;

				stringProp.Value = setText;
				/*  }
				  else
				  {
				      throw new NotSupportedException("Usage of TextEditor for non-spine WzStringProperty.");
				  }*/
			}
		}

		/// <summary>
		/// More option -- Shows ContextMenuStrip 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_MoreOption_Click(object sender, RoutedEventArgs e) {
			var clickSrc = (Button) sender;

			clickSrc.ContextMenu.IsOpen = true;
			//  System.Windows.Forms.ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();
			//  contextMenu.Show(clickSrc, 0, 0);
		}

		/// <summary>
		/// Menu item for animation. Appears when clicking on the "..." button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MenuItem_Animate_Click(object sender, RoutedEventArgs e) {
			StartAnimateSelectedCanvas();
		}

		/// <summary>
		/// Save the image animation into a JPG file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MenuItem_saveImageAnimation_Click(object sender, RoutedEventArgs e) {
			var seletedWzObject = (WzObject) DataTree.SelectedNode.Tag;

			if (!AnimationBuilder.IsValidAnimationWzObject(seletedWzObject)) {
				return;
			}

			// Check executing process architecture
			/*AssemblyName executingAssemblyName = Assembly.GetExecutingAssembly().GetName();
			var assemblyArchitecture = executingAssemblyName.ProcessorArchitecture;
			if (assemblyArchitecture == ProcessorArchitecture.None)
			{
			    System.Windows.Forms.MessageBox.Show(HaRepacker.Properties.Resources.ExecutingAssemblyError, HaRepacker.Properties.Resources.Warning, System.Windows.Forms.MessageBoxButtons.OK);
			    return;
			}*/

			var dialog = new SaveFileDialog {
				Title = Properties.Resources.SelectOutApng,
				Filter = string.Format("{0}|*.png", Properties.Resources.ApngFilter)
			};
			if (dialog.ShowDialog() != DialogResult.OK) {
				return;
			}

			AnimationBuilder.ExtractAnimation((WzSubProperty) seletedWzObject, dialog.FileName,
				Program.ConfigurationManager.UserSettings.UseApngIncompatibilityFrame);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MenuItem_changeImage_Click(object sender, RoutedEventArgs e) {
			if (DataTree.SelectedNode.Tag is WzCanvasProperty) // only allow button click if its an image property
			{
				var dialog = new OpenFileDialog {
					Title = "Select an image",
					Filter =
						"Supported Image Formats (*.png;*.bmp;*.jpg;*.gif;*.jpeg;*.tif;*.tiff)|*.png;*.bmp;*.jpg;*.gif;*.jpeg;*.tif;*.tiff"
				};
				if (dialog.ShowDialog() != DialogResult.OK) {
					return;
				}

				Bitmap bmp;
				try {
					bmp = (Bitmap) Image.FromFile(dialog.FileName);
				} catch {
					Warning.Error(Properties.Resources.MainImageLoadError);
					return;
				}
				//List<UndoRedoAction> actions = new List<UndoRedoAction>(); // Undo action

				ChangeCanvasPropBoxImage(bmp);
			}
		}

		/// <summary>
		/// Changes the displayed image in 'canvasPropBox' with a user defined input.
		/// </summary>
		/// <param name="image"></param>
		/// <param name=""></param>
		private void ChangeCanvasPropBoxImage(Bitmap bmp) {
			if (DataTree.SelectedNode.Tag is WzCanvasProperty property) {
				var selectedWzCanvas = property;

				if (selectedWzCanvas
				    .HaveInlinkProperty()) // if its an inlink property, remove that before updating base image.
				{
					selectedWzCanvas.RemoveProperty(selectedWzCanvas[WzCanvasProperty.InlinkPropertyName]);

					var parentCanvasNode = (WzNode) DataTree.SelectedNode;
					var childInlinkNode = WzNode.GetChildNode(parentCanvasNode, WzCanvasProperty.InlinkPropertyName);

					// Add undo actions
					//actions.Add(UndoRedoManager.ObjectRemoved((WzNode)parentCanvasNode, childInlinkNode));
					childInlinkNode.DeleteWzNode(); // Delete '_inlink' node

					// TODO: changing _Inlink image crashes
					// Mob2.wz/9400121/hit/0
				} else if
					(selectedWzCanvas
					 .HaveOutlinkProperty()) // if its an inlink property, remove that before updating base image.
				{
					selectedWzCanvas.RemoveProperty(selectedWzCanvas[WzCanvasProperty.OutlinkPropertyName]);

					var parentCanvasNode = (WzNode) DataTree.SelectedNode;
					var childInlinkNode =
						WzNode.GetChildNode(parentCanvasNode, WzCanvasProperty.OutlinkPropertyName);

					// Add undo actions
					//actions.Add(UndoRedoManager.ObjectRemoved((WzNode)parentCanvasNode, childInlinkNode));
					childInlinkNode.DeleteWzNode(); // Delete '_inlink' node
				}

				if (!PixelFormatSelector.Show(selectedWzCanvas.PngProperty.PixFormat, out var pixelFormat)) {
					return;
				}

				selectedWzCanvas.PngProperty.PixFormat = pixelFormat;
				selectedWzCanvas.PngProperty.SetImage(bmp);

				// Updates
				selectedWzCanvas.ParentImage.Changed = true;

				canvasPropBox.SetImage(bmp);

				// Add undo actions
				//UndoRedoMan.AddUndoBatch(actions);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MenuItem_changeSound_Click(object sender, RoutedEventArgs e) {
			if (DataTree.SelectedNode.Tag is WzSoundProperty) {
				var dialog = new OpenFileDialog {
					Title = "Select the sound",
					Filter = "Moving Pictures Experts Group Format 1 Audio Layer 3(*.mp3)|*.mp3"
				};
				if (dialog.ShowDialog() != DialogResult.OK) return;
				WzSoundProperty prop;
				try {
					prop = new WzSoundProperty(((WzSoundProperty) DataTree.SelectedNode.Tag).Name, dialog.FileName);
				} catch {
					Warning.Error(Properties.Resources.MainImageLoadError);
					return;
				}

				var parent = (IPropertyContainer) ((WzSoundProperty) DataTree.SelectedNode.Tag).Parent;
				((WzSoundProperty) DataTree.SelectedNode.Tag).ParentImage.Changed = true;
				((WzSoundProperty) DataTree.SelectedNode.Tag).Remove();
				DataTree.SelectedNode.Tag = prop;
				parent.AddProperty(prop);
				mp3Player.SoundProperty = prop;
			}
		}

		/// <summary>
		/// Saving the sound from WzSoundProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MenuItem_saveSound_Click(object sender, RoutedEventArgs e) {
			if (!(DataTree.SelectedNode.Tag is WzSoundProperty) && !(DataTree.SelectedNode.Tag is WzUOLProperty)) {
				return;
			}

			var fileName = string.Empty;
			WzSoundProperty mp3 = null;
			switch (DataTree.SelectedNode.Tag) {
				case WzSoundProperty prop:
					mp3 = prop;
					fileName = prop.Name;
					break;
				case WzUOLProperty uolProp:
					mp3 = (WzSoundProperty) uolProp.LinkValue;
					fileName = uolProp.Name; // We should be using the original name right?
					break;
			}

			if (mp3 == null) {
				return;
			}

			var dialog = new SaveFileDialog {
				FileName = fileName,
				Title = "Select where to save the .mp3 file.",
				Filter = "Moving Pictures Experts Group Format 1 Audio Layer 3 (*.mp3)|*.mp3"
			};
			if (dialog.ShowDialog() != DialogResult.OK) {
				return;
			}

			mp3.SaveToFile(dialog.FileName);
		}

		/// <summary>
		/// Saving the image from WzCanvasProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void menuItem_saveImage_Click(object sender, RoutedEventArgs e) {
			if (!(DataTree.SelectedNode.Tag is WzCanvasProperty) &&
			    !(DataTree.SelectedNode.Tag is WzUOLProperty)) {
				return;
			}

			Bitmap wzCanvasPropertyObjLocation;
			var fileName = string.Empty;

			if (DataTree.SelectedNode.Tag is WzCanvasProperty canvas) {
				wzCanvasPropertyObjLocation = canvas.GetLinkedWzCanvasBitmap();
				fileName = canvas.Name;
			} else {
				var linkValue = ((WzUOLProperty) DataTree.SelectedNode.Tag).LinkValue;
				if (linkValue is WzCanvasProperty linkedCanvas) {
					wzCanvasPropertyObjLocation = linkedCanvas.GetLinkedWzCanvasBitmap();
					fileName = linkValue.Name;
				} else {
					return;
				}
			}

			if (wzCanvasPropertyObjLocation == null) {
				return; // oops, we're fucked lulz
			}

			var dialog = new SaveFileDialog {
				FileName = fileName,
				Title = "Select where to save the image...",
				Filter =
					"Portable Network Graphics (*.png)|*.png|CompuServe Graphics Interchange Format (*.gif)|*.gif|Bitmap (*.bmp)|*.bmp|Joint Photographic Experts Group Format (*.jpg)|*.jpg|Tagged Image File Format (*.tif)|*.tif"
			};
			if (dialog.ShowDialog() != DialogResult.OK) return;
			switch (dialog.FilterIndex) {
				case 1: //png
					wzCanvasPropertyObjLocation.Save(dialog.FileName, ImageFormat.Png);
					break;
				case 2: //gif
					wzCanvasPropertyObjLocation.Save(dialog.FileName, ImageFormat.Gif);
					break;
				case 3: //bmp
					wzCanvasPropertyObjLocation.Save(dialog.FileName, ImageFormat.Bmp);
					break;
				case 4: //jpg
					wzCanvasPropertyObjLocation.Save(dialog.FileName, ImageFormat.Jpeg);
					break;
				case 5: //tiff
					wzCanvasPropertyObjLocation.Save(dialog.FileName, ImageFormat.Tiff);
					break;
			}
		}

		/// <summary>
		/// Export .json, .atlas, as file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void menuItem_ExportFile_Click(object sender, RoutedEventArgs e) {
			if (!(DataTree.SelectedNode.Tag is WzStringProperty)) return;

			var stProperty = DataTree.SelectedNode.Tag as WzStringProperty;

			var fileName = stProperty.Name;
			var value = stProperty.Value;

			var fileNameSplit = fileName.Split('.');
			var fileType = fileNameSplit.Length > 1 ? fileNameSplit[fileNameSplit.Length - 1] : "txt";

			var saveFileDialog1 = new SaveFileDialog {
					FileName = fileName,
					Title = "Select where to save the file...",
					Filter = fileType + " files (*." + fileType + ")|*." + fileType + "|All files (*.*)|*.*"
				}
				;
			if (saveFileDialog1.ShowDialog() != DialogResult.OK) {
				return;
			}

			using (var fs = (FileStream) saveFileDialog1.OpenFile()) {
				using (var sw = new StreamWriter(fs)) {
					sw.WriteLine(value);
				}
			}
		}

		#endregion

		#region Drag and Drop Image

		private bool bDragEnterActive;

		/// <summary>
		/// Scroll viewer drag enter
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void canvasPropBox_DragEnter(object sender, DragEventArgs e) {
			Debug.WriteLine("Drag Enter");
			if (!bDragEnterActive) bDragEnterActive = true;
		}

		/// <summary>
		///  Scroll viewer drag leave
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void canvasPropBox_DragLeave(object sender, DragEventArgs e) {
			Debug.WriteLine("Drag Leave");

			bDragEnterActive = false;
		}

		/// <summary>
		/// Scroll viewer drag drop
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void canvasPropBox_Drop(object sender, DragEventArgs e) {
			Debug.WriteLine("Drag Drop");
			if (bDragEnterActive &&
			    DataTree.SelectedNode.Tag is WzCanvasProperty) // only allow button click if its an image property
			{
				if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
					var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
					if (files.Length == 0) {
						return;
					}

					Bitmap bmp;
					try {
						bmp = (Bitmap) Image.FromFile(files[0]);
					} catch (Exception exp) {
						return;
					}

					if (bmp != null) {
						ChangeCanvasPropBoxImage(bmp);
					}
					//List<UndoRedoAction> actions = new List<UndoRedoAction>(); // Undo action
				}
			}
		}

		#endregion

		#region Copy & Paste

		/// <summary>
		/// Flag to determine if a copy task is currently active.
		/// </summary>
		private bool pasteTaskActive;

		/// <summary>
		/// Copies from the selected Wz object
		/// </summary>
		public void DoCopy() {
			if (!Warning.Warn(Properties.Resources.MainConfirmCopy) || pasteTaskActive) {
				return;
			}

			Program.Config.CopyMode.DoCopy(this, DataTree.SelectedNodes);
		}

		/// <summary>
		/// Paste to the selected WzObject
		/// </summary>
		public void DoPaste() {
			if (!Warning.Warn(Properties.Resources.MainConfirmPaste)) {
				return;
			}

			pasteTaskActive = true;
			try {
				Program.Config.CopyMode.DoPaste(this, DataTree.SelectedNodes);
			} finally {
				pasteTaskActive = false;
			}
		}

		#endregion

		#region UI layout

		/// <summary>
		/// Shows the selected data treeview object to UI
		/// </summary>
		/// <param name="obj"></param>
		public void ShowObjectValue(WzNode node, WzObject obj) {
			if (obj.WzFileParent != null &&
			    obj.WzFileParent
				    .IsUnloaded) // this WZ is already unloaded from memory, dont attempt to display it (when the user clicks "reload" button while selection is on that)
			{
				return;
			}

			mp3Player.SoundProperty = null;
			nameBox.Text = obj is WzFile file ? file.Header.Copyright : obj.Name;
			nameBox.ApplyButtonEnabled = false;

			toolStripStatusLabel_additionalInfo.Text = "-"; // Reset additional info to default
			if (isSelectingWzMapFieldLimit) // previously already selected. update again
			{
				isSelectingWzMapFieldLimit = false;
			}

			// Canvas animation
			if (DataTree.SelectedNodes.Count <= 1) {
				menuItem_Animate.Visibility =
					Visibility.Collapsed; // set invisible regardless if none of the nodes are selected.
			} else {
				var bIsAllCanvas = true;
				// check if everything selected is WzUOLProperty and WzCanvasProperty
				foreach (WzNode tree in DataTree.SelectedNodes) {
					var wzobj = (WzObject) tree.Tag;
					if (!(wzobj is WzUOLProperty) && !(wzobj is WzCanvasProperty)) {
						bIsAllCanvas = false;
						break;
					}
				}

				menuItem_Animate.Visibility = bIsAllCanvas ? Visibility.Visible : Visibility.Collapsed;
			}

			// Set default layout collapsed state
			mp3Player.Visibility = Visibility.Collapsed;

			// Button collapsed state
			menuItem_changeImage.Visibility = Visibility.Collapsed;
			menuItem_saveImage.Visibility = Visibility.Collapsed;
			menuItem_changeSound.Visibility = Visibility.Collapsed;
			menuItem_saveSound.Visibility = Visibility.Collapsed;
			menuItem_exportFile.Visibility = Visibility.Collapsed;
			menuItem_ChangePixelFormat.Visibility = Visibility.Collapsed;

			// Canvas collapsed state
			canvasPropBox.Visibility = Visibility.Collapsed;

			// Value
			textPropBox.Visibility = Visibility.Collapsed;

			// Field limit panel Map.wz/../fieldLimit
			fieldLimitPanelHost.Visibility = Visibility.Collapsed;
			// fieldType panel Map.wz/../fieldType
			fieldTypePanel.Visibility = Visibility.Collapsed;

			// Vector panel
			vectorPanel.Visibility = Visibility.Collapsed;

			// Avalon Text editor
			textEditor.Visibility = Visibility.Collapsed;

			// vars
			var bIsWzLuaProperty = obj is WzLuaProperty;
			var bIsWzSoundProperty = obj is WzSoundProperty;
			var bIsWzStringProperty = obj is WzStringProperty;
			var bIsWzIntProperty = obj is WzIntProperty;
			var bIsWzLongProperty = obj is WzLongProperty;
			var bIsWzDoubleProperty = obj is WzDoubleProperty;
			var bIsWzFloatProperty = obj is WzFloatProperty;
			var bIsWzShortProperty = obj is WzShortProperty;

			var bAnimateMoreButton = false; // The button to animate when there is more option under button_MoreOption

			// Set layout visibility
			if (obj is WzFile || obj is WzDirectory || obj is WzImage || obj is WzNullProperty ||
			    obj is WzSubProperty || obj is WzConvexProperty) {
				/*if (obj is WzSubProperty) { // detect String.wz/Npc.img/ directory for AI related tools
				     if (obj.Parent.Name == "Npc.img")
				     {
				         WzObject wzObj = obj.GetTopMostWzDirectory();
				         if (wzObj.Name == "String.wz" || (wzObj.Name.StartsWith("String") && wzObj.Name.EndsWith(".wz")))
				         {
				         }
				     }
				 }*/
			} else if (obj is WzCanvasProperty canvasProp) {
				bAnimateMoreButton = true; // flag

				UpdateImageView(node, canvasProp, true);
			} else if (obj is WzUOLProperty uolProperty) {
				bAnimateMoreButton = true; // flag

				// Image
				var linkValue = uolProperty.LinkValue;
				if (linkValue is WzCanvasProperty canvasUOL) {
					UpdateImageView(node, canvasUOL, false);
				} else if (linkValue is WzSoundProperty binProperty) { // Sound, used rarely in wz. i.e Sound.wz/Rune/1/Destroy
					mp3Player.Visibility = Visibility.Visible;
					mp3Player.SoundProperty = binProperty;

					menuItem_changeSound.Visibility = Visibility.Visible;
					menuItem_saveSound.Visibility = Visibility.Visible;
				}

				// Value
				textPropBox.Visibility = Visibility.Visible;
				textPropBox.ApplyButtonEnabled = false; // reset to disabled mode when changed
				textPropBox.Text = obj.ToString();
			} else if (bIsWzSoundProperty) {
				bAnimateMoreButton = true; // flag

				mp3Player.Visibility = Visibility.Visible;
				mp3Player.SoundProperty = (WzSoundProperty) obj;

				menuItem_changeSound.Visibility = Visibility.Visible;
				menuItem_saveSound.Visibility = Visibility.Visible;
			} else if (bIsWzLuaProperty) {
				textEditor.Visibility = Visibility.Visible;
				textEditor.SetHighlightingDefinitionIndex(2); // javascript

				textEditor.textEditor.Text = obj.ToString();
			} else if (bIsWzStringProperty || bIsWzIntProperty || bIsWzLongProperty || bIsWzDoubleProperty ||
			           bIsWzFloatProperty || bIsWzShortProperty) {
				// If text is a string property, expand the textbox
				if (bIsWzStringProperty) {
					var stringObj = (WzStringProperty) obj;

					if (stringObj.IsSpineAtlasResources) { // spine related resource
					
						bAnimateMoreButton = true;
						menuItem_exportFile.Visibility = Visibility.Visible;

						textEditor.Visibility = Visibility.Visible;
						textEditor.SetHighlightingDefinitionIndex(20); // json
						textEditor.textEditor.Text = obj.ToString();


						var path_title = stringObj.Parent?.FullPath ?? "Animate";

						var thread = new Thread(() => {
							try {
								var item = new WzSpineAnimationItem(stringObj);

								// Create xna window
								var Window = new SpineAnimationWindow(item, path_title);
								Window.Run();
							} catch (Exception e) {
								Warning.Error("Error initialising/ rendering spine object. " + e);
							}
						});
						thread.Start();
						thread.Join();
					} else if (stringObj.Name.EndsWith(".json")) { // Map001.wz/Back/BM3_3.img/spine/skeleton.json
						bAnimateMoreButton = true;
						menuItem_exportFile.Visibility = Visibility.Visible;

						textEditor.Visibility = Visibility.Visible;
						textEditor.SetHighlightingDefinitionIndex(20); // json
						textEditor.textEditor.Text = obj.ToString();
					} else {
						// Value
						textPropBox.Visibility = Visibility.Visible;
						textPropBox.Text = obj.ToString();
						textPropBox.ApplyButtonEnabled = false; // reset to disabled mode when changed

						if (stringObj.Name == PORTAL_NAME_OBJ_NAME) { // Portal type name display - "pn" = portal name 
						
							if (Tables.PortalTypeNames.ContainsKey(obj.GetString())) {
								toolStripStatusLabel_additionalInfo.Text =
									string.Format(Properties.Resources.MainAdditionalInfo_PortalType,
										Tables.PortalTypeNames[obj.GetString()]);
							} else {
								toolStripStatusLabel_additionalInfo.Text =
									string.Format(Properties.Resources.MainAdditionalInfo_PortalType, obj.GetString());
							}
						} else {
							textPropBox.AcceptsReturn = true;
						}
					}
				} else if (bIsWzLongProperty || bIsWzIntProperty || bIsWzShortProperty) {
					textPropBox.Visibility = Visibility.Visible;
					textPropBox.AcceptsReturn = false;

					textPropBox.Text = obj.WzValue.ToString();

					textPropBox.ApplyButtonEnabled = false; // reset to disabled mode when changed

					// field limit UI
					if (obj.Name == FIELD_LIMIT_OBJ_NAME && ulong.TryParse(obj.WzValue.ToString(), out var value)) {
						isSelectingWzMapFieldLimit = true;

						// Set visibility
						fieldLimitPanelHost.Visibility = Visibility.Visible;
						fieldLimitPanel1.UpdateFieldLimitCheckboxes(value);
					}
				} else if (bIsWzDoubleProperty || bIsWzFloatProperty) {
					textPropBox.Visibility = Visibility.Visible;
					textPropBox.AcceptsReturn = false;
					textPropBox.ApplyButtonEnabled = false; // reset to disabled mode when changed

					textPropBox.Text = obj.WzValue.ToString();
				} else {
					textPropBox.AcceptsReturn = false;
				}
			} else if (obj is WzVectorProperty property) {
				vectorPanel.Visibility = Visibility.Visible;

				vectorPanel.X = property.X.Value;
				vectorPanel.Y = property.Y.Value;
			}

			// Animation button
			if (AnimationBuilder.IsValidAnimationWzObject(obj)) {
				bAnimateMoreButton = true; // flag

				menuItem_saveImageAnimation.Visibility = Visibility.Visible;
			} else {
				menuItem_saveImageAnimation.Visibility = Visibility.Collapsed;
			}


			// Storyboard hint
			button_MoreOption.Visibility = bAnimateMoreButton ? Visibility.Visible : Visibility.Collapsed;
			if (bAnimateMoreButton) {
				var storyboard_moreAnimation =
					(Storyboard) FindResource(
						"Storyboard_TreeviewItemSelectedAnimation");
				storyboard_moreAnimation.Begin();
			}
		}

		private void UpdateImageView(WzNode node, WzCanvasProperty canvasProp, bool changeImage) {
			if (changeImage) {
				menuItem_changeImage.Visibility = Visibility.Visible;
			}

			menuItem_saveImage.Visibility = Visibility.Visible;
			menuItem_ChangePixelFormat.Visibility = Visibility.Visible;

			Image img = canvasProp.GetLinkedWzCanvasBitmap();
			if (img is Bitmap bmp) {
				canvasPropBox.SetImage(bmp);
			}

			var pngProp = canvasProp.PngProperty;

			var format = pngProp.PixFormat.ToString();
			if (Enum.IsDefined(typeof(WzPngProperty.CanvasPixFormat), pngProp.PixFormat)) {
				format = ((WzPngProperty.CanvasPixFormat) pngProp.PixFormat).ToString();
			}

			toolStripStatusLabel_additionalInfo.Text = string.Format(Properties.Resources.MainAdditionalInfo_PNG,
				format,
				pngProp.MagLevel, IsBadFormat(pngProp), pngProp.ListWzUsed);

			SetImageRenderView(node, canvasProp);
		}

		private string IsBadFormat(WzPngProperty pngProp) {
			if (pngProp.PixFormat != (int) WzPngProperty.CanvasPixFormat.Argb8888) return "Unknown";
			return pngProp.IsArgb4444Compatible().ToString();
		}

		/// <summary>
		///  Sets the ImageRender view on clicked, or via animation tick
		/// </summary>
		/// <param name="canvas"></param>
		/// <param name="animationFrame"></param>
		private void SetImageRenderView(WzNode node, WzCanvasProperty canvas) {
			canvasPropBox.PreLoad();
			// origin
			var delay = canvas[WzCanvasProperty.AnimationDelayPropertyName]?.GetInt();
			var originVector = canvas.GetCanvasOriginPosition();
			var headVector = canvas.GetCanvasHeadPosition();
			var ltVector = canvas.GetCanvasLtPosition();
			var rbVector = canvas.GetCanvasRbPosition();

			// Set XY point to canvas xaml
			canvasPropBox.ParentWzNode = node;
			canvasPropBox.ParentWzCanvasProperty = canvas;
			canvasPropBox.Delay = delay ?? 0;
			canvasPropBox.CanvasVectorOrigin = originVector;
			canvasPropBox.CanvasVectorHead = headVector;
			canvasPropBox.CanvasVectorLt = ltVector;
			canvasPropBox.CanvasVectorRb = rbVector;

			if (canvasPropBox.Visibility != Visibility.Visible) {
				canvasPropBox.Visibility = Visibility.Visible;
			}

			canvasPropBox.PostLoad();
		}

		#endregion

		#region Search

		/// <summary>
		/// On search box fade in completed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Storyboard_Find_FadeIn_Completed(object sender, EventArgs e) {
			findBox.Focus();
		}

		private int searchidx;
		private bool finished;
		private bool listSearchResults;
		private List<string> searchResultsList = new List<string>();
		private bool searchValues = true;
		private WzNode coloredNode;
		private int currentidx;
		private string searchText = "";
		private bool extractImages;

		/// <summary>
		/// Close search box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_closeSearch_Click(object sender, RoutedEventArgs e) {
			var sbb =
				(Storyboard) FindResource("Storyboard_Find_FadeOut");
			sbb.Begin();
		}

		private void button_searchSetting_Click(object sender, RoutedEventArgs e) {
			new SearchOptionsForm().ShowDialog();
		}

		private void SearchWzProperties(IPropertyContainer parent) {
			foreach (var prop in parent.WzProperties) {
				if (0 <= prop.Name.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) ||
				    (searchValues && prop is WzStringProperty && 0 <=
					    ((WzStringProperty) prop).Value.IndexOf(searchText,
						    StringComparison.InvariantCultureIgnoreCase))) {
					if (listSearchResults) {
						searchResultsList.Add(prop.FullPath.Replace(";", @"\"));
					} else if (currentidx == searchidx) {
						if (prop.HRTag == null) {
							((WzNode) prop.ParentImage.HRTag).Reparse();
						}

						var node = (WzNode) prop.HRTag;
						//if (node.Style == null) node.Style = new ElementStyle();
						node.Background = Brushes.Yellow;
						coloredNode = node;
						node.Focus();
						TreeViewHelper.BringIntoView(node);
						node.IsSelected = true;
						finished = true;
						searchidx++;
						return;
					} else {
						currentidx++;
					}
				}

				if (prop is IPropertyContainer && prop.WzProperties.Count != 0) {
					SearchWzProperties((IPropertyContainer) prop);
					if (finished) {
						return;
					}
				}
			}
		}

		private void SearchTV(WzNode node) {
			foreach (WzNode subnode in node.Nodes) {
				if (0 <= subnode.Text.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase)) {
					if (listSearchResults) {
						searchResultsList.Add(subnode.GetFullPath().Replace(";", @"\"));
					} else if (currentidx == searchidx) {
						//if (subnode.Style == null) subnode.Style = new ElementStyle();
						subnode.Background = Brushes.Yellow;
						coloredNode = subnode;
						subnode.Focus();
						TreeViewHelper.BringIntoView(subnode);
						subnode.IsSelected = true;
						finished = true;
						searchidx++;
						return;
					} else {
						currentidx++;
					}
				}

				if (subnode.Tag is WzImage) {
					var img = (WzImage) subnode.Tag;
					if (img.Parsed) {
						SearchWzProperties(img);
					} else if (extractImages) {
						img.ParseImage();
						SearchWzProperties(img);
					}

					if (finished) return;
				} else {
					SearchTV(subnode);
				}
			}
		}

		/// <summary>
		/// Find all
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_allSearch_Click(object sender, RoutedEventArgs e) {
			if (coloredNode != null) {
				coloredNode.Background = Brushes.White;
				coloredNode = null;
			}

			if (findBox.Text == "" || DataTree.Items.Count == 0) {
				return;
			}

			if (DataTree.SelectedNode == null) {
				(DataTree.Items[0] as WzNode).IsSelected = true;
			}

			finished = false;
			listSearchResults = true;
			searchResultsList.Clear();
			//searchResultsBox.Items.Clear();
			searchValues = Program.ConfigurationManager.UserSettings.SearchStringValues;
			currentidx = 0;
			searchText = findBox.Text;
			extractImages = Program.ConfigurationManager.UserSettings.ParseImagesInSearch;
			foreach (WzNode node in DataTree.SelectedNodes) {
				if (node.Tag is WzImageProperty) {
					continue;
				}

				if (node.Tag is IPropertyContainer) {
					SearchWzProperties((IPropertyContainer) node.Tag);
				} else {
					SearchTV(node);
				}
			}

			var form = SearchSelectionForm.Show(searchResultsList);
			form.OnSelectionChanged += Form_OnSelectionChanged;

			findBox.Focus();
		}

		/// <summary>
		/// On search selection from SearchSelectionForm list changed
		/// </summary>
		/// <param name="str"></param>
		private void Form_OnSelectionChanged(string str) {
			var splitPath = str.Split(@"\".ToCharArray());
			WzNode node = null;
			var collection = DataTree.Items;
			for (var i = 0; i < splitPath.Length; i++) {
				node = GetNodeByName(collection, splitPath[i]);
				if (node != null) {
					if (node.Tag is WzImage && !((WzImage) node.Tag).Parsed && i != splitPath.Length - 1) {
						TreeViewMS.ParseOnDataTreeSelectedItem(node, false);
					}

					collection = node.Nodes;
				}
			}

			if (node != null) {
				node.IsSelected = true;
				node.Focus();
				TreeViewHelper.BringIntoView(node);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private WzNode GetNodeByName(ItemCollection collection, string name) {
			foreach (WzNode node in collection) {
				if (node.Text == name) {
					return node;
				}
			}

			return null;
		}

		/// <summary>
		/// Find next
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_nextSearch_Click(object sender, RoutedEventArgs e) {
			if (coloredNode != null) {
				coloredNode.Background = Brushes.White;
				coloredNode = null;
			}

			if (findBox.Text == "" || DataTree.Items.Count == 0) {
				return;
			}

			if (DataTree.SelectedNode == null) {
				(DataTree.Items[0] as WzNode).IsSelected = true;
			}
			finished = false;
			listSearchResults = false;
			searchResultsList.Clear();
			searchValues = Program.ConfigurationManager.UserSettings.SearchStringValues;
			currentidx = 0;
			searchText = findBox.Text;
			extractImages = Program.ConfigurationManager.UserSettings.ParseImagesInSearch;
			foreach (WzNode node in DataTree.SelectedNodes) {
				if (node.Tag is IPropertyContainer) {
					SearchWzProperties((IPropertyContainer) node.Tag);
				} else if (node.Tag is WzImageProperty) {
					continue;
				} else {
					SearchTV(node);
				}

				if (finished) break;
			}

			if (!finished) {
				MessageBox.Show(Properties.Resources.MainTreeEnd);
				searchidx = 0;
				DataTree.SelectedNode.Focus();
				TreeViewHelper.BringIntoView(DataTree.SelectedNode);
			}

			findBox.Focus();
		}

		private void findBox_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				button_nextSearch_Click(null, null);
				e.Handled = true;
			}
		}

		private void findBox_TextChanged(object sender, TextChangedEventArgs e) {
			searchidx = 0;
		}

		#endregion

		private void MenuItem_changePixelFormat_OnClick(object sender, RoutedEventArgs e) {
			if (!(DataTree.SelectedNode.Tag is WzCanvasProperty canvas)) {
				return;
			}

			if (!PixelFormatSelector.Show(canvas.PngProperty.PixFormat, out var pixelFormat)) {
				return;
			}

			if (!canvas.PngProperty.ConvertPixFormat(pixelFormat)) {
				return;
			}

			canvas.ParentImage.Changed = true;
			// Update the preview
			UpdateImageView(canvasPropBox.ParentWzNode, canvasPropBox.ParentWzCanvasProperty, true);
		}

		private void Items_Filter(object sender, FilterEventArgs e) {
			var item = (WzNode) e.Item;
			e.Accepted = item.IsVisible;
		}
	}
}