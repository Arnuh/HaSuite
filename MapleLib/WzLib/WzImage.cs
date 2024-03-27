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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapleLib.Helpers;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib {
	/// <summary>
	/// A .img contained in a wz directory
	/// </summary>
	public class WzImage : WzObject, IPropertyContainer {
		//TODO: nest wzproperties in a wzsubproperty inside of WzImage

		/// <summary>
		/// bExistID_0x73
		/// </summary>
		public const int WzImageHeaderByte_WithoutOffset = 0x73;

		/// <summary>
		/// bNewID_0x1b
		/// </summary>
		public const int WzImageHeaderByte_WithOffset = 0x1B;

		#region Fields

		internal bool parsed;
		internal int size;
		private int checksum;
		internal uint offset;
		internal WzBinaryReader reader;
		internal List<WzImageProperty> properties = new List<WzImageProperty>();
		internal WzObject parent;
		internal int blockStart;
		internal long tempFileStart = 0;
		internal long tempFileEnd = 0;
		internal bool bIsImageChanged;
		private bool parseEverything;
		public WzMutableKey wzKey { get; set; }

		/// <summary>
		/// Wz image embedding .lua file.
		/// </summary>
		public bool IsLuaWzImage =>
			Name.EndsWith(".lua"); // TODO: find some ways to avoid user from adding a new image with .lua name

		#endregion

		#region Constructors\Destructors

		/// <summary>
		/// Creates a blank WzImage
		/// </summary>
		public WzImage() {
		}

		/// <summary>
		/// Creates a WzImage with the given name
		/// </summary>
		/// <param name="name">The name of the image</param>
		public WzImage(string name) {
			this.name = name;
		}

		public WzImage(string name, WzMapleVersion mapleVersion) {
			this.name = name;
			wzKey = WzKeyGenerator.GenerateWzKey(WzTool.GetIvByMapleVersion(mapleVersion), WzTool.GetUserKeyByMapleVersion(mapleVersion));
		}

		public WzImage(string name, byte[] iv, byte[] UserKey) {
			this.name = name;
			wzKey = WzKeyGenerator.GenerateWzKey(iv, UserKey);
		}

		public WzImage(string name, Stream dataStream, WzMapleVersion mapleVersion) {
			this.name = name;
			reader = new WzBinaryReader(dataStream, WzTool.GetIvByMapleVersion(mapleVersion), WzTool.GetUserKeyByMapleVersion(mapleVersion));
			wzKey = reader.WzKey;
		}

		internal WzImage(string name, WzBinaryReader reader) {
			this.name = name;
			this.reader = reader;
			wzKey = reader.WzKey;
			blockStart = (int) reader.BaseStream.Position;
			checksum = 0;
		}

		/// <summary>
		/// WzImage Constructor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="reader"></param>
		/// <param name="checksum"></param>
		/// <param name="unk_GMS230"></param>
		internal WzImage(string name, WzBinaryReader reader, int checksum) {
			this.name = name;
			this.reader = reader;
			wzKey = reader.WzKey;
			blockStart = (int) reader.BaseStream.Position;
			this.checksum = checksum;
		}

		public override void Dispose() {
			name = null;
			if (properties != null) {
				foreach (var prop in properties) prop.Dispose();

				properties.Clear();
				properties = null;
			}

			if (reader != null) {
				reader.Close();
				reader.Dispose();
				reader = null;
			}
		}

		#endregion

		#region Inherited Members

		/// <summary>
		/// The parent of the object
		/// </summary>
		public override WzObject Parent {
			get => parent;
			internal set => parent = value;
		}

		public override WzFile WzFileParent => Parent?.WzFileParent;

		/// <summary>
		/// Is the object parsed
		/// </summary>
		public bool Parsed {
			get => parsed;
			set => parsed = value;
		}

		/// <summary>
		/// Set the property if the image should be fully parsed
		/// </summary>
		public bool ParseEverything {
			get => parseEverything;
			set => parseEverything = value;
		}

		/// <summary>
		/// Was the image changed
		/// </summary>
		public bool Changed {
			get => bIsImageChanged;
			set => bIsImageChanged = value;
		}

		/// <summary>
		/// The size in the wz file of the image
		/// </summary>
		public int BlockSize {
			get => size;
			set => size = value;
		}

		/// <summary>
		/// The checksum of the image
		/// </summary>
		public int Checksum {
			get => checksum;
			private set { }
		}

		/// <summary>
		/// The offset of the start of this image
		/// </summary>
		public uint Offset {
			get => offset;
			set => offset = value;
		}

		public int BlockStart => blockStart;

		/// <summary>
		/// The WzObjectType of the image
		/// </summary>
		public override WzObjectType ObjectType {
			get {
				if (reader != null) {
					if (!parsed) {
						ParseImage();
					}
				}

				return WzObjectType.Image;
			}
		}

		/// <summary>
		/// The properties contained in the image
		/// </summary>
		public List<WzImageProperty> WzProperties {
			get {
				if (reader != null && !parsed) ParseImage();

				return properties;
			}
		}

		public WzImage DeepClone() {
			if (reader != null && !parsed) ParseImage();
			var clone = new WzImage(name, wzKey.CopyIv(), wzKey.CopyUserKey()) {
				bIsImageChanged = true
			};
			foreach (var prop in properties)
				clone.AddProperty(prop.DeepClone(), false);
			return clone;
		}

		/// <summary>
		/// Gets a wz property by it's name
		/// </summary>
		/// <param name="name">The name of the property</param>
		/// <returns>The wz property with the specified name</returns>
		public new WzImageProperty this[string name] {
			get {
				if (reader != null) {
					if (!parsed) {
						ParseImage();
					}
				}

				// Find the first WzImageProperty with a matching name (case-insensitive)
				return properties.FirstOrDefault(iwp => iwp.Name.ToLower() == name.ToLower());
			}
			set {
				if (value != null) {
					value.Name = name;
					AddProperty(value);
				}
			}
		}

		#endregion

		#region Custom Members

		/// <summary>
		/// Gets a WzImageProperty from a path
		/// </summary>
		/// <param name="path">path to object</param>
		/// <returns>the selected WzImageProperty</returns>
		public WzImageProperty GetFromPath(string path) {
			if (reader != null) {
				if (!parsed) {
					ParseImage();
				}
			}

			var segments = path.Split(new char[1] {'/'}, StringSplitOptions.RemoveEmptyEntries);

			// If the first segment is "..", return null
			if (segments[0] == "..") {
				return null;
			}

			WzImageProperty ret = null;

			foreach (var segment in segments) {
				// Check if the current property has a child with the matching name
				ret = (ret == null ? properties : ret.WzProperties)
					.FirstOrDefault(iwp => iwp.Name == segment);

				// If no matching child was found, return null
				if (ret == null) {
					return null;
				}
			}

			return ret;
		}

		/// <summary>
		/// Adds a property to the WzImage
		/// </summary>
		/// <param name="prop">Property to add</param>
		public void AddProperty(WzImageProperty prop, bool checkListWz = true) {
			prop.Parent = this;
			if (reader != null && !parsed) {
				ParseImage();
			}

			properties.Add(prop);
			if (!checkListWz || !parsed) {
				return;
			}

			var wzFile = WzFileParent;
			if (wzFile == null) return;
			ListWzContainerImpl.MarkListWzProperty(this, wzFile);
		}

		/// <summary>
		/// Add a list of properties to the WzImage
		/// </summary>
		/// <param name="props"></param>
		public void AddProperties(List<WzImageProperty> props) {
			foreach (var prop in props) AddProperty(prop);
		}

		/// <summary>
		/// Removes a property by name
		/// </summary>
		/// <param name="name">The name of the property to remove</param>
		public void RemoveProperty(WzImageProperty prop) {
			if (reader != null && !parsed) {
				ParseImage();
			}

			prop.Parent = null;
			properties.Remove(prop);
		}

		public void ClearProperties() {
			foreach (var prop in properties)
				prop.Parent = null;
			properties.Clear();
		}

		public override void Remove() {
			if (Parent != null) ((WzDirectory) Parent).RemoveImage(this);
		}

		#endregion

		#region Parsing Methods

		/// <summary>
		/// Calculates and set the image header checksum
		/// </summary>
		/// <param name="memStream"></param>
		internal void CalculateAndSetImageChecksum(byte[] bytes) {
			checksum = 0;
			foreach (var b in bytes) checksum += b;
		}

		/// <summary>
		/// Parses the image from the wz filetod
		/// </summary>
		/// <param name="wzReader">The BinaryReader that is currently reading the wz file</param>
		/// <returns>bool Parse status</returns>
		public bool ParseImage(bool forceReadFromData = false) {
			if (!forceReadFromData) { // only check if parsed or changed if its not false read
				if (Parsed) {
					return true;
				}

				if (Changed) {
					Parsed = true;
					return true;
				}
			}

			lock (reader) // for multi threaded XMLWZ export. 
			{
				var originalPos = reader.BaseStream.Position;
				reader.BaseStream.Position = offset;

				var b = reader.ReadByte();
				switch (b) {
					case 0x1: // .lua   
					{
						if (IsLuaWzImage) {
							var lua = WzImageProperty.ParseLuaProperty(offset, reader, this, this);
							var luaImage = new List<WzImageProperty> {
								lua
							};
							properties.AddRange(luaImage);
							parsed = true; // test
							return true;
						}

						return false; // unhandled for now, if it isnt an .lua image
					}
					case WzImageHeaderByte_WithoutOffset: {
						var prop = reader.ReadString();
						var val = reader.ReadUInt16();
						if (prop != "Property" || val != 0) return false;

						break;
					}
					default: {
						// todo: log this or warn.
						ErrorLogger.Log(ErrorLevel.MissingFeature,
							"[WzImage] New Wz image header found. b = " + b);
						return false;
					}
				}

				var images = WzImageProperty.ParsePropertyList(offset, reader, this, this);
				properties.AddRange(images);

				parsed = true;
			}

			return true;
		}

		/// <summary>
		/// Marks this WzImage as parsed to avoid loading from file once again
		/// This function will be used exclusively for creating new Data.wz file for now :) 
		/// </summary>
		public void MarkWzImageAsParsed() {
			Parsed = true;
		}

		public byte[] DataBlock {
			get {
				byte[] blockData = null;
				if (reader != null && size > 0) {
					blockData = reader.ReadBytes(size);
					reader.BaseStream.Position = blockStart;
				}

				return blockData;
			}
		}

		public void UnparseImage() {
			parsed = false;
			properties.Clear();
			properties = new List<WzImageProperty>();
		}

		/// <summary>
		/// Writes the WzImage object to the underlying WzBinaryWriter
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="isWzUserKeyDefault">Uses the default MapleStory UserKey or a custom key.</param>
		/// <param name="forceReadFromData">Read from data regardless of base data that's changed or not.</param>
		public void SaveImage(WzBinaryWriter writer, bool isWzIvSimilar = true, bool isWzUserKeyDefault = true, bool forceReadFromData = false) {
			if (bIsImageChanged ||
			    !isWzUserKeyDefault || //  everything needs to be re-written when a custom UserKey is used
			    forceReadFromData) { // if its not being force-read and written, it saves with the previous WZ encryption IV.
				if (reader != null && !parsed) {
					ParseEverything = true;
					ParseImage(forceReadFromData);
				}

				var imgProp = new WzSubProperty();

				var startPos = writer.BaseStream.Position;
				imgProp.AddPropertiesForWzImageDumping(WzProperties);
				if (!isWzIvSimilar) {
					ListWzContainerImpl.ConvertKey(imgProp, wzKey, writer.WzKey);
				}
				imgProp.WriteValue(writer);

				writer.StringCache.Clear();

				size = (int) (writer.BaseStream.Position - startPos);
			} else {
				var pos = reader.BaseStream.Position;
				reader.BaseStream.Position = offset;
				writer.Write(reader.ReadBytes(size));

				reader.BaseStream.Position = pos; // reset
			}
		}

		public void ExportXml(StreamWriter writer, bool oneFile, int level) {
			if (oneFile) {
				writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzImage", name, true));
				WzImageProperty.DumpPropertyList(writer, level, WzProperties);
				writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.CloseTag("WzImage"));
			} else {
				throw new Exception("Under Construction");
			}
		}

		#endregion

		#region Overrides

		public override string ToString() {
			var loggerSuffix = string.Format("WzImage: '{0}' {1}", Name,
				WzFileParent != null
					? ", ver. " + Enum.GetName(typeof(WzMapleVersion), WzFileParent.MapleVersion) + ", v" +
					  WzFileParent.Version
					: "");

			return loggerSuffix;
		}

		#endregion
	}
}