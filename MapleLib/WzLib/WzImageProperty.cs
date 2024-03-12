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
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib {
	/// <summary>
	/// An interface for wz img properties
	/// </summary>
	public abstract class WzImageProperty : WzObject {
		#region Virtual\Abstrcat Members

		public virtual List<WzImageProperty> WzProperties => null;

		public new virtual WzImageProperty this[string name] {
			get => null;
			set => throw new NotImplementedException();
		}

		public virtual WzImageProperty GetFromPath(string path) {
			return null;
		}

		public abstract WzPropertyType PropertyType { get; }

		/// <summary>
		/// The image that this property is contained in
		/// </summary>
		public WzImage ParentImage {
			get {
				var parent = Parent;
				while (parent != null) {
					if (parent is WzImage image) {
						return image;
					}

					parent = parent.Parent;
				}

				return null;
			}
		}

		public override WzObjectType ObjectType => WzObjectType.Property;

		public abstract void WriteValue(WzBinaryWriter writer);

		public abstract WzImageProperty DeepClone();

		public abstract void SetValue(object value);

		public override void Remove() {
			((IPropertyContainer) Parent).RemoveProperty(this);
		}

		public virtual void ExportXml(StreamWriter writer, int level) {
			writer.WriteLine(XmlUtil.Indentation(level) +
			                 XmlUtil.OpenNamedTag(PropertyType.ToString(), Name, true));
			writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.CloseTag(PropertyType.ToString()));
		}

		public override WzFile WzFileParent => ParentImage.WzFileParent;

		#endregion

		#region Extended Properties Parsing

		internal static void WritePropertyList(WzBinaryWriter writer, List<WzImageProperty> properties) {
			if (properties.Count == 1 && properties[0] is WzLuaProperty) {
				properties[0].WriteValue(writer);
			} else {
				writer.Write((ushort) 0);
				writer.WriteCompressedInt(properties.Count);
				foreach (var imgProperty in properties) {
					writer.WriteStringValue(imgProperty.Name, 0x00, 0x01);
					if (imgProperty is WzExtended extended) {
						WriteExtendedValue(writer, extended);
					} else {
						imgProperty.WriteValue(writer);
					}
				}
			}
		}

		internal static void DumpPropertyList(StreamWriter writer, int level, List<WzImageProperty> properties) {
			foreach (var prop in properties) prop.ExportXml(writer, level + 1);
		}

		/// <summary>
		/// Parses .lua property
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="reader"></param>
		/// <param name="parent"></param>
		/// <param name="parentImg"></param>
		/// <returns></returns>
		internal static WzLuaProperty ParseLuaProperty(uint offset, WzBinaryReader reader, WzObject parent,
			WzImage parentImg) {
			// 28 71 4F EF 1B 65 F9 1F A7 48 8D 11 73 E7 F0 27 55 09 DD 3C 07 32 D7 38 21 57 84 70 C1 79 9A 3F 49 F7 79 03 41 F4 9D B9 1B 5F CF 26 80 3D EC 25 5F 9C 
			// [compressed int] [bytes]
			var length = reader.ReadCompressedInt();
			var rawEncBytes = reader.ReadBytes(length);

			var lua = new WzLuaProperty("Script", rawEncBytes) {Parent = parent};
			return lua;
		}

		internal static List<WzImageProperty> ParsePropertyList(uint offset, WzBinaryReader reader, WzObject parent,
			WzImage parentImg) {
			var entryCount = reader.ReadCompressedInt();
			var properties = new List<WzImageProperty>(entryCount);
			for (var i = 0; i < entryCount; i++) {
				var name = reader.ReadStringBlock(offset);
				var ptype = reader.ReadByte();
				switch (ptype) // header value
				{
					case 0:
						properties.Add(new WzNullProperty(name) {Parent = parent});
						break;
					case 11:
					case 2:
						properties.Add(new WzShortProperty(name, reader.ReadInt16()) {Parent = parent});
						break;
					case 3:
					case 19:
						properties.Add(new WzIntProperty(name, reader.ReadCompressedInt()) {Parent = parent});
						break;
					case 20:
						properties.Add(new WzLongProperty(name, reader.ReadLong()) {Parent = parent});
						break;
					case 4:
						var type = reader.ReadByte();
						if (type == 0x80) {
							properties.Add(new WzFloatProperty(name, reader.ReadSingle()) {Parent = parent});
						} else if (type == 0) {
							properties.Add(new WzFloatProperty(name, 0f) {Parent = parent});
						}

						break;
					case 5:
						properties.Add(new WzDoubleProperty(name, reader.ReadDouble()) {Parent = parent});
						break;
					case 8:
						properties.Add(new WzStringProperty(name, reader.ReadStringBlock(offset)) {Parent = parent});
						break;
					case 9:
						var eob = (int) (reader.ReadUInt32() + reader.BaseStream.Position);
						WzImageProperty exProp = ParseExtendedProp(reader, offset, eob, name, parent, parentImg);
						properties.Add(exProp);
						if (reader.BaseStream.Position != eob) reader.BaseStream.Position = eob;

						break;
					default:
						throw new Exception("Unknown property type at ParsePropertyList, ptype = " + ptype);
				}
			}

			return properties;
		}

		internal static WzExtended ParseExtendedProp(WzBinaryReader reader, uint offset, int endOfBlock, string name,
			WzObject parent, WzImage imgParent) {
			switch (reader.ReadByte()) {
				case 0x01:
				case WzImage.WzImageHeaderByte_WithOffset:
					return ExtractMore(reader, offset, endOfBlock, name,
						reader.ReadStringAtOffset(offset + reader.ReadInt32()), parent, imgParent);
				case 0x00:
				case WzImage.WzImageHeaderByte_WithoutOffset:
					return ExtractMore(reader, offset, endOfBlock, name, "", parent, imgParent);
				default:
					throw new Exception("Invalid byte read at ParseExtendedProp");
			}
		}

		internal static WzExtended ExtractMore(WzBinaryReader reader, uint offset, int eob, string name, string iname,
			WzObject parent, WzImage imgParent) {
			if (iname == "") iname = reader.ReadString();

			switch (iname) {
				case "Property":
					var subProp = new WzSubProperty(name) {Parent = parent};
					reader.BaseStream.Position += 2; // Reserved?
					subProp.AddProperties(ParsePropertyList(offset, reader, subProp, imgParent));
					return subProp;
				case "Canvas":
					var canvasProp = new WzCanvasProperty(name) {Parent = parent};
					reader.BaseStream.Position++;
					if (reader.ReadByte() == 1) {
						reader.BaseStream.Position += 2;
						canvasProp.AddProperties(
							ParsePropertyList(offset, reader, canvasProp, imgParent));
					}

					canvasProp.PngProperty = new WzPngProperty(reader, imgParent.ParseEverything) {Parent = canvasProp};
					if (imgParent.ParseEverything) {
						canvasProp.PngProperty.CheckListWzUsed(canvasProp.PngProperty.GetCompressedBytes(true));
					}

					return canvasProp;
				case "Shape2D#Vector2D":
					var vecProp = new WzVectorProperty(name) {Parent = parent};
					vecProp.X = new WzIntProperty("X", reader.ReadCompressedInt()) {Parent = vecProp};
					vecProp.Y = new WzIntProperty("Y", reader.ReadCompressedInt()) {Parent = vecProp};
					return vecProp;
				case "Shape2D#Convex2D":
					var convexProp = new WzConvexProperty(name) {Parent = parent};
					var convexEntryCount = reader.ReadCompressedInt();
					convexProp.WzProperties.Capacity = convexEntryCount;
					for (var i = 0; i < convexEntryCount; i++)
						convexProp.AddProperty(ParseExtendedProp(reader, offset, 0, name, convexProp, imgParent));

					return convexProp;
				case "Sound_DX8":
					var soundProp = new WzSoundProperty(name, reader, imgParent.ParseEverything)
						{Parent = parent};
					return soundProp;
				case "UOL":
					reader.BaseStream.Position++;
					switch (reader.ReadByte()) {
						case 0:
							return new WzUOLProperty(name, reader.ReadString()) {Parent = parent};
						case 1:
							return new WzUOLProperty(name, reader.ReadStringAtOffset(offset + reader.ReadInt32()))
								{Parent = parent};
					}

					throw new Exception("Unsupported UOL type");
				default:
					throw new Exception("Unknown iname: " + iname);
			}
		}

		internal static void WriteExtendedValue(WzBinaryWriter writer, WzExtended property) {
			writer.Write((byte) 9);

			var beforePos = writer.BaseStream.Position;
			writer.Write(0); // Placeholder
			property.WriteValue(writer);

			var len = (int) (writer.BaseStream.Position - beforePos);
			var newPos = writer.BaseStream.Position;
			writer.BaseStream.Position = beforePos;
			writer.Write(len - 4);
			writer.BaseStream.Position = newPos;
		}

		#endregion

		#region Custom Members

		/// <summary>
		/// Gets the linked WzImageProperty via WzUOLProperty
		/// </summary>
		/// <returns></returns>
		public WzImageProperty GetLinkedWzImageProperty() {
			var thisWzImage = this;
			while (thisWzImage is WzUOLProperty) {
				if ((thisWzImage as WzUOLProperty).LinkValue is WzImageProperty newWzImage) {
					thisWzImage = newWzImage;
				} else // broken link
				{
					return this;
				}
			}

			return thisWzImage;
		}

		#endregion
	}
}