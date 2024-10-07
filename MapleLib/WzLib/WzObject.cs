/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2009, 2010, 2015 Snow and haha01haha01

 * This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using System;
using System.Drawing;
using MapleLib.Helpers;

namespace MapleLib.WzLib {
	/// <summary>
	/// An abstract class for wz objects
	/// </summary>
	public abstract class WzObject : IDisposable {
		internal string name;
		private object hcTag;
		private object hcTag_spine;
		private object msTag;
		private object msTag_spine;
		private object tag3;

		public abstract void Dispose();

		/// <summary>
		/// The name of the object
		/// </summary>
		public string Name {
			get => name;
			set {
				name = value;
				// Not greatest place to put List.wz checks but I'd rather
				// not include it in Name setter
				if (this is WzImageProperty property && property.ParentImage != null) {
					ListWzContainerImpl.MarkListWzProperty(property.ParentImage);
				} else if (this is WzImage image) {
					ListWzContainerImpl.MarkListWzProperty(image);
				}
			}
		}

		/// <summary>
		/// The WzObjectType of the object
		/// </summary>
		public abstract WzObjectType ObjectType { get; }

		/// <summary>
		/// Returns the parent object
		/// </summary>
		public abstract WzObject Parent { get; internal set; }

		/// <summary>
		/// Returns the parent WZ File
		/// </summary>
		public abstract WzFile WzFileParent { get; }

		public WzObject this[string name] {
			get {
				var wzObject = this;

				if (wzObject is WzFile file) {
					return file[name];
				}

				if (wzObject is WzDirectory dir) {
					return dir[name];
				}

				if (wzObject is WzImage img) {
					return img[name];
				}

				if (wzObject is WzImageProperty imgProp) {
					return imgProp[name];
				}

				throw new NotImplementedException();
			}
		}


		/// <summary>
		/// Gets the top most WZObject directory (i.e Map.wz, Skill.wz)
		/// </summary>
		/// <returns></returns>
		public WzObject GetTopMostWzDirectory() {
			var parent = Parent;
			if (parent == null) {
				return this; // this
			}

			while (parent.Parent != null) parent = parent.Parent;

			return parent;
		}

		public string FullPath {
			get {
				if (this is WzFile file) {
					return file.WzDirectory.Name;
				}

				var result = Name;
				var currObj = this;
				while (currObj.Parent != null) {
					currObj = currObj.Parent;
					result = currObj.Name + "/" + result;
				}

				return result;
			}
		}

		/// <summary>
		/// Used in HaCreator to save already parsed images
		/// </summary>
		public virtual object HCTag {
			get => hcTag;
			set => hcTag = value;
		}


		/// <summary>
		/// Used in HaCreator to save already parsed spine images
		/// </summary>
		public virtual object HCTagSpine {
			get => hcTag_spine;
			set => hcTag_spine = value;
		}

		/// <summary>
		/// Used in HaCreator's MapSimulator to save already parsed textures
		/// </summary>
		public virtual object MSTag {
			get => msTag;
			set => msTag = value;
		}

		/// <summary>
		/// Used in HaCreator's MapSimulator to save already parsed spine objects
		/// </summary>
		public virtual object MSTagSpine {
			get => msTag_spine;
			set => msTag_spine = value;
		}

		/// <summary>
		/// Used in HaRepacker to save WzNodes
		/// </summary>
		public virtual object HRTag {
			get => tag3;
			set => tag3 = value;
		}

		public virtual object WzValue => null;

		public abstract void Remove();

		/// <summary>
		/// Attempts to add the provided WzObject to the current object
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public (bool, ReplaceResult) Add(DuplicateHandler handler, WzObject obj) {
			var current = this;
			if (current is WzFile file) {
				current = file.WzDirectory;
			}
			var result = ReplaceResult.NoneSelectedYet;
			while (current[obj.Name] != null) {
				result = handler.Handle(this, obj);
				if (!result.IsSuccess()) {
					return (false, result);
				}
			}

			if (current is WzDirectory directory) {
				if (obj is WzDirectory wzDirectory) {
					directory.AddDirectory(wzDirectory);
				} else if (obj is WzImage wzImgProperty) {
					directory.AddImage(wzImgProperty);
				} else {
					return (false, result);
				}
			} else if (current is WzImage img) {
				if (!img.Parsed) {
					img.ParseImage();
				}

				if (obj is WzImageProperty imgProperty) {
					img.AddProperty(imgProperty);
					img.Changed = true;
				} else {
					return (false, result);
				}
			} else if (current is IPropertyContainer container) {
				if (obj is WzImageProperty property) {
					container.AddProperty(property);
					if (current is WzImageProperty imgProperty) {
						var parent = imgProperty.ParentImage;
						if (parent != null) {
							parent.Changed = true;
						}
					}
				} else {
					return (false, result);
				}
			} else {
				return (false, result);
			}

			return (true, result);
		}

		//Credits to BluePoop for the idea of using cast overriding
		//2015 - That is the worst idea ever, removed and replaced with Get* methods

		#region Cast Values

		public virtual int GetInt() {
			throw new NotImplementedException();
		}

		public virtual short GetShort() {
			throw new NotImplementedException();
		}

		public virtual long GetLong() {
			throw new NotImplementedException();
		}

		public virtual float GetFloat() {
			throw new NotImplementedException();
		}

		public virtual double GetDouble() {
			throw new NotImplementedException();
		}

		public virtual string GetString() {
			throw new NotImplementedException();
		}

		public virtual Point GetPoint() {
			throw new NotImplementedException();
		}

		public virtual Bitmap GetBitmap() {
			throw new NotImplementedException();
		}

		public virtual byte[] GetBytes() {
			throw new NotImplementedException();
		}

		#endregion
	}
}