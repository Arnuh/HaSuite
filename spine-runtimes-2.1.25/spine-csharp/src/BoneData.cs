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
	public class BoneData {
		internal BoneData parent;
		internal string name;
		internal float length, x, y, rotation, scaleX = 1, scaleY = 1;
		internal bool flipX, flipY;
		internal bool inheritScale = true, inheritRotation = true;

		/// <summary>May be null.</summary>
		public BoneData Parent => parent;

		public string Name => name;

		public float Length {
			get => length;
			set => length = value;
		}

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

		public bool FlipX {
			get => flipX;
			set => flipX = value;
		}

		public bool FlipY {
			get => flipY;
			set => flipY = value;
		}

		public bool InheritScale {
			get => inheritScale;
			set => inheritScale = value;
		}

		public bool InheritRotation {
			get => inheritRotation;
			set => inheritRotation = value;
		}

		/// <param name="parent">May be null.</param>
		public BoneData(string name, BoneData parent) {
			if (name == null) throw new ArgumentNullException("name cannot be null.");
			this.name = name;
			this.parent = parent;
		}

		public override string ToString() {
			return name;
		}
	}
}