﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace HaSharedLibrary.SharpApng {
	public class SharpApng : IDisposable {
		private readonly List<SharpApngFrame> m_frames = new List<SharpApngFrame>();

		public void Dispose() {
			foreach (var frame in m_frames)
				frame.Dispose();
			m_frames.Clear();
		}

		public SharpApngFrame this[int index] {
			get {
				if (index < m_frames.Count) {
					return m_frames[index];
				}

				return null;
			}
			set {
				if (index < m_frames.Count) m_frames[index] = value;
			}
		}

		public void AddFrame(SharpApngFrame frame) {
			m_frames.Add(frame);
		}

		public void AddFrame(Bitmap bmp, int num, int den) {
			m_frames.Add(new SharpApngFrame(bmp, num, den));
		}

		private Bitmap ExtendImage(Bitmap source, Size newSize) {
			var result = new Bitmap(newSize.Width, newSize.Height);
			using (var g = Graphics.FromImage(result)) {
				g.DrawImageUnscaled(source, 0, 0);
			}

			return result;
		}

		public void WriteApng(string path, bool firstFrameHidden, bool disposeAfter) {
			var maxSize = new Size();
			foreach (var frame in m_frames) {
				if (frame.Bitmap.Width > maxSize.Width) maxSize.Width = frame.Bitmap.Width;
				if (frame.Bitmap.Height > maxSize.Height) maxSize.Height = frame.Bitmap.Height;
			}

			for (var i = 0; i < m_frames.Count; i++) {
				var frame = m_frames[i];
				if (frame.Bitmap.Width != maxSize.Width || frame.Bitmap.Height != maxSize.Height) {
					frame.Bitmap = ExtendImage(frame.Bitmap, maxSize);
				}

				SharpApngBasicWrapper.CreateFrameManaged(frame.Bitmap, frame.DelayNum, frame.DelayDen, i);
			}

			SharpApngBasicWrapper.SaveApngManaged(path, m_frames.Count, maxSize.Width, maxSize.Height,
				firstFrameHidden);

			if (disposeAfter) {
				Dispose();
			}
		}
	}
}