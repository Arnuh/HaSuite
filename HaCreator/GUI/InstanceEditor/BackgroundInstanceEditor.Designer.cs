﻿namespace HaCreator.GUI.InstanceEditor
{
    partial class BackgroundInstanceEditor
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
            this.pathLabel = new System.Windows.Forms.Label();
            this.xInput = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.yInput = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.zInput = new System.Windows.Forms.NumericUpDown();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.typeBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.alphaBox = new System.Windows.Forms.NumericUpDown();
            this.front = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.ryBox = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.rxBox = new System.Windows.Forms.NumericUpDown();
            this.copyLabel = new System.Windows.Forms.Label();
            this.cyLabel = new System.Windows.Forms.Label();
            this.cyBox = new System.Windows.Forms.NumericUpDown();
            this.cxLabel = new System.Windows.Forms.Label();
            this.cxBox = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBox_screenMode = new System.Windows.Forms.ComboBox();
            this.checkBox_spineRandomStart = new System.Windows.Forms.CheckBox();
            this.groupBox_spine = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.comboBox_spineAnimation = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.trackBar_parallaxY = new System.Windows.Forms.TrackBar();
            this.trackBar_parallaxX = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.flipCheck = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize) (this.xInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.yInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.zInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.alphaBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.ryBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.rxBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.cyBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.cxBox)).BeginInit();
            this.groupBox_spine.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.trackBar_parallaxY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize) (this.trackBar_parallaxX)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // pathLabel
            // 
            this.pathLabel.Location = new System.Drawing.Point(2, 0);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(501, 41);
            this.pathLabel.TabIndex = 0;
            this.pathLabel.Text = "label1";
            this.pathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // xInput
            // 
            this.xInput.Location = new System.Drawing.Point(26, 21);
            this.xInput.Maximum = new decimal(new int[] {2147483647, 0, 0, 0});
            this.xInput.Minimum = new decimal(new int[] {-2147483648, 0, 0, -2147483648});
            this.xInput.Name = "xInput";
            this.xInput.Size = new System.Drawing.Size(50, 22);
            this.xInput.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(13, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "X";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(87, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Y";
            // 
            // yInput
            // 
            this.yInput.Location = new System.Drawing.Point(103, 21);
            this.yInput.Maximum = new decimal(new int[] {2147483647, 0, 0, 0});
            this.yInput.Minimum = new decimal(new int[] {-2147483648, 0, 0, -2147483648});
            this.yInput.Name = "yInput";
            this.yInput.Size = new System.Drawing.Size(50, 22);
            this.yInput.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(13, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Z";
            // 
            // zInput
            // 
            this.zInput.Location = new System.Drawing.Point(26, 49);
            this.zInput.Maximum = new decimal(new int[] {2147483647, 0, 0, 0});
            this.zInput.Name = "zInput";
            this.zInput.Size = new System.Drawing.Size(50, 22);
            this.zInput.TabIndex = 2;
            // 
            // okButton
            // 
            this.okButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.okButton.Location = new System.Drawing.Point(9, 421);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(228, 28);
            this.okButton.TabIndex = 10;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.cancelButton.Location = new System.Drawing.Point(254, 421);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(242, 28);
            this.cancelButton.TabIndex = 11;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Type:";
            // 
            // typeBox
            // 
            this.typeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeBox.FormattingEnabled = true;
            this.typeBox.ItemHeight = 13;
            this.typeBox.Location = new System.Drawing.Point(45, 21);
            this.typeBox.Name = "typeBox";
            this.typeBox.Size = new System.Drawing.Size(184, 21);
            this.typeBox.TabIndex = 3;
            this.typeBox.SelectedIndexChanged += new System.EventHandler(this.typeBox_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 79);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(116, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Alpha: (Transparency)";
            // 
            // alphaBox
            // 
            this.alphaBox.Location = new System.Drawing.Point(128, 77);
            this.alphaBox.Maximum = new decimal(new int[] {255, 0, 0, 0});
            this.alphaBox.Name = "alphaBox";
            this.alphaBox.Size = new System.Drawing.Size(100, 22);
            this.alphaBox.TabIndex = 4;
            // 
            // front
            // 
            this.front.AutoSize = true;
            this.front.Location = new System.Drawing.Point(9, 132);
            this.front.Name = "front";
            this.front.Size = new System.Drawing.Size(119, 17);
            this.front.TabIndex = 5;
            this.front.Text = "Front Background";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 73);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(12, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Y";
            // 
            // ryBox
            // 
            this.ryBox.Location = new System.Drawing.Point(27, 70);
            this.ryBox.Maximum = new decimal(new int[] {2147483647, 0, 0, 0});
            this.ryBox.Minimum = new decimal(new int[] {-2147483648, 0, 0, -2147483648});
            this.ryBox.Name = "ryBox";
            this.ryBox.Size = new System.Drawing.Size(50, 22);
            this.ryBox.TabIndex = 7;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(13, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "X";
            // 
            // rxBox
            // 
            this.rxBox.Location = new System.Drawing.Point(26, 15);
            this.rxBox.Maximum = new decimal(new int[] {2147483647, 0, 0, 0});
            this.rxBox.Minimum = new decimal(new int[] {-2147483648, 0, 0, -2147483648});
            this.rxBox.Name = "rxBox";
            this.rxBox.Size = new System.Drawing.Size(50, 22);
            this.rxBox.TabIndex = 6;
            // 
            // copyLabel
            // 
            this.copyLabel.AutoSize = true;
            this.copyLabel.Location = new System.Drawing.Point(6, 49);
            this.copyLabel.Name = "copyLabel";
            this.copyLabel.Size = new System.Drawing.Size(36, 13);
            this.copyLabel.TabIndex = 23;
            this.copyLabel.Text = "Copy:";
            // 
            // cyLabel
            // 
            this.cyLabel.AutoSize = true;
            this.cyLabel.Location = new System.Drawing.Point(134, 49);
            this.cyLabel.Name = "cyLabel";
            this.cyLabel.Size = new System.Drawing.Size(12, 13);
            this.cyLabel.TabIndex = 22;
            this.cyLabel.Text = "Y";
            // 
            // cyBox
            // 
            this.cyBox.Location = new System.Drawing.Point(150, 46);
            this.cyBox.Maximum = new decimal(new int[] {2147483647, 0, 0, 0});
            this.cyBox.Minimum = new decimal(new int[] {-2147483648, 0, 0, -2147483648});
            this.cyBox.Name = "cyBox";
            this.cyBox.Size = new System.Drawing.Size(50, 22);
            this.cyBox.TabIndex = 9;
            this.cyBox.ValueChanged += new System.EventHandler(this.cyBox_ValueChanged);
            // 
            // cxLabel
            // 
            this.cxLabel.AutoSize = true;
            this.cxLabel.Location = new System.Drawing.Point(57, 49);
            this.cxLabel.Name = "cxLabel";
            this.cxLabel.Size = new System.Drawing.Size(13, 13);
            this.cxLabel.TabIndex = 20;
            this.cxLabel.Text = "X";
            // 
            // cxBox
            // 
            this.cxBox.Location = new System.Drawing.Point(73, 46);
            this.cxBox.Maximum = new decimal(new int[] {2147483647, 0, 0, 0});
            this.cxBox.Minimum = new decimal(new int[] {-2147483648, 0, 0, -2147483648});
            this.cxBox.Name = "cxBox";
            this.cxBox.Size = new System.Drawing.Size(50, 22);
            this.cxBox.TabIndex = 8;
            this.cxBox.ValueChanged += new System.EventHandler(this.cxBox_ValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 107);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(77, 13);
            this.label9.TabIndex = 24;
            this.label9.Text = "Screen Mode:";
            // 
            // comboBox_screenMode
            // 
            this.comboBox_screenMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_screenMode.FormattingEnabled = true;
            this.comboBox_screenMode.ItemHeight = 13;
            this.comboBox_screenMode.Location = new System.Drawing.Point(86, 104);
            this.comboBox_screenMode.Name = "comboBox_screenMode";
            this.comboBox_screenMode.Size = new System.Drawing.Size(142, 21);
            this.comboBox_screenMode.TabIndex = 25;
            // 
            // checkBox_spineRandomStart
            // 
            this.checkBox_spineRandomStart.AutoSize = true;
            this.checkBox_spineRandomStart.Location = new System.Drawing.Point(7, 21);
            this.checkBox_spineRandomStart.Name = "checkBox_spineRandomStart";
            this.checkBox_spineRandomStart.Size = new System.Drawing.Size(96, 17);
            this.checkBox_spineRandomStart.TabIndex = 27;
            this.checkBox_spineRandomStart.Text = "Random Start";
            // 
            // groupBox_spine
            // 
            this.groupBox_spine.Controls.Add(this.label11);
            this.groupBox_spine.Controls.Add(this.comboBox_spineAnimation);
            this.groupBox_spine.Controls.Add(this.checkBox_spineRandomStart);
            this.groupBox_spine.Location = new System.Drawing.Point(9, 336);
            this.groupBox_spine.Name = "groupBox_spine";
            this.groupBox_spine.Size = new System.Drawing.Size(487, 79);
            this.groupBox_spine.TabIndex = 31;
            this.groupBox_spine.TabStop = false;
            this.groupBox_spine.Text = "Spine";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 45);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(63, 13);
            this.label11.TabIndex = 32;
            this.label11.Text = "Animation:";
            // 
            // comboBox_spineAnimation
            // 
            this.comboBox_spineAnimation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_spineAnimation.FormattingEnabled = true;
            this.comboBox_spineAnimation.Location = new System.Drawing.Point(75, 42);
            this.comboBox_spineAnimation.Name = "comboBox_spineAnimation";
            this.comboBox_spineAnimation.Size = new System.Drawing.Size(153, 21);
            this.comboBox_spineAnimation.TabIndex = 31;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.trackBar_parallaxY);
            this.groupBox1.Controls.Add(this.trackBar_parallaxX);
            this.groupBox1.Controls.Add(this.ryBox);
            this.groupBox1.Controls.Add(this.rxBox);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Location = new System.Drawing.Point(9, 196);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(487, 134);
            this.groupBox1.TabIndex = 32;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parallax";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(164, 114);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(281, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "Further <<<<<< Parallax distance  >>>>>>> Closer";
            // 
            // trackBar_parallaxY
            // 
            this.trackBar_parallaxY.LargeChange = 1;
            this.trackBar_parallaxY.Location = new System.Drawing.Point(95, 66);
            this.trackBar_parallaxY.Maximum = 200;
            this.trackBar_parallaxY.Minimum = -200;
            this.trackBar_parallaxY.Name = "trackBar_parallaxY";
            this.trackBar_parallaxY.Size = new System.Drawing.Size(378, 45);
            this.trackBar_parallaxY.TabIndex = 19;
            this.trackBar_parallaxY.Scroll += new System.EventHandler(this.trackBar_parallaxY_Scroll);
            // 
            // trackBar_parallaxX
            // 
            this.trackBar_parallaxX.LargeChange = 1;
            this.trackBar_parallaxX.Location = new System.Drawing.Point(95, 15);
            this.trackBar_parallaxX.Maximum = 200;
            this.trackBar_parallaxX.Minimum = -200;
            this.trackBar_parallaxX.Name = "trackBar_parallaxX";
            this.trackBar_parallaxX.Size = new System.Drawing.Size(378, 45);
            this.trackBar_parallaxX.TabIndex = 18;
            this.trackBar_parallaxX.Scroll += new System.EventHandler(this.trackBar_parallaxX_Scroll);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.flipCheck);
            this.groupBox2.Controls.Add(this.zInput);
            this.groupBox2.Controls.Add(this.xInput);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.yInput);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(9, 41);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(228, 155);
            this.groupBox2.TabIndex = 33;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Position";
            // 
            // flipCheck
            // 
            this.flipCheck.Location = new System.Drawing.Point(11, 77);
            this.flipCheck.Name = "flipCheck";
            this.flipCheck.Size = new System.Drawing.Size(104, 24);
            this.flipCheck.TabIndex = 7;
            this.flipCheck.Text = "Flip";
            this.flipCheck.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.comboBox_screenMode);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.typeBox);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.alphaBox);
            this.groupBox3.Controls.Add(this.front);
            this.groupBox3.Controls.Add(this.cxBox);
            this.groupBox3.Controls.Add(this.cxLabel);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.cyBox);
            this.groupBox3.Controls.Add(this.copyLabel);
            this.groupBox3.Controls.Add(this.cyLabel);
            this.groupBox3.Location = new System.Drawing.Point(254, 41);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(242, 155);
            this.groupBox3.TabIndex = 34;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Etc";
            // 
            // BackgroundInstanceEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(503, 453);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox_spine);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.pathLabel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BackgroundInstanceEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Background";
            ((System.ComponentModel.ISupportInitialize) (this.xInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.yInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.zInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.alphaBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.ryBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.rxBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.cyBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.cxBox)).EndInit();
            this.groupBox_spine.ResumeLayout(false);
            this.groupBox_spine.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) (this.trackBar_parallaxY)).EndInit();
            ((System.ComponentModel.ISupportInitialize) (this.trackBar_parallaxX)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.CheckBox flipCheck;

        #endregion

        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown xInput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown yInput;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown zInput;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox typeBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown alphaBox;
        private System.Windows.Forms.CheckBox front;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown ryBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown rxBox;
        private System.Windows.Forms.Label copyLabel;
        private System.Windows.Forms.Label cyLabel;
        private System.Windows.Forms.NumericUpDown cyBox;
        private System.Windows.Forms.Label cxLabel;
        private System.Windows.Forms.NumericUpDown cxBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox comboBox_screenMode;
        private System.Windows.Forms.CheckBox checkBox_spineRandomStart;
        private System.Windows.Forms.GroupBox groupBox_spine;
        private System.Windows.Forms.ComboBox comboBox_spineAnimation;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TrackBar trackBar_parallaxY;
        private System.Windows.Forms.TrackBar trackBar_parallaxX;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label8;
    }
}