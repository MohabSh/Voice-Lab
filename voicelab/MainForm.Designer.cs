using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace voicelab
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Top Toolbar ───────────────────────────────────────────
        private Panel panelToolbar;
        private Button btnOpen;
        private Button btnPlay;
        private Button btnStop;
        private Button btnReset;
        private Button btnSave;

        // ── Left Column ───────────────────────────────────────────
        private Panel panelLeft;

        // File Info
        private Panel panelFileInfo;
        private Label lblSectionFile;
        private Label lblFileName;
        private Label lblSize;
        private Label lblDuration;
        private Label lblSampleRate;
        private Label lblChannels;
        private Label lblBitRate;
        private Label lblEncoding;

        // Settings
        private Panel panelSettings;
        private Label lblSectionSettings;
        private Label lblAlgoHint;
        private ComboBox cmbAlgorithms;
        private Label lblSrHint;
        private ComboBox cmbSampleRate;
        private Label lblQHint;
        private ComboBox cmbQuantization;
        private Button btnCompress;
        private Button btnDecompress;
        private Button btnCancel;

        // Progress
        private Panel panelProgress;
        private Label lblSectionProgress;
        private ProgressBar progressBar;
        private Label lblProgressPercent;
        private Label lblProgressStatus;

        // Results
        private Panel panelResults;
        private Label lblSectionResults;
        private Label lblCompressedSize;
        private Label lblRatio;

        // ── Right Column – Charts ─────────────────────────────────
        private Panel panelRight;
        private Panel panelCharts;
        private Label lblSectionCharts;
        private Label lblChartRatioTitle;
        private Panel panelChartRatio;
        private Label lblChartSpeedTitle;
        private Panel panelChartSpeed;

        // ── Color palette ─────────────────────────────────────────
        private static readonly Color ColBg = Color.FromArgb(15, 17, 23);
        private static readonly Color ColSurface = Color.FromArgb(22, 25, 35);
        private static readonly Color ColSurface2 = Color.FromArgb(30, 34, 48);
        private static readonly Color ColBorder = Color.FromArgb(45, 50, 70);
        private static readonly Color ColAccentBlue = Color.FromArgb(64, 156, 255);
        private static readonly Color ColAccentGreen = Color.FromArgb(52, 211, 153);
        private static readonly Color ColAccentRed = Color.FromArgb(248, 81, 73);
        private static readonly Color ColAccentPurple = Color.FromArgb(167, 139, 250);
        private static readonly Color ColTextPrimary = Color.FromArgb(230, 235, 245);
        private static readonly Color ColTextMuted = Color.FromArgb(100, 110, 140);

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ── Toolbar ───────────────────────────────────────────
            panelToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ColSurface,
                Padding = new Padding(0),
            };

            // App title label in toolbar
            var lblAppTitle = new Label
            {
                Text = "🎵  VOICELAB",
                Font = new Font("Consolas", 13f, FontStyle.Bold),
                ForeColor = ColAccentBlue,
                Location = new Point(16, 18),
                AutoSize = true,
            };
            panelToolbar.Controls.Add(lblAppTitle);

            btnOpen = MakeToolBtn("📂  Open", ColAccentBlue, new Point(170, 10));
            btnPlay = MakeToolBtn("▶  Play", ColAccentGreen, new Point(306, 10));
            btnStop = MakeToolBtn("■  Stop", ColAccentRed, new Point(442, 10));
            btnReset = MakeToolBtn("↺  Reset", ColTextMuted, new Point(578, 10));
            btnSave = MakeToolBtn("💾  Save", ColAccentPurple, new Point(714, 10));

            btnOpen.Click += btnOpen_Click;
            btnPlay.Click += btnPlay_Click;
            btnStop.Click += btnStop_Click;
            btnReset.Click += btnReset_Click;
            btnSave.Click += btnSave_Click;

            panelToolbar.Controls.AddRange(new Control[]
                { btnOpen, btnPlay, btnStop, btnReset, btnSave });

            // ── Separator under toolbar ───────────────────────────
            var sep = new Panel
            {
                Dock = DockStyle.Top,
                Height = 2,
                BackColor = ColBorder,
            };

            // ── Left Column ───────────────────────────────────────
            panelLeft = new Panel
            {
                Location = new Point(12, 74),
                Size = new Size(400, 630),
                BackColor = Color.Transparent,
            };

            // -- File Info Card --
            panelFileInfo = MakeCard(new Point(0, 0), new Size(400, 192));

            lblSectionFile = MakeSectionLabel("FILE INFORMATION", new Point(16, 14));
            panelFileInfo.Controls.Add(lblSectionFile);

            // Horizontal rule inside card
            var hrFile = MakeHR(new Point(16, 34), 368);
            panelFileInfo.Controls.Add(hrFile);

            lblFileName = MakeDataLabel("File: —", new Point(16, 44), 368);
            lblSize = MakeDataLabel("Size: —", new Point(16, 68), 175);
            lblDuration = MakeDataLabel("Duration: —", new Point(209, 68), 175);
            lblSampleRate = MakeDataLabel("Sample Rate: —", new Point(16, 92), 175);
            lblChannels = MakeDataLabel("Channels: —", new Point(209, 92), 175);
            lblBitRate = MakeDataLabel("Bit Rate: —", new Point(16, 116), 175);
            lblEncoding = MakeDataLabel("Encoding: —", new Point(209, 116), 175);

            foreach (var l in new[]{ lblFileName, lblSize, lblDuration,
                                     lblSampleRate, lblChannels, lblBitRate, lblEncoding })
                panelFileInfo.Controls.Add(l);

            // -- Settings Card --
            panelSettings = MakeCard(new Point(0, 204), new Size(400, 172));

            lblSectionSettings = MakeSectionLabel("COMPRESSION SETTINGS", new Point(16, 14));
            panelSettings.Controls.Add(lblSectionSettings);
            panelSettings.Controls.Add(MakeHR(new Point(16, 34), 368));

            lblAlgoHint = MakeHintLabel("Algorithm", new Point(16, 44));
            cmbAlgorithms = MakeDarkCombo(new Point(16, 62), new Size(222, 30));
            cmbAlgorithms.Items.AddRange(new string[]
            {
                "DPCM",
                "Delta Modulation",
                "Nonlinear Quantization",
                "Predictive Differential Coding",
                "Adaptive Delta Modulation",
            });
            cmbAlgorithms.SelectedIndex = 0;

            lblSrHint = MakeHintLabel("Sample Rate (Hz)", new Point(252, 44));
            cmbSampleRate = MakeDarkCombo(new Point(252, 62), new Size(132, 30));
            cmbSampleRate.Items.AddRange(new string[] { "8000", "16000", "22050", "44100", "48000" });
            cmbSampleRate.SelectedIndex = 3;

            lblQHint = MakeHintLabel("Quantization Levels", new Point(16, 104));
            cmbQuantization = MakeDarkCombo(new Point(16, 122), new Size(100, 30));
            cmbQuantization.Items.AddRange(new string[] { "8", "16", "32", "64", "128", "256" });
            cmbQuantization.SelectedIndex = 3;

            btnCompress = MakeActionBtn("⚙  Compress", ColAccentBlue, new Point(144, 122), new Size(126, 32));
            btnDecompress = MakeActionBtn("↩  Decompress", ColAccentGreen, new Point(276, 122), new Size(108, 32));
            btnCancel = MakeActionBtn("✕  Cancel", ColAccentRed, new Point(276, 84), new Size(108, 32));
            btnCancel.Enabled = false;

            btnCompress.Click += btnCompress_Click;
            btnDecompress.Click += btnDecompress_Click;
            btnCancel.Click += btnCancel_Click;

            panelSettings.Controls.AddRange(new Control[]
            {
                lblAlgoHint, cmbAlgorithms,
                lblSrHint,   cmbSampleRate,
                lblQHint,    cmbQuantization,
                btnCompress, btnDecompress, btnCancel,
            });

            // -- Progress Card --
            panelProgress = MakeCard(new Point(0, 388), new Size(400, 110));

            lblSectionProgress = MakeSectionLabel("PROGRESS", new Point(16, 14));
            panelProgress.Controls.Add(lblSectionProgress);
            panelProgress.Controls.Add(MakeHR(new Point(16, 34), 368));

            progressBar = new ProgressBar
            {
                Location = new Point(16, 46),
                Size = new Size(296, 14),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous,
                BackColor = ColBorder,
                ForeColor = ColAccentBlue,
            };

            lblProgressPercent = new Label
            {
                Location = new Point(320, 44),
                Size = new Size(64, 18),
                Text = "0%",
                Font = new Font("Consolas", 9f, FontStyle.Bold),
                ForeColor = ColAccentBlue,
                TextAlign = ContentAlignment.MiddleLeft,
            };

            lblProgressStatus = new Label
            {
                Location = new Point(16, 68),
                Size = new Size(370, 16),
                Text = "Ready",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ColTextMuted,
            };

            panelProgress.Controls.AddRange(new Control[]
                { progressBar, lblProgressPercent, lblProgressStatus });

            // -- Results Card --
            panelResults = MakeCard(new Point(0, 510), new Size(400, 60));

            lblSectionResults = MakeSectionLabel("RESULTS", new Point(16, 14));
            lblCompressedSize = MakeDataLabel("Compressed Size: —", new Point(16, 36), 186);
            lblRatio = MakeDataLabel("Ratio: —", new Point(210, 36), 180);
            lblCompressedSize.ForeColor = ColAccentGreen;
            lblRatio.ForeColor = ColAccentBlue;

            panelResults.Controls.AddRange(new Control[]
                { lblSectionResults, lblCompressedSize, lblRatio });

            // Add all left panels
            panelLeft.Controls.AddRange(new Control[]
                { panelFileInfo, panelSettings, panelProgress, panelResults });

            // ── Right Column – Charts ─────────────────────────────
            panelRight = new Panel
            {
                Location = new Point(424, 74),
                Size = new Size(420, 630),
                BackColor = Color.Transparent,
            };

            panelCharts = MakeCard(new Point(0, 0), new Size(420, 580));

            lblSectionCharts = MakeSectionLabel("REAL-TIME PERFORMANCE", new Point(16, 14));
            panelCharts.Controls.Add(lblSectionCharts);
            panelCharts.Controls.Add(MakeHR(new Point(16, 34), 388));

            lblChartRatioTitle = new Label
            {
                Text = "Compression Ratio  (x)",
                Location = new Point(16, 44),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ColAccentBlue,
            };

            panelChartRatio = new Panel
            {
                Location = new Point(16, 64),
                Size = new Size(388, 224),
                BackColor = Color.FromArgb(18, 22, 32),
            };
            panelChartRatio.Paint += PanelChartRatio_Paint;

            lblChartSpeedTitle = new Label
            {
                Text = "Processing Speed  (K samples/sec)",
                Location = new Point(16, 304),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ColAccentGreen,
            };

            panelChartSpeed = new Panel
            {
                Location = new Point(16, 324),
                Size = new Size(388, 224),
                BackColor = Color.FromArgb(18, 24, 22),
            };
            panelChartSpeed.Paint += PanelChartSpeed_Paint;

            panelCharts.Controls.AddRange(new Control[]
            {
                lblChartRatioTitle, panelChartRatio,
                lblChartSpeedTitle, panelChartSpeed,
            });

            panelRight.Controls.Add(panelCharts);

            // ── Form assembly ──────────────────────────────────────
            SuspendLayout();

            Controls.AddRange(new Control[]
            {
                panelToolbar,
                sep,
                panelLeft,
                panelRight,
            });

            AllowDrop = true;
            ClientSize = new Size(856, 716);
            Text = "VoiceLab – Audio Compressor";
            BackColor = ColBg;
            Font = new Font("Segoe UI", 9f);
            MinimumSize = new Size(872, 755);

            ResumeLayout(false);
        }

        // ── Chart Paint Handlers ──────────────────────────────────
        private void PanelChartRatio_Paint(object sender, PaintEventArgs e)
            => DrawLineChart(e.Graphics, (Panel)sender,
                             ratioPoints, ColAccentBlue, "x");

        private void PanelChartSpeed_Paint(object sender, PaintEventArgs e)
            => DrawLineChart(e.Graphics, (Panel)sender,
                             speedPoints, ColAccentGreen, "K");

        private static void DrawLineChart(
            Graphics g, Panel panel,
            System.Collections.Generic.List<double> data,
            Color lineColor, string unit)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, panel.Width, panel.Height);

            using var bgBrush = new SolidBrush(panel.BackColor);
            g.FillRectangle(bgBrush, rect);

            if (data == null || data.Count < 2)
            {
                using var hint = new SolidBrush(Color.FromArgb(55, 65, 90));
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("Waiting for data…",
                    new Font("Segoe UI", 8.5f), hint,
                    new RectangleF(0, 0, panel.Width, panel.Height), fmt);
                return;
            }

            const int padL = 48, padR = 12, padT = 12, padB = 20;
            double max = double.MinValue, min = double.MaxValue;
            foreach (var v in data) { if (v > max) max = v; if (v < min) min = v; }
            if (max - min < 0.001) { max = min + 1; }

            float w = panel.Width - padL - padR;
            float h = panel.Height - padT - padB;

            // Grid
            using var gridPen = new Pen(Color.FromArgb(35, 40, 58), 1f);
            using var gridPenHL = new Pen(Color.FromArgb(48, 55, 78), 1f) { DashStyle = DashStyle.Dot };

            for (int i = 0; i <= 4; i++)
            {
                float y = padT + h * i / 4f;
                g.DrawLine(i == 4 ? gridPen : gridPenHL, padL, y, padL + w, y);
                double val = max - (max - min) * i / 4.0;
                using var tb = new SolidBrush(Color.FromArgb(80, 90, 120));
                g.DrawString($"{val:F1}", new Font("Consolas", 6.5f), tb,
                    new PointF(2, y - 7));
            }

            // X axis
            g.DrawLine(gridPen, padL, padT + h, padL + w, padT + h);

            // Gradient fill under line
            var pts = new PointF[data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                float x = padL + w * i / Math.Max(1, data.Count - 1);
                float y = padT + h * (float)(1.0 - (data[i] - min) / (max - min));
                pts[i] = new PointF(x, y);
            }

            // Fill polygon
            var fill = new PointF[data.Count + 2];
            fill[0] = new PointF(pts[0].X, padT + h);
            for (int i = 0; i < data.Count; i++) fill[i + 1] = pts[i];
            fill[^1] = new PointF(pts[^1].X, padT + h);

            var alphaLine = Color.FromArgb(30, lineColor.R, lineColor.G, lineColor.B);
            using var fillBrush = new LinearGradientBrush(
                new PointF(0, padT), new PointF(0, padT + h),
                Color.FromArgb(60, lineColor.R, lineColor.G, lineColor.B),
                Color.FromArgb(0, lineColor.R, lineColor.G, lineColor.B));
            g.FillPolygon(fillBrush, fill);

            // Main line
            using var linePen = new Pen(lineColor, 2f);
            g.DrawLines(linePen, pts);

            // Last value dot + label
            var last = pts[^1];
            using var dotBrush = new SolidBrush(lineColor);
            g.FillEllipse(dotBrush, last.X - 4, last.Y - 4, 8, 8);
            using var glowPen = new Pen(Color.FromArgb(60, lineColor.R, lineColor.G, lineColor.B), 8f);
            g.DrawEllipse(glowPen, last.X - 6, last.Y - 6, 12, 12);

            using var valBrush = new SolidBrush(ColTextPrimary);
            g.DrawString($"{data[^1]:F2}{unit}",
                new Font("Consolas", 7.5f, FontStyle.Bold),
                valBrush,
                new PointF(last.X + 8, last.Y - 9));

            // Unit label bottom-right
            using var unitBrush = new SolidBrush(Color.FromArgb(50, 60, 80));
            g.DrawString(unit, new Font("Consolas", 8f, FontStyle.Bold), unitBrush,
                new PointF(padL + w - 16, padT + h - 16));
        }

        // ── Helpers ───────────────────────────────────────────────

        private static Panel MakeCard(Point loc, Size size)
        {
            var p = new Panel
            {
                Location = loc,
                Size = size,
                BackColor = ColSurface,
                BorderStyle = BorderStyle.None,
            };
            p.Paint += CardPaint;
            return p;
        }

        private static void CardPaint(object sender, PaintEventArgs e)
        {
            var p = (Panel)sender;
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(ColBorder, 1f);
            using var path = RoundedRect(rect, 10);
            e.Graphics.DrawPath(pen, path);
        }

        private static Button MakeToolBtn(string text, Color accent, Point loc)
        {
            var b = new Button
            {
                Text = text,
                Location = loc,
                Size = new Size(126, 40),
                BackColor = Color.FromArgb(30, 34, 48),
                ForeColor = accent,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            b.FlatAppearance.BorderColor = accent;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Min(255, accent.R / 4 + 22),
                Math.Min(255, accent.G / 4 + 25),
                Math.Min(255, accent.B / 4 + 35));
            return b;
        }

        private static Button MakeActionBtn(string text, Color accent, Point loc, Size size)
        {
            var b = new Button
            {
                Text = text,
                Location = loc,
                Size = size,
                BackColor = accent,
                ForeColor = Color.FromArgb(10, 12, 18),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private static Label MakeSectionLabel(string text, Point loc)
        {
            return new Label
            {
                Text = text,
                Location = loc,
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = ColTextMuted,
                BackColor = Color.Transparent,
            };
        }

        private static Label MakeHintLabel(string text, Point loc)
        {
            return new Label
            {
                Text = text,
                Location = loc,
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = ColTextMuted,
                BackColor = Color.Transparent,
            };
        }

        private static Label MakeDataLabel(string text, Point loc, int width)
        {
            return new Label
            {
                Text = text,
                Location = loc,
                Size = new Size(width, 20),
                Font = new Font("Segoe UI", 9f),
                ForeColor = ColTextPrimary,
                BackColor = Color.Transparent,
            };
        }

        private static Panel MakeHR(Point loc, int width)
        {
            return new Panel
            {
                Location = loc,
                Size = new Size(width, 1),
                BackColor = ColBorder,
            };
        }

        private static ComboBox MakeDarkCombo(Point loc, Size size)
        {
            return new ComboBox
            {
                Location = loc,
                Size = size,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f),
                BackColor = ColSurface2,
                ForeColor = ColTextPrimary,
                FlatStyle = FlatStyle.Flat,
            };
        }

        private static GraphicsPath RoundedRect(Rectangle b, int r)
        {
            var path = new GraphicsPath();
            path.AddArc(b.X, b.Y, r, r, 180, 90);
            path.AddArc(b.Right - r, b.Y, r, r, 270, 90);
            path.AddArc(b.Right - r, b.Bottom - r, r, r, 0, 90);
            path.AddArc(b.X, b.Bottom - r, r, r, 90, 90);
            path.CloseFigure();
            return path;
        }

        // Keep color accessible for DrawLineChart (static)
         private static readonly Color ColBorder2 = Color.FromArgb(45, 50, 70);
    }
}