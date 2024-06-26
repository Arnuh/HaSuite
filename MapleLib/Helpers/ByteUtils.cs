﻿using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace MapleLib.Helpers {
	public static class ByteUtils {
		public static bool CompareBytearrays(byte[] a, byte[] b) {
			return a.Length == b.Length && a.SequenceEqual(b);
		}

		public static byte[] IntegerToLittleEndian(int data) {
			var b = BitConverter.GetBytes(data);
			if (!BitConverter.IsLittleEndian) Array.Reverse(b);

			return b;
		}

		public static byte[] HexToBytes(string pValue) {
			// FIRST. Use StringBuilder.
			var builder = new StringBuilder();
			// SECOND... USE STRINGBUILDER!... and LINQ.
			foreach (var c in pValue.Where(IsHexDigit).Select(char.ToUpper)) builder.Append(c);

			// THIRD. If you have an odd number of characters, something is very wrong.
			var hexString = builder.ToString();
			if (hexString.Length % 2 == 1)
				//throw new InvalidOperationException("There is an odd number of hexadecimal digits in this string.");
				// I will just add a zero to the end, who cares (0 padding)
				//      Log.WriteLine(LogLevel.Debug, "Hexstring had an odd number of hexadecimal digits.");
			{
				hexString += '0';
			}

			var bytes = new byte[hexString.Length / 2];
			var rand = new Random();
			// FOURTH. Use the for-loop like a pro :D
			for (int i = 0, j = 0; i < bytes.Length; i++, j += 2) {
				var byteString = string.Concat(hexString[j], hexString[j + 1]);
				if (byteString == "**") {
					bytes[i] = (byte) rand.Next(0, byte.MaxValue);
				} else {
					bytes[i] = HexToByte(byteString);
				}
			}

			return bytes;
		}

		/// <summary>
		/// Creates a hex-string from byte array.
		/// </summary>
		/// <param name="bytes">Input bytes.</param>
		/// <returns>String that represents the byte-array.</returns>
		public static string BytesToHex(byte[] bytes, string header = "") {
			var builder = new StringBuilder(header);
			foreach (var c in bytes) builder.AppendFormat("{0:X2} ", c);

			return builder.ToString();
		}

		/// <summary>
		/// Checks if a character is a hexadecimal digit.
		/// </summary>
		/// <param name="c">The character to check</param>
		/// <returns>true if <paramref name="c"/>is a hexadecimal digit; otherwise, false.</returns>
		public static bool IsHexDigit(char c) {
			return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f') || c == '*';
		}

		/// <summary>
		/// Convert a 2-digit hexadecimal string to a byte.
		/// </summary>
		/// <param name="hex">The hexadecimal string.</param>
		/// <returns>The byte representation of the string.</returns>
		private static byte HexToByte(string hex) {
			if (hex == null) throw new ArgumentNullException("hex");
			if (hex.Length == 0 || 2 < hex.Length) {
				throw new ArgumentOutOfRangeException("hex",
					"The hexadecimal string must be 1 or 2 characters in length.");
			}

			var newByte = byte.Parse(hex, NumberStyles.HexNumber);
			return newByte;
		}

		public static byte[] StructToBytes<T>(T obj) {
			var result = new byte[Marshal.SizeOf(obj)];
			var handle = GCHandle.Alloc(result, GCHandleType.Pinned);
			try {
				Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), false);
				return result;
			} finally {
				handle.Free();
			}
		}

		public static T BytesToStruct<T>(byte[] data) where T : new() {
			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			try {
				return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
			} finally {
				handle.Free();
			}
		}

		public static T BytesToStructConstructorless<T>(byte[] data) {
			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			try {
				var obj = (T) FormatterServices.GetUninitializedObject(typeof(T));
				Marshal.PtrToStructure(handle.AddrOfPinnedObject(), obj);
				return obj;
			} finally {
				handle.Free();
			}
		}
	}
}