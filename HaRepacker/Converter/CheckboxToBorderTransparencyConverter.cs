/* Copyright (C) 2020 LastBattle
 * https://github.com/lastbattle/Harepacker-resurrected
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HaRepacker.Converter {
	/// <summary>
	/// Converts CheckBox IsChecked to Visiblity (Transparency)
	/// </summary>
	public class CheckboxToBorderTransparencyConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture) {
			var isChecked = (bool?) value;

			if (isChecked == true) return new SolidColorBrush(Colors.Gray);

			return new SolidColorBrush(Colors.Transparent);
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) {
			return false; // faek value
		}
	}
}