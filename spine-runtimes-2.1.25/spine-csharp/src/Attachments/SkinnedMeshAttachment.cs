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
using System.Collections.Generic;

namespace Spine {
	/// <summary>Attachment that displays a texture region.</summary>
	public class SkinnedMeshAttachment : Attachment {
		internal int[] bones;
		internal float[] weights, uvs, regionUVs;
		internal int[] triangles;

		internal float regionOffsetX,
			regionOffsetY,
			regionWidth,
			regionHeight,
			regionOriginalWidth,
			regionOriginalHeight;

		internal float r = 1, g = 1, b = 1, a = 1;

		public int HullLength { get; set; }

		public int[] Bones {
			get => bones;
			set => bones = value;
		}

		public float[] Weights {
			get => weights;
			set => weights = value;
		}

		public float[] RegionUVs {
			get => regionUVs;
			set => regionUVs = value;
		}

		public float[] UVs {
			get => uvs;
			set => uvs = value;
		}

		public int[] Triangles {
			get => triangles;
			set => triangles = value;
		}

		public float R {
			get => r;
			set => r = value;
		}

		public float G {
			get => g;
			set => g = value;
		}

		public float B {
			get => b;
			set => b = value;
		}

		public float A {
			get => a;
			set => a = value;
		}

		public string Path { get; set; }
		public object RendererObject { get; set; }
		public float RegionU { get; set; }
		public float RegionV { get; set; }
		public float RegionU2 { get; set; }
		public float RegionV2 { get; set; }
		public bool RegionRotate { get; set; }

		public float RegionOffsetX {
			get => regionOffsetX;
			set => regionOffsetX = value;
		}

		public float RegionOffsetY {
			get => regionOffsetY;
			set => regionOffsetY = value;
		} // Pixels stripped from the bottom left, unrotated.

		public float RegionWidth {
			get => regionWidth;
			set => regionWidth = value;
		}

		public float RegionHeight {
			get => regionHeight;
			set => regionHeight = value;
		} // Unrotated, stripped size.

		public float RegionOriginalWidth {
			get => regionOriginalWidth;
			set => regionOriginalWidth = value;
		}

		public float RegionOriginalHeight {
			get => regionOriginalHeight;
			set => regionOriginalHeight = value;
		} // Unrotated, unstripped size.

		// Nonessential.
		public int[] Edges { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }

		public SkinnedMeshAttachment(string name)
			: base(name) {
		}

		public void UpdateUVs() {
			float u = RegionU, v = RegionV, width = RegionU2 - RegionU, height = RegionV2 - RegionV;
			var regionUVs = this.regionUVs;
			if (this.uvs == null || this.uvs.Length != regionUVs.Length) this.uvs = new float[regionUVs.Length];
			var uvs = this.uvs;
			if (RegionRotate)
				for (int i = 0, n = uvs.Length; i < n; i += 2) {
					uvs[i] = u + regionUVs[i + 1] * width;
					uvs[i + 1] = v + height - regionUVs[i] * height;
				}
			else
				for (int i = 0, n = uvs.Length; i < n; i += 2) {
					uvs[i] = u + regionUVs[i] * width;
					uvs[i + 1] = v + regionUVs[i + 1] * height;
				}
		}

		public void ComputeWorldVertices(Slot slot, float[] worldVertices) {
			var skeleton = slot.bone.skeleton;
			var skeletonBones = skeleton.bones;
			float x = skeleton.x, y = skeleton.y;
			var weights = this.weights;
			var bones = this.bones;
			if (slot.attachmentVerticesCount == 0) {
				for (int w = 0, v = 0, b = 0, n = bones.Length; v < n; w += 2) {
					float wx = 0, wy = 0;
					var nn = bones[v++] + v;
					for (; v < nn; v++, b += 3) {
						var bone = skeletonBones[bones[v]];
						float vx = weights[b], vy = weights[b + 1], weight = weights[b + 2];
						wx += (vx * bone.m00 + vy * bone.m01 + bone.worldX) * weight;
						wy += (vx * bone.m10 + vy * bone.m11 + bone.worldY) * weight;
					}

					worldVertices[w] = wx + x;
					worldVertices[w + 1] = wy + y;
				}
			}
			else {
				var ffd = slot.AttachmentVertices;
				for (int w = 0, v = 0, b = 0, f = 0, n = bones.Length; v < n; w += 2) {
					float wx = 0, wy = 0;
					var nn = bones[v++] + v;
					for (; v < nn; v++, b += 3, f += 2) {
						var bone = skeletonBones[bones[v]];
						float vx = weights[b] + ffd[f], vy = weights[b + 1] + ffd[f + 1], weight = weights[b + 2];
						wx += (vx * bone.m00 + vy * bone.m01 + bone.worldX) * weight;
						wy += (vx * bone.m10 + vy * bone.m11 + bone.worldY) * weight;
					}

					worldVertices[w] = wx + x;
					worldVertices[w + 1] = wy + y;
				}
			}
		}
	}
}