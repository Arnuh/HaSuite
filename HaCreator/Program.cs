/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using HaCreator.GUI;
using HaCreator.MapEditor;
using HaCreator.Properties;
using HaCreator.Wz;
using MapleLib;
using MapleLib.WzLib;
using MapleLib.WzLib.WzStructure.Data;

namespace HaCreator {
	internal static class Program {
		public static WzFileManager WzManager;
		public static WzInformationManager InfoManager;
		public static bool AbortThreads = false;
		public static bool Restarting;

		public const string APP_NAME = "PheCreator";

		public static HaEditor HaEditorWindow = null;

		#region Settings

		public static WzSettingsManager SettingsManager;

		public static string GetLocalSettingsFolder() {
			var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var our_folder = Path.Combine(appdata, APP_NAME);
			if (!Directory.Exists(our_folder)) {
				Directory.CreateDirectory(our_folder);
			}

			return our_folder;
		}

		public static string GetLocalSettingsPath() {
			return Path.Combine(GetLocalSettingsFolder(), "Settings.json");
		}

		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main() {
			// Startup
#if !DEBUG
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

			// Localisation
			var ci = GetMainCulture(CultureInfo.CurrentCulture);
			Resources.Culture = ci;

			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;

			CultureInfo.CurrentCulture = ci;
			CultureInfo.CurrentUICulture = ci;
			CultureInfo.DefaultThreadCurrentCulture = ci;
			CultureInfo.DefaultThreadCurrentUICulture = ci;


			Resources.Culture = CultureInfo.CurrentCulture;
			InfoManager = new WzInformationManager();
			SettingsManager =
				new WzSettingsManager(GetLocalSettingsPath(), typeof(UserSettings), typeof(ApplicationSettings));
			SettingsManager.LoadSettings();
			if (ApplicationSettings.lastDefaultLayer >= MapConstants.MaxMapLayers) {
				ApplicationSettings.lastDefaultLayer = 0;
			}

			MultiBoard.RecalculateSettings();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			// Program run here
			var initForm = new Initialization();
			Application.Run(initForm);

			// Shutdown
			if (initForm.editor != null) {
				initForm.editor.hcsm.backupMan.ClearBackups();
			}

			SettingsManager.SaveSettings();
			if (Restarting) {
				Application.Restart();
			}

			if (WzManager != null) // doesnt initialise on load until WZ files are loaded via Initialization.xaml.cs
			{
				WzManager.Dispose();
			}
		}

		/// <summary>
		/// Allows customisation of display text during runtime..
		/// </summary>
		/// <param name="ci"></param>
		/// <returns></returns>
		private static CultureInfo GetMainCulture(CultureInfo ci) {
			if (!ci.Name.Contains("-")) {
				return ci;
			}

			switch (ci.Name.Split("-".ToCharArray())[0]) {
				case "ko":
					return new CultureInfo("ko");
				case "ja":
					return new CultureInfo("ja");
				case "en":
					return new CultureInfo("en");
				case "zh":
					if (ci.EnglishName.Contains("Simplified")) {
						return new CultureInfo("zh-CHS");
					}

					return new CultureInfo("zh-CHT");
				default:
					return ci;
			}
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			new ThreadExceptionDialog((Exception) e.ExceptionObject).ShowDialog();
			Environment.Exit(-1);
		}
	}
}