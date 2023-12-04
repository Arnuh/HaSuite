﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using XNA = Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using System.Linq;
using HaCreator.MapEditor.UndoRedo;
using HaCreator.MapEditor.Instance;
using HaCreator.MapEditor.Instance.Misc;
using HaCreator.MapEditor.Instance.Shapes;
using HaCreator.Exceptions;
using HaCreator.MapEditor.Info;

namespace HaCreator.MapEditor.Input {
	public class InputHandler {
		private MultiBoard parentBoard;
		private int lastUserInteraction = 0;
		private int lastBackup = 0;

		public void OnUserInteraction() {
			lastUserInteraction = Environment.TickCount;
			if (parentBoard != null && parentBoard.SelectedBoard != null)
				parentBoard.SelectedBoard.Dirty = true;
		}

		public void OnBackup() {
			lastBackup = Environment.TickCount;
		}

		private bool IsTickCountDiff(ref int source, int ms) {
			var diff = Environment.TickCount - source;
			if (diff < 0) {
				// This can happen on TickCount overflow
				// We will just reset the timer and return false, to prevent anything special from happening
				source = Environment.TickCount;
				return false;
			}

			return diff >= ms;
		}

		public bool IsUserIdleFor(int ms) {
			return IsTickCountDiff(ref lastUserInteraction, ms);
		}

		public bool IsBackupDelayedFor(int ms) {
			return IsTickCountDiff(ref lastBackup, ms);
		}

		public InputHandler(MultiBoard parentBoard) {
			this.parentBoard = parentBoard;

			parentBoard.LeftMouseDown += new MultiBoard.LeftMouseDownDelegate(parentBoard_LeftMouseDown);
			parentBoard.LeftMouseUp += new MultiBoard.LeftMouseUpDelegate(parentBoard_LeftMouseUp);
			parentBoard.RightMouseClick += new MultiBoard.RightMouseClickDelegate(parentBoard_RightMouseClick);
			parentBoard.MouseDoubleClick += new MultiBoard.MouseDoubleClickDelegate(parentBoard_MouseDoubleClick);
			parentBoard.ShortcutKeyPressed += new MultiBoard.ShortcutKeyPressedDelegate(ParentBoard_ShortcutKeyPressed);
			parentBoard.MouseMoved += new MultiBoard.MouseMovedDelegate(parentBoard_MouseMoved);
		}

		public static XNA.Rectangle CreateRectangle(XNA.Point a, XNA.Point b) {
			int left, right, top, bottom;
			if (a.X < b.X) {
				left = a.X;
				right = b.X;
			} else {
				left = b.X;
				right = a.X;
			}

			if (a.Y < b.Y) {
				top = a.Y;
				bottom = b.Y;
			} else {
				top = b.Y;
				bottom = a.Y;
			}

			return new XNA.Rectangle(left, top, right - left, bottom - top);
		}

		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(Keys vKey);

		public static bool IsKeyPushedDown(Keys vKey) {
			return 0 != (GetAsyncKeyState(vKey) & 0x8000);
		}

		public static double Distance(double x, double y) {
			return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
		}

		private void parentBoard_MouseMoved(Board selectedBoard, XNA.Point oldPos, XNA.Point newPos,
			XNA.Point currPhysicalPos) {
			lock (parentBoard) {
				OnUserInteraction();
				if (selectedBoard.Mouse.CameraPanning) {
					selectedBoard.hScroll = selectedBoard.Mouse.CameraPanningStart.X - currPhysicalPos.X;
					selectedBoard.vScroll = selectedBoard.Mouse.CameraPanningStart.Y - currPhysicalPos.Y;
				} else if (selectedBoard.Mouse.MinimapBrowseOngoing && selectedBoard.Mouse.State == MouseState.Selection) {
					HandleMinimapBrowse(selectedBoard, currPhysicalPos);
				} else if (selectedBoard.Mouse.MultiSelectOngoing &&
				           (Math.Abs(selectedBoard.Mouse.X - selectedBoard.Mouse.MultiSelectStart.X) > 1 ||
				            Math.Abs(selectedBoard.Mouse.Y - selectedBoard.Mouse.MultiSelectStart.Y) > 1)) {
					var oldRect = CreateRectangle(oldPos, selectedBoard.Mouse.MultiSelectStart);
					var newRect = CreateRectangle(newPos, selectedBoard.Mouse.MultiSelectStart);
					var toRemove = new List<BoardItem>();
					var sel = selectedBoard.GetUserSelectionInfo();
					foreach (var item in selectedBoard.BoardItems.Items)
						if (MultiBoard.IsItemUnderRectangle(item, newRect) &&
						    (sel.editedTypes & item.Type) == item.Type && item.CheckIfLayerSelected(sel))
							item.Selected = true;
						else if (item.Selected && MultiBoard.IsItemUnderRectangle(item, oldRect))
							toRemove.Add(item);

					foreach (var item in toRemove)
						item.Selected = false;
					toRemove.Clear();
				} else if (selectedBoard.Mouse.SingleSelectStarting &&
				           (Distance(newPos.X - selectedBoard.Mouse.SingleSelectStart.X,
					            newPos.Y - selectedBoard.Mouse.SingleSelectStart.Y) > UserSettings.SignificantDistance ||
				            IsKeyPushedDown(Keys.Menu))) {
					BindAllSelectedItems(selectedBoard, selectedBoard.Mouse.SingleSelectStart);
					selectedBoard.Mouse.SingleSelectStarting = false;
				} else if (selectedBoard.Mouse.BoundItems.Count > 0) {
					//snapping
					if (UserSettings.useSnapping && selectedBoard.Mouse.BoundItems.Count != 0 &&
					    !IsKeyPushedDown(Keys.Menu)) {
						var state = selectedBoard.Mouse.State;
						if (state == MouseState.Selection || state == MouseState.StaticObjectAdding ||
						    state == MouseState.RandomTiles || state == MouseState.Ropes ||
						    state == MouseState.Footholds || state == MouseState.Chairs) {
							var items = selectedBoard.Mouse.BoundItems.Keys.ToList();
							foreach (var item in items)
								if (item is ISnappable)
									((ISnappable) item).DoSnap();
						}
					}
				} else if (selectedBoard.Mouse.State == MouseState.Footholds) {
					// Foothold snap-like behavior
					selectedBoard.Mouse.DoSnap();
				}

				if ((selectedBoard.Mouse.BoundItems.Count > 0 || selectedBoard.Mouse.MultiSelectOngoing) &&
				    selectedBoard.Mouse.State == MouseState.Selection) {
					// auto scrolling
					// Bind physicalpos to our dxcontainer, to prevent extremely fast scrolling
					currPhysicalPos = new XNA.Point(Math.Min(Math.Max(currPhysicalPos.X, 0), (int) parentBoard.Width),
						Math.Min(Math.Max(currPhysicalPos.Y, 0), (int) parentBoard.Height));

					if (currPhysicalPos.X - UserSettings.ScrollDistance < 0 && oldPos.X > newPos.X) //move to left
						selectedBoard.hScroll = (int) Math.Max(0,
							selectedBoard.hScroll -
							Math.Pow(UserSettings.ScrollBase,
								(UserSettings.ScrollDistance - currPhysicalPos.X) * UserSettings.ScrollExponentFactor) *
							UserSettings.ScrollFactor);
					else if (currPhysicalPos.X + UserSettings.ScrollDistance > parentBoard.Width &&
					         oldPos.X < newPos.X) //move to right
						selectedBoard.hScroll = (int) Math.Min(
							selectedBoard.hScroll +
							Math.Pow(UserSettings.ScrollBase,
								(currPhysicalPos.X - parentBoard.Width + UserSettings.ScrollDistance) *
								UserSettings.ScrollExponentFactor) * UserSettings.ScrollFactor, parentBoard.MaxHScroll);
					if (currPhysicalPos.Y - UserSettings.ScrollDistance < 0 && oldPos.Y > newPos.Y) //move to top
						selectedBoard.vScroll = (int) Math.Max(0,
							selectedBoard.vScroll -
							Math.Pow(UserSettings.ScrollBase,
								(UserSettings.ScrollDistance - currPhysicalPos.Y) * UserSettings.ScrollExponentFactor) *
							UserSettings.ScrollFactor);
					else if (currPhysicalPos.Y + UserSettings.ScrollDistance > parentBoard.Height &&
					         oldPos.Y < newPos.Y) //move to bottom
						selectedBoard.vScroll = (int) Math.Min(
							selectedBoard.vScroll +
							Math.Pow(UserSettings.ScrollBase,
								(currPhysicalPos.Y - parentBoard.Height + UserSettings.ScrollDistance) *
								UserSettings.ScrollExponentFactor) * UserSettings.ScrollFactor, parentBoard.MaxVScroll);
				}
			}
		}

		private UndoRedoAction CreateItemUndoMoveAction(BoardItem item, XNA.Point posChange) {
			if (item is BackgroundInstance)
				return UndoRedoManager.BackgroundMoved((BackgroundInstance) item,
					new XNA.Point(((BackgroundInstance) item).BaseX + posChange.X,
						((BackgroundInstance) item).BaseY + posChange.Y),
					new XNA.Point(((BackgroundInstance) item).BaseX, ((BackgroundInstance) item).BaseY));
			else
				return UndoRedoManager.ItemMoved(item, new XNA.Point(item.X + posChange.X, item.Y + posChange.Y),
					new XNA.Point(item.X, item.Y));
		}

		/// <summary>
		/// Keyboard navigation on the MultiBoard
		/// </summary>
		/// <param name="selectedBoard"></param>
		/// <param name="ctrl"></param>
		/// <param name="shift"></param>
		/// <param name="alt"></param>
		/// <param name="key"></param>
		private void ParentBoard_ShortcutKeyPressed(Board selectedBoard, bool ctrl, bool shift, bool alt, Keys key) {
			lock (parentBoard) {
				if (parentBoard == null || parentBoard.SelectedBoard == null)
					return;
				OnUserInteraction();
				var actions = new List<UndoRedoAction>();
				if (key == Keys.ControlKey || key == Keys.ShiftKey || key == Keys.Menu /*ALT key*/)
					return;
				var clearRedo = true;

				const int navigationSHVScrollSpeed = 16;

				switch (key) {
					case Keys.Left: {
						if (selectedBoard.SelectedItems.Count > 0) {
							foreach (var item in selectedBoard.SelectedItems)
								if (!item.BoundToSelectedItem(selectedBoard)) {
									item.X--;
									actions.Add(CreateItemUndoMoveAction(item, new XNA.Point(1, 0)));
								}
						} else // if no item is being selected, shift the view instead
						{
							selectedBoard.ParentControl.AddHScrollbarValue(-navigationSHVScrollSpeed);
						}

						break;
					}
					case Keys.Right: {
						if (selectedBoard.SelectedItems.Count > 0) {
							foreach (var item in selectedBoard.SelectedItems)
								if (!item.BoundToSelectedItem(selectedBoard)) {
									item.X++;
									actions.Add(CreateItemUndoMoveAction(item, new XNA.Point(-1, 0)));
								}
						} else // if no item is being selected, shift the view instead
						{
							selectedBoard.ParentControl.AddHScrollbarValue(navigationSHVScrollSpeed);
						}

						break;
					}
					case Keys.Up: {
						if (selectedBoard.SelectedItems.Count > 0) {
							foreach (var item in selectedBoard.SelectedItems)
								if (!item.BoundToSelectedItem(selectedBoard)) {
									item.Y--;
									actions.Add(CreateItemUndoMoveAction(item, new XNA.Point(0, 1)));
								}
						} else // if no item is being selected, shift the view instead
						{
							selectedBoard.ParentControl.AddVScrollbarValue(-navigationSHVScrollSpeed);
						}

						break;
					}
					case Keys.Down: {
						if (selectedBoard.SelectedItems.Count > 0) {
							foreach (var item in selectedBoard.SelectedItems)
								if (!item.BoundToSelectedItem(selectedBoard)) {
									item.Y++;
									actions.Add(CreateItemUndoMoveAction(item, new XNA.Point(0, -1)));
								}
						} else // if no item is being selected, shift the view instead
						{
							selectedBoard.ParentControl.AddVScrollbarValue(navigationSHVScrollSpeed);
						}

						break;
					}

					case Keys.PageUp: {
						selectedBoard.ParentControl.AddVScrollbarValue(-999);
						break;
					}
					case Keys.PageDown: {
						selectedBoard.ParentControl.AddVScrollbarValue(999);
						break;
					}

					case Keys.Delete:
						switch (selectedBoard.Mouse.State) {
							case MouseState.Selection:
								bool askedVr = false, askedMm = false;
								var
									selectedItems = selectedBoard.SelectedItems.ToList(); // Dupe the selection list
								foreach (var item in selectedItems)
									if (item is ToolTipDot || item is MiscDot) {
										continue;
									} else if (item is VRDot) {
										if (!askedVr) {
											askedVr = true;
											if (MessageBox.Show(
												    "This will remove the map's VR. This is not undoable, you must re-add VR from the map's main menu. Continue?",
												    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) ==
											    DialogResult.Yes)
												selectedBoard.VRRectangle.RemoveItem(null);
										}
									} else if (item is MinimapDot) {
										if (!askedMm) {
											askedMm = true;
											if (MessageBox.Show(
												    "This will remove the map's minimap. This is not undoable, you must re-add the minimap from the map's main menu. Continue?",
												    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) ==
											    DialogResult.Yes)
												selectedBoard.MinimapRectangle.RemoveItem(null);
										}
									} else {
										item.RemoveItem(actions);
									}

								break;
							case MouseState.RandomTiles:
							case MouseState.StaticObjectAdding:
							case MouseState.Chairs:
							case MouseState.Ropes:
								parentBoard.InvokeReturnToSelectionState();
								break;
							case MouseState.Footholds:
								while (selectedBoard.Mouse.connectedLines.Count > 0 &&
								       selectedBoard.Mouse.connectedLines[0].FirstDot.connectedLines.Count > 0)
									selectedBoard.Mouse.connectedLines[0].FirstDot.connectedLines[0]
										.Remove(false, actions);
								break;
						}

						break;
					case Keys.F:
						if (ctrl) {
							var anchors = new List<FootholdAnchor>();
							foreach (var item in selectedBoard.SelectedItems) {
								if (item is IFlippable flippable) {
									flippable.Flip = !flippable.Flip;
									actions.Add(UndoRedoManager.ItemFlipped(flippable));
								} else if (item is FootholdAnchor anchor) {
									anchors.Add(anchor);
								}
							}

							var anchorsToIgnore = new List<FootholdLine>();
							foreach (var anchor1 in anchors) {
								foreach (var anchor2 in anchors) {
									if (anchor1 == anchor2) continue;
									var line = anchor1.GetLineWith(anchor2);
									if (line == null) continue;
									if (anchorsToIgnore.Contains(line)) continue;
									line.Flip();
									anchorsToIgnore.Add(line);
									actions.Add(UndoRedoManager.FootholdFlipped(line));
									break;
								}
							}

							anchors.Clear();
						}

						break;
					case Keys.Add:
						foreach (var item in selectedBoard.SelectedItems) {
							item.Z += UserSettings.zShift;
							actions.Add(UndoRedoManager.ItemZChanged(item, item.Z - UserSettings.zShift, item.Z));
						}

						selectedBoard.BoardItems.Sort();
						break;
					case Keys.Subtract:
						foreach (var item in selectedBoard.SelectedItems) {
							item.Z -= UserSettings.zShift;
							actions.Add(UndoRedoManager.ItemZChanged(item, item.Z + UserSettings.zShift, item.Z));
						}

						selectedBoard.BoardItems.Sort();
						break;
					case Keys.A:
						if (ctrl)
							foreach (var item in selectedBoard.BoardItems.Items)
								if ((selectedBoard.EditedTypes & item.Type) == item.Type) {
									if (item is LayeredItem) {
										var li = (LayeredItem) item;
										if (li.CheckIfLayerSelected(selectedBoard.GetUserSelectionInfo()))
											item.Selected = true;
									} else {
										item.Selected = true;
									}
								}

						clearRedo = false;
						break;
					case Keys.X: // Cut
						if (ctrl && selectedBoard.Mouse.State == MouseState.Selection) {
							Clipboard.SetData(SerializationManager.HaClipboardData,
								selectedBoard.SerializationManager.SerializeList(selectedBoard.SelectedItems
									.Cast<ISerializableSelector>()));
							var selectedItemIndex = 0;
							while (selectedBoard.SelectedItems.Count > selectedItemIndex) {
								var item = selectedBoard.SelectedItems[selectedItemIndex];
								if (item is ToolTipDot || item is MiscDot || item is VRDot || item is MinimapDot)
									selectedItemIndex++;
								else
									item.RemoveItem(actions);
							}

							break;
						}

						break;
					case Keys.C: // Copy
						if (ctrl)
							Clipboard.SetData(SerializationManager.HaClipboardData,
								selectedBoard.SerializationManager.SerializeList(selectedBoard.SelectedItems
									.Cast<ISerializableSelector>()));

						break;
					case Keys.V: // Paste
						if (ctrl && Clipboard.ContainsData(SerializationManager.HaClipboardData)) {
							List<ISerializable> items;
							try {
								items = selectedBoard.SerializationManager.DeserializeList(
									(string) Clipboard.GetData(SerializationManager.HaClipboardData));
							} catch (SerializationException de) {
								MessageBox.Show(de.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
								return;
							} catch (Exception e) {
								MessageBox.Show(string.Format("An error occurred: {0}", e.ToString()), "Error",
									MessageBoxButtons.OK, MessageBoxIcon.Error);
								return;
							}

							var needsLayer = false;

							// Make sure we dont have any tS conflicts
							string tS = null;
							foreach (var item in items) {
								if (item is TileInstance) {
									var tile = (TileInstance) item;
									var currtS = ((TileInfo) tile.BaseInfo).tS;
									if (currtS != tS) {
										if (tS == null) {
											tS = currtS;
										} else {
											MessageBox.Show(
												"Clipboard contains two tiles with different tile sets, cannot paste.",
												"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
											return;
										}
									}
								}

								if (item is IContainsLayerInfo) needsLayer = true;
							}

							if (needsLayer && (selectedBoard.SelectedLayerIndex < 0 ||
							                   selectedBoard.SelectedPlatform < 0)) {
								MessageBox.Show(
									"Layered items in clipboard and no layer/platform selected, cannot paste.", "Error",
									MessageBoxButtons.OK, MessageBoxIcon.Error);
								return;
							}

							if (tS != null && selectedBoard.SelectedLayer.tS != null &&
							    tS != selectedBoard.SelectedLayer.tS) {
								MessageBox.Show(
									"Clipboard contains tile in a different set than the current selected layer, cannot paste.",
									"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
								return;
							}

							// Calculate offsetting
							var minPos = new XNA.Point(int.MaxValue, int.MaxValue);
							var maxPos = new XNA.Point(int.MinValue, int.MinValue);
							foreach (var item in items)
								if (item is BoardItem) {
									var bi = (BoardItem) item;
									if (bi.Left < minPos.X)
										minPos.X = bi.Left;
									if (bi.Top < minPos.Y)
										minPos.Y = bi.Top;
									if (bi.Right > maxPos.X)
										maxPos.X = bi.Right;
									if (bi.Bottom > maxPos.Y)
										maxPos.Y = bi.Bottom;
								} else if (item is Rope) {
									var r = (Rope) item;
									var x = r.FirstAnchor.X;
									var minY = Math.Min(r.FirstAnchor.Y, r.SecondAnchor.Y);
									var maxY = Math.Max(r.FirstAnchor.Y, r.SecondAnchor.Y);
									if (x < minPos.X)
										minPos.X = x;
									if (x > maxPos.X)
										maxPos.X = x;
									if (minY < minPos.Y)
										minPos.Y = minY;
									if (maxY > maxPos.Y)
										maxPos.Y = maxY;
								}

							var center = new XNA.Point((maxPos.X + minPos.X) / 2, (maxPos.Y + minPos.Y) / 2);
							var offset = new XNA.Point(selectedBoard.Mouse.X - center.X,
								selectedBoard.Mouse.Y - center.Y);

							// Add the items
							ClearSelectedItems(selectedBoard);
							var undoPipe = new List<UndoRedoAction>();
							foreach (var item in items) {
								item.AddToBoard(undoPipe);
								item.PostDeserializationActions(true, offset);
							}

							selectedBoard.BoardItems.Sort();
							selectedBoard.UndoRedoMan.AddUndoBatch(undoPipe);
						}

						break;
					case Keys.Z:
						if (ctrl && selectedBoard.UndoRedoMan.UndoList.Count > 0) selectedBoard.UndoRedoMan.Undo();
						clearRedo = false;
						break;
					case Keys.Y:
						if (ctrl && selectedBoard.UndoRedoMan.RedoList.Count > 0) selectedBoard.UndoRedoMan.Redo();
						clearRedo = false;
						break;
					case Keys.S:
						if (ctrl)
							parentBoard.OnExportRequested();
						break;
					case Keys.O:
						if (ctrl)
							parentBoard.OnLoadRequested();
						break;
					case Keys.Escape:
						if (selectedBoard.Mouse.State == MouseState.Selection) {
							ClearBoundItems(selectedBoard);
							ClearSelectedItems(selectedBoard);
							clearRedo = false;
						} else if (selectedBoard.Mouse.State == MouseState.Footholds) {
							selectedBoard.Mouse.Clear();
						} else {
							parentBoard.InvokeReturnToSelectionState();
						}

						break;
					default:
						clearRedo = false;
						break;
					case Keys.W:
						if (ctrl)
							parentBoard.OnCloseTabRequested();
						break;
					case Keys.Tab:
						if (ctrl)
							parentBoard.OnSwitchTabRequested(shift);
						break;
					case Keys.F5:
						UserSettings.altBackground = !UserSettings.altBackground;
						parentBoard.HaCreatorStateManager.Ribbon.altBackgroundToggle.IsChecked = UserSettings.altBackground;
						break;
					case Keys.F12:
						UserSettings.displayFHSide = !UserSettings.displayFHSide;
						parentBoard.HaCreatorStateManager.Ribbon.fhSideToggle.IsChecked = UserSettings.displayFHSide;
						break;
				}

				if (actions.Count > 0)
					selectedBoard.UndoRedoMan.AddUndoBatch(actions);
				if (clearRedo)
					selectedBoard.UndoRedoMan.RedoList.Clear();
			}
		}

		private bool ClickOnMinimap(Board selectedBoard, XNA.Point position) {
			if (selectedBoard.MiniMap == null || !UserSettings.useMiniMap) return false;
			return position.X > 0 && position.X < selectedBoard.MinimapArea.Width && position.Y > 0 &&
			       position.Y < selectedBoard.MinimapArea.Height;
		}

		private void parentBoard_MouseDoubleClick(Board selectedBoard, BoardItem target, XNA.Point realPosition,
			XNA.Point virtualPosition) {
			lock (parentBoard) {
				OnUserInteraction();
				if (ClickOnMinimap(selectedBoard, realPosition)) return;
				if (target != null) {
					ClearSelectedItems(selectedBoard);
					target.Selected = true;
					parentBoard.EditInstanceClicked(target);
				} else if (selectedBoard.Mouse.State == MouseState.Footholds) {
					selectedBoard.Mouse.CreateFhAnchor();
				}
			}
		}

		private void parentBoard_RightMouseClick(Board selectedBoard, BoardItem rightClickTarget,
			XNA.Point realPosition, XNA.Point virtualPosition, MouseState mouseState) {
			lock (parentBoard) {
				OnUserInteraction();
				if (mouseState == MouseState.Selection) {
					ClearBoundItems(selectedBoard);
					if (ClickOnMinimap(selectedBoard, realPosition))
						return;

					if (rightClickTarget == null)
						return;

					if (!rightClickTarget.Selected)
						ClearSelectedItems(selectedBoard);
					rightClickTarget.Selected = true;
					var bicm = new BoardItemContextMenu(parentBoard, selectedBoard, rightClickTarget);

					// be warned when run under visual studio. it inherits VS's scaling and VS's window location
					var point =
						parentBoard.PointToScreen(new System.Windows.Point(realPosition.X, realPosition.Y));

					bicm.Menu.Show(new System.Drawing.Point((int) point.X, (int) point.Y));
				} else {
					parentBoard.InvokeReturnToSelectionState();
				}
			}
		}

		private void parentBoard_LeftMouseUp(Board selectedBoard, BoardItem target, BoardItem selectedTarget,
			XNA.Point realPosition, XNA.Point virtualPosition, bool selectedItemHigher) {
			lock (parentBoard) {
				OnUserInteraction();
				if (selectedBoard.Mouse.State == MouseState.Selection) //handle drag-drop selection end
					ClearBoundItems(selectedBoard);
				else if (selectedBoard.Mouse.State == MouseState.StaticObjectAdding ||
				         selectedBoard.Mouse.State == MouseState.RandomTiles ||
				         selectedBoard.Mouse.State == MouseState.Chairs ||
				         selectedBoard.Mouse.State == MouseState.Ropes ||
				         selectedBoard.Mouse.State == MouseState.Tooltip ||
				         selectedBoard.Mouse.State ==
				         MouseState.Clock) //handle clicks that are meant to add an item to the board
					selectedBoard.Mouse.PlaceObject();
				else if (selectedBoard.Mouse.State == MouseState.Footholds) selectedBoard.Mouse.TryConnectFoothold();
			}
		}

		private void HandleMinimapBrowse(Board selectedBoard, XNA.Point realPosition) {
			var h = realPosition.X * selectedBoard.mag - (int) parentBoard.ActualWidth / 2;
			var v = realPosition.Y * selectedBoard.mag - (int) parentBoard.ActualHeight / 2;
			if (h < 0) selectedBoard.hScroll = 0;
			else if (h > parentBoard.MaxHScroll)
				selectedBoard.hScroll = (int) parentBoard.MaxHScroll;
			else selectedBoard.hScroll = h;
			if (v < 0) selectedBoard.vScroll = 0;
			else if (v > parentBoard.MaxVScroll)
				selectedBoard.vScroll = (int) parentBoard.MaxVScroll;
			else selectedBoard.vScroll = v;
		}

		private void parentBoard_LeftMouseDown(Board selectedBoard, BoardItem item, BoardItem selectedItem,
			XNA.Point realPosition, XNA.Point virtualPosition, bool selectedItemHigher) {
			lock (parentBoard) {
				OnUserInteraction();
				if (ClickOnMinimap(selectedBoard, realPosition) && selectedBoard.Mouse.State == MouseState.Selection) {
					//ClearSelectedItems(selectedBoard);
					selectedBoard.Mouse.MinimapBrowseOngoing = true;
					HandleMinimapBrowse(selectedBoard, realPosition);
				} else if (selectedBoard.Mouse.State == MouseState.Selection) {
					//handle drag-drop, multiple selection and all that
					var ctrlDown = (Control.ModifierKeys & Keys.Control) == Keys.Control;
					if (item == null && selectedItem == null) { //drag-selection is starting
						if (!ctrlDown) ClearSelectedItems(selectedBoard);

						selectedBoard.Mouse.MultiSelectOngoing = true;
						selectedBoard.Mouse.MultiSelectStart = virtualPosition;
					} else { //Single click on item
						BoardItem itemToSelect = null;
						var itemAlreadySelected = false;

						if (item == null) // If user didn't click on any non-selected item, we want to keep selectedItem as our bound item
						{
							itemToSelect = selectedItem;
							itemAlreadySelected = true;
						} else if
							(selectedItem ==
							 null) // We are guaranteed (item != null) at this point, so just select item
						{
							itemToSelect = item;
						} else if
							(!selectedItemHigher) // item needs to be selected but there is already a selectedItem; only switch selection if the selectedItem is not higher
						{
							itemToSelect = item;
						} else // Otherwise, just mark selectedItem as the item we are selecting
						{
							itemToSelect = selectedItem;
							itemAlreadySelected = true;
						}

						if (!itemAlreadySelected &&
						    !ctrlDown) // If we are changing selection and ctrl is not down, clear current selected items
							ClearSelectedItems(selectedBoard);

						if (ctrlDown) // If we are clicking an item and ctrl IS down, we need to toggle its selection
						{
							itemToSelect.Selected = !itemToSelect.Selected;
						} else // Otherwise, mark the item as selected (if it's already selected nothing will happen) and bind it to the mouse to start drag-drop action
						{
							itemToSelect.Selected = true;
							selectedBoard.Mouse.SingleSelectStarting = true;
							selectedBoard.Mouse.SingleSelectStart = virtualPosition;
							//BindAllSelectedItems(selectedBoard); // not binding selected items here because we will bind them after significant movement
						}
					}
				}
			}
		}

		private void BindAllSelectedItems(Board selectedBoard) {
			BindAllSelectedItems(selectedBoard, new XNA.Point(selectedBoard.Mouse.X, selectedBoard.Mouse.Y));
		}

		private void BindAllSelectedItems(Board selectedBoard, XNA.Point mousePosition) {
			foreach (var itemToSelect in selectedBoard.SelectedItems) {
				selectedBoard.Mouse.BindItem(itemToSelect,
					new XNA.Point(itemToSelect.X - mousePosition.X, itemToSelect.Y - mousePosition.Y));
				if (itemToSelect is BackgroundInstance)
					itemToSelect.moveStartPos = new XNA.Point(((BackgroundInstance) itemToSelect).BaseX,
						((BackgroundInstance) itemToSelect).BaseY);
				else
					itemToSelect.moveStartPos = new XNA.Point(itemToSelect.X, itemToSelect.Y);
			}
		}

		public static void ClearSelectedItems(Board board) {
			lock (board.ParentControl) {
				while (board.SelectedItems.Count > 0) board.SelectedItems[0].Selected = false;
			}
		}

		public static void ClearBoundItems(Board board) {
			lock (board.ParentControl) {
				var undoActions = new List<UndoRedoAction>();
				bool addUndo;
				var items = board.Mouse.BoundItems.Keys.ToList();
				foreach (var item in items) {
					addUndo = item.tempParent == null || !(item.tempParent.Parent is Mouse);
					board.Mouse.ReleaseItem(item);
					if (addUndo) {
						if (item is BackgroundInstance && (((BackgroundInstance) item).BaseX != item.moveStartPos.X ||
						                                   ((BackgroundInstance) item).BaseY != item.moveStartPos.Y))
							undoActions.Add(UndoRedoManager.BackgroundMoved((BackgroundInstance) item,
								new XNA.Point(item.moveStartPos.X, item.moveStartPos.Y),
								new XNA.Point(((BackgroundInstance) item).BaseX, ((BackgroundInstance) item).BaseY)));
						else if (!(item is BackgroundInstance) &&
						         (item.X != item.moveStartPos.X || item.Y != item.moveStartPos.Y))
							undoActions.Add(UndoRedoManager.ItemMoved(item,
								new XNA.Point(item.moveStartPos.X, item.moveStartPos.Y),
								new XNA.Point(item.X, item.Y)));
					}
				}

				if (undoActions.Count > 0)
					board.UndoRedoMan.AddUndoBatch(undoActions);
			}
		}
	}
}