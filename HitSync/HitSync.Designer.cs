
namespace HitSync
{
    partial class HitSync
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.EntertainmentToggleButton = new System.Windows.Forms.Button();
            this.scottPlot = new ScottPlot.FormsPlot();
            this.plotTimer = new System.Windows.Forms.Timer(this.components);
            this.ChangeModeButton = new System.Windows.Forms.Button();
            this.CurrentModeLabel = new System.Windows.Forms.Label();
            this.M1ColorPicker1 = new System.Windows.Forms.ColorDialog();
            this.M1ColorPicker2 = new System.Windows.Forms.ColorDialog();
            this.ColorPickerButton = new System.Windows.Forms.Button();
            this.M1ColorPicker3 = new System.Windows.Forms.ColorDialog();
            this.M1ColorPicker4 = new System.Windows.Forms.ColorDialog();
            this.M0ColorPicker1 = new System.Windows.Forms.ColorDialog();
            this.M1ColorPicker5 = new System.Windows.Forms.ColorDialog();
            this.M1ColorPicker7 = new System.Windows.Forms.ColorDialog();
            this.M1ColorPicker8 = new System.Windows.Forms.ColorDialog();
            this.M1ColorPicker6 = new System.Windows.Forms.ColorDialog();
            this.M0ColorPicker2 = new System.Windows.Forms.ColorDialog();
            this.ResetAudioButton = new System.Windows.Forms.Button();
            this.ColorRangePanel = new System.Windows.Forms.Panel();
            this.ResetColorToDefaultButton = new System.Windows.Forms.Button();
            this.ColorPointCountControl = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.ColorPointCountControl)).BeginInit();
            this.SuspendLayout();
            // 
            // EntertainmentToggleButton
            // 
            this.EntertainmentToggleButton.Location = new System.Drawing.Point(12, 12);
            this.EntertainmentToggleButton.Name = "EntertainmentToggleButton";
            this.EntertainmentToggleButton.Size = new System.Drawing.Size(87, 27);
            this.EntertainmentToggleButton.TabIndex = 0;
            this.EntertainmentToggleButton.Text = "Start";
            this.EntertainmentToggleButton.UseVisualStyleBackColor = true;
            this.EntertainmentToggleButton.Click += new System.EventHandler(this.EntertainmentToggleButton_Click);
            // 
            // scottPlot
            // 
            this.scottPlot.Location = new System.Drawing.Point(13, 78);
            this.scottPlot.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.scottPlot.Name = "scottPlot";
            this.scottPlot.Size = new System.Drawing.Size(775, 360);
            this.scottPlot.TabIndex = 1;
            // 
            // plotTimer
            // 
            this.plotTimer.Interval = 50;
            // 
            // ChangeModeButton
            // 
            this.ChangeModeButton.Location = new System.Drawing.Point(105, 45);
            this.ChangeModeButton.Name = "ChangeModeButton";
            this.ChangeModeButton.Size = new System.Drawing.Size(139, 27);
            this.ChangeModeButton.TabIndex = 2;
            this.ChangeModeButton.Text = "Next Mode";
            this.ChangeModeButton.UseVisualStyleBackColor = true;
            this.ChangeModeButton.Click += new System.EventHandler(this.ChangeModeButton_Click);
            // 
            // CurrentModeLabel
            // 
            this.CurrentModeLabel.AutoSize = true;
            this.CurrentModeLabel.Location = new System.Drawing.Point(105, 18);
            this.CurrentModeLabel.Name = "CurrentModeLabel";
            this.CurrentModeLabel.Size = new System.Drawing.Size(130, 15);
            this.CurrentModeLabel.TabIndex = 3;
            this.CurrentModeLabel.Text = "Current Mode: Colorful";
            // 
            // ColorPickerButton
            // 
            this.ColorPickerButton.Location = new System.Drawing.Point(253, 12);
            this.ColorPickerButton.Name = "ColorPickerButton";
            this.ColorPickerButton.Size = new System.Drawing.Size(75, 27);
            this.ColorPickerButton.TabIndex = 4;
            this.ColorPickerButton.Text = "Colors";
            this.ColorPickerButton.UseVisualStyleBackColor = true;
            this.ColorPickerButton.Click += new System.EventHandler(this.ColorPickerButton_Click);
            // 
            // ResetAudioButton
            // 
            this.ResetAudioButton.Location = new System.Drawing.Point(12, 45);
            this.ResetAudioButton.Name = "ResetAudioButton";
            this.ResetAudioButton.Size = new System.Drawing.Size(87, 27);
            this.ResetAudioButton.TabIndex = 5;
            this.ResetAudioButton.Text = "Reset Audio";
            this.ResetAudioButton.UseVisualStyleBackColor = true;
            // 
            // ColorRangePanel
            // 
            this.ColorRangePanel.Location = new System.Drawing.Point(334, 12);
            this.ColorRangePanel.Name = "ColorRangePanel";
            this.ColorRangePanel.Size = new System.Drawing.Size(454, 27);
            this.ColorRangePanel.TabIndex = 6;
            // 
            // ResetColorToDefaultButton
            // 
            this.ResetColorToDefaultButton.Location = new System.Drawing.Point(253, 45);
            this.ResetColorToDefaultButton.Name = "ResetColorToDefaultButton";
            this.ResetColorToDefaultButton.Size = new System.Drawing.Size(75, 27);
            this.ResetColorToDefaultButton.TabIndex = 7;
            this.ResetColorToDefaultButton.Text = "Reset Color";
            this.ResetColorToDefaultButton.UseVisualStyleBackColor = true;
            this.ResetColorToDefaultButton.Click += new System.EventHandler(this.ResetColorToDefaultButton_Click);
            // 
            // ColorPointCountControl
            // 
            this.ColorPointCountControl.Location = new System.Drawing.Point(334, 49);
            this.ColorPointCountControl.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.ColorPointCountControl.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.ColorPointCountControl.Name = "ColorPointCountControl";
            this.ColorPointCountControl.Size = new System.Drawing.Size(43, 23);
            this.ColorPointCountControl.TabIndex = 8;
            this.ColorPointCountControl.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.ColorPointCountControl.Visible = false;
            this.ColorPointCountControl.ValueChanged += new System.EventHandler(this.ColorPointCountControl_ValueChanged);
            // 
            // HitSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ColorPointCountControl);
            this.Controls.Add(this.ResetColorToDefaultButton);
            this.Controls.Add(this.ColorRangePanel);
            this.Controls.Add(this.ResetAudioButton);
            this.Controls.Add(this.ColorPickerButton);
            this.Controls.Add(this.CurrentModeLabel);
            this.Controls.Add(this.ChangeModeButton);
            this.Controls.Add(this.scottPlot);
            this.Controls.Add(this.EntertainmentToggleButton);
            this.Name = "HitSync";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.ColorPointCountControl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button EntertainmentToggleButton;
        private ScottPlot.FormsPlot scottPlot;
        private System.Windows.Forms.Timer plotTimer;
        private System.Windows.Forms.Button ChangeModeButton;
        private System.Windows.Forms.Label CurrentModeLabel;
        private System.Windows.Forms.ColorDialog M1ColorPicker1;
        private System.Windows.Forms.ColorDialog M1ColorPicker2;
        private System.Windows.Forms.Button ColorPickerButton;
        private System.Windows.Forms.ColorDialog M1ColorPicker3;
        private System.Windows.Forms.ColorDialog M1ColorPicker4;
        private System.Windows.Forms.ColorDialog M0ColorPicker1;
        private System.Windows.Forms.ColorDialog M1ColorPicker5;
        private System.Windows.Forms.ColorDialog M1ColorPicker7;
        private System.Windows.Forms.ColorDialog M1ColorPicker8;
        private System.Windows.Forms.ColorDialog M1ColorPicker6;
        private System.Windows.Forms.ColorDialog M0ColorPicker2;
        private System.Windows.Forms.Button ResetAudioButton;
        private System.Windows.Forms.Panel ColorRangePanel;
        private System.Windows.Forms.Button ResetColorToDefaultButton;
        private System.Windows.Forms.NumericUpDown ColorPointCountControl;
    }
}

