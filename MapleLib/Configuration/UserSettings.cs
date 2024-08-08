using MapleLib.WzLib.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MapleLib.Configuration {
	public class UserSettings {
		public enum UserSettingsThemeColor {
			Dark = 0,
			Light = 1
		}

		[JsonProperty(PropertyName = "Indentation")]
		public int Indentation = 4;

		[JsonProperty(PropertyName = "LineBreakType")] [JsonConverter(typeof(StringEnumConverter))]
		public LineBreak LineBreakType = LineBreak.Windows;

		[JsonProperty(PropertyName = "DefaultXmlFolder")]
		public string DefaultXmlFolder = "";

		[JsonProperty(PropertyName = "UseApngIncompatibilityFrame")]
		public bool UseApngIncompatibilityFrame = true;

		[JsonProperty(PropertyName = "AutoAssociate")]
		public bool AutoAssociate = true;

		[JsonProperty(PropertyName = "Sort")] public bool Sort = true;

		[JsonProperty(PropertyName = "QuickEdit")]
		public bool QuickEdit;

		[JsonProperty(PropertyName = "SuppressWarnings")]
		public bool SuppressWarnings;

		[JsonProperty(PropertyName = "ParseImagesInSearch")]
		public bool ParseImagesInSearch;

		[JsonProperty(PropertyName = "SearchStringValues")]
		public bool SearchStringValues = true;

		// Animate
		[JsonProperty(PropertyName = "DevImgSequences")]
		public bool DevImgSequences;

		[JsonProperty(PropertyName = "CartesianPlane")]
		public bool CartesianPlane = true;

		[JsonProperty(PropertyName = "DelayNextLoop")]
		public int DelayNextLoop = 60;

		[JsonProperty(PropertyName = "PlanePosition")]
		public string PlanePosition = "Center";

		// Themes
		[JsonProperty(PropertyName = "ThemeColor")]
		public int ThemeColor = (int) UserSettingsThemeColor.Light; //white = 1, black = 0


		// Settings not shown on the settings page
		[JsonProperty(PropertyName = "EnableCrossHairDebugInformation")]
		public bool EnableCrossHairDebugInformation = true;

		[JsonProperty(PropertyName = "EnableBorderDebugInformation")]
		public bool EnableBorderDebugInformation = true;

		[JsonProperty(PropertyName = "ImageZoomLevel")]
		public double ImageZoomLevel = 3.0f;

		[JsonProperty(PropertyName = "AutoloadRelatedWzFiles")]
		public bool AutoloadRelatedWzFiles;

		[JsonProperty(PropertyName = "PreviousLoadFolder")]
		public string PreviousLoadFolder = "";
	}
}