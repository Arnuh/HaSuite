﻿using HaSharedLibrary.Render;
using HaSharedLibrary.Render.DX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCreator.MapSimulator.Objects.UIObject {
	/// <summary>
	/// For cursor
	/// </summary>
	public class MouseCursorItem : BaseDXDrawableItem {
		private MouseState previousMouseState;

		public MouseState MouseState {
			get => previousMouseState;
			private set { }
		}

		private int mouseCursorItemStates; // enum

		private BaseDXDrawableItem cursorItemPressedState; // default state of the cursor = this instance


		public MouseCursorItem(List<IDXObject> frames, BaseDXDrawableItem cursorItemPressedState)
			: base(frames, false) {
			previousMouseState = Mouse.GetState();

			mouseCursorItemStates = (int) MouseCursorItemStates.Normal;
			this.cursorItemPressedState = cursorItemPressedState;
		}

		public void UpdateCursorState() {
			var newSetState = (int) MouseCursorItemStates.Normal; // 0

			// Left buttpn
			if (previousMouseState.LeftButton == ButtonState.Released &&
			    Mouse.GetState().LeftButton == ButtonState.Pressed) // Left click
			{
			} else if (Mouse.GetState().LeftButton == ButtonState.Pressed) {
				newSetState |= (int) MouseCursorItemStates.LeftPress;
			}

			// Right button
			if (previousMouseState.RightButton == ButtonState.Released &&
			    Mouse.GetState().RightButton == ButtonState.Pressed) // Right click
			{
			} else if (Mouse.GetState().RightButton == ButtonState.Pressed) {
				newSetState |= (int) MouseCursorItemStates.RightPress;
			}

			mouseCursorItemStates = newSetState;
			previousMouseState = Mouse.GetState(); // save
		}

		/// <summary>
		/// Draw 
		/// </summary>
		/// <param name="sprite"></param>
		/// <param name="skeletonMeshRenderer"></param>
		/// <param name="gameTime"></param>
		/// <param name="mapShiftX"></param>
		/// <param name="mapShiftY"></param>
		/// <param name="centerX"></param>
		/// <param name="centerY"></param>
		/// <param name="renderWidth"></param>
		/// <param name="renderHeight"></param>
		/// <param name="RenderObjectScaling"></param>
		/// <param name="mapRenderResolution"></param>
		/// <param name="TickCount"></param>
		public override void Draw(SpriteBatch sprite, SkeletonMeshRenderer skeletonMeshRenderer, GameTime gameTime,
			int mapShiftX, int mapShiftY, int centerX, int centerY,
			ReflectionDrawableBoundary drawReflectionInfo,
			int renderWidth, int renderHeight, float RenderObjectScaling, RenderResolution mapRenderResolution,
			int TickCount) {
			var MousePos = Mouse.GetState().Position; // relative to the window already

			if ((mouseCursorItemStates & (int) MouseCursorItemStates.LeftPress) != (int) MouseCursorItemStates.LeftPress
			    && (mouseCursorItemStates & (int) MouseCursorItemStates.RightPress) !=
			    (int) MouseCursorItemStates.RightPress) // default
				base.Draw(sprite, skeletonMeshRenderer, gameTime,
					-MousePos.X, -MousePos.Y, centerX, centerY,
					drawReflectionInfo,
					renderWidth, renderHeight, RenderObjectScaling, mapRenderResolution,
					TickCount);
			else // if left or right press is active, draw pressed state
				cursorItemPressedState.Draw(sprite, skeletonMeshRenderer, gameTime,
					-MousePos.X, -MousePos.Y, centerX, centerY,
					drawReflectionInfo,
					renderWidth, renderHeight, RenderObjectScaling, mapRenderResolution,
					TickCount);
		}
	}

	public enum MouseCursorItemStates {
		Normal = 0x0,
		LeftPress = 0x1,
		RightPress = 0x2
	}
}