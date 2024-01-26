/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using HaRepacker.GUI;
using HaRepacker.GUI.Input;
using HaRepacker.GUI.Panels;
using HaRepacker.Properties;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace HaRepacker {
	public class ContextMenuManager {
		private MainPanel parentPanel;

		public ContextMenuStrip WzFileMenu;
		public ContextMenuStrip WzDirectoryMenu;
		public ContextMenuStrip PropertyContainerMenu;
		public ContextMenuStrip SubPropertyMenu;
		public ContextMenuStrip PropertyMenu;

		private ToolStripMenuItem SaveFile;
		private ToolStripMenuItem Remove;
		private ToolStripMenuItem Unload;
		private ToolStripMenuItem Reload;
		private ToolStripMenuItem CollapseAllChildNode;
		private ToolStripMenuItem ExpandAllChildNode;
		private ToolStripMenuItem SortAllChildViewNode, SortAllChildViewNode2;
		private ToolStripMenuItem SortPropertiesByName;

		private ToolStripMenuItem AddPropsSubMenu;
		private ToolStripMenuItem AddDirsSubMenu;
		private ToolStripMenuItem AddEtcMenu;
		private ToolStripMenuItem AddSortMenu;
		private ToolStripMenuItem AddSortMenu_WithoutPropSort;
		private ToolStripMenuItem AddImage;
		private ToolStripMenuItem AddDirectory;
		private ToolStripMenuItem AddByteFloat;
		private ToolStripMenuItem AddCanvas;
		private ToolStripMenuItem AddLong;
		private ToolStripMenuItem AddInt;
		private ToolStripMenuItem AddConvex;
		private ToolStripMenuItem AddDouble;
		private ToolStripMenuItem AddNull;
		private ToolStripMenuItem AddSound;
		private ToolStripMenuItem AddString;
		private ToolStripMenuItem AddSub;
		private ToolStripMenuItem AddUshort;
		private ToolStripMenuItem AddUOL;
		private ToolStripMenuItem AddVector;
		private ToolStripMenuItem Rename;
		private ToolStripMenuItem FixLink;
		private ToolStripMenuItem FixPixFormat;


		public ContextMenuManager(MainPanel haRepackerMainPanel, UndoRedoManager undoMan) {
			parentPanel = haRepackerMainPanel;

			SaveFile = new ToolStripMenuItem("Save", Resources.disk, delegate(object sender, EventArgs e) {
				foreach (var node in GetNodes(sender)) new SaveForm(parentPanel, node).ShowDialog();
			});
			Rename = new ToolStripMenuItem("Rename", Resources.rename, delegate {
				var currentNode = currNode;

				haRepackerMainPanel.PromptRenameWzTreeNode(currentNode);
			});
			Remove = new ToolStripMenuItem("Remove", Resources.delete, delegate { haRepackerMainPanel.PromptRemoveSelectedTreeNodes(); });

			Unload = new ToolStripMenuItem("Unload", Resources.delete, delegate(object sender, EventArgs e) {
				if (!Warning.Warn("Are you sure you want to unload this file?")) {
					return;
				}

				var nodesSelected = GetNodes(sender);
				foreach (var node in nodesSelected) parentPanel.MainForm.UnloadWzFile(node.Tag as WzFile);
			});
			Reload = new ToolStripMenuItem("Reload", Resources.arrow_refresh, delegate(object sender, EventArgs e) {
				if (!Warning.Warn("Are you sure you want to reload this file?")) {
					return;
				}

				var nodesSelected = GetNodes(sender);
				foreach (var node in nodesSelected) // selected nodes
					parentPanel.MainForm.ReloadWzFile(node.Tag as WzFile);
			});
			CollapseAllChildNode = new ToolStripMenuItem("Collapse All", Resources.collapse,
				delegate(object sender, EventArgs e) {
					foreach (var node in GetNodes(sender)) node.Collapse();
				});
			ExpandAllChildNode = new ToolStripMenuItem("Expand all", Resources.expand, delegate(object sender, EventArgs e) {
				foreach (var node in GetNodes(sender)) node.ExpandAll();
			});
			// This only sorts the view, does not affect the actual order of the 
			// wz properties
			SortAllChildViewNode = new ToolStripMenuItem("Sort child nodes view", null,
				delegate(object sender, EventArgs e) {
					foreach (var node in GetNodes(sender)) parentPanel.MainForm.SortNodesRecursively(node, true);
				});
			SortAllChildViewNode2 = new ToolStripMenuItem("Sort child nodes view", null,
				delegate(object sender, EventArgs e) {
					foreach (var node in GetNodes(sender)) parentPanel.MainForm.SortNodesRecursively(node, true);
				});
			SortPropertiesByName = new ToolStripMenuItem("Sort properties by name", null, delegate(object sender, EventArgs e) {
				foreach (var node in GetNodes(sender)) parentPanel.MainForm.SortNodeProperties(node);
			});

			AddImage = new ToolStripMenuItem("Image", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				string name;
				if (NameInputBox.Show("Add Image", 0, out name)) {
					nodes[0].AddObject(new WzImage(name) {Changed = true}, undoMan);
				}
			});
			AddDirectory = new ToolStripMenuItem("Directory", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzDirectoryToSelectedNode(nodes[0]);
			});
			AddByteFloat = new ToolStripMenuItem("Float", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzByteFloatToSelectedNode(nodes[0]);
			});
			AddCanvas = new ToolStripMenuItem("Canvas", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzCanvasToSelectedNode(nodes[0]);
			});
			AddLong = new ToolStripMenuItem("Long", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzLongToSelectedNode(nodes[0]);
			});
			AddInt = new ToolStripMenuItem("Int", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzCompressedIntToSelectedNode(nodes[0]);
			});
			AddConvex = new ToolStripMenuItem("Convex", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzConvexPropertyToSelectedNode(nodes[0]);
			});
			AddDouble = new ToolStripMenuItem("Double", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzDoublePropertyToSelectedNode(nodes[0]);
			});
			AddNull = new ToolStripMenuItem("Null", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzNullPropertyToSelectedNode(nodes[0]);
			});
			AddSound = new ToolStripMenuItem("Sound", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzSoundPropertyToSelectedNode(nodes[0]);
			});
			AddString = new ToolStripMenuItem("String", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzStringPropertyToSelectedIndex(nodes[0]);
			});
			AddSub = new ToolStripMenuItem("Sub", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzSubPropertyToSelectedIndex(nodes[0]);
			});
			AddUshort = new ToolStripMenuItem("Short", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzUnsignedShortPropertyToSelectedIndex(nodes[0]);
			});
			AddUOL = new ToolStripMenuItem("UOL", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzUOLPropertyToSelectedIndex(nodes[0]);
			});
			AddVector = new ToolStripMenuItem("Vector", null, delegate(object sender, EventArgs e) {
				var nodes = GetNodes(sender);
				if (nodes.Length != 1) {
					MessageBox.Show("Please select only ONE node");
					return;
				}

				haRepackerMainPanel.AddWzVectorPropertyToSelectedIndex(nodes[0]);
			});

			FixLink = new ToolStripMenuItem("Fix linked image for old MapleStory ver.", null, delegate { haRepackerMainPanel.FixLinkForOldMS_Click(); });

			FixPixFormat = new ToolStripMenuItem("Fix wrong pixel formats", null, delegate { haRepackerMainPanel.FixAllIncorrectPixelFormats(); });

			AddDirsSubMenu = new ToolStripMenuItem("Add", Resources.add,
				AddDirectory, AddImage);
			AddPropsSubMenu = new ToolStripMenuItem("Add", Resources.add,
				AddCanvas, AddConvex, AddDouble, AddByteFloat, AddLong, AddInt, AddNull, AddUshort, AddSound, AddString, AddSub, AddUOL, AddVector);

			AddEtcMenu = new ToolStripMenuItem("Etc", Resources.add,
				FixLink, FixPixFormat);

			AddSortMenu = new ToolStripMenuItem("Sort", Resources.sort, SortAllChildViewNode, SortPropertiesByName);

			AddSortMenu_WithoutPropSort = new ToolStripMenuItem("Sort", Resources.sort, SortAllChildViewNode2);
		}

		/// <summary>
		/// Toolstrip menu when right clicking on nodes
		/// </summary>
		/// <param name="node"></param>
		/// <param name="Tag"></param>
		/// <returns></returns>
		public ContextMenuStrip CreateMenu(WzNode node, WzObject Tag) {
			var currentDataTreeSelectedCount = parentPanel.DataTree.SelectedNodes.Count;

			var toolStripmenuItems = new List<ToolStripItem>();

			var menu = new ContextMenuStrip();
			if (Tag is WzImage || Tag is IPropertyContainer) {
				toolStripmenuItems.Add(AddPropsSubMenu);
				toolStripmenuItems.Add(Rename);
				// export, import
				toolStripmenuItems.Add(Remove);
			} else if (Tag is WzImageProperty) {
				toolStripmenuItems.Add(Rename);
				toolStripmenuItems.Add(Remove);
			} else if (Tag is WzDirectory) {
				toolStripmenuItems.Add(AddDirsSubMenu);
				toolStripmenuItems.Add(Rename);
				toolStripmenuItems.Add(Remove);
			} else if (Tag is WzFile) {
				toolStripmenuItems.Add(AddDirsSubMenu);
				toolStripmenuItems.Add(Rename);
				toolStripmenuItems.Add(SaveFile);
				toolStripmenuItems.Add(Unload);
				toolStripmenuItems.Add(Reload);
			}

			toolStripmenuItems.Add(ExpandAllChildNode);
			toolStripmenuItems.Add(CollapseAllChildNode);
			toolStripmenuItems.Add(AddEtcMenu);

			if (Tag.GetType() == typeof(WzSubProperty)) {
				toolStripmenuItems.Add(AddSortMenu);
			} else {
				toolStripmenuItems.Add(AddSortMenu_WithoutPropSort);
			}

			// Add
			foreach (var toolStripItem in toolStripmenuItems) menu.Items.Add(toolStripItem);

			currNode = node;
			return menu;
		}

		private WzNode currNode;

		private WzNode[] GetNodes(object sender) {
			return new[] {currNode};
		}
	}
}