using Microsoft.Xna.Framework.Graphics;
using MonoGame.WpfControl;

namespace HaCreator.MapEditor.MonoGame {
	public class WinformGraphicsDeviceService : IGraphicsDeviceService {
		#region Constructors

		public WinformGraphicsDeviceService(WinformRenderer host) {
			if (host == null) {
				throw new ArgumentNullException(nameof(host));
			}

			if (host.Services.GetService(typeof(IGraphicsDeviceService)) != null) {
				throw new NotSupportedException("A graphics device service is already registered.");
			}

			GraphicsDevice = host.GraphicsDevice;
			host.Services.AddService(typeof(IGraphicsDeviceService), this);
		}

		#endregion

		#region Events

		[Obsolete("Dummy implementation will never call DeviceCreated")]
		public event EventHandler<EventArgs> DeviceCreated;

		[Obsolete("Dummy implementation will never call DeviceDisposing")]
		public event EventHandler<EventArgs> DeviceDisposing;

		[Obsolete("Dummy implementation will never call DeviceReset")]
		public event EventHandler<EventArgs> DeviceReset;

		[Obsolete("Dummy implementation will never call DeviceResetting")]
		public event EventHandler<EventArgs> DeviceResetting;

		#endregion

		#region Properties

		public GraphicsDevice GraphicsDevice { get; }

		#endregion
	}
}