﻿using System.Windows.Controls;
using System.Windows.Media;
using TreeView = System.Windows.Controls.TreeView;

namespace HaRepacker.GUI {
	public class TreeViewHelper {
		private static T FindVisualChild<T>(Visual visual) where T : Visual {
			for (var i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++) {
				var child = (Visual) VisualTreeHelper.GetChild(visual, i);
				if (child != null) {
					var correctlyTyped = child as T;
					if (correctlyTyped != null) {
						return correctlyTyped;
					}

					var descendent = FindVisualChild<T>(child);
					if (descendent != null) {
						return descendent;
					}
				}
			}

			return null;
		}

		public static void BringIntoView(TreeViewItem item) {
			// Virtualized TreeView's don't properly bring into view
			// So we do a workaround from stackoverflow
			var parent = item.Parent as ItemsControl;

			if (parent != null) {
				var itemHost = FindVisualChild<VirtualizingStackPanel>(parent);

				if (itemHost != null) {
					itemHost.BringIndexIntoViewPublic(parent.Items.IndexOf(item));
					item.Focus();
				}
			}
		}

		public static WzNode GetTopLevelNode(TreeViewItem item) {
			var parent = item.Parent;
			while (parent is TreeViewItem tvi) {
				if (tvi.Parent is TreeView) {
					break;
				}

				parent = tvi.Parent;
			}

			return parent as WzNode;
		}

		public static void Expand(WzNode node) {
			if (!node.IsExpanded) {
				node.IsExpanded = true;
			}

			var parent = node.Parent;
			if (parent is WzNode p) {
				Expand(p);
			}
		}
	}
}