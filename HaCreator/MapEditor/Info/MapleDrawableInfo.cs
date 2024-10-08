﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Drawing;
using HaCreator.CustomControls;
using HaCreator.MapEditor.MonoGame;
using HaCreator.Properties;
using HaSharedLibrary.Util;
using MapleLib.WzLib;
using Microsoft.Xna.Framework.Graphics;

namespace HaCreator.MapEditor.Info {
	public abstract class MapleDrawableInfo {
		private Bitmap image;
		private Texture2D texture;
		private Point origin;
		private WzObject parentObject;
		private int width;
		private int height;

		public MapleDrawableInfo(Bitmap image, Point origin, WzObject parentObject) {
			Image = image;
			this.origin = origin;
			this.parentObject = parentObject;
		}

		/// <summary>
		/// Create an instance of BoardItem from editor panels
		/// </summary>
		/// <param name="layer"></param>
		/// <param name="board"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="flip"></param>
		/// <returns></returns>
		public abstract BoardItem CreateInstance(Layer layer, Board board, int x, int y, int z, bool flip);

		public virtual Texture2D GetTexture(Renderer graphics) {
			if (texture == null) {
				if (image != null && image.Width == 1 && image.Height == 1) {
					texture = Resources.placeholder.ToTexture2D(graphics.GraphicsDevice);
				} else {
					texture = image.ToTexture2D(graphics.GraphicsDevice);
				}
			}

			return texture;
		}

		public virtual WzObject ParentObject {
			get => parentObject;
			set => parentObject = value;
		}

		public virtual Bitmap Image {
			get =>
				//  if(image.Width==1 && image.Height==1)
				//        return global::HaCreator.Properties.Resources.placeholder;
				image;
			set {
				image = value;
				texture = null;

				if (image != null) {
					width = image.Width;
					height = image.Height;
				}
			}
		}

		public virtual int Width => width;

		public virtual int Height => height;

		public virtual Point Origin {
			get => origin;
			set => origin = value;
		}
	}
}