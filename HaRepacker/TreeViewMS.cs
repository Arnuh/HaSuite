/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HaRepacker.GUI;
using HaRepacker.GUI.Input;
using HaRepacker.GUI.Panels;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TreeView = System.Windows.Controls.TreeView;

namespace HaRepacker {
	/// <summary>
	/// Summary description for TreeViewMS.
	/// </summary>
	public class TreeViewMS : TreeView {
		protected List<WzNode> m_coll;

		protected WzNode m_lastNode, m_firstNode;
		private MainPanel _mainPanel;

		public MainPanel MainPanel {
			get => _mainPanel;
			set => _mainPanel = value;
		}

		public bool Focused => IsFocused;

		private static readonly PropertyInfo IsSelectionChangeActiveProperty = typeof(TreeView).GetProperty("IsSelectionChangeActive", BindingFlags.NonPublic | BindingFlags.Instance);

		public static readonly SortDescription Sort = new SortDescription("Header", ListSortDirection.Ascending);

		// Legacy handling
		public WzNode SelectedNode => SelectedItem as WzNode;

		public List<WzNode> SelectedNodes => m_coll;
		
		public TreeViewMS() {
			m_coll = new List<WzNode>();
			DragEnter += Tree_DragEnter;
			Drop += Tree_DragDrop;
			MouseDoubleClick += DataTree_DoubleClick;
			SelectedItemChanged += DataTree_AfterSelect;
			KeyDown += Tree_KeyDown;
			TextInput += Tree_TextInput;

			PreviewMouseRightButtonDown += (sender, e) => {
				ContextMenu = new ContextMenuManager(this).CreateMenu();
				e.Handled = true;
			};
            
			if (IsSelectionChangeActiveProperty == null) {
				return;
			}
			SelectedItemChanged += (sender, e) => {
				if (SelectedItem is not WzNode node) {
					return;
				}

				// allow multiple selection
				// when control key is pressed
				if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
					m_firstNode = node;
					// suppress selection change notification
					// select all selected items
					// then restore selection change notifications
					var isSelectionChangeActive = IsSelectionChangeActiveProperty.GetValue(this, null);

					IsSelectionChangeActiveProperty.SetValue(this, true, null);
					m_coll.ForEach(item => item.IsSelected = true);

					IsSelectionChangeActiveProperty.SetValue(this, isSelectionChangeActive, null);
				} else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) {
					if (m_firstNode == null) {
						return;
					}

					m_lastNode = node;
					var isSelectionChangeActive = IsSelectionChangeActiveProperty.GetValue(this, null);

					IsSelectionChangeActiveProperty.SetValue(this, true, null);
					// select all nodes between m_firstNode and m_lastNode
					var firstParent = m_firstNode.Parent as ItemsControl ?? this;
					var lastParent = m_lastNode.Parent as ItemsControl ?? this;
					if (firstParent == lastParent) {
						SelectBetween(firstParent, m_firstNode, m_lastNode);
					} else {
						// eh, idk atm.
					}

					m_coll.ForEach(item => item.IsSelected = true);

					IsSelectionChangeActiveProperty.SetValue(this, isSelectionChangeActive, null);

					m_firstNode = node;
					m_lastNode = null;
					return;
				} else {
					m_firstNode = node;
					// deselect all selected items except the current one
					m_coll.ForEach(item => item.IsSelected = item == node);
					m_coll.Clear();
				}

				m_lastNode = null;

				if (!m_coll.Contains(node)) {
					m_coll.Add(node);
				} else {
					// deselect if already selected
					node.IsSelected = false;
					m_coll.Remove(node);
				}
			};
		}

		private void SelectBetween(ItemsControl firstParent, WzNode firstNode, WzNode lastNode) {
			var start = firstParent.Items.IndexOf(firstNode);
			var end = firstParent.Items.IndexOf(lastNode);
			if (start > end) {
				(start, end) = (end, start);
			}

			for (var i = start; i <= end; i++) {
				var add = firstParent.Items[i] as WzNode;
				if (m_coll.Contains(add)) {
					continue;
				}

				m_coll.Add(add);
			}
		}

		private void Tree_DragEnter(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				e.Effects = DragDropEffects.Move; // Allow the file to be copied
			}
		}

		private void Tree_DragDrop(object sender, DragEventArgs e) {
			if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop)) {
				var files = (string[]) e.Data.GetData(DataFormats.FileDrop);

				MainPanel.MainForm.OpenFileInternal(files);
			}
		}

		private void DataTree_DoubleClick(object sender, EventArgs e) {
			if (SelectedNode != null && SelectedNode.Tag is WzImage &&
			    SelectedNode.Nodes.Count == 0) {
				ParseOnDataTreeSelectedItem(SelectedNode);
			}
		}

		private void DataTree_AfterSelect(object sender, RoutedPropertyChangedEventArgs<object> e) {
			if (!IsSearching) {
				SearchTerm = "";
			}
			var node = SelectedNode;
			if (node == null) {
				return;
			}

			MainPanel.ShowObjectValue(node, (WzObject) node.Tag);
			MainPanel.selectionLabel.Text = string.Format(Properties.Resources.SelectionType, node.GetTypeName());
		}

		/// <summary>
		/// Parse the data tree selected item on double clicking, or copy pasting into it.
		/// </summary>
		/// <param name="selectedNode"></param>
		public static void ParseOnDataTreeSelectedItem(WzNode selectedNode, bool expandDataTree = true) {
			var wzImage = (WzImage) selectedNode.Tag;

			if (!wzImage.Parsed) {
				wzImage.ParseImage();
			}

			selectedNode.Reparse();
			if (expandDataTree) {
				selectedNode.IsExpanded = true;
			}
		}

		private void Tree_KeyDown(object sender, KeyEventArgs e) {
			//if (!IsFocused) return;
			var ctrl = Keyboard.IsKeyDown(Key.LeftCtrl);
			var alt = Keyboard.IsKeyDown(Key.LeftAlt);
			var shift = Keyboard.IsKeyDown(Key.LeftShift);

			switch (e.Key) {
				case Key.F2:
					e.Handled = true;
					//e.SuppressKeyPress = true;
					if (Program.ConfigurationManager.UserSettings.QuickEdit && SelectedNode.Header is EditableTextBlock header) {
						if (header.IsInEditMode) {
							header.IsInEditMode = false;
							SelectedNode.Focus();
						} else {
							header.IsInEditMode = true;
						}
					} else {
						_mainPanel.PromptRenameWzTreeNode(SelectedNode);
					}

					break;
				case Key.F5:
					MainPanel.StartAnimateSelectedCanvas();
					break;
				case Key.Escape:
					e.Handled = true;
					//e.SuppressKeyPress = true;
					break;

				case Key.Delete:
					e.Handled = true;
					//e.SuppressKeyPress = true;

					MainPanel.PromptRemoveSelectedTreeNodes();
					break;
			}

			if (ctrl) {
				switch (e.Key) {
					case Key.R: // Render map

						//HaRepackerMainPanel.
						e.Handled = true;
						//e.SuppressKeyPress = true;
						break;
					case Key.C:
						MainPanel.DoCopy();
						e.Handled = true;
						//e.SuppressKeyPress = true;
						break;
					case Key.V:
						MainPanel.DoPaste();
						e.Handled = true;
						//e.SuppressKeyPress = true;
						break;
					case Key.F: // open search box
						if (MainPanel.grid_FindPanel.Visibility == Visibility.Collapsed) {
							var sbb =
								(Storyboard)
								FindResource("Storyboard_Find_FadeIn");
							sbb.Begin();

							e.Handled = true;
							//e.SuppressKeyPress = true;
						}

						break;
					case Key.T:
					case Key.O:
						e.Handled = true;
						//e.SuppressKeyPress = true;
						break;
				}
			}
		}
		
		private string SearchTerm = "";
		private bool IsSearching;
		private DateTime LastSearch = DateTime.Now;

		private void Tree_TextInput(object sender, TextCompositionEventArgs e) {
			if ((DateTime.Now - LastSearch).Seconds > 1) {
				SearchTerm = "";
			}

			LastSearch = DateTime.Now;
			SearchTerm += e.Text;

			IsSearching = true;
			foreach (var t in Items) {
				if (SearchTreeView((WzNode) t, SearchTerm.ToLower())) {
					break;
				}
			}

			IsSearching = false;
		}

		private bool SearchTreeView(WzNode node, string searchterm) {
			foreach (WzNode subnode in node.Items) {
				if (subnode.IsExpanded) {
					if (SearchTreeView(subnode, searchterm)) {
						return true;
					}
				}

				if (!subnode.Text.ToLower().StartsWith(searchterm)) {
					continue;
				}

				subnode.IsSelected = true;
				TreeViewHelper.BringIntoView(subnode);
				return true;
			}
			return false;
		}
	}
}