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
			return ParseListFile(filePath, WzTool.GetIvByMapleVersion(version));
		}

		/// <summary>
		/// Parses a wz list file on the disk
		/// </summary>
		/// <param name="filePath">Path to the wz file</param>
		private static List<string> ParseListFile(string filePath, byte[] WzIv) {
			var listEntries = new List<string>();
			var wzFileBytes = File.ReadAllBytes(filePath);
			var wzParser = new WzBinaryReader(new MemoryStream(wzFileBytes), WzIv);
			while (wzParser.PeekChar() != -1) {
				var len = wzParser.ReadInt32();
				var strChrs = new char[len];
				for (var i = 0; i < len; i++)
					strChrs[i] = (char) wzParser.ReadInt16();
				wzParser.ReadUInt16(); //encrypted null
				var decryptedStr = wzParser.DecryptString(strChrs);
				listEntries.Add(decryptedStr);
			}

			wzParser.Close();
			var lastIndex = listEntries.Count - 1;
			var lastEntry = listEntries[lastIndex];
			listEntries[lastIndex] = lastEntry.Substring(0, lastEntry.Length - 1) + "g";
			return listEntries;
		}

		public static void SaveToDisk(string path, WzMapleVersion version, List<string> listEntries) {
			SaveToDisk(path, WzTool.GetIvByMapleVersion(version), listEntries);
		}

		public static void SaveToDisk(string path, byte[] WzIv, List<string> listEntries) {
			var lastIndex = listEntries.Count - 1;
			var lastEntry = listEntries[lastIndex];
			listEntries[lastIndex] = lastEntry.Substring(0, lastEntry.Length - 1) + "/";
			var wzWriter = new WzBinaryWriter(File.Create(path), WzIv);

			foreach (var listEntry in listEntries) {
				wzWriter.Write((int) listEntry.Length);
				var encryptedChars = wzWriter.EncryptString(listEntry + (char) 0);
				for (var j = 0; j < encryptedChars.Length; j++)
					wzWriter.Write((short) encryptedChars[j]);
			}

			listEntries[lastIndex] = lastEntry.Substring(0, lastEntry.Length - 1) + "/";
		}
	}
}