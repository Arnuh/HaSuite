using System.Collections.Generic;
using HaCreator.MapEditor.Instance;
using HaSharedLibrary.Render;
using HaSharedLibrary.Render.DX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using spine_2._1._25_netcore.spine_xna;

namespace HaCreator.MapSimulator.Objects.FieldObject {
	public class ReactorItem : BaseDXDrawableItem {
		private readonly ReactorInstance reactorInstance;

		public ReactorInstance ReactorInstance {
			get => reactorInstance;
			private set { }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="reactorInstance"></param>
		/// <param name="frames"></param>
		public ReactorItem(ReactorInstance reactorInstance, List<IDXObject> frames)
			: base(frames, reactorInstance.Flip) {
			this.reactorInstance = reactorInstance;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="reactorInstance"></param>
		/// <param name="frame0"></param>
		public ReactorItem(ReactorInstance reactorInstance, IDXObject frame0)
			: base(frame0, reactorInstance.Flip) {
			this.reactorInstance = reactorInstance;
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
		/// <param name="drawReflectionInfo"></param>
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
			base.Draw(sprite, skeletonMeshRenderer, gameTime,
				mapShiftX, mapShiftY, centerX, centerY,
				drawReflectionInfo,
				renderWidth, renderHeight, RenderObjectScaling, mapRenderResolution,
				TickCount);
		}
	}
}