using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.Instance.Misc;
using HaCreator.MapSimulator.Objects.FieldObject;
using HaCreator.MapSimulator.Objects.UIObject;
using HaRepacker.Utils;
using HaSharedLibrary;
using HaSharedLibrary.Render;
using HaSharedLibrary.Render.DX;
using HaSharedLibrary.Util;
using MapleLib.WzLib;
using HaSharedLibrary.Spine;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using spine_2._1._25_netcore;
using spine_2._1._25_netcore.spine_xna;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Encoder = System.Drawing.Imaging.Encoder;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace HaCreator.MapSimulator {
	/// <summary>
	/// 
	/// http://rbwhitaker.wikidot.com/xna-tutorials
	/// </summary>
	public class MapSimulator : Game {
		public int mapShiftX;
		public int mapShiftY;
		public Point minimapPos;

		private int Width;
		private int Height;
		private int RenderWidth;
		private int RenderHeight;
		private float RenderObjectScaling = 1.0f;
		private float UserScreenScaleFactor = 1.0f;
		private RenderResolution mapRenderResolution;
		private Matrix matrixScale;

		private GraphicsDeviceManager _DxDeviceManager;
		private readonly TexturePool texturePool = new();
		private bool bSaveScreenshot, bSaveScreenshotComplete = true; // flag for saving a screenshot file

		private SpriteBatch spriteBatch;


		// Objects, NPCs
		public List<BaseDXDrawableItem>[] mapObjects;
		private readonly List<BaseDXDrawableItem> mapObjects_NPCs = new();
		private readonly List<BaseDXDrawableItem> mapObjects_Mobs = new();
		private readonly List<BaseDXDrawableItem> mapObjects_Reactors = new();

		private readonly List<BaseDXDrawableItem>
			mapObjects_Portal = new(); // perhaps mapobjects should be in a single pool

		private readonly List<BaseDXDrawableItem> mapObjects_tooltips = new();

		// Backgrounds
		private readonly List<BackgroundItem> backgrounds_front = new();
		private readonly List<BackgroundItem> backgrounds_back = new();

		// Boundary, borders
		private Rectangle vr_fieldBoundary;
		private const int VR_BORDER_WIDTHHEIGHT = 600; // the height or width of the VR border
		private bool bDrawVRBorderLeftRight;

		private Texture2D texture_vrBoundaryRectLeft,
			texture_vrBoundaryRectRight,
			texture_vrBoundaryRectTop,
			texture_vrBoundaryRectBottom;

		// Mirror bottom boundaries (Reflections in Arcane river maps)
		private Rectangle rect_mirrorBottom;
		private ReflectionDrawableBoundary mirrorBottomReflection;

		// Minimap
		private MinimapItem miniMap;

		// Cursor, mouse
		private MouseCursorItem mouseCursor;

		// Audio
		private WzMp3Streamer audio;

		// Etc
		private readonly Board mapBoard;
		private bool bBigBangUpdate = true, bBigBang2Update = true;

		// Spine
		private SkeletonMeshRenderer skeletonMeshRenderer;

		// Text
		private SpriteFont font_navigationKeysHelper;
		private SpriteFont font_DebugValues;

		// Debug
		private Texture2D texture_debugBoundaryRect;
		private bool bShowDebugMode;

		/// <summary>
		/// MapSimulator Constructor
		/// </summary>
		/// <param name="mapBoard"></param>
		/// <param name="titleName"></param>
		public MapSimulator(Board mapBoard, string titleName) {
			this.mapBoard = mapBoard;

			mapRenderResolution = UserSettings.SimulateResolution;
			InitialiseWindowAndMap_WidthHeight();

			//RenderHeight += System.Windows.Forms.SystemInformation.CaptionHeight; // window title height

			//double dpi = ScreenDPIUtil.GetScreenScaleFactor();

			// set Form window height & width
			//this.Width = (int)(RenderWidth * dpi);
			//this.Height = (int)(RenderHeight * dpi);

			//Window.IsBorderless = true;
			//Window.Position = new Point(0, 0);
			Window.Title = titleName;
			IsFixedTimeStep = false; // dont cap fps
			IsMouseVisible = false; // draws our own custom cursor here.. 
			Content.RootDirectory = "Content";

			Window.ClientSizeChanged += Window_ClientSizeChanged;

			_DxDeviceManager = new GraphicsDeviceManager(this) {
				SynchronizeWithVerticalRetrace = true,
				HardwareModeSwitch = true,
				GraphicsProfile = GraphicsProfile.HiDef,
				IsFullScreen = false,
				PreferMultiSampling = true,
				SupportedOrientations = DisplayOrientation.Default,
				PreferredBackBufferWidth = Math.Max(RenderWidth, 1),
				PreferredBackBufferHeight = Math.Max(RenderHeight, 1),
				PreferredBackBufferFormat =
					SurfaceFormat.Color /* | SurfaceFormat.Bgr32 | SurfaceFormat.Dxt1| SurfaceFormat.Dxt5*/,
				PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
			};
			_DxDeviceManager.DeviceCreated += graphics_DeviceCreated;
			_DxDeviceManager.ApplyChanges();
		}

		#region Loading and unloading

		private void graphics_DeviceCreated(object sender, EventArgs e) {
		}

		private void InitialiseWindowAndMap_WidthHeight() {
			RenderObjectScaling = 1.0f;
			switch (mapRenderResolution) {
				case RenderResolution.Res_1024x768: // 1024x768
					Height = 768;
					Width = 1024;
					break;
				case RenderResolution.Res_1280x720: // 1280x720
					Height = 720;
					Width = 1280;
					break;
				case RenderResolution.Res_1366x768: // 1366x768
					Height = 768;
					Width = 1366;
					break;


				case RenderResolution.Res_1920x1080: // 1920x1080
					Height = 1080;
					Width = 1920;
					break;
				case RenderResolution.Res_1920x1080_120PercScaled: // 1920x1080
					Height = 1080;
					Width = 1920;
					RenderObjectScaling = 1.2f;
					break;
				case RenderResolution.Res_1920x1080_150PercScaled: // 1920x1080
					Height = 1080;
					Width = 1920;
					RenderObjectScaling = 1.5f;
					mapRenderResolution |=
						RenderResolution.Res_1366x768; // 1920x1080 is just 1366x768 with 150% scale.
					break;


				case RenderResolution.Res_1920x1200: // 1920x1200
					Height = 1200;
					Width = 1920;
					break;
				case RenderResolution.Res_1920x1200_120PercScaled: // 1920x1200
					Height = 1200;
					Width = 1920;
					RenderObjectScaling = 1.2f;
					break;
				case RenderResolution.Res_1920x1200_150PercScaled: // 1920x1200
					Height = 1200;
					Width = 1920;
					RenderObjectScaling = 1.5f;
					break;

				case RenderResolution.Res_All:
				case RenderResolution.Res_800x600: // 800x600
				default:
					Height = 600;
					Width = 800;
					break;
			}

			UserScreenScaleFactor = (float) ScreenDPIUtil.GetScreenScaleFactor();

			RenderHeight = (int) (Height * UserScreenScaleFactor);
			RenderWidth = (int) (Width * UserScreenScaleFactor);
			RenderObjectScaling = RenderObjectScaling * UserScreenScaleFactor;

			matrixScale = Matrix.CreateScale(RenderObjectScaling);
		}

		protected override void Initialize() {
			// TODO: Add your initialization logic here

			// Create map layers
			mapObjects = new List<BaseDXDrawableItem>[MapConstants.MaxMapLayers];
			for (var i = 0; i < MapConstants.MaxMapLayers; i++) {
				mapObjects[i] = new List<BaseDXDrawableItem>();
			}

			//GraphicsDevice.Viewport = new Viewport(RenderWidth / 2 - 800 / 2, RenderHeight / 2 - 600 / 2, 800, 600);

			// https://stackoverflow.com/questions/55045066/how-do-i-convert-a-ttf-or-other-font-to-a-xnb-xna-game-studio-font
			// if you're having issues building on w10, install Visual C++ Redistributable for Visual Studio 2012 Update 4
			// 
			// to build your own font: /MonoGame Font Builder/game.mgcb
			// build -> obj -> copy it over to HaRepacker-resurrected [Content]
			font_navigationKeysHelper = Content.Load<SpriteFont>("XnaDefaultFont");
			font_DebugValues = Content.Load<SpriteFont>("XnaFont_Debug");

			base.Initialize();
		}

		/// <summary>
		/// Load game assets
		/// </summary>
		protected override void LoadContent() {
			var mapHelperImage = (WzImage) Program.WzManager.FindWzImageByName("map", "MapHelper.img");
			var soundUIImage = (WzImage) Program.WzManager.FindWzImageByName("sound", "UI.img");
			var uiToolTipImage = (WzImage) Program.WzManager.FindWzImageByName("ui", "UIToolTip.img"); // UI_003.wz
			var uiBasicImage = (WzImage) Program.WzManager.FindWzImageByName("ui", "Basic.img");
			var uiWindow1Image = (WzImage) Program.WzManager.FindWzImageByName("ui", "UIWindow.img"); //
			var uiWindow2Image =
				(WzImage) Program.WzManager.FindWzImageByName("ui", "UIWindow2.img"); // doesnt exist before big-bang

			bBigBangUpdate =
				uiWindow2Image?["BigBang!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"] !=
				null; // different rendering for pre and post-bb, to support multiple vers
			bBigBang2Update = uiWindow2Image?["BigBang2!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"] != null; // chaos update

			// BGM
			if (Program.InfoManager.BGMs.ContainsKey(mapBoard.MapInfo.bgm)) {
				audio = new WzMp3Streamer(Program.InfoManager.BGMs[mapBoard.MapInfo.bgm], true);
				if (audio != null) {
					audio.Volume = 0.3f;
					audio.Play();
				}
			}

			if (mapBoard.VRRectangle == null) {
				vr_fieldBoundary = new Rectangle(0, 0, mapBoard.MapSize.X, mapBoard.MapSize.Y);
			} else {
				vr_fieldBoundary = new Rectangle(mapBoard.VRRectangle.X + mapBoard.CenterPoint.X,
					mapBoard.VRRectangle.Y + mapBoard.CenterPoint.Y, mapBoard.VRRectangle.Width,
					mapBoard.VRRectangle.Height);
			}
			//SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);

			// test benchmark
#if DEBUG
			var watch = new Stopwatch();
			watch.Start();
#endif

			/////// Background and objects
			var usedProps = new List<WzObject>();

			// Objects
			var t_tiles = Task.Run(() => {
				foreach (var tileObj in mapBoard.BoardItems.TileObjs) {
					var tileParent = (WzImageProperty) tileObj.BaseInfo.ParentObject;

					mapObjects[tileObj.LayerNumber].Add(
						MapSimulatorLoader.CreateMapItemFromProperty(texturePool, tileParent, tileObj.X, tileObj.Y,
							mapBoard.CenterPoint, _DxDeviceManager.GraphicsDevice, ref usedProps,
							tileObj is IFlippable ? ((IFlippable) tileObj).Flip : false));
				}
			});

			// Background
			var t_Background = Task.Run(() => {
				foreach (var background in mapBoard.BoardItems.BackBackgrounds) {
					var bgParent = (WzImageProperty) background.BaseInfo.ParentObject;
					var bgItem = MapSimulatorLoader.CreateBackgroundFromProperty(texturePool, bgParent,
						background, _DxDeviceManager.GraphicsDevice, ref usedProps, background.Flip);

					if (bgItem != null) {
						backgrounds_back.Add(bgItem);
					}
				}

				foreach (var background in mapBoard.BoardItems.FrontBackgrounds) {
					var bgParent = (WzImageProperty) background.BaseInfo.ParentObject;
					var bgItem = MapSimulatorLoader.CreateBackgroundFromProperty(texturePool, bgParent,
						background, _DxDeviceManager.GraphicsDevice, ref usedProps, background.Flip);

					if (bgItem != null) {
						backgrounds_front.Add(bgItem);
					}
				}
			});

			// Reactors
			var t_reactor = Task.Run(() => {
				foreach (var reactor in mapBoard.BoardItems.Reactors) {
					//WzImage imageProperty = (WzImage)NPCWZFile[reactorInfo.ID + ".img"];

					var reactorItem = MapSimulatorLoader.CreateReactorFromProperty(texturePool, reactor,
						_DxDeviceManager.GraphicsDevice, ref usedProps);
					if (reactorItem != null) {
						mapObjects_Reactors.Add(reactorItem);
					}
				}
			});

			// NPCs
			var t_npc = Task.Run(() => {
				foreach (var npc in mapBoard.BoardItems.NPCs) {
					//WzImage imageProperty = (WzImage) NPCWZFile[npcInfo.ID + ".img"];
					if (npc.Hide) {
						continue;
					}

					var npcItem = MapSimulatorLoader.CreateNpcFromProperty(texturePool, npc,
						_DxDeviceManager.GraphicsDevice, ref usedProps);
					mapObjects_NPCs.Add(npcItem);
				}
			});

			// Mobs
			var t_mobs = Task.Run(() => {
				foreach (var mob in mapBoard.BoardItems.Mobs) {
					//WzImage imageProperty = Program.WzManager.FindMobImage(mobInfo.ID); // Mob.wz Mob2.img Mob001.wz
					if (mob.Hide) {
						continue;
					}

					var npcItem = MapSimulatorLoader.CreateMobFromProperty(texturePool, mob,
						_DxDeviceManager.GraphicsDevice, ref usedProps);
					mapObjects_Mobs.Add(npcItem);
				}
			});

			// Portals
			var t_portal = Task.Run(() => {
				var portalParent = (WzSubProperty) mapHelperImage["portal"];

				var gameParent = (WzSubProperty) portalParent["game"];
				//WzSubProperty editorParent = (WzSubProperty) portalParent["editor"];

				foreach (var portal in mapBoard.BoardItems.Portals) {
					var portalItem = MapSimulatorLoader.CreatePortalFromProperty(texturePool, gameParent, portal,
						_DxDeviceManager.GraphicsDevice, ref usedProps);
					if (portalItem != null) {
						mapObjects_Portal.Add(portalItem);
					}
				}
			});

			// Tooltips
			var t_tooltips = Task.Run(() => {
				var
					farmFrameParent =
						(WzSubProperty) uiToolTipImage?["Item"]?["FarmFrame"]; // not exist before V update.
				foreach (var tooltip in mapBoard.BoardItems.ToolTips) {
					var item = MapSimulatorLoader.CreateTooltipFromProperty(texturePool, UserScreenScaleFactor,
						farmFrameParent, tooltip, _DxDeviceManager.GraphicsDevice);

					mapObjects_tooltips.Add(item);
				}
			});

			// Cursor
			var t_cursor = Task.Run(() => {
				var cursorImageProperty = uiBasicImage["Cursor"];
				mouseCursor = MapSimulatorLoader.CreateMouseCursorFromProperty(texturePool, cursorImageProperty, 0,
					0, _DxDeviceManager.GraphicsDevice, ref usedProps, false);
			});

			// Spine object
			var t_spine = Task.Run(() => {
				skeletonMeshRenderer = new SkeletonMeshRenderer(GraphicsDevice) {
					PremultipliedAlpha = false
				};
				skeletonMeshRenderer.Effect.World = matrixScale;
			});

			// Minimap
			var t_minimap = Task.Run(() => {
				if (!mapBoard.MapInfo.hideMinimap) {
					miniMap = MapSimulatorLoader.CreateMinimapFromProperty(uiWindow1Image, uiWindow2Image, uiBasicImage,
						mapBoard, GraphicsDevice, UserScreenScaleFactor, mapBoard.MapInfo.strMapName,
						mapBoard.MapInfo.strStreetName, soundUIImage, bBigBangUpdate);
				}
			});

			while (!t_tiles.IsCompleted || !t_Background.IsCompleted || !t_reactor.IsCompleted || !t_npc.IsCompleted ||
			       !t_mobs.IsCompleted || !t_portal.IsCompleted ||
			       !t_tooltips.IsCompleted || !t_cursor.IsCompleted || !t_spine.IsCompleted || !t_minimap.IsCompleted) {
				Thread.Sleep(100);
			}

#if DEBUG
			// test benchmark
			watch.Stop();
			Debug.WriteLine($"Map WZ files loaded. Execution Time: {watch.ElapsedMilliseconds} ms");
#endif
			//
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// default positioning for character
			SetCameraMoveX(true, true, 0);
			SetCameraMoveY(true, true, 0);

			///////////// Border
			var leftRightVRDifference = (int) ((vr_fieldBoundary.Right - vr_fieldBoundary.Left) * RenderObjectScaling);
			if (leftRightVRDifference < RenderWidth) // viewing range is smaller than the render width.. 
			{
				bDrawVRBorderLeftRight = true; // flag

				texture_vrBoundaryRectLeft = CreateVRBorder(VR_BORDER_WIDTHHEIGHT, vr_fieldBoundary.Height,
					_DxDeviceManager.GraphicsDevice);
				texture_vrBoundaryRectRight = CreateVRBorder(VR_BORDER_WIDTHHEIGHT, vr_fieldBoundary.Height,
					_DxDeviceManager.GraphicsDevice);
				texture_vrBoundaryRectTop = CreateVRBorder(vr_fieldBoundary.Width * 2, VR_BORDER_WIDTHHEIGHT,
					_DxDeviceManager.GraphicsDevice);
				texture_vrBoundaryRectBottom = CreateVRBorder(vr_fieldBoundary.Width * 2, VR_BORDER_WIDTHHEIGHT,
					_DxDeviceManager.GraphicsDevice);
			}

			// mirror bottom boundaries
			//rect_mirrorBottom
			if (mapBoard.MapInfo.mirror_Bottom) {
				if (mapBoard.MapInfo.VRLeft != null && mapBoard.MapInfo.VRRight != null) {
					var vr_width = mapBoard.MapInfo.VRRight - mapBoard.MapInfo.VRLeft;
					const int obj_mirrorBottom_height = 200;

					rect_mirrorBottom = new Rectangle(mapBoard.MapInfo.VRLeft,
						mapBoard.MapInfo.VRBottom - obj_mirrorBottom_height, vr_width, obj_mirrorBottom_height);

					mirrorBottomReflection = new ReflectionDrawableBoundary(128, 255, "mirror", true, false);
				}
			}
			/*
			DXObject leftDXVRObject = new DXObject(
			    vr_fieldBoundary.Left - VR_BORDER_WIDTHHEIGHT,
			    vr_fieldBoundary.Top,
			    texture_vrBoundaryRectLeft);
			this.leftVRBorderDrawableItem = new BaseDXDrawableItem(leftDXVRObject, false);
			//new BackgroundItem(int cx, int cy, int rx, int ry, BackgroundType.Regular, 255, true, leftDXVRObject, false, (int) RenderResolution.Res_All);

			// Right VR
			DXObject rightDXVRObject = new DXObject(
			    vr_fieldBoundary.Right,
			    vr_fieldBoundary.Top,
			    texture_vrBoundaryRectRight);
			this.rightVRBorderDrawableItem = new BaseDXDrawableItem(rightDXVRObject, false);
			*/
			///////////// End Border

			// Debug items
			var bitmap_debug = new Bitmap(1, 1);
			bitmap_debug.SetPixel(0, 0, System.Drawing.Color.White);
			texture_debugBoundaryRect = bitmap_debug.ToTexture2D(_DxDeviceManager.GraphicsDevice);

			// cleanup
			// clear used items
			foreach (var obj in usedProps) {
				if (obj == null) {
					continue; // obj copied twice in usedProps?
				}

				// Spine events
				var spineObj = (WzSpineObject) obj.MSTagSpine;
				if (spineObj != null) {
					spineObj.state.Start += Start;
					spineObj.state.End += End;
					spineObj.state.Complete += Complete;
					spineObj.state.Event += Event;
				}

				obj.MSTag = null;
				obj.MSTagSpine = null; // cleanup
			}

			usedProps.Clear();
		}

		/// <summary>
		/// Creates the black VR Border Texture2D object
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="graphicsDevice"></param>
		/// <returns></returns>
		private static Texture2D CreateVRBorder(int width, int height, GraphicsDevice graphicsDevice) {
			var brBorderColor = System.Drawing.Color.Black;
			var bitmap_vrBorder = new Bitmap(width, height);

			for (var x = 0; x < bitmap_vrBorder.Width; x++)
			for (var y = 0; y < bitmap_vrBorder.Height; y++) {
				bitmap_vrBorder.SetPixel(x, y, brBorderColor); // is there a better way of doing this than looping?
			}

			var texture_vrBoundaryRect = bitmap_vrBorder.ToTexture2D(graphicsDevice);
			return texture_vrBoundaryRect;
		}

		protected override void UnloadContent() {
			if (audio != null)
				//audio.Pause();
			{
				audio.Dispose();
			}

			skeletonMeshRenderer.End();

			_DxDeviceManager.EndDraw();
			_DxDeviceManager.Dispose();


			mapObjects_NPCs.Clear();
			mapObjects_Mobs.Clear();
			mapObjects_Reactors.Clear();
			mapObjects_Portal.Clear();

			backgrounds_front.Clear();
			backgrounds_back.Clear();

			texturePool.Dispose();

			// clear prior mirror bottom boundary
			rect_mirrorBottom = new Rectangle();
			mirrorBottomReflection = null;
		}

		#endregion

		#region Update and Drawing

		/// <summary>
		/// On game window size changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_ClientSizeChanged(object sender, EventArgs e) {
		}

		private int currTickCount = Environment.TickCount;
		private KeyboardState oldKeyboardState = Keyboard.GetState();
		private MouseState oldMouseState;

		/// <summary>
		/// Key, and frame update handling
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update(GameTime gameTime) {
			var frameRate = 1 / (float) gameTime.ElapsedGameTime.TotalSeconds;
			currTickCount = Environment.TickCount;
			var delta = gameTime.ElapsedGameTime.Milliseconds / 1000f;
			var newKeyboardState = Keyboard.GetState(); // get the newest state
			var newMouseState = mouseCursor.MouseState;

			// Allows the game to exit
#if !WINDOWS_STOREAPP
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
			    || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
				Exit();
				return;
			}
#endif
			// Handle full screen
			var bIsAltEnterPressed =
				newKeyboardState.IsKeyDown(Keys.LeftAlt) && newKeyboardState.IsKeyDown(Keys.Enter);
			if (bIsAltEnterPressed) {
				_DxDeviceManager.IsFullScreen = !_DxDeviceManager.IsFullScreen;
				_DxDeviceManager.ApplyChanges();
				return;
			}

			// Handle print screen
			if (newKeyboardState.IsKeyDown(Keys.PrintScreen)) {
				if (!bSaveScreenshot && bSaveScreenshotComplete) {
					bSaveScreenshot = true; // flag for screenshot
					bSaveScreenshotComplete = false;
				}
			}


			// Handle mouse
			mouseCursor.UpdateCursorState();


			// Navigate around the rendered object
			var bIsShiftPressed = newKeyboardState.IsKeyDown(Keys.LeftShift) ||
			                      newKeyboardState.IsKeyDown(Keys.RightShift);

			var bIsUpKeyPressed = newKeyboardState.IsKeyDown(Keys.Up);
			var bIsDownKeyPressed = newKeyboardState.IsKeyDown(Keys.Down);
			var bIsLeftKeyPressed = newKeyboardState.IsKeyDown(Keys.Left);
			var bIsRightKeyPressed = newKeyboardState.IsKeyDown(Keys.Right);

			var moveOffset =
				bIsShiftPressed
					? (int) (3000f / frameRate)
					: (int) (1500f / frameRate); // move a fixed amount a second, not dependent on GPU speed
			if (bIsLeftKeyPressed || bIsRightKeyPressed) {
				SetCameraMoveX(bIsLeftKeyPressed, bIsRightKeyPressed, moveOffset);
			}

			if (bIsUpKeyPressed || bIsDownKeyPressed) {
				SetCameraMoveY(bIsUpKeyPressed, bIsDownKeyPressed, moveOffset);
			}

			// Minimap M
			if (newKeyboardState.IsKeyDown(Keys.M)) {
			}

			// Debug keys
			if (newKeyboardState.IsKeyUp(Keys.F5) && oldKeyboardState.IsKeyDown(Keys.F5)) {
				bShowDebugMode = !bShowDebugMode;
			}

			oldKeyboardState = newKeyboardState; // set the new state as the old state for next time
			oldMouseState = newMouseState; // set the new state as the old state for next time

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			var frameRate = 1 / (float) gameTime.ElapsedGameTime.TotalSeconds;
			var TickCount = currTickCount;
			//float delta = gameTime.ElapsedGameTime.Milliseconds / 1000f;

			var mouseState = oldMouseState;
			var mouseXRelativeToMap = mouseState.X - mapShiftX;
			var mouseYRelativeToMap = mouseState.Y - mapShiftY;
			//System.Diagnostics.Debug.WriteLine("Mouse relative to map: X {0}, Y {1}", mouseXRelativeToMap, mouseYRelativeToMap);

			var mapCenterX = mapBoard.CenterPoint.X;
			var mapCenterY = mapBoard.CenterPoint.Y;
			var shiftCenteredX = mapShiftX - mapCenterX;
			var shiftCenteredY = mapShiftY - mapCenterY;

			//GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0); // Clear the window to black
			GraphicsDevice.Clear(Color.Black);

			spriteBatch.Begin(
				SpriteSortMode.Immediate, // spine :( needs to be drawn immediately to maintain the layer orders
				//SpriteSortMode.Deferred,
				BlendState.NonPremultiplied, null, null, null, null, matrixScale);
			//skeletonMeshRenderer.Begin();

			// Back Backgrounds
			backgrounds_back.ForEach(bg => {
				bg.Draw(spriteBatch, skeletonMeshRenderer, gameTime,
					mapShiftX, mapShiftY, mapCenterX, mapCenterY,
					null,
					RenderWidth, RenderHeight, RenderObjectScaling, mapRenderResolution,
					TickCount);
			});

			// Map objects
			foreach (var mapItem in mapObjects)
			foreach (var item in mapItem) {
				item.Draw(spriteBatch, skeletonMeshRenderer, gameTime,
					mapShiftX, mapShiftY, mapCenterX, mapCenterY,
					null,
					RenderWidth, RenderHeight, RenderObjectScaling, mapRenderResolution,
					TickCount);
			}

			// Portals
			foreach (PortalItem portalItem in mapObjects_Portal) {
				var instance = portalItem.PortalInstance;

				portalItem.Draw(spriteBatch, skeletonMeshRenderer, gameTime,
					mapShiftX, mapShiftY, mapCenterX, mapCenterY,
					null,
					RenderWidth, RenderHeight, RenderObjectScaling, mapRenderResolution,
					TickCount);

				// Draw portal debug tooltip
				if (bShowDebugMode) {
					var rect = new Rectangle(
						instance.X - shiftCenteredX - (instance.Width - 20),
						instance.Y - shiftCenteredY - instance.Height,
						instance.Width + 40,
						instance.Height);

					DrawBorder(spriteBatch, rect, 1, Color.White, new Color(Color.Gray, 0.3f));

					if (portalItem.CanUpdateDebugText(TickCount, 1000)) {
						var sb = new StringBuilder();
						sb.Append(" x: ").Append(rect.X).Append(Environment.NewLine);
						sb.Append(" y: ").Append(rect.Y).Append(Environment.NewLine);
						sb.Append(" script: ").Append(instance.script).Append(Environment.NewLine);
						sb.Append(" tm: ").Append(instance.tm).Append(Environment.NewLine);
						sb.Append(" pt: ").Append(instance.pt).Append(Environment.NewLine);
						sb.Append(" pn: ").Append(instance.pt).Append(Environment.NewLine);

						portalItem.DebugText = sb.ToString();
					}

					spriteBatch.DrawString(font_DebugValues, portalItem.DebugText, new Vector2(rect.X, rect.Y),
						Color.White);
					Debug.WriteLine(rect.ToString());
				}
			}

			// Reactors
			foreach (ReactorItem reactorItem in mapObjects_Reactors) {
				var instance = reactorItem.ReactorInstance;

				reactorItem.Draw(spriteBatch, skeletonMeshRenderer, gameTime,
					mapShiftX, mapShiftY, mapCenterX, mapCenterY,
					null,
					RenderWidth, RenderHeight, RenderObjectScaling, mapRenderResolution,
					TickCount);

				// Draw reactor debug tooltip
				if (bShowDebugMode) {
					var rect = new Rectangle(
						instance.X - shiftCenteredX - (instance.Width - 20),
						instance.Y - shiftCenteredY - instance.Height,
						Math.Max(80, instance.Width + 40),
						Math.Max(120, instance.Height));

					DrawBorder(spriteBatch, rect, 1, Color.White, new Color(Color.Gray, 0.3f));

					if (reactorItem.CanUpdateDebugText(TickCount, 1000)) {
						var sb = new StringBuilder();
						sb.Append(" x: ").Append(rect.X).Append(Environment.NewLine);
						sb.Append(" y: ").Append(rect.Y).Append(Environment.NewLine);
						sb.Append(" id: ").Append(instance.ReactorInfo.ID).Append(Environment.NewLine);
						sb.Append(" name: ").Append(instance.Name).Append(Environment.NewLine);

						reactorItem.DebugText = sb.ToString();
					}

					spriteBatch.DrawString(font_DebugValues, reactorItem.DebugText, new Vector2(rect.X, rect.Y),
						Color.White);
					Debug.WriteLine(rect.ToString());
				}
			}

			// Life (NPC + Mobs)
			foreach (MobItem mobItem in mapObjects_Mobs) // Mobs
			{
				var instance = mobItem.MobInstance;

				ReflectionDrawableBoundary mirrorFieldData = null;
				if (mirrorBottomReflection != null) {
					if (rect_mirrorBottom.Contains(new Point(mobItem.MobInstance.X, mobItem.MobInstance.Y))) {
						mirrorFieldData = mirrorBottomReflection;
					}
				}

				if (mirrorFieldData == null) // a field may contain both 'info/mirror_Bottom' and 'MirrorFieldData'
				{
					mirrorFieldData = mapBoard.BoardItems
						.CheckObjectWithinMirrorFieldDataBoundary(mobItem.MobInstance.X, mobItem.MobInstance.Y,
							MirrorFieldDataType.mob)?.ReflectionInfo;
				}

				mobItem.Draw(spriteBatch, skeletonMeshRenderer, gameTime,
					mapShiftX, mapShiftY, mapCenterX, mapCenterY,
					mirrorFieldData,
					RenderWidth, RenderHeight, RenderObjectScaling, mapRenderResolution,
					TickCount);

				// Draw mobs debug tooltip
				if (bShowDebugMode) {
					var rect = new Rectangle(
						instance.X - shiftCenteredX - (instance.Width - 20),
						instance.Y - shiftCenteredY - instance.Height,
						Math.Max(100, instance.Width + 40),
						Math.Max(120, instance.Height));

					DrawBorder(spriteBatch, rect, 1, Color.White, new Color(Color.Gray, 0.3f));

					if (mobItem.CanUpdateDebugText(TickCount, 1000)) {
						var sb = new StringBuilder();
						sb.Append(" x: ").Append(rect.X).Append(Environment.NewLine);
						sb.Append(" y: ").Append(rect.Y).Append(Environment.NewLine);
						sb.Append(" id: ").Append(instance.MobInfo.ID).Append(Environment.NewLine);
						sb.Append(" name: ").Append(instance.MobInfo.Name).Append(Environment.NewLine);

						mobItem.DebugText = sb.ToString();
					}

					spriteBatch.DrawString(font_DebugValues, mobItem.DebugText, new Vector2(rect.X, rect.Y),
						Color.White);
					Debug.WriteLine(rect.ToString());
				}
			}

			foreach (NpcItem npcItem in mapObjects_NPCs) // NPCs (always in front of mobs)
			{
				var instance = npcItem.NpcInstance;

				ReflectionDrawableBoundary mirrorFieldData = null;
				if (mirrorBottomReflection != null) {
					if (rect_mirrorBottom.Contains(new Point(npcItem.NpcInstance.X, npcItem.NpcInstance.Y))) {
						mirrorFieldData = mirrorBottomReflection;
					}
				}

				if (mirrorFieldData == null) // a field may contain both 'info/mirror_Bottom' and 'MirrorFieldData'
				{
					mirrorFieldData = mapBoard.BoardItems
						.CheckObjectWithinMirrorFieldDataBoundary(npcItem.NpcInstance.X, npcItem.NpcInstance.Y,
							MirrorFieldDataType.npc)?.ReflectionInfo;
				}

				npcItem.Draw(spriteBatch, skeletonMeshRenderer, gameTime,
					mapShiftX, mapShiftY, mapCenterX, mapCenterY,
					mirrorFieldData,
					RenderWidth, RenderHeight, RenderObjectScaling, mapRenderResolution,
					TickCount);

				// Draw npc debug tooltip
				if (bShowDebugMode) {
					var rect = new Rectangle(
						instance.X - shiftCenteredX - (instance.Width - 20),
						instance.Y - shiftCenteredY - instance.Height,
						Math.Max(100, instance.Width + 40),
						Math.Max(120, instance.Height));

					DrawBorder(spriteBatch, rect, 1, Color.White, new Color(Color.Gray, 0.3f));

					if (npcItem.CanUpdateDebugText(TickCount, 1000)) {
						var sb = new StringBuilder();
						sb.Append(" x: ").Append(rect.X).Append(Environment.NewLine);
						sb.Append(" y: ").Append(rect.Y).Append(Environment.NewLine);
						sb.Append(" id: ").Append(instance.NpcInfo.ID).Append(Environment.NewLine);
						sb.Append(" name: ").Append(instance.NpcInfo.Name).Append(Environment.NewLine);

						npcItem.DebugText = sb.ToString();
					}

					spriteBatch.DrawString(font_DebugValues, npcItem.DebugText, new Vector2(rect.X, rect.Y),
						Color.White);
					Debug.WriteLine(rect.ToString());
				}
			}

			// Front Backgrounds
			backgrounds_front.ForEach(bg => {
				bg.Draw(spriteBatch, skeletonMeshRenderer, gameTime,
					mapShiftX, mapShiftY, mapCenterX, mapCenterY,
					null,
					RenderWidth, RenderHeight, RenderObjectScaling, mapRenderResolution,
					TickCount);
			});

			// Borders
			// Create any rectangle you want. Here we'll use the TitleSafeArea for fun.
			//Rectangle titleSafeRectangle = GraphicsDevice.Viewport.TitleSafeArea;
			//DrawBorder(spriteBatch, titleSafeRectangle, 1, Color.Black);

			DrawVRFieldBorder(spriteBatch);

			//////////////////// UI related here ////////////////////
			// Tooltips
			if (mapObjects_tooltips.Count > 0) {
				foreach (TooltipItem tooltip in mapObjects_tooltips) // NPCs (always in front of mobs)
				{
					if (tooltip.TooltipInstance.CharacterToolTip != null) {
						var tooltipRect = tooltip.TooltipInstance.CharacterToolTip.Rectangle;
						if (tooltipRect != null) // if this is null, show it at all times
						{
							var rect = new Rectangle(
								tooltipRect.X - shiftCenteredX,
								tooltipRect.Y - shiftCenteredY,
								tooltipRect.Width, tooltipRect.Height);

							if (bShowDebugMode) {
								DrawBorder(spriteBatch, rect, 1, Color.White, new Color(Color.Gray, 0.3f)); // test

								if (tooltip.CanUpdateDebugText(TickCount, 1000)) {
									var text = "X: " + rect.X + ", Y: " + rect.Y;

									tooltip.DebugText = text;
								}

								spriteBatch.DrawString(font_DebugValues, tooltip.DebugText, new Vector2(rect.X, rect.Y),
									Color.White);
							}

							if (!rect.Contains(mouseState.X, mouseState.Y)) {
								continue;
							}
						}
					}

					tooltip.Draw(spriteBatch, skeletonMeshRenderer, gameTime,
						mapShiftX, mapShiftY, mapBoard.CenterPoint.X, mapBoard.CenterPoint.Y,
						null,
						RenderWidth, RenderHeight, RenderObjectScaling, mapRenderResolution,
						TickCount);
				}
			}

			// Minimap
			if (miniMap != null) {
				miniMap.Draw(spriteBatch, skeletonMeshRenderer, gameTime,
					mapShiftX, mapShiftY, minimapPos.X, minimapPos.Y,
					null,
					RenderWidth, RenderHeight, RenderObjectScaling, mapRenderResolution,
					TickCount);

				miniMap.CheckMouseEvent(shiftCenteredX, shiftCenteredY, mouseState);
			}

			if (gameTime.TotalGameTime.TotalSeconds < 4) {
				spriteBatch.DrawString(font_navigationKeysHelper,
					string.Format(
						"[Left] [Right] [Up] [Down] [Shift] for navigation.{0}[F5] for debug mode{1}[Alt+Enter] Full screen{2}[PrintSc] Screenshot",
						Environment.NewLine, Environment.NewLine, Environment.NewLine),
					new Vector2(20, Height - 140), Color.White);
			}

			if (!bSaveScreenshot && bShowDebugMode) {
				var sb = new StringBuilder();
				sb.Append("FPS: ").Append(frameRate).Append(Environment.NewLine);
				sb.Append("Mouse : X ").Append(mouseXRelativeToMap).Append(", Y ").Append(mouseYRelativeToMap)
					.Append(Environment.NewLine);
				sb.Append("RMouse: X ").Append(mouseState.X).Append(", Y ").Append(mouseState.Y);
				spriteBatch.DrawString(font_DebugValues, sb.ToString(),
					new Vector2(Width - 170, 10), Color.White); // use the original width to render text
			}

			// Cursor [this is in front of everything else]
			mouseCursor.Draw(spriteBatch, skeletonMeshRenderer, gameTime,
				0, 0, 0, 0, // pos determined in the class
				null,
				RenderWidth, RenderHeight, RenderObjectScaling, mapRenderResolution, TickCount);

			spriteBatch.End();
			//skeletonMeshRenderer.End();

			// Save screenshot if render is activated
			DoScreenshot();


			base.Draw(gameTime);
		}

		/// <summary>
		/// Draws the VR border
		/// </summary>
		/// <param name="sprite"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DrawVRFieldBorder(SpriteBatch sprite) {
			if (bBigBang2Update || !bDrawVRBorderLeftRight || (vr_fieldBoundary.X == 0 && vr_fieldBoundary.Y == 0)) {
				return;
			}

			var borderColor = Color.Black;

			// Draw top line
			/*     sprite.Draw(texture_vrBoundaryRectTop,
			         new Rectangle(
			             vr_fieldBoundary.Left - (VR_BORDER_WIDTHHEIGHT + mapShiftX),
			             vr_fieldBoundary.Top - (VR_BORDER_WIDTHHEIGHT + mapShiftY),
			             vr_fieldBoundary.Width * 2,
			             VR_BORDER_WIDTHHEIGHT),
			         borderColor);

			     // Draw bottom line
			     sprite.Draw(texture_vrBoundaryRectBottom,
			         new Rectangle(
			             vr_fieldBoundary.Left - (VR_BORDER_WIDTHHEIGHT + mapShiftX),
			             vr_fieldBoundary.Bottom - (mapShiftY),
			             vr_fieldBoundary.Width * 2,
			             VR_BORDER_WIDTHHEIGHT),
			         borderColor);*/

			// Draw left line
			sprite.Draw(texture_vrBoundaryRectLeft,
				new Rectangle(
					vr_fieldBoundary.Left - (VR_BORDER_WIDTHHEIGHT + mapShiftX),
					vr_fieldBoundary.Top - mapShiftY,
					VR_BORDER_WIDTHHEIGHT,
					vr_fieldBoundary.Height),
				borderColor);

			// Draw right line
			sprite.Draw(texture_vrBoundaryRectRight,
				new Rectangle(
					vr_fieldBoundary.Right - mapShiftX,
					vr_fieldBoundary.Top - mapShiftY,
					VR_BORDER_WIDTHHEIGHT,
					vr_fieldBoundary.Height),
				borderColor);
		}

		/// <summary>
		/// Draws a border
		/// </summary>
		/// <param name="sprite"></param>
		/// <param name="rectangleToDraw"></param>
		/// <param name="thicknessOfBorder"></param>
		/// <param name="borderColor"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DrawBorder(SpriteBatch sprite, Rectangle rectangleToDraw, int thicknessOfBorder, Color borderColor,
			Color backgroundColor) {
			// Draw top line
			sprite.Draw(texture_debugBoundaryRect,
				new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, thicknessOfBorder),
				borderColor);

			// Draw left line
			sprite.Draw(texture_debugBoundaryRect,
				new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, thicknessOfBorder, rectangleToDraw.Height),
				borderColor);

			// Draw right line
			sprite.Draw(texture_debugBoundaryRect, new Rectangle(
				rectangleToDraw.X + rectangleToDraw.Width - thicknessOfBorder,
				rectangleToDraw.Y,
				thicknessOfBorder,
				rectangleToDraw.Height), borderColor);
			// Draw bottom line
			sprite.Draw(texture_debugBoundaryRect, new Rectangle(rectangleToDraw.X,
				rectangleToDraw.Y + rectangleToDraw.Height - thicknessOfBorder,
				rectangleToDraw.Width,
				thicknessOfBorder), borderColor);

			// Draw background
			if (backgroundColor != Color.Transparent)
				// draw a black background sprite with the rectangleToDraw as area
			{
				sprite.Draw(texture_debugBoundaryRect, rectangleToDraw, backgroundColor);
			}
		}

		/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DrawLine(SpriteBatch sprite, Vector2 start, Vector2 end, Color color)
		{
		    int width = (int)Vector2.Distance(start, end);
		    float rotation = (float)Math.Atan2((double)(end.Y - start.Y), (double)(end.X - start.X));
		    sprite.Draw(texture_miniMapPixel, new Rectangle((int)start.X, (int)start.Y, width, UserSettings.LineWidth), null, color, rotation, new Vector2(0f, 0f), SpriteEffects.None, 1f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FillRectangle(SpriteBatch sprite, Rectangle rectangle, Color color)
		{
		    sprite.Draw(texture_miniMapPixel, rectangle, color);
		}*/

		#endregion

		#region Screenshot

		/// <summary>
		/// Creates a snapshot of the current Graphics Device back buffer data 
		/// and save as JPG in the local folder
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DoScreenshot() {
			if (!bSaveScreenshotComplete) {
				return;
			}

			if (bSaveScreenshot) {
				bSaveScreenshot = false;

				//Pull the picture from the buffer 
				var backBufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
				var backBufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
				var backBuffer = new int[backBufferWidth * backBufferHeight];
				GraphicsDevice.GetBackBufferData(backBuffer);

				//Copy to texture
				using (var texture = new Texture2D(GraphicsDevice, backBufferWidth, backBufferHeight, false,
					       SurfaceFormat.Color /*RGBA8888*/)) {
					texture.SetData(backBuffer);

					//Get a date for file name
					var dateTimeNow = DateTime.Now;
					var fileName = string.Format("Maple_{0}{1}{2}_{3}{4}{5}.png",
						dateTimeNow.Day.ToString("D2"), dateTimeNow.Month.ToString("D2"),
						(dateTimeNow.Year - 2000).ToString("D2"),
						dateTimeNow.Hour.ToString("D2"), dateTimeNow.Minute.ToString("D2"),
						dateTimeNow.Second.ToString("D2")
					); // same naming scheme as the official client.. except that they save in JPG

					using (var stream_png = new MemoryStream()) // memorystream for png
					{
						texture.SaveAsPng(stream_png, backBufferWidth, backBufferHeight); // save to png stream

						var bitmap = new Bitmap(stream_png);
						var jpgEncoder = GetEncoder(ImageFormat.Jpeg);

						// Create an EncoderParameters object.
						// An EncoderParameters object has an array of EncoderParameter
						// objects. 
						var myEncoderParameters = new EncoderParameters(1);
						myEncoderParameters.Param[0] =
							new EncoderParameter(Encoder.Quality, 100L); // max quality

						bitmap.Save(fileName, jpgEncoder, myEncoderParameters);

						/*using (MemoryStream stream_jpeg = new MemoryStream()) // memorystream for jpeg
						{
						    var imageStream_png = System.Drawing.Image.FromStream(stream_png, true); // png Image
						    imageStream_png.Save(stream_jpeg, ImageFormat.Jpeg); // save as jpeg  - TODO: Fix System.Runtime.InteropServices.ExternalException: 'A generic error occurred in GDI+.' sometimes.. no idea

						    byte[] jpegOutStream = stream_jpeg.ToArray();

						    // Save
						    using (FileStream fs = File.Open(fileName, FileMode.OpenOrCreate))
						    {
						        fs.Write(jpegOutStream, 0, jpegOutStream.Length);
						    }
						}*/
					}
				}

				bSaveScreenshotComplete = true;
			}
		}

		private ImageCodecInfo GetEncoder(ImageFormat format) {
			var codecs = ImageCodecInfo.GetImageDecoders();

			foreach (var codec in codecs) {
				if (codec.FormatID == format.Guid) {
					return codec;
				}
			}

			return null;
		}

		#endregion

		#region Boundaries

		/// <summary>
		/// Move the camera X viewing range by a specific offset, & centering if needed.
		/// </summary>
		/// <param name="bIsLeftKeyPressed"></param>
		/// <param name="bIsRightKeyPressed"></param>
		/// <param name="moveOffset"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetCameraMoveX(bool bIsLeftKeyPressed, bool bIsRightKeyPressed, int moveOffset) {
			var leftRightVRDifference = (int) ((vr_fieldBoundary.Right - vr_fieldBoundary.Left) * RenderObjectScaling);
			if (leftRightVRDifference <
			    RenderWidth) // viewing range is smaller than the render width.. keep the rendering position at the center instead (starts from left to right)
			{
				/*
				 * Orbis Tower <20th Floor>
				 *  |____________|
				 *  |____________|
				 *  |____________|
				 *  |____________|
				 *  |____________|
				 *  |____________|
				 *  |____________|
				 *  |____________|
				 *
				 * vr.Left = 87
				 * vr.Right = 827
				 * Difference = 740px
				 * vr.Center = ((vr.Right - vr.Left) / 2) + vr.Left
				 *
				 * Viewing Width = 1024
				 * Relative viewing center = vr.Center - (Viewing Width / 2)
				 */
				mapShiftX = leftRightVRDifference / 2 + (int) (vr_fieldBoundary.Left * RenderObjectScaling) -
				            RenderWidth / 2;
			} else {
				// System.Diagnostics.Debug.WriteLine("[{4}] VR.Right {0}, Width {1}, Relative {2}. [Scaling {3}]", 
				//      vr.Right, RenderWidth, (int)(vr.Right - RenderWidth), (int)((vr.Right - (RenderWidth * RenderObjectScaling)) * RenderObjectScaling),
				//     mapShiftX + offset);

				if (bIsLeftKeyPressed) {
					mapShiftX = Math.Max((int) (vr_fieldBoundary.Left * RenderObjectScaling),
						mapShiftX - moveOffset);
				} else if (bIsRightKeyPressed) {
					mapShiftX = Math.Min((int) (vr_fieldBoundary.Right - RenderWidth / RenderObjectScaling),
						mapShiftX + moveOffset);
				}
			}
		}

		/// <summary>
		/// Move the camera Y viewing range by a specific offset, & centering if needed.
		/// </summary>
		/// <param name="bIsUpKeyPressed"></param>
		/// <param name="bIsDownKeyPressed"></param>
		/// <param name="moveOffset"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetCameraMoveY(bool bIsUpKeyPressed, bool bIsDownKeyPressed, int moveOffset) {
			var topDownVRDifference = (int) ((vr_fieldBoundary.Bottom - vr_fieldBoundary.Top) * RenderObjectScaling);
			if (topDownVRDifference < RenderHeight) {
				mapShiftY = topDownVRDifference / 2 + (int) (vr_fieldBoundary.Top * RenderObjectScaling) -
				            RenderHeight / 2;
			} else {
				/*System.Diagnostics.Debug.WriteLine("[{0}] VR.Bottom {1}, Height {2}, Relative {3}. [Scaling {4}]",
				    (int)((vr.Bottom - (RenderHeight))),
				    vr.Bottom, RenderHeight, (int)(vr.Bottom - RenderHeight),
				    mapShiftX + offset);*/


				if (bIsUpKeyPressed) {
					mapShiftY = Math.Max(vr_fieldBoundary.Top, mapShiftY - moveOffset);
				} else if (bIsDownKeyPressed) {
					mapShiftY = Math.Min((int) (vr_fieldBoundary.Bottom - RenderHeight / RenderObjectScaling),
						mapShiftY + moveOffset);
				}
			}
		}

		#endregion

		#region Spine specific

		public void Start(AnimationState state, int trackIndex) {
#if !WINDOWS_STOREAPP
			Console.WriteLine(trackIndex + " " + state.GetCurrent(trackIndex) + ": start");
#endif
		}

		public void End(AnimationState state, int trackIndex) {
#if !WINDOWS_STOREAPP
			Console.WriteLine(trackIndex + " " + state.GetCurrent(trackIndex) + ": end");
#endif
		}

		public void Complete(AnimationState state, int trackIndex, int loopCount) {
#if !WINDOWS_STOREAPP
			Console.WriteLine(trackIndex + " " + state.GetCurrent(trackIndex) + ": complete " + loopCount);
#endif
		}

		public void Event(AnimationState state, int trackIndex, Event e) {
#if !WINDOWS_STOREAPP
			Console.WriteLine(trackIndex + " " + state.GetCurrent(trackIndex) + ": event " + e);
#endif
		}

		#endregion
	}
}