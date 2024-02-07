using System.Windows.Forms;
using HaSharedLibrary.Properties;
using MapleLib.WzLib;

namespace HaSharedLibrary.Wz {
	public class WzEncryptionTypeHelper {
		public static void Setup(ComboBox encryptionBox, WzMapleVersion mapleVersion, bool includeAuto = false, bool includeBruteforce = false) {
			var index = GetIndexByWzMapleVersion(mapleVersion, includeAuto, includeBruteforce);
			Setup(encryptionBox, index, includeAuto, includeBruteforce);
		}

		public static void Setup(ComboBox encryptionBox, int index, bool includeAuto = false, bool includeBruteforce = false) {
			AddWzEncryptionTypesToComboBox(encryptionBox, includeAuto, includeBruteforce);
			if (encryptionBox.Items.Count < index + 1) {
				encryptionBox.SelectedIndex = encryptionBox.Items.Count - 1;
			} else {
				encryptionBox.SelectedIndex = index;
			}
		}

		public static void Setup(ToolStripComboBox encryptionBox, WzMapleVersion mapleVersion, bool includeAuto = false, bool includeBruteforce = false) {
			var index = GetIndexByWzMapleVersion(mapleVersion, includeAuto, includeBruteforce);
			Setup(encryptionBox, index, includeAuto, includeBruteforce);
		}

		public static void Setup(ToolStripComboBox encryptionBox, int index, bool includeAuto = false, bool includeBruteforce = false) {
			AddWzEncryptionTypesToComboBox(encryptionBox, includeAuto, includeBruteforce);
			if (encryptionBox.Items.Count < index + 1) {
				encryptionBox.SelectedIndex = encryptionBox.Items.Count - 1;
			} else {
				encryptionBox.SelectedIndex = index;
			}
		}

		/// <summary>
		/// Adds the WZ encryption types to ToolstripComboBox.
		/// Shared code between WzMapleVersionInputBox.cs
		/// </summary>
		/// <param name="encryptionBox"></param>
		/// <param name="includeAuto"></param>
		/// <param name="includeBruteforce"></param>
		public static void AddWzEncryptionTypesToComboBox(object encryptionBox, bool includeAuto = false, bool includeBruteforce = false) {
			string[] resources = {
				HaResources.EncTypeGMS,
				HaResources.EncTypeMSEA,
				HaResources.EncTypeNone,
				HaResources.EncTypeAuto,
				HaResources.EncTypeCustom,
				HaResources.EncTypeBruteforce
			};

			foreach (var res in resources) {
				if (!includeAuto && res.Equals(HaResources.EncTypeAuto)) {
					continue;
				}

				if (!includeBruteforce && res.Equals(HaResources.EncTypeBruteforce)) {
					continue;
				}

				if (encryptionBox is ToolStripComboBox tsComboBox) {
					tsComboBox.Items.Add(res);
				} else if (encryptionBox is ComboBox comboBox) {
					comboBox.Items.Add(res);
				}
			}
		}

		/// <summary>
		/// Gets the WzMapleVersion enum by encryptionBox selection index
		/// </summary>
		/// <param name="selectedIndex"></param>
		/// <returns></returns>
		public static WzMapleVersion GetWzMapleVersionByWzEncryptionBoxSelection(int selectedIndex, bool includeAuto = false, bool includeBruteforce = false) {
			if (!includeAuto) {
				if (selectedIndex >= 3) ++selectedIndex;
			}

			switch (selectedIndex) {
				case 0:
					return WzMapleVersion.GMS;
				case 1:
					return WzMapleVersion.EMS;
				case 2:
					return WzMapleVersion.BMS;
				case 3:
					return WzMapleVersion.AUTO;
				case 4:
					return WzMapleVersion.CUSTOM;
				case 5:
					return WzMapleVersion.BRUTEFORCE;
				default:
					return WzMapleVersion.BMS; // just default anyway to modern maplestory
			}
		}

		/// <summary>
		/// Gets the Combobox selection index by WzMapleVersion
		/// </summary>
		/// <param name="versionSelected"></param>
		/// <returns></returns>
		public static int GetIndexByWzMapleVersion(WzMapleVersion versionSelected, bool includeAuto = false, bool includeBruteforce = false) {
			if (!includeBruteforce && versionSelected == WzMapleVersion.BRUTEFORCE) {
				return 2;
			}

			if (!includeAuto && versionSelected == WzMapleVersion.AUTO) {
				return 2;
			}

			var index = 2;
			switch (versionSelected) {
				case WzMapleVersion.GMS:
					index = 0;
					break;
				case WzMapleVersion.EMS:
					index = 1;
					break;
				case WzMapleVersion.BMS:
					index = 2;
					break;
				case WzMapleVersion.AUTO:
					index = 3;
					break;
				case WzMapleVersion.CUSTOM:
					index = 4;
					break;
				case WzMapleVersion.BRUTEFORCE:
					index = 5;
					break;
			}

			if (!includeBruteforce) {
				if (index >= 5) --index;
			}

			if (!includeAuto) {
				if (index >= 3) --index;
			}

			return index;
		}
	}
}