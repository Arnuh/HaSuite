﻿using HaRepacker.GUI.Input;

namespace HaRepacker.GUI
{
    partial class OptionsForm
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
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
            this.sortBox = new System.Windows.Forms.CheckBox();
            this.loadRelated = new System.Windows.Forms.CheckBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.indentBox = new IntegerInput();
            this.lineBreakBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.apngIncompEnable = new System.Windows.Forms.CheckBox();
            this.defXmlFolderEnable = new System.Windows.Forms.CheckBox();
            this.defXmlFolderBox = new System.Windows.Forms.TextBox();
            this.browse = new System.Windows.Forms.Button();
            this.autoAssociateBox = new System.Windows.Forms.CheckBox();
            this.themeColor__comboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.openAI_apiKey_textBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // sortBox
            // 
            resources.ApplyResources(this.sortBox, "sortBox");
            this.sortBox.Name = "sortBox";
            this.sortBox.UseVisualStyleBackColor = true;
            // 
            // loadRelated
            // 
            resources.ApplyResources(this.loadRelated, "loadRelated");
            this.loadRelated.Name = "loadRelated";
            this.loadRelated.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            //
            // indentBox
            // 
            resources.ApplyResources(this.indentBox, "indentBox");
            this.indentBox.Name = "indentBox";
            this.indentBox.Value = 0;
            // 
            // 
            // lineBreakBox
            // 
            this.lineBreakBox.FormattingEnabled = true;
            this.lineBreakBox.Items.AddRange(new object[] {
            resources.GetString("lineBreakBox.Items"),
            resources.GetString("lineBreakBox.Items1"),
            resources.GetString("lineBreakBox.Items2")});
            resources.ApplyResources(this.lineBreakBox, "lineBreakBox");
            this.lineBreakBox.Name = "lineBreakBox";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // apngIncompEnable
            // 
            resources.ApplyResources(this.apngIncompEnable, "apngIncompEnable");
            this.apngIncompEnable.Name = "apngIncompEnable";
            this.apngIncompEnable.UseVisualStyleBackColor = true;
            // 
            // defXmlFolderEnable
            // 
            resources.ApplyResources(this.defXmlFolderEnable, "defXmlFolderEnable");
            this.defXmlFolderEnable.Name = "defXmlFolderEnable";
            this.defXmlFolderEnable.UseVisualStyleBackColor = true;
            this.defXmlFolderEnable.CheckedChanged += new System.EventHandler(this.defXmlFolderEnable_CheckedChanged);
            // 
            // defXmlFolderBox
            // 
            resources.ApplyResources(this.defXmlFolderBox, "defXmlFolderBox");
            this.defXmlFolderBox.Name = "defXmlFolderBox";
            // 
            // browse
            // 
            resources.ApplyResources(this.browse, "browse");
            this.browse.Name = "browse";
            this.browse.UseVisualStyleBackColor = true;
            this.browse.Click += new System.EventHandler(this.browse_Click);
            // 
            // autoAssociateBox
            // 
            resources.ApplyResources(this.autoAssociateBox, "autoAssociateBox");
            this.autoAssociateBox.Name = "autoAssociateBox";
            this.autoAssociateBox.UseVisualStyleBackColor = true;
            // 
            // themeColor__comboBox
            // 
            this.themeColor__comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.themeColor__comboBox, "themeColor__comboBox");
            this.themeColor__comboBox.FormattingEnabled = true;
            this.themeColor__comboBox.Items.AddRange(new object[] {
            resources.GetString("themeColor__comboBox.Items"),
            resources.GetString("themeColor__comboBox.Items1")});
            this.themeColor__comboBox.Name = "themeColor__comboBox";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.sortBox);
            this.panel1.Controls.Add(this.themeColor__comboBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.indentBox);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.lineBreakBox);
            this.panel1.Controls.Add(this.loadRelated);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.autoAssociateBox);
            this.panel1.Controls.Add(this.apngIncompEnable);
            this.panel1.Controls.Add(this.browse);
            this.panel1.Controls.Add(this.defXmlFolderEnable);
            this.panel1.Controls.Add(this.defXmlFolderBox);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.linkLabel1);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.openAI_apiKey_textBox);
            this.panel2.Controls.Add(this.label4);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // openAI_apiKey_textBox
            // 
            resources.ApplyResources(this.openAI_apiKey_textBox, "openAI_apiKey_textBox");
            this.openAI_apiKey_textBox.Name = "openAI_apiKey_textBox";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // linkLabel1
            // 
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // OptionsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "OptionsForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox sortBox;
        private System.Windows.Forms.CheckBox loadRelated;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label1;
        private IntegerInput indentBox;
        private System.Windows.Forms.ComboBox lineBreakBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox apngIncompEnable;
        private System.Windows.Forms.CheckBox defXmlFolderEnable;
        private System.Windows.Forms.TextBox defXmlFolderBox;
        private System.Windows.Forms.Button browse;
        private System.Windows.Forms.CheckBox autoAssociateBox;
        private System.Windows.Forms.ComboBox themeColor__comboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox openAI_apiKey_textBox;
        private System.Windows.Forms.Label label4;
    }
}