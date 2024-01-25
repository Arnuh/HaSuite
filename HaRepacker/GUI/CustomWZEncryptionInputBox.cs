﻿/* Copyright (C) 2020 LastBattle
 * https://github.com/lastbattle/Harepacker-resurrected
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Windows.Forms;
using MapleLib.MapleCryptoLib;
using MapleLib.PacketLib;

namespace HaRepacker.GUI {
	public partial class CustomWZEncryptionInputBox : Form {
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="panel"></param>
		public CustomWZEncryptionInputBox() {
			InitializeComponent();
		}


		/// <summary>
		/// Form load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SaveForm_Load(object sender, EventArgs e) {
			var appSettings = Program.ConfigurationManager.ApplicationSettings;

			// AES IV
			var storedCustomEnc = appSettings.MapleVersion_CustomEncryptionBytes;
			var splitBytes = storedCustomEnc.Split(' ');

			var parsed = true;
			if (splitBytes.Length == 4) {
				foreach (var byte_ in splitBytes) {
					if (!CheckHexDigits(byte_)) {
						parsed = false;
						break;
					}
				}
			} else {
				parsed = false;
			}

			if (!parsed) {
				// do nothing.. default, could be corrupted anyway
				appSettings.MapleVersion_CustomEncryptionBytes = "00 00 00 00";
				Program.ConfigurationManager.Save();
			} else {
				var i = 0;
				foreach (var byte_ in splitBytes) {
					switch (i) {
						case 0:
							textBox_byte0.Text = byte_;
							break;
						case 1:
							textBox_byte1.Text = byte_;
							break;
						case 2:
							textBox_byte2.Text = byte_;
							break;
						case 3:
							textBox_byte3.Text = byte_;
							break;
					}

					i++;
				}
			}

			// AES User key
			if (appSettings.MapleVersion_CustomAESUserKey == string.Empty) // set default if there's none
			{
				SetDefaultTextBoxAESUserKey();
			} else {
				var storedCustomAESKey = appSettings.MapleVersion_CustomAESUserKey;
				var splitAESKeyBytes = storedCustomAESKey.Split(' ');

				var parsed2 = true;
				if (splitAESKeyBytes.Length == 32) {
					foreach (var byte_ in splitAESKeyBytes) {
						if (!CheckHexDigits(byte_)) {
							parsed2 = false;
							break;
						}
					}
				} else {
					parsed2 = false;
				}

				if (!parsed2) {
					// do nothing.. default, could be corrupted anyway
					appSettings.MapleVersion_CustomAESUserKey = string.Empty;
					Program.ConfigurationManager.Save();
				} else {
					var i = 0;
					foreach (var byte_ in splitAESKeyBytes) {
						switch (i) {
							case 0:
								textBox_AESUserKey1.Text = byte_;
								break;
							case 1:
								textBox_AESUserKey2.Text = byte_;
								break;
							case 2:
								textBox_AESUserKey3.Text = byte_;
								break;
							case 3:
								textBox_AESUserKey4.Text = byte_;
								break;
							case 4:
								textBox_AESUserKey5.Text = byte_;
								break;
							case 5:
								textBox_AESUserKey6.Text = byte_;
								break;
							case 6:
								textBox_AESUserKey7.Text = byte_;
								break;
							case 7:
								textBox_AESUserKey8.Text = byte_;
								break;
							case 8:
								textBox_AESUserKey9.Text = byte_;
								break;
							case 9:
								textBox_AESUserKey10.Text = byte_;
								break;
							case 10:
								textBox_AESUserKey11.Text = byte_;
								break;
							case 11:
								textBox_AESUserKey12.Text = byte_;
								break;
							case 12:
								textBox_AESUserKey13.Text = byte_;
								break;
							case 13:
								textBox_AESUserKey14.Text = byte_;
								break;
							case 14:
								textBox_AESUserKey15.Text = byte_;
								break;
							case 15:
								textBox_AESUserKey16.Text = byte_;
								break;
							case 16:
								textBox_AESUserKey17.Text = byte_;
								break;
							case 17:
								textBox_AESUserKey18.Text = byte_;
								break;
							case 18:
								textBox_AESUserKey19.Text = byte_;
								break;
							case 19:
								textBox_AESUserKey20.Text = byte_;
								break;
							case 20:
								textBox_AESUserKey21.Text = byte_;
								break;
							case 21:
								textBox_AESUserKey22.Text = byte_;
								break;
							case 22:
								textBox_AESUserKey23.Text = byte_;
								break;
							case 23:
								textBox_AESUserKey24.Text = byte_;
								break;
							case 24:
								textBox_AESUserKey25.Text = byte_;
								break;
							case 25:
								textBox_AESUserKey26.Text = byte_;
								break;
							case 26:
								textBox_AESUserKey27.Text = byte_;
								break;
							case 27:
								textBox_AESUserKey28.Text = byte_;
								break;
							case 28:
								textBox_AESUserKey29.Text = byte_;
								break;
							case 29:
								textBox_AESUserKey30.Text = byte_;
								break;
							case 30:
								textBox_AESUserKey31.Text = byte_;
								break;
							case 31:
								textBox_AESUserKey32.Text = byte_;
								break;
						}

						i++;
					}
				}
			}
		}

		/// <summary>
		/// On save button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SaveButton_Click(object sender, EventArgs e) {
			// IV 
			var strByte0 = textBox_byte0.Text;
			var strByte1 = textBox_byte1.Text;
			var strByte2 = textBox_byte2.Text;
			var strByte3 = textBox_byte3.Text;

			if (!CheckHexDigits(strByte0) || !CheckHexDigits(strByte1) || !CheckHexDigits(strByte2) ||
			    !CheckHexDigits(strByte3)) {
				MessageBox.Show("Wrong format for AES IV. Please check the input bytes.", "Error");
				return;
			}

			// AES User Key
			var strUserKey1 = textBox_AESUserKey1.Text;
			var strUserKey2 = textBox_AESUserKey2.Text;
			var strUserKey3 = textBox_AESUserKey3.Text;
			var strUserKey4 = textBox_AESUserKey4.Text;
			var strUserKey5 = textBox_AESUserKey5.Text;
			var strUserKey6 = textBox_AESUserKey6.Text;
			var strUserKey7 = textBox_AESUserKey7.Text;
			var strUserKey8 = textBox_AESUserKey8.Text;
			var strUserKey9 = textBox_AESUserKey9.Text;
			var strUserKey10 = textBox_AESUserKey10.Text;
			var strUserKey11 = textBox_AESUserKey11.Text;
			var strUserKey12 = textBox_AESUserKey12.Text;
			var strUserKey13 = textBox_AESUserKey13.Text;
			var strUserKey14 = textBox_AESUserKey14.Text;
			var strUserKey15 = textBox_AESUserKey15.Text;
			var strUserKey16 = textBox_AESUserKey16.Text;
			var strUserKey17 = textBox_AESUserKey17.Text;
			var strUserKey18 = textBox_AESUserKey18.Text;
			var strUserKey19 = textBox_AESUserKey19.Text;
			var strUserKey20 = textBox_AESUserKey20.Text;
			var strUserKey21 = textBox_AESUserKey21.Text;
			var strUserKey22 = textBox_AESUserKey22.Text;
			var strUserKey23 = textBox_AESUserKey23.Text;
			var strUserKey24 = textBox_AESUserKey24.Text;
			var strUserKey25 = textBox_AESUserKey25.Text;
			var strUserKey26 = textBox_AESUserKey26.Text;
			var strUserKey27 = textBox_AESUserKey27.Text;
			var strUserKey28 = textBox_AESUserKey28.Text;
			var strUserKey29 = textBox_AESUserKey29.Text;
			var strUserKey30 = textBox_AESUserKey30.Text;
			var strUserKey31 = textBox_AESUserKey31.Text;
			var strUserKey32 = textBox_AESUserKey32.Text;

			if (
				!CheckHexDigits(strUserKey1) || !CheckHexDigits(strUserKey2) || !CheckHexDigits(strUserKey3) ||
				!CheckHexDigits(strUserKey4) || !CheckHexDigits(strUserKey5) || !CheckHexDigits(strUserKey6) ||
				!CheckHexDigits(strUserKey7) || !CheckHexDigits(strUserKey8) || !CheckHexDigits(strUserKey9) ||
				!CheckHexDigits(strUserKey10) ||
				!CheckHexDigits(strUserKey11) || !CheckHexDigits(strUserKey12) || !CheckHexDigits(strUserKey13) ||
				!CheckHexDigits(strUserKey14) || !CheckHexDigits(strUserKey15) || !CheckHexDigits(strUserKey16) ||
				!CheckHexDigits(strUserKey17) || !CheckHexDigits(strUserKey18) || !CheckHexDigits(strUserKey19) ||
				!CheckHexDigits(strUserKey20) ||
				!CheckHexDigits(strUserKey21) || !CheckHexDigits(strUserKey22) || !CheckHexDigits(strUserKey23) ||
				!CheckHexDigits(strUserKey24) || !CheckHexDigits(strUserKey25) || !CheckHexDigits(strUserKey26) ||
				!CheckHexDigits(strUserKey27) || !CheckHexDigits(strUserKey28) || !CheckHexDigits(strUserKey29) ||
				!CheckHexDigits(strUserKey30) ||
				!CheckHexDigits(strUserKey31) || !CheckHexDigits(strUserKey32)) {
				MessageBox.Show("Wrong format for AES User Key. Please check the input bytes.", "Error");
				return;
			}

			// Save
			Program.ConfigurationManager.ApplicationSettings.MapleVersion_CustomEncryptionBytes =
				string.Format("{0} {1} {2} {3}",
					strByte0,
					strByte1,
					strByte2,
					strByte3);

			Program.ConfigurationManager.ApplicationSettings.MapleVersion_CustomAESUserKey =
				string.Format(
					"{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18} {19} {20} {21} {22} {23} {24} {25} {26} {27} {28} {29} {30} {31}",
					strUserKey1, strUserKey2, strUserKey3, strUserKey4, strUserKey5, strUserKey6, strUserKey7,
					strUserKey8, strUserKey9, strUserKey10,
					strUserKey11, strUserKey12, strUserKey13, strUserKey14, strUserKey15, strUserKey16, strUserKey17,
					strUserKey18, strUserKey19, strUserKey20,
					strUserKey21, strUserKey22, strUserKey23, strUserKey24, strUserKey25, strUserKey26, strUserKey27,
					strUserKey28, strUserKey29, strUserKey30,
					strUserKey31, strUserKey32
				);
			Program.ConfigurationManager.Save();


			// Set the UserKey in memory.
			Program.ConfigurationManager.SetCustomWzUserKeyFromConfig();

			Close();
		}

		/// <summary>
		/// Checks the input hex string i.e "0x5E" if its valid or not.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private bool CheckHexDigits(string input) {
			if (input.Length >= 1 && input.Length <= 2) {
				for (var i = 0; i < input.Length; i++) {
					if (!HexEncoding.IsHexDigit(input[i])) {
						return false;
					}
				}
			} else {
				return false;
			}

			return true;
		}

		/// <summary>
		/// On clicked reset AES User Key
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_resetAESUserKey_Click(object sender, EventArgs e) {
			SetDefaultTextBoxAESUserKey();
		}

		private void SetDefaultTextBoxAESUserKey() {
			var AESUserKey = MapleCryptoConstants.MAPLESTORY_USERKEY_DEFAULT;

			textBox_AESUserKey1.Text = HexTool.ToString(AESUserKey[0 * 4]);
			textBox_AESUserKey2.Text = HexTool.ToString(AESUserKey[1 * 4]);
			textBox_AESUserKey3.Text = HexTool.ToString(AESUserKey[2 * 4]);
			textBox_AESUserKey4.Text = HexTool.ToString(AESUserKey[3 * 4]);
			textBox_AESUserKey5.Text = HexTool.ToString(AESUserKey[4 * 4]);
			textBox_AESUserKey6.Text = HexTool.ToString(AESUserKey[5 * 4]);
			textBox_AESUserKey7.Text = HexTool.ToString(AESUserKey[6 * 4]);
			textBox_AESUserKey8.Text = HexTool.ToString(AESUserKey[7 * 4]);
			textBox_AESUserKey9.Text = HexTool.ToString(AESUserKey[8 * 4]);
			textBox_AESUserKey10.Text = HexTool.ToString(AESUserKey[9 * 4]);
			textBox_AESUserKey11.Text = HexTool.ToString(AESUserKey[10 * 4]);
			textBox_AESUserKey12.Text = HexTool.ToString(AESUserKey[11 * 4]);
			textBox_AESUserKey13.Text = HexTool.ToString(AESUserKey[12 * 4]);
			textBox_AESUserKey14.Text = HexTool.ToString(AESUserKey[13 * 4]);
			textBox_AESUserKey15.Text = HexTool.ToString(AESUserKey[14 * 4]);
			textBox_AESUserKey16.Text = HexTool.ToString(AESUserKey[15 * 4]);
			textBox_AESUserKey17.Text = HexTool.ToString(AESUserKey[16 * 4]);
			textBox_AESUserKey18.Text = HexTool.ToString(AESUserKey[17 * 4]);
			textBox_AESUserKey19.Text = HexTool.ToString(AESUserKey[18 * 4]);
			textBox_AESUserKey20.Text = HexTool.ToString(AESUserKey[19 * 4]);
			textBox_AESUserKey21.Text = HexTool.ToString(AESUserKey[20 * 4]);
			textBox_AESUserKey22.Text = HexTool.ToString(AESUserKey[21 * 4]);
			textBox_AESUserKey23.Text = HexTool.ToString(AESUserKey[22 * 4]);
			textBox_AESUserKey24.Text = HexTool.ToString(AESUserKey[23 * 4]);
			textBox_AESUserKey25.Text = HexTool.ToString(AESUserKey[24 * 4]);
			textBox_AESUserKey26.Text = HexTool.ToString(AESUserKey[25 * 4]);
			textBox_AESUserKey27.Text = HexTool.ToString(AESUserKey[26 * 4]);
			textBox_AESUserKey28.Text = HexTool.ToString(AESUserKey[27 * 4]);
			textBox_AESUserKey29.Text = HexTool.ToString(AESUserKey[28 * 4]);
			textBox_AESUserKey30.Text = HexTool.ToString(AESUserKey[29 * 4]);
			textBox_AESUserKey31.Text = HexTool.ToString(AESUserKey[30 * 4]);
			textBox_AESUserKey32.Text = HexTool.ToString(AESUserKey[31 * 4]);
		}

		/// <summary>
		/// On hyperlink click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			const string link = "http://forum.ragezone.com/f921/maplestorys-aes-userkey-1116849/";

			System.Diagnostics.Process.Start(link);
		}
	}
}