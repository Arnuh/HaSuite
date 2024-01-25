/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Windows;
using System.Windows.Controls;

namespace HaCreator.CustomControls {
	public class CheckboxButton : CheckBox {
		protected override void OnClick() {
			if (Clicked != null) {
				Clicked.Invoke(this, new RoutedEventArgs());
			}
		}

		public event EventHandler<RoutedEventArgs> Clicked;
	}
}