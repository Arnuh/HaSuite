/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Windows.Forms;

namespace HaRepacker.GUI.Input {
	public class DoubleInput : TextBox {
		public DoubleInput() {
			KeyPress += new KeyPressEventHandler(HandleKeyPress);
		}

		private void HandleKeyPress(object sender, KeyPressEventArgs e) {
			if (!(char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar) ||
			      (e.KeyChar == "."[0] && !Text.Contains("."))))
				e.Handled = true;
		}

		protected override void WndProc(ref Message msg) {
			if (msg.Msg == 770) {
				var cbdata = (string) Clipboard.GetDataObject().GetData(typeof(string));
				double foo = 0;
				if (!double.TryParse(cbdata, out foo)) {
					msg.Result = IntPtr.Zero;
					return;
				}
			}

			base.WndProc(ref msg);
		}

		public double Value {
			get {
				double result = 0;
				if (double.TryParse(Text, out result)) return result;
				else return 0;
			}
			set => Text = value.ToString();
		}

		private void InitializeComponent() {
			SuspendLayout();
			// 
			// DoubleInput
			// 
			Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular,
				System.Drawing.GraphicsUnit.Point, (byte) 0);
			ResumeLayout(false);
		}
	}
}