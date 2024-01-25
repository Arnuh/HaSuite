/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace HaCreator.MapEditor.UndoRedo {
	public enum UndoRedoType {
		ItemDeleted,
		ItemAdded,
		ItemMoved,
		ItemFlipped,
		LineRemoved,
		LineAdded,
		FootholdFlipped,
		ToolTipLinked,
		ToolTipUnlinked,
		BackgroundMoved,
		ItemsUnlinked,
		ItemsLinked,
		ItemsLayerChanged,
		ItemLayerPlatChanged,
		RopeRemoved,
		RopeAdded,
		ItemZChanged,
		VRChanged,
		MapCenterChanged,
		LayerTSChanged,
		zMChanged
	}
}