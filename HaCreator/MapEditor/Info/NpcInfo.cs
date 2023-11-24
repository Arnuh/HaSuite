﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using HaCreator.GUI;
using HaCreator.MapEditor.Instance;
using HaCreator.Wz;
using HaSharedLibrary.Wz;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCreator.MapEditor.Info {
	public class NpcInfo : MapleExtractableInfo {
		private readonly string id;
		private readonly string name;

		private WzImage _LinkedWzImage;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="image"></param>
		/// <param name="origin"></param>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="parentObject"></param>
		public NpcInfo(Bitmap image, Point origin, string id, string name, WzObject parentObject)
			: base(image, origin, parentObject) {
			this.id = id;
			this.name = name;
			if (image != null && image.Width == 1 && image.Height == 1)
				image = Properties.Resources.placeholder;
		}

		private void ExtractPNGFromImage(WzImage image) {
			var npcImage = WzInfoTools.GetNpcImage(image);
			if (npcImage != null) {
				Image = npcImage.GetLinkedWzCanvasBitmap();
				if (Image.Width == 1 && Image.Height == 1) Image = Properties.Resources.placeholder;

				Origin = WzInfoTools.PointFToSystemPoint(npcImage.GetCanvasOriginPosition());
			}
			else {
				Image = new Bitmap(1, 1);
				Origin = new Point();
			}
		}

		public override void ParseImage() {
			if (LinkedWzImage != null) // attempt to load from here too
				ExtractPNGFromImage(LinkedWzImage);
			else
				ExtractPNGFromImage((WzImage) ParentObject);
		}

		public static NpcInfo Get(string id) {
			var imgName = WzInfoTools.AddLeadingZeros(id, 7) + ".img";
			var npcImage = (WzImage) Program.WzManager.FindWzImageByName("npc", imgName);
			if (npcImage == null)
				return null;

			if (!npcImage.Parsed)
				npcImage.ParseImage();
			if (npcImage.HCTag == null)
				npcImage.HCTag = Load(npcImage);
			var result = (NpcInfo) npcImage.HCTag;
			result.ParseImageIfNeeded();
			return result;
		}

		private static NpcInfo Load(WzImage parentObject) {
			var id = WzInfoTools.RemoveExtension(parentObject.Name);
			return new NpcInfo(null, new Point(), id, WzInfoTools.GetNpcNameById(id, Program.WzManager),
				parentObject);
		}

		public override BoardItem CreateInstance(Layer layer, Board board, int x, int y, int z, bool flip) {
			if (Image == null) ParseImage();
			return new NpcInstance(this, board, x, y, UserSettings.Npcrx0Offset, UserSettings.Npcrx1Offset, 8, null, 0,
				flip, false, null, null);
		}

		public BoardItem CreateInstance(Board board, int x, int y, int rx0Shift, int rx1Shift, int yShift,
			string limitedname, int? mobTime, MapleBool flip, MapleBool hide, int? info, int? team) {
			if (Image == null) ParseImage();
			return new NpcInstance(this, board, x, y, rx0Shift, rx1Shift, yShift, limitedname, mobTime, flip, hide,
				info, team);
		}

		public string ID {
			get => id;
			private set { }
		}

		public string Name {
			get => name;
			private set { }
		}

		/// <summary>
		/// The source WzImage of the reactor or default
		/// </summary>
		public WzImage LinkedWzImage {
			get {
				if (_LinkedWzImage == null) {
					var imgName = WzInfoTools.AddLeadingZeros(id, 7) + ".img";
					var npcImage = (WzImage) Program.WzManager.FindWzImageByName("npc", imgName);

					var link = (WzStringProperty) npcImage?["info"]?["link"];
					if (link != null) {
						var linkImgName = WzInfoTools.AddLeadingZeros(link.Value, 7) + ".img";
						var linkedImage = (WzImage) Program.WzManager.FindWzImageByName("npc", linkImgName);

						_LinkedWzImage = linkedImage ?? npcImage; // fallback to npcImage if null
					}
					else {
						_LinkedWzImage = npcImage;
					}
				}

				return _LinkedWzImage;
			}

			set => _LinkedWzImage = value;
		}
	}
}