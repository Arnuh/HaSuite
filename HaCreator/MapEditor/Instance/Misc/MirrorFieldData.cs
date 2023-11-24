/* Copyright (C) 2022 LastBattle
 * https://github.com/lastbattle/Harepacker-resurrected
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using XNA = Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HaCreator.MapEditor.Instance.Shapes;
using Microsoft.Xna.Framework;
using HaSharedLibrary.Render;

namespace HaCreator.MapEditor.Instance.Misc {
	public enum MirrorFieldDataType {
		info,
		mob,
		user,
		npc,

		NULL
	}

	/// <summary>
	/// Boundary where the reflections of map objects should be rendered
	/// </summary>
	public class MirrorFieldData : MiscRectangle, ISerializable {
		#region Fields

		private ReflectionDrawableBoundary reflectionInfo;
		private MirrorFieldDataType mirrorFieldDataType;
		private Vector2 offset;

		#endregion

		/// <summary>
		/// Constructor
		/// Load from data
		/// </summary>
		/// <param name="board"></param>
		/// <param name="rect"></param>
		public MirrorFieldData(Board board, Rectangle rect, Vector2 offset,
			ReflectionDrawableBoundary reflectionInfo, MirrorFieldDataType mirrorFieldDataType)
			: base(board, rect) {
			this.reflectionInfo = reflectionInfo;
			this.mirrorFieldDataType = mirrorFieldDataType;
			this.offset = offset;
		}

		#region Inherited Members

		/// <summary>
		/// Mirror Field Data special X_x it is not handled in the dots
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public override void Move(int x, int y) {
			rect = new Rectangle(x, y, Rectangle.Width, Rectangle.Height);
		}

		public override string Name => "MirrorFieldData (Reflections)";

		#endregion

		#region Custom Members

		public ReflectionDrawableBoundary ReflectionInfo {
			get => reflectionInfo;
			set => reflectionInfo = value;
		}

		public MirrorFieldDataType MirrorFieldDataType {
			get => mirrorFieldDataType;
			set => mirrorFieldDataType = value;
		}

		public Vector2 Offset {
			get => offset;
			set => offset = value;
		}

		#endregion

		#region Cast Values

		/*public override string ToString()
		{
		    return "X: " + x.val.ToString() + ", Y: " + y.val.ToString();
		}*/

		#endregion

		#region Serialization

		/// <summary>
		/// Constructor
		/// Load from serialized form
		/// </summary>
		/// <param name="board"></param>
		/// <param name="json"></param>
		public MirrorFieldData(Board board, SerializationForm json)
			: base(board, json) {
			mirrorFieldDataType = json.mirrorFieldDataType;
			offset = json.offset;
			reflectionInfo = new ReflectionDrawableBoundary(
				json.reflectionInfo.Gradient,
				json.reflectionInfo.Alpha,
				json.reflectionInfo.ObjectForOverlay,
				json.reflectionInfo.Reflection,
				json.reflectionInfo.AlphaTest);
		}

		public new class SerializationForm : MapleRectangle.SerializationForm {
			public ReflectionDrawableBoundary reflectionInfo;
			public MirrorFieldDataType mirrorFieldDataType;
			public Vector2 offset;
		}

		public override object Serialize() {
			var result = new SerializationForm();
			UpdateSerializedForm(result);
			return result;
		}

		protected void UpdateSerializedForm(SerializationForm result) {
			base.UpdateSerializedForm(result);

			result.mirrorFieldDataType = mirrorFieldDataType;
			result.offset = offset;
			result.reflectionInfo = new ReflectionDrawableBoundary(
				reflectionInfo.Gradient,
				reflectionInfo.Alpha,
				reflectionInfo.ObjectForOverlay,
				reflectionInfo.Reflection,
				reflectionInfo.AlphaTest
			);
		}

		#endregion
	}
}