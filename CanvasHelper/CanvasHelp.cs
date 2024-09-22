using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace CanvasHelper {
	public class CanvasHelp {
		public static List<WzFile> GetMainFiles(string path) {
			var wzFiles = new List<WzFile>();
			WzMapleVersion? version = null;
			foreach (var wzFile in Directory.GetFiles(path, "*_*.wz", SearchOption.TopDirectoryOnly)) {
				version ??= WzTool.DetectMapleVersion(wzFile, out _);
				var file = new WzFile(wzFile, version.Value);
				if (file.ParseWzFile() == WzFileParseStatus.Success) {
					wzFiles.Add(file);
				}
			}

			return wzFiles;
		}

		public static List<WzFile> GetCanvasFiles(string path) {
			var wzFiles = new List<WzFile>();
			WzMapleVersion? version = null;
			foreach (var wzFile in Directory.GetFiles(path, "_Canvas_*.wz", SearchOption.TopDirectoryOnly)) {
				version ??= WzTool.DetectMapleVersion(wzFile, out _);
				var file = new WzFile(wzFile, version.Value);
				if (file.ParseWzFile() == WzFileParseStatus.Success) {
					wzFiles.Add(file);
				}
			}

			return wzFiles;
		}

		public static void MergeImage(List<WzFile> canvasFiles, WzImage canvasImg, WzImage orgImg) {
			if (!orgImg.Parsed) {
				orgImg.ParseImage();
			}


			foreach (var prop in orgImg.WzProperties) {
				var clone = prop.DeepClone();
				canvasImg[clone.Name] = clone;
				FixCanvas(canvasFiles, clone);
			}

			canvasImg.Changed = true;
		}

		public static void FixCanvas(List<WzFile> canvasFiles, WzImageProperty fix) {
			if (fix is WzCanvasProperty canvas) {
				var outlink = fix["_outlink"] as WzStringProperty;
				if (outlink != null) {
					var startIndex = outlink.Value.IndexOf("_Canvas/") + "_Canvas/".Length;
					var imgName = outlink.Value.Substring(startIndex, outlink.Value.IndexOf(".img") + 4 - startIndex);
					var img = GetImage(canvasFiles, imgName);
					var path = outlink.Value.Substring(outlink.Value.IndexOf(".img/") + 5);

					var properCanvas = img.GetFromPath(path) as WzCanvasProperty;
					if (properCanvas == null) {
						Console.WriteLine($"{outlink.Value}");
					} else {
						canvas.PngProperty = properCanvas.PngProperty.DeepClone() as WzPngProperty;
					}

					outlink.Remove();
				} else {
					//Console.WriteLine($"outlink missing for {canvas.FullPath}");
				}
			}

			if (fix.WzProperties == null) {
				return;
			}

			foreach (var sub in fix.WzProperties) {
				FixCanvas(canvasFiles, sub);
			}
		}

		public static WzImage? GetImage(List<WzFile> canvasFiles, string imgName) {
			foreach (var file in canvasFiles) {
				foreach (var img in file.WzDirectory.WzImages) {
					if (img.Name.Equals(imgName)) {
						return img;
					}
				}
			}

			return null;
		}
	}
}