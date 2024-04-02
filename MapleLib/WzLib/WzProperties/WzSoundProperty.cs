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
using System.Diagnostics;
using System.IO;
using System.Linq;
using MapleLib.Helpers;
using MapleLib.WzLib.Util;
using NAudio.Wave;

namespace MapleLib.WzLib.WzProperties {

	/// <summary>
	/// A property that contains data for an MP3
	/// </summary>
	public class WzSoundProperty : WzExtended {
		#region Constants

		public static readonly byte[] GUID_NULL = new byte[16];

		public static readonly byte[] HEADER = {
			0x81, 0x9F, 0x58, 0x05, 0x56, 0xC3, 0xCE, 0x11, 0xBF, 0x01, 0x00, 0xAA, 0x00, 0x55, 0x59, 0x5A
		};

		public static readonly byte[] GUID_SOUND_DEFAULT = {
			0x83, 0xEB, 0x36, 0xE4, 0x4F, 0x52, 0xCE, 0x11, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70
		};

		public static readonly byte[] GUID_SOUNDDX8_DEFAULT = {
			0x8B, 0xEB, 0x36, 0xE4, 0x4F, 0x52, 0xCE, 0x11, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70
		};

		#endregion
		
		#region Fields
		
		internal WzObject parent;
		internal WzBinaryReader wzReader;

		internal long offset;
		internal int length;
		internal int sampleLength;
		internal int count;
		internal byte[] GUID_Sound, GUID_SoundDX8;
		internal int mask;
		internal byte[] header;

		internal byte[] soundBytes;

		internal WaveFormat wavFormat;

		#endregion

		#region Inherited Members

		public override WzImageProperty DeepClone() {
			var clone = new WzSoundProperty(this);
			return clone;
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
			writer.WriteStringValue("Sound_DX8", WzImage.WzImageHeaderByte_WithoutOffset,
				WzImage.WzImageHeaderByte_WithOffset);
			writer.Write((byte) 0);
			writer.WriteCompressedInt(data.Length);
			writer.WriteCompressedInt(sampleLength);
			writer.Write((byte) count);
			writer.Write(GUID_Sound);
			writer.Write(GUID_SoundDX8);
			writer.Write((byte) 0);
			writer.Write((byte) mask);
			writer.Write(header);
			if (wavFormat != null) {
				var formatBytes = GetWaveFormatBytes();
				writer.Write((byte) formatBytes.Length);
				writer.Write(formatBytes);
			}
			writer.Write(data);
		}

		public override void ExportXml(StreamWriter writer, int level) {
			writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.EmptyNamedTag("WzSound", Name));
		}

		/// <summary>
		/// Disposes the object
		/// </summary>
		public override void Dispose() {
			name = null;
			GUID_Sound = null;
			GUID_SoundDX8 = null;
			header = null;
			wavFormat = null;
			soundBytes = null;
		}

		#endregion

		#region Custom Members

		/// <summary>
		/// The data of the mp3 header
		/// </summary>
		public byte[] Header {
			get => header;
			set => header = value;
		}

		/// <summary>
		/// Length of the mp3 file in milliseconds
		/// </summary>
		public int Length {
			get => sampleLength;
			set => sampleLength = value;
		}

		/// <summary>
		/// Frequency of the mp3 file in Hz
		/// </summary>
		public int Frequency => wavFormat?.SampleRate ?? 0;

		public WaveFormat WavFormat {
			get => wavFormat;
			private set { }
		}
		
		/// <summary>
		/// Creates a WzSoundProperty with the specified name
		/// </summary>
		/// <param name="name">The name of the property</param>
		/// <param name="reader">The wz reader</param>
		/// <param name="parseNow">Indicating whether to parse the property now</param>
		public WzSoundProperty(string name, WzBinaryReader reader, bool parseNow) {
			this.name = name;
			wzReader = reader;

			reader.ReadByte(); // always 0, throws error if not.

			length = reader.ReadCompressedInt();
			sampleLength = reader.ReadCompressedInt();

			count = reader.Read();

			GUID_Sound = reader.ReadBytes(16);
			GUID_SoundDX8 = reader.ReadBytes(16);

			reader.Read(); // always 0?
			mask = reader.Read();

			header = reader.ReadBytes(16);

			if (!header.SequenceEqual(HEADER)) {
				if (!header.SequenceEqual(GUID_NULL)) {
					int size = reader.ReadByte();
					if (size == 0) {
						// exception
					}

					GUID_SoundDX8 = reader.ReadBytes(size);
				}
			}

			ParseWaveFormat();

			//sound file offset
			offset = reader.BaseStream.Position;
			if (parseNow) {
				soundBytes = reader.ReadBytes(length);
			} else {
				reader.BaseStream.Position += length;
			}
		}

		/// <summary>
		/// Creates a WzSoundProperty with the specified name and data from another WzSoundProperty Object
		/// </summary>
		private WzSoundProperty(WzSoundProperty copy) {
			name = copy.name;

			length = copy.length;
			sampleLength = copy.sampleLength;
			count = copy.count;

			GUID_Sound = new byte[copy.GUID_Sound.Length];
			Array.Copy(copy.GUID_Sound, GUID_Sound, copy.GUID_Sound.Length);

			GUID_SoundDX8 = new byte[copy.GUID_SoundDX8.Length];
			Array.Copy(copy.GUID_SoundDX8, GUID_SoundDX8, copy.GUID_SoundDX8.Length);

			mask = copy.mask;

			header = new byte[copy.header.Length];
			Array.Copy(copy.header, header, copy.header.Length);

			if (copy.soundBytes == null) {
				soundBytes = copy.GetBytes(false);
			} else {
				soundBytes = new byte[copy.soundBytes.Length];
				Array.Copy(copy.soundBytes, soundBytes, copy.soundBytes.Length);
			}

			wavFormat = copy.wavFormat;
		}

		/// <summary>
		/// Creates a WzSoundProperty with the specified name and data
		/// </summary>
		/// <param name="name"></param>
		/// <param name="sampleLength"></param>
		/// <param name="header"></param>
		/// <param name="data"></param>
		/// <param name="format"></param>
		public WzSoundProperty(string name, int sampleLength, byte[] format, byte[] header, byte[] data) {
			this.name = name;
			this.sampleLength = sampleLength;

			// Probably fine to not keep the original, who cares.
			count = 2;
			GUID_Sound = new byte[GUID_SOUND_DEFAULT.Length];
			Array.Copy(GUID_SOUND_DEFAULT, GUID_Sound, GUID_SOUND_DEFAULT.Length);

			GUID_SoundDX8 = new byte[GUID_SOUNDDX8_DEFAULT.Length];
			Array.Copy(GUID_SOUNDDX8_DEFAULT, GUID_SoundDX8, GUID_SOUNDDX8_DEFAULT.Length);

			// Also probably fine
			mask = 1;

			ParseWaveFormat(format);

			this.header = new byte[header.Length];
			Array.Copy(header, this.header, header.Length);

			soundBytes = new byte[data.Length];
			Array.Copy(data, soundBytes, data.Length);
			length = soundBytes.Length;
		}

		/// <summary>
		/// Creates a WzSoundProperty with the specified name from a file
		/// </summary>
		/// <param name="name">The name of the property</param>
		/// <param name="file">The path to the sound file</param>
		public WzSoundProperty(string name, string file) {
			this.name = name;
			using (var reader = new Mp3FileReader(file)) {
				wavFormat = reader.Mp3WaveFormat;
				sampleLength = (int) (reader.Length * 1000d / reader.WaveFormat.AverageBytesPerSecond);
			}

			count = 2;
			GUID_Sound = new byte[GUID_SOUND_DEFAULT.Length];
			Array.Copy(GUID_SOUND_DEFAULT, GUID_Sound, GUID_SOUND_DEFAULT.Length);

			GUID_SoundDX8 = new byte[GUID_SOUNDDX8_DEFAULT.Length];
			Array.Copy(GUID_SOUNDDX8_DEFAULT, GUID_SoundDX8, GUID_SOUNDDX8_DEFAULT.Length);

			mask = 1;

			header = new byte[HEADER.Length];
			Array.Copy(HEADER, header, HEADER.Length);

			soundBytes = File.ReadAllBytes(file);
			length = soundBytes.Length;
		}

		public byte[] GetWaveFormatBytes() {
			return wavFormat == null ? null : ByteUtils.StructToBytes(wavFormat);
		}

		private void ParseWaveFormat() {
			if (header.SequenceEqual(GUID_NULL)) {
				// Not sure, but the only case this happens is when the below data doesn't exist
				return;
			}

			var waveFormatExLen = wzReader.ReadByte();

			if (waveFormatExLen == 30) {
				var data = wzReader.ReadBytes(waveFormatExLen);
				ParseWaveFormat(data);
			} else if (waveFormatExLen == 18) {
				var data = wzReader.ReadBytes(waveFormatExLen);
				ParseWaveFormat(data);
			} else {
				ErrorLogger.Log(ErrorLevel.MissingFeature, $"Unknown wave encoding length {waveFormatExLen}");
			}
		}

		private void ParseWaveFormat(byte[] data) {
			if (data == null || data.Length == 0) {
				return;
			}

			if (data.Length == 30) {
				wavFormat = ByteUtils.BytesToStructConstructorless<Mp3WaveFormat>(data);
			} else if (data.Length == 18) {
				wavFormat = ByteUtils.BytesToStruct<WaveFormat>(data);
			}
		}

		#endregion

		#region Parsing Methods

		public byte[] GetBytes(bool saveInMemory) {
			if (soundBytes != null) {
				return soundBytes;
			}

			if (wzReader == null) {
				return null;
			}

			var currentPos = wzReader.BaseStream.Position;
			wzReader.BaseStream.Position = offset;
			soundBytes = wzReader.ReadBytes(length);
			wzReader.BaseStream.Position = currentPos;
			if (saveInMemory) {
				return soundBytes;
			}

			var result = soundBytes;
			soundBytes = null;
			return result;
		}

		public void SaveToFile(string file) {
			File.WriteAllBytes(file, GetBytes(false));
		}

		#endregion

		#region Cast Values

		public override byte[] GetBytes() {
			return GetBytes(false);
		}

		#endregion
	}
}