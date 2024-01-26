using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using LZ4;
using MapleLib.WzLib.Serialization;
using MapleLib.WzLib.WzProperties;

/// <summary>
/// The code was modified from https://github.com/angelsl/wz2nx
/// </summary>
namespace MapleLib.WzLib.Nx {
	internal static class Extension {
		public static void EnsureMultiple(this Stream s, int multiple) {
			var skip = (int) (multiple - s.Position % multiple);
			if (skip == multiple) {
				return;
			}

			s.Write(new byte[skip], 0, skip);
		}


		public static T[] SubArray<T>(this T[] array, int offset, int length) {
			var result = new T[length];
			Array.Copy(array, offset, result, 0, length);
			return result;
		}
	}

	public class WzToNxSerializer : ProgressingWzSerializer, IWzFileSerializer {
		private static readonly byte[] PKG4 = {0x50, 0x4B, 0x47, 0x34}; // PKG4
		private static readonly bool _is64bit = IntPtr.Size == 8;


		public void SerializeFile(WzFile file, string path) {
			var filename = file.Name.Replace(".wz", ".nx");

			using (var fs = new FileStream(Path.Combine(Path.GetDirectoryName(path), filename), FileMode.Create,
				       FileAccess.ReadWrite,
				       FileShare.None))
			using (var bw = new BinaryWriter(fs)) {
				var state = new DumpState();
				bw.Write(PKG4);
				bw.Write(new byte[(4 + 8) * 4]);
				fs.EnsureMultiple(4);
				var nodeOffset = (ulong) bw.BaseStream.Position;
				var nodeLevel = new List<WzObject> {file.WzDirectory};
				while (nodeLevel.Count > 0)
					WriteNodeLevel(ref nodeLevel, state, bw);

				ulong stringOffset;
				var stringCount = (uint) state.Strings.Count;
				{
					var strings = state.Strings.ToDictionary(kvp => kvp.Value,
						kvp => kvp.Key);
					var offsets = new ulong[stringCount];
					for (uint idx = 0; idx < stringCount; ++idx) {
						fs.EnsureMultiple(2);
						offsets[idx] = (ulong) bw.BaseStream.Position;
						WriteString(strings[idx], bw);
					}

					fs.EnsureMultiple(8);
					stringOffset = (ulong) bw.BaseStream.Position;
					for (uint idx = 0; idx < stringCount; ++idx)
						bw.Write(offsets[idx]);
				}

				var bitmapOffset = 0UL;
				var bitmapCount = 0U;
				var flag = true;
				if (flag) {
					bitmapCount = (uint) state.Canvases.Count;
					var offsets = new ulong[bitmapCount];
					long cId = 0;
					foreach (var cNode in state.Canvases) {
						fs.EnsureMultiple(8);
						offsets[cId++] = (ulong) bw.BaseStream.Position;
						WriteBitmap(cNode, bw);
					}

					fs.EnsureMultiple(8);
					bitmapOffset = (ulong) bw.BaseStream.Position;
					for (var idx3 = 0U; idx3 < bitmapCount; idx3 += 1U) bw.Write(offsets[(int) idx3]);
				}

				var soundOffset = 0UL;
				var soundCount = 0U;
				var flag2 = true;
				if (flag2) {
					soundCount = (uint) state.MP3s.Count;
					var offsets = new ulong[soundCount];
					var cId = 0L;
					foreach (var mNode in state.MP3s) {
						fs.EnsureMultiple(8);
						offsets[cId++] = (ulong) bw.BaseStream.Position;
						WriteMP3(mNode, bw);
					}

					fs.EnsureMultiple(8);
					soundOffset = (ulong) bw.BaseStream.Position;
					for (var idx4 = 0U; idx4 < soundCount; idx4 += 1U) bw.Write(offsets[(int) idx4]);
				}

				var uolReplace = new byte[16];
				foreach (var pair in state.UOLs) {
					var result = pair.Key.LinkValue;
					var flag3 = result == null;
					if (!flag3) {
						bw.BaseStream.Position = (long) (nodeOffset + state.GetNodeID(result) * 20U + 4UL);
						bw.BaseStream.Read(uolReplace, 0, 16);
						pair.Value(bw, uolReplace);
					}
				}

				bw.Seek(4, SeekOrigin.Begin);
				bw.Write((uint) state.Nodes.Count);
				bw.Write(nodeOffset);
				bw.Write(stringCount);
				bw.Write(stringOffset);
				bw.Write(bitmapCount);
				bw.Write(bitmapOffset);
				bw.Write(soundCount);
				bw.Write(soundOffset);
			}
		}


		private byte[] GetCompressedBitmap(Bitmap b) {
			var bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly,
				PixelFormat.Format32bppArgb);
			var inLen = Math.Abs(bd.Stride) * bd.Height;
			var rgbValues = new byte[inLen];
			Marshal.Copy(bd.Scan0, rgbValues, 0, inLen);

			var compressed = LZ4Codec.WrapHC(rgbValues);

			return compressed.SubArray(8, compressed.Length - 8);
		}

		private void WriteBitmap(WzCanvasProperty node, BinaryWriter bw) {
			var b = node.PngProperty.GetBitmap();
			var compressed = GetCompressedBitmap(b);
			bw.Write((uint) compressed.Length);
			bw.Write(compressed);
		}

		private void WriteMP3(WzBinaryProperty node, BinaryWriter bw) {
			var i = node.GetBytes();
			bw.Write(i);
		}

		private void WriteString(string s, BinaryWriter bw) {
			var flag = s.Any(char.IsControl);
			if (flag) Console.WriteLine("Warning; control character in string. Perhaps toggle /wzn?");

			var toWrite = Encoding.UTF8.GetBytes(s);
			bw.Write((ushort) toWrite.Length);
			bw.Write(toWrite);
		}

		private void WriteNodeLevel(ref List<WzObject> nodeLevel, DumpState ds, BinaryWriter bw) {
			var nextChildId = (uint) (ds.GetNextNodeID() + (ulong) nodeLevel.Count);
			foreach (var levelNode in nodeLevel) {
				var flag = levelNode is WzUOLProperty;
				if (flag) {
					WriteUOL((WzUOLProperty) levelNode, ds, bw);
				} else {
					WriteNode(levelNode, ds, bw, nextChildId);
				}

				nextChildId += (uint) GetChildCount(levelNode);
			}

			var @out = new List<WzObject>();
			foreach (var levelNode2 in nodeLevel) {
				var childs = GetChildObjects(levelNode2);
				@out.AddRange(childs);
			}

			nodeLevel.Clear();
			nodeLevel = @out;
		}

		private void WriteUOL(WzUOLProperty node, DumpState ds, BinaryWriter bw) {
			ds.AddNode(node);
			bw.Write(ds.AddString(node.Name));
			ds.AddUOL(node, bw.BaseStream.Position);
			bw.Write(0L);
			bw.Write(0L);
		}

		public List<WzObject> GetChildObjects(WzObject node) {
			var childs = new List<WzObject>();
			var flag = node is WzDirectory;
			if (flag) {
				childs.AddRange(((WzDirectory) node).WzImages);
				childs.AddRange(((WzDirectory) node).WzDirectories);
			} else {
				var flag2 = node is WzImage;
				if (flag2) {
					childs.AddRange(((WzImage) node).WzProperties);
				} else {
					var flag3 = node is WzImageProperty && !(node is WzUOLProperty);
					if (flag3) {
						var flag4 = ((WzImageProperty) node).WzProperties != null;
						if (flag4) childs.AddRange(((WzImageProperty) node).WzProperties);
					}
				}
			}

			return childs;
		}

		private int GetChildCount(WzObject node) {
			return GetChildObjects(node).Count();
		}

		private void WriteNode(WzObject node, DumpState ds, BinaryWriter bw, uint nextChildID) {
			ds.AddNode(node);
			bw.Write(ds.AddString(node.Name));
			bw.Write(nextChildID);
			bw.Write((ushort) GetChildCount(node));

			ushort type;

			if (node is WzDirectory || node is WzImage || node is WzSubProperty || node is WzConvexProperty ||
			    node is WzNullProperty) {
				type = 0; // no data; children only (8)
			} else if (node is WzIntProperty || node is WzShortProperty || node is WzLongProperty) {
				type = 1; // int32 (4)
			} else if (node is WzDoubleProperty || node is WzFloatProperty) {
				type = 2; // Double (0)
			} else if (node is WzStringProperty || node is WzLuaProperty) {
				type = 3; // String (4)
			} else if (node is WzVectorProperty) {
				type = 4; // (0)
			} else if (node is WzCanvasProperty) {
				type = 5; // (4)
			} else if (node is WzBinaryProperty) {
				type = 6; // (4)
			} else {
				throw new InvalidOperationException("Unhandled WZ node type [1]");
			}

			bw.Write(type);
			if (node is WzIntProperty) {
				bw.Write((long) ((WzIntProperty) node).Value);
			} else if (node is WzShortProperty) {
				bw.Write((long) ((WzShortProperty) node).Value);
			} else if (node is WzLongProperty) {
				bw.Write(((WzLongProperty) node).Value);
			} else if (node is WzFloatProperty) {
				bw.Write((double) ((WzFloatProperty) node).Value);
			} else if (node is WzDoubleProperty) {
				bw.Write(((WzDoubleProperty) node).Value);
			} else if (node is WzStringProperty) {
				bw.Write(ds.AddString(((WzStringProperty) node).Value));
			} else if (node is WzVectorProperty) {
				var pNode = ((WzVectorProperty) node).Pos;
				bw.Write(pNode.X);
				bw.Write(pNode.Y);
			} else if (node is WzCanvasProperty) {
				var wzcp = (WzCanvasProperty) node;
				bw.Write(ds.AddCanvas(wzcp));
				var flag16 = true; // export canvas
				if (flag16) {
					bw.Write((ushort) wzcp.PngProperty.GetBitmap().Width);
					bw.Write((ushort) wzcp.PngProperty.GetBitmap().Height);
				} else {
					bw.Write(0);
				}
			} else if (node is WzBinaryProperty) {
				var wzmp = (WzBinaryProperty) node;
				bw.Write(ds.AddMP3(wzmp));
				var flag18 = true;
				if (flag18) {
					bw.Write((uint) wzmp.GetBytes().Length);
				} else {
					bw.Write(0);
				}
			}

			switch (type) {
				case 0:
					bw.Write(0L);
					break;
				case 3:
					bw.Write(0);
					break;
			}
		}

		private sealed class DumpState {
			public DumpState() {
				Canvases = new List<WzCanvasProperty>();
				Strings = new Dictionary<string, uint>(StringComparer.Ordinal) {{"", 0}};
				MP3s = new List<WzBinaryProperty>();
				UOLs = new Dictionary<WzUOLProperty, Action<BinaryWriter, byte[]>>();
				Nodes = new Dictionary<WzObject, uint>();
			}

			public List<WzCanvasProperty> Canvases { get; }

			public Dictionary<string, uint> Strings { get; }

			public List<WzBinaryProperty> MP3s { get; }

			public Dictionary<WzUOLProperty, Action<BinaryWriter, byte[]>> UOLs { get; }

			public Dictionary<WzObject, uint> Nodes { get; }

			public uint AddCanvas(WzCanvasProperty node) {
				var ret = (uint) Canvases.Count;
				Canvases.Add(node);
				return ret;
			}

			public uint AddMP3(WzBinaryProperty node) {
				var ret = (uint) MP3s.Count;
				MP3s.Add(node);
				return ret;
			}

			public uint AddString(string str) {
				if (Strings.ContainsKey(str)) {
					return Strings[str];
				}

				var ret = (uint) Strings.Count;
				Strings.Add(str, ret);
				return ret;
			}

			public void AddNode(WzObject node) {
				var ret = (uint) Nodes.Count;
				Nodes.Add(node, ret);
			}

			public uint GetNodeID(WzObject node) {
				return Nodes[node];
			}

			public uint GetNextNodeID() {
				return (uint) Nodes.Count;
			}

			public void AddUOL(WzUOLProperty node, long currentPosition) {
				UOLs.Add(node, (bw, data) => {
					bw.BaseStream.Position = currentPosition;
					bw.Write(data);
				});
			}
		}
	}
}