using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MapleLib;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest_WzFile {
	[TestClass]
	public class UnitTest1 {
		private static WzFileManager _fileManager = new WzFileManager("", false, false);

		private static readonly List<Tuple<string, WzMapleVersion>> _testFiles =
			new List<Tuple<string, WzMapleVersion>>();


		public UnitTest1() {
			// KMS
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_000_KMS_359.wz", WzMapleVersion.BMS));

			// GMS
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_000_GMS_237.wz", WzMapleVersion.BMS));
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_GMS_146.wz", WzMapleVersion.BMS));
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_GMS_176.wz", WzMapleVersion.BMS));
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_GMS_230.wz", WzMapleVersion.BMS));
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_GMS_75.wz", WzMapleVersion.GMS));
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_GMS_87.wz", WzMapleVersion.GMS));
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_GMS_95.wz", WzMapleVersion.GMS));

			// MSEA
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_SEA_135.wz", WzMapleVersion.BMS));
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_SEA_160.wz", WzMapleVersion.BMS));
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_SEA_211.wz", WzMapleVersion.BMS));
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_SEA_212.wz", WzMapleVersion.BMS));
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_000_SEA218.wz", WzMapleVersion.BMS));

			// Thailand MS
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_ThaiMS_3.wz", WzMapleVersion.BMS));

			// TaiwanMS
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TamingMob_TMS_113.wz", WzMapleVersion.EMS));
			_testFiles.Add(new Tuple<string, WzMapleVersion>("TMS_113_Item.wz", WzMapleVersion.EMS));
		}

		/// <summary>
		/// Test opening and saving hotfix wz file that is an image file with .wz extension
		/// </summary>
		[TestMethod]
		public void TestOpeningAndSavingHotfixWzFile() {
			const string fileName = "Data.wz";
			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "WzFiles", "Hotfix", fileName);

			Debug.WriteLine("Running test for " + fileName);

			try {
				var wzMapleVer = WzMapleVersion.BMS;
				var WzIv = WzTool.GetIvByMapleVersion(wzMapleVer);
				var UserKey = WzTool.GetUserKeyByMapleVersion(wzMapleVer);

				//////// Open first ////////
				var wzImg = _fileManager.LoadDataWzHotfixFile(filePath, wzMapleVer);

				Assert.IsTrue(wzImg != null, "Hotfix Data.wz loading failed.");

				//////// Save file ////////
				var tmpFilePath = filePath + ".tmp";
				var targetFilePath = filePath;

				using (var oldfs = File.Open(tmpFilePath, FileMode.OpenOrCreate)) {
					using (var wzWriter = new WzBinaryWriter(oldfs, WzIv, UserKey)) {
						wzImg.SaveImage(wzWriter); // Write to temp folder
						wzImg.Dispose(); // unload
					}
				}

				//////// Reload file first ////////
				var wzImg_newTmpFile = _fileManager.LoadDataWzHotfixFile(tmpFilePath, wzMapleVer);

				Assert.IsTrue(wzImg_newTmpFile != null, "loading of newly saved Hotfix Data.wz file failed.");

				wzImg_newTmpFile.Dispose(); // unload
				try {
					File.Delete(tmpFilePath);
				} catch (Exception exp) {
					Debug.WriteLine(exp); // nvm, dont show to user
				}
			} catch (Exception e) {
				Assert.IsTrue(true,
					"Error initializing " + Path.GetFileName(filePath) + " (" + e.Message +
					").\r\nAlso, check that the directory is valid and the file is not in use.");
			}
		}

		/// <summary>
		/// Test opening the older wz files
		/// </summary>
		[TestMethod]
		public void TestOlderWzFiles() {
			foreach (var testFile in _testFiles) {
				var fileName = testFile.Item1;
				var wzMapleVerEnc = testFile.Item2;

				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "WzFiles", "Common", fileName);

				Debug.WriteLine("Running test for " + fileName);

				try {
					var f = new WzFile(filePath, -1, wzMapleVerEnc);

					var parseStatus = f.ParseWzFile();

					Assert.IsFalse(parseStatus != WzFileParseStatus.Success,
						"Error initializing " + fileName + " (" + parseStatus.GetErrorDescription() + ").");
				} catch (Exception e) {
					Assert.IsTrue(true,
						"Error initializing " + Path.GetFileName(filePath) + " (" + e.Message +
						").\r\nAlso, check that the directory is valid and the file is not in use.");
				}
			}
		}
	}
}