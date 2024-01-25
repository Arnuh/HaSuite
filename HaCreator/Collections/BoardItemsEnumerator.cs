/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections;
using System.Collections.Generic;
using HaCreator.MapEditor;

namespace HaCreator.Collections {
	public class BoardItemsEnumerator : ItemsEnumeratorBase, IEnumerator<BoardItem> {
		public BoardItemsEnumerator(BoardItemsCollection bic) : base(bic) {
		}

		public BoardItem Current => (BoardItem) CurrentObject;

		object IEnumerator.Current => CurrentObject;
	}
}