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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MapleLib.ClientLib;
using MapleLib.Helpers;
using MapleLib.MapleCryptoLib;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib {
	/// <summary>
	/// A class that contains all the information of a wz file
	/// </summary>
	public class WzFile : WzObject, ListWzContainer {
		#region Fields

		internal string path;
		internal WzDirectory wzDir;
		internal WzHeader header;
		internal string name = "";

		internal ushort wzVersionHeader;
		internal const ushort wzVersionHeader64bit_start = 770; // 777 for KMS, GMS v230 uses 778.. wut

		internal uint versionHash;
		internal short mapleStoryPatchVersion;
		internal WzMapleVersion maplepLocalVersion;
		internal MapleStoryLocalisation mapleLocaleVersion = MapleStoryLocalisation.Not_Known;

		internal bool
			wz_withEncryptVersionHeader =
				true; // KMS update after Q4 2021, ver 1.2.357 does not contain any wz enc header information

		internal byte[] WzIv;
		internal byte[] UserKey;

		internal string listWzPath = string.Empty;

		#endregion

		/// <summary>
		/// The parsed IWzDir after having called ParseWzDirectory(), this can either be a WzDirectory or a WzListDirectory
		/// </summary>
		public WzDirectory WzDirectory => wzDir;

		/// <summary>
		/// Name of the WzFile
		/// </summary>
		public override string Name {
			get => name;
			set => name = value;
		}

		/// <summary>
		/// The WzObjectType of the file
		/// </summary>
		public override WzObjectType ObjectType => WzObjectType.File;

		public string ListWzPath => listWzPath;

		/// <summary>
		/// Returns WzDirectory[name]
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns>WzDirectory[name]</returns>
		public new WzObject this[string name] => WzDirectory[name];

		public WzHeader Header {
			get => header;
			set => header = value;
		}

		public short Version {
			get => mapleStoryPatchVersion;
			set => mapleStoryPatchVersion = value;
		}

		public string FilePath => path;

		public WzMapleVersion MapleVersion {
			get => maplepLocalVersion;
			set => maplepLocalVersion = value;
		}

		/// <summary>
		/// The detected MapleStory locale version from 'MapleStory.exe' client.
		/// KMST, GMS, EMS, MSEA, CMS, TWMS, etc.
		/// </summary>
		public MapleStoryLocalisation MapleLocaleVersion {
			get => mapleLocaleVersion;
			private set { }
		}

		/// <summary>
		///  Since KMST1132 / GMSv230 around 2022/02/09, wz removed the 2-byte encVer at position 0x3C, and use a fixed encVer 777.
		/// </summary>
		public bool Is64BitWzFile {
			get => !wz_withEncryptVersionHeader;
			private set { }
		}

		public override WzObject Parent {
			get => null;
			internal set { }
		}

		public override WzFile WzFileParent => this;

		public override void Dispose() {
			_isUnloaded = true; // flag first

			if (wzDir?.reader == null) {
				Debug.WriteLine("WzFile.Dispose() : wzDir.reader is null");
				return;
			}

			wzDir.reader.Close();
			wzDir.reader = null;
			Header = null;
			path = null;
			name = null;
			ListWzEntries = null;
			wzDir.Dispose();
		}

		private bool _isUnloaded;

		/// <summary>
		/// Returns true if this WZ file has been unloaded
		/// </summary>
		public bool IsUnloaded {
			get => _isUnloaded;
			private set { }
		}

		/// <summary>
		/// Initialize MapleStory WZ file
		/// </summary>
		/// <param name="gameVersion"></param>
		/// <param name="version"></param>
		public WzFile(short gameVersion, WzMapleVersion version) {
			wzDir = new WzDirectory();
			Header = WzHeader.GetDefault();
			mapleStoryPatchVersion = gameVersion;
			maplepLocalVersion = version;
			WzIv = WzTool.GetIvByMapleVersion(version);
			UserKey = WzTool.GetUserKeyByMapleVersion(version);
			wzDir.WzIv = WzIv;
			wzDir.UserKey = UserKey;
		}

		/// <summary>
		/// Open a wz file from a file on the disk
		/// </summary>
		/// <param name="filePath">Path to the wz file</param>
		/// <param name="version"></param>
		public WzFile(string filePath, WzMapleVersion version) : this(filePath, -1, version) {
		}

		/// <summary>
		/// Open a wz file from a file on the disk
		/// </summary>
		/// <param name="filePath">Path to the wz file</param>
		/// <param name="gameVersion"></param>
		/// <param name="version"></param>
		public WzFile(string filePath, short gameVersion, WzMapleVersion version) {
			name = Path.GetFileName(filePath);
			path = filePath;
			mapleStoryPatchVersion = gameVersion;
			maplepLocalVersion = version;

			if (version == WzMapleVersion.GETFROMZLZ) {
				using (var zlzStream = File.OpenRead(Path.Combine(Path.GetDirectoryName(filePath), "ZLZ.dll"))) {
					WzIv = WzKeyGenerator.GetIvFromZlz(zlzStream);
					UserKey = MapleCryptoConstants.UserKey_WzLib;
				}
			} else {
				WzIv = WzTool.GetIvByMapleVersion(version);
				UserKey = WzTool.GetUserKeyByMapleVersion(version);
			}

			var directory = Path.GetDirectoryName(path);
			if (directory != null) {
				listWzPath = Path.Combine(directory, "List.wz");
			}
		}

		/// <summary>
		/// Open a wz file from a file on the disk with a custom WzIv key
		/// </summary>
		/// <param name="filePath">Path to the wz file</param>
		public WzFile(string filePath, byte[] wzIv) {
			name = Path.GetFileName(filePath);
			path = filePath;
			mapleStoryPatchVersion = -1;
			maplepLocalVersion = WzMapleVersion.BRUTEFORCE;

			WzIv = wzIv;
			UserKey = MapleCryptoConstants.UserKey_WzLib;
		}

		/// <summary>
		/// Parses the wz file, if the wz file is a list.wz file, WzDirectory will be a WzListDirectory, if not, it'll simply be a WzDirectory
		/// </summary>
		/// <param name="WzIv">WzIv is not set if null (Use existing iv)</param>
		public WzFileParseStatus ParseWzFile(byte[] WzIv = null) {
			/*if (maplepLocalVersion != WzMapleVersion.GENERATE)
			{
			    parseErrorMessage = ("Cannot call ParseWzFile() if WZ file type is not GENERATE. Have you entered an invalid WZ key? ");
			    return false;
			}*/
			if (WzIv != null) this.WzIv = WzIv;

			var result = ParseMainWzDirectory();
			if (result == WzFileParseStatus.Success) {
				LoadListWz(listWzPath);
			}

			return result;
		}


		/// <summary>
		/// Parse directories in the WZ file
		/// </summary>
		/// <param name="parseErrorMessage"></param>
		/// <param name="lazyParse">Only load the firt WzDirectory found if true</param>
		/// <returns></returns>
		internal WzFileParseStatus ParseMainWzDirectory(bool lazyParse = false) {
			if (path == null) {
				ErrorLogger.Log(ErrorLevel.Critical, "[Error] Path is null");
				return WzFileParseStatus.Path_Is_Null;
			}

			var reader =
				new WzBinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), WzIv, UserKey);

			Header = new WzHeader();
			Header.Ident = reader.ReadString(4);
			Header.FSize = reader.ReadUInt64();
			Header.FStart = reader.ReadUInt32();
			Header.Copyright = reader.ReadString((int) (Header.FStart - 17U));

			var unk1 = reader.ReadByte();
			var unk2 = reader.ReadBytes((int) (Header.FStart - (ulong) reader.BaseStream.Position));
			reader.Header = Header;

			Check64BitClient(reader); // update b64BitClient flag

			// the value of wzVersionHeader is less important. It is used for reading/writing from/to WzFile Header, and calculating the versionHash.
			// it can be any number if the client is 64-bit. Assigning 777 is just for convenience when calculating the versionHash.
			wzVersionHeader = wz_withEncryptVersionHeader ? reader.ReadUInt16() : wzVersionHeader64bit_start;

			Debug.WriteLine("----------------------------------------");
			Debug.WriteLine($"Read Wz File {path}");
			Debug.WriteLine($"wz_withEncryptVersionHeader: {wz_withEncryptVersionHeader}");
			Debug.WriteLine($"wzVersionHeader: {wzVersionHeader}");
			Debug.WriteLine($"wzIv: {string.Join(",", WzIv.Select(x => x.ToString()).ToArray())}");
			Debug.WriteLine("----------------------------------------");

			if (mapleStoryPatchVersion == -1) {
				// for 64-bit client, return immediately if version 777 works correctly.
				// -- the latest KMS update seems to have changed it to 778? 779?
				if (!wz_withEncryptVersionHeader) {
					for (var maplestoryVerToDecode = wzVersionHeader64bit_start;
					     maplestoryVerToDecode < wzVersionHeader64bit_start + 10;
					     maplestoryVerToDecode++) // 770 ~ 780
					{
						if (TryDecodeWithWZVersionNumber(reader, wzVersionHeader, maplestoryVerToDecode, lazyParse)) {
							return WzFileParseStatus.Success;
						}
					}
				}

				// Attempt to get version from MapleStory.exe first
				var maplestoryVerDetectedFromClient = GetMapleStoryVerFromExe(path, out mapleLocaleVersion);

				// this step is actually not needed if we know the maplestory patch version (the client .exe), but since we dont..
				// we'll need a bruteforce way around it. 
				const short MAX_PATCH_VERSION = 1000; // wont be reached for the forseeable future.

				for (int j = maplestoryVerDetectedFromClient; j < MAX_PATCH_VERSION; j++)
					//Debug.WriteLine("Try decode 1 with maplestory ver: " + j);
				{
					if (TryDecodeWithWZVersionNumber(reader, wzVersionHeader, j, lazyParse)) {
						return WzFileParseStatus.Success;
					}
				}

				//parseErrorMessage = "Error with game version hash : The specified game version is incorrect and WzLib was unable to determine the version itself";
				return WzFileParseStatus.Error_Game_Ver_Hash;
			}

			versionHash = CheckAndGetVersionHash(wzVersionHeader, mapleStoryPatchVersion);
			reader.Hash = versionHash;

			var directory = new WzDirectory(reader, name, versionHash, WzIv, UserKey, this);
			directory.ParseDirectory();
			wzDir = directory;

			return WzFileParseStatus.Success;
		}

		/// <summary>
		/// encVer detecting:
		/// Since KMST1132 (GMSv230, 2022/02/09), wz removed the 2-byte encVer at 0x3C, and use a fixed encVer 777.
		/// Here we try to read the first 2 bytes from data part (0x3C) and guess if it looks like an encVer.
		///
		/// Credit: WzComparerR2 project
		/// </summary>
		private void Check64BitClient(WzBinaryReader reader) {
			if (Header.FSize >= 2) {
				reader.BaseStream.Position = header.FStart; // go back to 0x3C

				int encver = reader.ReadUInt16();
				if (encver > 0xff) { // encver always less than 256
					wz_withEncryptVersionHeader = false;
				} else if (encver == 0x80) {
					// there's an exceptional case that the first field of data part is a compressed int which determined property count,
					// if the value greater than 127 and also to be a multiple of 256, the first 5 bytes will become to
					//   80 00 xx xx xx
					// so we additional check the int value, at most time the child node count in a wz won't greater than 65536.
					if (Header.FSize >= 5) {
						reader.BaseStream.Position = header.FStart; // go back to 0x3C
						var propCount = reader.ReadInt32();
						if (propCount > 0 && (propCount & 0xff) == 0 && propCount <= 0xffff) {
							wz_withEncryptVersionHeader = false;
						}
					}
				}
				// old wz file with header version
			} else {
				// Obviously, if data part have only 1 byte, encver must be deleted.
				wz_withEncryptVersionHeader = false;
			}


			// reset position
			reader.BaseStream.Position = Header.FStart;
		}

		private bool TryDecodeWithWZVersionNumber(WzBinaryReader reader, int useWzVersionHeader,
			int useMapleStoryPatchVersion, bool lazyParse) {
			mapleStoryPatchVersion = (short) useMapleStoryPatchVersion;

			versionHash = CheckAndGetVersionHash(useWzVersionHeader, mapleStoryPatchVersion);

			if (versionHash == 0) {
				// ugly hack, but that's the only way if the version number isnt known (nexon stores this in the .exe)
				return false;
			}

			reader.Hash = versionHash;
			var fallbackOffsetPosition =
				reader.BaseStream.Position; // save position to rollback to, if should parsing fail from here
			WzDirectory testDirectory;
			try {
				testDirectory = new WzDirectory(reader, name, versionHash, WzIv, UserKey, this);
				testDirectory.ParseDirectory(lazyParse);
			} catch (Exception exp) {
				Debug.WriteLine(exp.ToString());

				reader.BaseStream.Position = fallbackOffsetPosition;
				return false;
			}

			// test the image and see if its correct by parsing it 
			var closeTestDirectory = true;
			var testImage = testDirectory.WzImages.FirstOrDefault();
			if (testImage != null) {
				try {
					reader.BaseStream.Position = testImage.Offset;
					var checkByte = reader.ReadByte();
					reader.BaseStream.Position = fallbackOffsetPosition;

					switch (checkByte) {
						case 0x73:
						case 0x1b: {
							var directory = new WzDirectory(reader, name, versionHash, WzIv, UserKey, this);

							directory.ParseDirectory(lazyParse);
							wzDir = directory;
							return true;
						}
						case 0x30:
						case 0x6C: // idk
						case 0xBC: // Map002.wz? KMST?
						// v72 and v73 have a 79 and 193 check byte for mob.wz
						// Idk what this is even for...  
						default: {
							var printError =
								$"[WzFile.cs] New Wz image header found. checkByte = {checkByte} for version {mapleStoryPatchVersion}. File Name = {Name}";

							ErrorLogger.Log(ErrorLevel.MissingFeature, printError);
							Debug.WriteLine(printError);
							// log or something
							break;
						}
					}

					reader.BaseStream.Position = fallbackOffsetPosition; // reset
					return false;
				} catch {
					reader.BaseStream.Position = fallbackOffsetPosition; // reset
					return false;
				}

				return true;
			}

			// if there's no image in the WZ file (new KMST Base.wz), test the directory instead
			// coincidentally in msea v194 Map001.wz, the hash matches exactly using mapleStoryPatchVersion of 113, and it fails to decrypt later on (probably 1 in a million chance? o_O).
			// damn, technical debt accumulating here
			// also needs to check for 'Is64BitWzFile' as it may match TaiwanMS v113 (pre-bb) and return as false.
			if (Is64BitWzFile && mapleStoryPatchVersion == 113) {
				// hack for now
				reader.BaseStream.Position = fallbackOffsetPosition; // reset
				return false;
			}

			wzDir = testDirectory;
			closeTestDirectory = false;

			return true;
		}

		/// <summary>
		/// Attempts to get the MapleStory patch version number from MapleStory.exe
		/// </summary>
		/// <returns>0 if the exe could not be found, or version number be detected</returns>
		private static short GetMapleStoryVerFromExe(string wzFilePath, out MapleStoryLocalisation mapleLocaleVersion) {
			// https://github.com/lastbattle/Harepacker-resurrected/commit/63e2d72ac006f0a45fc324a2c33c23f0a4a988fa#r56759414
			// <3 mechpaul
			const string MAPLESTORY_EXE_NAME = "MapleStory.exe";
			const string MAPLESTORYT_EXE_NAME = "MapleStoryT.exe";
			const string MAPLESTORYADMIN_EXE_NAME = "MapleStoryA.exe";

			var wzFileInfo = new FileInfo(wzFilePath);
			if (!wzFileInfo.Exists) {
				mapleLocaleVersion = MapleStoryLocalisation.Not_Known; // set
				return 0;
			}

			var currentDirectory = wzFileInfo.Directory;
			for (var i = 0; i < 4; i++) // just attempt 4 directories here
			{
				var msExeFileInfos =
					currentDirectory.GetFiles(MAPLESTORY_EXE_NAME, SearchOption.TopDirectoryOnly); // case insensitive 
				var msTExeFileInfos =
					currentDirectory.GetFiles(MAPLESTORYT_EXE_NAME, SearchOption.TopDirectoryOnly); // case insensitive 
				var msAdminExeFileInfos =
					currentDirectory.GetFiles(MAPLESTORYADMIN_EXE_NAME,
						SearchOption.TopDirectoryOnly); // case insensitive 

				var exeFileInfo = new List<FileInfo>();
				if (msTExeFileInfos.Length > 0 && msTExeFileInfos[0].Exists) // prioritize MapleStoryT.exe first
				{
					exeFileInfo.Add(msTExeFileInfos[0]);
				}

				if (msAdminExeFileInfos.Length > 0 && msAdminExeFileInfos[0].Exists) {
					exeFileInfo.Add(msAdminExeFileInfos[0]);
				}

				if (msExeFileInfos.Length > 0 && msExeFileInfos[0].Exists) exeFileInfo.Add(msExeFileInfos[0]);

				foreach (var msExeFileInfo in exeFileInfo) {
					var versionInfo =
						FileVersionInfo.GetVersionInfo(Path.Combine(currentDirectory.FullName, msExeFileInfo.FullName));

					if ((versionInfo.FileMajorPart == 1 && versionInfo.FileMinorPart == 0 &&
					     versionInfo.FileBuildPart == 0)
					    || (versionInfo.FileMajorPart == 0 && versionInfo.FileMinorPart == 0 &&
					        versionInfo.FileBuildPart == 0)) // older client uses 1.0.0.1 
					{
						continue;
					}

					var locale = versionInfo.FileMajorPart;
					var localeVersion = MapleStoryLocalisation.Not_Known;
					if (Enum.IsDefined(typeof(MapleStoryLocalisation), locale)) {
						localeVersion = (MapleStoryLocalisation) locale;
					}

					var msVersion = versionInfo.FileMinorPart;
					var msMinorPatchVersion = versionInfo.FileBuildPart;

					mapleLocaleVersion = localeVersion; // set
					return (short) msVersion;
				}

				currentDirectory = currentDirectory.Parent; // check the parent folder on the next run
				if (currentDirectory == null) {
					break;
				}
			}

			mapleLocaleVersion = MapleStoryLocalisation.Not_Known; // set
			return 0;
		}

		/// <summary>
		/// Check and gets the version hash.
		/// </summary>
		/// <param name="wzVersionHeader">The version header from .wz file.</param>
		/// <param name="maplestoryPatchVersion"></param>
		/// <returns></returns>
		private static uint CheckAndGetVersionHash(int wzVersionHeader, int maplestoryPatchVersion) {
			uint versionHash = 0;

			foreach (var ch in maplestoryPatchVersion.ToString()) versionHash = versionHash * 32 + (byte) ch + 1;

			if (wzVersionHeader == wzVersionHeader64bit_start) {
				return versionHash; // always 59192
			}

			int decryptedVersionNumber = (byte) ~(((versionHash >> 24) & 0xFF) ^ ((versionHash >> 16) & 0xFF) ^
			                                      ((versionHash >> 8) & 0xFF) ^ (versionHash & 0xFF));

			if (wzVersionHeader == decryptedVersionNumber) {
				return versionHash;
			}

			return 0; // invalid
		}

		/// <summary>
		/// Version hash
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CreateWZVersionHash() {
			versionHash = 0;
			foreach (var ch in mapleStoryPatchVersion.ToString()) versionHash = versionHash * 32 + (byte) ch + 1;

			wzVersionHeader = (byte) ~(((versionHash >> 24) & 0xFF) ^ ((versionHash >> 16) & 0xFF) ^
			                           ((versionHash >> 8) & 0xFF) ^ (versionHash & 0xFF));
		}

		/// <summary>
		/// Saves a wz file to the disk, AKA repacking.
		/// </summary>
		/// <param name="path">Path to the output wz file</param>
		/// <param name="override_saveAs64BitWZ"></param>
		/// <param name="savingToPreferredWzVer"></param>
		public void SaveToDisk(string path, bool? override_saveAs64BitWZ = null,
			WzMapleVersion savingToPreferredWzVer = WzMapleVersion.UNKNOWN) {
			// WZ IV
			if (savingToPreferredWzVer == WzMapleVersion.UNKNOWN) {
				WzIv = WzTool.GetIvByMapleVersion(maplepLocalVersion); // get from local WzFile
				UserKey = WzTool.GetUserKeyByMapleVersion(maplepLocalVersion);
			} else {
				WzIv = WzTool.GetIvByMapleVersion(savingToPreferredWzVer); // custom selected
				UserKey = WzTool.GetUserKeyByMapleVersion(savingToPreferredWzVer);
			}

			var isWzIvSimilar = WzIv.SequenceEqual(wzDir.WzIv); // check if its saving to the same IV.
			var isWzUserKeyDefault = UserKey.SequenceEqual(wzDir.UserKey); // check if its saving to the same UserKey.
			wzDir.WzIv = WzIv;
			wzDir.UserKey = UserKey;

			// Save WZ as 64-bit wz format
			var saveAs64BitWz = Is64BitWzFile;
			if (override_saveAs64BitWZ != null) saveAs64BitWz = (bool) override_saveAs64BitWZ;

			CreateWZVersionHash();
			wzDir.SetVersionHash(versionHash);

			Debug.WriteLine("----------------------------------------");
			Debug.WriteLine($"Saving Wz File {Name}");
			Debug.WriteLine($"wzVersionHeader: {wzVersionHeader}");
			Debug.WriteLine($"saveAs64BitWz: {saveAs64BitWz}");
			Debug.WriteLine("----------------------------------------");

			try {
				var tempFile = Path.GetFileNameWithoutExtension(path) + ".TEMP";
				// WzFile has a path but saving has a different path
				// Which to use....
				var directory = Path.GetDirectoryName(path);
				var listWzPath = string.Empty;
				if (directory != null) {
					listWzPath = Path.Combine(directory, "List.wz");
				}

				using (var fs = new FileStream(tempFile, FileMode.Append, FileAccess.Write)) {
					wzDir.GenerateDataFile(listWzPath, isWzIvSimilar ? null : WzIv, isWzUserKeyDefault, fs);
				}

				WzTool.StringCache.Clear();

				using (var wzWriter = new WzBinaryWriter(File.Create(path), WzIv, UserKey)) {
					wzWriter.Hash = versionHash;

					var totalLen = wzDir.GetImgOffsets(wzDir.GetOffsets(Header.FStart + (!saveAs64BitWz ? 2u : 0)));
					Header.FSize = totalLen - Header.FStart;
					for (var i = 0; i < 4; i++) wzWriter.Write((byte) Header.Ident[i]);

					wzWriter.Write((long) Header.FSize);
					wzWriter.Write(Header.FStart);
					wzWriter.WriteNullTerminatedString(Header.Copyright);

					var extraHeaderLength = Header.FStart - wzWriter.BaseStream.Position;
					if (extraHeaderLength > 0) wzWriter.Write(new byte[(int) extraHeaderLength]);

					if (!saveAs64BitWz) // 64 bit doesnt have a version number.
					{
						wzWriter.Write(wzVersionHeader);
					}

					wzWriter.Header = Header;
					wzDir.SaveDirectory(wzWriter);
					wzWriter.StringCache.Clear();

					using (var fs = File.OpenRead(tempFile)) {
						wzDir.SaveImages(wzWriter, fs);
					}

					File.Delete(tempFile);

					wzWriter.StringCache.Clear();
				}
			} finally {
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}

		public void ExportXml(string path, bool oneFile) {
			if (oneFile) {
				var fs = File.Create(path + "/" + name + ".xml");
				var writer = new StreamWriter(fs);

				var level = 0;
				writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzFile", name, true));
				wzDir.ExportXml(writer, oneFile, level, false);
				writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.CloseTag("WzFile"));

				writer.Close();
			} else {
				throw new Exception("Under Construction");
			}
		}

		/// <summary>
		/// Returns an array of objects from a given path. Wild cards are supported
		/// For example :
		/// GetObjectsFromPath("Map.wz/Map0/*");
		/// Would return all the objects (in this case images) from the sub directory Map0
		/// </summary>
		/// <param name="path">The path to the object(s)</param>
		/// <returns>An array of IWzObjects containing the found objects</returns>
		public List<WzObject> GetObjectsFromWildcardPath(string path) {
			if (path.ToLower() == name.ToLower()) {
				return new List<WzObject> {WzDirectory};
			}

			if (path == "*") {
				var fullList = new List<WzObject> {WzDirectory};
				fullList.AddRange(GetObjectsFromDirectory(WzDirectory));
				return fullList;
			}

			if (!path.Contains("*")) {
				return new List<WzObject> {GetObjectFromPath(path)};
			}

			var seperatedNames = path.Split("/".ToCharArray());
			if (seperatedNames.Length == 2 && seperatedNames[1] == "*") {
				return GetObjectsFromDirectory(WzDirectory);
			}

			// Use Linq to flatten the sequence of paths returned by the GetPathsFromImage and GetPathsFromDirectory methods
			// and filter the paths that match the given wildcard pattern
			var objList = WzDirectory.WzImages.SelectMany(img => GetPathsFromImage(img, name + "/" + img.Name))
				.Concat(wzDir.WzDirectories.SelectMany(dir => GetPathsFromDirectory(dir, name + "/" + dir.Name)))
				.Where(spath => StringMatch(path, spath)) // filter the paths that match the pattern
				.Select(spath => GetObjectFromPath(spath)) // convert the filtered paths into WzObjects
				.ToList();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			return objList;
		}

		public List<WzObject> GetObjectsFromRegexPath(string path) {
			if (path.ToLower() == name.ToLower()) {
				return new List<WzObject> {WzDirectory};
			}

			// Use Linq to flatten the sequence of paths returned by the GetPathsFromImage and GetPathsFromDirectory methods
			// and filter the paths that match the given regular expression
			var objList = WzDirectory.WzImages.SelectMany(img => GetPathsFromImage(img, name + "/" + img.Name))
				.Concat(wzDir.WzDirectories.SelectMany(dir => GetPathsFromDirectory(dir, name + "/" + dir.Name)))
				.Where(spath => Regex.Match(spath, path).Success)
				.Select(spath => GetObjectFromPath(spath)) // convert the filtered paths into WzObjects
				.ToList();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			return objList;
		}

		public List<WzObject> GetObjectsFromDirectory(WzDirectory dir) {
			// Create a list to store the objects
			var objList = new List<WzObject>();

			// Get the objects from the WzImages in the directory
			// and add them to the list
			objList.AddRange(dir.WzImages.SelectMany(img => GetObjectsFromImage(img)));

			// Get the objects from the WzDirectories in the directory
			// and add them to the list
			objList.AddRange(dir.WzDirectories.SelectMany(subdir => GetObjectsFromDirectory(subdir)));

			// Return the list of objects
			return objList;
		}

		public List<WzObject> GetObjectsFromImage(WzImage img) {
			// Use Linq to flatten the sequence of WzObjects returned by the GetObjectsFromProperty method
			// and convert the results into a List<WzObject>
			var objList = img.WzProperties.SelectMany(prop => {
				var objects = new List<WzObject> {prop}; // initialize the list with the current property
				objects.AddRange(GetObjectsFromProperty(prop)); // add the objects from the property
				return objects; // return the list of objects
			}).ToList();

			return objList;
		}

		public List<WzObject> GetObjectsFromProperty(WzImageProperty prop) {
			var objList = new List<WzObject>();
			var subProperties = new List<WzImageProperty>();

			var bAddRange = true;
			switch (prop.PropertyType) {
				case WzPropertyType.Canvas:
					subProperties = ((WzCanvasProperty) prop).WzProperties;
					objList.Add(((WzCanvasProperty) prop).PngProperty);
					break;
				case WzPropertyType.Convex:
					subProperties = ((WzConvexProperty) prop).WzProperties;
					break;
				case WzPropertyType.SubProperty:
					subProperties = ((WzSubProperty) prop).WzProperties;
					break;
				case WzPropertyType.Vector:
					objList.Add(((WzVectorProperty) prop).X);
					objList.Add(((WzVectorProperty) prop).Y);
					bAddRange = false;
					break;
			}

			if (bAddRange) {
				objList.AddRange(subProperties.SelectMany(p => GetObjectsFromProperty(p)));
			}

			return objList;
		}

		internal List<string> GetPathsFromDirectory(WzDirectory dir, string curPath) {
			// Use Linq to flatten the sequence of paths returned by the GetPathsFromImage and GetPathsFromDirectory methods
			// and convert the results into a List<string>
			var objList = dir.WzImages.SelectMany(img => {
				var paths = new List<string> {curPath + "/" + img.Name}; // initialize the list with the current path
				paths.AddRange(GetPathsFromImage(img, curPath + "/" + img.Name)); // add the paths from the image
				return paths; // return the list of paths
			}).Concat(dir.WzDirectories.SelectMany(subdir => {
				var paths = new List<string> {curPath + "/" + subdir.Name}; // initialize the list with the current path
				paths.AddRange(GetPathsFromDirectory(subdir,
					curPath + "/" + subdir.Name)); // add the paths from the subdirectory
				return paths; // return the list of paths
			})).ToList();

			return objList;
		}


		internal List<string> GetPathsFromImage(WzImage img, string curPath) {
			// Use Linq to flatten the sequence of paths returned by the GetPathsFromProperty method
			// and convert the results into a List<string>
			var objList = img.WzProperties.SelectMany(prop => {
				var paths = new List<string> {curPath + "/" + prop.Name}; // initialize the list with the current path
				paths.AddRange(GetPathsFromProperty(prop,
					curPath + "/" + prop.Name)); // add the paths from the property
				return paths; // return the list of paths
			}).ToList();

			return objList;
		}

		internal List<string> GetPathsFromProperty(WzImageProperty prop, string curPath) {
			var objList = new List<string>();
			var subProperties = new List<WzImageProperty>();

			var bAddRange = true;
			switch (prop.PropertyType) {
				case WzPropertyType.Canvas:
					subProperties = ((WzCanvasProperty) prop).WzProperties;
					objList.Add(curPath + "/PNG");
					break;
				case WzPropertyType.Convex:
					subProperties = ((WzConvexProperty) prop).WzProperties;
					break;
				case WzPropertyType.SubProperty:
					subProperties = ((WzSubProperty) prop).WzProperties;
					break;
				case WzPropertyType.Vector:
					objList.Add(curPath + "/X");
					objList.Add(curPath + "/Y");
					bAddRange = false;
					break;
			}

			if (bAddRange) {
				objList.AddRange(subProperties.SelectMany(p => GetPathsFromProperty(p, curPath + "/" + p.Name)));
			}

			return objList;
		}

		/// <summary>
		/// Get WZ objects from path
		/// </summary>
		/// <param name="path"></param>
		/// <param name="lookupOtherOpenedWzFile"></param>
		/// <returns></returns>
		public WzObject GetObjectFromPath(string path, bool checkFirstDirectoryName = true) {
			var seperatedPath = path.Split("/".ToCharArray());
			if (seperatedPath.Length == 1) {
				return WzDirectory;
			}

			WzObject checkObjInOtherWzFile = null;

			if (checkFirstDirectoryName) {
				if (WzFileManager.fileManager != null) {
					// Use FirstOrDefault() and Any() to find the first matching WzDirectory
					// and check if there are any matching WzDirectory in the list
					var wzDir = WzFileManager.fileManager.GetWzDirectoriesFromBase(seperatedPath[0])
						.FirstOrDefault(
							dir => dir.name.ToLower() == seperatedPath[0].ToLower() ||
							       dir.name.Substring(0, dir.name.Length - 3).ToLower() == seperatedPath[0].ToLower());
					if (wzDir == null && seperatedPath.Length >= 1) {
						checkObjInOtherWzFile =
							WzFileManager.fileManager.FindWzImageByName(seperatedPath[0],
								seperatedPath[1]); // Map/xxx.img

						if (checkObjInOtherWzFile == null && seperatedPath.Length >= 2) // Map/Obj/xxx.img -> Obj.wz
						{
							checkObjInOtherWzFile = WzFileManager.fileManager.FindWzImageByName(
								seperatedPath[0] + Path.DirectorySeparatorChar + seperatedPath[1], seperatedPath[2]);
							if (checkObjInOtherWzFile == null) {
								return null;
							}

							seperatedPath = seperatedPath.Skip(2).ToArray();
						} else {
							seperatedPath = seperatedPath.Skip(1).ToArray();
						}
					} else {
						return null;
					}
				} else {
					return null;
				}
			}

			var curObj = checkObjInOtherWzFile ?? WzDirectory;
			if (curObj == null) {
				return null;
			}

			var bFirst = true;
			foreach (var pathPart in seperatedPath) {
				if (bFirst) {
					bFirst = false;
					continue;
				}

				if (curObj == null) {
					return null;
				}

				switch (curObj.ObjectType) {
					case WzObjectType.Directory:
						curObj = ((WzDirectory) curObj)[pathPart];
						continue;
					case WzObjectType.Image:
						curObj = ((WzImage) curObj)[pathPart];
						continue;
					case WzObjectType.Property:
						switch (((WzImageProperty) curObj).PropertyType) {
							case WzPropertyType.Canvas:
								curObj = ((WzCanvasProperty) curObj)[pathPart];
								continue;
							case WzPropertyType.Convex:
								curObj = ((WzConvexProperty) curObj)[pathPart];
								continue;
							case WzPropertyType.SubProperty:
								curObj = ((WzSubProperty) curObj)[pathPart];
								continue;
							case WzPropertyType.Vector:
								if (pathPart == "X") {
									return ((WzVectorProperty) curObj).X;
								}

								if (pathPart == "Y") {
									return ((WzVectorProperty) curObj).Y;
								}

								return null;
							default: // Wut?
								return null;
						}
				}
			}

			if (curObj == null) return null;

			return curObj;
		}

		/// <summary>
		/// Get WZ object from multiple loaded WZ files in memory
		/// </summary>
		/// <param name="path"></param>
		/// <param name="wzFiles"></param>
		/// <returns></returns>
		public static WzObject GetObjectFromMultipleWzFilePath(string path, IReadOnlyCollection<WzFile> wzFiles) {
			// Use Select() and FirstOrDefault() to transform and find the first matching WzObject
			return wzFiles.Select(file => file.GetObjectFromPath(path, false))
				.FirstOrDefault(obj => obj != null);
		}


		internal bool StringMatch(string strWildCard, string strCompare) {
			var wildCardLength = strWildCard.Length;
			var compareLength = strCompare.Length;
			var wildCardIndex = 0;
			var compareIndex = 0;

			while (wildCardIndex < wildCardLength && compareIndex < compareLength) {
				if (strWildCard[wildCardIndex] == '*') {
					// If there are multiple * in the wildcard, move to the last *
					while (wildCardIndex < wildCardLength && strWildCard[wildCardIndex] == '*') wildCardIndex++;

					// If there are no characters left in the wildcard, return true
					if (wildCardIndex == wildCardLength) return true;

					// Try to match the remaining part of the wildcard with the remaining part of the compare string
					// starting from the current compare index.
					while (compareIndex < compareLength) {
						if (StringMatch(strWildCard.Substring(wildCardIndex), strCompare.Substring(compareIndex))) {
							return true;
						}

						compareIndex++;
					}

					// If we reached here, it means the remaining part of the wildcard could not be matched
					// with the remaining part of the compare string, so return false.
					return false;
				}

				if (strWildCard[wildCardIndex] == strCompare[compareIndex]) {
					wildCardIndex++;
					compareIndex++;
				} else {
					// If the current characters do not match and the wildcard character is not a *,
					// return false.
					return false;
				}
			}

			// If we reached here, it means one of the strings has been fully processed.
			// If both strings have been fully processed, return true, else return false.
			return wildCardIndex == wildCardLength && compareIndex == compareLength;
		}

		public override void Remove() {
			Dispose();
		}

		public List<string> ListWzEntries = new List<string>();

		public bool LoadListWz(string file) {
			return ListWzContainerImpl.LoadListWz(ListWzEntries, WzIv, UserKey, file);
		}

		public bool ListWzContains(string wzName, string wzEntry) {
			return ListWzContainerImpl.ListWzContains(ListWzEntries, wzName, wzEntry);
		}
	}
}