namespace MapleLib.Helpers {
	public class StringUtility {
		public static string CapitalizeFirstCharacter(string x) {
			if (x.Length > 0 && char.IsLower(x[0])) {
				return new string(new char[] {char.ToUpper(x[0])}) + x.Substring(1);
			}

			return x;
		}
	}
}