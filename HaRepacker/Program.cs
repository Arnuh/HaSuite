﻿/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Windows.Forms;
using HaRepacker.GUI;
using Microsoft.Win32;
using System.Threading;
using MapleLib.WzLib;
using System.IO.Pipes;
using System.IO;
using System.Security.Principal;
using System.Globalization;
using MapleLib.Configuration;
using HaSharedLibrary;
using System.Runtime.CompilerServices;
using MapleLib;

namespace HaRepacker {
	public static class Program {
		private static WzFileManager _wzFileManager;

		public static WzFileManager WzFileManager {
			get => _wzFileManager;
			set => _wzFileManager = value;
		}

		public static NamedPipeServerStream pipe;
		public static Thread pipeThread;

		private static ConfigurationManager _ConfigurationManager; // default for VS UI designer

		public static ConfigurationManager ConfigurationManager {
			get => _ConfigurationManager;
			private set { }
		}

		public const string pipeName = "HaRepacker";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args) {
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			// Localisation
			var ci = GetMainCulture(CultureInfo.CurrentCulture);
			Properties.Resources.Culture = ci;

			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;

			CultureInfo.CurrentCulture = ci;
			CultureInfo.CurrentUICulture = ci;
			CultureInfo.DefaultThreadCurrentCulture = ci;
			CultureInfo.DefaultThreadCurrentUICulture = ci;

			// Threads
			ThreadPool.SetMaxThreads(Environment.ProcessorCount * 3,
				Environment.ProcessorCount * 3); // This includes hyper-threading(Intel)/SMT (AMD) count.

			// App
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			// Load WZFileManager
			_wzFileManager = new WzFileManager();

			// Parameters
			var firstRun = PrepareApplication(true);
			string wzToLoad = null;
			if (args.Length > 0)
				wzToLoad = args[0];
			Application.Run(new MainForm(wzToLoad, true, firstRun));
			EndApplication(true, true);
		}

		/// <summary>
		/// Allows customisation of display text during runtime..
		/// </summary>
		/// <param name="ci"></param>
		/// <returns></returns>
		private static CultureInfo GetMainCulture(CultureInfo ci) {
			if (!ci.Name.Contains("-"))
				return ci;
			switch (ci.Name.Split("-".ToCharArray())[0]) {
				case "ko":
					return new CultureInfo("ko");
				case "ja":
					return new CultureInfo("ja");
				case "en":
					return new CultureInfo("en");
				case "zh":
					if (ci.EnglishName.Contains("Simplified"))
						return new CultureInfo("zh-CHS");
					else
						return new CultureInfo("zh-CHT");
				default:
					return ci;
			}
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			new ThreadExceptionDialog((Exception) e.ExceptionObject).ShowDialog();
			Environment.Exit(-1);
		}

		/// <summary>
		/// Gets the local folder path
		/// </summary>
		/// <returns></returns>
		public static string GetLocalFolderPath() {
			var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var our_folder = Path.Combine(appdata, pipeName);
			if (!Directory.Exists(our_folder))
				Directory.CreateDirectory(our_folder);
			return our_folder;
		}


		public static bool IsUserAdministrator() {
			//bool value to hold our return value
			bool isAdmin;
			try {
				//get the currently logged in user
				var user = WindowsIdentity.GetCurrent();
				var principal = new WindowsPrincipal(user);
				isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
			} catch (Exception) {
				isAdmin = false;
			}

			return isAdmin;
		}

		public static bool PrepareApplication(bool from_internal) {
			_ConfigurationManager = new ConfigurationManager();

			var loaded = _ConfigurationManager.Load();
			if (!loaded) return true;

			var firstRun = ConfigurationManager.ApplicationSettings.FirstRun;
			if (ConfigurationManager.ApplicationSettings.FirstRun) {
				//new FirstRunForm().ShowDialog();
				ConfigurationManager.ApplicationSettings.FirstRun = false;
				_ConfigurationManager.Save();
			}

			if (ConfigurationManager.UserSettings.AutoAssociate && from_internal && IsUserAdministrator()) {
				var path = Application.ExecutablePath;
				Registry.ClassesRoot.CreateSubKey(".wz").SetValue("", "WzFile");
				var wzKey = Registry.ClassesRoot.CreateSubKey("WzFile");
				wzKey.SetValue("", "Wz File");
				wzKey.CreateSubKey("DefaultIcon").SetValue("", path + ",1");
				wzKey.CreateSubKey("shell\\open\\command").SetValue("", "\"" + path + "\" \"%1\"");
			}

			return firstRun;
		}

		public static void EndApplication(bool usingPipes, bool disposeFiles) {
			if (pipe != null && usingPipes) pipe.Close();

			if (disposeFiles) WzFileManager.Dispose();

			_ConfigurationManager.Save();
		}
	}
}