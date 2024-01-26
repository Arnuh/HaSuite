using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spine;

namespace HaSharedLibrary.Render.DX {
	public interface IDXObject {
		void DrawObject(SpriteBatch sprite, SkeletonMeshRenderer meshRenderer,
			GameTime gameTime,
			int mapShiftX, int mapShiftY, bool flip, ReflectionDrawableBoundary drawReflectionInfo);

		void DrawBackground(SpriteBatch sprite, SkeletonMeshRenderer meshRenderer,
			GameTime gameTime,
			int x, int y, Color color, bool flip, ReflectionDrawableBoundary drawReflectionInfo);

		/// <summary>
		/// The delay of the current frame
		/// </summary>
		int Delay { get; }

		/// <summary>
		/// The origin X
		/// </summary>
		int X { get; }

		/// <summary>
		/// The origin Y
		/// </summary>
		int Y { get; }

		/// <summary>
		/// The Width of the Texture2D
		/// </summary>
		int Width { get; }

		/// <summary>
		/// The height of the Texture2D
		/// </summary>
		int Height { get; }

		/// <summary>
		/// Tag - For storing anything
		/// </summary>
		object Tag { get; set; }
	}
}