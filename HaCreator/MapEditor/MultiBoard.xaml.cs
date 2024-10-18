/* Copyright (C) 2015 haha01haha01
 * 2020 lastbattle

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

// uncomment line below to use XNA's Z-order functions
// #define UseXNAZorder

// uncomment line below to show FPS counter
// #define FPS_TEST


using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using HaCreator.Collections;
using HaCreator.GUI;
using HaCreator.MapEditor.Input;
using HaCreator.MapEditor.Instance;
using MapleLib.WzLib.WzStructure.Data;
using Color = Microsoft.Xna.Framework.Color;
using ContextMenu = System.Windows.Controls.ContextMenu;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using DragEventArgs = System.Windows.Forms.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using Mouse = HaCreator.MapEditor.Input.Mouse;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using ScrollEventArgs = System.Windows.Controls.Primitives.ScrollEventArgs;
using Size = System.Windows.Size;
using UserControl = System.Windows.Controls.UserControl;

namespace HaCreator.MapEditor {
	public partial class MultiBoard : UserControl {
		private readonly IntPtr dxHandle;
		private readonly UserObjectsManager userObjs;

		// UI
		private readonly List<Board> boards = new();
		private Board selectedBoard;
		private HaCreatorStateManager _HaCreatorStateManager;

		private static double _scale = 1;
		public double Scale => _scale;

		public HaCreatorStateManager HaCreatorStateManager {
			get => _HaCreatorStateManager;
			set {
				if (_HaCreatorStateManager != null) {
					throw new Exception("HaCreatorStateManager already set.");
				}

				_HaCreatorStateManager = value;
			}
		}

		public WindowState CurrentHostWindowState = WindowState.Normal;

		public double ActualHeight => Device.ActualHeight;
		public double ActualWidth => Device.ActualWidth;

#if FPS_TEST
        private FPSCounter fpsCounter = new FPSCounter();
#endif


		#region Window

		/// <summary>
		/// Update the state of the host window
		/// </summary>
		/// <param name="CurrentHostWindowState"></param>
		public void UpdateWindowState(WindowState CurrentHostWindowState) {
			this.CurrentHostWindowState = CurrentHostWindowState;
		}

		public void UpdateWindowSize(Size CurrentWindowSize) {
			//_CurrentDXWindowSize = DxContainer.ClientSize;

			Device.OnSizeChanged();
		}

		private void MultiBoard2_SizeChanged(object sender, SizeChangedEventArgs e) {
			//_CurrentDXWindowSize = DxContainer.ClientSize;
		}

		#endregion

		#region Initialization

		public MultiBoard() {
			InitializeComponent();

			if (!DesignerProperties.GetIsInDesignMode(this)) { // stupid errors popping up in design mode 
				winFormDXHolder.Visibility = Visibility.Visible;
			}

			Device.MultiBoard = this;

			userObjs = new UserObjectsManager(this);
			SizeChanged += MultiBoard2_SizeChanged;
		}

		public void Start() {
			if (Device.DeviceReady) {
				return;
			}

			Device.Start();
			//if (selectedBoard == null) 
			//    throw new Exception("Cannot start without a selected board");
			Visibility = Visibility.Visible;

			AdjustScrollBars();
		}

		public void Stop() {
			//
		}

		#endregion

		#region Methods

		public void OnMinimapStateChanged(Board board, bool hasMm) {
			if (MinimapStateChanged != null) {
				MinimapStateChanged.Invoke(board, hasMm);
			}
		}

		public void OnBoardRemoved(Board board) {
			if (BoardRemoved != null) {
				BoardRemoved.Invoke(board, null);
			}
		}

		/// <summary>
		/// Creates a new board object
		/// </summary>
		/// <param name="mapSize"></param>
		/// <param name="centerPoint"></param>
		/// <param name="menu"></param>
		/// <param name="bIsNewMapDesign">Determines if this board is a new map design or editing an existing map.</param>
		/// <returns></returns>
		public Board CreateBoard(Point mapSize, Point centerPoint, ContextMenu menu, bool bIsNewMapDesign) {
			lock (this) {
				var newBoard = new Board(mapSize, centerPoint, this, bIsNewMapDesign, menu, ApplicationSettings.theoreticalVisibleTypes,
					ApplicationSettings.theoreticalEditedTypes);
				boards.Add(newBoard);
				newBoard.InitMapLayers();
				return newBoard;
			}
		}

		public bool IsItemInRange(int x, int y, int w, int h, int xshift, int yshift) {
			return x + xshift + w > 0 && y + yshift + h > 0 && x + xshift < Device.ActualWidth / _scale &&
			       y + yshift < Device.ActualHeight / _scale;
		}

		#endregion

		#region Properties

		public double MaxHScroll => hScrollBar.Maximum;

		public double MaxVScroll => vScrollBar.Maximum;

		public List<Board> Boards => boards;

		public Board? SelectedBoard {
			get => selectedBoard;
			set {
				lock (this) {
					selectedBoard = value;
					if (value != null) {
						AdjustScrollBars();
					}
				}
			}
		}

		public Point MapSize {
			get {
				if (selectedBoard == null) {
					return new Point(0, 0);
				}

				return selectedBoard.MapSize;
			}
		}

		public UserObjectsManager UserObjects => userObjs;

		public bool AssertLayerSelected() {
			if (SelectedBoard.SelectedLayerIndex == -1) {
				MessageBox.Show("Select a real layer", "Error", MessageBoxButton.OK,
					MessageBoxImage.Error);
				return false;
			}

			return true;
		}

		#endregion

		#region Human I\O Handling

		private BoardItem GetHighestBoardItem(List<BoardItem> items) {
			if (items.Count < 1) {
				return null;
			}

			var highestZ = -1;
			BoardItem highestItem = null;
			int zSum;
			foreach (var item in items) {
				zSum = item is LayeredItem ? ((LayeredItem) item).Layer.LayerNumber * 100 + item.Z : 900 + item.Z;
				if (zSum > highestZ) {
					highestZ = zSum;
					highestItem = item;
				}
			}

			return highestItem;
		}

		public static int PhysicalToVirtual(int location, int center, int scroll, int origin) {
			return location - center + scroll + origin;
		}

		public static int VirtualToPhysical(int location, int center, int scroll, int origin) {
			return location + center - scroll - origin;
		}

		public static bool IsItemUnderRectangle(BoardItem item, Rectangle rect) {
			return item.Right > rect.Left && item.Left < rect.Right && item.Bottom > rect.Top &&
			       item.Top < rect.Bottom;
		}

		public static bool IsItemInsideRectangle(BoardItem item, Rectangle rect) {
			return item.Left > rect.Left && item.Right < rect.Right && item.Top > rect.Top &&
			       item.Bottom < rect.Bottom;
		}

		private void GetObjsUnderPointFromList(IMapleList list, Point locationVirtualPos, ref BoardItem itemUnderPoint,
			ref BoardItem selectedUnderPoint, ref bool selectedItemHigher) {
			if (!list.IsItem) {
				return;
			}

			var sel = selectedBoard.GetUserSelectionInfo();
			if (list.ListType == ItemTypes.None) {
				for (var i = 0; i < list.Count; i++) {
					var item = (BoardItem) list[i];
					if ((selectedBoard.EditedTypes & item.Type) != item.Type) {
						continue;
					}

					if (IsPointInsideRectangle(locationVirtualPos, item.Left, item.Top, item.Right, item.Bottom)
					    && !(item is Mouse)
					    && item.CheckIfLayerSelected(sel)
					    && !item.IsPixelTransparent(locationVirtualPos.X - item.Left,
						    locationVirtualPos.Y - item.Top)) {
						if (item.Selected) {
							selectedUnderPoint = item;
							selectedItemHigher = true;
						} else {
							itemUnderPoint = item;
							selectedItemHigher = false;
						}
					}
				}
			} else if ((selectedBoard.EditedTypes & list.ListType) == list.ListType) {
				for (var i = 0; i < list.Count; i++) {
					var item = (BoardItem) list[i];
					if (IsPointInsideRectangle(locationVirtualPos, item.Left, item.Top, item.Right, item.Bottom)
					    && !(item is Mouse)
					    && item.CheckIfLayerSelected(sel)
					    && !item.IsPixelTransparent(locationVirtualPos.X - item.Left,
						    locationVirtualPos.Y - item.Top)) {
						if (item.Selected) {
							selectedUnderPoint = item;
							selectedItemHigher = true;
						} else {
							itemUnderPoint = item;
							selectedItemHigher = false;
						}
					}
				}
			}
		}

		private BoardItemPair GetObjectsUnderPoint(Point location, out bool selectedItemHigher) {
			selectedItemHigher = false; //to stop VS from bitching
			BoardItem itemUnderPoint = null, selectedUnderPoint = null;
			var locationVirtualPos =
				new Point(
					PhysicalToVirtual(location.X, selectedBoard.CenterPoint.X, selectedBoard.hScroll, /*item.Origin.X*/
						0),
					PhysicalToVirtual(location.Y, selectedBoard.CenterPoint.Y, selectedBoard.vScroll, /*item.Origin.Y*/
						0));
			for (var i = 0; i < selectedBoard.BoardItems.AllItemLists.Length; i++) {
				GetObjsUnderPointFromList(selectedBoard.BoardItems.AllItemLists[i], locationVirtualPos,
					ref itemUnderPoint, ref selectedUnderPoint, ref selectedItemHigher);
			}

			return new BoardItemPair(itemUnderPoint, selectedUnderPoint);
		}

		private BoardItemPair GetObjectsUnderPoint(Point location) {
			bool foo;
			return GetObjectsUnderPoint(location, out foo);
		}

		private BoardItem GetObjectUnderPoint(Point location) {
			bool selectedItemHigher;
			var objsUnderPoint = GetObjectsUnderPoint(location, out selectedItemHigher);
			if (objsUnderPoint.SelectedItem == null && objsUnderPoint.NonSelectedItem == null) {
				return null;
			}

			if (objsUnderPoint.SelectedItem == null) {
				return objsUnderPoint.NonSelectedItem;
			}

			if (objsUnderPoint.NonSelectedItem == null) {
				return objsUnderPoint.SelectedItem;
			}

			return selectedItemHigher ? objsUnderPoint.SelectedItem : objsUnderPoint.NonSelectedItem;
		}

		public static bool IsPointInsideRectangle(Point point, int left, int top, int right, int bottom) {
			if (bottom > point.Y && top < point.Y && left < point.X && right > point.X) {
				return true;
			}

			return false;
		}

		public void OnExportRequested() {
			if (ExportRequested != null) {
				ExportRequested.Invoke();
			}
		}

		public void OnLoadRequested() {
			if (LoadRequested != null) {
				LoadRequested.Invoke();
			}
		}

		public void OnCloseTabRequested() {
			if (CloseTabRequested != null) {
				CloseTabRequested.Invoke();
			}
		}

		public void OnSwitchTabRequested(bool reverse) {
			if (SwitchTabRequested != null) {
				SwitchTabRequested.Invoke(this, reverse);
			}
		}

		public delegate void LeftMouseDownDelegate(Board selectedBoard, BoardItem item, BoardItem selectedItem,
			Point realPosition, Point virtualPosition, bool selectedItemHigher);

		public event LeftMouseDownDelegate LeftMouseDown;

		public delegate void LeftMouseUpDelegate(Board selectedBoard, BoardItem item, BoardItem selectedItem,
			Point realPosition, Point virtualPosition, bool selectedItemHigher);

		public event LeftMouseUpDelegate LeftMouseUp;

		public delegate void RightMouseClickDelegate(Board selectedBoard, BoardItem target, Point realPosition,
			Point virtualPosition, MouseState mouseState);

		public event RightMouseClickDelegate RightMouseClick;

		public delegate void MouseDoubleClickDelegate(Board selectedBoard, BoardItem target, Point realPosition,
			Point virtualPosition);

		public new event MouseDoubleClickDelegate MouseDoubleClick; //"new" is to make VS shut up with it's warnings

		public delegate void ShortcutKeyPressedDelegate(Board selectedBoard, bool ctrl, bool shift, bool alt,
			Key key);

		public event ShortcutKeyPressedDelegate ShortcutKeyPressed;

		public delegate void MouseMovedDelegate(Board selectedBoard, Point oldPos, Point newPos, Point currPhysicalPos);

		public event MouseMovedDelegate MouseMoved;

		public delegate void ImageDroppedDelegate(Board selectedBoard, Bitmap bmp, string name,
			Point pos);

		public event ImageDroppedDelegate ImageDropped;

		public event HaRibbon.EmptyEvent ExportRequested;
		public event HaRibbon.EmptyEvent LoadRequested;
		public event HaRibbon.EmptyEvent CloseTabRequested;
		public event EventHandler<bool> SwitchTabRequested;
		public event HaRibbon.EmptyEvent BackupCheck;
		
		/// <summary>
		/// Mouse click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Device_MouseClick(object sender, MouseEventArgs e) {
			// We only handle right click here because left click is handled more thoroughly by up-down handlers
			var x = (int) (e.X / _scale);
			var y = (int) (e.Y / _scale);
			if (e.Button == MouseButtons.Right && RightMouseClick != null) {
				var realPosition = new Point(e.X, e.Y);
				lock (this) {
					RightMouseClick(
						selectedBoard,
						GetObjectUnderPoint(new Point(x, y)),
						realPosition,
						new Point(
							PhysicalToVirtual(x, selectedBoard.CenterPoint.X, selectedBoard.hScroll, 0),
							PhysicalToVirtual(y, selectedBoard.CenterPoint.Y, selectedBoard.vScroll, 0)),
						selectedBoard.Mouse.State);
				}
			}
		}

		/// <summary>
		/// Mouse double click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Device_MouseDoubleClick(object sender, MouseEventArgs e) {
			var x = (int) (e.X / _scale);
			var y = (int) (e.Y / _scale);
			if (e.Button == MouseButtons.Left && MouseDoubleClick != null) {
				var realPosition = new Point(e.X, e.Y);
				lock (this) {
					MouseDoubleClick(selectedBoard, GetObjectUnderPoint(new Point(x, y)), realPosition,
						new Point(PhysicalToVirtual(x, selectedBoard.CenterPoint.X, selectedBoard.hScroll, 0),
							PhysicalToVirtual(y, selectedBoard.CenterPoint.Y, selectedBoard.vScroll, 0)));
				}
			}
		}

		public void Device_OnMouseDoubleClick(object sender, System.Windows.Input.MouseEventArgs e) {
			var position = e.GetPosition(Device as IInputElement);
			var x = (int) (position.X / _scale);
			var y = (int) (position.Y / _scale);
			if (e.LeftButton == MouseButtonState.Released && MouseDoubleClick != null) {
				var realPosition = new Point((int) position.X, (int) position.Y);
				lock (this) {
					MouseDoubleClick(selectedBoard, GetObjectUnderPoint(new Point(x, y)), realPosition,
						new Point(PhysicalToVirtual(x, selectedBoard.CenterPoint.X, selectedBoard.hScroll, 0),
							PhysicalToVirtual(y, selectedBoard.CenterPoint.Y, selectedBoard.vScroll, 0)));
				}
			}
		}
		
		/// <summary>
		/// Mouse wheel
		/// Wheelup = positive, Wheeldown = negative
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Device_MouseWheel(object sender, MouseEventArgs e) {
			MouseWheel(e.Delta);
		}

		public void Device_OnMouseWheel(object sender, MouseWheelEventArgs e) {
			MouseWheel(e.Delta);
		}

		private void MouseWheel(int rotationDelta) {
			if (Keyboard.Modifiers == ModifierKeys.Control) {
				if (rotationDelta < 0) {
					_scale -= 0.1;
				} else if (rotationDelta > 0) {
					_scale += 0.1;
				}

				AdjustScrollBars();
			} else {
				// wheel up = positive, wheel down = negative
				if (!AddHScrollbarValue(rotationDelta)) {
					//AddVScrollbarValue((int)rotationDelta); // scroll v scroll bar instead if its not possible
				}
			}
		}
		
		/// <summary>
		/// Mouse down
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Device_MouseDown(object sender, MouseEventArgs e) {
			if (selectedBoard == null) {
				return;
			}

			var x = (int) (e.X / _scale);
			var y = (int) (e.Y / _scale);

			// If the mouse has not been moved while we were in focus (e.g. when clicking on the editor while another window focused), this event will be sent without a mousemove event preceding it.
			// We will move it to its correct position by invoking the move event handler manually.
			if (selectedBoard.Mouse.X != x || selectedBoard.Mouse.Y != y) {
				// No need to lock because MouseMove locks anyway
				Device_MouseMove(sender, e);
			}

			selectedBoard.Mouse.IsDown = true;
			if (e.Button == MouseButtons.Middle) {
				selectedBoard.Mouse.CameraPanning = true;
				selectedBoard.Mouse.CameraPanningStart = new Point(e.X + selectedBoard.hScroll, e.Y + selectedBoard.vScroll);
			} else if (e.Button == MouseButtons.Left && LeftMouseDown != null) {
				var realPosition = new Point(e.X, e.Y);
				lock (this) {
					var objsUnderMouse = GetObjectsUnderPoint(new Point(x, y), out var selectedItemHigher);
					LeftMouseDown(selectedBoard, objsUnderMouse.NonSelectedItem, objsUnderMouse.SelectedItem,
						realPosition,
						new Point(PhysicalToVirtual(x, selectedBoard.CenterPoint.X, selectedBoard.hScroll, 0),
							PhysicalToVirtual(y, selectedBoard.CenterPoint.Y, selectedBoard.vScroll, 0)),
						selectedItemHigher);
				}
			}
		}

		public void Device_OnMouseDown(object sender, MouseButtonEventArgs e) {
			if (selectedBoard == null) {
				return;
			}

			var position = e.GetPosition(Device as IInputElement);

			var x = (int) (position.X / _scale);
			var y = (int) (position.Y / _scale);

			// If the mouse has not been moved while we were in focus (e.g. when clicking on the editor while another window focused), this event will be sent without a mousemove event preceding it.
			// We will move it to its correct position by invoking the move event handler manually.
			if (selectedBoard.Mouse.X != x || selectedBoard.Mouse.Y != y) {
				// No need to lock because MouseMove locks anyway
				Device_OnMouseMove(sender, e);
			}

			selectedBoard.Mouse.IsDown = true;
			if (e.ChangedButton == MouseButton.Middle) {
				selectedBoard.Mouse.CameraPanning = true;
				selectedBoard.Mouse.CameraPanningStart = new Point((int) (position.X + selectedBoard.hScroll), (int) (position.Y + selectedBoard.vScroll));
			} else if (e.ChangedButton == MouseButton.Left && LeftMouseDown != null) {
				var realPosition = new Point((int) position.X, (int) position.Y);
				lock (this) {
					var objsUnderMouse = GetObjectsUnderPoint(new Point(x, y), out var selectedItemHigher);
					LeftMouseDown(selectedBoard, objsUnderMouse.NonSelectedItem, objsUnderMouse.SelectedItem,
						realPosition,
						new Point(PhysicalToVirtual(x, selectedBoard.CenterPoint.X, selectedBoard.hScroll, 0),
							PhysicalToVirtual(y, selectedBoard.CenterPoint.Y, selectedBoard.vScroll, 0)),
						selectedItemHigher);
				}
			}
		}

		/// <summary>
		/// Mouse up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Device_MouseUp(object sender, MouseEventArgs e) {
			if (selectedBoard == null) {
				return;
			}

			var x = (int) (e.X / _scale);
			var y = (int) (e.Y / _scale);
			selectedBoard.Mouse.IsDown = false;
			if (e.Button == MouseButtons.Middle) {
				selectedBoard.Mouse.CameraPanning = false;
			} else if (e.Button == MouseButtons.Left && LeftMouseUp != null) {
				var realPosition = new Point(e.X, e.Y);
				lock (this) {
					var objsUnderMouse = GetObjectsUnderPoint(new Point(x, y), out var selectedItemHigher);
					LeftMouseUp(selectedBoard, objsUnderMouse.NonSelectedItem, objsUnderMouse.SelectedItem,
						realPosition,
						new Point(PhysicalToVirtual(x, selectedBoard.CenterPoint.X, selectedBoard.hScroll, 0),
							PhysicalToVirtual(y, selectedBoard.CenterPoint.Y, selectedBoard.vScroll, 0)),
						selectedItemHigher);
				}
			}
		}

		public void Device_OnMouseUp(object sender, MouseButtonEventArgs e) {
			if (selectedBoard == null) {
				return;
			}

			if (e.ClickCount == 2) {
				Device_OnMouseDoubleClick(sender, e);
				return;
			}

			var position = e.GetPosition(Device as IInputElement);

			var x = (int) (position.X / _scale);
			var y = (int) (position.Y / _scale);

			if (e.ChangedButton == MouseButton.Right && RightMouseClick != null) {
				var realPosition = new Point((int) position.X, (int) position.Y);
				lock (this) {
					RightMouseClick(
						selectedBoard,
						GetObjectUnderPoint(new Point(x, y)),
						realPosition,
						new Point(
							PhysicalToVirtual(x, selectedBoard.CenterPoint.X, selectedBoard.hScroll, 0),
							PhysicalToVirtual(y, selectedBoard.CenterPoint.Y, selectedBoard.vScroll, 0)),
						selectedBoard.Mouse.State);
				}
			}

			selectedBoard.Mouse.IsDown = false;
			if (e.ChangedButton == MouseButton.Middle) {
				selectedBoard.Mouse.CameraPanning = false;
			} else if (e.ChangedButton == MouseButton.Left && LeftMouseUp != null) {
				var realPosition = new Point((int) position.X, (int) position.Y);
				lock (this) {
					var objsUnderMouse = GetObjectsUnderPoint(new Point(x, y), out var selectedItemHigher);
					LeftMouseUp(selectedBoard, objsUnderMouse.NonSelectedItem, objsUnderMouse.SelectedItem,
						realPosition,
						new Point(PhysicalToVirtual(x, selectedBoard.CenterPoint.X, selectedBoard.hScroll, 0),
							PhysicalToVirtual(y, selectedBoard.CenterPoint.Y, selectedBoard.vScroll, 0)),
						selectedItemHigher);
				}
			}
		}

		public void Device_PreviewKeyDownEventArgs(object? sender, PreviewKeyDownEventArgs e) {
			switch (e.KeyCode) {
				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
					e.IsInputKey = true;
					break;
			}
		}

		public void Device_KeyDownEventArgs(object? sender, System.Windows.Forms.KeyEventArgs e) {
			if (selectedBoard == null) {
				return;
			}

			lock (this) {
				if (ShortcutKeyPressed == null) {
					return;
				}

				var ctrl = (Control.ModifierKeys & Keys.Control) ==
				           Keys.Control;
				var alt = (Control.ModifierKeys & Keys.Alt) ==
				          Keys.Alt;
				var shift = (Control.ModifierKeys & Keys.Shift) ==
				            Keys.Shift;
				var filteredKeys = e.KeyData;

				if (ctrl && (filteredKeys & Keys.Control) != 0) {
					filteredKeys = filteredKeys ^ Keys.Control;
				}

				if (alt && (filteredKeys & Keys.Alt) != 0) {
					filteredKeys = filteredKeys ^ Keys.Alt;
				}

				if (shift && (filteredKeys & Keys.Shift) != 0) {
					filteredKeys = filteredKeys ^ Keys.Shift;
				}

				lock (this) {
					ShortcutKeyPressed(selectedBoard, ctrl, shift, alt, KeyInterop.KeyFromVirtualKey((int) filteredKeys));
				}
			}
		}

		/// <summary>
		/// Key down
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Device_OnKeyDown(object? sender, KeyEventArgs e) {
			if (selectedBoard == null) {
				return;
			}

			lock (this) {
				if (ShortcutKeyPressed == null) {
					return;
				}

				var ctrl = (Keyboard.Modifiers & ModifierKeys.Control) ==
				           ModifierKeys.Control;
				var alt = (Keyboard.Modifiers & ModifierKeys.Alt) ==
				          ModifierKeys.Alt;
				var shift = (Keyboard.Modifiers & ModifierKeys.Shift) ==
				            ModifierKeys.Shift;

				lock (this) {
					ShortcutKeyPressed(selectedBoard, ctrl, shift, alt, e.Key);
				}

				e.Handled = true;
			}
		}

		/// <summary>
		/// Mouse move
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Device_MouseMove(object sender, MouseEventArgs e) {
			if (selectedBoard == null) {
				return;
			}

			MouseMove(e.X, e.Y);
		}

		public void Device_OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
			if (selectedBoard == null) {
				return;
			}

			var position = e.GetPosition(Device as IInputElement);

			MouseMove(position.X, position.Y);
		}

		private void MouseMove(double posX, double posY) {
			var x = (int) (posX / _scale);
			var y = (int) (posY / _scale);
			lock (this) {
				if (VirtualToPhysical(selectedBoard.Mouse.X, selectedBoard.CenterPoint.X, selectedBoard.hScroll, 0) !=
				    x
				    || VirtualToPhysical(selectedBoard.Mouse.Y, selectedBoard.CenterPoint.Y, selectedBoard.vScroll,
					    0) != y) {
					var oldPos = new Point(selectedBoard.Mouse.X, selectedBoard.Mouse.Y);
					var newPos =
						new Point(
							PhysicalToVirtual(x, selectedBoard.CenterPoint.X, selectedBoard.hScroll, 0),
							PhysicalToVirtual(y, selectedBoard.CenterPoint.Y, selectedBoard.vScroll, 0));
					selectedBoard.Mouse.Move(newPos.X, newPos.Y);

					if (MouseMoved != null) {
						MouseMoved.Invoke(selectedBoard, oldPos, newPos, new Point((int) posX, (int) posY));
					}
				}
			}
		}

		/// <summary>
		/// Drag enter
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Device_DragEnter(object sender, DragEventArgs e) {
			lock (this) {
				if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
					e.Effect = DragDropEffects.Copy;
				} else {
					e.Effect = DragDropEffects.None;
				}
			}
		}

		public void Device_OnDragEnter(object sender, System.Windows.DragEventArgs e) {
			lock (this) {
				if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
					e.Effects = System.Windows.DragDropEffects.Copy;
				} else {
					e.Effects = System.Windows.DragDropEffects.None;
				}
			}
		}

		/// <summary>
		/// Drag drop
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Device_DragDrop(object sender, DragEventArgs e) {
			lock (this) {
				if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

				if (!AssertLayerSelected()) {
					return;
				}

				var data = (string[]) e.Data.GetData(DataFormats.FileDrop);

				DragDrop(e.X, e.Y, data);
			}
		}

		public void Device_OnDrop(object sender, System.Windows.DragEventArgs e) {
			lock (this) {
				if (!e.Data.GetDataPresent(DataFormats.FileDrop)) {
					return;
				}

				if (!AssertLayerSelected()) {
					return;
				}

				var data = (string[]) e.Data.GetData(DataFormats.FileDrop);

				var position = e.GetPosition(Device as IInputElement);

				DragDrop(position.X, position.Y, data);
			}
		}

		private void DragDrop(double posX, double posY, string[] data) {
			var x = (int) (posX / _scale);
			var y = (int) (posY / _scale);
			// be warned when run under visual studio. it inherits VS's scaling and VS's window location
			var p = PointToScreen(new System.Windows.Point(x, y));
			foreach (var file in data) {
				Bitmap bmp;
				try {
					bmp = (Bitmap) Image.FromFile(file);
				} catch {
					continue;
				}

				if (ImageDropped != null) {
					ImageDropped.Invoke(selectedBoard, bmp, Path.GetFileNameWithoutExtension(file),
						new Point(
							PhysicalToVirtual((int) p.X, selectedBoard.CenterPoint.X, selectedBoard.hScroll, 0),
							PhysicalToVirtual((int) p.Y, selectedBoard.CenterPoint.Y, selectedBoard.vScroll, 0)));
				}
			}
		}

		#endregion

		#region Event Handlers

		private System.Windows.Point _mousePoint; // Initial point of drag

		public bool
			TriggerMouseWheel(System.Windows.Input.MouseEventArgs e,
				UIElement sender) // Were not overriding OnMouseWheel anymore because it's better to override it in mainform
		{
			lock (this) {
				var newMousePoint = e.MouseDevice.GetPosition(sender);

				var oldvalue = vScrollBar.Value;
				var delta = _mousePoint - newMousePoint;
				var scrollValue = delta.Length / 10 * vScrollBar.LargeChange;

				_mousePoint = newMousePoint;

				if (vScrollBar.Value - scrollValue < vScrollBar.Minimum) {
					vScrollBar.Value = vScrollBar.Minimum;
				} else if (vScrollBar.Value - scrollValue > vScrollBar.Maximum) {
					vScrollBar.Value = vScrollBar.Maximum;
				} else {
					vScrollBar.Value -= (int) scrollValue;
				}

				VScrollBar_Scroll(null, null);

				return true;
			}
		}


		public void AdjustScrollBars() {
			lock (this) {
				if (MapSize.X > ActualWidth) {
					hScrollBar.IsEnabled = true;
					hScrollBar.Maximum = MapSize.X - ActualWidth;
					hScrollBar.Minimum = 0;
					if (hScrollBar.Maximum < selectedBoard.hScroll) {
						hScrollBar.Value = hScrollBar.Maximum - 1;
						selectedBoard.hScroll = (int) hScrollBar.Value;
					} else {
						hScrollBar.Value = selectedBoard.hScroll;
					}
				} else {
					hScrollBar.IsEnabled = false;
					hScrollBar.Value = 0;
					hScrollBar.Maximum = 0;
				}

				if (MapSize.Y > ActualHeight) {
					vScrollBar.IsEnabled = true;
					vScrollBar.Maximum = MapSize.Y - ActualHeight;
					vScrollBar.Minimum = 0;
					if (vScrollBar.Maximum < selectedBoard.vScroll) {
						vScrollBar.Value = vScrollBar.Maximum - 1;
						selectedBoard.vScroll = (int) vScrollBar.Value;
					} else {
						vScrollBar.Value = selectedBoard.vScroll;
					}
				} else {
					vScrollBar.IsEnabled = false;
					vScrollBar.Value = 0;
					vScrollBar.Maximum = 0;
				}
			}
		}

		/// <summary>
		/// Adds the horizontal scroll bar value
		/// </summary>
		/// <param name="value"></param>
		/// <returns>True if scrolling is possible</returns>
		public bool AddHScrollbarValue(int value) {
			if (hScrollBar.Value + value == hScrollBar.Value) {
				return false;
			}

			SetHScrollbarValue((int) (hScrollBar.Value + value));

			// Update display
			HScrollBar_Scroll(null, null);
			return true;
		}

		/// <summary>
		/// Sets the horizontal scroll bar value
		/// </summary>
		/// <param name="value"></param>
		public void SetHScrollbarValue(int value) {
			lock (this) {
				hScrollBar.Value = value;
			}
		}

		/// <summary>
		/// Adds the horizontal scroll bar value
		/// </summary>
		/// <param name="value"></param>
		/// <returns>True if scrolling is possible</returns>
		public bool AddVScrollbarValue(int value) {
			if (vScrollBar.Value + value == hScrollBar.Value) {
				return false;
			}

			SetVScrollbarValue((int) (vScrollBar.Value + value));

			// Update display
			VScrollBar_Scroll(null, null);
			return true;
		}

		/// <summary>
		/// Sets the vertical scroll bar value
		/// </summary>
		/// <param name="value"></param>
		public void SetVScrollbarValue(int value) {
			lock (this) {
				vScrollBar.Value = value;
			}
		}

		/// <summary>
		/// Vertical scroll bar
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void VScrollBar_Scroll(object sender, ScrollEventArgs e) {
			lock (this) {
				selectedBoard.vScroll = (int) vScrollBar.Value;
			}
		}

		/// <summary>
		/// Horizontal scroll bar
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void HScrollBar_Scroll(object sender, ScrollEventArgs e) {
			lock (this) {
				selectedBoard.hScroll = (int) hScrollBar.Value;
			}
		}

		#endregion

		#region Events

		public delegate void UndoRedoDelegate();

		public event UndoRedoDelegate OnUndoListChanged;
		public event UndoRedoDelegate OnRedoListChanged;

		public delegate void LayerTSChangedDelegate(Layer layer);

		public event LayerTSChangedDelegate OnLayerTSChanged;

		public delegate void MenuItemClickedDelegate(BoardItem item);

		public event MenuItemClickedDelegate OnEditInstanceClicked;
		public event MenuItemClickedDelegate OnEditBaseClicked;
		public event MenuItemClickedDelegate OnSendToBackClicked;
		public event MenuItemClickedDelegate OnBringToFrontClicked;

		public delegate void ReturnToSelectionStateDelegate();

		public event ReturnToSelectionStateDelegate ReturnToSelectionState;

		public delegate void SelectedItemChangedDelegate(BoardItem selectedItem);

		public event SelectedItemChangedDelegate SelectedItemChanged;

		public event EventHandler BoardRemoved;
		public event EventHandler<bool> MinimapStateChanged;

		public void OnSelectedItemChanged(BoardItem selectedItem) {
			if (SelectedItemChanged != null) {
				SelectedItemChanged.Invoke(selectedItem);
			}
		}

		public void InvokeReturnToSelectionState() {
			if (ReturnToSelectionState != null) {
				ReturnToSelectionState.Invoke();
			}
		}

		public void SendToBackClicked(BoardItem item) {
			if (OnSendToBackClicked != null) {
				OnSendToBackClicked.Invoke(item);
			}
		}

		public void BringToFrontClicked(BoardItem item) {
			if (OnBringToFrontClicked != null) {
				OnBringToFrontClicked.Invoke(item);
			}
		}

		public void EditInstanceClicked(BoardItem item) {
			if (OnEditInstanceClicked != null) {
				OnEditInstanceClicked.Invoke(item);
			}
		}

		public void EditBaseClicked(BoardItem item) {
			if (OnEditBaseClicked != null) {
				OnEditBaseClicked.Invoke(item);
			}
		}

		public void LayerTSChanged(Layer layer) {
			if (OnLayerTSChanged != null) {
				OnLayerTSChanged.Invoke(layer);
			}
		}

		public void UndoListChanged() {
			if (OnUndoListChanged != null) {
				OnUndoListChanged.Invoke();
			}
		}

		public void RedoListChanged() {
			if (OnRedoListChanged != null) {
				OnRedoListChanged.Invoke();
			}
		}

		#endregion

		#region Static Settings

		public static float FirstSnapVerification;
		public static Color InactiveColor;
		public static Color RopeInactiveColor;
		public static Color FootholdInactiveColor;
		public static Color FootholdSideInactiveColor;
		public static Color ChairInactiveColor;
		public static Color ToolTipInactiveColor;
		public static Color MiscInactiveColor;
		public static Color VRInactiveColor;
		public static Color MinimapBoundInactiveColor;

		static MultiBoard() {
			RecalculateSettings();
		}

		public static Color CreateTransparency(Color orgColor, int alpha) {
			return new Color(orgColor.R, orgColor.B, orgColor.G, alpha);
		}

		public static void RecalculateSettings() {
			var alpha = UserSettings.NonActiveAlpha;
			FirstSnapVerification = UserSettings.SnapDistance * 20;
			InactiveColor = CreateTransparency(Color.White, alpha);
			RopeInactiveColor = CreateTransparency(UserSettings.RopeColor, alpha);
			FootholdInactiveColor = CreateTransparency(UserSettings.FootholdColor, alpha);
			FootholdSideInactiveColor = CreateTransparency(UserSettings.FootholdSideColor, alpha);
			ChairInactiveColor = CreateTransparency(UserSettings.ChairColor, alpha);
			ToolTipInactiveColor = CreateTransparency(UserSettings.ToolTipColor, alpha);
			MiscInactiveColor = CreateTransparency(UserSettings.MiscColor, alpha);
			VRInactiveColor = CreateTransparency(UserSettings.VRColor, alpha);
			MinimapBoundInactiveColor = CreateTransparency(UserSettings.MinimapBoundColor, alpha);
		}

		#endregion
	}
}