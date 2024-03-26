using System;

namespace MapleLib.Helpers {
	public class DuplicateHandler {
		private bool yesToAll;
		private bool noToAll;
		private Func<string, ReplaceResult> onDuplicateEntry;

		public bool YesToAll {
			get => yesToAll;
			set => yesToAll = value;
		}

		public bool NoToAll {
			get => noToAll;
			set => noToAll = value;
		}

		public Func<string, ReplaceResult> OnDuplicateEntry {
			get => onDuplicateEntry;
			set => onDuplicateEntry = value;
		}

		public bool HandleResult(ReplaceResult result) {
			switch (result) {
				case ReplaceResult.NoToAll:
					noToAll = true;
					return false;
				case ReplaceResult.No:
					return false;
				case ReplaceResult.YesToAll:
					yesToAll = true;
					return true;
				case ReplaceResult.Yes:
					return true;
			}

			return false;
		}

		public bool Handle(string name) {
			if (YesToAll) {
				return true;
			}

			if (NoToAll) {
				return false;
			}

			if (OnDuplicateEntry == null) {
				return false;
			}

			var result = OnDuplicateEntry.Invoke(name);
			return HandleResult(result);
		}
	}
}