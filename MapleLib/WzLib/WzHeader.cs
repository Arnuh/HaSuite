﻿/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2009, 2010, 2015 Snow and haha01haha01

 * This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

namespace MapleLib.WzLib {
	public class WzHeader {
		private const string DEFAULT_WZ_HEADER_COPYRIGHT = "Package file v1.0 Copyright 2002 Wizet, ZMS";

		private string ident;
		private string copyright;
		private ulong fsize;
		private uint fstart;

		public string Ident {
			get => ident;
			set => ident = value;
		}

		/// <summary>
		/// see: DEFAULT_WZ_HEADER_COPYRIGHT
		/// </summary>
		public string Copyright {
			get => copyright;
			set => copyright = value;
		}

		public ulong FSize {
			get => fsize;
			set => fsize = value;
		}

		public uint FStart {
			get => fstart;
			set => fstart = value;
		}

		public void RecalculateFileStart() {
			fstart = (uint) (ident.Length + sizeof(ulong) + sizeof(uint) + copyright.Length + 1);
		}

		public static WzHeader GetDefault() {
			var header = new WzHeader {
				ident = "PKG1",
				copyright = DEFAULT_WZ_HEADER_COPYRIGHT,
				fstart = 60,
				fsize = 0
			};
			return header;
		}
	}
}