/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using HaCreator.MapEditor.Text;
using HaSharedLibrary.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = System.Drawing.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace HaCreator.MapEditor.MonoGame {
	public interface Renderer {
		public MultiBoard MultiBoard { get; set; }

		public bool DeviceReady { get; set; }

		public SpriteBatch Sprite { get; }
		public FontEngine FontEngine { get; }
		public GraphicsDevice GraphicsDevice { get; }

		public Texture2D Pixel { get; }

		public void Start();

		public void OnSizeChanged();

		public void DrawLine(Vector2 start, Vector2 end, Color color);

		public void DrawRectangle(Rectangle rectangle, Color color);

		public void FillRectangle(Rectangle rectangle, Color color);

		public void DrawDot(int x, int y, Color color, int dotSize);

		public void DrawString(string str, int x, int y);

		public void DrawString(Point position, Color color, string str, int maxWidth);

		public void Draw(
			Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effects,
			float layerDepth);
	}
}