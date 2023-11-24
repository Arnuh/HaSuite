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

namespace Spine {
	/// <summary>Attachment that displays a texture region.</summary>
	public class RegionAttachment : Attachment {
		public const int X1 = 0;
		public const int Y1 = 1;
		public const int X2 = 2;
		public const int Y2 = 3;
		public const int X3 = 4;
		public const int Y3 = 5;
		public const int X4 = 6;
		public const int Y4 = 7;

		internal float x, y, rotation, scaleX = 1, scaleY = 1, width, height;

		internal float regionOffsetX,
			regionOffsetY,
			regionWidth,
			regionHeight,
			regionOriginalWidth,
			regionOriginalHeight;

		internal float[] offset = new float[8], uvs = new float[8];
		internal float r = 1, g = 1, b = 1, a = 1;

		public float X {
			get => x;
			set => x = value;
		}

		public float Y {
			get => y;
			set => y = value;
		}

		public float Rotation {
			get => rotation;
			set => rotation = value;
		}

		public float ScaleX {
			get => scaleX;
			set => scaleX = value;
		}

		public float ScaleY {
			get => scaleY;
			set => scaleY = value;
		}

		public float Width {
			get => width;
			set => width = value;
		}

		public float Height {
			get => height;
			set => height = value;
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

		public float[] Offset => offset;

		public float[] UVs => uvs;

		public RegionAttachment(string name)
			: base(name) {
		}

		public void SetUVs(float u, float v, float u2, float v2, bool rotate) {
			var uvs = this.uvs;
			if (rotate) {
				uvs[X2] = u;
				uvs[Y2] = v2;
				uvs[X3] = u;
				uvs[Y3] = v;
				uvs[X4] = u2;
				uvs[Y4] = v;
				uvs[X1] = u2;
				uvs[Y1] = v2;
			}
			else {
				uvs[X1] = u;
				uvs[Y1] = v2;
				uvs[X2] = u;
				uvs[Y2] = v;
				uvs[X3] = u2;
				uvs[Y3] = v;
				uvs[X4] = u2;
				uvs[Y4] = v2;
			}
		}

		public void UpdateOffset() {
			var width = this.width;
			var height = this.height;
			var scaleX = this.scaleX;
			var scaleY = this.scaleY;
			var regionScaleX = width / regionOriginalWidth * scaleX;
			var regionScaleY = height / regionOriginalHeight * scaleY;
			var localX = -width / 2 * scaleX + regionOffsetX * regionScaleX;
			var localY = -height / 2 * scaleY + regionOffsetY * regionScaleY;
			var localX2 = localX + regionWidth * regionScaleX;
			var localY2 = localY + regionHeight * regionScaleY;
			var radians = rotation * (float) Math.PI / 180;
			var cos = (float) Math.Cos(radians);
			var sin = (float) Math.Sin(radians);
			var x = this.x;
			var y = this.y;
			var localXCos = localX * cos + x;
			var localXSin = localX * sin;
			var localYCos = localY * cos + y;
			var localYSin = localY * sin;
			var localX2Cos = localX2 * cos + x;
			var localX2Sin = localX2 * sin;
			var localY2Cos = localY2 * cos + y;
			var localY2Sin = localY2 * sin;
			var offset = this.offset;
			offset[X1] = localXCos - localYSin;
			offset[Y1] = localYCos + localXSin;
			offset[X2] = localXCos - localY2Sin;
			offset[Y2] = localY2Cos + localXSin;
			offset[X3] = localX2Cos - localY2Sin;
			offset[Y3] = localY2Cos + localX2Sin;
			offset[X4] = localX2Cos - localYSin;
			offset[Y4] = localYCos + localX2Sin;
		}

		public void ComputeWorldVertices(Bone bone, float[] worldVertices) {
			float x = bone.skeleton.x + bone.worldX, y = bone.skeleton.y + bone.worldY;
			float m00 = bone.m00, m01 = bone.m01, m10 = bone.m10, m11 = bone.m11;
			var offset = this.offset;
			worldVertices[X1] = offset[X1] * m00 + offset[Y1] * m01 + x;
			worldVertices[Y1] = offset[X1] * m10 + offset[Y1] * m11 + y;
			worldVertices[X2] = offset[X2] * m00 + offset[Y2] * m01 + x;
			worldVertices[Y2] = offset[X2] * m10 + offset[Y2] * m11 + y;
			worldVertices[X3] = offset[X3] * m00 + offset[Y3] * m01 + x;
			worldVertices[Y3] = offset[X3] * m10 + offset[Y3] * m11 + y;
			worldVertices[X4] = offset[X4] * m00 + offset[Y4] * m01 + x;
			worldVertices[Y4] = offset[X4] * m10 + offset[Y4] * m11 + y;
		}
	}
}