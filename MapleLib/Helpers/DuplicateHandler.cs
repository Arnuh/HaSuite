using System;
using MapleLib.WzLib;

namespace MapleLib.Helpers {
	public class DuplicateHandler {
		private bool yesToAll;
		private bool noToAll;
		private Func<WzObject, ReplaceResult> onDuplicateEntry;
		private Action<WzObject> onRenameEntry;

		public bool YesToAll {
			get => yesToAll;
			set => yesToAll = value;
		}

		public bool NoToAll {
			get => noToAll;
			set => noToAll = value;
		}

		public Func<WzObject, ReplaceResult> OnDuplicateEntry {
			get => onDuplicateEntry;
			set => onDuplicateEntry = value;
		}

		public Action<WzObject> OnRenameEntry {
			get => onRenameEntry;
			set => onRenameEntry = value;
		}

		public bool HandleResult(WzObject parent, WzObject obj, ReplaceResult result) {
			switch (result) {
				case ReplaceResult.NoToAll:
					noToAll = true;
					return false;
				case ReplaceResult.No:
					return false;
				case ReplaceResult.YesToAll:
					parent[obj.Name].Remove();
					yesToAll = true;
					return true;
				case ReplaceResult.Yes:
					parent[obj.Name].Remove();
					return true;
				case ReplaceResult.Rename:
					OnRenameEntry?.Invoke(obj);
					return true;
			}

			return false;
		}

		public bool Handle(WzObject parent, WzObject obj) {
			if (YesToAll) {
				parent[obj.Name].Remove();
				return true;
			}

			if (NoToAll) {
				return false;
			}

			if (OnDuplicateEntry == null) {
				return false;
			}

			var result = OnDuplicateEntry.Invoke(obj);
			return HandleResult(parent, obj, result);
		}
	}
}