using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleLib.PacketLib {
	public static class HexTool {
		private static char[] HEX = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		/// <summary>
		/// Converts a byte value to readable hex representation
		/// </summary>
		/// <param name="byteValue"></param>
		/// <returns></returns>
		public static string ToString(byte byteValue) {
			var tmp = byteValue << 8;
			var retstr = new char[] {HEX[(tmp >> 12) & 0x0F], HEX[(tmp >> 8) & 0x0F]};
			return new string(retstr);
		}

		/// <summary>
		/// Converts an array of bytes to readable hex representation
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string ToString(byte[] bytes) {
			var hexed = new StringBuilder();
			for (var i = 0; i < bytes.Length; i++) {
				hexed.Append(ToString(bytes[i]));
				hexed.Append(' ');
			}

			return hexed.ToString();
		}


		/// <summary>
		/// Converts an array of bytes to readable hex representation
		/// Extension method for PacketWriter 
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string ToString(this PacketReader reader) {
			var bytes = reader.ToArray();

			var hexed = new StringBuilder();
			for (var i = 0; i < bytes.Length; i++) {
				hexed.Append(ToString(bytes[i]));
				hexed.Append(' ');
			}

			return hexed.ToString();
		}

		/// <summary>
		/// Converts an array of bytes to readable hex representation
		/// Extension method for PacketWriter 
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string ToString(this PacketWriter writer) {
			var bytes = writer.ToArray();

			var hexed = new StringBuilder();
			for (var i = 0; i < bytes.Length; i++) {
				hexed.Append(ToString(bytes[i]));
				hexed.Append(' ');
			}

			return hexed.ToString();
		}

		public static string ByteArrayToString(byte[] ba) {
			var hex = new StringBuilder(ba.Length * 3);
			foreach (var b in ba)
				hex.AppendFormat("{0:x2} ", b);
			return hex.ToString();
		}
	}
}