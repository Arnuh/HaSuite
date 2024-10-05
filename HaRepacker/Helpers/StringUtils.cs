namespace HaRepacker.Helpers {
	public class StringUtils {
		public static string ToFileSize(long value, FileSizeUnits unit) {
			return (value / Math.Pow(1024, (long) unit)).ToString("0.00");
		}
	}
}