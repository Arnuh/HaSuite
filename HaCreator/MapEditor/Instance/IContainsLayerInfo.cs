﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace HaCreator.MapEditor.Instance {
	// The difference between LayeredItem and this is that LayeredItems are actually 
	// ordered according to their layer (tiles\objs) in the editor. IContainsLayerInfo only
	// contains info about layers, and is not necessarily drawn according to it.
	public interface IContainsLayerInfo {
		int LayerNumber { get; set; }
		int PlatformNumber { get; set; }
	}
}