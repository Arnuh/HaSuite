/*  MapleLib - A general-purpose MapleStory library
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

using System.IO;
using MapleLib.MapleCryptoLib;

namespace MapleLib.WzLib.Util {
	public class WzKeyGenerator {
		#region Methods

		public static byte[] GetIvFromZlz(FileStream zlzStream) {
			var iv = new byte[4];

			zlzStream.Seek(0x10040, SeekOrigin.Begin);
			zlzStream.Read(iv, 0, 4);
			return iv;
		}

		private static byte[] GetAesKeyFromZlz(FileStream zlzStream) {
			var aes = new byte[32];

			zlzStream.Seek(0x10060, SeekOrigin.Begin);
			for (var i = 0; i < 8; i++) {
				zlzStream.Read(aes, i * 4, 4);
				zlzStream.Seek(12, SeekOrigin.Current);
			}

			return aes;
		}

		/// <summary>
		/// Generates the WZ Key for .Lua property
		/// </summary>
		/// <returns></returns>
		public static WzMutableKey GenerateLuaWzKey() {
			return new WzMutableKey(
				MapleCryptoConstants.WZ_MSEAIV,
				MapleCryptoConstants.GetTrimmedUserKey(ref MapleCryptoConstants.MAPLESTORY_USERKEY_DEFAULT));
		}

		public static WzMutableKey GenerateWzKey(byte[] WzIv) {
			return GenerateWzKey(WzIv, MapleCryptoConstants.UserKey_WzLib);
		}

		public static WzMutableKey GenerateWzKey(byte[] WzIv, byte[] UserKey) {
			return new WzMutableKey(WzIv,
				MapleCryptoConstants.GetTrimmedUserKey(ref UserKey));
		}

		#endregion
	}
}