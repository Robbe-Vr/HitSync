using HitSync.Core;
using HitSync.Core.Animation;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HitSync
{
    public partial class HitSync : Form
    {
        private Dictionary<AnimationMode, List<ColorDialog>> ColorPointPickersPerMode = new Dictionary<AnimationMode, List<ColorDialog>>();

        public HitSync()
        {
            InitializeComponent();

            SignalPlot[] plots = new SignalPlot[]
            {
                scottPlot.Plot.AddSignal(new double[2048], sampleRate: 0.095, color: Color.DarkBlue),
                scottPlot.Plot.AddSignal(new double[2048], sampleRate: 0.095, color: Color.SkyBlue),
            };

            SignalPlot correctionPlot = scottPlot.Plot.AddSignal(new double[2048], sampleRate: 0.095, color: Color.DarkRed);

            scottPlot.Plot.SetAxisLimits(0, 22_000, -1, 5);

            scottPlot.Refresh();

            plotTimer.Tick += (sender, e) =>
            {
                double[][] data = HitSyncManager.FFTData;

                if (data == null) return;

                int channel = 0;
                foreach (double[] channelData in data)
                {
                    plots[channel].Ys = channelData;

                    channel++;
                }

                correctionPlot.Ys = HitSyncManager.AnimationMode == AnimationMode.Colorful ? HitSyncManager.Corrections : new double[2048];

                scottPlot.Refresh();
            };

            EntertainmentToggleButton.Enabled = false;
            EntertainmentToggleButton.Text = "Initializing";

            HitSyncManager.OnStarted += (sender, e) =>
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        EntertainmentToggleButton.Text = "Stop";
                        plotTimer.Start();

                        if (!EntertainmentToggleButton.Enabled)
                            EntertainmentToggleButton.Enabled = true;
                    }));
                }
                else
                {
                    EntertainmentToggleButton.Text = "Stop";
                    plotTimer.Start();

                    if (!EntertainmentToggleButton.Enabled)
                        EntertainmentToggleButton.Enabled = true;
                }
            };

            HitSyncManager.OnStopped += (sender, e) =>
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        EntertainmentToggleButton.Text = "Start";
                        plotTimer.Stop();

                        if (!EntertainmentToggleButton.Enabled)
                            EntertainmentToggleButton.Enabled = true;
                    }));
                }
                else
                {
                    EntertainmentToggleButton.Text = "Start";
                    plotTimer.Stop();

                    if (!EntertainmentToggleButton.Enabled)
                        EntertainmentToggleButton.Enabled = true;
                }
            };

            HitSyncManager.OnUpdated += OnUpdated;

            HitSyncManager.SetAnimationMode(AnimationMode.Intensity);
            int[][] intensityColorRange = HitSyncManager.GetDefaultColorRangeForAnimationMode(HitSyncManager.AnimationMode);

            HitSyncManager.SetColors(
                ColorRangeMode.Simple,
                intensityColorRange
            );

            ColorPointPickersPerMode.Add(AnimationMode.Intensity, new List<ColorDialog>());

            foreach (int[] colorPoint in intensityColorRange)
            {
                ColorDialog colorPicker = new ColorDialog();
                colorPicker.Color = Color.FromArgb(colorPoint[0], colorPoint[1], colorPoint[2]);

                ColorPointPickersPerMode[AnimationMode.Intensity].Add(colorPicker);
            }

            HitSyncManager.SetAnimationMode(AnimationMode.Colorful);
            int[][] defaultColorfulColorRange = HitSyncManager.GetDefaultColorRangeForAnimationMode(HitSyncManager.AnimationMode);

            int[][] defaultSetColorRange = HitSyncManager.SetColors(
                ColorRangeMode.FrequencyDependant,
                defaultColorfulColorRange[0],
                defaultColorfulColorRange[1],
                defaultColorfulColorRange[2],
                defaultColorfulColorRange[3],
                defaultColorfulColorRange[4],
                defaultColorfulColorRange[5],
                defaultColorfulColorRange[6],
                defaultColorfulColorRange[7]
            );

            UpdateColorRange(defaultSetColorRange);
        }

        internal void OnInitialized()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    EntertainmentToggleButton.Text = "Start";
                    EntertainmentToggleButton.Enabled = true;
                }));
            }
            else
            {
                EntertainmentToggleButton.Text = "Start";
                EntertainmentToggleButton.Enabled = true;
            }
        }

        internal void OnError()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    EntertainmentToggleButton.Text = "Error!";
                    EntertainmentToggleButton.Enabled = false;
                }));
            }
            else
            {
                EntertainmentToggleButton.Text = "Start";
                EntertainmentToggleButton.Enabled = true;
            }
        }

        internal void OnUpdated(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    CurrentModeLabel.Text = "Current Mode: " + HitSyncManager.AnimationMode.ToString();

                    if (!EntertainmentToggleButton.Enabled)
                    {
                        EntertainmentToggleButton.Enabled = true;
                    }

                    int[][] colorRange = HitSyncManager.GetCurrentColorRange();
                    UpdateColorRange(colorRange);
                }));
            }
            else
            {
                CurrentModeLabel.Text = "Current Mode: " + HitSyncManager.AnimationMode.ToString();

                if (!EntertainmentToggleButton.Enabled)
                {
                    EntertainmentToggleButton.Enabled = true;
                }

                int[][] colorRange = HitSyncManager.GetCurrentColorRange();
                UpdateColorRange(colorRange);
            }
        }

        private void ResetAudioButton_Click(object sender, EventArgs e)
        {
            Task.Run(() => HitSyncManager.ResetAudioDevice());
        }

        private void EntertainmentToggleButton_Click(object sender, EventArgs e)
        {
            if (HitSyncManager.Running)
            {
                EntertainmentToggleButton.Enabled = false;

                Task.Run(() =>
                {
                    if (HitSyncManager.Stop())
                    {
                        plotTimer.Stop();
                        EntertainmentToggleButton.Text = "Start";
                    }
                    EntertainmentToggleButton.Enabled = true;
                });
            }
            else
            {
                EntertainmentToggleButton.Enabled = false;

                Task.Run(() =>
                {
                    if (HitSyncManager.Start("Robbe"))
                    {
                        EntertainmentToggleButton.Text = "Stop";
                        plotTimer.Start();
                    }
                    EntertainmentToggleButton.Enabled = true;
                });
            }
        }

        private void ChangeModeButton_Click(object sender, EventArgs e)
        {
            AnimationMode[] modes = Enum.GetValues<AnimationMode>();

            int newMode = (int)HitSyncManager.AnimationMode + 1;

            if (newMode  >= modes.Length)
            {
                newMode = 0;
            }

            HitSyncManager.SetAnimationMode(modes[newMode]);
            
            if (ColorPointCountControl.Visible = HitSyncManager.AnimationMode != AnimationMode.Colorful)
            {
                ColorPointCountControl.Value = ColorPointPickersPerMode.ContainsKey(HitSyncManager.AnimationMode) ? ColorPointPickersPerMode[HitSyncManager.AnimationMode].Count : 2;
            }

            CurrentModeLabel.Text = "Current Mode: " + modes[newMode].ToString();

            int[][] colorRange = HitSyncManager.GetCurrentColorRange();
            UpdateColorRange(colorRange);
        }

        internal void ToggleForm(bool show)
        {
            if (show)
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { this.Show(); }));
                }
                else Show();
            }
            else
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { this.Hide(); }));
                }
                else Hide();
            }
        }

        private List<ColorDialog> M0ColorPickers = new List<ColorDialog>();

        private void ColorPickerButton_Click(object sender, EventArgs e)
        {
            int[][] colorRange = Array.Empty<int[]>();

            switch (HitSyncManager.AnimationMode)
            {
                case AnimationMode.Intensity:
                    List<int[]> M0ColorPoints = new List<int[]>();

                    for (int i = 0; i < ColorPointCountControl.Value; i++)
                    {
                        int[] M0color = new int[3];

                        if (M0ColorPickers[i].ShowDialog() == DialogResult.OK)
                        {
                            M0color[0] = M0ColorPickers[i].Color.R;
                            M0color[1] = M0ColorPickers[i].Color.G;
                            M0color[2] = M0ColorPickers[i].Color.B;
                        }

                        M0ColorPoints.Add(M0color);
                    }

                    colorRange = HitSyncManager.SetColors(ColorRangeMode.Simple, M0ColorPoints.ToArray());
                    break;

                case AnimationMode.Colorful:
                    int[] M1color1 = new int[3];
                    int[] M1color2 = new int[3];
                    int[] M1color3 = new int[3];
                    int[] M1color4 = new int[3];
                    int[] M1color5 = new int[3];
                    int[] M1color6 = new int[3];
                    int[] M1color7 = new int[3];
                    int[] M1color8 = new int[3];

                    if (M1ColorPicker1.ShowDialog() == DialogResult.OK)
                    {
                        M1color1[0] = M1ColorPicker1.Color.R;
                        M1color1[1] = M1ColorPicker1.Color.G;
                        M1color1[2] = M1ColorPicker1.Color.B;
                    }

                    if (M1ColorPicker2.ShowDialog() == DialogResult.OK)
                    {
                        M1color2[0] = M1ColorPicker2.Color.R;
                        M1color2[1] = M1ColorPicker2.Color.G;
                        M1color2[2] = M1ColorPicker2.Color.B;
                    }

                    if (M1ColorPicker3.ShowDialog() == DialogResult.OK)
                    {
                        M1color3[0] = M1ColorPicker3.Color.R;
                        M1color3[1] = M1ColorPicker3.Color.G;
                        M1color3[2] = M1ColorPicker3.Color.B;
                    }

                    if (M1ColorPicker4.ShowDialog() == DialogResult.OK)
                    {
                        M1color4[0] = M1ColorPicker4.Color.R;
                        M1color4[1] = M1ColorPicker4.Color.G;
                        M1color4[2] = M1ColorPicker4.Color.B;
                    }

                    if (M1ColorPicker5.ShowDialog() == DialogResult.OK)
                    {
                        M1color5[0] = M1ColorPicker5.Color.R;
                        M1color5[1] = M1ColorPicker5.Color.G;
                        M1color5[2] = M1ColorPicker5.Color.B;
                    }

                    if (M1ColorPicker6.ShowDialog() == DialogResult.OK)
                    {
                        M1color6[0] = M1ColorPicker6.Color.R;
                        M1color6[1] = M1ColorPicker6.Color.G;
                        M1color6[2] = M1ColorPicker6.Color.B;
                    }

                    if (M1ColorPicker7.ShowDialog() == DialogResult.OK)
                    {
                        M1color7[0] = M1ColorPicker7.Color.R;
                        M1color7[1] = M1ColorPicker7.Color.G;
                        M1color7[2] = M1ColorPicker7.Color.B;
                    }

                    if (M1ColorPicker8.ShowDialog() == DialogResult.OK)
                    {
                        M1color8[0] = M1ColorPicker8.Color.R;
                        M1color8[1] = M1ColorPicker8.Color.G;
                        M1color8[2] = M1ColorPicker8.Color.B;
                    }

                    colorRange = HitSyncManager.SetColors(ColorRangeMode.FrequencyDependant, M1color1, M1color2, M1color3, M1color4, M1color5, M1color6, M1color7, M1color8);
                    break;
            }

            UpdateColorRange(colorRange);
        }

        private void UpdateColorRange(int[][] range)
        {
            if (range == null || range.Length < 1) return;

            const double displayColorCount = 50.0;

            if (ColorRangePanel.Controls.Count == displayColorCount)
                ColorRangePanel.Controls.Clear();

            int step = (int)Math.Round(range.Length / displayColorCount);

            for (int i = 0; i < displayColorCount - 1; i++)
            {
                Panel color = new Panel();
                
                color.BackColor = Color.FromArgb(range[i * step][0], range[i * step][1], range[i * step][2]);
                
                color.Height = ColorRangePanel.Height;
                color.Width = ColorRangePanel.Width / (int)displayColorCount;

                color.Location = new Point(color.Width * i, 0);

                if (ColorRangePanel.Controls.Count == displayColorCount)
                    ColorRangePanel.Controls.RemoveAt(0);
                ColorRangePanel.Controls.Add(color);
            }

            Panel finalColor = new Panel();

            finalColor.BackColor = Color.FromArgb(range[^1][0], range[^1][1], range[^1][2]);

            finalColor.Height = ColorRangePanel.Height;
            finalColor.Width = ColorRangePanel.Width / (int)displayColorCount;

            finalColor.Location = new Point(finalColor.Width * (int)(displayColorCount - 1), 0);

            if (ColorRangePanel.Controls.Count == displayColorCount)
                ColorRangePanel.Controls.RemoveAt(0);
            ColorRangePanel.Controls.Add(finalColor);
        }

        private void ResetColorToDefaultButton_Click(object sender, EventArgs e)
        {
            int[][] defaultColorRange = HitSyncManager.GetDefaultColorRangeForAnimationMode(HitSyncManager.AnimationMode);

            int[][] colorRange = Array.Empty<int[]>();

            switch (HitSyncManager.AnimationMode)
            {
                case AnimationMode.Intensity:
                    colorRange = HitSyncManager.SetColors(ColorRangeMode.Simple, defaultColorRange);
                    break;

                case AnimationMode.Colorful:
                    colorRange = HitSyncManager.SetColors(ColorRangeMode.FrequencyDependant, defaultColorRange);
                    break;
            }

            UpdateColorRange(colorRange);
        }

        private void ColorPointCountControl_ValueChanged(object sender, EventArgs e)
        {
            switch (HitSyncManager.AnimationMode)
            {
                case AnimationMode.Intensity:
                    M0ColorPickers.ForEach(x => x.Dispose());
                    M0ColorPickers.Clear();

                    for (int i = 0; i < (int)ColorPointCountControl.Value; i++)
                    {
                        M0ColorPickers.Add(new ColorDialog());
                    }

                    break;
            }
        }
    }
}
