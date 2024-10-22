/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HaRepacker.Converters;
using HaRepacker.GUI;
using HaRepacker.GUI.Input;
using HaRepacker.GUI.Panels;
using HaRepacker.Helpers;
using HaRepacker.Properties;
using MapleLib.Helpers;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.Forms.MessageBox;

namespace HaRepacker {
	public class ContextMenuManager {
		private MainPanel parentPanel;
		private TreeViewMS _treeViewMs;

		private MenuItem SaveFile;
		private MenuItem Remove;
		private MenuItem Unload;
		private MenuItem Reload;
		private MenuItem CollapseAllChildNode;

		private MenuItem ExpandAllChildNode;
		//private MenuItem SortAllChildViewNode, SortAllChildViewNode2;
		//private MenuItem SortPropertiesByName;

		private MenuItem AddPropsSubMenu;
		private MenuItem AddDirsSubMenu;

		private MenuItem AddEtcMenu;

		//private MenuItem AddSortMenu;
		//private MenuItem AddSortMenu_WithoutPropSort;
		private MenuItem AddImage;
		private MenuItem AddDirectory;
		private MenuItem AddByteFloat;
		private MenuItem AddCanvas;
		private MenuItem AddLong;
		private MenuItem AddInt;
		private MenuItem AddConvex;
		private MenuItem AddDouble;
		private MenuItem AddNull;
		private MenuItem AddSound;
		private MenuItem AddString;
		private MenuItem AddSub;
		private MenuItem AddUshort;
		private MenuItem AddUOL;
		private MenuItem AddVector;
		private MenuItem Rename;
		private MenuItem FixLink;
		private MenuItem FixPixFormat;
		private MenuItem CheckListWzEntries;

		public ContextMenuManager(TreeViewMS treeView) {
			_treeViewMs = treeView;
			parentPanel = _treeViewMs.MainPanel;
			CreateItems();
		}

		private void CreateItems() {
			SaveFile = new MenuItem() {Header = "Save", Icon = new Image {
				Source = Resources.disk.ToWpfBitmap()
			}};
			SaveFile.Click += delegate {
				foreach (var node in GetNodes()) {
					new SaveForm(parentPanel, node).ShowDialog();
				}
			};
			Rename = new MenuItem() {
				Header = "Rename", Icon = new Image {
					Source = Resources.rename.ToWpfBitmap()
				}
			};
			Rename.Click += delegate { parentPanel.PromptRenameWzTreeNode(GetNodes()[0]); };
			Remove = new MenuItem() {Header = "Remove", Icon = new Image {
				Source = Resources.delete.ToWpfBitmap()
			}};
			Remove.Click += delegate { parentPanel.PromptRemoveSelectedTreeNodes(); };

			Unload = new MenuItem() {Header = "Unload", Icon = new Image {
				Source = Resources.delete.ToWpfBitmap()
			}};
			Unload.Click += delegate {
				if (!Warning.Warn(Resources.MainUnloadFile)) {
					return;
				}

				foreach (var node in GetNodes()) {
					parentPanel.MainForm.UnloadNode(node);
				}
			};
			Reload = new MenuItem() {
				Header = "Reload", Icon = new Image {
					Source = Resources.arrow_refresh.ToWpfBitmap()
				}
			};
			Reload.Click += delegate {
				if (!Warning.Warn("Are you sure you want to reload this file?")) {
					return;
				}

				foreach (var node in GetNodes()) // selected nodes
				{
					parentPanel.MainForm.ReloadWzFile(node.Tag as WzFile);
				}
			};
			CollapseAllChildNode = new MenuItem() {Header = "Collapse All", Icon = new Image {
				Source = Resources.collapse.ToWpfBitmap()
			}};
			CollapseAllChildNode.Click += delegate {
				foreach (var node in GetNodes()) {
					node.CollapseAll();
				}
			};
			ExpandAllChildNode = new MenuItem() {
				Header = "Expand all", Icon = new Image {
					Source = Resources.expand.ToWpfBitmap()
				}
			};
			ExpandAllChildNode.Click += delegate {
				foreach (var node in GetNodes()) {
					node.ExpandSubtree();
				}
			};
			// This only sorts the view, does not affect the actual order of the 
			// wz properties
			/*SortAllChildViewNode = new MenuItem() {
				Header = "Sort child nodes view"
			};
			SortAllChildViewNode.Click += delegate(object sender, RoutedEventArgs e) {
				foreach (var node in GetNodes(sender)) parentPanel.MainForm.SortNodesRecursively(node);
			};
			SortAllChildViewNode2 = new MenuItem() {Header = "Sort child nodes view"};
			SortAllChildViewNode2.Click += delegate(object sender, RoutedEventArgs e) {
				foreach (var node in GetNodes(sender)) parentPanel.MainForm.SortNodesRecursively(node);
			};
			SortPropertiesByName = new MenuItem() {Header = "Sort properties by name"};
			SortPropertiesByName.Click += delegate(object sender, RoutedEventArgs e) {
				foreach (var node in GetNodes(sender)) parentPanel.MainForm.SortNodeProperties(node);
			};*/

			AddImage = new MenuItem() {Header = "Image"};
			AddImage.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				string name;
				if (NameInputBox.Show("Add Image", 0, out name)) {
					nodes[0].AddObject(new WzImage(name) {Changed = true}, parentPanel.UndoRedoMan);
				}
			};
			AddDirectory = new MenuItem() {Header = "Directory"};
			AddDirectory.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzDirectoryToSelectedNode(nodes[0]);
			};
			AddByteFloat = new MenuItem() {Header = "Float"};
			AddByteFloat.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzByteFloatToSelectedNode(nodes[0]);
			};
			AddCanvas = new MenuItem() {Header = "Canvas"};
			AddCanvas.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzCanvasToSelectedNode(nodes[0]);
			};
			AddLong = new MenuItem() {Header = "Long"};
			AddLong.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzLongToSelectedNode(nodes[0]);
			};
			AddInt = new MenuItem() {Header = "Int"};
			AddInt.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzCompressedIntToSelectedNode(nodes[0]);
			};
			AddConvex = new MenuItem() {Header = "Convex"};
			AddConvex.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzConvexPropertyToSelectedNode(nodes[0]);
			};
			AddDouble = new MenuItem() {Header = "Double"};
			AddDouble.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzDoublePropertyToSelectedNode(nodes[0]);
			};
			AddNull = new MenuItem() {Header = "Null"};
			AddNull.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzNullPropertyToSelectedNode(nodes[0]);
			};
			AddSound = new MenuItem() {Header = "Sound"};
			AddSound.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzSoundPropertyToSelectedNode(nodes[0]);
			};
			AddString = new MenuItem() {Header = "String"};
			AddString.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzStringPropertyToSelectedIndex(nodes[0]);
			};
			AddSub = new MenuItem() {Header = "Sub"};
			AddSub.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzSubPropertyToSelectedIndex(nodes[0]);
			};
			AddUshort = new MenuItem() {Header = "Short"};
			AddUshort.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzUnsignedShortPropertyToSelectedIndex(nodes[0]);
			};
			AddUOL = new MenuItem() {Header = "UOL"};
			AddUOL.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzUOLPropertyToSelectedIndex(nodes[0]);
			};
			AddVector = new MenuItem() {Header = "Vector"};
			AddVector.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes();
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzVectorPropertyToSelectedIndex(nodes[0]);
			};
			FixLink = new MenuItem() {Header = "Fix linked image for old MapleStory ver."};
			FixLink.Click += delegate { FixLinkForOldMS_Click(GetNodes()); };
			FixPixFormat = new MenuItem() {Header = "Fix wrong pixel formats"};
			FixPixFormat.Click += delegate { FixAllIncorrectPixelFormats_Click(GetNodes()); };
			CheckListWzEntries = new MenuItem() {Header = "Check List.wz entries"};
			CheckListWzEntries.Click += delegate { CheckListWzEntries_Click(GetNodes()); };
			AddDirsSubMenu = new MenuItem() {
				Header = "Add"
			};
			AddDirsSubMenu.Items.Add(AddDirectory);
			AddDirsSubMenu.Items.Add(AddImage);
			AddPropsSubMenu = new MenuItem() {Header = "Add"};
			AddPropsSubMenu.Items.Add(AddCanvas);
			AddPropsSubMenu.Items.Add(AddConvex);
			AddPropsSubMenu.Items.Add(AddDouble);
			AddPropsSubMenu.Items.Add(AddByteFloat);
			AddPropsSubMenu.Items.Add(AddLong);
			AddPropsSubMenu.Items.Add(AddInt);
			AddPropsSubMenu.Items.Add(AddNull);
			AddPropsSubMenu.Items.Add(AddUshort);
			AddPropsSubMenu.Items.Add(AddSound);
			AddPropsSubMenu.Items.Add(AddString);
			AddPropsSubMenu.Items.Add(AddSub);
			AddPropsSubMenu.Items.Add(AddUOL);
			AddPropsSubMenu.Items.Add(AddVector);
			AddEtcMenu = new MenuItem() {
				Header = "Etc"
			};
			AddEtcMenu.Items.Add(FixLink);
			AddEtcMenu.Items.Add(FixPixFormat);
			AddEtcMenu.Items.Add(CheckListWzEntries);
			var RetrieveSize = new MenuItem() {Header = "Get Size"};
			RetrieveSize.Click += delegate {
				var nodes = GetNodes();
				long totalSize = 0;
				var nodesChecked = 0;
				foreach (var node in nodes) {
					if (node.Tag is WzImage img) {
						totalSize += img.BlockSize;
						nodesChecked++;
					}else if (node.Tag is WzDirectory dir) {
						totalSize += dir.BlockSize;
						nodesChecked++;
					} else if (node.Tag is WzFile file) {
						nodesChecked++;
						foreach (var i in file.WzDirectory.WzImages) {
							totalSize += i.BlockSize;
						}

						foreach (var i in file.WzDirectory.WzDirectories) {
							totalSize += i.BlockSize;
						}
					}
				}

				MessageBox.Show($"Counted {StringUtils.ToFileSize(totalSize, FileSizeUnits.MB)} MB from {nodesChecked} nodes");
			};
			AddEtcMenu.Items.Add(RetrieveSize);

			//AddSortMenu = new MenuItem(){ Header = "Sort", Icon = Resources.sort, SortAllChildViewNode, SortPropertiesByName);

			//AddSortMenu_WithoutPropSort = new MenuItem(){ Header = "Sort", Icon = Resources.sort, SortAllChildViewNode2);
		}

		private ContextMenu _contextMenu;

		/// <summary>
		/// Toolstrip menu when right clicking on nodes
		/// </summary>
		/// <param name="node"></param>
		/// <param name="Tag"></param>
		/// <returns></returns>
		public ContextMenu CreateMenu() {
			var menu = new ContextMenu();

			var nodes = GetNodes();
			if (nodes.Length == 0) {
				return menu;
			}

			var Tag = nodes[0].Tag as WzObject;
			var node = nodes[0];

			if (Tag is WzImage || Tag is IPropertyContainer) {
				menu.Items.Add(AddPropsSubMenu);
				menu.Items.Add(Rename);
				// export, import
				if (node.Parent != null) {
					menu.Items.Add(Remove);
				}
			} else if (Tag is WzImageProperty) {
				menu.Items.Add(Rename);
				menu.Items.Add(Remove);
			} else if (Tag is WzDirectory) {
				menu.Items.Add(AddDirsSubMenu);
				menu.Items.Add(Rename);
				menu.Items.Add(Remove);
			} else if (Tag is WzFile) {
				menu.Items.Add(AddDirsSubMenu);
				menu.Items.Add(Rename);
				menu.Items.Add(SaveFile);
				menu.Items.Add(Unload);
				menu.Items.Add(Reload);
			}

			if (Tag is WzImage && node.Parent == null) {
				menu.Items.Add(Unload);
			}

			menu.Items.Add(ExpandAllChildNode);
			menu.Items.Add(CollapseAllChildNode);
			menu.Items.Add(AddEtcMenu);

			var BatchMenu = new MenuItem() {
				Header = "Batch"
			};

			var BatchDelete = new MenuItem() {
				Header = "Delete", Icon = new Image {
					Source = Resources.delete.ToWpfBitmap()
				}
			};
			BatchDelete.Click += delegate {
				if (!NameInputBox.Show(Resources.ContextMenu_BatchDelete_Title, 0, out var name)) {
					return;
				}

				var inputNodes = GetNodes();

				var nodesModified = RecursiveHelper.CheckAllNodes(Array.ConvertAll(inputNodes, item => (WzObject) item.Tag), obj => !obj.Name.Equals(name), obj => {
					if (!obj.Name.Equals(name)) {
						return false;
					}

					if (RemoveNode(inputNodes, obj)) {
						return true;
					}

					// Nodes will not be loaded in all cases, so if it's not loaded yet
					// just delete the wz object and continue
					if (obj is WzImageProperty property) {
						var parent = property.ParentImage;
						if (parent != null) {
							parent.Changed = true;
						}
					}

					obj.Remove();
					return true;
				});
				MessageBox.Show(string.Format(Resources.ContextMenu_BatchDelete_Deleted, nodesModified));
			};

			BatchMenu.Items.Add(BatchDelete);

			menu.Items.Add(BatchMenu);

			/*if (Tag.GetType() == typeof(WzSubProperty)) {
				menu.Items.Add(AddSortMenu);
			} else {
				menu.Items.Add(AddSortMenu_WithoutPropSort);
			}*/
			return menu;
		}

		private WzNode[] GetNodes() {
			return _treeViewMs.SelectedNodes.ToArray();
		}

		private static void FixAllIncorrectPixelFormats_Click(WzNode[] selectedNodes) {
			var nodes = RecursiveHelper.CheckAllNodes(Array.ConvertAll(selectedNodes, item => (WzObject) item.Tag), _ => true, obj => {
				if (obj is not WzCanvasProperty canvas) {
					return false;
				}

				var pngProp = canvas.PngProperty;
				if (pngProp.PixFormat != (int) WzPngProperty.CanvasPixFormat.Argb8888 || !pngProp.IsArgb4444Compatible()) {
					return false;
				}

				pngProp.ConvertPixFormat((int) WzPngProperty.CanvasPixFormat.Argb4444);
				canvas.ParentImage.Changed = true;
				return true;
			});
			MessageBox.Show($"Done.\r\nFixed {nodes} images");
		}

		private static void CheckListWzEntries_Click(WzNode[] selectedNodes) {
			foreach (var node in selectedNodes) {
				if (node.Tag is WzImage or WzDirectory or WzFile) {
					continue;
				}

				MessageBox.Show("Please select a WzFile, WzDirectory or WzImage node to check for List.wz entries");
				return;
			}

			var nodes = RecursiveHelper.CheckAllNodes(Array.ConvertAll(selectedNodes, item => (WzObject) item.Tag), obj => obj is WzDirectory or WzFile, obj => {
				if (obj is not WzImage img) {
					return false;
				}

				ListWzContainerImpl.MarkListWzProperty(img);
				return true;
			});
			MessageBox.Show($"Done.\r\nChecked {nodes} properties");
		}

		/// <summary>
		/// Fix the '_inlink' and '_outlink' image property for compatibility to old MapleStory ver.
		/// </summary>
		private static void FixLinkForOldMS_Click(WzNode[] selectedNodes) {
			var inputNodes = selectedNodes;

			var nodesModified = RecursiveHelper.CheckAllNodes(Array.ConvertAll(selectedNodes, item => (WzObject) item.Tag), _ => true, obj => {
				if (obj.Name.Equals("_hash")) {
					if (!RemoveNode(inputNodes, obj)) {
						obj.Remove();
					}

					return true;
				}

				if (obj is not WzCanvasProperty selectedWzCanvas) {
					return false;
				}

				var linkedTarget = selectedWzCanvas.GetLinkedWzImageProperty();
				// Check if the linked wz prop is changed
				// if it is, that means inlink/outlink exists & the resulting link
				// exists and is found.
				if (selectedWzCanvas == linkedTarget) {
					return false;
				}

				var inlink = selectedWzCanvas[WzCanvasProperty.InlinkPropertyName];
				if (inlink != null) { // if its an inlink property, remove that before updating base image.
					if (!RemoveNode(inputNodes, inlink)) {
						inlink.Remove();
					}
				}

				var outlink = selectedWzCanvas[WzCanvasProperty.OutlinkPropertyName];
				if (outlink != null) { // if its an outlink property, remove that before updating base image.
					if (!RemoveNode(inputNodes, outlink)) {
						outlink.Remove();
					}
				}

				selectedWzCanvas.PngProperty.SetImage(linkedTarget.GetBitmap());

				selectedWzCanvas.ParentImage.Changed = true;
				return true;
			});
			MessageBox.Show($"Done.\r\nUpdated {nodesModified} nodes");
		}

		private static bool RemoveNode(WzNode[] inputNodes, WzObject obj) {
			foreach (var node in inputNodes) {
				var child = node.GetChildNode(obj);
				if (child == null) {
					continue;
				}

				child.DeleteWzNode();
				return true;
			}

			return false;
		}
	}
}