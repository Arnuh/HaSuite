/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HaRepacker {
	/// <summary>
	/// Summary description for TreeViewMS.
	/// </summary>
	public class TreeViewMS : TreeView {
		protected ArrayList m_coll;
		protected TreeNode m_lastNode, m_firstNode;

		public TreeViewMS() {
			m_coll = new ArrayList();
		}

		protected override void OnPaint(PaintEventArgs pe) {
			// TODO: Add custom paint code here

			// Calling the base class OnPaint
			base.OnPaint(pe);
		}

		public ArrayList SelectedNodes {
			get => m_coll;
			set {
				if (value == null) {
					m_coll.Clear();
					return;
				}

				removePaintFromNodes();
				m_coll.Clear();
				m_coll = value;
				paintSelectedNodes();
			}
		}


// Triggers
//
// (overriden method, and base class called to ensure events are triggered)


		protected override void OnBeforeSelect(TreeViewCancelEventArgs e) {
			var bControl = ModifierKeys == Keys.Control;
			var bShift = ModifierKeys == Keys.Shift;

			// selecting twice the node while pressing CTRL ?
			if (bControl && m_coll.Contains(e.Node)) {
				// unselect it (let framework know we don't want selection this time)
				e.Cancel = true;

				// update nodes
				removePaintFromNodes();
				m_coll.Remove(e.Node);
				paintSelectedNodes();
				return;
			}

			m_lastNode = e.Node;
			if (!bShift) m_firstNode = e.Node; // store begin of shift sequence

			base.OnBeforeSelect(e);
		}


		protected override void OnAfterSelect(TreeViewEventArgs e) {
			var bControl = ModifierKeys == Keys.Control;
			var bShift = ModifierKeys == Keys.Shift;

			if (bControl) {
				if (!m_coll.Contains(e.Node)) // new node ?
				{
					m_coll.Add(e.Node);
				} else // not new, remove it from the collection
				{
					removePaintFromNodes();
					m_coll.Remove(e.Node);
				}

				paintSelectedNodes();
			} else {
				// SHIFT is pressed
				if (bShift) {
					var myQueue = new Queue();

					var uppernode = m_firstNode;
					var bottomnode = e.Node;
					// case 1 : begin and end nodes are parent
					var bParent = isParent(m_firstNode, e.Node); // is m_firstNode parent (direct or not) of e.Node
					if (!bParent) {
						bParent = isParent(bottomnode, uppernode);
						if (bParent) // swap nodes
						{
							var t = uppernode;
							uppernode = bottomnode;
							bottomnode = t;
						}
					}

					if (bParent) {
						var n = bottomnode;
						while (n != uppernode.Parent) {
							if (!m_coll.Contains(n)) // new node ?
								myQueue.Enqueue(n);

							n = n.Parent;
						}
					}
					// case 2 : nor the begin nor the end node are descendant one another
					else {
						if ((uppernode.Parent == null && bottomnode.Parent == null) ||
						    (uppernode.Parent != null &&
						     uppernode.Parent.Nodes.Contains(bottomnode))) // are they siblings ?
						{
							var nIndexUpper = uppernode.Index;
							var nIndexBottom = bottomnode.Index;
							if (nIndexBottom < nIndexUpper) // reversed?
							{
								var t = uppernode;
								uppernode = bottomnode;
								bottomnode = t;
								nIndexUpper = uppernode.Index;
								nIndexBottom = bottomnode.Index;
							}

							var n = uppernode;
							while (nIndexUpper <= nIndexBottom) {
								if (!m_coll.Contains(n)) // new node ?
									myQueue.Enqueue(n);

								n = n.NextNode;

								nIndexUpper++;
							} // end while
						} else {
							if (!m_coll.Contains(uppernode)) myQueue.Enqueue(uppernode);
							if (!m_coll.Contains(bottomnode)) myQueue.Enqueue(bottomnode);
						}
					}

					m_coll.AddRange(myQueue);

					paintSelectedNodes();
					m_firstNode = e.Node; // let us chain several SHIFTs if we like it
				} // end if m_bShift
				else {
					// in the case of a simple click, just add this item
					if (m_coll != null && m_coll.Count > 0) {
						removePaintFromNodes();
						m_coll.Clear();
					}

					m_coll.Add(e.Node);
				}
			}

			base.OnAfterSelect(e);
		}


// Helpers
//
//


		protected bool isParent(TreeNode parentNode, TreeNode childNode) {
			if (parentNode == childNode)
				return true;

			var n = childNode;
			var bFound = false;
			while (!bFound && n != null) {
				n = n.Parent;
				bFound = n == parentNode;
			}

			return bFound;
		}

		protected void paintSelectedNodes() {
			foreach (TreeNode n in m_coll) {
				n.BackColor = SystemColors.Highlight;
				n.ForeColor = SystemColors.HighlightText;
			}
		}

		protected void removePaintFromNodes() {
			if (m_coll.Count == 0) return;

			var n0 = (TreeNode) m_coll[0];
			/*Color back = n0.TreeView.BackColor;
			Color fore = n0.TreeView.ForeColor;*/
			var back = BackColor;
			var fore = ForeColor;

			foreach (TreeNode n in m_coll) {
				n.BackColor = back;

				var node = (WzNode) n;
				if (node.IsWzObjectAddedManually)
					n.ForeColor = WzNode.NewObjectForeColor;
				else
					n.ForeColor = fore;
			}
		}

		private void InitializeComponent() {
			SuspendLayout();
			// 
			// TreeViewMS
			// 
			Font = new Font("Segoe UI", 8F, FontStyle.Regular,
				GraphicsUnit.Point, (byte) 0);
			ResumeLayout(false);
		}

		public void RefreshSelectedNodes() {
			paintSelectedNodes();
		}
	}
}