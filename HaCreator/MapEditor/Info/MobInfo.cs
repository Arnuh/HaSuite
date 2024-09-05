/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Drawing;
using HaCreator.MapEditor.Info.Default;
using HaCreator.MapEditor.Instance;
using HaSharedLibrary.Wz;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace HaCreator.MapEditor.Info {
	public class MobInfo : MapleExtractableInfo {
		private readonly string id;
		private readonly string name;

		private WzImage _LinkedWzImage;
		private string link;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="image"></param>
		/// <param name="origin"></param>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="parentObject"></param>
		public MobInfo(Bitmap image, Point origin, string id, string name, WzObject parentObject)
			: base(image, origin, parentObject) {
			this.id = id;
			this.name = name;
		}

		private void ExtractPNGFromImage(WzImage image) {
			var mobImage = WzInfoTools.GetMobImage(image);
			if (mobImage != null) {
				Image = mobImage.GetLinkedWzCanvasBitmap();
				Origin = WzInfoTools.PointFToSystemPoint(mobImage.GetCanvasOriginPosition());
			} else {
				Image = new Bitmap(1, 1);
				Origin = new Point();
			}
		}

		public override void ParseImage() {
			if (LinkedWzImage != null) // load from here too
			{
				ExtractPNGFromImage(_LinkedWzImage);
			} else {
				ExtractPNGFromImage((WzImage) ParentObject);
			}
		}

		/// <summary>
		/// Get monster by ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static MobInfo Get(string id) {
			var imgName = WzInfoTools.AddLeadingZeros(id, 7) + ".img";

			var mobImage = (WzImage) Program.WzManager.FindWzImageByName("mob", imgName);
			if (mobImage == null) {
				return null;
			}

			if (!mobImage.Parsed) {
				mobImage.ParseImage();
			}

			if (mobImage.HCTag == null) {
				mobImage.HCTag = Load(mobImage);
			}

			var result = (MobInfo) mobImage.HCTag;
			result.ParseImageIfNeeded();
			return result;
		}

		private static MobInfo Load(WzImage parentObject) {
			var id = WzInfoTools.RemoveExtension(parentObject.Name);
			return new MobInfo(null, new Point(), id, WzInfoTools.GetMobNameById(id, Program.WzManager),
				parentObject);
		}

		public override BoardItem CreateInstance(Layer layer, Board board, int x, int y, int z, bool flip) {
			if (Image == null) {
				ParseImage();
			}

			return new MobInstance(this, board, x, y, UserSettings.Mobrx0Offset, UserSettings.Mobrx1Offset, 20, Defaults.Life.LimitedName,
				UserSettings.defaultMobTime, flip, Defaults.Life.Hide, Defaults.Life.Info, Defaults.Life.Team);
		}

		public BoardItem CreateInstance(Board board, int x, int y, int rx0Shift, int rx1Shift, int yShift,
			string limitedname, int mobTime, bool flip, bool hide, int info, int team) {
			if (Image == null) {
				ParseImage();
			}

			return new MobInstance(this, board, x, y, rx0Shift, rx1Shift, yShift, limitedname, mobTime, flip, hide,
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
		/// The source WzImage of the reactor
		/// </summary>
		public WzImage LinkedWzImage {
			get {
				if (_LinkedWzImage != null) {
					return _LinkedWzImage;
				}

				var imgName = WzInfoTools.AddLeadingZeros(id, 7) + ".img";

				var mobImage = (WzImage) Program.WzManager.FindWzImageByName("mob", imgName); // default;

				var linkProp = (WzStringProperty) mobImage?["info"]?["link"];
				if (linkProp != null) {
					link = linkProp.Value;
					var linkImgName = WzInfoTools.AddLeadingZeros(linkProp.Value, 7) + ".img";
					var linkedImage = (WzImage) Program.WzManager.FindWzImageByName("mob", linkImgName);

					_LinkedWzImage = linkedImage ?? mobImage; // fallback to mobImage if linkedimage isnt available
				} else {
					_LinkedWzImage = mobImage;
				}

				return _LinkedWzImage;
			}
			set => _LinkedWzImage = value;
		}

		public string Link => link;
	}
}