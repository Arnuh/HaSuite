/* Copyright (C) 2020 LastBattle
 * https://github.com/lastbattle/Harepacker-resurrected
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using HaRepacker.Utils;

namespace HaRepacker.Converter {
	/// <summary>
	/// Converts PointF vector origin to XAML Margin
	/// </summary>
	public class VectorOriginPointFToMarginConverter : IValueConverter {
		private readonly float fCrossHairWidthHeight = 10f / 2f;

		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture) {
			if (value == null) return new Thickness(0, 0, 0, 0);
			var originValue = (PointF) value;

			// converted
			// its always -50, as it is 50px wide, as specified in the xaml
			var margin = new Thickness(originValue.X / ScreenDPIUtil.GetScreenScaleFactor(),
				(originValue.Y - fCrossHairWidthHeight) / ScreenDPIUtil.GetScreenScaleFactor(), 0, 0); // 20,75


			return margin;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) {
			var value_ = (Thickness) value;

			// converted
			var originValue = new PointF((float) (value_.Left * ScreenDPIUtil.GetScreenScaleFactor()),
				(float) ((value_.Top + fCrossHairWidthHeight) * ScreenDPIUtil.GetScreenScaleFactor()));
			return originValue;
		}
	}
}