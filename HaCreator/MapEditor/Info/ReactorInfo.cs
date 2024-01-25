/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Drawing;
using HaCreator.MapEditor.Instance;
using HaSharedLibrary.Wz;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace HaCreator.MapEditor.Info {
	public class ReactorInfo : MapleExtractableInfo {
		private readonly string id;

		private WzImage _LinkedWzImage;

		public ReactorInfo(Bitmap image, Point origin, string id, WzObject parentObject)
			: base(image, origin, parentObject) {
			this.id = id;
		}

		private void ExtractPNGFromImage(WzImage image) {
			var reactorImage = WzInfoTools.GetReactorImage(image);
			if (reactorImage != null) {
				Image = reactorImage.GetLinkedWzCanvasBitmap();
				Origin = WzInfoTools.PointFToSystemPoint(reactorImage.GetCanvasOriginPosition());
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

		public static ReactorInfo Get(string id) {
			var result = Program.InfoManager.Reactors[id];
			result.ParseImageIfNeeded();
			return result;
		}

		public static ReactorInfo Load(WzImage parentObject) {
			return new ReactorInfo(null, new Point(), WzInfoTools.RemoveExtension(parentObject.Name),
				parentObject);
		}

		public override BoardItem CreateInstance(Layer layer, Board board, int x, int y, int z, bool flip) {
			if (Image == null) ParseImage();
			return new ReactorInstance(this, board, x, y, UserSettings.defaultReactorTime, "", flip);
		}

		public BoardItem CreateInstance(Board board, int x, int y, int reactorTime, string name, bool flip) {
			if (Image == null) ParseImage();
			return new ReactorInstance(this, board, x, y, reactorTime, name, flip);
		}

		public string ID {
			get => id;
			private set { }
		}

		/// <summary>
		/// The source WzImage of the reactor
		/// </summary>
		public WzImage LinkedWzImage {
			get {
				if (_LinkedWzImage == null) {
					var imgName = WzInfoTools.AddLeadingZeros(id, 7) + ".img";
					var reactorObject = Program.WzManager.FindWzImageByName("reactor", imgName);

					var link = (WzStringProperty) reactorObject?["info"]?["link"];
					if (link != null) {
						var linkImgName = WzInfoTools.AddLeadingZeros(link.Value, 7) + ".img";
						var findLinkedImg = (WzImage) Program.WzManager.FindWzImageByName("reactor", linkImgName);

						_LinkedWzImage = findLinkedImg ?? (WzImage) reactorObject; // fallback if link is null
					} else {
						_LinkedWzImage = (WzImage) reactorObject;
					}
				}

				return _LinkedWzImage;
			}
			set => _LinkedWzImage = value;
		}
	}
}