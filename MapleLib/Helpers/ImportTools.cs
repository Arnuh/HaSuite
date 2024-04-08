using System;
using System.IO;
using MapleLib.WzLib;
using MapleLib.WzLib.Serialization;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.Helpers {
	public static class ImportTools {
		public static void importFolder(DuplicateHandler handler, WzXmlDeserializer xmlDeserializer, WzImgDeserializer imgDeserializer, WzObject parent,
			string filePath) {
			var fileName = Path.GetFileName(filePath);
			if (fileName.Equals(parent.Name)) {
				importFolderContent(handler, xmlDeserializer, imgDeserializer, parent, filePath);
				return;
			}

			var folderIsDirectory = !(parent is WzImageProperty prop && prop.ParentImage != null);

			if (File.Exists(filePath)) {
				var extension = Path.GetExtension(filePath);
				if (extension.Equals(".img")) {
					var img = imgDeserializer.WzImageFromIMGFile(filePath, fileName, out var successful);
					if (!successful) {
						return;
					}

					if (parent is WzDirectory dir) {
						while (dir[img.Name] != null) {
							if (!handler.Handle(dir, img)) {
								return;
							}
						}

						dir.AddImage(img, false);
					} else if (parent is WzFile file) {
						while (file.WzDirectory[img.Name] != null) {
							if (!handler.Handle(file.WzDirectory, img)) {
								return;
							}
						}

						file.WzDirectory.AddImage(img, false);
					} else {
						throw new InvalidOperationException("Parent is not a directory or file");
					}
				} else if (extension.Equals(".xml")) {
					var objects = xmlDeserializer.ParseXML(filePath);
					foreach (var obj in objects) {
						if (parent is WzDirectory dir) {
							while (dir[obj.Name] != null) {
								if (!handler.Handle(dir, obj)) {
									return;
								}
							}

							dir.AddImage((WzImage) obj, false);
						} else if (parent is WzFile file) {
							while (file.WzDirectory[obj.Name] != null) {
								if (!handler.Handle(file.WzDirectory, obj)) {
									return;
								}
							}

							file.WzDirectory.AddImage((WzImage) obj, false);
						} else if (parent is IPropertyContainer subProp) {
							while (subProp[obj.Name] != null) {
								if (!handler.Handle(parent, obj)) {
									return;
								}
							}

							subProp.AddProperty((WzImageProperty) obj, false);
						} else {
							throw new InvalidOperationException("Parent is not a directory, file, or property container");
						}
					}
				}
			} else if (Directory.Exists(filePath)) {
				WzObject newParent;
				if (folderIsDirectory) {
					var dir = new WzDirectory(fileName);

					if (parent is WzDirectory parentDir) {
						while (parentDir[dir.Name] != null) {
							if (!handler.Handle(parentDir, dir)) {
								return;
							}
						}

						parentDir.AddDirectory(dir);
					} else if (parent is WzFile file) {
						while (file.WzDirectory[dir.Name] != null) {
							if (!handler.Handle(file.WzDirectory, dir)) {
								return;
							}
						}

						file.WzDirectory.AddDirectory(dir);
					} else {
						throw new InvalidOperationException("Parent is not a directory or file");
					}

					newParent = dir;
				} else if (parent is IPropertyContainer propCon) {
					var sub = new WzSubProperty(fileName);
					while (propCon[fileName] != null) {
						if (!handler.Handle(parent, sub)) {
							return;
						}
					}
					
					propCon.AddProperty(sub, false);
					newParent = sub;
				} else {
					throw new InvalidOperationException("Parent is not a directory or property container");
				}

				importFolderContent(handler, xmlDeserializer, imgDeserializer, newParent, filePath);
			}
		}

		private static void importFolderContent(DuplicateHandler handler, WzXmlDeserializer xmlDeserializer, WzImgDeserializer imgDeserializer, WzObject parent, string filePath) {
			foreach (var child in Directory.GetFiles(filePath)) {
				importFolder(handler, xmlDeserializer, imgDeserializer, parent, child);
			}

			foreach (var child in Directory.GetDirectories(filePath)) {
				importFolder(handler, xmlDeserializer, imgDeserializer, parent, child);
			}
		}
	}
}