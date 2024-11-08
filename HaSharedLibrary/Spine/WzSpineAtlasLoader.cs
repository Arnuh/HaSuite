/*
 * Copyright (c) 2018~2020, LastBattle https://github.com/lastbattle
 * Copyright (c) 2010~2013, haha01haha http://forum.ragezone.com/f701/release-universal-harepacker-version-892005/

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.IO;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using spine_2._1._25_netcore;

namespace HaSharedLibrary.Spine {
	public class WzSpineAtlasLoader {
		/// <summary>
		/// Loads skeleton 
		/// </summary>
		/// <param name="atlasNode"></param>
		/// <param name="textureLoader"></param>
		/// <returns></returns>
		public static SkeletonData LoadSkeleton(WzStringProperty atlasNode, TextureLoader textureLoader) {
			var atlasData = atlasNode.GetString();
			if (string.IsNullOrEmpty(atlasData)) return null;

			var atlasReader = new StringReader(atlasData);

			var atlas = new Atlas(atlasReader, string.Empty, textureLoader);
			SkeletonData skeletonData;

			if (!TryLoadSkeletonJsonOrBinary(atlasNode, atlas, out skeletonData)) {
				atlas.Dispose();
				return null;
			}

			return skeletonData;
		}

		/// <summary>
		/// Load skeleton data by json or binary automatically
		/// </summary>
		/// <param name="atlasNode"></param>
		/// <param name="atlas"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		private static bool TryLoadSkeletonJsonOrBinary(WzImageProperty atlasNode, Atlas atlas, out SkeletonData data) {
			data = null;

			if (atlasNode == null || atlasNode.Parent == null || atlas == null) return false;

			var parent = atlasNode.Parent;

			List<WzImageProperty> childProperties;
			if (parent is WzImageProperty) {
				childProperties = ((WzImageProperty) parent).WzProperties;
			} else {
				childProperties = ((WzImage) parent).WzProperties;
			}


			if (childProperties != null) {
				var stringJsonProp =
					(WzStringProperty) childProperties.Where(child => child.Name.EndsWith(".json")).FirstOrDefault();

				if (stringJsonProp != null) // read json based 
				{
					var skeletonReader = new StringReader(stringJsonProp.GetString());
					var json = new SkeletonJson(atlas);
					data = json.ReadSkeletonData(skeletonReader);

					return true;
				}

				// try read binary based 
				foreach (var property in childProperties) {
					var linkedProperty = property.GetLinkedWzImageProperty();

					if (linkedProperty is WzSoundProperty soundProp) {
						using (var ms = new MemoryStream(soundProp.GetBytes(false))) {
							var skeletonBinary = new SkeletonBinary(atlas);
							data = skeletonBinary.ReadSkeletonData(ms);
							return true;
						}
					}
				}
			}

			return false;
		}
	}
}