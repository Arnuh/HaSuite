﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Threading;
using HaRepacker.Comparer;
using HaRepacker.GUI.Input;
using HaRepacker.GUI.Interaction;
using HaRepacker.GUI.Panels;
using HaRepacker.Properties;
using HaSharedLibrary;
using MapleLib.Helpers;
using MapleLib.MapleCryptoLib;
using MapleLib.PacketLib;
using MapleLib.WzLib;
using MapleLib.WzLib.Nx;
using MapleLib.WzLib.Serialization;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;
using static MapleLib.Configuration.UserSettings;
using Application = System.Windows.Forms.Application;
using DataFormats = System.Windows.Forms.DataFormats;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using DragEventArgs = System.Windows.Forms.DragEventArgs;
using MessageBox = System.Windows.Forms.MessageBox;
using ProgressBar = System.Windows.Controls.ProgressBar;
using Size = System.Drawing.Size;
using Timer = System.Timers.Timer;

namespace HaRepacker.GUI {
	public partial class MainForm : Form {
		private readonly bool mainFormLoaded;

		private MainPanel MainPanel;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="wzPathToLoad"></param>
		/// <param name="usingPipes"></param>
		/// <param name="firstrun"></param>
		public MainForm(string wzPathToLoad, bool usingPipes, bool firstrun) {
			InitializeComponent();
			StartPosition = FormStartPosition.CenterScreen;

			AddTabsInternal("Default");

			// Sets theme color
			SetThemeColor();

			// encryptions
			AddWzEncryptionTypesToComboBox(encryptionBox);
			// Set encryption box
			SetWzEncryptionBoxSelectionByWzMapleVersion(Program.ConfigurationManager.ApplicationSettings.MapleVersion);


			WindowState = Program.ConfigurationManager.ApplicationSettings.WindowMaximized
				? FormWindowState.Maximized
				: FormWindowState.Normal;
			Size = new Size(
				Program.ConfigurationManager.ApplicationSettings.Width,
				Program.ConfigurationManager.ApplicationSettings.Height);

			// Drag and drop file
			DragEnter += MainForm_DragEnter;
			DragDrop += MainForm_DragDrop;

			// Drag and drop at the data tree
			MainPanel.DataTree.DragEnter += MainForm_DragEnter;
			MainPanel.DataTree.DragDrop += MainForm_DragDrop;

			// Set default selected main panel
			UpdateSelectedMainPanelTab();

			if (usingPipes) {
				try {
					Program.pipe = new NamedPipeServerStream(Program.pipeName, PipeDirection.In);
					Program.pipeThread = new Thread(PipeServer) {
						IsBackground = true
					};
					Program.pipeThread.Start();
				} catch (IOException) {
					if (wzPathToLoad != null) {
						try {
							using (var clientPipe =
							       new NamedPipeClientStream(".", Program.pipeName, PipeDirection.Out)) {
								clientPipe.Connect(1000);
								using (var sw = new StreamWriter(clientPipe)) {
									sw.WriteLine(wzPathToLoad);
								}
							}

							Environment.Exit(0);
						} catch (TimeoutException) {
						}
					}
				}
			}

			if (wzPathToLoad != null && File.Exists(wzPathToLoad)) {
				var encVersion = WzTool.DetectMapleVersion(wzPathToLoad, out _);
				SetWzEncryptionBoxSelectionByWzMapleVersion(encVersion);

				LoadWzFileCallback(wzPathToLoad);
			}

			var manager = new ContextMenuManager(MainPanel, MainPanel.UndoRedoMan);
			WzNode.ContextMenuBuilder = manager.CreateMenu;

			// Focus on the tab control
			tabControl_MainPanels.Focus();

			// flag. loaded
			mainFormLoaded = true;
		}

		#region Load, unload WZ files + Panels & TreeView management

		/// <summary>
		/// MainForm -- Drag the file from Windows Explorer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <exception cref="NotImplementedException"></exception>
		private void MainForm_DragEnter(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Move; // Allow the file to be copied
		}

		/// <summary>
		/// MainForm -- Drop the file from Windows Explorer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainForm_DragDrop(object sender, DragEventArgs e) {
			if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop)) {
				var files = (string[]) e.Data.GetData(DataFormats.FileDrop);

				// process the drag and dropped files
				OpenFileInternal(files);
			}
		}

		public void Interop_AddLoadedWzFileToManager(WzFile f) {
			InsertWzFileToPanel(f);
		}

		private delegate void LoadWzFileDelegate(string path);

		private void LoadWzFileCallback(string path) {
			try {
				var loadedWzFile = Program.WzFileManager.LoadWzFile(path,
					GetWzMapleVersionByWzEncryptionBoxSelection(encryptionBox.SelectedIndex));
				if (loadedWzFile == null) return;

				var node = new WzNode(loadedWzFile);

				MainPanel.DataTree.BeginUpdate();

				MainPanel.DataTree.Nodes.Add(node);
				SortNodesRecursively(node);
				MainPanel.DataTree.EndUpdate();
			} catch (Exception ex) {
				Warning.Error(string.Format(Resources.MainCouldntOpenWZ, path));
			}
		}

		/// <summary>
		/// Sort all nodes that is a parent of 
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="sortFromTheParentNode">Sorts only items in the parent node</param>
		public void SortNodesRecursively(WzNode parent, bool sortFromTheParentNode = false) {
			if (Program.ConfigurationManager.UserSettings.Sort || sortFromTheParentNode) {
				parent.TreeView.TreeViewNodeSorter = new TreeViewNodeSorter(sortFromTheParentNode ? parent : null);

				parent.TreeView.BeginUpdate();
				parent.TreeView.Sort();
				parent.TreeView.EndUpdate();
			}
		}

		public void SortNodeProperties(WzNode node) {
			if (node.Tag is WzSubProperty) {
				var nodeParent = (WzNode) node.Parent;

				nodeParent.TreeView.BeginUpdate();

				// sort the order in the WzSubProperty
				var subProperties = node.Tag as WzSubProperty;
				subProperties.SortProperties();

				// Refresh the TreeView view to be in synchronized with the new WzSubProperty's order
				var newNode = new WzNode(subProperties, true);
				nodeParent.Nodes[node.Index] = newNode;
				nodeParent.Nodes.Remove(node);

				nodeParent.TreeView.EndUpdate();
			}
		}

		/// <summary>
		/// Insert the WZ file to the main panel UI
		/// </summary>
		/// <param name="f"></param>
		/// <param name="panel"></param>
		public void InsertWzFileToPanel(WzFile f) {
			var node = new WzNode(f);

			MainPanel.DataTree.BeginUpdate();
			MainPanel.DataTree.Nodes.Add(node);
			MainPanel.DataTree.EndUpdate();

			SortNodesRecursively(node);
		}

		/// <summary>
		/// Delayed loading of the loaded WzFile to the TreeNode panel
		/// This primarily fixes some performance issue when loading multiple WZ concurrently.
		/// </summary>
		/// <param name="wzObj"></param>
		/// <param name="panel"></param>
		/// <param name="currentDispatcher"></param>
		public async void AddLoadedWzObjectToMainPanel(WzObject wzObj, Dispatcher currentDispatcher = null) {
			var node = new WzNode(wzObj);

			Debug.WriteLine("Adding wz object {0}, total size: {1}", wzObj.Name, MainPanel.DataTree.Nodes.Count);

			// execute in main thread
			if (currentDispatcher != null) {
				await currentDispatcher.BeginInvoke((Action) (() => {
					MainPanel.DataTree.BeginUpdate();

					MainPanel.DataTree.Nodes.Add(node);
					if (Program.ConfigurationManager.UserSettings.Sort) SortNodesRecursively(node);

					MainPanel.DataTree.EndUpdate();
					//MainPanel.DataTree.Update();
				}));
			} else {
				MainPanel.DataTree.BeginUpdate();

				MainPanel.DataTree.Nodes.Add(node);
				if (Program.ConfigurationManager.UserSettings.Sort) SortNodesRecursively(node);

				MainPanel.DataTree.EndUpdate();
				//MainPanel.DataTree.Update();
			}

			Debug.WriteLine("Done adding wz object {0}, total size: {1}", wzObj.Name, MainPanel.DataTree.Nodes.Count);
		}

		/// <summary>
		/// Reloaded the loaded wz file
		/// </summary>
		/// <param name="existingLoadedWzFile"></param>
		/// <param name="currentDispatcher"></param>
		public async void ReloadWzFile(WzFile existingLoadedWzFile, Dispatcher currentDispatcher = null) {
			// Get the current loaded wz file information
			var encVersion = existingLoadedWzFile.MapleVersion;
			var path = existingLoadedWzFile.FilePath;

			// Unload it
			if (currentDispatcher != null) {
				await currentDispatcher.BeginInvoke((Action) (() => { UnloadWzFile(existingLoadedWzFile, currentDispatcher); }));
			} else {
				UnloadWzFile(existingLoadedWzFile, currentDispatcher);
			}

			// Load the new wz file from the same path
			var newWzFile = Program.WzFileManager.LoadWzFile(path, encVersion);
			if (newWzFile != null) AddLoadedWzObjectToMainPanel(newWzFile, currentDispatcher);
		}

		/// <summary>
		/// Unload the loaded WZ file
		/// </summary>
		/// <param name="file"></param>
		public async void UnloadWzFile(WzFile file, Dispatcher currentDispatcher = null) {
			var node = (WzNode) file.HRTag; // get the ref first

			// unload the wz file
			Program.WzFileManager.UnloadWzFile(file, file.FilePath);

			// remove from treeview
			if (node != null) {
				if (currentDispatcher != null) {
					await currentDispatcher.BeginInvoke((Action) (() => { node.DeleteWzNode(); }));
				} else {
					node.DeleteWzNode();
				}
			}
		}

		#endregion

		#region Theme colors

		public void SetThemeColor() {
			if (Program.ConfigurationManager.UserSettings.ThemeColor == (int) UserSettingsThemeColor.Dark) //black
			{
				BackColor = Color.Black;
				mainMenu.BackColor = Color.Black;
				mainMenu.ForeColor = Color.White;

				/*for (int i = 0; i < mainMenu.Items.Count; i++)
				{
				    try
				    {
				        foreach (ToolStripMenuItem item in ((ToolStripMenuItem)mainMenu.Items[i]).DropDownItems)
				        {
				            item.BackColor = Color.Black;
				            item.ForeColor = Color.White;
				            MessageBox.Show(item.Name);
				        }
				    }
				    catch (Exception)
				    {
				        continue;
				        //throw;
				    }
				}*/
				button_addTab.ForeColor = Color.White;
				button_addTab.BackColor = Color.Black;
			} else {
				BackColor = DefaultBackColor;
				mainMenu.BackColor = DefaultBackColor;
				mainMenu.ForeColor = Color.Black;

				button_addTab.ForeColor = Color.Black;
				button_addTab.BackColor = Color.White;
			}
		}

		#endregion

		#region Wz Encryption selection combobox

		/// <summary>
		/// Adds the WZ encryption types to ToolstripComboBox.
		/// Shared code between WzMapleVersionInputBox.cs
		/// </summary>
		/// <param name="encryptionBox"></param>
		public static void AddWzEncryptionTypesToComboBox(object encryptionBox) {
			string[] resources = {
				Resources.EncTypeGMS,
				Resources.EncTypeMSEA,
				Resources.EncTypeNone,
				Resources.EncTypeCustom,
				Resources.EncTypeGenerate
			};
			var isToolStripComboBox = encryptionBox is ToolStripComboBox;

			var i = 0;
			foreach (var res in resources) {
				if (isToolStripComboBox) {
					((ToolStripComboBox) encryptionBox).Items.Add(res); // in mainform
				} else {
					if (i != 4) // dont show bruteforce option in SaveForm
					{
						((ComboBox) encryptionBox).Items.Add(res); // in saveForm
					}
				}

				i++;
			}
		}

		/// <summary>
		/// On encryption box selection changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EncryptionBox_SelectedIndexChanged(object sender, EventArgs e) {
			if (!mainFormLoaded) // first run during app startup
			{
				return;
			}

			var selectedIndex = encryptionBox.SelectedIndex;
			var wzMapleVer = GetWzMapleVersionByWzEncryptionBoxSelection(selectedIndex);
			Program.ConfigurationManager.ApplicationSettings.MapleVersion = wzMapleVer;

			if (wzMapleVer == WzMapleVersion.CUSTOM) {
				var customWzInputBox = new CustomWZEncryptionInputBox();
				customWzInputBox.ShowDialog();
			} else {
				MapleCryptoConstants.UserKey_WzLib = MapleCryptoConstants.MAPLESTORY_USERKEY_DEFAULT.ToArray();
			}
		}

		/// <summary>
		/// Gets the WzMapleVersion enum by encryptionBox selection index
		/// </summary>
		/// <param name="selectedIndex"></param>
		/// <returns></returns>
		public static WzMapleVersion GetWzMapleVersionByWzEncryptionBoxSelection(int selectedIndex) {
			WzMapleVersion wzMapleVer;
			switch (selectedIndex) {
				case 0:
					wzMapleVer = WzMapleVersion.GMS;
					break;
				case 1:
					wzMapleVer = WzMapleVersion.EMS;
					break;
				case 2:
					wzMapleVer = WzMapleVersion.BMS;
					break;
				case 3:
					wzMapleVer = WzMapleVersion.CUSTOM;
					break;
				case 4:
					wzMapleVer = WzMapleVersion.GENERATE;
					break;
				default: // hmm?
					wzMapleVer = WzMapleVersion.BMS; // just default anyway to modern maplestory
					break;
			}

			return wzMapleVer;
		}

		/// <summary>
		/// Gets the Combobox selection index by WzMapleVersion
		/// </summary>
		/// <param name="versionSelected"></param>
		/// <param name="fromNewForm">Called from NewForm.cs</param>
		/// <returns></returns>
		public static int GetIndexByWzMapleVersion(WzMapleVersion versionSelected, bool fromNewForm = false) {
			var setIndex = 0;
			switch (versionSelected) {
				case WzMapleVersion.GMS:
					setIndex = 0;
					break;
				case WzMapleVersion.EMS:
					setIndex = 1;
					break;
				case WzMapleVersion.BMS:
					setIndex = 2;
					break;
				case WzMapleVersion.CUSTOM:
					setIndex = 3;
					break;
				case WzMapleVersion.GENERATE:
					if (fromNewForm) // dont return GENERATE, as that option is unavailable when creating a new WZ via NewForm.
					{
						setIndex = 2; // BMS
					} else {
						setIndex = 4;
					}

					break;
			}

			return setIndex;
		}

		/// <summary>
		/// Sets the ComboBox selection index by WzMapleVersion enum 
		/// on program init.
		/// </summary>
		/// <param name="versionSelected"></param>
		private void SetWzEncryptionBoxSelectionByWzMapleVersion(WzMapleVersion versionSelected) {
			encryptionBox.SelectedIndex = GetIndexByWzMapleVersion(versionSelected);
			if (versionSelected == WzMapleVersion.CUSTOM) Program.ConfigurationManager.SetCustomWzUserKeyFromConfig();
		}

		#endregion

		#region Win32 API interop

		private delegate void SetWindowStateDelegate(FormWindowState state);

		private void SetWindowStateCallback(FormWindowState state) {
			WindowState = state;
			user32.SetWindowPos(Handle, user32.HWND_TOPMOST, 0, 0, 0, 0, user32.SWP_NOMOVE | user32.SWP_NOSIZE);
			user32.SetWindowPos(Handle, user32.HWND_NOTOPMOST, 0, 0, 0, 0, user32.SWP_NOMOVE | user32.SWP_NOSIZE);
		}

		private void SetWindowStateThreadSafe(FormWindowState state) {
			if (InvokeRequired) {
				Invoke(new SetWindowStateDelegate(SetWindowStateCallback), state);
			} else {
				SetWindowStateCallback(state);
			}
		}

		#endregion

		private string OnPipeRequest(string requestPath) {
			if (File.Exists(requestPath)) {
				MainPanel.Dispatcher.Invoke(() => { LoadWzFileCallback(requestPath); });
			}

			return "OK";
		}

		private void PipeServer() {
			try {
				while (true) {
					Program.pipe.WaitForConnection();
					var sr = new StreamReader(Program.pipe);
					OnPipeRequest(sr.ReadLine());
					Program.pipe.Disconnect();
				}
			} catch {
			}
		}

		#region UI Handlers

		private void MainForm_Load(object sender, EventArgs e) {
		}

		/// <summary>
		/// Redocks the list of controls on the panel
		/// </summary>
		private void RedockControls() {
			/*   int mainControlHeight = this.Size.Height;
			   int mainControlWidth = this.Size.Width;

			   foreach (TabPage page in tabControl_MainPanels.TabPages)
			   {
			       page.Size = new Size(mainControlWidth, mainControlHeight);
			   }*/
		}

		private void MainForm_SizeChanged(object sender, EventArgs e) {
			if (!mainFormLoaded) {
				return;
			}

			if (Size.Width * Size.Height != 0) {
				RedockControls();

				Program.ConfigurationManager.ApplicationSettings.Height = Size.Height;
				Program.ConfigurationManager.ApplicationSettings.Width = Size.Width;
				Program.ConfigurationManager.ApplicationSettings.WindowMaximized =
					WindowState == FormWindowState.Maximized;
			}
		}

		/// <summary>
		/// When the selected tab in the MainForm change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tabControl_MainPanels_TabIndexChanged(object sender, EventArgs e) {
			UpdateSelectedMainPanelTab();
		}

		/// <summary>
		///  On key up event for hotkeys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tabControl_MainPanels_KeyUp(object sender, KeyEventArgs e) {
			var countTabs = Convert.ToByte(tabControl_MainPanels.TabCount);

			if (e.Control) {
				switch (e.KeyCode) {
					case Keys.T: // Open new tab
						AddTabsInternal();
						break;
					case Keys.O: // Open new WZ file
						openToolStripMenuItem_Click(null, null);
						break;
					case Keys.I: // Open new Wz format
						toolStripMenuItem_newWzFormat_Click(null, null);
						break;
					case Keys.N: // New
						newToolStripMenuItem_Click(null, null);
						break;
					case Keys.A:
						MainPanel.StartAnimateSelectedCanvas();
						break;
					case Keys.P:
						break;

					// Switch between tabs
					case Keys.NumPad1:
						tabControl_MainPanels.SelectTab(0);
						break;
					case Keys.NumPad2:
						if (countTabs < 2) return;
						tabControl_MainPanels.SelectTab(1);
						break;
					case Keys.NumPad3:
						if (countTabs < 3) return;
						tabControl_MainPanels.SelectTab(2);
						break;
					case Keys.NumPad4:
						if (countTabs < 4) return;
						tabControl_MainPanels.SelectTab(3);
						break;
					case Keys.NumPad5:
						if (countTabs < 5) return;
						tabControl_MainPanels.SelectTab(4);
						break;
					case Keys.NumPad6:
						if (countTabs < 6) return;
						tabControl_MainPanels.SelectTab(5);
						break;
					case Keys.NumPad7:
						if (countTabs < 7) return;
						tabControl_MainPanels.SelectTab(6);
						break;
					case Keys.NumPad8:
						if (countTabs < 8) return;
						tabControl_MainPanels.SelectTab(7);
						break;
					case Keys.NumPad9:
						if (countTabs < 9) return;
						tabControl_MainPanels.SelectTab(8);
						break;
					case Keys.NumPad0:
						if (countTabs < 10) return;
						tabControl_MainPanels.SelectTab(9);
						break;
				}
			}
		}

		private void UpdateSelectedMainPanelTab() {
			var selectedTab = tabControl_MainPanels.SelectedTab;
			if (selectedTab != null && selectedTab.Controls.Count > 0) {
				var elemntHost =
					(ElementHost) selectedTab.Controls[0];

				MainPanel = (MainPanel) elemntHost?.Child;
			}
		}

		/// <summary>
		/// Add a new tab to the TabControl
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_addTab_Click(object sender, EventArgs e) {
			AddTabsInternal();
		}

		/// <summary>
		/// Prompts a window to add a new tab
		/// </summary>
		private void AddTabsInternal(string defaultName = null) {
			if (tabControl_MainPanels.TabCount > 10) return;

			var tabPage = new TabPage {
				Margin = new Padding(1, 1, 1, 1)
			};
			var elemHost = new ElementHost {
				Dock = DockStyle.Fill,
				Child = new MainPanel(this)
			};
			tabPage.Controls.Add(elemHost);


			if (defaultName == null) {
				if (!NameInputBox.Show(Resources.MainAddTabTitle, 25, out var tabName)) return;

				defaultName = tabName;
			} else {
				MainPanel = (MainPanel) elemHost.Child;
			}

			tabPage.Text = defaultName;

			tabControl_MainPanels.TabPages.Add(tabPage);

			// Focus on that tab control
			tabControl_MainPanels.Focus();
		}

		#endregion

		#region WZ IV Key bruteforcing

		private ulong wzKeyBruteforceTries;
		private DateTime wzKeyBruteforceStartTime = DateTime.Now;
		private bool wzKeyBruteforceCompleted;

		private Timer aTimer_wzKeyBruteforce;

		/// <summary>
		/// Find needles in a haystack o_O
		/// </summary>
		/// <param name="currentDispatcher"></param>
		private void StartWzKeyBruteforcing(string fileName, Dispatcher currentDispatcher) {
			if (!fileName.ToLower().Contains("tamingmob.wz") && !fileName.ToLower().Contains("data.wz")) {
				// Suggest to use a smaller file
				MessageBox.Show("You may be using a file that is large and could slow the process down.", "Error");
				return;
			}

			// Show splash screen
			MainPanel.OnSetPanelLoading(currentDispatcher);
			MainPanel.loadingPanel.SetWzIvBruteforceStackpanelVisiblity(Visibility.Visible);


			// Reset variables
			wzKeyBruteforceTries = 0;
			wzKeyBruteforceStartTime = DateTime.Now;
			wzKeyBruteforceCompleted = false;


			var processorCount =
				Environment.ProcessorCount *
				2; // 8 core = 16 (with ht, smt)
			// can * 3 but I noticed timer was no longer updating.
			var cpuIds = new List<int>();
			for (var cpuId_ = 0; cpuId_ < processorCount; cpuId_++) cpuIds.Add(cpuId_);

			// UI update thread
			if (aTimer_wzKeyBruteforce != null) {
				aTimer_wzKeyBruteforce.Stop();
				aTimer_wzKeyBruteforce = null;
			}

			aTimer_wzKeyBruteforce = new Timer();
			aTimer_wzKeyBruteforce.Elapsed += OnWzIVKeyUIUpdateEvent;
			aTimer_wzKeyBruteforce.Interval = 5000;
			aTimer_wzKeyBruteforce.Enabled = true;


			// Key finder thread
			Task.Run(() => {
				Thread.Sleep(3000); // delay 3 seconds before starting

				var parallelOption = new ParallelOptions {
					MaxDegreeOfParallelism = processorCount
				};
				var loop = Parallel.ForEach(cpuIds, parallelOption,
					cpuId => { WzKeyBruteforceComputeTask(cpuId, processorCount, fileName, currentDispatcher); });
			});
		}

		/// <summary>
		/// UI Updating thread
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void OnWzIVKeyUIUpdateEvent(object source, ElapsedEventArgs e) {
			if (aTimer_wzKeyBruteforce == null) {
				return;
			}

			if (wzKeyBruteforceCompleted) {
				aTimer_wzKeyBruteforce.Stop();
				aTimer_wzKeyBruteforce = null;

				MainPanel.loadingPanel.SetWzIvBruteforceStackpanelVisiblity(Visibility.Collapsed);
			}

			MainPanel.loadingPanel.WzIvKeyDuration = DateTime.Now.Ticks - wzKeyBruteforceStartTime.Ticks;
			MainPanel.loadingPanel.WzIvKeyTries = wzKeyBruteforceTries;
		}

		/// <summary>
		/// Internal compute task for figuring out the WzKey automaticagically 
		/// </summary>
		/// <param name="cpuId_"></param>
		/// <param name="processorCount"></param>
		/// <param name="filePath"></param>
		/// <param name="currentDispatcher"></param>
		private void WzKeyBruteforceComputeTask(int cpuId_, int processorCount, string filePath,
			Dispatcher currentDispatcher) {
			var cpuId = cpuId_;

			// try bruteforce keys
			const long startValue = int.MinValue;
			const long endValue = int.MaxValue;

			var lookupRangePerCPU = (endValue - startValue) / processorCount;

			Debug.WriteLine("CPUID {0}. Looking up from {1} to {2}. [Range = {3}]  TEST: {4} {5}",
				cpuId,
				startValue + lookupRangePerCPU * cpuId,
				startValue + lookupRangePerCPU * (cpuId + 1),
				lookupRangePerCPU,
				lookupRangePerCPU * cpuId, lookupRangePerCPU * (cpuId + 1));

			for (var i = startValue + lookupRangePerCPU * cpuId;
			     i < startValue + lookupRangePerCPU * (cpuId + 1);
			     i++) // 2 bill key pairs? o_O
			{
				if (wzKeyBruteforceCompleted) {
					break;
				}

				var bytes = new byte[4];
				unsafe {
					fixed (byte* pbytes = &bytes[0]) {
						*(int*) pbytes = (int) i;
					}
				}

				var tryDecrypt = WzTool.TryBruteforcingWzIVKey(filePath, bytes);
				//Debug.WriteLine("{0} = {1}", cpuId, HexTool.ToString(new PacketWriter(bytes).ToArray()));
				if (tryDecrypt) {
					wzKeyBruteforceCompleted = true;

					// Hide panel splash sdcreen
					Action action = () => {
						MainPanel.OnSetPanelLoadingCompleted(currentDispatcher);
						MainPanel.loadingPanel.SetWzIvBruteforceStackpanelVisiblity(Visibility
							.Collapsed);
					};
					currentDispatcher.BeginInvoke(action);

					MessageBox.Show("Found the encryption key to the WZ file:\r\n" + HexTool.ToString(bytes),
						"Success");
					Debug.WriteLine($"Found key. Key = {HexTool.ToString(bytes)}");

					break;
				}

				wzKeyBruteforceTries++;
			}
		}

		#endregion

		#region Open WZ File

		private async void OpenFileInternal(string[] fileNames) {
			var currentDispatcher = Dispatcher.CurrentDispatcher;

			var mapleVersionEncryptionSelected = GetWzMapleVersionByWzEncryptionBoxSelection(encryptionBox.SelectedIndex);

			var wzfilePathsToLoad = new List<string>();

			if (fileNames.All(s => s.ToLower().EndsWith(".xml"))) {
				ImportXml(mapleVersionEncryptionSelected, fileNames);
			} else if (fileNames.All(s => s.ToLower().EndsWith(".img"))) {
				ImportImg(mapleVersionEncryptionSelected, fileNames);
			} else if (fileNames.All(s => s.ToLower().EndsWith(".png"))) {
				ImportImages(fileNames);
			} else {
				foreach (var filePath in fileNames) {
					var filePathLowerCase = filePath.ToLower();

					if (filePathLowerCase.EndsWith("zlz.dll")) { // ZLZ.dll encryption keys
						var executingAssemblyName = Assembly.GetExecutingAssembly().GetName();
						//similarly to find process architecture  
						var assemblyArchitecture = executingAssemblyName.ProcessorArchitecture;

						if (assemblyArchitecture == ProcessorArchitecture.X86) {
							var form = new ZLZPacketEncryptionKeyForm();
							var opened = form.OpenZLZDllFile();

							if (opened) {
								form.Show();
							}
						} else {
							MessageBox.Show(Resources.ExecutingAssemblyError, Resources.Warning, MessageBoxButtons.OK);
						}

						return;
					}

					if (filePathLowerCase.EndsWith("data.wz") && WzTool.IsDataWzHotfixFile(filePath)) { // Other WZs
						var img = Program.WzFileManager.LoadDataWzHotfixFile(filePath, mapleVersionEncryptionSelected);
						if (img == null) {
							MessageBox.Show(Resources.MainFileOpenFail, Resources.Error);
							break;
						}

						AddLoadedWzObjectToMainPanel(img);
					} else if (filePathLowerCase.EndsWith(".xml")) {
						ImportXml(mapleVersionEncryptionSelected, new[] {filePath});
					} else if (filePathLowerCase.EndsWith(".img")) {
						ImportImg(mapleVersionEncryptionSelected, new[] {filePath});
					} else if (filePathLowerCase.EndsWith(".png")) {
						ImportImages(new[] {filePath});
					} else if (WzTool.IsListFile(filePath)) { // List.wz file (pre-bb maplestory enc)
						new ListEditor(filePath, mapleVersionEncryptionSelected).Show();
					} else {
						if (mapleVersionEncryptionSelected == WzMapleVersion.GENERATE) {
							StartWzKeyBruteforcing(filePath, currentDispatcher); // find needles in a haystack
							return;
						}

						wzfilePathsToLoad.Add(filePath); // add to list, so we can load it concurrently

						// Check if there are any related files
						string[] wzsWithRelatedFiles = {"Map", "Mob", "Skill", "Sound"};
						var bWithRelated = false;
						string relatedFileName = null;

						foreach (var wz in wzsWithRelatedFiles) {
							if (!filePathLowerCase.EndsWith(wz.ToLower() + ".wz")) continue;
							bWithRelated = true;
							relatedFileName = wz;
							break;
						}

						if (!bWithRelated) continue;
						if (!Program.ConfigurationManager.UserSettings.AutoloadRelatedWzFiles) continue;
						var otherMapWzFiles = Directory.GetFiles(filePath.Substring(0, filePath.LastIndexOf("\\")), relatedFileName + "*.wz");
						foreach (var filePath_Others in otherMapWzFiles) {
							if (filePath_Others != filePath) {
								wzfilePathsToLoad.Add(filePath_Others);
							}
						}
					}
				}
			}

			if (wzfilePathsToLoad.Count == 0) return;

			// Show splash screen
			MainPanel.OnSetPanelLoading();

			// Try opening one, to see if the user is having the right priviledge
			// Load all original WZ files 
			await Task.Run(() => {
				var loadedWzFiles = new List<WzFile>();
				var loop = Parallel.ForEach(wzfilePathsToLoad, filePath => {
					var f = Program.WzFileManager.LoadWzFile(filePath, mapleVersionEncryptionSelected);
					if (f == null) {
						// error should be thrown 
					} else {
						lock (loadedWzFiles) {
							loadedWzFiles.Add(f);
						}
					}
				});
				while (!loop.IsCompleted) Thread.Sleep(100); //?

				foreach (var wzFile in loadedWzFiles) // add later, once everything is loaded to memory
					AddLoadedWzObjectToMainPanel(wzFile, currentDispatcher);
			}); // load complete

			// Hide panel splash sdcreen
			MainPanel.OnSetPanelLoadingCompleted();
		}

		#endregion

		#region Toolstrip Menu items

		/// <summary>
		/// Open WZ file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void openToolStripMenuItem_Click(object sender, EventArgs e) {
			// Load WZ file
			using (var dialog = new OpenFileDialog {
				       Title = Resources.SelectWz,
				       Filter = $"{Resources.WzFilter}|*.wz;ZLZ.dll",
				       Multiselect = true
			       }) {
				if (dialog.ShowDialog() != DialogResult.OK) {
					return;
				}

				// Opens the selected file
				OpenFileInternal(dialog.FileNames);
			}
		}

		/// <summary>
		/// Open new WZ file (KMST) 
		/// with the split format
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void toolStripMenuItem_newWzFormat_Click(object sender, EventArgs e) {
			var currentDispatcher = Dispatcher.CurrentDispatcher;

			var MapleVersionEncryptionSelected =
				GetWzMapleVersionByWzEncryptionBoxSelection(encryptionBox.SelectedIndex);

			// Load WZ file
			using (var fbd = new FolderBrowserDialog {
				       Description = "Select the WZ folder (Base, Mob, Character, etc)",
				       ShowNewFolderButton = true
			       }) {
				var result = fbd.ShowDialog();
				if (result != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
					return;
				}

				var iniFilesPath = Directory.GetFiles(fbd.SelectedPath, "*.ini", SearchOption.AllDirectories);

				// Search for all '.ini' file, and for each .ini found, proceed to parse all items in the sub directory
				// merge all parsed directory as a single WZ

				var wzfilePathsToLoad = new List<string>();
				foreach (var iniFilePath in iniFilesPath) {
					var directoryName = Path.GetDirectoryName(iniFilePath);
					var wzFilesPath = Directory.GetFiles(directoryName, "*.wz", SearchOption.TopDirectoryOnly);

					foreach (var wzFilePath in wzFilesPath) {
						wzfilePathsToLoad.Add(wzFilePath);
						Debug.WriteLine(wzFilePath);
					}
				}

				// Show splash screen
				MainPanel.OnSetPanelLoading();


				// Load all original WZ files 
				await Task.Run(() => {
					var loadedWzFiles = new List<WzFile>();
					var loop = Parallel.ForEach(wzfilePathsToLoad, filePath => {
						var f = Program.WzFileManager.LoadWzFile(filePath, MapleVersionEncryptionSelected);
						if (f == null) {
							// error should be thrown 
						} else {
							lock (loadedWzFiles) {
								loadedWzFiles.Add(f);
							}
						}
					});
					while (!loop.IsCompleted) Thread.Sleep(100); //?

					foreach (var wzFile in loadedWzFiles) // add later, once everything is loaded to memory
						AddLoadedWzObjectToMainPanel(wzFile, currentDispatcher);
				}); // load complete

				// Hide panel splash sdcreen
				MainPanel.OnSetPanelLoadingCompleted();
			}
		}

		/// <summary>
		/// Unload all wz file -- toolstrip button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void unloadAllToolStripMenuItem_Click(object sender, EventArgs e) {
			if (Warning.Warn(Resources.MainUnloadAll)) {
				var currentThread = Dispatcher.CurrentDispatcher;

				var wzFiles = Program.WzFileManager.WzFileList;
				/*foreach (WzFile wzFile in wzFiles)
				{
				    UnloadWzFile(wzFile);
				};*/
				Parallel.ForEach(wzFiles, wzFile => { UnloadWzFile(wzFile, currentThread); });
			}
		}

		/// <summary>
		/// Reload all wz file -- toolstrip button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void reloadAllToolStripMenuItem_Click(object sender, EventArgs e) {
			if (Warning.Warn(Resources.MainReloadAll)) {
				var currentThread = Dispatcher.CurrentDispatcher;

				var wzFiles = Program.WzFileManager.WzFileList;
				/*foreach (WzFile wzFile in wzFiles)
				{
				    ReloadLoadedWzFile(wzFile);
				};*/
				Parallel.ForEach(wzFiles, wzFile => { ReloadWzFile(wzFile, currentThread); });
			}
		}

		/// <summary>
		/// Field/ map rendering
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void renderMapToolStripMenuItem_Click(object sender, EventArgs e) {
			if (MainPanel.DataTree.SelectedNode == null) {
				return;
			}

			if (MainPanel.DataTree.SelectedNode.Tag is WzImage) {
				var zoomLevel = double.Parse(zoomTextBox.TextBox.Text);
				var img = (WzImage) MainPanel.DataTree.SelectedNode.Tag;
				var mapName = img.Name.Substring(0, img.Name.Length - 4);

				if (!Directory.Exists("Renders\\" + mapName)) Directory.CreateDirectory("Renders\\" + mapName);

				try {
					var renderErrorList = new List<string>();

					var mapper = new FHMapper.FHMapper(MainPanel);
					mapper.ParseSettings();
					var rendered = mapper.TryRenderMapAndSave(img, zoomLevel, ref renderErrorList);

					if (!rendered) {
						var sb = new StringBuilder();
						var i = 1;
						foreach (var error in renderErrorList) {
							sb.Append("[").Append(i).Append("] ").Append(error);
							sb.AppendLine();
							i++;
						}

						MessageBox.Show(sb.ToString(), "Error rendering map");
					}
				} catch (ArgumentException argExp) {
					MessageBox.Show(argExp.Message, "Error rendering map");
				}
			}
		}

		/// <summary>
		/// Settings  -- toolstripmenu item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void settingsToolStripMenuItem_Click(object sender, EventArgs e) {
			var mapper = new FHMapper.FHMapper(MainPanel);
			mapper.ParseSettings();
			var settingsDialog = new Settings();
			settingsDialog.settings = mapper.settings;
			settingsDialog.main = mapper;
			settingsDialog.ShowDialog();
		}

		/// <summary>
		/// About -- toolstripmenu item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
			new AboutForm().ShowDialog();
		}

		/// <summary>
		/// Options - toolstripmenuitem
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void optionsToolStripMenuItem_Click(object sender, EventArgs e) {
			new OptionsForm().ShowDialog();
		}

		/// <summary>
		/// New WZ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void newToolStripMenuItem_Click(object sender, EventArgs e) {
			new NewForm(MainPanel).ShowDialog();
		}

		/// <summary>
		/// Save tool strip menu button clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SaveToolStripMenuItem_Click(object sender, EventArgs e) {
			WzNode node;
			if (MainPanel.DataTree.SelectedNode == null) {
				if (MainPanel.DataTree.Nodes.Count == 1) {
					node = (WzNode) MainPanel.DataTree.Nodes[0];
				} else {
					MessageBox.Show(Resources.MainSelectWzFolder,
						Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			} else {
				if (MainPanel.DataTree.SelectedNode.Tag is WzFile) {
					node = (WzNode) MainPanel.DataTree.SelectedNode;
				} else {
					node = ((WzNode) MainPanel.DataTree.SelectedNode).TopLevelNode;
				}
			}

			// Save to file.
			if (node.Tag is WzFile || node.Tag is WzImage) new SaveForm(MainPanel, node).ShowDialog();
		}

		/// <summary>
		/// On closing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
			Program.ConfigurationManager.ApplicationSettings.WindowMaximized = WindowState == FormWindowState.Maximized;
			e.Cancel = !Warning.Warn(Resources.MainConfirmExit);

			// Save app settings quickly
			if (!e.Cancel) Program.ConfigurationManager.Save();
		}

		private void RemoveToolStripMenuItem_Click(object sender, EventArgs e) {
			RemoveSelectedNodes();
		}

		private void RemoveSelectedNodes() {
			if (!Warning.Warn(Resources.MainConfirmRemoveNode)) return;

			MainPanel.PromptRemoveSelectedTreeNodes();
		}

		//yes I know this is a stupid way to synchronize threads, I'm just too lazy to use events or locks
		private bool threadDone;
		private Thread runningThread;


		private delegate void ChangeAppStateDelegate(bool enabled);

		private void ChangeApplicationStateCallback(bool enabled) {
			mainMenu.Enabled = enabled;
			MainPanel.IsEnabled = enabled;
			button_addTab.Enabled = enabled;
			tabControl_MainPanels.Enabled = enabled;
			AbortButton.Visible = !enabled;
		}

		private void ChangeApplicationState(bool enabled) {
			Invoke(new ChangeAppStateDelegate(ChangeApplicationStateCallback), enabled);
		}

		private void xMLToolStripMenuItem_Click(object sender, EventArgs e) {
			var dialog = new OpenFileDialog {
				Title = Resources.SelectWz,
				Filter = $"{Resources.WzFilter}|*.wz",
				Multiselect = true,
				InitialDirectory = Program.ConfigurationManager.UserSettings.PreviousLoadFolder
			};

			if (dialog.ShowDialog() != DialogResult.OK) {
				return;
			}

			var folderDialog = new FolderBrowserDialog {
				Description = Resources.SelectOutDir
			};
			if (folderDialog.ShowDialog() != DialogResult.OK) {
				return;
			}

			foreach (var filePath in dialog.FileNames)
				UpdatePreviousLoadDirectory(filePath);

			var serializer = new WzClassicXmlSerializer(
				Program.ConfigurationManager.UserSettings.Indentation,
				Program.ConfigurationManager.UserSettings.LineBreakType, false);

			threadDone = false;
			new Thread(RunWzFilesExtraction).Start(new object[]
				{dialog.FileNames, folderDialog.SelectedPath, encryptionBox.SelectedIndex, serializer});
			new Thread(ProgressBarThread).Start(serializer);
		}

		/// <summary>
		/// Updates the progress bar
		/// </summary>
		/// <param name="pbar"></param>
		/// <param name="value"></param>
		/// <param name="setMaxValue"></param>
		/// <param name="absolute"></param>
		private void UpdateProgressBar(ProgressBar pbar, int value, bool setMaxValue,
			bool absolute) {
			pbar.Dispatcher.Invoke(() => {
				if (setMaxValue) {
					if (absolute) {
						pbar.Maximum = value;
					} else {
						pbar.Maximum += value;
					}
				} else {
					if (absolute) {
						pbar.Value = value;
					} else {
						pbar.Value += value;
					}
				}
			});
		}


		private void ProgressBarThread(object param) {
			var serializer = (ProgressingWzSerializer) param;
			while (!threadDone) {
				var total = serializer.Total;
				UpdateProgressBar(MainPanel.secondaryProgressBar, total, true, true);
				UpdateProgressBar(MainPanel.secondaryProgressBar, Math.Min(total, serializer.Current), false, true);
				Thread.Sleep(500);
			}

			UpdateProgressBar(MainPanel.mainProgressBar, 1, true, true);
			UpdateProgressBar(MainPanel.mainProgressBar, 0, false, true);

			UpdateProgressBar(MainPanel.secondaryProgressBar, 1, true, true);
			UpdateProgressBar(MainPanel.secondaryProgressBar, 0, false, true);

			ChangeApplicationState(true);

			threadDone = false;
		}

		private string GetOutputDirectory() {
			return Program.ConfigurationManager.UserSettings.DefaultXmlFolder == ""
				? SavedFolderBrowser.Show(Resources.SelectOutDir)
				: Program.ConfigurationManager.UserSettings.DefaultXmlFolder;
		}

		private static void UpdatePreviousLoadDirectory(string fileName) {
			var directory = Directory.GetParent(fileName);
			if (directory == null) return;
			Program.ConfigurationManager.UserSettings.PreviousLoadFolder = directory.FullName;
		}

		private void rawDataToolStripMenuItem_Click(object sender, EventArgs e) {
			var dialog = new OpenFileDialog {
				Title = Resources.SelectWz,
				Filter = $"{Resources.WzFilter}|*.wz",
				Multiselect = true,
				InitialDirectory = Program.ConfigurationManager.UserSettings.PreviousLoadFolder
			};
			if (dialog.ShowDialog() != DialogResult.OK) {
				return;
			}

			foreach (var filePath in dialog.FileNames)
				UpdatePreviousLoadDirectory(filePath);

			var outPath = GetOutputDirectory();
			if (outPath == string.Empty) {
				MessageBox.Show(Resources.MainWzExportError, Resources.Warning,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			var serializer = new WzPngMp3Serializer();
			threadDone = false;
			runningThread = new Thread(RunWzFilesExtraction);
			runningThread.Start(new object[]
				{dialog.FileNames, outPath, encryptionBox.SelectedIndex, serializer});
			new Thread(ProgressBarThread).Start(serializer);
		}

		private void imgToolStripMenuItem_Click(object sender, EventArgs e) {
			var dialog = new OpenFileDialog {
				Title = Resources.SelectWz,
				Filter = $"{Resources.WzFilter}|*.wz",
				Multiselect = true,
				InitialDirectory = Program.ConfigurationManager.UserSettings.PreviousLoadFolder
			};

			if (dialog.ShowDialog() != DialogResult.OK) {
				return;
			}

			foreach (var filePath in dialog.FileNames)
				UpdatePreviousLoadDirectory(filePath);

			var outPath = GetOutputDirectory();
			if (outPath == string.Empty) {
				MessageBox.Show(Resources.MainWzExportError, Resources.Warning,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			var serializer = new WzImgSerializer();
			threadDone = false;
			runningThread = new Thread(RunWzFilesExtraction);
			runningThread.Start(new object[]
				{dialog.FileNames, outPath, encryptionBox.SelectedIndex, serializer});
			new Thread(ProgressBarThread).Start(serializer);
		}

		/// <summary>
		/// Export IMG
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void imgToolStripMenuItem1_Click(object sender, EventArgs e) {
			var outPath = GetOutputDirectory();
			if (outPath == string.Empty) {
				MessageBox.Show(Resources.MainWzExportError, Resources.Warning,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			var dirs = new List<WzDirectory>();
			var imgs = new List<WzImage>();
			foreach (WzNode node in MainPanel.DataTree.SelectedNodes) {
				if (node.Tag is WzDirectory) {
					dirs.Add((WzDirectory) node.Tag);
				} else if (node.Tag is WzImage) imgs.Add((WzImage) node.Tag);
			}

			var serializer = new WzImgSerializer();
			threadDone = false;
			runningThread = new Thread(RunWzImgDirsExtraction);
			runningThread.Start(new object[] {dirs, imgs, outPath, serializer});
			new Thread(ProgressBarThread).Start(serializer);
		}

		/// <summary>
		/// Export PNG / MP3
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void pNGsToolStripMenuItem_Click(object sender, EventArgs e) {
			var outPath = GetOutputDirectory();
			if (outPath == string.Empty) {
				MessageBox.Show(Resources.MainWzExportError, Resources.Warning,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			var objs = new List<WzObject>();
			foreach (WzNode node in MainPanel.DataTree.SelectedNodes) {
				if (node.Tag is WzObject) {
					objs.Add((WzObject) node.Tag);
				}
			}

			var serializer = new WzPngMp3Serializer();
			threadDone = false;

			runningThread = new Thread(RunWzObjExtraction);
			runningThread.Start(new object[] {objs, outPath, serializer});
			new Thread(ProgressBarThread).Start(serializer);
		}


		/// <summary>
		/// Export as Json
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void jSONToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportBsonJsonInternal(true);
		}

		/// <summary>
		/// Export as BSON
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void bSONToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportBsonJsonInternal(false);
		}

		/// <summary>
		/// Export as Json or Bson
		/// </summary>
		/// <param name="isJson"></param>
		private void ExportBsonJsonInternal(bool isJson) {
			var outPath = GetOutputDirectory();
			if (outPath == string.Empty) {
				MessageBox.Show(Resources.MainWzExportError, Resources.Warning,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			var dlgResult = MessageBox.Show(Resources.MainWzExportJson_IncludeBase64,
				Resources.MainWzExportJson_IncludeBase64_Title, MessageBoxButtons.YesNoCancel);
			if (dlgResult == DialogResult.Cancel) {
				return;
			}

			var bIncludeBase64BinData = dlgResult == DialogResult.Yes;

			var dirs = new List<WzDirectory>();
			var imgs = new List<WzImage>();
			foreach (WzNode node in MainPanel.DataTree.SelectedNodes) {
				if (node.Tag is WzDirectory directory) {
					dirs.Add(directory);
				} else if (node.Tag is WzImage image) {
					imgs.Add(image);
				} else if (node.Tag is WzFile file) dirs.Add(file.WzDirectory);
			}

			var serializer = new WzJsonBsonSerializer(
				Program.ConfigurationManager.UserSettings.Indentation,
				Program.ConfigurationManager.UserSettings.LineBreakType, bIncludeBase64BinData, isJson);
			threadDone = false;

			runningThread = new Thread(RunWzImgDirsExtraction);
			runningThread.Start(new object[] {dirs, imgs, outPath, serializer});

			new Thread(ProgressBarThread).Start(serializer);
		}

		/// <summary>
		/// Export to private server toolstrip
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void privateServerToolStripMenuItem_Click(object sender, EventArgs e) {
			var outPath = GetOutputDirectory();
			if (outPath == string.Empty) {
				MessageBox.Show(Resources.MainWzExportError, Resources.Warning,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			var dirs = new List<WzDirectory>();
			var imgs = new List<WzImage>();
			foreach (WzNode node in MainPanel.DataTree.SelectedNodes) {
				if (node.Tag is WzDirectory directory) {
					dirs.Add(directory);
				} else if (node.Tag is WzImage image) {
					imgs.Add(image);
				} else if (node.Tag is WzFile file) dirs.Add(file.WzDirectory);
			}

			var serializer = new WzClassicXmlSerializer(
				Program.ConfigurationManager.UserSettings.Indentation,
				Program.ConfigurationManager.UserSettings.LineBreakType, false);
			threadDone = false;

			runningThread = new Thread(RunWzImgDirsExtraction);
			runningThread.Start(new object[] {dirs, imgs, outPath, serializer});

			new Thread(ProgressBarThread).Start(serializer);
		}

		/// <summary>
		/// Export as XML,  classic
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void classicToolStripMenuItem_Click(object sender, EventArgs e) {
			var outPath = GetOutputDirectory();
			if (outPath == string.Empty) {
				MessageBox.Show(Resources.MainWzExportError, Resources.Warning,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			var dirs = new List<WzDirectory>();
			var imgs = new List<WzImage>();
			foreach (WzNode node in MainPanel.DataTree.SelectedNodes) {
				if (node.Tag is WzDirectory) {
					dirs.Add((WzDirectory) node.Tag);
				} else if (node.Tag is WzImage) {
					imgs.Add((WzImage) node.Tag);
				} else if (node.Tag is WzFile) dirs.Add(((WzFile) node.Tag).WzDirectory);
			}

			var serializer = new WzClassicXmlSerializer(
				Program.ConfigurationManager.UserSettings.Indentation,
				Program.ConfigurationManager.UserSettings.LineBreakType, true);
			threadDone = false;

			runningThread = new Thread(RunWzImgDirsExtraction);
			runningThread.Start(new object[] {dirs, imgs, outPath, serializer});

			new Thread(ProgressBarThread).Start(serializer);
		}

		/// <summary>
		/// Export as XML, new
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void newToolStripMenuItem1_Click(object sender, EventArgs e) {
			var dialog = new SaveFileDialog {
				Title = Resources.SelectOutXml,
				Filter = string.Format("{0}|*.xml", Resources.XmlFilter)
			};
			if (dialog.ShowDialog() != DialogResult.OK) {
				return;
			}

			var objs = new List<WzObject>();

			foreach (WzNode node in MainPanel.DataTree.SelectedNodes) {
				if (node.Tag is WzObject) {
					objs.Add((WzObject) node.Tag);
				}
			}

			var serializer = new WzNewXmlSerializer(
				Program.ConfigurationManager.UserSettings.Indentation,
				Program.ConfigurationManager.UserSettings.LineBreakType);
			threadDone = false;

			runningThread = new Thread(RunWzObjExtraction);
			runningThread.Start(new object[] {objs, dialog.FileName, serializer});

			new Thread(ProgressBarThread).Start(serializer);
		}

		private void expandAllToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.DataTree.BeginUpdate();
			MainPanel.DataTree.ExpandAll();
			MainPanel.DataTree.EndUpdate();
		}

		private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.DataTree.BeginUpdate();
			MainPanel.DataTree.CollapseAll();
			MainPanel.DataTree.EndUpdate();
		}

		private bool IsChildHoldingSelectedNode() {
			var selectedNode = MainPanel.DataTree.SelectedNode;
			if (selectedNode == null) return false;
			var tag = selectedNode.Tag;
			return tag is WzDirectory ||
			       tag is WzFile ||
			       tag is IPropertyContainer;
		}

		private void xMLToolStripMenuItem2_Click(object sender, EventArgs e) {
			if (!IsChildHoldingSelectedNode()) {
				return;
			}

			var wzFile = ((WzObject) MainPanel.DataTree.SelectedNode.Tag).WzFileParent;
			if (!(wzFile is WzFile)) {
				return;
			}

			var dialog = new OpenFileDialog {
				Title = Resources.SelectXml,
				Filter = $"{Resources.XmlFilter}|*.xml",
				Multiselect = true,
				InitialDirectory = Program.ConfigurationManager.UserSettings.PreviousLoadFolder
			};

			if (dialog.ShowDialog() != DialogResult.OK) {
				return;
			}

			foreach (var filePath in dialog.FileNames)
				UpdatePreviousLoadDirectory(filePath);

			ImportXml(wzFile.MapleVersion, dialog.FileNames);
		}

		private void iMGToolStripMenuItem2_Click(object sender, EventArgs e) {
			if (!IsChildHoldingSelectedNode()) {
				return;
			}

			var wzFile = ((WzObject) MainPanel.DataTree.SelectedNode.Tag).WzFileParent;
			if (!(wzFile is WzFile)) {
				return;
			}

			var dialog = new OpenFileDialog {
				Title = Resources.SelectWzImg,
				Filter = $"{Resources.WzImgFilter}|*.img",
				Multiselect = true,
				InitialDirectory = Program.ConfigurationManager.UserSettings.PreviousLoadFolder
			};
			if (dialog.ShowDialog() != DialogResult.OK) {
				return;
			}

			foreach (var filePath in dialog.FileNames)
				UpdatePreviousLoadDirectory(filePath);

			var input = WzMapleVersionInputBox.Show(Resources.InteractionWzMapleVersionTitle,
				out var wzImageImportVersion);
			if (!input) {
				return;
			}

			ImportImg(wzImageImportVersion, dialog.FileNames);
		}

		private void searchToolStripMenuItem_Click(object sender, EventArgs e) {
			//MainPanel.findStrip.Visible = true;
		}

		private static readonly string HelpFile = "Help.htm";

		private void ViewHelpToolStripMenuItem_Click(object sender, EventArgs e) {
			var helpPath = Path.Combine(Application.StartupPath, HelpFile);
			if (File.Exists(helpPath)) {
				Help.ShowHelp(this, HelpFile);
			} else {
				Warning.Error(string.Format(Resources.MainHelpOpenFail, HelpFile));
			}
		}

		private void CopyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.DoCopy();
		}

		private void PasteToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.DoPaste();
		}

		/// <summary>
		/// Wz string searcher tool
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToolStripMenuItem_searchWzStrings_Click(object sender, EventArgs e) {
			// Map name load
			string loadedWzVersion;
			var dataCache =
				new WzStringSearchFormDataCache(
					GetWzMapleVersionByWzEncryptionBoxSelection(encryptionBox.SelectedIndex));
			if (dataCache.OpenBaseWZFile(out loadedWzVersion)) {
				var form = new WzStringSearchForm(dataCache, loadedWzVersion);
				form.Show();
			}
		}

		#endregion

		private void AbortButton_Click(object sender, EventArgs e) {
			if (Warning.Warn(Resources.MainConfirmAbort)) {
				threadDone = true;
				runningThread.Abort();
			}
		}

		#region Image directory add

		/// <summary>
		/// Add WzDirectory
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzDirectoryToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzDirectoryToSelectedNode(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzImage
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzImageToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzImageToSelectedNode(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzByte
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzByteFloatPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzByteFloatToSelectedNode(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add new canvas toolstrip
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzCanvasPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzCanvasToSelectedNode(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzIntProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzCompressedIntPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzCompressedIntToSelectedNode(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzLongProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzLongPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzLongToSelectedNode(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzConvexProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzConvexPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzConvexPropertyToSelectedNode(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzDoubleProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzDoublePropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzDoublePropertyToSelectedNode(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzNullProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzNullPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzNullPropertyToSelectedNode(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzSoundProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzSoundPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzSoundPropertyToSelectedNode(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzStringProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzStringPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzStringPropertyToSelectedIndex(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzSubProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzSubPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzSubPropertyToSelectedIndex(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzShortProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzUnsignedShortPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzUnsignedShortPropertyToSelectedIndex(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzUOLProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzUolPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzUOLPropertyToSelectedIndex(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add WzVectorProperty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WzVectorPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzVectorPropertyToSelectedIndex(MainPanel.DataTree.SelectedNode);
		}

		/// <summary>
		/// Add Lua script property
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void wzLuaPropertyToolStripMenuItem_Click(object sender, EventArgs e) {
			MainPanel.AddWzLuaPropertyToSelectedIndex(MainPanel.DataTree.SelectedNode);
		}

		#endregion


		private delegate void InsertWzNode(WzNode node, WzNode parent);

		private void InsertWzNodeCallback(WzNode node, WzNode parent) {
			var child = WzNode.GetChildNode(parent, node.Text);
			if (child != null) {
				if (ShowReplaceDialog(node.Text)) {
					child.DeleteWzNode();
				} else {
					return;
				}
			}

			parent.AddNode(node, true);
		}

		private void InsertWzNodeThreadSafe(WzNode node, WzNode parent) {
			MainPanel.Dispatcher.Invoke(() => { InsertWzNodeCallback(node, parent); });
		}

		private bool yesToAll;
		private bool noToAll;
		private ReplaceResult result;

		private bool ShowReplaceDialog(string name) {
			if (yesToAll) {
				return true;
			}

			if (noToAll) {
				return false;
			}

			ReplaceBox.Show(name, out result);
			switch (result) {
				case ReplaceResult.NoToAll:
					noToAll = true;
					return false;
				case ReplaceResult.No:
					return false;
				case ReplaceResult.YesToAll:
					yesToAll = true;
					return true;
				case ReplaceResult.Yes:
					return true;
			}

			throw new Exception("cant get here anyway");
		}

		private void nXForamtToolStripMenuItem_Click(object sender, EventArgs e) {
			var dialog = new OpenFileDialog {
				Title = Resources.SelectWz,
				Filter = $"{Resources.WzFilter}|*.wz",
				Multiselect = true,
				InitialDirectory = Program.ConfigurationManager.UserSettings.PreviousLoadFolder
			};

			if (dialog.ShowDialog() != DialogResult.OK) {
				return;
			}

			foreach (var filePath in dialog.FileNames)
				UpdatePreviousLoadDirectory(filePath);

			var outPath = GetOutputDirectory();
			if (outPath == string.Empty) {
				MessageBox.Show(Resources.MainWzExportError, Resources.Warning,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			var serializer = new WzToNxSerializer();
			threadDone = false;

			runningThread = new Thread(RunWzFilesExtraction);
			runningThread.Start(new object[]
				{dialog.FileNames, outPath, encryptionBox.SelectedIndex, serializer});
			new Thread(ProgressBarThread).Start(serializer);
		}

		#region Import Helpers

		private void ImportXml(WzMapleVersion version, IEnumerable fileNames) {
			if (!IsChildHoldingSelectedNode()) {
				return;
			}

			var deserializer = new WzXmlDeserializer(true, WzTool.GetIvByMapleVersion(version));
			yesToAll = false;
			noToAll = false;
			threadDone = false;

			runningThread = new Thread(WzDeserializeImportThread);
			runningThread.Start(new object[] {
				deserializer, fileNames, MainPanel.DataTree.SelectedNode, null
			});
			new Thread(ProgressBarThread).Start(deserializer);
		}

		private void ImportImg(WzMapleVersion wzImageImportVersion, IEnumerable fileNames) {
			var selectedNode = MainPanel.DataTree.SelectedNode;
			if (selectedNode != null && !IsChildHoldingSelectedNode()) {
				return;
			}

			var iv = WzTool.GetIvByMapleVersion(wzImageImportVersion);
			var deserializer = new WzImgDeserializer(true);
			yesToAll = false;
			noToAll = false;
			threadDone = false;

			runningThread = new Thread(WzDeserializeImportThread);
			runningThread.Start(
				new object[] {
					deserializer, fileNames, selectedNode, iv
				});
			new Thread(ProgressBarThread).Start(deserializer);
		}

		private void ImportImages(string[] files) {
			if (!IsChildHoldingSelectedNode()) {
				return;
			}

			if (!PixelFormatSelector.Show((int) WzPngProperty.WzPixelFormat.Bgra8888, out var pixelFormat)) {
				return;
			}

			var parent = (WzNode) MainPanel.DataTree.SelectedNode;

			foreach (var file in files) {
				var name = Path.GetFileNameWithoutExtension(file);
				var img = (Bitmap) Image.FromFile(file);
				var canvas = new WzCanvasProperty(name);
				var pngProperty = new WzPngProperty();
				pngProperty.PixFormat = pixelFormat;
				pngProperty.SetImage(img);
				canvas.PngProperty = pngProperty;
				var node = new WzNode(canvas, true);
				InsertWzNodeThreadSafe(node, parent);
				node.AddObject(new WzVectorProperty(WzCanvasProperty.OriginPropertyName, new WzIntProperty("X", 0),
					new WzIntProperty("Y", 0)), MainPanel.UndoRedoMan);
			}
		}

		#endregion

		#region Import Threads

		private void WzDeserializeImportThread(object param) {
			ChangeApplicationState(false);

			var arr = (object[]) param;
			var deserializer = (ProgressingWzSerializer) arr[0];
			var files = (string[]) arr[1];
			var parent = (WzNode) arr[2];
			var iv = (byte[]) arr[3];

			var parentObj = (WzObject) parent?.Tag;
			if (parentObj is WzFile wzFile) {
				parentObj = wzFile.WzDirectory;
			}

			UpdateProgressBar(MainPanel.mainProgressBar, files.Length, true, true);

			foreach (var file in files) {
				List<WzObject> objs;
				try {
					if (deserializer is WzXmlDeserializer xmlDeserializer) {
						objs = xmlDeserializer.ParseXML(file);
					} else if (deserializer is WzImgDeserializer imgDeserializer) {
						objs = new List<WzObject> {
							imgDeserializer.WzImageFromIMGFile(file, iv, Path.GetFileName(file), out var successfullyParsedImage)
						};

						if (!successfullyParsedImage) {
							MessageBox.Show(
								string.Format(Resources.MainErrorImportingWzImageFile, file),
								Resources.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
							continue;
						}
					} else {
						return;
					}
				} catch (ThreadAbortException) {
					return;
				} catch (Exception e) {
					Warning.Error(string.Format(Resources.MainInvalidFileError, file, e.Message));
					UpdateProgressBar(MainPanel.mainProgressBar, 1, false, false);
					continue;
				}

				foreach (var obj in objs) {
					if (parent == null) {
						AddLoadedWzObjectToMainPanel(obj, MainPanel.Dispatcher);
						continue;
					}

					if (((!(obj is WzDirectory) && !(obj is WzImage)) || !(parentObj is WzDirectory)) &&
					    (!(obj is WzImageProperty) || !(parentObj is IPropertyContainer))) {
						continue;
					}

					var node = new WzNode(obj, true);
					InsertWzNodeThreadSafe(node, parent);
				}

				UpdateProgressBar(MainPanel.mainProgressBar, 1, false, false);
			}

			ErrorLogger.SaveToFile("WzImport_Errors.txt");

			threadDone = true;
		}

		#endregion

		#region Export Threads

		private void RunWzFilesExtraction(object param) {
			ChangeApplicationState(false);

			var wzFilesToDump = (string[]) ((object[]) param)[0];
			var baseDir = (string) ((object[]) param)[1];
			var version = GetWzMapleVersionByWzEncryptionBoxSelection((int) ((object[]) param)[2]);
			var serializer = (IWzFileSerializer) ((object[]) param)[3];

			UpdateProgressBar(MainPanel.mainProgressBar, 0, false, true);
			UpdateProgressBar(MainPanel.mainProgressBar, wzFilesToDump.Length, true, true);

			if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

			foreach (var wzpath in wzFilesToDump) {
				if (WzTool.IsListFile(wzpath)) {
					Warning.Error(string.Format(Resources.MainListWzDetected, wzpath));
					continue;
				}

				var f = new WzFile(wzpath, version);

				var parseStatus = f.ParseWzFile();

				serializer.SerializeFile(f, Path.Combine(baseDir, f.Name));
				f.Dispose();
				UpdateProgressBar(MainPanel.mainProgressBar, 1, false, false);
			}

			ErrorLogger.SaveToFile("WzExtract_Errors.txt");

			// Reset progress bar to 0
			UpdateProgressBar(MainPanel.mainProgressBar, 0, false, true);
			UpdateProgressBar(MainPanel.mainProgressBar, 0, true, true);

			threadDone = true;
		}

		private void RunWzImgDirsExtraction(object param) {
			ChangeApplicationState(false);

			var dirsToDump = (List<WzDirectory>) ((object[]) param)[0];
			var imgsToDump = (List<WzImage>) ((object[]) param)[1];
			var baseDir = (string) ((object[]) param)[2];
			var serializer = (IWzImageSerializer) ((object[]) param)[3];

			UpdateProgressBar(MainPanel.mainProgressBar, 0, false, true);
			UpdateProgressBar(MainPanel.mainProgressBar, dirsToDump.Count + imgsToDump.Count, true, true);


			if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

			foreach (var img in imgsToDump) {
				var escapedPath =
					Path.Combine(baseDir, ProgressingWzSerializer.EscapeInvalidFilePathNames(img.Name));

				serializer.SerializeImage(img, escapedPath);
				UpdateProgressBar(MainPanel.mainProgressBar, 1, false, false);
			}

			foreach (var dir in dirsToDump) {
				var escapedPath =
					Path.Combine(baseDir, ProgressingWzSerializer.EscapeInvalidFilePathNames(dir.Name));

				serializer.SerializeDirectory(dir, escapedPath);
				UpdateProgressBar(MainPanel.mainProgressBar, 1, false, false);
			}

			ErrorLogger.SaveToFile("WzExtract_Errors.txt");

			// Reset progress bar to 0
			UpdateProgressBar(MainPanel.mainProgressBar, 0, false, true);
			UpdateProgressBar(MainPanel.mainProgressBar, 0, true, true);

			threadDone = true;
		}

		private void RunWzObjExtraction(object param) {
			ChangeApplicationState(false);

#if DEBUG
			var watch = new Stopwatch();
			watch.Start();
#endif
			var objsToDump = (List<WzObject>) ((object[]) param)[0];
			var path = (string) ((object[]) param)[1];
			var serializers = (ProgressingWzSerializer) ((object[]) param)[2];

			UpdateProgressBar(MainPanel.mainProgressBar, 0, false, true);

			if (serializers is IWzObjectSerializer serializer) {
				UpdateProgressBar(MainPanel.mainProgressBar, objsToDump.Count, true, true);
				foreach (var obj in objsToDump) {
					serializer.SerializeObject(obj, path);
					UpdateProgressBar(MainPanel.mainProgressBar, 1, false, false);
				}
			} else if (serializers is WzNewXmlSerializer serializer_) {
				UpdateProgressBar(MainPanel.mainProgressBar, 1, true, true);
				serializer_.ExportCombinedXml(objsToDump, path);
				UpdateProgressBar(MainPanel.mainProgressBar, 1, false, false);
			}

			ErrorLogger.SaveToFile("WzExtract_Errors.txt");
#if DEBUG
			// test benchmark
			watch.Stop();
			Debug.WriteLine($"WZ files Extracted. Execution Time: {watch.ElapsedMilliseconds} ms");
#endif

			// Reset progress bar to 0
			UpdateProgressBar(MainPanel.mainProgressBar, 0, false, true);
			UpdateProgressBar(MainPanel.mainProgressBar, 0, true, true);

			threadDone = true;
		}

		#endregion
	}
}