using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using HaCreator.MapEditor;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Info.Default;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.Instance.Shapes;
using HaCreator.MapSimulator.MapObjects.UIObject;
using HaCreator.MapSimulator.Objects.FieldObject;
using HaCreator.MapSimulator.Objects.UIObject;
using HaCreator.Properties;
using HaSharedLibrary.Render.DX;
using HaSharedLibrary.Util;
using HaSharedLibrary.Wz;
using MapleLib.Converters;
using MapleLib.Helpers;
using MapleLib.WzLib;
using MapleLib.WzLib.Spine;
using MapleLib.WzLib.WzProperties;
using MapleLib.WzLib.WzStructure;
using MapleLib.WzLib.WzStructure.Data;
using Microsoft.Xna.Framework.Graphics;
using Spine;
using Color = System.Drawing.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace HaCreator.MapSimulator {
	public class MapSimulatorLoader {
		private const string GLOBAL_FONT = "Arial";

		/// <summary>
		/// Create map simulator board
		/// </summary>
		/// <param name="mapBoard"></param>
		/// <param name="titleName"></param>
		/// <returns></returns>
		public static MapSimulator CreateAndShowMapSimulator(Board mapBoard, string titleName) {
			if (mapBoard.MiniMap == null) {
				mapBoard.RegenerateMinimap();
			}

			MapSimulator mapSimulator = null;

			var thread = new Thread(() => {
				mapSimulator = new MapSimulator(mapBoard, titleName);
				mapSimulator.Run();
			}) {
				Priority = ThreadPriority.Highest
			};
			thread.Start();
			thread.Join();

			return mapSimulator;
		}

		#region Common

		/// <summary>
		/// Load frames from WzSubProperty or WzCanvasProperty
		/// </summary>
		/// <param name="texturePool"></param>
		/// <param name="source"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="device"></param>
		/// <param name="usedProps"></param>
		/// <param name="spineAni">Spine animation path</param>
		/// <returns></returns>
		private static List<IDXObject> LoadFrames(TexturePool texturePool, WzImageProperty source, int x, int y,
			GraphicsDevice device, ref List<WzObject> usedProps, string spineAni = Defaults.Background.SpineAni) {
			var frames = new List<IDXObject>();

			source = WzInfoTools.GetRealProperty(source);

			if (source is WzSubProperty property1 && property1.WzProperties.Count == 1) {
				source = property1.WzProperties[0];
			}

			if (source is WzCanvasProperty property) { //one-frame
				var bLoadedSpine = LoadSpineMapObjectItem(source, source, device, spineAni);
				if (!bLoadedSpine) {
					var canvasBitmapPath = property.FullPath;
					var textureFromCache = texturePool.GetTexture(canvasBitmapPath);
					if (textureFromCache != null) {
						source.MSTag = textureFromCache;
					} else {
						source.MSTag = property.GetLinkedWzCanvasBitmap().ToTexture2D(device);

						// add to cache
						texturePool.AddTextureToPool(canvasBitmapPath, (Texture2D) source.MSTag);
					}
				}

				usedProps.Add(source);

				if (source.MSTagSpine != null) {
					var spineObject = (WzSpineObject) source.MSTagSpine;
					var origin = property.GetCanvasOriginPosition();

					frames.Add(new DXSpineObject(spineObject, x, y, origin));
				} else if (source.MSTag != null) {
					var texture = (Texture2D) source.MSTag;
					var origin = property.GetCanvasOriginPosition();

					frames.Add(new DXObject(x - (int) origin.X, y - (int) origin.Y, texture));
				} else // fallback
				{
					var texture = Resources.placeholder.ToTexture2D(device);
					var origin = property.GetCanvasOriginPosition();

					frames.Add(new DXObject(x - (int) origin.X, y - (int) origin.Y, texture));
				}
			} else if (source is WzSubProperty) // animated
			{
				WzImageProperty _frameProp;
				var i = 0;

				while ((_frameProp = WzInfoTools.GetRealProperty(source[(i++).ToString()])) != null) {
					if (_frameProp is WzSubProperty) // issue with 867119250
					{
						frames.AddRange(LoadFrames(texturePool, _frameProp, x, y, device, ref usedProps));
					} else {
						WzCanvasProperty frameProp;

						if (_frameProp is WzUOLProperty) // some could be UOL. Ex: 321100000 Mirror world: [Mirror World] Leafre
						{
							var linkVal = ((WzUOLProperty) _frameProp).LinkValue;
							if (linkVal is WzCanvasProperty linkCanvas) {
								frameProp = linkCanvas;
							} else {
								continue;
							}
						} else {
							frameProp = (WzCanvasProperty) _frameProp;
						}

						var delay = frameProp["delay"].GetOptionalInt(100);

						var bLoadedSpine = LoadSpineMapObjectItem((WzImageProperty) frameProp.Parent, frameProp,
							device, spineAni);
						if (!bLoadedSpine) {
							if (frameProp.MSTag == null) {
								var canvasBitmapPath = frameProp.FullPath;
								var textureFromCache = texturePool.GetTexture(canvasBitmapPath);
								if (textureFromCache != null) {
									frameProp.MSTag = textureFromCache;
								} else {
									frameProp.MSTag = frameProp.GetLinkedWzCanvasBitmap().ToTexture2D(device);

									// add to cache
									texturePool.AddTextureToPool(canvasBitmapPath, (Texture2D) frameProp.MSTag);
								}
							}
						}

						usedProps.Add(frameProp);

						if (frameProp.MSTagSpine != null) {
							var spineObject = (WzSpineObject) frameProp.MSTagSpine;
							var origin = frameProp.GetCanvasOriginPosition();

							frames.Add(new DXSpineObject(spineObject, x, y, origin, delay));
						} else if (frameProp.MSTag != null) {
							var texture = (Texture2D) frameProp.MSTag;
							var origin = frameProp.GetCanvasOriginPosition();

							frames.Add(new DXObject(x - (int) origin.X, y - (int) origin.Y, texture, delay));
						} else {
							var texture = Resources.placeholder.ToTexture2D(device);
							var origin = frameProp.GetCanvasOriginPosition();

							frames.Add(new DXObject(x - (int) origin.X, y - (int) origin.Y, texture, delay));
						}
					}
				}
			}

			return frames;
		}

		#endregion

		/// <summary>
		/// Map item
		/// </summary>
		/// <param name="texturePool"></param>
		/// <param name="source"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="mapCenterX"></param>
		/// <param name="mapCenterY"></param>
		/// <param name="device"></param>
		/// <param name="usedProps"></param>
		/// <param name="flip"></param>
		/// <returns></returns>
		public static BaseDXDrawableItem CreateMapItemFromProperty(TexturePool texturePool, WzImageProperty source,
			int x, int y, Point mapCenter, GraphicsDevice device, ref List<WzObject> usedProps, bool flip) {
			var mapItem =
				new BaseDXDrawableItem(LoadFrames(texturePool, source, x, y, device, ref usedProps), flip);
			return mapItem;
		}

		/// <summary>
		/// Background
		/// </summary>
		/// <param name="texturePool"></param>
		/// <param name="source"></param>
		/// <param name="bgInstance"></param>
		/// <param name="device"></param>
		/// <param name="usedProps"></param>
		/// <param name="flip"></param>
		/// <returns></returns>
		public static BackgroundItem CreateBackgroundFromProperty(TexturePool texturePool, WzImageProperty source,
			BackgroundInstance bgInstance, GraphicsDevice device, ref List<WzObject> usedProps, bool flip) {
			var frames = LoadFrames(texturePool, source, bgInstance.BaseX, bgInstance.BaseY, device,
				ref usedProps, bgInstance.SpineAni);
			if (frames.Count == 0) {
				var error = string.Format("[MapSimulatorLoader] 0 frames loaded for bg texture from src: '{0}'",
					source.FullPath); // Back_003.wz\\BM3_3.img\\spine\\0

				ErrorLogger.Log(ErrorLevel.IncorrectStructure, error);
				return null;
			}

			if (frames.Count == 1) {
				return new BackgroundItem(bgInstance.cx, bgInstance.cy, bgInstance.rx, bgInstance.ry, bgInstance.type,
					bgInstance.a, bgInstance.front, frames[0], flip, bgInstance.screenMode);
			}

			return new BackgroundItem(bgInstance.cx, bgInstance.cy, bgInstance.rx, bgInstance.ry, bgInstance.type,
				bgInstance.a, bgInstance.front, frames, flip, bgInstance.screenMode);
		}

		#region Spine

		/// <summary>
		/// Load spine object from WzImageProperty (bg, map item)
		/// </summary>
		/// <param name="source"></param>
		/// <param name="prop"></param>
		/// <param name="device"></param>
		/// <returns></returns>
		private static bool LoadSpineMapObjectItem(WzImageProperty source, WzImageProperty prop, GraphicsDevice device,
			string spineAniPath = Defaults.Background.SpineAni) {
			WzImageProperty spineAtlas = null;

			var isObjectLayer = source.Parent.Name == "spine";
			if (isObjectLayer) { // load spine if the source is already the directory we need
				var spineAtlasPath = ((WzStringProperty) source["spine"])?.GetString();
				if (spineAtlasPath != null) spineAtlas = source[spineAtlasPath + ".atlas"];
			} else if (!string.IsNullOrEmpty(spineAniPath)) {
				var spineSource = (WzImageProperty) source.Parent?.Parent["spine"]?[source.Name];

				var spineAtlasPath = ((WzStringProperty) spineSource["spine"])?.GetString();
				if (spineAtlasPath != null) spineAtlas = spineSource[spineAtlasPath + ".atlas"];
			} else { // simply check if 'spine' WzStringProperty exist, fix for Adele town
				var spineAtlasPath = ((WzStringProperty) source["spine"])?.GetString();
				if (spineAtlasPath != null) {
					spineAtlas = source[spineAtlasPath + ".atlas"];
					isObjectLayer = true;
				}
			}

			if (!(spineAtlas is WzStringProperty stringObj)) return false;
			if (!stringObj.IsSpineAtlasResources) {
				return false;
			}

			var spineObject = new WzSpineObject(new WzSpineAnimationItem(stringObj));

			spineObject.spineAnimationItem
				.LoadResources(device); //  load spine resources (this must happen after window is loaded)
			spineObject.skeleton = new Skeleton(spineObject.spineAnimationItem.SkeletonData);

			//spineObject.skeleton.R =153;
			//spineObject.skeleton.G = 255;
			//spineObject.skeleton.B = 0;
			//spineObject.skeleton.A = 1f;
			// Skin
			foreach (var skin in spineObject.spineAnimationItem.SkeletonData.Skins) {
				spineObject.skeleton.SetSkin(skin); // just set the first skin
				break;
			}

			// Define mixing between animations.
			spineObject.stateData = new AnimationStateData(spineObject.skeleton.Data);
			spineObject.state = new AnimationState(spineObject.stateData);
			if (!isObjectLayer) {
				spineObject.state.TimeScale = 0.1f;
			}

			if (spineAniPath != null) {
				spineObject.state.SetAnimation(0, spineAniPath, true);
			} else {
				var i = 0;
				foreach (var animation in spineObject.spineAnimationItem.SkeletonData.Animations)
					spineObject.state.SetAnimation(i++, animation.Name, true);
			}

			prop.MSTagSpine = spineObject;
			return true;
		}

		#endregion

		#region Reactor

		/// <summary>
		/// Create reactor item
		/// </summary>
		/// <param name="texturePool"></param>
		/// <param name="reactorInstance"></param>
		/// <param name="device"></param>
		/// <param name="usedProps"></param>
		/// <returns></returns>
		public static ReactorItem CreateReactorFromProperty(TexturePool texturePool, ReactorInstance reactorInstance,
			GraphicsDevice device, ref List<WzObject> usedProps) {
			var reactorInfo = (ReactorInfo) reactorInstance.BaseInfo;

			var frames = new List<IDXObject>();

			var linkedReactorImage = reactorInfo.LinkedWzImage;
			if (linkedReactorImage != null) {
				var framesImage = linkedReactorImage?["0"]?["0"];
				if (framesImage != null) {
					frames = LoadFrames(texturePool, framesImage, reactorInstance.X, reactorInstance.Y, device,
						ref usedProps);
				}
			}

			if (frames.Count == 0)
				//string error = string.Format("[MapSimulatorLoader] 0 frames loaded for reactor from src: '{0}'",  reactorInfo.ID);
				//ErrorLogger.Log(ErrorLevel.IncorrectStructure, error);
			{
				return null;
			}

			return new ReactorItem(reactorInstance, frames);
		}

		#endregion

		#region Portal

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texturePool"></param>
		/// <param name="gameParent"></param>
		/// <param name="portalInstance"></param>
		/// <param name="device"></param>
		/// <param name="usedProps"></param>
		/// <returns></returns>
		public static PortalItem CreatePortalFromProperty(TexturePool texturePool, WzSubProperty gameParent,
			PortalInstance portalInstance, GraphicsDevice device, ref List<WzObject> usedProps) {
			var portalInfo = (PortalInfo) portalInstance.BaseInfo;

			if (portalInstance.pt == PortalType.StartPoint ||
			    portalInstance.pt == PortalType.Invisible ||
			    //portalInstance.pt == PortalType.Changeable_Invisible ||
			    portalInstance.pt == PortalType.ScriptInvisible ||
			    portalInstance.pt == PortalType.Script ||
			    portalInstance.pt == PortalType.Collision ||
			    portalInstance.pt == PortalType.CollisionScript ||
			    portalInstance.pt ==
			    PortalType.CollisionCustomImpact || // springs in Mechanical grave 350040240
			    portalInstance.pt == PortalType.CollisionVerticalJump) // vertical spring actually
			{
				return null;
			}

			var
				frames = new List<IDXObject>(); // All frames "stand", "speak" "blink" "hair", "angry", "wink" etc

			//string portalType = portalInstance.pt;
			//int portalId = Program.InfoManager.PortalIdByType[portalInstance.pt];

			var portalTypeProperty = (WzSubProperty) gameParent[Program.InfoManager.PortalTypeById[portalInstance.pt]];
			if (portalTypeProperty == null) {
				portalTypeProperty = (WzSubProperty) gameParent["pv"];
			} else {
				// Support for older versions of MapleStory where 'pv' is a subproperty for the image frame than a collection of subproperty of frames
				if (portalTypeProperty["0"] is WzCanvasProperty) {
					frames.AddRange(LoadFrames(texturePool, portalTypeProperty, portalInstance.X, portalInstance.Y,
						device, ref usedProps));
					portalTypeProperty = null;
				}
			}

			if (portalTypeProperty != null) {
				var portalImageProperty =
					(WzSubProperty) portalTypeProperty[portalInstance.image == null ? "default" : portalInstance.image];

				if (portalImageProperty != null) {
					WzSubProperty framesPropertyParent;
					if (portalImageProperty["portalContinue"] != null) {
						framesPropertyParent = (WzSubProperty) portalImageProperty["portalContinue"];
					} else {
						framesPropertyParent = portalImageProperty;
					}

					if (framesPropertyParent != null) {
						frames.AddRange(LoadFrames(texturePool, framesPropertyParent, portalInstance.X,
							portalInstance.Y, device, ref usedProps));
					}
				}
			}

			if (frames.Count == 0) {
				return null;
			}

			return new PortalItem(portalInstance, frames);
		}

		#endregion

		#region Life

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texturePool"></param>
		/// <param name="mobInstance"></param>
		/// <param name="device"></param>
		/// <param name="usedProps"></param>
		/// <returns></returns>
		public static MobItem CreateMobFromProperty(TexturePool texturePool, MobInstance mobInstance,
			GraphicsDevice device, ref List<WzObject> usedProps) {
			var mobInfo = (MobInfo) mobInstance.BaseInfo;
			var source = mobInfo.LinkedWzImage;

			var
				frames = new List<IDXObject>(); // All frames "stand", "speak" "blink" "hair", "angry", "wink" etc

			foreach (var childProperty in source.WzProperties) {
				if (childProperty is WzSubProperty mobStateProperty) // issue with 867119250, Eluna map mobs
				{
					switch (mobStateProperty.Name) {
						case "info": // info/speak/0 WzStringProperty
						{
							break;
						}
						default: {
							frames.AddRange(LoadFrames(texturePool, mobStateProperty, mobInstance.X, mobInstance.Y,
								device, ref usedProps));
							break;
						}
					}
				}
			}

			return new MobItem(mobInstance, frames);
		}

		/// <summary>
		/// NPC
		/// </summary>
		/// <param name="texturePool"></param>
		/// <param name="npcInstance"></param>
		/// <param name="device"></param>
		/// <param name="usedProps"></param>
		/// <returns></returns>
		public static NpcItem CreateNpcFromProperty(TexturePool texturePool, NpcInstance npcInstance,
			GraphicsDevice device, ref List<WzObject> usedProps) {
			var npcInfo = (NpcInfo) npcInstance.BaseInfo;
			var source = npcInfo.LinkedWzImage;

			var
				frames = new List<IDXObject>(); // All frames "stand", "speak" "blink" "hair", "angry", "wink" etc

			foreach (var childProperty in source.WzProperties) {
				var npcStateProperty = (WzSubProperty) childProperty;
				switch (npcStateProperty.Name) {
					case "info": // info/speak/0 WzStringProperty
					{
						break;
					}
					default: {
						frames.AddRange(LoadFrames(texturePool, npcStateProperty, npcInstance.X, npcInstance.Y, device,
							ref usedProps));
						break;
					}
				}
			}

			return new NpcItem(npcInstance, frames);
		}

		#endregion

		#region UI

		/// <summary>
		/// Draws the frame and the UI of the minimap.
		/// TODO: This whole thing needs to be dramatically simplified via further abstraction to keep it noob-proof :(
		/// </summary>
		/// <param name="uiWindow1Image">UI.wz/UIWindow1.img pre-bb</param>
		/// <param name="uiWindow2Image">UI.wz/UIWindow2.img post-bb</param>
		/// <param name="uiBasicImage">UI.wz/Basic.img</param>
		/// <param name="mapBoard"></param>
		/// <param name="device"></param>
		/// <param name="UserScreenScaleFactor">The scale factor of the window (DPI)</param>
		/// <param name="MapName">The map name. i.e The Hill North</param>
		/// <param name="StreetName">The street name. i.e Hidden street</param>
		/// <param name="soundUIImage">Sound.wz/UI.img</param>
		/// <param name="bBigBang">Big bang update</param>
		/// <returns></returns>
		public static MinimapItem CreateMinimapFromProperty(WzImage uiWindow1Image, WzImage uiWindow2Image,
			WzImage uiBasicImage, Board mapBoard, GraphicsDevice device, float UserScreenScaleFactor, string MapName,
			string StreetName, WzImage soundUIImage, bool bBigBang) {
			if (mapBoard.MiniMap == null) {
				return null;
			}

			var minimapFrameProperty = (WzSubProperty) uiWindow2Image?["MiniMap"];
			if (minimapFrameProperty == null) // UIWindow2 not available pre-BB.
			{
				minimapFrameProperty = (WzSubProperty) uiWindow1Image["MiniMap"];
			}

			var maxMapProperty = (WzSubProperty) minimapFrameProperty["MaxMap"];
			var miniMapProperty = (WzSubProperty) minimapFrameProperty["MinMap"];
			var maxMapMirrorProperty = (WzSubProperty) minimapFrameProperty["MaxMapMirror"]; // for Zero maps
			var miniMapMirrorProperty = (WzSubProperty) minimapFrameProperty["MinMapMirror"]; // for Zero maps


			WzSubProperty useFrame;
			if (mapBoard.MapInfo.zeroSideOnly || MapConstants.IsZerosTemple(mapBoard.MapInfo.id)) // zero's temple
			{
				useFrame = maxMapMirrorProperty;
			} else {
				useFrame = maxMapProperty;
			}

			// Wz frames
			var c = ((WzCanvasProperty) useFrame?["c"])?.GetLinkedWzCanvasBitmap();
			var e = ((WzCanvasProperty) useFrame?["e"])?.GetLinkedWzCanvasBitmap();
			var n = ((WzCanvasProperty) useFrame?["n"])?.GetLinkedWzCanvasBitmap();
			var s = ((WzCanvasProperty) useFrame?["s"])?.GetLinkedWzCanvasBitmap();
			var w = ((WzCanvasProperty) useFrame?["w"])?.GetLinkedWzCanvasBitmap();
			var ne = ((WzCanvasProperty) useFrame?["ne"])?.GetLinkedWzCanvasBitmap(); // top right
			var nw = ((WzCanvasProperty) useFrame?["nw"])?.GetLinkedWzCanvasBitmap(); // top left
			var se = ((WzCanvasProperty) useFrame?["se"])?.GetLinkedWzCanvasBitmap(); // bottom right
			var sw = ((WzCanvasProperty) useFrame?["sw"])?.GetLinkedWzCanvasBitmap(); // bottom left

			// Constants
			const float TOOLTIP_FONTSIZE = 10f;
			const int MAPMARK_IMAGE_ALIGN_LEFT = 7; // the number of pixels from the left to draw the map mark image
			const int MAP_IMAGE_PADDING = 2; // the number of pixels from the left to draw the minimap image
			var color_bgFill = Color.Transparent;
			var color_foreGround = Color.White;

			var renderText = string.Format("{0}{1}{2}", StreetName, Environment.NewLine, MapName);


			// Map background image
			var
				miniMapImage = mapBoard.MiniMap; // the original minimap image without UI frame overlay
			var effective_width = miniMapImage.Width + e.Width + w.Width;
			var effective_height = miniMapImage.Height + n.Height + s.Height;

			using (var font =
			       new Font(GLOBAL_FONT, TOOLTIP_FONTSIZE / UserScreenScaleFactor)) {
				// Get the width of the 'streetName' or 'mapName'
				var graphics_dummy =
					Graphics.FromImage(new Bitmap(1,
						1)); // dummy image just to get the Graphics object for measuring string
				var tooltipSize = graphics_dummy.MeasureString(renderText, font);

				effective_width = Math.Max((int) tooltipSize.Width + nw.Width, effective_width); // set new width

				var miniMapAlignXFromLeft = MAP_IMAGE_PADDING;
				if (effective_width >
				    miniMapImage
					    .Width) // if minimap is smaller in size than the (text + frame), minimap will be aligned to the center instead
				{
					miniMapAlignXFromLeft = (effective_width - miniMapImage.Width) / 2 /* - miniMapAlignXFromLeft*/;
				}

				var miniMapUIImage = new Bitmap(effective_width, effective_height);
				using (var graphics = Graphics.FromImage(miniMapUIImage)) {
					// Frames and background
					UIFrameHelper.DrawUIFrame(graphics, color_bgFill, ne, nw, se, sw, e, w, n, s, null, effective_width,
						effective_height);

					// Map name + street name
					graphics.DrawString(
						renderText,
						font, new SolidBrush(color_foreGround), 50, 20);

					// Map mark
					if (Program.InfoManager.MapMarks.ContainsKey(mapBoard.MapInfo.mapMark)) {
						var mapMark = Program.InfoManager.MapMarks[mapBoard.MapInfo.mapMark];
						graphics.DrawImage(mapMark.ToImage(), MAPMARK_IMAGE_ALIGN_LEFT, 17);
					}

					// Map image
					graphics.DrawImage(miniMapImage,
						miniMapAlignXFromLeft, // map is on the center
						n.Height);

					graphics.Flush();
				}

				// Dots pixel 
				var bmp_DotPixel = new Bitmap(2, 4);
				using (var graphics = Graphics.FromImage(bmp_DotPixel)) {
					graphics.FillRectangle(new SolidBrush(Color.Yellow),
						new RectangleF(0, 0, bmp_DotPixel.Width, bmp_DotPixel.Height));
					graphics.Flush();
				}

				IDXObject dxObj_miniMapPixel = new DXObject(0, n.Height, bmp_DotPixel.ToTexture2D(device));
				var item_pixelDot = new BaseDXDrawableItem(dxObj_miniMapPixel, false) {
					Position = new Point(
						miniMapAlignXFromLeft, // map is on the center
						0)
				};

				// Map
				var texturer_miniMap = miniMapUIImage.ToTexture2D(device);

				IDXObject dxObj = new DXObject(0, 0, texturer_miniMap);
				var minimapItem = new MinimapItem(dxObj, item_pixelDot);

				////////////// Minimap buttons////////////////////
				// This must be in order. 
				// >>> If aligning from the left to the right. Items at the left must be at the top of the code
				// >>> If aligning from the right to the left. Items at the right must be at the top of the code with its (x position - parent width).
				// TODO: probably a wrapper class in the future, such as HorizontalAlignment and VerticalAlignment, or Grid/ StackPanel 
				var BtMouseClickSoundProperty = (WzBinaryProperty) soundUIImage["BtMouseClick"];
				var BtMouseOverSoundProperty = (WzBinaryProperty) soundUIImage["BtMouseOver"];

				if (bBigBang) {
					var BtNpc = (WzSubProperty) minimapFrameProperty["BtNpc"]; // npc button
					var BtMin = (WzSubProperty) minimapFrameProperty["BtMin"]; // mininise button
					var BtMax = (WzSubProperty) minimapFrameProperty["BtMax"]; // maximise button
					var BtBig = (WzSubProperty) minimapFrameProperty["BtBig"]; // big button
					var BtMap = (WzSubProperty) minimapFrameProperty["BtMap"]; // world button

					var objUIBtMap = new UIObject(BtMap, BtMouseClickSoundProperty, BtMouseOverSoundProperty,
						false,
						new Point(MAP_IMAGE_PADDING, MAP_IMAGE_PADDING), device);
					objUIBtMap.X =
						effective_width - objUIBtMap.CanvasSnapshotWidth -
						8; // render at the (width of minimap - obj width)

					var objUIBtBig = new UIObject(BtBig, BtMouseClickSoundProperty, BtMouseOverSoundProperty,
						false,
						new Point(MAP_IMAGE_PADDING, MAP_IMAGE_PADDING), device);
					objUIBtBig.X =
						objUIBtMap.X - objUIBtBig.CanvasSnapshotWidth; // render at the (width of minimap - obj width)

					var objUIBtMax = new UIObject(BtMax, BtMouseClickSoundProperty, BtMouseOverSoundProperty,
						false,
						new Point(MAP_IMAGE_PADDING, MAP_IMAGE_PADDING), device);
					objUIBtMax.X =
						objUIBtBig.X - objUIBtMax.CanvasSnapshotWidth; // render at the (width of minimap - obj width)

					var objUIBtMin = new UIObject(BtMin, BtMouseClickSoundProperty, BtMouseOverSoundProperty,
						false,
						new Point(MAP_IMAGE_PADDING, MAP_IMAGE_PADDING), device);
					objUIBtMin.X =
						objUIBtMax.X - objUIBtMin.CanvasSnapshotWidth; // render at the (width of minimap - obj width)

					// BaseClickableUIObject objUINpc = new BaseClickableUIObject(BtNpc, false, new Point(objUIBtMap.CanvasSnapshotWidth + objUIBtBig.CanvasSnapshotWidth + objUIBtMax.CanvasSnapshotWidth + objUIBtMin.CanvasSnapshotWidth, MAP_IMAGE_PADDING), device);

					minimapItem.InitializeMinimapButtons(objUIBtMin, objUIBtMax, objUIBtBig, objUIBtMap);
				} else {
					var BtMin = (WzSubProperty) uiBasicImage["BtMin"]; // mininise button
					var BtMax = (WzSubProperty) uiBasicImage["BtMax"]; // maximise button
					var BtMap = (WzSubProperty) minimapFrameProperty["BtMap"]; // world button

					var objUIBtMap = new UIObject(BtMap, BtMouseClickSoundProperty, BtMouseOverSoundProperty,
						false,
						new Point(MAP_IMAGE_PADDING, MAP_IMAGE_PADDING), device);
					objUIBtMap.X =
						effective_width - objUIBtMap.CanvasSnapshotWidth -
						8; // render at the (width of minimap - obj width)

					var objUIBtMax = new UIObject(BtMax, BtMouseClickSoundProperty, BtMouseOverSoundProperty,
						false,
						new Point(MAP_IMAGE_PADDING, MAP_IMAGE_PADDING), device);
					objUIBtMax.X =
						objUIBtMap.X - objUIBtMax.CanvasSnapshotWidth; // render at the (width of minimap - obj width)

					var objUIBtMin = new UIObject(BtMin, BtMouseClickSoundProperty, BtMouseOverSoundProperty,
						false,
						new Point(MAP_IMAGE_PADDING, MAP_IMAGE_PADDING), device);
					objUIBtMin.X =
						objUIBtMax.X - objUIBtMin.CanvasSnapshotWidth; // render at the (width of minimap - obj width)

					// BaseClickableUIObject objUINpc = new BaseClickableUIObject(BtNpc, false, new Point(objUIBtMap.CanvasSnapshotWidth + objUIBtBig.CanvasSnapshotWidth + objUIBtMax.CanvasSnapshotWidth + objUIBtMin.CanvasSnapshotWidth, MAP_IMAGE_PADDING), device);

					minimapItem.InitializeMinimapButtons(objUIBtMin, objUIBtMax, null, objUIBtMap);
				}

				//////////////////////////////////////////////////

				return minimapItem;
			}
		}

		/// <summary>
		/// Tooltip
		/// </summary>
		/// <param name="texturePool"></param>
		/// <param name="UserScreenScaleFactor">The scale factor of the window (DPI)</param>
		/// <param name="farmFrameParent"></param>
		/// <param name="tooltip"></param>
		/// <param name="device"></param>
		/// <returns></returns>
		public static TooltipItem CreateTooltipFromProperty(TexturePool texturePool, float UserScreenScaleFactor,
			WzSubProperty farmFrameParent, ToolTipInstance tooltip, GraphicsDevice device) {
			// Wz frames
			var c = ((WzCanvasProperty) farmFrameParent?["c"])?.GetLinkedWzCanvasBitmap();
			var cover = ((WzCanvasProperty) farmFrameParent?["cover"])?.GetLinkedWzCanvasBitmap();
			var e = ((WzCanvasProperty) farmFrameParent?["e"])?.GetLinkedWzCanvasBitmap();
			var n = ((WzCanvasProperty) farmFrameParent?["n"])?.GetLinkedWzCanvasBitmap();
			var s = ((WzCanvasProperty) farmFrameParent?["s"])?.GetLinkedWzCanvasBitmap();
			var w = ((WzCanvasProperty) farmFrameParent?["w"])?.GetLinkedWzCanvasBitmap();
			var
				ne = ((WzCanvasProperty) farmFrameParent?["ne"])?.GetLinkedWzCanvasBitmap(); // top right
			var
				nw = ((WzCanvasProperty) farmFrameParent?["nw"])?.GetLinkedWzCanvasBitmap(); // top left
			var
				se = ((WzCanvasProperty) farmFrameParent?["se"])?.GetLinkedWzCanvasBitmap(); // bottom right
			var
				sw = ((WzCanvasProperty) farmFrameParent?["sw"])?.GetLinkedWzCanvasBitmap(); // bottom left


			// tooltip property
			var title = tooltip.Title;
			var desc = tooltip.Desc;

			var renderText = string.Format("{0}{1}{2}", title, Environment.NewLine, desc);

			// Constants
			const float TOOLTIP_FONTSIZE = 9.25f; // thankie willified, ya'll be remembered forever here <3
			//System.Drawing.Color color_bgFill = System.Drawing.Color.FromArgb(230, 17, 54, 82); // pre V patch (dark blue theme used post-bb), leave this here in case someone needs it
			var
				color_bgFill =
					Color.FromArgb(255, 17, 17,
						17); // post V patch (dark black theme used), use color picker on paint via image extracted from WZ if you need to get it
			var color_foreGround = Color.White;
			const int WIDTH_PADDING = 10;
			const int HEIGHT_PADDING = 6;

			// Create
			using (var font =
			       new Font(GLOBAL_FONT, TOOLTIP_FONTSIZE / UserScreenScaleFactor)) {
				var graphics_dummy =
					Graphics.FromImage(new Bitmap(1,
						1)); // dummy image just to get the Graphics object for measuring string
				var tooltipSize = graphics_dummy.MeasureString(renderText, font);

				var effective_width = (int) tooltipSize.Width + WIDTH_PADDING;
				var effective_height = (int) tooltipSize.Height + HEIGHT_PADDING;

				var bmp_tooltip = new Bitmap(effective_width, effective_height);
				using (var graphics = Graphics.FromImage(bmp_tooltip)) {
					// Frames and background
					UIFrameHelper.DrawUIFrame(graphics, color_bgFill, ne, nw, se, sw, e, w, n, s, c, effective_width,
						effective_height);

					// Text
					graphics.DrawString(renderText, font, new SolidBrush(color_foreGround),
						WIDTH_PADDING / 2, HEIGHT_PADDING / 2);
					graphics.Flush();
				}

				IDXObject dxObj = new DXObject(tooltip.X, tooltip.Y, bmp_tooltip.ToTexture2D(device));
				var item = new TooltipItem(tooltip, dxObj);

				return item;
			}
		}


		/// <summary>
		/// Map item
		/// </summary>
		/// <param name="texturePool"></param>
		/// <param name="source"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="mapCenterX"></param>
		/// <param name="mapCenterY"></param>
		/// <param name="device"></param>
		/// <param name="usedProps"></param>
		/// <param name="flip"></param>
		/// <returns></returns>
		public static MouseCursorItem CreateMouseCursorFromProperty(TexturePool texturePool, WzImageProperty source,
			int x, int y, GraphicsDevice device, ref List<WzObject> usedProps, bool flip) {
			var cursorCanvas = (WzSubProperty) source?["0"];
			var cursorPressedCanvas = (WzSubProperty) source?["1"]; // click

			var frames = LoadFrames(texturePool, cursorCanvas, x, y, device, ref usedProps);

			var clickedState = CreateMapItemFromProperty(texturePool, cursorPressedCanvas, 0, 0,
				new Point(0, 0), device, ref usedProps, false);
			return new MouseCursorItem(frames, clickedState);
		}

		#endregion

		private static string DumpFhList(List<FootholdLine> fhs) {
			var res = "";
			foreach (var fh in fhs)
				res += fh.FirstDot.X + "," + fh.FirstDot.Y + " : " + fh.SecondDot.X + "," + fh.SecondDot.Y + "\r\n";
			return res;
		}
	}
}