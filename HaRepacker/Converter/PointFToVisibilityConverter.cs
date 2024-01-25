/* Copyright (C) 2020 LastBattle
 * https://github.com/lastbattle/Harepacker-resurrected
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Data;

namespace HaRepacker.Converter {
	/// <summary>
	/// PointF to System.Windows.Visiblity converter.
	/// Returns Visiblity.Visible if the X and Y coordinates of PointF is not 0,
	/// otherwise Visiblity.Collapsed
	/// </summary>
	public class PointFToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture) {
			var point = (PointF?) value;

			return point == null ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture) {
			return new PointF(0, 0); // anyway wtf
		}
	}
}