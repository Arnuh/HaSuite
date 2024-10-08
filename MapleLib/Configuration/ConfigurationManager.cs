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

using System;
using System.IO;
using MapleLib.MapleCryptoLib;
using MapleLib.PacketLib;
using Newtonsoft.Json;

namespace MapleLib.Configuration {
	public class ConfigurationManager {
		private const string SETTINGS_FILE_USER = "Settings.txt";
		private const string SETTINGS_FILE_APPLICATION = "ApplicationSettings.txt";
		public const string configPipeName = "PheRepacker";

		private string folderPath;

		private UserSettings _userSettings = new UserSettings(); // default configuration for UI designer :( 

		public UserSettings UserSettings {
			get => _userSettings;
			private set { }
		}

		private ApplicationSettings
			_appSettings = new ApplicationSettings(); // default configuration for UI designer :( 

		public ApplicationSettings ApplicationSettings {
			get => _appSettings;
			private set { }
		}

		private bool isLoaded;

		/// <summary>
		/// Constructor
		/// </summary>
		public ConfigurationManager() {
			folderPath = GetLocalFolderPath();
		}

		/// <summary>
		/// Gets the local folder path
		/// </summary>
		/// <returns></returns>
		public static string GetLocalFolderPath() {
			var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var our_folder = Path.Combine(appdata, configPipeName);
			if (!Directory.Exists(our_folder)) {
				Directory.CreateDirectory(our_folder);
			}

			return our_folder;
		}

		public static string GetNewLocalFolderPath() {
			var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var ourFolder = Path.Combine(appdata, "HaSuite");
			if (!Directory.Exists(ourFolder)) {
				Directory.CreateDirectory(ourFolder);
			}

			return ourFolder;
		}

		/// <summary>
		/// Load application setting from user application data 
		/// </summary>
		/// <returns></returns>
		public bool Load() {
			var userFilePath = Path.Combine(folderPath, SETTINGS_FILE_USER);
			var applicationFilePath = Path.Combine(folderPath, SETTINGS_FILE_APPLICATION);

			if (File.Exists(userFilePath) && File.Exists(applicationFilePath)) {
				var userFileContent = File.ReadAllText(userFilePath);
				var applicationFileContent = File.ReadAllText(applicationFilePath);

				try {
					_userSettings =
						JsonConvert.DeserializeObject<UserSettings>(
							userFileContent); // deserialize to static content... 
					_appSettings = JsonConvert.DeserializeObject<ApplicationSettings>(applicationFileContent);
					isLoaded = true;
					return true;
				} catch (Exception) {
					// delete all
					try {
						File.Delete(userFilePath);
						File.Delete(applicationFilePath);
					} catch {
					} // throws if it cant access without admin
				}
			}

			_userSettings = new UserSettings(); // defaults
			_appSettings = new ApplicationSettings();
			return false;
		}

		/// <summary>
		/// Saves setting to user application data
		/// </summary>
		/// <returns></returns>
		public bool Save() {
			var userSettingsSerialised =
				JsonConvert.SerializeObject(_userSettings, Formatting.Indented); // format for user
			var appSettingsSerialised = JsonConvert.SerializeObject(_appSettings, Formatting.Indented);

			var userFilePath = Path.Combine(folderPath, SETTINGS_FILE_USER);
			var applicationFilePath = Path.Combine(folderPath, SETTINGS_FILE_APPLICATION);

			try {
				// user setting
				using (var file = new StreamWriter(userFilePath)) {
					file.Write(userSettingsSerialised);
				}

				// app setting
				using (var file = new StreamWriter(applicationFilePath)) {
					file.Write(appSettingsSerialised);
				}

				return true;
			} catch {
			}

			return false;
		}


		/// <summary>
		/// Gets the custom WZ IV from settings
		/// </summary>
		/// <returns></returns>
		public byte[] GetCusomWzIVEncryption() {
			if (!isLoaded) {
				if (!Load()) {
					return new byte[4] {0x0, 0x0, 0x0, 0x0}; // fallback with BMS
				}
			}

			var storedCustomEnc = ApplicationSettings.MapleVersion_CustomEncryptionBytes;
			var bytes = HexEncoding.GetBytes(storedCustomEnc);

			if (ValidateCustomWzIVEncryption(bytes)) return bytes;
			return new byte[4] {0x0, 0x0, 0x0, 0x0}; // fallback with BMS
		}

		public bool ValidateCustomWzIVEncryption(byte[] iv) {
			return iv.Length == 4;
		}

		public byte[] GetCustomWzUserKeyFromConfig() {
			var UserKey_WzLib = new byte[128];
			if (!isLoaded) {
				if (!Load()) {
					return UserKey_WzLib;
				}
			}

			var bytes = HexEncoding.GetBytes(ApplicationSettings.MapleVersion_CustomAESUserKey);
			if (!ValidateCustomWzUserKey(bytes)) {
				return UserKey_WzLib;
			}

			UserKey_WzLib = new byte[MapleCryptoConstants.MAPLESTORY_USERKEY_DEFAULT.Length];
			for (var i = 0; i < UserKey_WzLib.Length; i += 4) {
				UserKey_WzLib[i] = bytes[i / 4];
				UserKey_WzLib[i + 1] = 0;
				UserKey_WzLib[i + 2] = 0;
				UserKey_WzLib[i + 3] = 0;
			}

			return UserKey_WzLib;
		}

		public bool ValidateCustomWzUserKey(byte[] key) {
			return key.Length != 0;
		}
	}
}