/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

// Note - Foothold mapper code originally by Odecey

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace Footholds {
	public partial class DisplayMap : Form {
		/*bool sSwitch = true;
		  private bool isDown = false;
		  private int mouseXOffsset = 0;
		  private int mouseYOffset = 0;*/


		public List<object> settings;
		public int xOffset = 0;
		public int yOffset = 0;
		public double scale = 1;
		public Image map;

		public List<SpawnPoint.Spawnpoint> MobSpawnPoints;
		public List<FootHold.Foothold> Footholds;
		public List<Portals.Portal> thePortals;

		public DisplayMap() {
			InitializeComponent();

			FormClosing += DisplayMap_FormClosing;
		}

		private void DisplayMap_FormClosing(object sender, FormClosingEventArgs e) {
			if (map != null)
				map.Dispose();

			if (MapPBox.Image != null)
				MapPBox.Image.Dispose();

			if (MobSpawnPoints != null)
				MobSpawnPoints.Clear();
			if (Footholds != null)
				Footholds.Clear();
			if (thePortals != null)
				thePortals.Clear();
		}

		private void DisplayMap_Load(object sender, EventArgs e) {
			AutoScroll = true;
			var theMap = ResizeBitMap(new Bitmap(map), (int) (map.Width * scale), (int) (map.Height * scale));

			MapPBox.Size = theMap.Size;
			MapPBox.Image = theMap;

			xOffset = (int) ((thePortals.ToArray()[0].Shape.X + 20 -
			                  ((WzIntProperty) thePortals.ToArray()[0].Data["x"]).Value) * -1);
			yOffset = (int) ((thePortals.ToArray()[0].Shape.Y + 20 -
			                  ((WzIntProperty) thePortals.ToArray()[0].Data["y"]).Value) * -1);
		}

		public Bitmap ResizeBitMap(Bitmap img, int nWidth, int nHeight) {
			var result = new Bitmap(nWidth, nHeight);
			using (var g = Graphics.FromImage((Image) result)) {
				g.DrawImage(img, 0, 0, nWidth, nHeight);
			}

			return result;
		}


		private void MapPBox_MouseClick(object sender, MouseEventArgs e) {
			var index = 0;
			int tempX;
			int tempY;
			var tempRect = new Rectangle();
			foreach (var foothold in Footholds) {
				tempX = (int) (Footholds.ToArray()[index].Shape.X * scale);
				tempY = (int) (Footholds.ToArray()[index].Shape.Y * scale);
				tempRect = new Rectangle(tempX, tempY, (int) (foothold.Shape.Width * scale),
					(int) (foothold.Shape.Height * scale));
				if (tempRect.IntersectsWith(new Rectangle(e.X, e.Y, 1, 1))) {
					var editFoothold = new Edit();
					editFoothold.Text = string.Format("{0}: {1}", HaRepacker.Properties.Resources.EditFoothold,
						foothold.Data.Name);
					editFoothold.fh = Footholds.ToArray()[index];
					editFoothold.settings = settings;
					editFoothold.ShowDialog();
				}

				index++;
			}

			index = 0;
			foreach (var portal in thePortals) {
				tempX = (int) (thePortals.ToArray()[index].Shape.X * scale);
				tempY = (int) (thePortals.ToArray()[index].Shape.Y * scale);
				tempRect = new Rectangle(tempX, tempY, (int) (portal.Shape.Width * scale),
					(int) (portal.Shape.Height * scale));
				if (tempRect.IntersectsWith(new Rectangle(e.X, e.Y, 1, 1))) {
					var editPortals = new EditPortals();
					editPortals.Text = string.Format("{0}: {1}", HaRepacker.Properties.Resources.EditPortal,
						portal.Data.Name);
					editPortals.portal = thePortals.ToArray()[index];
					editPortals.Settings = settings;
					editPortals.ShowDialog();
				}

				index++;
			}

			index = 0;
			foreach (var spawnpoint in MobSpawnPoints) {
				tempX = (int) (MobSpawnPoints.ToArray()[index].Shape.X * scale);
				tempY = (int) (MobSpawnPoints.ToArray()[index].Shape.Y * scale);
				tempRect = new Rectangle(tempX, tempY, (int) (spawnpoint.Shape.Width * scale),
					(int) (spawnpoint.Shape.Height * scale));
				if (tempRect.IntersectsWith(new Rectangle(e.X, e.Y, 1, 1))) {
					var spawnInfo = new SpawnpointInfo();
					spawnInfo.spawnpoint = spawnpoint;
					spawnInfo.Text = string.Format("{0}: {1}", HaRepacker.Properties.Resources.EditSP,
						spawnpoint.Data.Name);
					spawnInfo.ShowDialog();
				}

				index++;
			}
		}

		private void DisplayMap_Resize(object sender, EventArgs e) {
			if (WindowState == FormWindowState.Maximized)
				Size = MapPBox.Size;
		}

		private void DisplayMap_MouseMove(object sender, MouseEventArgs e) {
		}

		private void MapPBox_MouseMove(object sender, MouseEventArgs e) {
			// The commented code was an attempt to use the mouse to navigate around the map
			// with the mouse. Unfortunately I did not get it to work properly.
			//  if (!isDown)
			Text = "Map X: " + ((int) (xOffset + e.X / scale)).ToString() + " Y: " +
			       ((int) (yOffset + e.Y / scale)).ToString();
			/*  else
			  {
			      if (sSwitch)
			      {
			          this.AutoScrollPosition = new Point((mouseXOffset - e.X), (mouseYOffset - e.Y));
			      }
			          mouseXBuffer = e.X;
			          mouseYBuffer = e.Y;

			      sSwitch = !sSwitch;
			  }*/
		}

		private void MapPBox_MouseDown(object sender, MouseEventArgs e) {
			/*  isDown = true;
			  mouseXOffsset = e.X;
			  mouseYOffset = e.Y;*/
		}

		private void MapPBox_MouseUp(object sender, MouseEventArgs e) {
			//    isDown = false;
		}
	}
}