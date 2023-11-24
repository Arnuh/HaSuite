/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 *
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;

#if WINDOWS_STOREAPP
using System.Threading.Tasks;
using Windows.Storage;
#endif

namespace Spine {
	public class SkeletonBinary {
		public const int TIMELINE_SCALE = 0;
		public const int TIMELINE_ROTATE = 1;
		public const int TIMELINE_TRANSLATE = 2;
		public const int TIMELINE_ATTACHMENT = 3;
		public const int TIMELINE_COLOR = 4;
		public const int TIMELINE_FLIPX = 5;
		public const int TIMELINE_FLIPY = 6;

		public const int CURVE_LINEAR = 0;
		public const int CURVE_STEPPED = 1;
		public const int CURVE_BEZIER = 2;

		private AttachmentLoader attachmentLoader;
		public float Scale { get; set; }
		private char[] chars = new char[32];
		private byte[] buffer = new byte[4];

		public SkeletonBinary(params Atlas[] atlasArray)
			: this(new AtlasAttachmentLoader(atlasArray)) {
		}

		public SkeletonBinary(AttachmentLoader attachmentLoader) {
			if (attachmentLoader == null) throw new ArgumentNullException("attachmentLoader cannot be null.");
			this.attachmentLoader = attachmentLoader;
			Scale = 1;
		}

#if WINDOWS_STOREAPP
		private async Task<SkeletonData> ReadFile(string path) {
			var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
			using (var input = new BufferedStream(await folder.GetFileAsync(path).AsTask().ConfigureAwait(false))) {
				SkeletonData skeletonData = ReadSkeletonData(input);
				skeletonData.Name = Path.GetFileNameWithoutExtension(path);
				return skeletonData;
			}
		}

		public SkeletonData ReadSkeletonData (String path) {
			return this.ReadFile(path).Result;
		}
#else
		public SkeletonData ReadSkeletonData(string path) {
#if WINDOWS_PHONE
			using (var input = new BufferedStream(Microsoft.Xna.Framework.TitleContainer.OpenStream(path)))
			{
#else
			using (var input = new BufferedStream(new FileStream(path, FileMode.Open))) {
#endif
				var skeletonData = ReadSkeletonData(input);
				skeletonData.name = Path.GetFileNameWithoutExtension(path);
				return skeletonData;
			}
		}
#endif

		public SkeletonData ReadSkeletonData(Stream input) {
			if (input == null) throw new ArgumentNullException("input cannot be null.");
			var scale = Scale;

			var skeletonData = new SkeletonData();
			skeletonData.hash = ReadString(input);
			if (skeletonData.hash.Length == 0) skeletonData.hash = null;
			skeletonData.version = ReadString(input);
			if (skeletonData.version.Length == 0) skeletonData.version = null;
			skeletonData.width = ReadFloat(input);
			skeletonData.height = ReadFloat(input);

			var nonessential = ReadBoolean(input);

			if (nonessential) {
				skeletonData.imagesPath = ReadString(input);
				if (skeletonData.imagesPath.Length == 0) skeletonData.imagesPath = null;
			}

			// Bones.
			for (int i = 0, n = ReadInt(input, true); i < n; i++) {
				var name = ReadString(input);
				BoneData parent = null;
				var parentIndex = ReadInt(input, true) - 1;
				if (parentIndex != -1) parent = skeletonData.bones[parentIndex];
				var boneData = new BoneData(name, parent);
				boneData.x = ReadFloat(input) * scale;
				boneData.y = ReadFloat(input) * scale;
				boneData.scaleX = ReadFloat(input);
				boneData.scaleY = ReadFloat(input);
				boneData.rotation = ReadFloat(input);
				boneData.length = ReadFloat(input) * scale;
				boneData.flipX = ReadBoolean(input);
				boneData.flipY = ReadBoolean(input);
				boneData.inheritScale = ReadBoolean(input);
				boneData.inheritRotation = ReadBoolean(input);
				if (nonessential) ReadInt(input); // Skip bone color.
				skeletonData.bones.Add(boneData);
			}

			// IK constraints.
			for (int i = 0, n = ReadInt(input, true); i < n; i++) {
				var ikConstraintData = new IkConstraintData(ReadString(input));
				for (int ii = 0, nn = ReadInt(input, true); ii < nn; ii++)
					ikConstraintData.bones.Add(skeletonData.bones[ReadInt(input, true)]);
				ikConstraintData.target = skeletonData.bones[ReadInt(input, true)];
				ikConstraintData.mix = ReadFloat(input);
				ikConstraintData.bendDirection = ReadSByte(input);
				skeletonData.ikConstraints.Add(ikConstraintData);
			}

			// Slots.
			for (int i = 0, n = ReadInt(input, true); i < n; i++) {
				var slotName = ReadString(input);
				var boneData = skeletonData.bones[ReadInt(input, true)];
				var slotData = new SlotData(slotName, boneData);
				var color = ReadInt(input);
				slotData.r = ((color & 0xff000000) >> 24) / 255f;
				slotData.g = ((color & 0x00ff0000) >> 16) / 255f;
				slotData.b = ((color & 0x0000ff00) >> 8) / 255f;
				slotData.a = (color & 0x000000ff) / 255f;
				slotData.attachmentName = ReadString(input);
				slotData.additiveBlending = ReadBoolean(input);
				skeletonData.slots.Add(slotData);
			}

			// Default skin.
			var defaultSkin = ReadSkin(input, "default", nonessential);
			if (defaultSkin != null) {
				skeletonData.defaultSkin = defaultSkin;
				skeletonData.skins.Add(defaultSkin);
			}

			// Skins.
			for (int i = 0, n = ReadInt(input, true); i < n; i++)
				skeletonData.skins.Add(ReadSkin(input, ReadString(input), nonessential));

			// Events.
			for (int i = 0, n = ReadInt(input, true); i < n; i++) {
				var eventData = new EventData(ReadString(input));
				eventData.Int = ReadInt(input, false);
				eventData.Float = ReadFloat(input);
				eventData.String = ReadString(input);
				skeletonData.events.Add(eventData);
			}

			// Animations.
			for (int i = 0, n = ReadInt(input, true); i < n; i++)
				ReadAnimation(ReadString(input), input, skeletonData);

			skeletonData.bones.TrimExcess();
			skeletonData.slots.TrimExcess();
			skeletonData.skins.TrimExcess();
			skeletonData.events.TrimExcess();
			skeletonData.animations.TrimExcess();
			skeletonData.ikConstraints.TrimExcess();
			return skeletonData;
		}

		/** @return May be null. */
		private Skin ReadSkin(Stream input, string skinName, bool nonessential) {
			var slotCount = ReadInt(input, true);
			if (slotCount == 0) return null;
			var skin = new Skin(skinName);
			for (var i = 0; i < slotCount; i++) {
				var slotIndex = ReadInt(input, true);
				for (int ii = 0, nn = ReadInt(input, true); ii < nn; ii++) {
					var name = ReadString(input);
					skin.AddAttachment(slotIndex, name, ReadAttachment(input, skin, name, nonessential));
				}
			}

			return skin;
		}

		private Attachment ReadAttachment(Stream input, Skin skin, string attachmentName, bool nonessential) {
			var scale = Scale;

			var name = ReadString(input);
			if (name == null) name = attachmentName;

			switch ((AttachmentType) input.ReadByte()) {
				case AttachmentType.region: {
					var path = ReadString(input);
					if (path == null) path = name;
					var region = attachmentLoader.NewRegionAttachment(skin, name, path);
					if (region == null) return null;
					region.Path = path;
					region.x = ReadFloat(input) * scale;
					region.y = ReadFloat(input) * scale;
					region.scaleX = ReadFloat(input);
					region.scaleY = ReadFloat(input);
					region.rotation = ReadFloat(input);
					region.width = ReadFloat(input) * scale;
					region.height = ReadFloat(input) * scale;
					var color = ReadInt(input);
					region.r = ((color & 0xff000000) >> 24) / 255f;
					region.g = ((color & 0x00ff0000) >> 16) / 255f;
					region.b = ((color & 0x0000ff00) >> 8) / 255f;
					region.a = (color & 0x000000ff) / 255f;
					region.UpdateOffset();
					return region;
				}
				case AttachmentType.boundingbox: {
					var box = attachmentLoader.NewBoundingBoxAttachment(skin, name);
					if (box == null) return null;
					box.vertices = ReadFloatArray(input, scale);
					return box;
				}
				case AttachmentType.mesh: {
					var path = ReadString(input);
					if (path == null) path = name;
					var mesh = attachmentLoader.NewMeshAttachment(skin, name, path);
					if (mesh == null) return null;
					mesh.Path = path;
					mesh.regionUVs = ReadFloatArray(input, 1);
					mesh.triangles = ReadShortArray(input);
					mesh.vertices = ReadFloatArray(input, scale);
					mesh.UpdateUVs();
					var color = ReadInt(input);
					mesh.r = ((color & 0xff000000) >> 24) / 255f;
					mesh.g = ((color & 0x00ff0000) >> 16) / 255f;
					mesh.b = ((color & 0x0000ff00) >> 8) / 255f;
					mesh.a = (color & 0x000000ff) / 255f;
					mesh.HullLength = ReadInt(input, true) * 2;
					if (nonessential) {
						mesh.Edges = ReadIntArray(input);
						mesh.Width = ReadFloat(input) * scale;
						mesh.Height = ReadFloat(input) * scale;
					}

					return mesh;
				}
				case AttachmentType.skinnedmesh: {
					var path = ReadString(input);
					if (path == null) path = name;
					var mesh = attachmentLoader.NewSkinnedMeshAttachment(skin, name, path);
					if (mesh == null) return null;
					mesh.Path = path;
					var uvs = ReadFloatArray(input, 1);
					var triangles = ReadShortArray(input);

					var vertexCount = ReadInt(input, true);
					var weights = new List<float>(uvs.Length * 3 * 3);
					var bones = new List<int>(uvs.Length * 3);
					for (var i = 0; i < vertexCount; i++) {
						var boneCount = (int) ReadFloat(input);
						bones.Add(boneCount);
						for (var nn = i + boneCount * 4; i < nn; i += 4) {
							bones.Add((int) ReadFloat(input));
							weights.Add(ReadFloat(input) * scale);
							weights.Add(ReadFloat(input) * scale);
							weights.Add(ReadFloat(input));
						}
					}

					mesh.bones = bones.ToArray();
					mesh.weights = weights.ToArray();
					mesh.triangles = triangles;
					mesh.regionUVs = uvs;
					mesh.UpdateUVs();
					var color = ReadInt(input);
					mesh.r = ((color & 0xff000000) >> 24) / 255f;
					mesh.g = ((color & 0x00ff0000) >> 16) / 255f;
					mesh.b = ((color & 0x0000ff00) >> 8) / 255f;
					mesh.a = (color & 0x000000ff) / 255f;
					mesh.HullLength = ReadInt(input, true) * 2;
					if (nonessential) {
						mesh.Edges = ReadIntArray(input);
						mesh.Width = ReadFloat(input) * scale;
						mesh.Height = ReadFloat(input) * scale;
					}

					return mesh;
				}
			}

			return null;
		}

		private float[] ReadFloatArray(Stream input, float scale) {
			var n = ReadInt(input, true);
			var array = new float[n];
			if (scale == 1)
				for (var i = 0; i < n; i++)
					array[i] = ReadFloat(input);
			else
				for (var i = 0; i < n; i++)
					array[i] = ReadFloat(input) * scale;

			return array;
		}

		private int[] ReadShortArray(Stream input) {
			var n = ReadInt(input, true);
			var array = new int[n];
			for (var i = 0; i < n; i++)
				array[i] = (input.ReadByte() << 8) + input.ReadByte();
			return array;
		}

		private int[] ReadIntArray(Stream input) {
			var n = ReadInt(input, true);
			var array = new int[n];
			for (var i = 0; i < n; i++)
				array[i] = ReadInt(input, true);
			return array;
		}

		private void ReadAnimation(string name, Stream input, SkeletonData skeletonData) {
			var timelines = new List<Timeline>();
			var scale = Scale;
			float duration = 0;

			// Slot timelines.
			for (int i = 0, n = ReadInt(input, true); i < n; i++) {
				var slotIndex = ReadInt(input, true);
				for (int ii = 0, nn = ReadInt(input, true); ii < nn; ii++) {
					var timelineType = input.ReadByte();
					var frameCount = ReadInt(input, true);
					switch (timelineType) {
						case TIMELINE_COLOR: {
							var timeline = new ColorTimeline(frameCount);
							timeline.slotIndex = slotIndex;
							for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								var time = ReadFloat(input);
								var color = ReadInt(input);
								var r = ((color & 0xff000000) >> 24) / 255f;
								var g = ((color & 0x00ff0000) >> 16) / 255f;
								var b = ((color & 0x0000ff00) >> 8) / 255f;
								var a = (color & 0x000000ff) / 255f;
								timeline.SetFrame(frameIndex, time, r, g, b, a);
								if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
							}

							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[frameCount * 5 - 5]);
							break;
						}
						case TIMELINE_ATTACHMENT: {
							var timeline = new AttachmentTimeline(frameCount);
							timeline.slotIndex = slotIndex;
							for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
								timeline.SetFrame(frameIndex, ReadFloat(input), ReadString(input));
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[frameCount - 1]);
							break;
						}
					}
				}
			}

			// Bone timelines.
			for (int i = 0, n = ReadInt(input, true); i < n; i++) {
				var boneIndex = ReadInt(input, true);
				for (int ii = 0, nn = ReadInt(input, true); ii < nn; ii++) {
					var timelineType = input.ReadByte();
					var frameCount = ReadInt(input, true);
					switch (timelineType) {
						case TIMELINE_ROTATE: {
							var timeline = new RotateTimeline(frameCount);
							timeline.boneIndex = boneIndex;
							for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								timeline.SetFrame(frameIndex, ReadFloat(input), ReadFloat(input));
								if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
							}

							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[frameCount * 2 - 2]);
							break;
						}
						case TIMELINE_TRANSLATE:
						case TIMELINE_SCALE: {
							TranslateTimeline timeline;
							float timelineScale = 1;
							if (timelineType == TIMELINE_SCALE) {
								timeline = new ScaleTimeline(frameCount);
							}
							else {
								timeline = new TranslateTimeline(frameCount);
								timelineScale = scale;
							}

							timeline.boneIndex = boneIndex;
							for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
								timeline.SetFrame(frameIndex, ReadFloat(input), ReadFloat(input) * timelineScale,
									ReadFloat(input)
									* timelineScale);
								if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
							}

							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[frameCount * 3 - 3]);
							break;
						}
						case TIMELINE_FLIPX:
						case TIMELINE_FLIPY: {
							var timeline = timelineType == TIMELINE_FLIPX
								? new FlipXTimeline(frameCount)
								: new FlipYTimeline(
									frameCount);
							timeline.boneIndex = boneIndex;
							for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
								timeline.SetFrame(frameIndex, ReadFloat(input), ReadBoolean(input));
							timelines.Add(timeline);
							duration = Math.Max(duration, timeline.frames[frameCount * 2 - 2]);
							break;
						}
					}
				}
			}

			// IK timelines.
			for (int i = 0, n = ReadInt(input, true); i < n; i++) {
				var ikConstraint = skeletonData.ikConstraints[ReadInt(input, true)];
				var frameCount = ReadInt(input, true);
				var timeline = new IkConstraintTimeline(frameCount);
				timeline.ikConstraintIndex = skeletonData.ikConstraints.IndexOf(ikConstraint);
				for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
					timeline.SetFrame(frameIndex, ReadFloat(input), ReadFloat(input), ReadSByte(input));
					if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
				}

				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[frameCount * 3 - 3]);
			}

			// FFD timelines.
			for (int i = 0, n = ReadInt(input, true); i < n; i++) {
				var skin = skeletonData.skins[ReadInt(input, true)];
				for (int ii = 0, nn = ReadInt(input, true); ii < nn; ii++) {
					var slotIndex = ReadInt(input, true);
					for (int iii = 0, nnn = ReadInt(input, true); iii < nnn; iii++) {
						var attachment = skin.GetAttachment(slotIndex, ReadString(input));
						var frameCount = ReadInt(input, true);
						var timeline = new FFDTimeline(frameCount);
						timeline.slotIndex = slotIndex;
						timeline.attachment = attachment;
						for (var frameIndex = 0; frameIndex < frameCount; frameIndex++) {
							var time = ReadFloat(input);

							float[] vertices;
							int vertexCount;
							if (attachment is MeshAttachment)
								vertexCount = ((MeshAttachment) attachment).vertices.Length;
							else
								vertexCount = ((SkinnedMeshAttachment) attachment).weights.Length / 3 * 2;

							var end = ReadInt(input, true);
							if (end == 0) {
								if (attachment is MeshAttachment)
									vertices = ((MeshAttachment) attachment).vertices;
								else
									vertices = new float[vertexCount];
							}
							else {
								vertices = new float[vertexCount];
								var start = ReadInt(input, true);
								end += start;
								if (scale == 1)
									for (var v = start; v < end; v++)
										vertices[v] = ReadFloat(input);
								else
									for (var v = start; v < end; v++)
										vertices[v] = ReadFloat(input) * scale;

								if (attachment is MeshAttachment) {
									var meshVertices = ((MeshAttachment) attachment).vertices;
									for (int v = 0, vn = vertices.Length; v < vn; v++)
										vertices[v] += meshVertices[v];
								}
							}

							timeline.SetFrame(frameIndex, time, vertices);
							if (frameIndex < frameCount - 1) ReadCurve(input, frameIndex, timeline);
						}

						timelines.Add(timeline);
						duration = Math.Max(duration, timeline.frames[frameCount - 1]);
					}
				}
			}

			// Draw order timeline.
			var drawOrderCount = ReadInt(input, true);
			if (drawOrderCount > 0) {
				var timeline = new DrawOrderTimeline(drawOrderCount);
				var slotCount = skeletonData.slots.Count;
				for (var i = 0; i < drawOrderCount; i++) {
					var offsetCount = ReadInt(input, true);
					var drawOrder = new int[slotCount];
					for (var ii = slotCount - 1; ii >= 0; ii--)
						drawOrder[ii] = -1;
					var unchanged = new int[slotCount - offsetCount];
					int originalIndex = 0, unchangedIndex = 0;
					for (var ii = 0; ii < offsetCount; ii++) {
						var slotIndex = ReadInt(input, true);
						// Collect unchanged items.
						while (originalIndex != slotIndex)
							unchanged[unchangedIndex++] = originalIndex++;
						// Set changed items.
						drawOrder[originalIndex + ReadInt(input, true)] = originalIndex++;
					}

					// Collect remaining unchanged items.
					while (originalIndex < slotCount)
						unchanged[unchangedIndex++] = originalIndex++;
					// Fill in unchanged items.
					for (var ii = slotCount - 1; ii >= 0; ii--)
						if (drawOrder[ii] == -1)
							drawOrder[ii] = unchanged[--unchangedIndex];
					timeline.SetFrame(i, ReadFloat(input), drawOrder);
				}

				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[drawOrderCount - 1]);
			}

			// Event timeline.
			var eventCount = ReadInt(input, true);
			if (eventCount > 0) {
				var timeline = new EventTimeline(eventCount);
				for (var i = 0; i < eventCount; i++) {
					var time = ReadFloat(input);
					var eventData = skeletonData.events[ReadInt(input, true)];
					var e = new Event(eventData);
					e.Int = ReadInt(input, false);
					e.Float = ReadFloat(input);
					e.String = ReadBoolean(input) ? ReadString(input) : eventData.String;
					timeline.SetFrame(i, time, e);
				}

				timelines.Add(timeline);
				duration = Math.Max(duration, timeline.frames[eventCount - 1]);
			}

			timelines.TrimExcess();
			skeletonData.animations.Add(new Animation(name, timelines, duration));
		}

		private void ReadCurve(Stream input, int frameIndex, CurveTimeline timeline) {
			switch (input.ReadByte()) {
				case CURVE_STEPPED:
					timeline.SetStepped(frameIndex);
					break;
				case CURVE_BEZIER:
					timeline.SetCurve(frameIndex, ReadFloat(input), ReadFloat(input), ReadFloat(input),
						ReadFloat(input));
					break;
			}
		}

		private sbyte ReadSByte(Stream input) {
			var value = input.ReadByte();
			if (value == -1) throw new EndOfStreamException();
			return (sbyte) value;
		}

		private bool ReadBoolean(Stream input) {
			return input.ReadByte() != 0;
		}

		private float ReadFloat(Stream input) {
			buffer[3] = (byte) input.ReadByte();
			buffer[2] = (byte) input.ReadByte();
			buffer[1] = (byte) input.ReadByte();
			buffer[0] = (byte) input.ReadByte();
			return BitConverter.ToSingle(buffer, 0);
		}

		private int ReadInt(Stream input) {
			return (input.ReadByte() << 24) + (input.ReadByte() << 16) + (input.ReadByte() << 8) + input.ReadByte();
		}

		private int ReadInt(Stream input, bool optimizePositive) {
			var b = input.ReadByte();
			var result = b & 0x7F;
			if ((b & 0x80) != 0) {
				b = input.ReadByte();
				result |= (b & 0x7F) << 7;
				if ((b & 0x80) != 0) {
					b = input.ReadByte();
					result |= (b & 0x7F) << 14;
					if ((b & 0x80) != 0) {
						b = input.ReadByte();
						result |= (b & 0x7F) << 21;
						if ((b & 0x80) != 0) {
							b = input.ReadByte();
							result |= (b & 0x7F) << 28;
						}
					}
				}
			}

			return optimizePositive ? result : (result >> 1) ^ -(result & 1);
		}

		private string ReadString(Stream input) {
			var charCount = ReadInt(input, true);
			switch (charCount) {
				case 0:
					return null;
				case 1:
					return "";
			}

			charCount--;
			var chars = this.chars;
			if (chars.Length < charCount) this.chars = chars = new char[charCount];
			// Try to read 7 bit ASCII chars.
			var charIndex = 0;
			var b = 0;
			while (charIndex < charCount) {
				b = input.ReadByte();
				if (b > 127) break;
				chars[charIndex++] = (char) b;
			}

			// If a char was not ASCII, finish with slow path.
			if (charIndex < charCount) ReadUtf8_slow(input, charCount, charIndex, b);
			return new string(chars, 0, charCount);
		}

		private void ReadUtf8_slow(Stream input, int charCount, int charIndex, int b) {
			var chars = this.chars;
			while (true) {
				switch (b >> 4) {
					case 0:
					case 1:
					case 2:
					case 3:
					case 4:
					case 5:
					case 6:
					case 7:
						chars[charIndex] = (char) b;
						break;
					case 12:
					case 13:
						chars[charIndex] = (char) (((b & 0x1F) << 6) | (input.ReadByte() & 0x3F));
						break;
					case 14:
						chars[charIndex] = (char) (((b & 0x0F) << 12) | ((input.ReadByte() & 0x3F) << 6) |
						                           (input.ReadByte() & 0x3F));
						break;
				}

				if (++charIndex >= charCount) break;
				b = input.ReadByte() & 0xFF;
			}
		}
	}
}