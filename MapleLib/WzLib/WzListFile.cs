﻿/*  MapleLib - A general-purpose MapleStory library
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

using System.Collections.Generic;
using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib {
	/// <summary>
	/// A class that parses and contains the data of a wz list file
	/// </summary>
	public static class ListFileParser {
		/// <summary>
		/// Parses a wz list file on the disk
		/// </summary>
		/// <param name="filePath">Path to the wz file</param>
		public static List<string> ParseListFile(string filePath, WzMapleVersion version) {
			return ParseListFile(filePath, WzTool.GetIvByMapleVersion(version), WzTool.GetUserKeyByMapleVersion(version));
		}

		/// <summary>
		/// Parses a wz list file on the disk
		/// </summary>
		/// <param name="filePath">Path to the wz file</param>
		public static List<string> ParseListFile(string filePath, byte[] WzIv, byte[] UserKey) {
			var listEntries = new List<string>();
			var wzFileBytes = File.ReadAllBytes(filePath);
			using (var wzParser = new WzBinaryReader(new MemoryStream(wzFileBytes), WzIv, UserKey)) {
				while (wzParser.PeekChar() != -1) {
					var len = wzParser.ReadInt32();
					var strChrs = new char[len];
					for (var i = 0; i < len; i++) {
						strChrs[i] = (char) wzParser.ReadInt16();
					}

					// This isn't included in length provided above
					wzParser.ReadUInt16(); //encrypted null

					var decryptedStr = wzParser.DecryptString(strChrs);
					listEntries.Add(decryptedStr);
				}
			}

			return listEntries;
		}

		public static void SaveToDisk(string path, WzMapleVersion version, List<string> listEntries) {
			SaveToDisk(path, WzTool.GetIvByMapleVersion(version), WzTool.GetUserKeyByMapleVersion(version), listEntries);
		}

		public static void SaveToDisk(string path, byte[] WzIv, byte[] UserKey, List<string> listEntries) {
			using (var wzWriter = new WzBinaryWriter(File.Create(path), WzIv, UserKey)) {
				foreach (var listEntry in listEntries) {
					wzWriter.Write(listEntry.Length);
					var encryptedChars = wzWriter.EncryptString(listEntry + (char) 0);
					foreach (var c in encryptedChars) {
						wzWriter.Write((short) c);
					}
				}
			}
		}
	}
}