/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Windows;
using System.Windows.Controls;
using HaRepacker.GUI;
using HaRepacker.GUI.Input;
using HaRepacker.GUI.Panels;
using HaRepacker.Properties;
using MapleLib.WzLib;
using ContextMenu = System.Windows.Controls.ContextMenu;

namespace HaRepacker {
	public class ContextMenuManager {
		public MainPanel parentPanel;

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

		private WzNode previousNode;

		public ContextMenuManager(MainPanel haRepackerMainPanel) {
			parentPanel = haRepackerMainPanel;
			CreateItems();
		}

		private void CreateItems() {
			SaveFile = new MenuItem() {Header = "Save", Icon = Resources.disk};
			SaveFile.Click += delegate(object sender, RoutedEventArgs e) {
				foreach (var node in GetNodes(sender)) {
					new SaveForm(parentPanel, node).ShowDialog();
				}
			};
			Rename = new MenuItem() {
				Header = "Rename", Icon = Resources.rename
			};
			Rename.Click += delegate(object sender, RoutedEventArgs e) { parentPanel.PromptRenameWzTreeNode(GetNodes(sender)[0]); };
			Remove = new MenuItem() {Header = "Remove", Icon = Resources.delete};
			Remove.Click += delegate { parentPanel.PromptRemoveSelectedTreeNodes(); };

			Unload = new MenuItem() {Header = "Unload", Icon = Resources.delete};
			Unload.Click += delegate(object sender, RoutedEventArgs e) {
				if (!Warning.Warn(Resources.MainUnloadFile)) {
					return;
				}

				var nodesSelected = GetNodes(sender);
				foreach (var node in nodesSelected) {
					parentPanel.MainForm.UnloadNode(node);
				}
			};
			Reload = new MenuItem() {
				Header = "Reload", Icon = Resources.arrow_refresh
			};
			Reload.Click += delegate(object sender, RoutedEventArgs e) {
				if (!Warning.Warn("Are you sure you want to reload this file?")) {
					return;
				}

				var nodesSelected = GetNodes(sender);
				foreach (var node in nodesSelected) // selected nodes
				{
					parentPanel.MainForm.ReloadWzFile(node.Tag as WzFile);
				}
			};
			CollapseAllChildNode = new MenuItem() {Header = "Collapse All", Icon = Resources.collapse};
			CollapseAllChildNode.Click += delegate(object sender, RoutedEventArgs e) {
				foreach (var node in GetNodes(sender)) {
					node.CollapseAll();
				}
			};
			ExpandAllChildNode = new MenuItem() {
				Header = "Expand all", Icon = Resources.expand
			};
			ExpandAllChildNode.Click += delegate(object sender, RoutedEventArgs e) {
				foreach (var node in GetNodes(sender)) {
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
				var nodes = GetNodes(sender);
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
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzDirectoryToSelectedNode(nodes[0]);
			};
			AddByteFloat = new MenuItem() {Header = "Float"};
			AddByteFloat.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzByteFloatToSelectedNode(nodes[0]);
			};
			AddCanvas = new MenuItem() {Header = "Canvas"};
			AddCanvas.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzCanvasToSelectedNode(nodes[0]);
			};
			AddLong = new MenuItem() {Header = "Long"};
			AddLong.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzLongToSelectedNode(nodes[0]);
			};
			AddInt = new MenuItem() {Header = "Int"};
			AddInt.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzCompressedIntToSelectedNode(nodes[0]);
			};
			AddConvex = new MenuItem() {Header = "Convex"};
			AddConvex.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzConvexPropertyToSelectedNode(nodes[0]);
			};
			AddDouble = new MenuItem() {Header = "Double"};
			AddDouble.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzDoublePropertyToSelectedNode(nodes[0]);
			};
			AddNull = new MenuItem() {Header = "Null"};
			AddNull.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzNullPropertyToSelectedNode(nodes[0]);
			};
			AddSound = new MenuItem() {Header = "Sound"};
			AddSound.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzSoundPropertyToSelectedNode(nodes[0]);
			};
			AddString = new MenuItem() {Header = "String"};
			AddString.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzStringPropertyToSelectedIndex(nodes[0]);
			};
			AddSub = new MenuItem() {Header = "Sub"};
			AddSub.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzSubPropertyToSelectedIndex(nodes[0]);
			};
			AddUshort = new MenuItem() {Header = "Short"};
			AddUshort.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzUnsignedShortPropertyToSelectedIndex(nodes[0]);
			};
			AddUOL = new MenuItem() {Header = "UOL"};
			AddUOL.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzUOLPropertyToSelectedIndex(nodes[0]);
			};
			AddVector = new MenuItem() {Header = "Vector"};
			AddVector.Click += delegate(object sender, RoutedEventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				parentPanel.AddWzVectorPropertyToSelectedIndex(nodes[0]);
			};
			FixLink = new MenuItem() {Header = "Fix linked image for old MapleStory ver."};
			FixLink.Click += delegate { parentPanel.FixLinkForOldMS_Click(); };
			FixPixFormat = new MenuItem() {Header = "Fix wrong pixel formats"};
			FixPixFormat.Click += delegate { parentPanel.FixAllIncorrectPixelFormats(); };
			CheckListWzEntries = new MenuItem() {Header = "Check List.wz entries"};
			CheckListWzEntries.Click += delegate { parentPanel.CheckListWzEntries(); };
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
		public ContextMenu CreateMenu(WzNode node, WzObject Tag) {
			previousNode?.ContextMenu?.Items.Clear();

			var menu = new ContextMenu();

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

			/*if (Tag.GetType() == typeof(WzSubProperty)) {
				menu.Items.Add(AddSortMenu);
			} else {
				menu.Items.Add(AddSortMenu_WithoutPropSort);
			}*/
			previousNode = node;
			return menu;
		}

		private WzNode[] GetNodes(object sender) {
			return new[] {previousNode};
		}
	}
}