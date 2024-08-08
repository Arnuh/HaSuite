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

		public ReplaceResult HandleResult(WzObject parent, WzObject obj, ReplaceResult result) {
			switch (result) {
				case ReplaceResult.NoToAll:
					noToAll = true;
					return result;
				case ReplaceResult.No:
					return result;
				case ReplaceResult.YesToAll:
					parent[obj.Name].Remove();
					yesToAll = true;
					return result;
				case ReplaceResult.Yes:
					parent[obj.Name].Remove();
					return result;
				case ReplaceResult.Rename:
					OnRenameEntry?.Invoke(obj);
					return result;
			}

			return result;
		}

		public ReplaceResult Handle(WzObject parent, WzObject obj) {
			if (YesToAll) {
				parent[obj.Name].Remove();
				return ReplaceResult.YesToAll;
			}

			if (NoToAll) {
				return ReplaceResult.NoToAll;
			}

			if (OnDuplicateEntry == null) {
				return ReplaceResult.No;
			}

			var result = OnDuplicateEntry.Invoke(obj);
			return HandleResult(parent, obj, result);
		}
	}
}