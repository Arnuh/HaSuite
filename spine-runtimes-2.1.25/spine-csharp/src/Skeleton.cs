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
	public class Skeleton {
		internal SkeletonData data;
		internal List<Bone> bones;
		internal List<Slot> slots;
		internal List<Slot> drawOrder;
		internal List<IkConstraint> ikConstraints;
		private List<List<Bone>> boneCache = new List<List<Bone>>();
		internal Skin skin;
		internal float r = 1, g = 1, b = 1, a = 1;
		internal float time;
		internal bool flipX, flipY;
		internal float x, y;

		public SkeletonData Data => data;

		public List<Bone> Bones => bones;

		public List<Slot> Slots => slots;

		public List<Slot> DrawOrder => drawOrder;

		public List<IkConstraint> IkConstraints {
			get => ikConstraints;
			set => ikConstraints = value;
		}

		public Skin Skin {
			get => skin;
			set => skin = value;
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

		public float Time {
			get => time;
			set => time = value;
		}

		public float X {
			get => x;
			set => x = value;
		}

		public float Y {
			get => y;
			set => y = value;
		}

		public bool FlipX {
			get => flipX;
			set => flipX = value;
		}

		public bool FlipY {
			get => flipY;
			set => flipY = value;
		}

		public Bone RootBone => bones.Count == 0 ? null : bones[0];

		public Skeleton(SkeletonData data) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			this.data = data;

			bones = new List<Bone>(data.bones.Count);
			foreach (var boneData in data.bones) {
				var parent = boneData.parent == null ? null : bones[data.bones.IndexOf(boneData.parent)];
				var bone = new Bone(boneData, this, parent);
				if (parent != null) parent.children.Add(bone);
				bones.Add(bone);
			}

			slots = new List<Slot>(data.slots.Count);
			drawOrder = new List<Slot>(data.slots.Count);
			foreach (var slotData in data.slots) {
				var bone = bones[data.bones.IndexOf(slotData.boneData)];
				var slot = new Slot(slotData, bone);
				slots.Add(slot);
				drawOrder.Add(slot);
			}

			ikConstraints = new List<IkConstraint>(data.ikConstraints.Count);
			foreach (var ikConstraintData in data.ikConstraints)
				ikConstraints.Add(new IkConstraint(ikConstraintData, this));

			UpdateCache();
		}

		/// <summary>Caches information about bones and IK constraints. Must be called if bones or IK constraints are added or
		/// removed.</summary>
		public void UpdateCache() {
			var boneCache = this.boneCache;
			var ikConstraints = this.ikConstraints;
			var ikConstraintsCount = ikConstraints.Count;

			var arrayCount = ikConstraintsCount + 1;
			if (boneCache.Count > arrayCount) boneCache.RemoveRange(arrayCount, boneCache.Count - arrayCount);
			for (int i = 0, n = boneCache.Count; i < n; i++)
				boneCache[i].Clear();
			while (boneCache.Count < arrayCount)
				boneCache.Add(new List<Bone>());

			var nonIkBones = boneCache[0];

			for (int i = 0, n = bones.Count; i < n; i++) {
				var bone = bones[i];
				var current = bone;
				do {
					for (var ii = 0; ii < ikConstraintsCount; ii++) {
						var ikConstraint = ikConstraints[ii];
						var parent = ikConstraint.bones[0];
						var child = ikConstraint.bones[ikConstraint.bones.Count - 1];
						while (true) {
							if (current == child) {
								boneCache[ii].Add(bone);
								boneCache[ii + 1].Add(bone);
								goto outer;
							}

							if (child == parent) break;
							child = child.parent;
						}
					}

					current = current.parent;
				} while (current != null);

				nonIkBones.Add(bone);
				outer:
				{
				}
			}
		}

		/// <summary>Updates the world transform for each bone and applies IK constraints.</summary>
		public void UpdateWorldTransform() {
			var bones = this.bones;
			for (int ii = 0, nn = bones.Count; ii < nn; ii++) {
				var bone = bones[ii];
				bone.rotationIK = bone.rotation;
			}

			var boneCache = this.boneCache;
			var ikConstraints = this.ikConstraints;
			int i = 0, last = boneCache.Count - 1;
			while (true) {
				var updateBones = boneCache[i];
				for (int ii = 0, nn = updateBones.Count; ii < nn; ii++)
					updateBones[ii].UpdateWorldTransform();
				if (i == last) break;
				ikConstraints[i].apply();
				i++;
			}
		}

		/// <summary>Sets the bones and slots to their setup pose values.</summary>
		public void SetToSetupPose() {
			SetBonesToSetupPose();
			SetSlotsToSetupPose();
		}

		public void SetBonesToSetupPose() {
			var bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++)
				bones[i].SetToSetupPose();

			var ikConstraints = this.ikConstraints;
			for (int i = 0, n = ikConstraints.Count; i < n; i++) {
				var ikConstraint = ikConstraints[i];
				ikConstraint.bendDirection = ikConstraint.data.bendDirection;
				ikConstraint.mix = ikConstraint.data.mix;
			}
		}

		public void SetSlotsToSetupPose() {
			var slots = this.slots;
			drawOrder.Clear();
			drawOrder.AddRange(slots);
			for (int i = 0, n = slots.Count; i < n; i++)
				slots[i].SetToSetupPose(i);
		}

		/// <returns>May be null.</returns>
		public Bone FindBone(string boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName cannot be null.");
			var bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++) {
				var bone = bones[i];
				if (bone.data.name == boneName) return bone;
			}

			return null;
		}

		/// <returns>-1 if the bone was not found.</returns>
		public int FindBoneIndex(string boneName) {
			if (boneName == null) throw new ArgumentNullException("boneName cannot be null.");
			var bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++)
				if (bones[i].data.name == boneName)
					return i;
			return -1;
		}

		/// <returns>May be null.</returns>
		public Slot FindSlot(string slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			var slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++) {
				var slot = slots[i];
				if (slot.data.name == slotName) return slot;
			}

			return null;
		}

		/// <returns>-1 if the bone was not found.</returns>
		public int FindSlotIndex(string slotName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			var slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++)
				if (slots[i].data.name.Equals(slotName))
					return i;
			return -1;
		}

		/// <summary>Sets a skin by name (see SetSkin).</summary>
		public void SetSkin(string skinName) {
			var skin = data.FindSkin(skinName);
			if (skin == null) throw new ArgumentException("Skin not found: " + skinName);
			SetSkin(skin);
		}

		/// <summary>Sets the skin used to look up attachments before looking in the {@link SkeletonData#getDefaultSkin() default 
		/// skin}. Attachmentsfrom the new skin are attached if the corresponding attachment from the old skin was attached. If 
		/// there was no old skin, each slot's setup mode attachment is attached from the new skin.</summary>
		/// <param name="newSkin">May be null.</param>
		public void SetSkin(Skin newSkin) {
			if (newSkin != null) {
				if (skin != null) {
					newSkin.AttachAll(this, skin);
				} else {
					var slots = this.slots;
					for (int i = 0, n = slots.Count; i < n; i++) {
						var slot = slots[i];
						var name = slot.data.attachmentName;
						if (name != null) {
							var attachment = newSkin.GetAttachment(i, name);
							if (attachment != null) slot.Attachment = attachment;
						}
					}
				}
			}

			skin = newSkin;
		}

		/// <returns>May be null.</returns>
		public Attachment GetAttachment(string slotName, string attachmentName) {
			return GetAttachment(data.FindSlotIndex(slotName), attachmentName);
		}

		/// <returns>May be null.</returns>
		public Attachment GetAttachment(int slotIndex, string attachmentName) {
			if (attachmentName == null) throw new ArgumentNullException("attachmentName cannot be null.");
			if (skin != null) {
				var attachment = skin.GetAttachment(slotIndex, attachmentName);
				if (attachment != null) return attachment;
			}

			if (data.defaultSkin != null) return data.defaultSkin.GetAttachment(slotIndex, attachmentName);
			return null;
		}

		/// <param name="attachmentName">May be null.</param>
		public void SetAttachment(string slotName, string attachmentName) {
			if (slotName == null) throw new ArgumentNullException("slotName cannot be null.");
			var slots = this.slots;
			for (int i = 0, n = slots.Count; i < n; i++) {
				var slot = slots[i];
				if (slot.data.name == slotName) {
					Attachment attachment = null;
					if (attachmentName != null) {
						attachment = GetAttachment(i, attachmentName);
						if (attachment == null)
							throw new Exception("Attachment not found: " + attachmentName + ", for slot: " + slotName);
					}

					slot.Attachment = attachment;
					return;
				}
			}

			throw new Exception("Slot not found: " + slotName);
		}

		/** @return May be null. */
		public IkConstraint FindIkConstraint(string ikConstraintName) {
			if (ikConstraintName == null) throw new ArgumentNullException("ikConstraintName cannot be null.");
			var ikConstraints = this.ikConstraints;
			for (int i = 0, n = ikConstraints.Count; i < n; i++) {
				var ikConstraint = ikConstraints[i];
				if (ikConstraint.data.name == ikConstraintName) return ikConstraint;
			}

			return null;
		}

		public void Update(float delta) {
			time += delta;
		}
	}
}