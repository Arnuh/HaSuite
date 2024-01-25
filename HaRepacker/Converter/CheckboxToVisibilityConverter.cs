/* Copyright (C) 2020 LastBattle
 * https://github.com/lastbattle/Harepacker-resurrected
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Windows;
using System.Windows.Data;

namespace HaRepacker.Converter {
	/// <summary>
	/// Converts CheckBox IsChecked to Visiblity
	/// </summary>
	public class CheckboxToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture) {
			var isChecked = (bool?) value;

			if (isChecked == true) return Visibility.Visible;

			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture) {
			return false; // faek value
		}
	}
}