namespace MapleLib.Helpers {
	public static class ReplaceResultMethods {
		
		public static bool IsRemoval(this ReplaceResult result) {
			return result == ReplaceResult.Yes || result == ReplaceResult.YesToAll;
		}
		
		public static bool IsSuccess(this ReplaceResult result) {
			return result == ReplaceResult.Yes || result == ReplaceResult.YesToAll || result == ReplaceResult.Rename;
		}
	}
}