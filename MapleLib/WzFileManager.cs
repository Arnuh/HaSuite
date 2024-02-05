using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using MapleLib.Helpers;
using MapleLib.WzLib;

namespace MapleLib {
	public class WzFileManager : IDisposable {
		#region Constants

		private static readonly string[] EXCLUDED_DIRECTORY_FROM_WZ_LIST =
			{"bak", "backup", "original", "xml", "hshield", "blackcipher", "harepacker", "hacreator", "xml"};

		public static readonly string[] COMMON_MAPLESTORY_DIRECTORY = {
			@"C:\Nexon\MapleStory",
			@"D:\Nexon\Maple",
			@"C:\Program Files\WIZET\MapleStory",
			@"C:\MapleStory",
			@"C:\Program Files (x86)\Wizet\MapleStorySEA"
		};

		#endregion

		#region Fields

		public static WzFileManager fileManager; // static, to allow access from anywhere

		private readonly string baseDir;

		/// <summary>
		/// Gets the base directory of the WZ file.
		/// Returns the "Data" folder if 64-bit client.
		/// </summary>
		/// <returns></returns>
		public string WzBaseDirectory {
			get => _bInitAs64Bit ? baseDir + "\\Data\\" : baseDir;
			private set { }
		}

		private readonly bool _bInitAs64Bit;

		public bool Is64Bit {
			get => _bInitAs64Bit;
			private set { }
		}

		private readonly bool _bIsPreBBDataWzFormat;

		/// <summary>
		/// Defines if the currently loaded WZ directory are in the pre-BB format with only Data.wz (beta version?)
		/// </summary>
		public bool IsPreBBDataWzFormat {
			get => _bIsPreBBDataWzFormat;
			private set { }
		}


		private readonly ReaderWriterLockSlim
			_readWriteLock =
				new ReaderWriterLockSlim(); // for '_wzFiles', '_wzFilesUpdated', '_updatedImages', & '_wzDirs'

		private readonly Dictionary<string, WzFile> _wzFiles = new Dictionary<string, WzFile>();

		private readonly Dictionary<WzFile, bool>
			_wzFilesUpdated =
				new Dictionary<WzFile, bool>(); // key = WzFile, flag for the list of WZ files changed to be saved later via Repack 

		private readonly HashSet<WzImage> _updatedWzImages = new HashSet<WzImage>();
		private readonly Dictionary<string, WzMainDirectory> _wzDirs = new Dictionary<string, WzMainDirectory>();


		/// <summary>
		/// The list of sub wz files.
		/// Key, <List of files, directory path>
		/// i.e sound.wz expands to the list array of "Mob001", "Mob2"
		/// 
		/// {[Map\Map\Map4, Count = 1]}
		/// </summary>
		private readonly Dictionary<string, List<string>> _wzFilesList = new Dictionary<string, List<string>>();

		/// <summary>
		/// The list of directory where the wz file residues
		/// </summary>
		private readonly Dictionary<string, string> _wzFilesDirectoryList = new Dictionary<string, string>();

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor to init WzFileManager for HaRepacker
		/// </summary>
		public WzFileManager() {
			baseDir = string.Empty;
			_bInitAs64Bit = false;

			fileManager = this;
		}

		/// <summary>
		/// Constructor to init WzFileManager for HaCreator
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="bInitAs64Bit"></param>
		/// <param name="bIsPreBBDataWzFormat"></param>
		public WzFileManager(string directory, bool bInitAs64Bit, bool bIsPreBBDataWzFormat) {
			baseDir = directory;
			_bInitAs64Bit = bInitAs64Bit;
			_bIsPreBBDataWzFormat = bIsPreBBDataWzFormat;

			fileManager = this;
		}

		#endregion

		#region Loader

		/// <summary>
		/// Automagically detect if the following directory where MapleStory installation is saved
		/// is a 64-bit wz directory
		/// </summary>
		/// <returns></returns>
		public static bool Detect64BitDirectoryWzFileFormat(string baseDirectoryPath) {
			if (!Directory.Exists(baseDirectoryPath)) {
				throw new Exception("Non-existent directory provided.");
			}

			var dataDirectoryPath = Path.Combine(baseDirectoryPath, "Data");
			var bDirectoryContainsDataDir = Directory.Exists(dataDirectoryPath);

			if (bDirectoryContainsDataDir) {
				// Use a regular expression to search for .wz files in the Data directory
				var searchPattern = @"*.wz";
				var nNumWzFilesInDataDir = Directory
					.EnumerateFileSystemEntries(dataDirectoryPath, searchPattern, SearchOption.AllDirectories).Count();

				if (nNumWzFilesInDataDir > 40) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Automagically detect if the following directory where MapleStory installation is saved
		/// is a pre-bb WZ with only Data.wz
		/// </summary>
		/// <returns></returns>
		public static bool DetectIsPreBBDataWZFormat(string baseDirectoryPath) {
			if (!Directory.Exists(baseDirectoryPath)) {
				throw new Exception("Non-existent directory provided.");
			}

			// Check if the directory contains a "Data.wz" file
			var dataWzFilePath = Path.Combine(baseDirectoryPath, "Data.wz");
			var bDirectoryContainsDataWz = File.Exists(dataWzFilePath);
			if (bDirectoryContainsDataWz) {
				// Check if Skill.wz, String.wz, Character.wz exist in the base directory
				var skillWzFilePath = Path.Combine(baseDirectoryPath, "Skill.wz");
				var stringWzFilePath = Path.Combine(baseDirectoryPath, "String.wz");
				var characterWzFilePath = Path.Combine(baseDirectoryPath, "Character.wz");

				var skillWzExist = File.Exists(skillWzFilePath);
				var stringWzExist = File.Exists(stringWzFilePath);
				var characterWzExist = File.Exists(characterWzFilePath);

				if (!skillWzExist && !stringWzExist && !characterWzExist) {
					// Check if "Data" directory contains a "Character", "Skill", or "String" directory
					// to filter for 64-bit wz maplestory
					var skillDirectoryPath = Path.Combine(baseDirectoryPath, "Data", "Skill");
					var stringDirectoryPath = Path.Combine(baseDirectoryPath, "Data", "String");
					var characterDirectoryPath = Path.Combine(baseDirectoryPath, "Data", "Character");

					var skillDirExist = Directory.Exists(skillDirectoryPath);
					var stringDirExist = Directory.Exists(stringDirectoryPath);
					var characterDirExist = Directory.Exists(characterDirectoryPath);

					if (!skillDirExist && !stringDirExist && !characterDirExist) {
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Builds the list of WZ files in the MapleStory directory (for HaCreator only, not used for HaRepacker)
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public void BuildWzFileList() {
			var b64BitClient = _bInitAs64Bit;
			if (b64BitClient) {
				// parse through "Data" directory and iterate through every folder
				var baseDir = WzBaseDirectory;

				// Use Where() and Select() to filter and transform the directories
				var directories = Directory.EnumerateDirectories(baseDir, "*", SearchOption.AllDirectories)
					.Where(dir => !EXCLUDED_DIRECTORY_FROM_WZ_LIST.Any(x => dir.ToLower().Contains(x)));

				// Iterate over the filtered and transformed directories
				foreach (var dir in directories) {
					//string folderName = new DirectoryInfo(Path.GetDirectoryName(dir)).Name.ToLower();
					//Debug.WriteLine("----");
					//Debug.WriteLine(dir);

					var iniFiles = Directory.GetFiles(dir, "*.ini");
					if (iniFiles.Length <= 0 || iniFiles.Length > 1) {
						throw new Exception(".ini file at the directory '" + dir + "' is missing, or unavailable.");
					}

					var iniFile = iniFiles[0];
					if (!File.Exists(iniFile)) {
						throw new Exception(".ini file at the directory '" + dir + "' is missing.");
					}

					var iniFileLines = File.ReadAllLines(iniFile);
					if (iniFileLines.Length <= 0) {
						throw new Exception(".ini file does not contain LastWzIndex information.");
					}

					var iniFileSplit = iniFileLines[0].Split('|');
					if (iniFileSplit.Length <= 1) {
						throw new Exception(".ini file does not contain LastWzIndex information.");
					}

					var index = int.Parse(iniFileSplit[1]);

					for (var i = 0; i <= index; i++) {
						var partialWzFilePath =
							string.Format(iniFile.Replace(".ini", "_{0}.wz"), i.ToString("D3")); // 3 padding '0's
						var fileName = Path.GetFileName(partialWzFilePath);
						var fileName2 = fileName.Replace(".wz", "");

						var wzDirectoryNameOfWzFile = dir.Replace(baseDir, "").ToLower();

						if (EXCLUDED_DIRECTORY_FROM_WZ_LIST.Any(item => fileName2.ToLower().Contains(item))) {
							continue; // backup files
						}

						//Debug.WriteLine(partialWzFileName);
						//Debug.WriteLine(wzDirectoryOfWzFile);

						if (_wzFilesList.TryGetValue(wzDirectoryNameOfWzFile, out var fullPaths)) {
							fullPaths.Add(fileName2);
						} else {
							_wzFilesList.Add(wzDirectoryNameOfWzFile, new List<string> {fileName2});
						}

						if (!_wzFilesDirectoryList.ContainsKey(fileName2)) {
							_wzFilesDirectoryList.Add(fileName2, dir);
						}
					}
				}
			} else {
				// Don't look into subdirectories as it can cause conflicts if you use that as a backup folder
				// Some people might move wz files into a subdirectory for a cleaner root directory
				// but that is a custom case so I think I'll ignore for now and check TopDirectoryOnly
				var wzFileNames = Directory.EnumerateFileSystemEntries(baseDir, "*.wz", SearchOption.TopDirectoryOnly)
					.Where(f => !File.GetAttributes(f).HasFlag(FileAttributes.Directory) // exclude directories
					            && !EXCLUDED_DIRECTORY_FROM_WZ_LIST.Any(x =>
						            x.ToLower() ==
						            new DirectoryInfo(Path.GetDirectoryName(f)).Name)); // exclude folders
				foreach (var wzFileName in wzFileNames) {
					var directory = Path.GetDirectoryName(wzFileName);

					var fileName = Path.GetFileName(wzFileName);
					var fileName2 = fileName.Replace(".wz", "");
					if (fileName2.Contains("_BAK_")) {
						// HaCreator made backup files
						// Just ignore to avoid adding useless files
						continue;
					}

					// Mob2, Mob001, Map001, Map002
					// remove the numbers to get the base name 'map'
					var wzBaseFileName = new string(fileName2.ToLower().Where(c => char.IsLetter(c)).ToArray());

					// This should fix the issue noted at the top if AllDirectories is every used again
					// Checks if file is unchanged(besides lowercase) and if it already exists in the list
					// Should only be possible if the user has something like a backup folder
					if (wzBaseFileName.Equals(fileName2.ToLower()) && _wzFilesList.ContainsKey(wzBaseFileName)) {
						Debug.WriteLine($"Wz file {wzFileName} already exists in the list. Ignoring.");
						continue;
					}

					if (_wzFilesList.TryGetValue(wzBaseFileName, out var fullPaths)) {
						fullPaths.Add(fileName2);
					} else {
						_wzFilesList.Add(wzBaseFileName, new List<string> {fileName2});
					}

					if (!_wzFilesDirectoryList.ContainsKey(fileName2)) {
						_wzFilesDirectoryList.Add(fileName2, directory);
					}
				}
			}
		}

		/// <summary>
		/// Loads the oridinary WZ file
		/// </summary>
		/// <param name="baseName"></param>
		/// <param name="encVersion"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public WzFile LoadWzFile(string baseName, WzMapleVersion encVersion) {
			var filePath = GetWzFilePath(baseName);
			var wzf = new WzFile(filePath, encVersion);

			var parseStatus = wzf.ParseWzFile();
			if (parseStatus != WzFileParseStatus.Success) {
				throw new Exception("Error parsing " + baseName + ".wz (" + parseStatus.GetErrorDescription() + ")");
			}

			var fileName_ = baseName.ToLower().Replace(".wz", "");

			if (_wzFilesUpdated.ContainsKey(wzf)) // some safety check
			{
				throw new Exception(string.Format(
					"Wz {0} at the path {1} has already been loaded, and cannot be loaded again. Remove it from memory first.",
					fileName_, wzf.FilePath));
			}

			// write lock to begin adding to the dictionary
			_readWriteLock.EnterWriteLock();
			try {
				_wzFiles[fileName_] = wzf;
				_wzFilesUpdated[wzf] = false;
				_wzDirs[fileName_] = new WzMainDirectory(wzf);
			} finally {
				_readWriteLock.ExitWriteLock();
			}

			return wzf;
		}

		/// <summary>
		/// Loads the Data.wz file (Legacy MapleStory WZ before version 30)
		/// </summary>
		/// <param name="baseName"></param>
		/// <returns></returns>
		public bool LoadLegacyDataWzFile(string baseName, WzMapleVersion encVersion) {
			var filePath = GetWzFilePath(baseName);
			var wzf = new WzFile(filePath, encVersion);

			var parseStatus = wzf.ParseWzFile();
			if (parseStatus != WzFileParseStatus.Success) {
				MessageBox.Show("Error parsing " + baseName + ".wz (" + parseStatus.GetErrorDescription() + ")");
				return false;
			}

			baseName = baseName.ToLower();

			if (_wzFilesUpdated.ContainsKey(wzf)) // some safety check
			{
				throw new Exception(string.Format(
					"Wz file {0} at the path {1} has already been loaded, and cannot be loaded again.", baseName,
					wzf.FilePath));
			}

			// write lock to begin adding to the dictionary
			_readWriteLock.EnterWriteLock();
			try {
				_wzFiles[baseName] = wzf;
				_wzFilesUpdated[wzf] = false;
				_wzDirs[baseName] = new WzMainDirectory(wzf);
			} finally {
				_readWriteLock.ExitWriteLock();
			}

			foreach (var mainDir in wzf.WzDirectory.WzDirectories)
				_wzDirs[mainDir.Name.ToLower()] = new WzMainDirectory(wzf, mainDir);

			return true;
		}

		/// <summary>
		/// Loads the hotfix Data.wz file
		/// </summary>
		/// <param name="baseName"></param>
		/// <param name="encVersion"></param>
		/// <param name="panel"></param>
		/// <returns></returns>
		public WzImage LoadDataWzHotfixFile(string baseName, WzMapleVersion encVersion) {
			var filePath = GetWzFilePath(baseName);
			var
				fs = File.Open(filePath, FileMode.Open); // dont close this file stream until it is unloaded from memory

			var img = new WzImage(Path.GetFileName(filePath), fs, encVersion);
			img.ParseImage(true);

			return img;
		}

		#endregion

		#region Loaded Items

		/// <summary>
		/// Sets WZ file as updated for saving
		/// </summary>
		/// <param name="name"></param>
		/// <param name="img"></param>
		public void SetWzFileUpdated(string name, WzImage img) {
			img.Changed = true;
			_updatedWzImages.Add(img);

			var wzFile = GetMainDirectoryByName(name).File;
			SetWzFileUpdated(wzFile);
		}

		/// <summary>
		/// Sets WZ file as updated for saving
		/// </summary>
		/// <param name="wzFile"></param>
		/// <exception cref="Exception"></exception>
		public void SetWzFileUpdated(WzFile wzFile) {
			if (_wzFilesUpdated.ContainsKey(wzFile)) {
				// write lock to begin adding to the dictionary
				_readWriteLock.EnterWriteLock();
				try {
					_wzFilesUpdated[wzFile] = true;
				} finally {
					_readWriteLock.ExitWriteLock();
				}
			} else {
				throw new Exception("wz file to be flagged do not exist in memory " + wzFile.FilePath);
			}
		}

		/// <summary>
		/// Gets the list of updated or changed WZ files.
		/// </summary>
		/// <returns></returns>
		public List<WzFile> GetUpdatedWzFiles() {
			var updatedWzFiles = new List<WzFile>();
			// readlock
			_readWriteLock.EnterReadLock();
			try {
				foreach (var wzFileUpdated in _wzFilesUpdated) {
					if (wzFileUpdated.Value) {
						updatedWzFiles.Add(wzFileUpdated.Key);
					}
				}
			} finally {
				_readWriteLock.ExitReadLock();
			}

			return updatedWzFiles;
		}

		/// <summary>
		/// Unload the wz file from memory
		/// </summary>
		/// <param name="wzFile"></param>
		public void UnloadWzFile(WzFile wzFile, string wzFilePath) {
			// wzFilePath can be null when you make a new wz file and save it
			// This should be the only scenario as of making this check
			if (wzFilePath == null) {
				wzFile.Dispose();
				return;
			}
			var baseName = wzFilePath.ToLower().Replace(".wz", "");
			if (!_wzFiles.ContainsKey(baseName)) {
				return;
			}

			// write lock to begin adding to the dictionary
			_readWriteLock.EnterWriteLock();
			try {
				_wzFiles.Remove(baseName);
				_wzFilesUpdated.Remove(wzFile);
				_wzDirs.Remove(baseName);
			} finally {
				_readWriteLock.ExitWriteLock();
			}

			wzFile.Dispose();
		}

		#endregion

		#region Inherited Members

		/// <summary>
		/// Dispose when shutting down the application
		/// </summary>
		public void Dispose() {
			_readWriteLock.EnterWriteLock();
			try {
				foreach (var wzf in _wzFiles.Values) wzf.Dispose();

				_wzFiles.Clear();
				_wzFilesUpdated.Clear();
				_updatedWzImages.Clear();
				_wzDirs.Clear();
			} finally {
				_readWriteLock.ExitWriteLock();
			}
		}

		#endregion

		#region Custom Members

		public WzDirectory this[string name] =>
			_wzDirs.ContainsKey(name.ToLower())
				? _wzDirs[name.ToLower()].MainDir
				: null; //really not very useful to return null in this case

		/// <summary>
		/// Gets a read-only list of loaded WZ files in the WzFileManager
		/// </summary>
		/// <returns></returns>
		public ReadOnlyCollection<WzFile> WzFileList {
			get => new List<WzFile>(_wzFiles.Values).AsReadOnly();
			private set { }
		}

		/// <summary>
		/// Gets a read-only list of loaded WZ files in the WzFileManager
		/// </summary>
		/// <returns></returns>
		public ReadOnlyCollection<WzImage> WzUpdatedImageList {
			get => new List<WzImage>(_updatedWzImages).AsReadOnly();
			private set { }
		}

		#endregion

		#region Finder

		/// <summary>
		/// Gets WZ by name from the list of loaded files
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public WzMainDirectory GetMainDirectoryByName(string name) {
			name = name.ToLower();

			if (name.EndsWith(".wz")) {
				name = name.Replace(".wz", "");
			}

			return _wzDirs[name];
		}

		/// <summary>
		/// Get the list of sub wz files by its base name ("mob")
		/// i.e 'mob' expands to the list array of files "Mob001", "Mob2"
		/// exception: returns Data.wz regardless for pre-bb beta maplestory
		/// </summary>
		/// <param name="baseName"></param>
		/// <returns></returns>
		public List<string> GetWzFileNameListFromBase(string baseName) {
			if (_bIsPreBBDataWzFormat) {
				if (!_wzFilesList.ContainsKey("data")) {
					return new List<string>(); // return as an empty list if none
				}

				return _wzFilesList["data"];
			}

			if (!_wzFilesList.ContainsKey(baseName)) {
				return new List<string>(); // return as an empty list if none
			}

			return _wzFilesList[baseName];
		}

		/// <summary>
		/// Get the list of sub wz directories by its base name ("mob")
		/// </summary>
		/// <param name="baseName"></param>
		/// <returns></returns>
		public List<WzDirectory> GetWzDirectoriesFromBase(string baseName) {
			var wzDirs = GetWzFileNameListFromBase(baseName);
			// Use Select() and Where() to transform and filter the WzDirectory list
			if (_bIsPreBBDataWzFormat) {
				return wzDirs
					.Select(name => this["data"][baseName] as WzDirectory)
					.Where(dir => dir != null)
					.ToList();
			}

			return wzDirs
				.Select(name => this[name])
				.Where(dir => dir != null)
				.ToList();
		}

		/// <summary>
		/// Finds the wz image within the multiple wz files (by the base wz name)
		/// </summary>
		/// <param name="baseWzName"></param>
		/// <param name="imageName">Matches any if string.empty.</param>
		/// <returns></returns>
		public WzObject FindWzImageByName(string baseWzName, string imageName) {
			baseWzName = baseWzName.ToLower();

			var dirs = GetWzDirectoriesFromBase(baseWzName);
			// Use Where() and FirstOrDefault() to filter the WzDirectories and find the first matching WzObject
			var image = dirs
				.Where(wzFile => wzFile != null && wzFile[imageName] != null)
				.Select(wzFile => wzFile[imageName])
				.FirstOrDefault();

			return image;
		}

		/// <summary>
		/// Finds the wz image within the multiple wz files (by the base wz name)
		/// </summary>
		/// <param name="baseWzName"></param>
		/// <param name="imageName">Matches any if string.empty.</param>
		/// <returns></returns>
		public List<WzObject> FindWzImagesByName(string baseWzName, string imageName) {
			baseWzName = baseWzName.ToLower();

			var dirs = GetWzDirectoriesFromBase(baseWzName);
			// Use Where() and FirstOrDefault() to filter the WzDirectories and find the first matching WzObject
			return dirs
				.Where(wzFile => wzFile != null && wzFile[imageName] != null)
				.Select(wzFile => wzFile[imageName])
				.ToList();
		}

		/// <summary>
		/// Gets the wz file path by its base name, or check if it is a file path.
		/// </summary>
		/// <param name="filePathOrBaseFileName"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		private string GetWzFilePath(string filePathOrBaseFileName) {
			// find the base directory from 'wzFilesList'
			if (!_wzFilesDirectoryList.ContainsKey(
				    filePathOrBaseFileName)) // if the key is not found, it might be a path instead
			{
				if (File.Exists(filePathOrBaseFileName)) {
					return filePathOrBaseFileName;
				}

				throw new Exception("Couldnt find the directory key for the wz file " + filePathOrBaseFileName);
			}

			var fileName = StringUtility.CapitalizeFirstCharacter(filePathOrBaseFileName) + ".wz";
			var filePath = Path.Combine(_wzFilesDirectoryList[filePathOrBaseFileName], fileName);
			if (!File.Exists(filePath)) {
				throw new Exception("wz file at the path '" + filePathOrBaseFileName + "' does not exist.");
			}

			return filePath;
		}

		#endregion
	}
}