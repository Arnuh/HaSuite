﻿using System;

namespace MapleLib.Helpers.Exceptions {
	[Serializable]
	public class ParseException : Exception {
		public ParseException() {
		}

		public ParseException(string message)
			: base(message) {
		}

		public ParseException(string message, Exception innerException)
			: base(message, innerException) {
		}
	}
}