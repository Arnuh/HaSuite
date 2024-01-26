/* Copyright (C) 2020 LastBattle
 * https://github.com/lastbattle/Harepacker-resurrected
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */


using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace HaRepacker.Converter {
	/// <summary>
	/// Converts PointF X and Y coordinates to homosapien-ape readable string
	/// </summary>
	public class PointFOriginToStringConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture) {
			var point = (PointF) value;

			return string.Format("X {0}, Y {1}", point.X, point.Y);
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) {
			return new PointF(0, 0); // anyway wtf
		}
	}
}