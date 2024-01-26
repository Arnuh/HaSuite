﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */


using System.Drawing;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spine;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace HaSharedLibrary.Render.DX {
	public class DXObject : IDXObject {
		protected Texture2D texture;
		private readonly int _x;
		private readonly int _y;

		private readonly int delay;

		private object _Tag;

		// the color to use for reflection object
		private static Color
			_REFLECTION_OPACITY_COLOR =
				new Color(255, 255, 255,
					255 / 4); // who knows the real value, just a guesstimate. 'alpha' in the wz states 255 

		public DXObject(int x, int y, Texture2D texture, int delay = 0) {
			_x = x;
			_y = y;
			this.texture = texture;

			this.delay = delay;
		}

		public DXObject(PointF point, Texture2D texture, int delay = 0) {
			_x = (int) point.X;
			_y = (int) point.Y;
			this.texture = texture;

			this.delay = delay;
		}

		/// <summary>
		/// Draw map objects
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="meshRenderer"></param>
		/// <param name="gameTime"></param>
		/// <param name="mapShiftX"></param>
		/// <param name="mapShiftY"></param>
		/// <param name="flip"></param>
		/// <param name="drawReflectionInfo">Draws a reflection of the map object below it. Null if none</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DrawObject(SpriteBatch spriteBatch,
			SkeletonMeshRenderer meshRenderer, GameTime gameTime,
			int mapShiftX, int mapShiftY, bool flip, ReflectionDrawableBoundary drawReflectionInfo) {
			var drawX = X - mapShiftX;
			var drawY = Y - mapShiftY;

			spriteBatch.Draw(texture,
				new Rectangle(drawX, drawY, texture.Width, texture.Height),
				null, // src rectangle
				Color.White, // color
				0f, // angle
				new Vector2(0f, 0f), // origin
				flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, // flip
				0f // layer depth
			);

			if (drawReflectionInfo != null && drawReflectionInfo.Reflection) {
				const float reflectionAngle = 0f; // using flip instead of angle
				// TODO gradient in an optimized way.. hm

				spriteBatch.Draw(texture,
					new Rectangle(drawX, drawY, texture.Width, texture.Height),
					null, // src rectangle
					_REFLECTION_OPACITY_COLOR,
					reflectionAngle,
					new Vector2(0, -texture.Height), // origin
					flip
						? SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically
						: SpriteEffects.None | SpriteEffects.FlipVertically,
					0f // layer depth
				);
			}
		}

		/// <summary>
		/// Draw background
		/// </summary>
		/// <param name="sprite"></param>
		/// <param name="meshRenderer"></param>
		/// <param name="gameTime"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="color"></param>
		/// <param name="flip"></param>
		/// <param name="drawReflectionInfo">Draws a reflection of the map object below it. Null if none</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DrawBackground(SpriteBatch sprite,
			SkeletonMeshRenderer meshRenderer, GameTime gameTime,
			int x, int y, Color color, bool flip, ReflectionDrawableBoundary drawReflectionInfo) {
			sprite.Draw(texture, new
					Rectangle(x, y, texture.Width, texture.Height),
				null, color, 0f, new Vector2(0f, 0f), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
		}

		public int Delay => delay;

		public int X => _x;

		public int Y => _y;

		public int Width => texture.Width;

		public int Height => texture.Height;

		public object Tag {
			get => _Tag;
			set => _Tag = value;
		}
	}
}