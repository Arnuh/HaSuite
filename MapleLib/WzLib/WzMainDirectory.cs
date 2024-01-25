/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace MapleLib.WzLib {
	public class WzMainDirectory {
		private readonly WzFile file;
		private readonly WzDirectory directory;

		/// <summary>
		/// Constructor for oridinary Wz file
		/// </summary>
		/// <param name="file"></param>
		public WzMainDirectory(WzFile file) {
			this.file = file;
			directory = file.WzDirectory;
		}

		/// <summary>
		/// Constructor for hotfix Data.wz file
		/// </summary>
		/// <param name="file"></param>
		/// <param name="directory"></param>
		public WzMainDirectory(WzFile file, WzDirectory directory) {
			this.file = file;
			this.directory = directory;
		}

		public WzFile File => file;

		public WzDirectory MainDir => directory;
	}
}