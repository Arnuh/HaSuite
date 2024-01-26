﻿using MapleLib.WzLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MapleLib.Configuration {
	public class ApplicationSettings {
		#region Application Window

		[JsonProperty(PropertyName = "WindowMaximized")]
		public bool WindowMaximized;

		[JsonProperty(PropertyName = "WindowWidth")]
		public int Width = 1024;

		[JsonProperty(PropertyName = "WindowHeight")]
		public int Height = 768;

		#endregion

		#region Etc

		[JsonProperty(PropertyName = "FirstRun")]
		public bool FirstRun = true;

		[JsonProperty(PropertyName = "LastBrowserPath")]
		public string LastBrowserPath = "";

		#endregion

		#region Encryption

		/// <summary>
		/// The MapleStory encryption to use.
		/// </summary>
		[JsonProperty(PropertyName = "MapleStoryVersion")] [JsonConverter(typeof(StringEnumConverter))]
		public WzMapleVersion MapleVersion = WzMapleVersion.BMS;

		/// <summary>
		/// The custom AES user key to use when encrypting and decrypting WZ files
		/// </summary>
		[JsonProperty(PropertyName = "MapleStoryVersion_CustomAESUserKey")]
		public string MapleVersion_CustomAESUserKey = string.Empty; // str empty as default.

		/// <summary>
		/// The custom IV encryption bytes to use when encrypting and decrypting WZ files
		/// </summary>
		[JsonProperty(PropertyName = "MapleStoryVersion_EncryptionBytes")]
		public string MapleVersion_CustomEncryptionBytes = "0x00-0x00-0x00-0x00";

		#endregion

		#region API Key

		[JsonProperty(PropertyName = "OpenAI_ApiKey")]
		public string OpenAI_ApiKey = "";

		#endregion
	}
}