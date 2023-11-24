﻿using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCreator.MapSimulator.MapObjects.UIObject {
	public interface IUIObjectEvents {
		void CheckMouseEvent(int shiftCenteredX, int shiftCenteredY, MouseState mouseState);
	}
}