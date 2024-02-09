/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using HaCreator.Exceptions;
using HaCreator.MapEditor.Info;
using HaCreator.MapEditor.Instance;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using Newtonsoft.Json;

namespace HaCreator.MapEditor {
	public class UserObjectsManager {
		public const string oS = "haha01haha01";
		public const string l1 = "userObjs";

		private MultiBoard multiBoard;
		private WzImageProperty l1prop;
		private List<ObjectInfo> newObjects = new List<ObjectInfo>();
		private Dictionary<string, byte[]> newObjectsData = new Dictionary<string, byte[]>();
		private string serializedFormCache;
		private bool dirty;

		public UserObjectsManager(MultiBoard multiBoard) {
			this.multiBoard = multiBoard;

			if (Program.InfoManager == null)
				// Prevents VS designer from crashing when rendering this control; there is no way that Program.InfoManager will be null
				// in the real execution of this code.
			{
				return;
			}

			// Make sure that all our structures exist
			if (!Program.InfoManager.ObjectSets.ContainsKey(oS)) {
				Program.InfoManager.ObjectSets[oS] = new WzImage(oS);
				Program.InfoManager.ObjectSets[oS].Changed = true;
			}

			var osimg = Program.InfoManager.ObjectSets[oS];
			if (osimg[Program.APP_NAME] == null) osimg[Program.APP_NAME] = new WzSubProperty();

			var l0prop = osimg[Program.APP_NAME];
			if (l0prop[l1] == null) l0prop[l1] = new WzSubProperty();

			l1prop = l0prop[l1];
		}

		private byte[] SaveImageToBytes(Bitmap bmp) {
			var ms = new MemoryStream();
			bmp.Save(ms, ImageFormat.Png);
			return ms.ToArray();
		}

		public ObjectInfo Add(Bitmap bmp, string name) {
			if (!IsNameValid(name)) {
				throw new NameAlreadyUsedException();
			}

			var origin = new Point(bmp.Width / 2, bmp.Height / 2);

			var prop = new WzSubProperty();
			var canvasProp = new WzCanvasProperty();
			canvasProp.PngProperty = new WzPngProperty();
			canvasProp.PngProperty.PixFormat = (int) WzPngProperty.CanvasPixFormat.Argb4444;
			canvasProp.PngProperty.SetImage(bmp);
			canvasProp["origin"] =
				new WzVectorProperty("", new WzIntProperty("X", origin.X), new WzIntProperty("Y", origin.Y));
			canvasProp["z"] = new WzIntProperty("", 0);
			prop["0"] = canvasProp;

			var oi = new ObjectInfo(bmp, origin, oS, Program.APP_NAME, l1, name, prop);
			newObjects.Add(oi);
			newObjectsData.Add(name, SaveImageToBytes(bmp));
			SerializeObjects();
			l1prop[name] = prop;

			return oi;
		}

		public void Remove(string l2) {
			// Remove it from the serialized form
			if (newObjectsData.ContainsKey(l2)) {
				newObjectsData.Remove(l2);
				SerializeObjects();
			}

			// Remove all instances of it
			foreach (var board in multiBoard.Boards) {
				for (var i = 0; i < board.BoardItems.TileObjs.Count; i++) {
					var li = board.BoardItems.TileObjs[i];
					if (li is ObjectInstance) {
						var oi = (ObjectInfo) li.BaseInfo;
						if (oi.oS == oS && oi.l0 == Program.APP_NAME && oi.l1 == l1 && oi.l2 == l2) {
							li.RemoveItem(null);
							i--;
						}
					}
				}
			}

			// Search it in newObjects
			foreach (var oi in newObjects) {
				if (oi.l2 == l2) {
					newObjects.Remove(oi);
					oi.ParentObject.Remove();
					return;
				}
			}

			// Search it in wz objects
			foreach (var prop in l1prop.WzProperties) {
				if (prop.Name == l2) {
					prop.Remove();
					// We removed a property that existed in the file already, so we must set it as updated
					SetOsUpdated();
					return;
				}
			}

			throw new Exception("Could not find " + l2 + " in userObjs");
		}

		public void Flush() {
			if (newObjects.Count == 0) {
				return;
			}

			var objsDir = (WzDirectory) Program.WzManager["map"]["Obj"]; // "obj' is in Map.wz or Map2.wz (TODO)
			if (objsDir[oS + ".img"] == null) {
				objsDir[oS + ".img"] = Program.InfoManager.ObjectSets[oS];
			}

			SetOsUpdated();
			newObjects.Clear();
		}

		private void SerializeObjects() {
			if (newObjectsData.Count == 0) {
				serializedFormCache = null;
			} else {
				serializedFormCache = JsonConvert.SerializeObject(newObjectsData);
			}

			dirty = true;
		}

		public void DeserializeObjects(string data) {
			var newObjectsData2 =
				JsonConvert.DeserializeObject<Dictionary<string, byte[]>>(data);
			foreach (var obj in newObjectsData2) {
				if (IsNameValid(obj.Key)) {
					Add((Bitmap) Image.FromStream(new MemoryStream(obj.Value)), obj.Key);
				}
			}

			SerializeObjects();
		}

		private void SetOsUpdated() {
			Program.WzManager.SetWzFileUpdated(
				"map", // "obj' is in Map.wz or Map2.wz (TODO)
				Program.InfoManager.ObjectSets[oS]);
		}

		private bool IsNameValid(string name) {
			return l1prop[name] == null;
		}

		public WzImageProperty L1Property => l1prop;

		public List<ObjectInfo> NewObjects => newObjects;

		public MultiBoard MultiBoard => multiBoard;

		public string SerializedForm => serializedFormCache;

		public bool Dirty {
			get => dirty;
			set => dirty = value;
		}
	}
}