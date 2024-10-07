using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using HaRepacker.GUI.Panels;
using MapleLib.Helpers;
using MapleLib.WzLib;
using MapleLib.WzLib.Serialization;
using MapleLib.WzLib.Util;

namespace HaRepacker {
	public static class CopyModeMethods {
		private static readonly List<WzObject> clipboard = [];
		private static PathCopy? pathCopy;

		public static void DoCopy(this CopyMode mode, MainPanel panel, List<WzNode> selectedNodes) {
			if (mode.Equals(CopyMode.Object)) {
				foreach (var obj in clipboard)
					//this causes minor weirdness with png's in copied nodes but otherwise memory is not free'd
				{
					obj.Dispose();
				}

				clipboard.Clear();

				foreach (var node in selectedNodes) {
					var wzObj = (WzObject) node.Tag;
					clipboard.Add(wzObj);
				}
			} else if (mode.Equals(CopyMode.Path)) {
				pathCopy = new PathCopy {
					mainPanel = panel
				};
				foreach (var node in selectedNodes) {
					var obj = node.Tag as WzObject;
					var wzFile = obj.WzFileParent;
					var fixedPath = obj.FullPath.Replace($"{wzFile.Name}/", "");
					pathCopy.paths.Add($"{wzFile.FilePath},{fixedPath}");
				}
			} else if (mode.Equals(CopyMode.XmlClipboard)) {
				var serializer = new WzCopyXmlSerializer(
					Program.ConfigurationManager.UserSettings.Indentation,
					Program.ConfigurationManager.UserSettings.LineBreakType);
				using var stream = new MemoryStream();
				using TextWriter tw = new StreamWriter(stream);
				var context = new SerializationContext {
					Writer = tw
				};
				foreach (var node in selectedNodes) {
					var wzObj = (WzObject) node.Tag;
					serializer.Serialize(wzObj, context);
				}

				tw.Flush();
				Clipboard.SetText(Encoding.UTF8.GetString(stream.ToArray()));
			}
		}

		public static void DoPaste(this CopyMode mode, MainPanel panel, List<WzNode> selectedNodes) {
			// Safe to reuse this in a loop of nodes right..?
			var handler = panel.MainForm.CreateDefaultDuplicateHandler();
			if (mode.Equals(CopyMode.Object)) {
				foreach (var parent in selectedNodes) {
					var parentObj = (WzObject) parent.Tag;

					if (parent.Tag is WzImage && parent.Nodes.Count == 0) {
						TreeViewMS.ParseOnDataTreeSelectedItem(parent); // only parse the main node.
					}

					if (parentObj is WzFile file) {
						parentObj = file.WzDirectory;
					}

					foreach (var obj in clipboard) {
						if (!CreateClone(obj, out var clone)) {
							continue;
						}

						PasteObject(panel, clone, parentObj, handler, parent);
					}
				}
			} else if (mode.Equals(CopyMode.Path)) {
				if (pathCopy == null) {
					return;
				}

				var tree = pathCopy.mainPanel.DataTree;
				foreach (WzNode parentNode in tree.Items) {
					if (parentNode.Tag is not WzFile wzFile) {
						continue;
					}

					foreach (var path in pathCopy.paths) {
						var split = path.Split(',');
						if (!split[0].Equals(wzFile.FilePath)) {
							continue;
						}

						var obj = wzFile.GetFromPath(split[1]);
						if (obj == null) {
							continue;
						}

						foreach (var parent in selectedNodes) {
							var parentObj = (WzObject) parent.Tag;

							if (parent.Tag is WzImage && parent.Nodes.Count == 0) {
								TreeViewMS.ParseOnDataTreeSelectedItem(parent); // only parse the main node.
							}

							if (parentObj is WzFile file) {
								parentObj = file.WzDirectory;
							}

							if (!CreateClone(obj, out var clone)) {
								continue;
							}

							PasteObject(panel, clone, parentObj, handler, parent);
						}
					}
				}
			} else if (mode.Equals(CopyMode.XmlClipboard)) {
				// hmm
			}
		}

		private static void PasteObject(MainPanel panel, WzObject obj, WzObject parentObj, DuplicateHandler handler, WzNode parent) {
			if (((!(obj is WzDirectory) && !(obj is WzImage)) || !(parentObj is WzDirectory)) &&
			    (!(obj is WzImageProperty) || !(parentObj is IPropertyContainer))) {
				return;
			}

			var clone = CloneWzObject(obj);
			if (clone == null) {
				return;
			}

			if (clone is WzImage image) {
				// Cloning also clones wz key
				// But they could now be different, update it
				image.wzKey = WzKeyGenerator.GenerateWzKey(WzTool.GetIvByMapleVersion(parentObj.WzFileParent.MapleVersion));
			}

			panel.MainForm.AddObjectWithNode(handler, parent, clone);
		}

		private static bool CreateClone(WzObject wzObj, out WzObject? clone) {
			clone = CloneWzObject(wzObj);
			if (clone == null) {
				return false;
			}

			if (clone is WzImage image) {
				// Decrypt any List.wz entries so that we paste them as decrypted
				// Otherwise we need to know the key to decrypt them on paste
				ListWzContainerImpl.MarkListWzProperty(image, false);
			} else if (clone is WzImageProperty prop) {
				// No parent image so we need to grab it from source
				ListWzContainerImpl.MarkListWzProperty(prop, false, ((WzImageProperty) wzObj).ParentImage.wzKey);
			} // Can't copy directories atm so they don't need handling

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static WzObject? CloneWzObject(WzObject obj) {
			switch (obj) {
				case WzDirectory dir:
					return dir.DeepClone();
				case WzImage image:
					return image.DeepClone();
				case WzImageProperty property:
					return property.DeepClone();
				default:
					ErrorLogger.Log(ErrorLevel.MissingFeature,
						"The current WZ object type cannot be cloned " + obj + " " + obj.FullPath);
					return null;
			}
		}

		public class PathCopy {
			public MainPanel mainPanel;
			public List<string> paths = [];

			public string? GetFromWzPath(string wzPath) {
				foreach (var path in paths) {
					var split = path.Split(',');
					if (split[0].Equals(path)) {
						return split[1];
					}
				}

				return null;
			}
		}
	}
}