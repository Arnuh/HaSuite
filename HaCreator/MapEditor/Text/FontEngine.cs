﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using HaSharedLibrary.Util;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace HaCreator.MapEditor.Text {
	public class FontEngine {
		private Bitmap globalBitmap;
		private Graphics globalGraphics;
		private Font font;
		private GraphicsDevice device;

		private CharTexture[] characters = new CharTexture[0x100];

		public FontEngine(string fontName, FontStyle fontStyle, float size, GraphicsDevice device) {
			this.device = device;
			font = new Font(fontName, size, fontStyle);
			globalBitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
			globalGraphics = Graphics.FromImage(globalBitmap);
			globalGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

			//format.Alignment = StringAlignment.Near;
			//format.LineAlignment = StringAlignment.Near;

			for (var ch = (char) 0; ch < 0x100; ch++) {
				characters[ch] = RasterizeCharacter(ch);
			}
		}

		private Brush brush = new SolidBrush(Color.White);
		private StringFormat format = new();

		private CharTexture RasterizeCharacter(char ch) {
			var text = ch.ToString();

			// Causes truetype fonts to be rendered in their exact width
			var format = StringFormat.GenericTypographic;
			var size = globalGraphics.MeasureString(text, font, new PointF(0, 0), format);

			// If the character is unprintable, measure it with the truetype padding to receive its padding width
			if (size.Width < 1) {
				format = StringFormat.GenericDefault;
				size = globalGraphics.MeasureString(text, font);
			}

			var width = (int) Math.Ceiling(size.Width);
			var height = (int) Math.Ceiling(size.Height);

			var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

			using (var graphics = Graphics.FromImage(bitmap)) {
				graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
				graphics.DrawString(text, font, brush, 0, 0, format);
			}

			return new CharTexture(bitmap.ToTexture2D(device), width, height);
		}


		//draws a string to the global device using the font textures.
		//if the string exceedds maxWidth, it will be truncated with dots
		public void DrawString(SpriteBatch sprite, Point position, Microsoft.Xna.Framework.Color color, string str,
			int maxWidth) {
			//if the string is too long, truncate it and place "..."
			if (UserSettings.ClipText && globalGraphics.MeasureString(str, font).Width > maxWidth) {
				var dotsWidth = (int) globalGraphics
					.MeasureString("...", font, new PointF(0, 0), StringFormat.GenericTypographic).Width;
				do {
					str = str.Substring(0, str.Length - 1);
				} while (globalGraphics.MeasureString(str, font).Width + dotsWidth > maxWidth);

				str += "...";
			}

			var xOffs = 0;
			foreach (var c in str.ToCharArray()) {
				if (c > 256)
					//hack to stop attempting to draw languages other than english
				{
					return;
				}

				var w = characters[c].w;
				var h = characters[c].h;
				sprite.Draw(characters[c].texture,
					new Rectangle(position.X + xOffs, position.Y, w, h), color);
				xOffs += w;
			}
		}

		public SizeF MeasureString(string s) {
			return globalGraphics.MeasureString(s, font);
		}
	}
}