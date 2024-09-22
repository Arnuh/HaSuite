﻿using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties {
	public class WzRawDataProperty : WzExtended {

		#region Fields
		
		internal WzObject parent;
		internal WzBinaryReader wzReader;
		
		internal long offset;
		internal int length;
		
		internal byte[] bytes;
		
		#endregion
		
		#region Inherited Members

		public override WzImageProperty DeepClone() {
			return new WzRawDataProperty(this);
		}

		public override object WzValue => GetBytes(false);

		public override void SetValue(object value) {
		}

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
		public override WzPropertyType PropertyType => WzPropertyType.Sound;

		public override void WriteValue(WzBinaryWriter writer) {
			var data = GetBytes(false);
			writer.WriteStringValue("RawData", WzImage.WzImageHeaderByte_WithoutOffset,
				WzImage.WzImageHeaderByte_WithOffset);
			writer.Write((byte) 0);
			writer.Write(data.Length);
			writer.Write(data);
		}

		public override void ExportXml(StreamWriter writer, int level) {
			writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.EmptyNamedTag("RawData", Name));
		}

		/// <summary>
		/// Disposes the object
		/// </summary>
		public override void Dispose() {
			name = null;
			bytes = null;
		}

		#endregion
		
		public WzRawDataProperty(string name, WzBinaryReader reader, bool parseNow) {
			this.name = name;
			wzReader = reader;
			reader.BaseStream.Position++;
			length = reader.ReadInt32();
			offset = reader.BaseStream.Position;
			if (parseNow) {
				GetBytes(true);
			} else {
				reader.BaseStream.Position += length;
			}
		}

		private WzRawDataProperty(WzRawDataProperty copy) {
			name = copy.name;
			bytes = new byte[copy.length];
			copy.GetBytes(false).CopyTo(bytes, 0);
			length = copy.length;
		}

		public byte[] GetBytes(bool saveInMemory) {
			if (bytes != null) {
				return bytes;
			}

			if (wzReader == null) {
				return null;
			}

			var currentPos = wzReader.BaseStream.Position;
			wzReader.BaseStream.Position = offset;
			bytes = wzReader.ReadBytes(length);
			wzReader.BaseStream.Position = currentPos;
			if (saveInMemory) {
				return bytes;
			}

			var result = bytes;
			bytes = null;
			return result;
		}
	}
}