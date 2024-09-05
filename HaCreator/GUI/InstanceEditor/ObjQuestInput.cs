/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Windows.Forms;
using HaCreator.MapEditor.Instance;
using MapleLib.WzLib.WzStructure.Data;

namespace HaCreator.GUI.InstanceEditor {
	public partial class ObjQuestInput : EditorBase {
		public ObjectInstanceQuest result;

		public ObjQuestInput() {
			InitializeComponent();

			foreach (QuestState state in Enum.GetValues(typeof(QuestState))) {
				stateInput.Items.Add(state.ToString());
			}

			DialogResult = DialogResult.No;
			stateInput.SelectedIndex = 0;
		}

		protected override void cancelButton_Click(object sender, EventArgs e) {
			Close();
		}

		protected override void okButton_Click(object sender, EventArgs e) {
			result = new ObjectInstanceQuest((int) idInput.Value, (QuestState) stateInput.SelectedIndex);
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}