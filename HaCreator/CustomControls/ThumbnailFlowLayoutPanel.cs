/*
  koolk's Map Editor

  Copyright (c) 2009-2013 koolk

  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

     1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.

     2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.

     3. This notice may not be removed or altered from any source
     distribution.
*/

using System.Drawing;
using System.Windows.Forms;

namespace HaCreator.CustomControls {
	public class ThumbnailFlowLayoutPanel : FlowLayoutPanel {
		/// <summary>
		/// Constructor
		/// </summary>
		public ThumbnailFlowLayoutPanel() {
		}

		protected override Point ScrollToControl(Control activeControl) {
			return AutoScrollPosition;
		}

		public ImageViewer Add(Bitmap bitmap, string name, bool Text) {
			var imageViewer = new ImageViewer();
			imageViewer.Dock = DockStyle.Left;

			if (bitmap == null) {
				var fallbackBmp = Properties.Resources.placeholder;

				imageViewer.Image = fallbackBmp; // fallback in case its null
				imageViewer.Width = fallbackBmp.Width + 8;
				imageViewer.Height = fallbackBmp.Height + 8 + (Text ? 16 : 0);
			} else {
				imageViewer.Image = new Bitmap(bitmap); // Copying the bitmap for thread safety
				imageViewer.Width = bitmap.Width + 8;
				imageViewer.Height = bitmap.Height + 8 + (Text ? 16 : 0);
			}

			imageViewer.IsText = Text;
			imageViewer.Name = name;
			imageViewer.IsThumbnail = false;

			Controls.Add(imageViewer);

			return imageViewer;
		}

		private void InitializeComponent() {
			SuspendLayout();
			// 
			// ThumbnailFlowLayoutPanel
			// 
			Font = new Font("Segoe UI", 8.25F);
			ResumeLayout(false);
		}
	}
}