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

namespace Spine {
	/// <summary>Attachment that displays a texture region.</summary>
	public class MeshAttachment : Attachment {
		internal float[] vertices, uvs, regionUVs;
		internal int[] triangles;

		internal float regionOffsetX,
			regionOffsetY,
			regionWidth,
			regionHeight,
			regionOriginalWidth,
			regionOriginalHeight;

		internal float r = 1, g = 1, b = 1, a = 1;

		public int HullLength { get; set; }

		public float[] Vertices {
			get => vertices;
			set => vertices = value;
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

		public MeshAttachment(string name)
			: base(name) {
		}

		public void UpdateUVs() {
			float u = RegionU, v = RegionV, width = RegionU2 - RegionU, height = RegionV2 - RegionV;
			var regionUVs = this.regionUVs;
			if (this.uvs == null || this.uvs.Length != regionUVs.Length) this.uvs = new float[regionUVs.Length];
			var uvs = this.uvs;
			if (RegionRotate) {
				for (int i = 0, n = uvs.Length; i < n; i += 2) {
					uvs[i] = u + regionUVs[i + 1] * width;
					uvs[i + 1] = v + height - regionUVs[i] * height;
				}
			} else {
				for (int i = 0, n = uvs.Length; i < n; i += 2) {
					uvs[i] = u + regionUVs[i] * width;
					uvs[i + 1] = v + regionUVs[i + 1] * height;
				}
			}
		}

		public void ComputeWorldVertices(Slot slot, float[] worldVertices) {
			var bone = slot.bone;
			float x = bone.skeleton.x + bone.worldX, y = bone.skeleton.y + bone.worldY;
			float m00 = bone.m00, m01 = bone.m01, m10 = bone.m10, m11 = bone.m11;
			var vertices = this.vertices;
			var verticesCount = vertices.Length;
			if (slot.attachmentVerticesCount == verticesCount) vertices = slot.AttachmentVertices;
			for (var i = 0; i < verticesCount; i += 2) {
				var vx = vertices[i];
				var vy = vertices[i + 1];
				worldVertices[i] = vx * m00 + vy * m01 + x;
				worldVertices[i + 1] = vx * m10 + vy * m11 + y;
			}
		}
	}
}