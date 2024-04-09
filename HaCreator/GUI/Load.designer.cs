﻿using HaCreator.CustomControls;

namespace HaCreator.GUI
{
    partial class FieldSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FieldSelector));
            this.loadButton = new System.Windows.Forms.Button();
            this.WZSelect = new System.Windows.Forms.RadioButton();
            this.XMLSelect = new System.Windows.Forms.RadioButton();
            this.XMLBox = new System.Windows.Forms.TextBox();
            this.IMGBox = new System.Windows.Forms.TextBox();
            this.IMGSelect = new System.Windows.Forms.RadioButton();
            this.tabControl_maps = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.mapBrowser = new HaCreator.CustomControls.MapBrowser();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.searchBox = new HaCreator.CustomControls.WatermarkTextBox();
            this.tabControl_maps.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // loadButton
            // 
            this.loadButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.loadButton.Enabled = false;
            this.loadButton.Location = new System.Drawing.Point(8, 601);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(765, 30);
            this.loadButton.TabIndex = 9;
            this.loadButton.Text = "Load";
            this.loadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // WZSelect
            // 
            this.WZSelect.AutoSize = true;
            this.WZSelect.Checked = true;
            this.WZSelect.Location = new System.Drawing.Point(11, 63);
            this.WZSelect.Name = "WZSelect";
            this.WZSelect.Size = new System.Drawing.Size(42, 17);
            this.WZSelect.TabIndex = 6;
            this.WZSelect.TabStop = true;
            this.WZSelect.Text = "WZ";
            this.WZSelect.UseVisualStyleBackColor = true;
            this.WZSelect.CheckedChanged += new System.EventHandler(this.SelectionChanged);
            // 
            // XMLSelect
            // 
            this.XMLSelect.AutoSize = true;
            this.XMLSelect.Location = new System.Drawing.Point(11, 39);
            this.XMLSelect.Name = "XMLSelect";
            this.XMLSelect.Size = new System.Drawing.Size(46, 17);
            this.XMLSelect.TabIndex = 3;
            this.XMLSelect.Text = "XML";
            this.XMLSelect.UseVisualStyleBackColor = true;
            this.XMLSelect.CheckedChanged += new System.EventHandler(this.SelectionChanged);
            // 
            // XMLBox
            // 
            this.XMLBox.Enabled = false;
            this.XMLBox.Location = new System.Drawing.Point(64, 38);
            this.XMLBox.Name = "XMLBox";
            this.XMLBox.Size = new System.Drawing.Size(692, 22);
            this.XMLBox.TabIndex = 4;
            this.XMLBox.Click += new System.EventHandler(this.BrowseXML_Click);
            this.XMLBox.TextChanged += new System.EventHandler(this.XMLBox_TextChanged);
            // 
            // IMGBox
            // 
            this.IMGBox.Enabled = false;
            this.IMGBox.Location = new System.Drawing.Point(64, 12);
            this.IMGBox.Name = "IMGBox";
            this.IMGBox.Size = new System.Drawing.Size(692, 22);
            this.IMGBox.TabIndex = 1;
            this.IMGBox.Click += new System.EventHandler(this.BrowseIMG_Click);
            this.IMGBox.TextChanged += new System.EventHandler(this.IMGBox_TextChanged);
            // 
            // IMGSelect
            // 
            this.IMGSelect.AutoSize = true;
            this.IMGSelect.Location = new System.Drawing.Point(11, 13);
            this.IMGSelect.Name = "IMGSelect";
            this.IMGSelect.Size = new System.Drawing.Size(46, 17);
            this.IMGSelect.TabIndex = 0;
            this.IMGSelect.Text = "IMG";
            this.IMGSelect.UseVisualStyleBackColor = true;
            // 
            // tabControl_maps
            // 
            this.tabControl_maps.Controls.Add(this.tabPage1);
            this.tabControl_maps.Controls.Add(this.tabPage2);
            this.tabControl_maps.Location = new System.Drawing.Point(8, 86);
            this.tabControl_maps.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_maps.Name = "tabControl_maps";
            this.tabControl_maps.SelectedIndex = 0;
            this.tabControl_maps.Size = new System.Drawing.Size(769, 510);
            this.tabControl_maps.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.mapBrowser);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(761, 484);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Maps";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // mapBrowser
            // 
            this.mapBrowser.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.mapBrowser.Location = new System.Drawing.Point(6, 5);
            this.mapBrowser.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.mapBrowser.Name = "mapBrowser";
            this.mapBrowser.Size = new System.Drawing.Size(746, 465);
            this.mapBrowser.TabIndex = 8;
            this.mapBrowser.SelectionChanged += new HaCreator.CustomControls.MapBrowser.MapSelectChangedDelegate(this.MapBrowser_SelectionChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage2.Size = new System.Drawing.Size(761, 484);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "History";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // searchBox
            // 
            this.searchBox.ForeColor = System.Drawing.Color.Gray;
            this.searchBox.Location = new System.Drawing.Point(64, 62);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(692, 22);
            this.searchBox.TabIndex = 7;
            this.searchBox.Text = "Type here to search";
            this.searchBox.WatermarkActive = true;
            this.searchBox.WatermarkText = "Type here";
            // 
            // FieldSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(783, 637);
            this.Controls.Add(this.tabControl_maps);
            this.Controls.Add(this.IMGBox);
            this.Controls.Add(this.IMGSelect);
            this.Controls.Add(this.searchBox);
            this.Controls.Add(this.XMLBox);
            this.Controls.Add(this.XMLSelect);
            this.Controls.Add(this.WZSelect);
            this.Controls.Add(this.loadButton);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "FieldSelector";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Load";
            this.Load += new System.EventHandler(this.Load_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Load_KeyDown);
            this.tabControl_maps.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.RadioButton WZSelect;
        private System.Windows.Forms.RadioButton XMLSelect;
        private System.Windows.Forms.TextBox XMLBox;
        private WatermarkTextBox searchBox;
        private CustomControls.MapBrowser mapBrowser;
        private System.Windows.Forms.TextBox IMGBox;
        private System.Windows.Forms.RadioButton IMGSelect;
        private System.Windows.Forms.TabControl tabControl_maps;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
    }
}