﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using Microsoft.Xna.Framework.Graphics;

namespace HaCreator.MapEditor {
	public class GraphicsDeviceService : IGraphicsDeviceService {
		private GraphicsDevice device;

		public GraphicsDeviceService(GraphicsDevice device) {
			this.device = device;
			device.Disposing += device_Disposing;
			device.DeviceResetting += device_DeviceResetting;
			device.DeviceReset += device_DeviceReset;
			if (DeviceCreated != null) DeviceCreated.Invoke(device, new EventArgs());
		}

		private void device_DeviceReset(object sender, EventArgs e) {
			if (DeviceReset != null) DeviceReset.Invoke(sender, e);
		}

		private void device_DeviceResetting(object sender, EventArgs e) {
			if (DeviceResetting != null) DeviceResetting.Invoke(sender, e);
		}

		private void device_Disposing(object sender, EventArgs e) {
			if (DeviceDisposing != null) DeviceDisposing.Invoke(sender, e);
		}

		public GraphicsDevice GraphicsDevice => device;

		public event EventHandler<EventArgs> DeviceCreated;
		public event EventHandler<EventArgs> DeviceDisposing;
		public event EventHandler<EventArgs> DeviceReset;
		public event EventHandler<EventArgs> DeviceResetting;
	}
}