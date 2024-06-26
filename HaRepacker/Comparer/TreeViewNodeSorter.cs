﻿using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace HaRepacker.Comparer {
	public class TreeViewNodeSorter : IComparer {

		/// <summary>
		/// Constructor
		/// </summary>
		public TreeViewNodeSorter() {
			
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Compare(object s1_, object s2_) {
			var t1 = s1_ as TreeNode;
			var t2 = s2_ as TreeNode;

			var s1Text = t1.Text;
			var s2Text = t2.Text;

			var isS1Numeric = IsNumeric(s1Text);
			var isS2Numeric = IsNumeric(s2Text);

			if (isS1Numeric && isS2Numeric) {
				var s1val = Convert.ToInt32(s1Text);
				var s2val = Convert.ToInt32(s2Text);

				if (s1val > s2val) {
					return 1;
				}

				if (s1val < s2val) {
					return -1;
				}

				if (s1val == s2val) {
					return 0;
				}
			} else if (isS1Numeric && !isS2Numeric) {
				return -1;
			} else if (!isS1Numeric && isS2Numeric) {
				return 1;
			}

			return string.Compare(s1Text, s2Text, true);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsNumeric(string value) {
			var parseInt = 0;
			return int.TryParse(value, out parseInt);
		}
	}
}