/* Copyright (C) 2020 LastBattle
 * https://github.com/lastbattle/Harepacker-resurrected
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Globalization;
using System.Windows.Data;

namespace HaRepacker.Converter {
	/// <summary>
	/// Converts a ulong integer to a string seperated by commas
	/// </summary>
	public class IntegerULongCommasConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture) {
			var value_ = (ulong) value;

			return value_.ToString("N0");
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) {
			return 0;
		}
	}
}