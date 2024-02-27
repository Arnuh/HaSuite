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

using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties {
	/// <summary>
	/// A property with a string as a value
	/// </summary>
	public class WzStringProperty : WzImageProperty {
		#region Fields

		internal string val;

		internal WzObject parent;
		//internal WzImage imgParent;

		#endregion

		#region Inherited Members

		public override void SetValue(object value) {
			val = (string) value;
		}

		public override WzImageProperty DeepClone() {
			var clone = new WzStringProperty(name, val);
			return clone;
		}

		public override object WzValue => Value;

		/// <summary>
		/// The parent of the object
		/// </summary>
		public override WzObject Parent {
			get => parent;
			internal set => parent = value;
		}

		/// <summary>
		/// The WzPropertyType of the property
		/// </summary>
		public override WzPropertyType PropertyType => WzPropertyType.String;

		public override void WriteValue(WzBinaryWriter writer) {
			writer.Write((byte) 8);
			writer.WriteStringValue(Value, 0, 1);
		}

		public override void ExportXml(StreamWriter writer, int level) {
			writer.WriteLine(
				XmlUtil.Indentation(level) + XmlUtil.EmptyNamedValuePair("WzString", Name, Value));
		}

		/// <summary>
		/// Disposes the object
		/// </summary>
		public override void Dispose() {
			name = null;
			val = null;
		}

		#endregion

		#region Custom Members

		/// <summary>
		/// The value of the property
		/// </summary>
		public string Value {
			get => val;
			set => val = value;
		}

		/// <summary>
		/// Creates a blank WzStringProperty
		/// </summary>
		public WzStringProperty() {
		}

		/// <summary>
		/// Creates a WzStringProperty with the specified name
		/// </summary>
		/// <param name="name">The name of the property</param>
		public WzStringProperty(string name) {
			this.name = name;
		}

		/// <summary>
		/// Creates a WzStringProperty with the specified name and value
		/// </summary>
		/// <param name="name">The name of the property</param>
		/// <param name="value">The value of the property</param>
		public WzStringProperty(string name, string value) {
			this.name = name;
			val = value;
		}

		/// <summary>
		/// Spine runtime related resources
		/// p.s just assuming it should be, if its a WzStingProperty and .atlas, .skel or .json until there is a better way to detect it
		/// </summary>
		public bool IsSpineRelatedResources =>
			name.EndsWith(".atlas") || name.EndsWith(".json") || name.EndsWith(".skel");

		public bool IsSpineAtlasResources => name.EndsWith(".atlas");

		#endregion

		#region Cast Values

		public override int GetInt() {
			var outvalue = 0;
			int.TryParse(val, out outvalue);

			return outvalue; // stupid nexon . fu, some shit that should be WzIntProperty
		}

		public override short GetShort() {
			short outvalue = 0;
			short.TryParse(val, out outvalue);

			return outvalue; // stupid nexon . fu, some shit that should be WzIntProperty
		}

		public override long GetLong() {
			long outvalue = 0;
			long.TryParse(val, out outvalue);

			return outvalue; // stupid nexon . fu, some shit that should be WzIntProperty
		}

		public override string GetString() {
			return val;
		}

		public override string ToString() {
			return val;
		}

		#endregion
	}
}