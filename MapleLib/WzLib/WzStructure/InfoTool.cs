/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2009, 2010, 2015 Snow and haha01haha01

 * This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using System;
using System.Drawing;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib.WzStructure {
	public static class InfoTool {
		#region String

		public static string GetString(this WzImageProperty source) {
			return source.GetString();
		}

		public static WzStringProperty SetString(string value) {
			return new WzStringProperty("", value);
		}

		public static string GetOptionalString(this WzImageProperty source, string def) {
			return source?.GetString() ?? def;
		}

		public static WzStringProperty SetOptionalString(string value, string def) {
			return value != def ? SetString(value) : null;
		}

		#endregion


		#region Double

		public static double GetDouble(this WzImageProperty source) {
			return source.GetDouble();
		}

		public static WzDoubleProperty SetDouble(double value) {
			return new WzDoubleProperty("", value);
		}

		public static double GetOptionalDouble(this WzImageProperty source, double def) {
			return source?.GetDouble() ?? def;
		}

		public static WzDoubleProperty SetOptionalDouble(this double value, double def) {
			return Math.Abs(value - def) > double.Epsilon ? SetDouble(value) : null;
		}

		#endregion


		#region Integer

		public static int GetInt(this WzImageProperty source) {
			return source.GetInt();
		}

		public static WzIntProperty SetInt(int value) {
			return new WzIntProperty("", value);
		}

		public static int GetOptionalInt(this WzImageProperty source, int def) {
			return source?.GetInt() ?? def;
		}

		public static WzIntProperty SetOptionalInt(this int value, int def) {
			return value != def ? SetInt(value) : null;
		}

		#endregion

		#region Rectangle

		/// <summary>
		/// Gets System.Drawing.Rectangle from "lt" and "rb"
		/// </summary>
		/// <param name="parentSource"></param>
		/// <returns></returns>
		public static Rectangle GetLtRbRectangle(this WzImageProperty parentSource) {
			var lt = GetOptionalVector(parentSource["lt"]);
			var rb = GetOptionalVector(parentSource["rb"]);

			var width = rb.X.Value - lt.X.Value;
			var height = rb.Y.Value - lt.Y.Value;

			var rectangle = new Rectangle(
				lt.X.Value,
				lt.Y.Value,
				width,
				height);
			return rectangle;
		}

		/// <summary>
		/// Sets the "lt" and "rb" value in a WzImageProperty parentSource
		/// derived from Rectangle
		/// </summary>
		/// <param name="parentSource"></param>
		/// <param name="rect"></param>
		public static void SetLtRbRectangle(this WzImageProperty parentSource, Rectangle rect) {
			parentSource["lt"] = SetVector(rect.X, rect.Y);
			parentSource["rb"] = SetVector(rect.X + rect.Width, rect.Y + rect.Height);
		}

		#endregion

		#region Vector

		/// <summary>
		/// Gets the vector value of the WzImageProperty
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static WzVectorProperty GetVector(this WzImageProperty source) {
			return (WzVectorProperty) source;
		}

		/// <summary>
		/// Sets vector
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static WzVectorProperty SetVector(float x, float y) {
			return new WzVectorProperty("", x, y);
		}


		/// <summary>
		/// Gets an optional Vector. 
		/// Returns x = 0, and y = 0 if the WzImageProperty is not found.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static WzVectorProperty GetOptionalVector(this WzImageProperty source) {
			if (source == null) {
				return new WzVectorProperty(string.Empty, 0, 0);
			}

			return GetVector(source);
		}

		#endregion

		#region Long

		public static long GetLong(this WzImageProperty source) {
			return source.GetLong();
		}

		public static WzLongProperty SetLong(long value) {
			return new WzLongProperty("", value);
		}

		public static long? GetOptionalLong(this WzImageProperty source, long def) {
			return source?.GetLong() ?? def;
		}

		public static WzLongProperty SetOptionalLong(this long value, long def) {
			return value != def ? SetLong(value) : null;
		}

		#endregion

		#region Boolean

		public static bool GetBool(this WzImageProperty source) {
			return source.GetInt() != 0;
		}

		public static WzIntProperty SetBool(bool value) {
			return new WzIntProperty("", value ? 1 : 0);
		}

		public static bool GetOptionalBool(this WzImageProperty source, bool def) {
			if (source == null) return def;
			return source.GetInt() != 0;
		}

		public static WzIntProperty SetOptionalBool(this bool value, bool def) {
			return value != def ? SetBool(value) : null;
		}

		#endregion


		#region Float

		public static float GetFloat(this WzImageProperty source) {
			return source.GetFloat();
		}

		public static WzFloatProperty SetFloat(float value) {
			return new WzFloatProperty("", value);
		}

		public static float? GetOptionalFloat(this WzImageProperty source, float def) {
			return source?.GetFloat() ?? def;
		}

		public static WzFloatProperty SetOptionalFloat(float value, float def) {
			return Math.Abs(value - def) > float.Epsilon ? SetFloat(value) : null;
		}

		#endregion
	}
}