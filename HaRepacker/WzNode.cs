/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using HaRepacker.GUI.Input;
using MapleLib.WzLib;
using MessageBox = System.Windows.Forms.MessageBox;

namespace HaRepacker {
	public class WzNode : TreeViewItem {
		public static ContextMenuManager ContextMenuManager = null;

		private bool isWzObjectAddedManually;
		public static Brush NewObjectForeColor = Brushes.Red;

		public string Text {
			get {
				if (Header is EditableTextBlock editableTextBlock) {
					return editableTextBlock.Text;
				}

				return Header as string;
			}
			set {
				if (Header is EditableTextBlock editableTextBlock) {
					editableTextBlock.Text = value;
				} else {
					Header = value;
				}
			}
		}

		public ItemCollection Nodes => Items;

		public WzNode(WzObject SourceObject, bool isWzObjectAddedManually = false)
			: base() {
			this.isWzObjectAddedManually = isWzObjectAddedManually;
			if (isWzObjectAddedManually) {
				Foreground = NewObjectForeColor;
			}

			// Can't figure out how to keep performance comparable to the original
			// So add an option to decide if people want this feature but slower sorts
			// or just directly use strings which sort faster.
			if (Program.ConfigurationManager.UserSettings.QuickEdit) {
				var textBlock = new EditableTextBlock() {Text = SourceObject.Name};
				textBlock.EditListener += (sender, e) => { ChangeName(textBlock.Text); };
				Header = textBlock;
			} else {
				Header = SourceObject.Name;
			}

			if (Program.ConfigurationManager.UserSettings.Sort) {
				Items.SortDescriptions.Clear();
				Items.SortDescriptions.Add(TreeViewMS.Sort);
			}

			PreviewMouseRightButtonDown += (sender, e) => {
				if (!(sender is WzNode node)) {
					return;
				}

				node.IsSelected = true;
				node.ContextMenu = ContextMenuManager.CreateMenu(this, SourceObject);
			};
			// Childs
			ParseChilds(SourceObject);
		}

		private void ParseChilds(WzObject SourceObject) {
			Tag = SourceObject ?? throw new NullReferenceException("Cannot create a null WzNode");
			SourceObject.HRTag = this;

			if (SourceObject is WzFile file) {
				SourceObject = file.WzDirectory;
			}

			if (SourceObject is WzDirectory directory) {
				foreach (var dir in directory.WzDirectories)
					Items.Add(new WzNode(dir));
				foreach (var img in directory.WzImages)
					Items.Add(new WzNode(img));
			} else if (SourceObject is WzImage image) {
				if (!image.Parsed) return;
				foreach (var prop in image.WzProperties)
					Items.Add(new WzNode(prop));
			} else if (SourceObject is IPropertyContainer container) {
				foreach (var prop in container.WzProperties)
					Items.Add(new WzNode(prop));
			}

			Items.Refresh();
		}
		
		private void RefreshChilds(WzObject SourceObject) {
			Tag = SourceObject ?? throw new NullReferenceException("Cannot create a null WzNode");
			SourceObject.HRTag = this;

			if (SourceObject is WzFile file) {
				SourceObject = file.WzDirectory;
			}

			if (SourceObject is WzDirectory directory) {
				foreach (var dir in directory.WzDirectories)
					AddIfMissing(dir);
				foreach (var img in directory.WzImages)
					AddIfMissing(img);
			} else if (SourceObject is WzImage image) {
				if (!image.Parsed) return;
				foreach (var prop in image.WzProperties)
					AddIfMissing(prop);
			} else if (SourceObject is IPropertyContainer container) {
				foreach (var prop in container.WzProperties)
					AddIfMissing(prop);
			}
		}

		private void AddIfMissing(WzObject obj) {
			if (GetChildNode(this, obj.Name) == null) {
				Items.Add(new WzNode(obj));
			}
		}

		public void DeleteWzNode() {
			if (Parent is WzNode parent) {
				parent.Items.Remove(this);
			} else if (Parent is TreeViewMS treeView) {
				treeView.Items.Remove(this);
			}

			if (Tag is WzImageProperty property) {
				if (property.ParentImage == null) // _inlink WzNode doesnt have a parent
				{
					return;
				}

				property.ParentImage.Changed = true;
			}

			((WzObject) Tag).Remove();
		}

		public bool IsWzObjectAddedManually {
			get => isWzObjectAddedManually;
			private set { }
		}

		public bool CanHaveChilds =>
			Tag is WzFile ||
			Tag is WzDirectory ||
			Tag is WzImage ||
			Tag is IPropertyContainer;

		public static WzNode GetChildNode(WzNode parentNode, string name) {
			foreach (WzNode node in parentNode.Items) {
				if (node.Text == name) {
					return node;
				}
			}

			return null;
		}

		public static bool CanNodeBeInserted(WzNode parentNode, string name) {
			var obj = (WzObject) parentNode.Tag;
			if (obj is IPropertyContainer container) {
				return container[name] == null;
			}

			if (obj is WzDirectory directory) {
				return directory[name] == null;
			}

			if (obj is WzFile file) {
				return file.WzDirectory?[name] == null;
			}

			return false;
		}

		private bool AddObjInternal(WzObject obj) {
			var TaggedObject = (WzObject) Tag;
			if (TaggedObject is WzFile file) {
				TaggedObject = file.WzDirectory;
			}

			if (TaggedObject is WzDirectory directory) {
				if (obj is WzDirectory wzDirectory) {
					directory.AddDirectory(wzDirectory);
				} else if (obj is WzImage wzImgProperty) {
					directory.AddImage(wzImgProperty);
				} else {
					return false;
				}
			} else if (TaggedObject is WzImage wzImageProperty) {
				if (!wzImageProperty.Parsed) {
					wzImageProperty.ParseImage();
				}

				if (obj is WzImageProperty imgProperty) {
					wzImageProperty.AddProperty(imgProperty);
					wzImageProperty.Changed = true;
				} else {
					return false;
				}
			} else if (TaggedObject is IPropertyContainer container) {
				if (obj is WzImageProperty property) {
					container.AddProperty(property);
					if (TaggedObject is WzImageProperty imgProperty) {
						imgProperty.ParentImage.Changed = true;
					}
				} else {
					return false;
				}
			} else {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Adds a node
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public bool AddNode(WzNode node, bool reparseImage) {
			if (CanNodeBeInserted(this, node.Text)) {
				TryParseImage(reparseImage);
				Items.Add(node);
				AddObjInternal((WzObject) node.Tag);
				return true;
			}

			MessageBox.Show(
				"Cannot insert node \"" + node.Text +
				"\" because a node with the same name already exists. Skipping.", "Skipping Node",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return false;
		}

		/// <summary>
		/// Try parsing the WzImage if it have not been loaded
		/// </summary>
		private void TryParseImage(bool reparseImage = true) {
			if (!(Tag is WzImage image)) return;
			image.ParseImage();
			if (reparseImage) Reparse();
		}

		/// <summary>
		/// Adds a WzObject to the WzNode and returns the newly created WzNode
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="undoRedoMan"></param>
		/// <returns></returns>
		public WzNode AddObject(WzObject obj, UndoRedoManager undoRedoMan) {
			if (CanNodeBeInserted(this, obj.Name)) {
				TryParseImage();
				if (AddObjInternal(obj)) {
					var node = new WzNode(obj, true);
					Nodes.Add(node);

					if (node.Tag is WzImageProperty property) property.ParentImage.Changed = true;

					undoRedoMan.AddUndoBatch(new List<UndoRedoAction>
						{UndoRedoManager.ObjectAdded(this, node)});
					//node.EnsureVisible();
					return node;
				}

				Warning.Error("Could not insert property, make sure all types are correct");
				return null;
			}

			MessageBox.Show(
				"Cannot insert object \"" + obj.Name +
				"\" because an object with the same name already exists. Skipping.", "Skipping Object",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			return null;
		}

		public void Reparse() {
			Nodes.Clear();
			ParseChilds((WzObject) Tag);
		}

		public void Refresh() {
			RefreshChilds((WzObject) Tag);
		}

		public void RemoveExisting(WzObject obj) {
			var existingNode = GetChildNode(this, obj.Name);
			if (existingNode != null) {
				Nodes.Remove(existingNode);
			}
		}
        
		public void OnWzObjectAdded(WzObject obj, UndoRedoManager undoRedoMan) {
			var node = new WzNode(obj, true);
			Nodes.Add(node);
			undoRedoMan?.AddUndoBatch(new List<UndoRedoAction>
				{UndoRedoManager.ObjectAdded(this, node)});
		}

		public string GetTypeName() {
			return Tag.GetType().Name;
		}

		/// <summary>
		/// Change the name of the WzNode
		/// </summary>
		/// <param name="name"></param>
		public void ChangeName(string name) {
			if (Tag is WzObject obj) {
				// Should we check node instead of wzobj?
				if (obj.Name.Equals(name)) {
					return;
				}

				obj.Name = Name;
			}
			Text = name;
			if (Tag is WzImageProperty property) {
				property.ParentImage.Changed = true;
			}

			isWzObjectAddedManually = true;
			Foreground = NewObjectForeColor;
		}

		public WzNode TopLevelNode {
			get {
				var parent = TreeViewMS.GetSelectedTreeViewItemParent(this);
				return parent as WzNode;
			}
		}

		private static WzNode GetParentItem(TreeViewItem item) {
			for (var i = VisualTreeHelper.GetParent(item); i != null; i = VisualTreeHelper.GetParent(i)) {
				if (i is WzNode viewItem) {
					return viewItem;
				}
			}

			return null;
		}

		public string GetFullPath() {
			var result = Text;

			for (var i = GetParentItem(this); i != null; i = GetParentItem(i)) {
				result = i.Text + "\\" + result;
			}

			return result;
		}

		public void CollapseAll() {
			CollapseTreeviewItems(this);
		}

		public static void CollapseTreeviewItems(WzNode Item) {
			Item.IsExpanded = false;
			foreach (WzNode item in Item.Items) {
				item.IsExpanded = false;

				if (item.HasItems) {
					CollapseTreeviewItems(item);
				}
			}
		}

		public T ParentOfType<T>(DependencyObject element) where T : DependencyObject {
			if (element == null) {
				return default;
			}

			return GetParents(element).OfType<T>().FirstOrDefault();
		}

		public IEnumerable<DependencyObject> GetParents(DependencyObject element) {
			if (element == null) {
				throw new ArgumentNullException("element");
			}

			while ((element = GetParent(element)) != null) {
				yield return element;
			}
		}

		private DependencyObject GetParent(DependencyObject element) {
			var parent = VisualTreeHelper.GetParent(element);
			if (parent != null) {
				return parent;
			}

			if (element is FrameworkElement frameworkElement) {
				parent = frameworkElement.Parent;
			}

			return parent;
		}
	}
}