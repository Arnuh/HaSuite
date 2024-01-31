using System.Collections.Generic;
using System.IO;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib {
	public interface ListWzContainer {
		bool LoadListWz(string file);

		bool ListWzContains(string wzName, string wzEntry);
	}

	public class ListWzContainerImpl {
		public static bool LoadListWz(List<string> ListWzEntries, byte[] WzIv, byte[] UserKey, string file) {
			if (!File.Exists(file)) return false;
			ListWzEntries.Clear();
			ListWzEntries.AddRange(ListFileParser.ParseListFile(file, WzIv, UserKey));
			return true;
		}

		public static bool ListWzContains(List<string> ListWzEntries, string wzName, string wzEntry) {
			wzEntry = wzEntry.Replace("\\", "/").ToLower();
			wzEntry = wzEntry.Replace(".wz/", "/");
			if (string.IsNullOrEmpty(wzName)) return ListWzEntries.Contains(wzEntry);
			// Strips anything that isn't the base wz name
			// Since I think.... those aren't needed
			wzName = WzFileManager.CleanWzName(wzName);
			if (!wzEntry.StartsWith(wzName)) {
				return ListWzEntries.Contains(wzName + "/" + wzEntry);
			}

			//wzEntry = wzEntry.Replace($"{wzName}.wz", wzName);
			return ListWzEntries.Contains(wzEntry);
		}

		public static void MarkListWzProperty(WzImage image, WzFile wzFile, string overrideFullPath = null) {
			MarkListWzProperty(image, wzFile.ListWzEntries, wzFile.Name, overrideFullPath);
		}

		public static void MarkListWzProperty(WzImage image, List<string> ListWzEntries, string wzName, string overrideFullPath = null) {
			var listWz = ListWzContains(ListWzEntries, wzName, overrideFullPath ?? image.FullPath);
			foreach (var prop in image.WzProperties) {
				MarkListWzProperty(prop, listWz);
			}
		}

		public static void MarkListWzProperty(WzImage image, bool listWz) {
			foreach (var prop in image.WzProperties) {
				MarkListWzProperty(prop, listWz);
			}
		}

		public static void MarkListWzProperty(WzImageProperty parent, bool listWz, WzMutableKey wzKey = null) {
			if (parent.WzProperties == null) return;
			foreach (var prop in parent.WzProperties) {
				if (prop is WzCanvasProperty canvas) {
					canvas.PngProperty.ConvertCompressed(parent.ParentImage?.wzKey ?? wzKey, listWz ? parent.ParentImage?.wzKey ?? wzKey : null, !listWz);
				}

				MarkListWzProperty(prop, listWz);
			}
		}
	}
}