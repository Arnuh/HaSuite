/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2015 haha01haha01 and contributors

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

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace MapleLib.WzLib.Util {
	public class WzMutableKey {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="WzIv"></param>
		/// <param name="AesKey">The 32-byte AES UserKey (derived from 32 DWORD)</param>
		public WzMutableKey(byte[] WzIv, byte[] AesKey) {
			IV = WzIv;
			AESUserKey = AesKey;
		}

		private static readonly int BatchSize = 4096;
		private readonly byte[] IV;
		private readonly byte[] AESUserKey;

		private byte[] keys;

		public byte this[int index] {
			get {
				if (keys == null || keys.Length <= index) EnsureKeySize(index + 1);

				return keys[index];
			}
		}

		public void EnsureKeySize(int size) {
			if (keys != null && keys.Length >= size) return;

			size = (int) Math.Ceiling(1.0 * size / BatchSize) * BatchSize;
			var newKeys = new byte[size];

			if (BitConverter.ToInt32(IV, 0) == 0) {
				keys = newKeys;
				return;
			}

			var startIndex = 0;

			if (keys != null) {
				Buffer.BlockCopy(keys, 0, newKeys, 0, keys.Length);
				startIndex = keys.Length;
			}

			var aes = Rijndael.Create();
			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Key = AESUserKey;
			aes.Mode = CipherMode.ECB;
			var ms = new MemoryStream(newKeys, startIndex, newKeys.Length - startIndex, true);
			var s = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);

			for (var i = startIndex; i < size; i += 16) {
				if (i == 0) {
					var block = new byte[16];
					for (var j = 0; j < block.Length; j++) block[j] = IV[j % 4];

					s.Write(block, 0, block.Length);
				} else {
					s.Write(newKeys, i - 16, 16);
				}
			}

			s.Flush();
			ms.Close();
			keys = newKeys;
		}

		public byte[] CopyIv() {
			var ret = new byte[IV.Length];
			IV.CopyTo(ret, 0);
			return ret;
		}

		public byte[] CopyUserKey() {
			var ret = new byte[AESUserKey.Length];
			AESUserKey.CopyTo(ret, 0);
			return ret;
		}

		public bool Equals(WzMutableKey compare) {
			if (!IV.SequenceEqual(compare.IV)) return false;
			if (!AESUserKey.SequenceEqual(compare.AESUserKey)) return false;
			return true;
		}
	}
}