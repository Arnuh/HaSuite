/* Copyright (C) 2020 LastBattle
 * https://github.com/lastbattle/Harepacker-resurrected
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */


using System;
using System.Globalization;
using System.Windows.Data;
using HaRepacker.Utils;

namespace HaRepacker.Converter {
	/// <summary>
	/// Converts the image (x, y) width or height to the correct size according to the screen's DPI scaling factor
	/// </summary>
	public class ImageWidthOrHeightToScreenDPIConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture) {
			var widthOrHeight = (int) value;
			var realWidthOrHeightToDisplay = widthOrHeight * ScreenDPIUtil.GetScreenScaleFactor();

			return realWidthOrHeightToDisplay;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) {
			var value_ = (int) value;
			var imageWidthOrHeight = value_ / ScreenDPIUtil.GetScreenScaleFactor();

			return imageWidthOrHeight;
		}
	}
}