using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Image = System.Windows.Controls.Image;

namespace MonoGame.WpfControl {
	/// <summary>
	/// Specifies the rendering mode of the D3D11Host.
	/// </summary>
	public enum RenderMode {
		Continuous,
		Manual
	}

	/// <summary>
	/// Host a Direct3D 11 scene.
	/// </summary>
	public class D3D11Host : Image, IDisposable {
		#region Fields

		private static readonly object _graphicsDeviceLock = new();
		private readonly Stopwatch _timer;
		private static GraphicsDevice _graphicsDevice;
		private static bool? _isInDesignMode;
		private static int _referenceCount;
		private D3D11Image _d3D11Image;
		private bool _disposed;
		private TimeSpan _lastRenderingTime;
		private bool _loaded;
		private RenderTarget2D _renderTarget;
		private bool _resetBackBuffer;
		private TimeSpan _timeSinceStart = TimeSpan.Zero;
		private bool _isDirty = true;

		#endregion

		#region Constructors

		public D3D11Host() {
			Stretch = Stretch.Uniform;
			_timer = new Stopwatch();
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
		}

		~D3D11Host() {
			Dispose(false);
		}

		#endregion

		#region Properties

		public static bool IsInDesignMode {
			get {
				if (!_isInDesignMode.HasValue) {
					_isInDesignMode = (bool) DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement)).Metadata.DefaultValue;
				}

				return _isInDesignMode.Value;
			}
		}

		public GraphicsDevice GraphicsDevice => _graphicsDevice;

		public GameServiceContainer Services { get; } = new();

		/// <summary>
		/// Gets or sets the rendering mode.
		/// </summary>
		public virtual RenderMode RenderMode { get; set; } = RenderMode.Continuous;

		#endregion

		#region Methods

		public void Dispose() {
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing) {
			if (_disposed) {
				return;
			}

			_disposed = true;
		}

		protected virtual void Initialize() {
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
			_resetBackBuffer = true;
			_isDirty = true; // Mark as dirty on resize
			base.OnRenderSizeChanged(sizeInfo);

			if (RenderMode == RenderMode.Manual) {
				RenderIfDirty();
			}
		}
		protected virtual void Render(GameTime time) {
		}

		private static void InitializeGraphicsDevice() {
			lock (_graphicsDeviceLock) {
				_referenceCount++;
				if (_referenceCount == 1) {
					var presentationParameters = new PresentationParameters {
						BackBufferWidth = 1,
						BackBufferHeight = 1,
						BackBufferFormat = SurfaceFormat.Color,
						DepthStencilFormat = DepthFormat.Depth24Stencil8,
						DeviceWindowHandle = IntPtr.Zero,
						IsFullScreen = false
					};
					_graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, presentationParameters);
				}
			}
		}

		private static void UninitializeGraphicsDevice() {
			lock (_graphicsDeviceLock) {
				_referenceCount--;
				if (_referenceCount == 0) {
					_graphicsDevice.Dispose();
					_graphicsDevice = null;
				}
			}
		}

		private void CreateBackBuffer() {
			_d3D11Image.SetBackBuffer(null);
			_renderTarget?.Dispose();
			var width = Math.Max((int) ActualWidth, 1);
			var height = Math.Max((int) ActualHeight, 1);
			_renderTarget = new RenderTarget2D(_graphicsDevice, width, height, false, SurfaceFormat.Bgra32, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents, true);
			_d3D11Image.SetBackBuffer(_renderTarget);
		}

		private void InitializeImageSource() {
			_d3D11Image = new D3D11Image();
			_d3D11Image.IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;
			CreateBackBuffer();
			Source = _d3D11Image;
		}

		private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs eventArgs) {
			if (_d3D11Image.IsFrontBufferAvailable) {
				StartRendering();
				_resetBackBuffer = true;
			} else {
				StopRendering();
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs eventArgs) {
			if (IsInDesignMode || _loaded) {
				return;
			}

			_loaded = true;
			InitializeGraphicsDevice();
			InitializeImageSource();
			Initialize();
			StartRendering();
		}

		private void OnRendering(object sender, EventArgs eventArgs) {
			// Ensure the timer is running for continuous mode
			if (RenderMode == RenderMode.Continuous && !_timer.IsRunning) {
				_timer.Start();
			} else if (RenderMode == RenderMode.Manual && _timer.IsRunning) {
				_timer.Stop();
			}

			// Check if rendering is necessary
			if (RenderMode == RenderMode.Manual && !_isDirty && !_resetBackBuffer) {
				return;
			}

			// Recreate back buffer if necessary
			if (_resetBackBuffer) {
				CreateBackBuffer();
				_resetBackBuffer = false;
			}

			// Only render if dirty or in continuous mode
			if (_isDirty || RenderMode == RenderMode.Continuous) {
				GraphicsDevice.SetRenderTarget(_renderTarget);
				var diff = _timer.Elapsed - _timeSinceStart;
				_timeSinceStart = _timer.Elapsed;
				Render(new GameTime(_timer.Elapsed, diff));
				GraphicsDevice.Flush();

				_isDirty = false; // Reset dirty flag after rendering
			}

			// Always invalidate the D3D11Image to update the display
			_d3D11Image.Invalidate();
		}

		private void OnUnloaded(object sender, RoutedEventArgs eventArgs) {
			if (IsInDesignMode) {
				return;
			}

			StopRendering();
			Dispose();
			UnitializeImageSource();
			UninitializeGraphicsDevice();
		}

		private void StartRendering() {
			if (RenderMode == RenderMode.Continuous) {
				if (!_timer.IsRunning) {
					CompositionTarget.Rendering += OnRendering;
					_timer.Start();
				}
			} else if (RenderMode == RenderMode.Manual) {
				// Directly trigger rendering in manual mode
				RenderIfDirty();
			}
		}

		private void StopRendering() {
			if (RenderMode == RenderMode.Continuous && _timer.IsRunning) {
				CompositionTarget.Rendering -= OnRendering;
				_timer.Stop();
			}
			// In manual mode, there's no need to stop the rendering since it's manually controlled.
			// The timer is already not running in manual mode, so nothing additional is needed.
		}

		private void UnitializeImageSource() {
			_d3D11Image.IsFrontBufferAvailableChanged -= OnIsFrontBufferAvailableChanged;
			Source = null;

			_d3D11Image?.Dispose();
			_d3D11Image = null;
			_renderTarget?.Dispose();
			_renderTarget = null;
		}

		/// <summary>
		/// Marks the control as dirty, meaning it needs to be re-rendered.
		/// </summary>
		public void MarkAsDirty() {
			_isDirty = true;

			if (RenderMode == RenderMode.Manual) {
				RenderIfDirty();
			}
		}

		private void RenderIfDirty() {
			if (_isDirty || _resetBackBuffer) {
				// Manually invoke the rendering process
				OnRendering(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}