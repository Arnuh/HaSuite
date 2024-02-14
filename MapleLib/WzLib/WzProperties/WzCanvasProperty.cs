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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties {
	/// <summary>
	/// A property that can contain sub properties and has one png image
	/// </summary>
	public class WzCanvasProperty : WzExtended, IPropertyContainer {
		#region Constants

		/// <summary>
		/// The propertyname used for inlink
		/// </summary>
		public const string InlinkPropertyName = "_inlink";

		public const string OutlinkPropertyName = "_outlink";
		public const string OriginPropertyName = "origin";
		public const string HeadPropertyName = "head";
		public const string LtPropertyName = "lt";
		public const string RbPropertyName = "rb";
		public const string AnimationDelayPropertyName = "delay";

		#endregion

		#region Fields

		internal List<WzImageProperty> properties = new List<WzImageProperty>();
		internal WzPngProperty imageProp;

		internal WzObject parent;
		//internal WzImage imgParent;

		#endregion

		#region Inherited Members

		public override void SetValue(object value) {
			imageProp.SetValue(value);
		}

		public override WzImageProperty DeepClone() {
			var clone = new WzCanvasProperty(name);
			foreach (var prop in properties) {
				clone.AddProperty(prop.DeepClone(), false);
			}

			clone.imageProp = (WzPngProperty) imageProp.DeepClone();
			clone.imageProp.Parent = this;
			return clone;
		}

		public override object WzValue => PngProperty;

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
		public override WzPropertyType PropertyType => WzPropertyType.Canvas;

		/// <summary>
		/// The properties contained in this property
		/// </summary>
		public override List<WzImageProperty> WzProperties => properties;

		/// <summary>
		/// Gets a wz property by it's name
		/// </summary>
		/// <param name="name">The name of the property</param>
		/// <returns>The wz property with the specified name</returns>
		public override WzImageProperty this[string name] {
			get {
				if (name == "PNG") {
					return imageProp;
				}

				return properties.FirstOrDefault(iwp => iwp.Name.ToLower() == name.ToLower());
			}
			set {
				if (value != null) {
					if (name == "PNG") {
						imageProp = (WzPngProperty) value;
						return;
					}

					value.Name = name;
					AddProperty(value);
				}
			}
		}

		public WzImageProperty GetProperty(string name) {
			return properties.FirstOrDefault(iwp => iwp.Name.ToLower() == name.ToLower());
		}

		/// Gets a wz property by a path name
		/// </summary>
		/// <param name="path">path to property</param>
		/// <returns>the wz property with the specified name</returns>
		public override WzImageProperty GetFromPath(string path) {
			var segments = path.Split(new char[1] {'/'}, StringSplitOptions.RemoveEmptyEntries);
			if (segments[0] == "..") return ((WzImageProperty) Parent)[path.Substring(name.IndexOf('/') + 1)];

			WzImageProperty ret = this;
			foreach (var segment in segments) {
				if (segment == "PNG") {
					return imageProp;
				}

				var iwp = ret.WzProperties.FirstOrDefault(p => p.Name == segment);
				if (iwp == null) return null;

				ret = iwp;
			}

			return ret;
		}

		public override void WriteValue(WzBinaryWriter writer) {
			writer.WriteStringValue("Canvas", WzImage.WzImageHeaderByte_WithoutOffset,
				WzImage.WzImageHeaderByte_WithOffset);
			writer.Write((byte) 0);
			if (properties.Count > 0) { // subproperty in the canvas
				writer.Write((byte) 1);
				WritePropertyList(writer, properties);
			} else {
				writer.Write((byte) 0);
			}

			// Image info
			writer.WriteCompressedInt(PngProperty.Width);
			writer.WriteCompressedInt(PngProperty.Height);
			writer.WriteCompressedInt(PngProperty.PixFormat);
			writer.WriteCompressedInt(PngProperty.MagLevel);
			writer.Write(0);

			// Write image
			var bytes = PngProperty.GetCompressedBytes(false);
			writer.Write(bytes.Length + 1);
			writer.Write((byte) 0); // header? see WzImageProperty.ParseExtendedProp "0x00"
			writer.Write(bytes);
		}

		public override void ExportXml(StreamWriter writer, int level) {
			writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzCanvas", Name, false, false) +
			                 XmlUtil.Attrib("width", PngProperty.Width.ToString()) +
			                 XmlUtil.Attrib("height", PngProperty.Height.ToString(), true, false));
			DumpPropertyList(writer, level, WzProperties);
			writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.CloseTag("WzCanvas"));
		}

		/// <summary>
		/// Dispose the object
		/// </summary>
		public override void Dispose() {
			name = null;
			imageProp.Dispose();
			imageProp = null;
			foreach (var prop in properties) prop.Dispose();

			properties.Clear();
			properties = null;
		}

		#endregion

		#region Custom Members

		/// <summary>
		/// Gets the 'origin' position of the Canvas
		/// If not available, it defaults to xy of 0, 0
		/// </summary>
		/// <returns></returns>
		public PointF GetCanvasOriginPosition() {
			var originPos = (WzVectorProperty) this[OriginPropertyName];
			if (originPos != null) {
				return new PointF(originPos.X.Value, originPos.Y.Value);
			}

			return new PointF(0, 0);
		}

		/// <summary>
		/// Gets the 'head' position of the Canvas
		/// If not available, it defaults to xy of 0, 0
		/// </summary>
		/// <returns></returns>
		public PointF? GetCanvasHeadPosition() {
			var headPos = (WzVectorProperty) this[HeadPropertyName];
			if (headPos != null) {
				return new PointF(headPos.X.Value, headPos.Y.Value);
			}

			return null;
		}

		/// <summary>
		/// Gets the 'lt' position of the Canvas
		/// If not available, it defaults to xy of 0, 0
		/// </summary>
		/// <returns></returns>
		public PointF? GetCanvasLtPosition() {
			var headPos = (WzVectorProperty) this[LtPropertyName];
			if (headPos != null) {
				return new PointF(headPos.X.Value, headPos.Y.Value);
			}

			return null;
		}

		/// <summary>
		/// Gets the 'rb' position of the Canvas
		/// If not available, it defaults to xy of 0, 0
		/// </summary>
		/// <returns></returns>
		public PointF? GetCanvasRbPosition() {
			var headPos = (WzVectorProperty) this[RbPropertyName];
			if (headPos != null) {
				return new PointF(headPos.X.Value, headPos.Y.Value);
			}

			return null;
		}

		/// <summary>
		/// Gets whether this WzCanvasProperty contains an '_inlink' for modern maplestory version. v150++
		/// </summary>
		/// <returns></returns>
		public bool HaveInlinkProperty() {
			return this[InlinkPropertyName] != null;
		}

		/// <summary>
		/// Gets whether this WzCanvasProperty contains an '_outlink' for modern maplestory version. v150++
		/// </summary>
		/// <returns></returns>
		public bool HaveOutlinkProperty() {
			return this[OutlinkPropertyName] != null;
		}

		/// <summary>
		/// Gets the '_inlink' WzCanvasProperty of this.
		/// 
		/// '_inlink' is not implemented as part of WzCanvasProperty as I dont want to override existing Wz structure. 
		/// It will be handled via HaRepackerMainPanel instead.
		/// </summary>
		/// <returns></returns>
		public Bitmap GetLinkedWzCanvasBitmap() {
			return GetLinkedWzImageProperty().GetBitmap();
		}

		/// <summary>
		/// Gets the '_inlink' WzCanvasProperty of this.
		/// 
		/// '_inlink' is not implemented as part of WzCanvasProperty as I dont want to override existing Wz structure. 
		/// It will be handled via HaRepackerMainPanel instead.
		/// </summary>
		/// <returns></returns>
		public WzImageProperty GetLinkedWzImageProperty() {
			var
				_inlink = ((WzStringProperty) this[InlinkPropertyName])
					?.Value; // could get nexon'd here. In case they place an _inlink that's not WzStringProperty
			var
				_outlink = ((WzStringProperty) this[OutlinkPropertyName])
					?.Value; // could get nexon'd here. In case they place an _outlink that's not WzStringProperty

			if (_inlink != null) {
				WzObject currentWzObj = this; // first object to work with
				while ((currentWzObj = currentWzObj.Parent) != null) {
					if (!(currentWzObj is WzImage)) // keep looping if its not a WzImage
					{
						continue;
					}

					var wzImageParent = (WzImage) currentWzObj;
					var foundProperty = wzImageParent.GetFromPath(_inlink);
					if (foundProperty != null && foundProperty is WzImageProperty property) return property;
				}
			} else if (_outlink != null) {
				WzObject currentWzObj = this; // first object to work with
				while ((currentWzObj = currentWzObj.Parent) != null) {
					if (!(currentWzObj is WzDirectory)) // keep looping if its not a WzImage
					{
						continue;
					}

					var wzFileParent = ((WzDirectory) currentWzObj).wzFile;

					// TODO
					// Given the way it is structured, it might possibility also point to a different WZ file (i.e NPC.wz instead of Mob.wz).
					// Mob001.wz/8800103.img/8800103.png has an outlink to "Mob/8800141.img/8800141.png"
					// https://github.com/lastbattle/Harepacker-resurrected/pull/142

					var match = Regex.Match(wzFileParent.Name, @"^([A-Za-z]+)([0-9]*).wz");
					var prefixWz = match.Groups[1].Value + "/"; // remove ended numbers and .wz from wzfile name 

					WzObject foundProperty;

					if (_outlink.StartsWith(prefixWz)) {
						// fixed root path
						var realpath = _outlink.Replace(prefixWz, WzFileParent.Name.Replace(".wz", "") + "/");
						foundProperty = wzFileParent.GetObjectFromPath(realpath);
					} else {
						foundProperty = wzFileParent.GetObjectFromPath(_outlink);
					}

					if (foundProperty != null && foundProperty is WzImageProperty property) return property;
				}
			}

			return this;
		}

		/// <summary>
		/// The png image for this canvas property
		/// </summary>
		public WzPngProperty PngProperty {
			get => imageProp;
			set {
				imageProp = value;
				// Kind of weird to do this but I need the parent
				// This is essentially an AddProperty call
				imageProp.Parent = this;
			}
		}

		/// <summary>
		/// Creates a blank WzCanvasProperty
		/// </summary>
		public WzCanvasProperty() {
		}

		/// <summary>
		/// Creates a WzCanvasProperty with the specified name
		/// </summary>
		/// <param name="name">The name of the property</param>
		public WzCanvasProperty(string name) {
			this.name = name;
		}

		/// <summary>
		/// Adds a property to the property list of this property
		/// </summary>
		/// <param name="prop">The property to add</param>
		public void AddProperty(WzImageProperty prop, bool checkListWz = true) {
			prop.Parent = this;
			properties.Add(prop);
			if (!checkListWz) {
				return;
			}

			var image = ParentImage;
			if (image == null || !image.Parsed) return;
			var wzFile = WzFileParent;
			if (wzFile == null) return;
			ListWzContainerImpl.MarkListWzProperty(image, wzFile);
		}

		public void AddProperties(List<WzImageProperty> props) {
			foreach (var prop in props) AddProperty(prop);
		}

		/// <summary>
		/// Remove a property
		/// </summary>
		/// <param name="name">Name of Property</param>
		public void RemoveProperty(WzImageProperty prop) {
			prop.Parent = null;
			properties.Remove(prop);
		}

		/// <summary>
		/// Clears the list of properties
		/// </summary>
		public void ClearProperties() {
			foreach (var prop in properties) prop.Parent = null;
			properties.Clear();
		}

		#endregion

		#region Cast Values

		public override Bitmap GetBitmap() {
			return imageProp.GetImage(false);
		}

		#endregion
	}
}