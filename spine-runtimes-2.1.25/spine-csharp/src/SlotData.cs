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
	public class SlotData {
		internal string name;
		internal BoneData boneData;
		internal float r = 1, g = 1, b = 1, a = 1;
		internal string attachmentName;
		internal bool additiveBlending;

		public string Name => name;

		public BoneData BoneData => boneData;

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

		/// <summary>May be null.</summary>
		public string AttachmentName {
			get => attachmentName;
			set => attachmentName = value;
		}

		public bool AdditiveBlending {
			get => additiveBlending;
			set => additiveBlending = value;
		}

		public SlotData(string name, BoneData boneData) {
			if (name == null) throw new ArgumentNullException("name cannot be null.");
			if (boneData == null) throw new ArgumentNullException("boneData cannot be null.");
			this.name = name;
			this.boneData = boneData;
		}

		public override string ToString() {
			return name;
		}
	}
}