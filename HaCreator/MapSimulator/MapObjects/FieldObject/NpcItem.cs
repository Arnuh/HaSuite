using System.Collections.Generic;
using HaCreator.MapEditor.Instance;
using HaSharedLibrary.Render;
using HaSharedLibrary.Render.DX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using spine_2._1._25_netcore.spine_xna;

namespace HaCreator.MapSimulator.Objects.FieldObject {
	public class NpcItem : BaseDXDrawableItem {
		private readonly NpcInstance npcInstance;

		public NpcInstance NpcInstance {
			get => npcInstance;
			private set { }
		}

		public NpcItem(NpcInstance npcInstance, List<IDXObject> frames)
			: base(frames, npcInstance.Flip) {
			this.npcInstance = npcInstance;
		}


		public NpcItem(NpcInstance npcInstance, IDXObject frame0)
			: base(frame0, npcInstance.Flip) {
			this.npcInstance = npcInstance;
		}

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