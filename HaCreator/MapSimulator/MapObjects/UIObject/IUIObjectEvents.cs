using Microsoft.Xna.Framework.Input;

namespace HaCreator.MapSimulator.MapObjects.UIObject {
	public interface IUIObjectEvents {
		void CheckMouseEvent(int shiftCenteredX, int shiftCenteredY, MouseState mouseState);
	}
}