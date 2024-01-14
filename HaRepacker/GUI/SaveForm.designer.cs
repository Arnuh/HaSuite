﻿using HaRepacker.GUI.Input;

namespace HaRepacker.GUI
{
    partial class SaveForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveForm));
            this.encryptionBox = new System.Windows.Forms.ComboBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_64BitFile = new System.Windows.Forms.CheckBox();
            this.versionBox = new HaRepacker.GUI.Input.IntegerInput();
            this.SuspendLayout();
            // 
            // encryptionBox
            // 
            this.encryptionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encryptionBox.FormattingEnabled = true;
            resources.ApplyResources(this.encryptionBox, "encryptionBox");
            this.encryptionBox.Name = "encryptionBox";
            this.encryptionBox.SelectedIndexChanged += new System.EventHandler(this.encryptionBox_SelectedIndexChanged);
            // 
            // saveButton
            // 
            resources.ApplyResources(this.saveButton, "saveButton");
            this.saveButton.Name = "saveButton";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // checkBox_64BitFile
            // 
            resources.ApplyResources(this.checkBox_64BitFile, "checkBox_64BitFile");
            this.checkBox_64BitFile.Name = "checkBox_64BitFile";
            this.checkBox_64BitFile.UseVisualStyleBackColor = true;
            this.checkBox_64BitFile.CheckedChanged += new System.EventHandler(this.checkBox_64BitFile_CheckedChanged);
            // 
            // versionBox
            // 
            resources.ApplyResources(this.versionBox, "versionBox");
            this.versionBox.Name = "versionBox";
            this.versionBox.Value = 0;
            // 
            // SaveForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBox_64BitFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.versionBox);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.encryptionBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SaveForm";
            this.Load += new System.EventHandler(this.SaveForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        public System.Windows.Forms.ComboBox encryptionBox;
        private IntegerInput versionBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox_64BitFile;
    }
}