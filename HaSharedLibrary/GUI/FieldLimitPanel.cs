/* Copyright (C) 2018 LastBattle
https://github.com/lastbattle/Harepacker-resurrected
*/

using System;
using System.Windows.Forms;
using MapleLib.WzLib.WzStructure.Data;

namespace HaSharedLibrary.GUI {
	public partial class FieldLimitPanel : UserControl {
		// UI
		private ChangeableTextBox textBox;

		// misc
		private bool initializingListViewForFieldLimit;


		public FieldLimitPanel() {
			InitializeComponent();

			Load += FieldLimitPanel_Load;
		}

		#region Events

		/// <summary>
		/// Loaded
		/// Note: This doesnt seems to get called automatically on WinForm.. 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FieldLimitPanel_Load(object sender, EventArgs e) {
			PopulateDefaultListView();
		}

		public void SetTextboxOnFieldLimitChange(ChangeableTextBox textBox) {
			this.textBox = textBox;
		}

		/// <summary>
		/// Update the checkboxes upon selection of a 'fieldLimit' WzIntProperty
		/// </summary>
		public void UpdateFieldLimitCheckboxes(ulong propertyValue) {
			initializingListViewForFieldLimit = true;

			_fieldLimit = propertyValue;

			// Fill checkboxes
			foreach (ListViewItem item in listView_fieldLimitType.Items)
				item.Checked = FieldLimitTypeExtension.Check((int) item.Tag, (long) propertyValue);

			initializingListViewForFieldLimit = false;
		}

		/// <summary>
		/// Populates the default values based upon hard coded WzFieldLimitType list
		/// </summary>
		public void PopulateDefaultListView() {
			initializingListViewForFieldLimit = true;

			// Populate FieldLimitType
			if (listView_fieldLimitType.Items.Count == 0) {
				// dummy column
				listView_fieldLimitType.Columns.Add(new ColumnHeader {
					Text = "",
					Name = "col1",
					Width = 550
				});

				var index = 0;
				foreach (FieldLimitType limitType in Enum.GetValues(typeof(FieldLimitType))) {
					var item = new ListViewItem(
						$"{index.ToString()} - {limitType.ToString().Replace("_", " ")}") {
						Tag = limitType // starts from 0
					};
					listView_fieldLimitType.Items.Add(item);

					index++;
				}
			}

			initializingListViewForFieldLimit = false;
		}

		/// <summary>
		/// On WzFieldLimitType listview item checked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListView_fieldLimitType_ItemChecked(object sender, ItemCheckedEventArgs e) {
			if (initializingListViewForFieldLimit) {
				return;
			}

			var numShift = (int) e.Item.Tag;
			var flag = (ulong) (1L << numShift);

			var curFlag = _fieldLimit;

			if (textBox != null) {
				curFlag = ulong.Parse(textBox.Text);
			}

			if (e.Item.Checked) {
				curFlag |= flag;
			} else {
				curFlag &= ~flag;
			}

			_fieldLimit = curFlag;

			if (textBox == null || string.Equals(textBox.Text, curFlag.ToString())) {
				return;
			}

			textBox.Text = curFlag.ToString();
		}

		#endregion

		#region Member Values

		private ulong _fieldLimit;

		public ulong FieldLimit {
			get => _fieldLimit;
			set => _fieldLimit = value;
		}

		#endregion
	}
}