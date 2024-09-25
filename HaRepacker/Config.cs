using Config.Net;

namespace HaRepacker {
	public interface Config {
		[Option(DefaultValue = CopyMode.Object)]
		public CopyMode CopyMode { get; set; }
	}
}