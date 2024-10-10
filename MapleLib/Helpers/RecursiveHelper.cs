using MapleLib.WzLib;

namespace MapleLib.Helpers {
	public class RecursiveHelper {
		public static int CheckAllNodes(WzObject[] objects, Func<WzObject, bool> shouldCheckBelow, Func<WzObject, bool> check) {
			var nodes = 0;
			foreach (var obj in objects) {
				nodes += CheckRecursively(obj, shouldCheckBelow, check);
			}
			return nodes;
		}

		private static int CheckRecursively(WzObject obj, Func<WzObject, bool> shouldCheckBelow, Func<WzObject, bool> check) {
			if (obj is WzImage {Parsed: false} img) {
				img.ParseImage();
			}

			var count = 0;

			if (check(obj)) {
				++count;
			}

			if (!shouldCheckBelow(obj)) {
				return count;
			}

			if (obj is IPropertyContainer container) {
				var list = container.WzProperties;
				for (var i = list.Count - 1; i >= 0; i--) {
					count += CheckRecursively(list[i], shouldCheckBelow, check);
				}
			} else if (obj is WzFile file) {
				count += CheckRecursively(file.WzDirectory, shouldCheckBelow, check);
			} else if (obj is WzDirectory dir) {
				var imgList = dir.WzImages;
				for (var i = imgList.Count - 1; i >= 0; i--) {
					count += CheckRecursively(imgList[i], shouldCheckBelow, check);
				}

				var dirList = dir.WzDirectories;
				for (var i = dirList.Count - 1; i >= 0; i--) {
					count += CheckRecursively(dirList[i], shouldCheckBelow, check);
				}
			}

			return count;
		}
	}
}