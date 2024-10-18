using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using HaCreator.MapEditor.Text;
using HaSharedLibrary.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Point = System.Drawing.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace HaCreator.MapEditor.MonoGame {
	public partial class WinformRenderer : UserControl, Renderer {
		private MultiBoard _multiBoard;

		public MultiBoard MultiBoard {
			get => _multiBoard;
			set => _multiBoard = value;
		}

		private readonly PresentationParameters presParams = new();
		private IGraphicsDeviceService _graphicsDeviceManager;
		private GraphicsDevice _graphicsDevice;
		private SpriteBatch sprite;
		private FontEngine fontEngine;

		public bool DeviceReady { get; set; }

		public SpriteBatch Sprite => sprite;
		public FontEngine FontEngine => fontEngine;

		public Texture2D Pixel { get; set; }

		public double ActualHeight => Height;

		public double ActualWidth => Width;

		private Thread renderLoopThread;

		private bool needsReset;

		public WinformRenderer() {
			InitializeComponent();
			SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint, true);
			InitializeGraphicsDevice();
			Initialize();
			DragEnter += (sender, e) => _multiBoard.Device_DragEnter(sender, e);
			DragDrop += (sender, e) => _multiBoard.Device_DragDrop(sender, e);
			PreviewKeyDown += (sender, e) => _multiBoard.Device_PreviewKeyDownEventArgs(sender, e);
			KeyDown += (sender, e) => _multiBoard.Device_KeyDownEventArgs(sender, e);
			MouseClick += (sender, e) => _multiBoard.Device_MouseClick(sender, e);
			MouseDoubleClick += (sender, e) => _multiBoard.Device_MouseDoubleClick(sender, e);
			MouseDown += (sender, e) => _multiBoard.Device_MouseDown(sender, e);
			MouseMove += (sender, e) => _multiBoard.Device_MouseMove(sender, e);
			MouseUp += (sender, e) => _multiBoard.Device_MouseUp(sender, e);
			MouseWheel += (sender, e) => _multiBoard.Device_MouseWheel(sender, e);
		}

		public void Start() {
			DeviceReady = true;
		}

		public void OnSizeChanged() {
			needsReset = true;
		}

		public Texture2D CreatePixel() {
			var bmp = new Bitmap(1, 1);
			bmp.SetPixel(0, 0, System.Drawing.Color.White);

			return bmp.ToTexture2D(GraphicsDevice);
		}

		protected void Initialize() {
			_graphicsDeviceManager = new WinformGraphicsDeviceService(this);
			_graphicsDevice = _graphicsDeviceManager.GraphicsDevice;

			sprite = new SpriteBatch(_graphicsDevice);
			fontEngine = new FontEngine(UserSettings.FontName, UserSettings.FontStyle, UserSettings.FontSize, _graphicsDevice);
			Pixel = CreatePixel();
			renderLoopThread = new Thread(RenderLoop);
			renderLoopThread.Start();
		}

		private void RenderLoop() {
			while (!Program.AbortThreads) {
				if (DeviceReady && _multiBoard.CurrentHostWindowState != WindowState.Minimized) {
					Draw();
#if FPS_TEST
                    fpsCounter.Tick();
#endif
				} else {
					Thread.Sleep(100);
				}
			}
		}

		protected void Draw() {
			if (needsReset) {
				needsReset = false;

				//Dispatcher.Invoke(delegate { AdjustScrollBars(); });
				ResetDevice();
			}

			var selectedBoard = _multiBoard.SelectedBoard;
			_graphicsDevice.Clear(ClearOptions.Target, UserSettings.altBackground ? UserSettings.altBackgroundColor : Color.White, 1.0f, 0);
#if UseXNAZorder
            sprite.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.FrontToBack, SaveStateMode.None);
#else
			sprite.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null,
				Matrix.CreateScale((float) _multiBoard.Scale, (float) _multiBoard.Scale, 1f));
#endif
			if (selectedBoard != null) // No map selected to draw on
			{
				lock (_multiBoard) {
					if (selectedBoard != null) { // check again
						selectedBoard.Draw(this);
						if (selectedBoard.MapSize.X < _multiBoard.ActualWidth) {
							DrawLine(new Vector2(_multiBoard.MapSize.X, 0),
								new Vector2(_multiBoard.MapSize.X, (float) _multiBoard.ActualHeight), Color.Black);
						}

						if (selectedBoard.MapSize.Y < _multiBoard.ActualHeight) {
							DrawLine(new Vector2(0, _multiBoard.MapSize.Y),
								new Vector2((float) _multiBoard.ActualWidth, _multiBoard.MapSize.Y), Color.Black);
						}
					}
				}
			}
#if FPS_TEST
            fontEngine.DrawString(sprite, new System.Drawing.Point(), Color.Black, fpsCounter.Frames.ToString(), 1000);
#endif
			sprite.End();
			try {
				_graphicsDevice.Present();
			} catch (DeviceLostException) {
			} catch (DeviceNotResetException) {
				needsReset = true;
			}
		}

		public void DrawLine(Vector2 start, Vector2 end, Color color) {
			var width = (int) Vector2.Distance(start, end);
			var rotation = (float) Math.Atan2(end.Y - start.Y, end.X - start.X);
			Sprite.Draw(Pixel, new Rectangle((int) start.X, (int) start.Y, width, UserSettings.LineWidth), null, color,
				rotation, new Vector2(0f, 0f), SpriteEffects.None, 1f);
		}

		public void DrawRectangle(Rectangle rectangle, Color color) {
			//clockwise
			var pt1 = new Vector2(rectangle.Left, rectangle.Top);
			var pt2 = new Vector2(rectangle.Right, rectangle.Top);
			var pt3 = new Vector2(rectangle.Right, rectangle.Bottom);
			var pt4 = new Vector2(rectangle.Left, rectangle.Bottom);

			DrawLine(pt1, pt2, color);
			DrawLine(pt2, pt3, color);
			DrawLine(pt3, pt4, color);
			DrawLine(pt4, pt1, color);
		}

		public void FillRectangle(Rectangle rectangle, Color color) {
			Sprite.Draw(Pixel, rectangle, color);
		}

		public void DrawDot(int x, int y, Color color, int dotSize) {
			var dotW = UserSettings.DotWidth * dotSize;
			FillRectangle(new Rectangle(x - dotW, y - dotW, dotW * 2, dotW * 2), color);
		}

		public void DrawString(string str, int x, int y) {
			FontEngine.DrawString(Sprite, new Point(x, y), Color.Black, str, 1000);
		}

		public void DrawString(Point position, Color color, string str, int maxWidth) {
			FontEngine.DrawString(Sprite, position, color, str, maxWidth);
		}

		public void Draw(
			Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effects,
			float layerDepth) {
			Sprite.Draw(texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth);
		}

		public GraphicsDevice GraphicsDevice => _graphicsDevice;

		public GameServiceContainer Services { get; } = new();

		private void InitializeGraphicsDevice() {
			presParams.BackBufferWidth = (int) Math.Max(ActualWidth, 1);
			presParams.BackBufferHeight = (int) Math.Max(ActualHeight, 1);
			presParams.BackBufferFormat = SurfaceFormat.Color;
			presParams.DepthStencilFormat = DepthFormat.Depth24Stencil8;
			presParams.DeviceWindowHandle = Handle;
			presParams.IsFullScreen = false;
			_graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, presParams);
		}

		private void ResetDevice() {
			presParams.BackBufferWidth = (int) ActualWidth;
			presParams.BackBufferHeight = (int) ActualHeight;
			_graphicsDevice.Reset(presParams);
		}
	}
}