using System.ComponentModel;

namespace HaRepacker.GUI.Input {
	partial class PixelFormatSelector {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.formatSelector = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(12, 55);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(99, 23);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(203, 55);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(99, 23);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// formatSelector
			// 
			this.formatSelector.FormattingEnabled = true;
			this.formatSelector.Items.AddRange(new object[] {"argb4444", "argb8888", "argb1555", "rgb565", "DXT3", "DXT5"});
			this.formatSelector.Location = new System.Drawing.Point(12, 12);
			this.formatSelector.Name = "formatSelector";
			this.formatSelector.Size = new System.Drawing.Size(290, 21);
			this.formatSelector.TabIndex = 0;
			this.formatSelector.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.format_OnKeyPress);
			// 
			// PixelFormatSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(314, 82);
			this.Controls.Add(this.formatSelector);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "PixelFormatSelector";
			this.Text = "PixelFormatSelector";
			this.ResumeLayout(false);
		}

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.ComboBox formatSelector;

		private System.Windows.Forms.Button cancelButton;

		#endregion
	}
}