/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace HaCreator.MapEditor {
	public class Scheduler : IDisposable {
		private Dictionary<Action, int> clients;
		private Thread schedThread;

		public Scheduler(Dictionary<Action, int> clients) {
			this.clients = clients;
			if (clients.Count > 0) {
				schedThread = new Thread(SchedulerProc);
				schedThread.Start();
			}
		}

		private void SchedulerProc() {
			var sw = new Stopwatch();
			sw.Start();
			var nextTimes = new Dictionary<Action, int>(clients);
			while (!Program.AbortThreads) {
				// Get nearest action
				Action nearestAction = null;
				var nearestTime = int.MaxValue;
				foreach (var nextActionTime in nextTimes) {
					if (nextActionTime.Value < nearestTime) {
						nearestAction = nextActionTime.Key;
						nearestTime = nextActionTime.Value;
					}
				}

				// If we have spare time, sleep it
				var currTime = sw.ElapsedMilliseconds;
				if (currTime < nearestTime)
					// We can safely cast to int since nobody will ever add a timer with an interval > MAXINT
				{
					Thread.Sleep((int) (nearestTime - currTime));
				}

				// It is now guaranteed we are at (or past) the time needed to nearestAction, so we will execute it
				nearestAction.Invoke();
				// Update nearestAction's next wakeup time
				nextTimes[nearestAction] = nearestTime + clients[nearestAction];
			}
		}

		public void Dispose() {
			if (schedThread != null) {
				schedThread.Join();
				schedThread = null;
			}
		}
	}
}