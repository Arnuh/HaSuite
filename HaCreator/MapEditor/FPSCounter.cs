﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Threading;

namespace HaCreator.MapEditor {
	public class FPSCounter : IDisposable {
		private Thread resetThread;
		private int frames;
		private int frames_next;

		public FPSCounter() {
			resetThread = new Thread(new ThreadStart(delegate {
				while (!Program.AbortThreads) {
					frames = Interlocked.Exchange(ref frames_next, 0);
					Thread.Sleep(1000);
				}
			}));
			resetThread.Start();
		}

		public void Tick() {
			Interlocked.Increment(ref frames_next);
		}

		public int Frames => frames;

		public void Dispose() {
			if (resetThread != null) {
				resetThread.Join();
				resetThread = null;
			}
		}
	}
}