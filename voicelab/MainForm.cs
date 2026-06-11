using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AudioCompressor.Services;
using voicelab.Interface;

namespace voicelab
{
    public partial class MainForm : Form
    {
        // ── Services ──────────────────────────────────────────────
        private readonly AudioLoader loader = new();
        private readonly AudioPlayer player = new();

        // ── State ─────────────────────────────────────────────────
        private AudioFile currentAudio;
        private byte[] lastCompressedData;
        private short[] lastDecompressedSamples;
        private IAudioCompressionAlgorithm lastAlgorithm;
        private CompressionReport lastReport;

        // ── VLB state (set when a .vlb is loaded) ─────────────────
        private bool isVlbLoaded = false;
        private int vlbSampleRate = 44100;
        private int vlbChannels = 1;

        // ── Chart data ────────────────────────────────────────────
        internal readonly List<double> ratioPoints = new();
        internal readonly List<double> speedPoints = new();

        // ── Cancellation ──────────────────────────────────────────
        private CancellationTokenSource cts;

        public MainForm()
        {
            InitializeComponent();
            AllowDrop = true;
            DragEnter += MainForm_DragEnter;
            DragDrop += MainForm_DragDrop;
        }

        // ═══════════════════════════════════════════════════════════
        //  OPEN / LOAD
        // ═══════════════════════════════════════════════════════════
        private void btnOpen_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "All Supported|*.mp3;*.wav;*.aac;*.wma;*.vlb" +
                         "|Audio Files|*.mp3;*.wav;*.aac;*.wma" +
                         "|Compressed VLB|*.vlb",
                FilterIndex = 1,
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            if (Path.GetExtension(dlg.FileName).ToLowerInvariant() == ".vlb")
                LoadVlb(dlg.FileName);
            else
                LoadAudio(dlg.FileName);
        }

        private void LoadAudio(string path)
        {
            try
            {
                isVlbLoaded = false;
                currentAudio = loader.Load(path);

                lblFileName.Text = "📄  " + currentAudio.FileName;
                lblSize.Text = $"💾  {currentAudio.FileSize / 1024.0:F2} KB";
                lblDuration.Text = $"⏱  {currentAudio.Duration:hh\\:mm\\:ss}";
                lblSampleRate.Text = $"📶  {currentAudio.SampleRate} Hz";
                lblChannels.Text = $"🔊  {currentAudio.Channels} ch";
                lblBitRate.Text = $"⚡  {currentAudio.BitRate / 1000.0:F0} kbps";
                lblEncoding.Text = $"🔖  {currentAudio.EncodingType}";

                ResetResults();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading file:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Load a .vlb compressed file ───────────────────────────
        // ── Load a .vlb compressed file ───────────────────────────
private void LoadVlb(string path)
{
    try
    {
        var fileInfo = new FileInfo(path);
        using var br = new BinaryReader(File.OpenRead(path));

        // Read header
        byte[] magic = br.ReadBytes(4);
        if (magic[0] != 'V' || magic[1] != 'L' || magic[2] != 'B')
            throw new InvalidDataException("Not a valid VLB file.");

        int algoLen = br.ReadByte();
        string algoName = System.Text.Encoding.UTF8.GetString(br.ReadBytes(algoLen));
        int sampleRate = br.ReadInt32();
        int channels = br.ReadInt16();
        int quantLvls = br.ReadInt32();
        int sampleCount = br.ReadInt32();

        // Rest is compressed data
        lastCompressedData = br.ReadBytes((int)(fileInfo.Length - br.BaseStream.Position));

        // ✅ التعديل هنا: تمرير quantLvls إلى المصنع
        lastAlgorithm = CompressionFactory.Create(algoName, quantLvls);

        // Decompress immediately so we can play + show info
        lastDecompressedSamples = lastAlgorithm.Decompress(lastCompressedData);

        // Store VLB playback settings
        isVlbLoaded = true;
        vlbSampleRate = sampleRate;
        vlbChannels = channels;

        // Sync UI combos to saved settings
        SelectComboItem(cmbSampleRate, sampleRate.ToString());
        SelectComboItem(cmbQuantization, quantLvls.ToString());
        SelectComboItem(cmbAlgorithms, algoName);

        // Show file info
        double origKB = (double)sampleCount * 2 / 1024.0;
        double compKB = lastCompressedData.Length / 1024.0;
        double ratio = origKB > 0 ? origKB / compKB : 1;

        lblFileName.Text = "📦  " + Path.GetFileName(path) + "  [VLB]";
        lblSize.Text = $"💾  {compKB:F2} KB  (compressed)";
        lblDuration.Text = $"⏱  {TimeSpan.FromSeconds((double)sampleCount / sampleRate):hh\\:mm\\:ss}";
        lblSampleRate.Text = $"📶  {sampleRate} Hz";
        lblChannels.Text = $"🔊  {channels} ch";
        lblBitRate.Text = $"⚡  {algoName}";
        lblEncoding.Text = $"🔖  VLB / {algoName}";

        lblCompressedSize.Text = $"Compressed: {compKB:F2} KB";
        lblRatio.Text = $"Ratio: {ratio:F2}x";
        lblProgressStatus.Text = $"✅  VLB loaded — {sampleCount:N0} samples recovered";
        progressBar.Value = 100;
        lblProgressPercent.Text = "100%";
    }
    catch (Exception ex)
    {
        MessageBox.Show("Error loading VLB:\n" + ex.Message,
            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

// ═══════════════════════════════════════════════════════════
//  COMPRESS
// ═══════════════════════════════════════════════════════════
private async void btnCompress_Click(object sender, EventArgs e)
{
    if (currentAudio == null)
    {
        MessageBox.Show("Open an audio file first.", "Warning",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    string algoName = cmbAlgorithms.SelectedItem?.ToString();
    if (string.IsNullOrEmpty(algoName))
    {
        MessageBox.Show("Select an algorithm first.");
        return;
    }

    int sampleRate = int.Parse(cmbSampleRate.Text);
    int quantizationLevels = int.Parse(cmbQuantization.Text);

    SetUIBusy(true);
    ResetResults();

    // ✅ دايماً أنشئ CancellationTokenSource جديد
    cts?.Dispose();
    cts = new CancellationTokenSource();
    var token = cts.Token;

    try
    {
        lblProgressStatus.Text = "Decoding audio to PCM…";
        short[] samples = await Task.Run(
            () => AudioConverter.ToPCM(currentAudio.FilePath), token);

        if (token.IsCancellationRequested)
        {
            HandleCancellation();
            return;
        }

        // ✅ التعديل هنا: تمرير quantizationLevels إلى المصنع
        lastAlgorithm = CompressionFactory.Create(algoName, quantizationLevels);
        
        lblProgressStatus.Text = $"Compressing with {algoName}…";

        var sw = Stopwatch.StartNew();
        lastCompressedData = await Task.Run(
            () => CompressWithProgress(samples, lastAlgorithm, token), token);
        sw.Stop();

        // ✅ تحقق إذا تم الإلغاء (النتيجة null)
        if (lastCompressedData == null || token.IsCancellationRequested)
        {
            HandleCancellation();
            return;
        }

        double origBytes = samples.Length * sizeof(short);
        double compBytes = lastCompressedData.Length;
        double ratio = origBytes / compBytes;
        double saving = (1.0 - compBytes / origBytes) * 100.0;

        lastReport = new CompressionReport
        {
            Algorithm = algoName,
            SampleRate = sampleRate,
            QuantizationLevels = quantizationLevels,
            OriginalSizeKB = origBytes / 1024.0,
            CompressedSizeKB = compBytes / 1024.0,
            Ratio = ratio,
            SavingPercent = saving,
            ElapsedMs = sw.ElapsedMilliseconds,
            SampleCount = samples.Length,
            Channels = currentAudio.Channels,
            OriginalBitRate = currentAudio.BitRate,
            EncodingType = currentAudio.EncodingType,
        };

        lblCompressedSize.Text = $"Compressed: {compBytes / 1024.0:F2} KB";
        lblRatio.Text = $"Ratio: {ratio:F2}x  ({saving:F1}% saved)";
        lblProgressStatus.Text = "✅  Done!";
        progressBar.Value = 100;
        lblProgressPercent.Text = "100%";

        ShowReport(lastReport);
    }
    catch (OperationCanceledException)
    {
        HandleCancellation();
    }
    catch (Exception ex)
    {
        MessageBox.Show("Compression error:\n" + ex.Message,
            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        SetUIBusy(false);
    }
}
        private static void SelectComboItem(ComboBox cmb, string value)
        {
            int idx = cmb.Items.IndexOf(value);
            if (idx >= 0) cmb.SelectedIndex = idx;
        }

        // ═══════════════════════════════════════════════════════════
        //  PLAY / STOP / RESET
        // ═══════════════════════════════════════════════════════════
        private void btnPlay_Click(object sender, EventArgs e)
        {
            // If a VLB is loaded, play the decompressed samples from memory
            if (isVlbLoaded && lastDecompressedSamples != null)
            {
                player.PlayFromSamples(lastDecompressedSamples, vlbSampleRate, vlbChannels);
                return;
            }

            // Otherwise play the original audio file
            if (currentAudio != null)
                player.Play(currentAudio.FilePath);
        }

        private void btnStop_Click(object sender, EventArgs e) => player.Stop();

        private void btnReset_Click(object sender, EventArgs e)
        {
            player.Stop();
            isVlbLoaded = false;
            ResetResults();
            if (currentAudio != null) LoadAudio(currentAudio.FilePath);
        }

        private void ResetResults()
        {
            lastCompressedData = null;
            lastDecompressedSamples = null;
            lastAlgorithm = null;
            lastReport = null;

            ratioPoints.Clear();
            speedPoints.Clear();

            progressBar.Value = 0;
            lblProgressPercent.Text = "0%";
            lblProgressStatus.Text = "Ready";
            lblCompressedSize.Text = "Compressed Size: —";
            lblRatio.Text = "Ratio: —";

            panelChartRatio.Invalidate();
            panelChartSpeed.Invalidate();
        }

        // ═══════════════════════════════════════════════════════════
        //  COMPRESS
        // ═══════════════════════════════════════════════════════════
        
        // ✅ دالة مساعدة لمعالجة الإلغاء بشكل موحد
        private void HandleCancellation()
        {
            lblProgressStatus.Text = "❌  Cancelled by user";
            progressBar.Value = 0;
            lblProgressPercent.Text = "0%";
            lblCompressedSize.Text = "Compressed Size: —";
            lblRatio.Text = "Ratio: —";

            // تنظيف البيانات الجزئية
            lastCompressedData = null;
            lastDecompressedSamples = null;
            lastReport = null;

            ratioPoints.Clear();
            speedPoints.Clear();
            panelChartRatio.Invalidate();
            panelChartSpeed.Invalidate();
        }
        private byte[] CompressWithProgress(
      short[] samples,
      IAudioCompressionAlgorithm algo,
      CancellationToken token)
        {
            const int chunkSize = 2048;
            int totalChunks = (int)Math.Ceiling(samples.Length / (double)chunkSize);
            var output = new List<byte>(samples.Length);
            var sw = Stopwatch.StartNew();

            for (int c = 0; c < totalChunks; c++)
            {
                // ✅ تحقق أول الـ loop
                if (token.IsCancellationRequested)
                {
                    // بدلاً من throw، نرجع null للإشارة إلى الإلغاء
                    return null;
                }

                int start = c * chunkSize;
                int len = Math.Min(chunkSize, samples.Length - start);
                var chunk = new short[len];
                Array.Copy(samples, start, chunk, 0, len);

                var compressed = algo.Compress(chunk);
                output.AddRange(compressed);

                // ✅ تحقق ثاني بعد الضغط
                if (token.IsCancellationRequested)
                {
                    return null;
                }

                double pct = (c + 1.0) / totalChunks * 100.0;
                double cRatio = (double)(chunk.Length * sizeof(short))
                                    / Math.Max(1, compressed.Length);
                double elapsed = sw.Elapsed.TotalSeconds;
                double kSampSec = elapsed > 0
                    ? ((c + 1.0) * chunkSize) / elapsed / 1000.0
                    : 0;

                // ✅ تأكد إن الفورم ما زال موجوداً قبل Invoke
                if (!IsDisposed && IsHandleCreated)
                {
                    try
                    {
                        Invoke(() =>
                        {
                            if (!IsDisposed && IsHandleCreated)
                            {
                                progressBar.Value = (int)pct;
                                lblProgressPercent.Text = $"{(int)pct}%";
                                ratioPoints.Add(Math.Round(cRatio, 2));
                                speedPoints.Add(Math.Round(kSampSec, 1));
                                panelChartRatio.Invalidate();
                                panelChartSpeed.Invalidate();
                            }
                        });
                    }
                    catch (ObjectDisposedException)
                    {
                        // تم إغلاق النموذج - نتوقف عن التحديث
                        return null;
                    }
                }
            }

            return output.ToArray();
        }

        // ═══════════════════════════════════════════════════════════
        //  DECOMPRESS
        // ═══════════════════════════════════════════════════════════
        private void btnDecompress_Click(object sender, EventArgs e)
        {
            if (lastCompressedData == null || lastAlgorithm == null)
            {
                MessageBox.Show("Compress a file first (or load a .vlb file).", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                lastDecompressedSamples = lastAlgorithm.Decompress(lastCompressedData);
                MessageBox.Show(
                    $"✅  Decompression completed!\n\n" +
                    $"Recovered Samples: {lastDecompressedSamples.Length:N0}\n\n" +
                    $"You can now press ▶ Play to hear the result.",
                    "Decompress", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Decompression error:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  CANCEL
        // ═══════════════════════════════════════════════════════════
        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                cts?.Cancel();
                btnCancel.Enabled = false; // تعطيل زر الإلغاء فوراً
                lblProgressStatus.Text = "Cancelling...";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cancel error: {ex.Message}");
            }
        }
        // ═══════════════════════════════════════════════════════════
        //  SAVE
        // ═══════════════════════════════════════════════════════════
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (lastCompressedData == null)
            {
                MessageBox.Show("Nothing to save. Compress a file first.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Title = "Save Audio",
                Filter = "WAV File (decompressed PCM)|*.wav|Compressed VLB File|*.vlb",
                FileName = Path.GetFileNameWithoutExtension(
                               currentAudio?.FileName ?? "output") + "_decompressed",
                FilterIndex = 1  // جعل WAV هو الخيار الافتراضي
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                // الخيار 1: حفظ كـ WAV (ملف مفكوك - غير مضغوط)
                if (dlg.FilterIndex == 1)
                {
                    // التأكد من وجود البيانات المفكوكة
                    if (lastDecompressedSamples == null)
                    {
                        MessageBox.Show(
                            "Please decompress the file first before saving as WAV.\n\n" +
                            "Click 'Decompress' button first, then try saving again.",
                            "Warning",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    int sampleRate;

                    if (isVlbLoaded)
                    {
                        sampleRate = vlbSampleRate;
                    }
                    else
                    {
                        sampleRate = currentAudio?.SampleRate ?? 44100;
                    }

                    AudioConverter.SaveAsWav(
                        lastDecompressedSamples,
                        dlg.FileName,
                        sampleRate);

                    MessageBox.Show(
                        $"✅ Decompressed WAV file saved successfully!\n\n" +
                        $"File: {dlg.FileName}\n" +
                        $"Recovered Samples: {lastDecompressedSamples.Length:N0}\n" +
                        $"Sample Rate: {sampleRate} Hz\n" +
                        $"This is the DECOMPRESSED (uncompressed) audio.",
                        "Save Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                // الخيار 2: حفظ كـ VLB (ملف مضغوط)
                else
                {
                    string algoName = lastAlgorithm.Name;

                    int sampleRate = currentAudio != null
                        ? currentAudio.SampleRate
                        : int.Parse(cmbSampleRate.Text);

                    int quantLevels = int.Parse(cmbQuantization.Text);

                    using var ms = new MemoryStream();
                    using var bw = new BinaryWriter(ms);

                    bw.Write(new byte[] { (byte)'V', (byte)'L', (byte)'B', 0 });
                    byte[] algoBytes = System.Text.Encoding.UTF8.GetBytes(algoName);
                    bw.Write((byte)algoBytes.Length);
                    bw.Write(algoBytes);
                    bw.Write(sampleRate);
                    bw.Write((short)(currentAudio?.Channels ?? 1));
                    bw.Write(quantLevels);
                    bw.Write(lastReport?.SampleCount ?? 0);
                    bw.Write(lastCompressedData);

                    File.WriteAllBytes(dlg.FileName, ms.ToArray());

                    MessageBox.Show(
                        $"✅ Compressed VLB file saved!\n\n" +
                        $"File: {dlg.FileName}\n" +
                        $"Original PCM: {lastReport?.OriginalSizeKB:F2} KB\n" +
                        $"VLB size: {ms.Length / 1024.0:F2} KB\n" +
                        $"Ratio: {lastReport?.Ratio:F2}x\n\n" +
                        $"This is the COMPRESSED audio.",
                        "Save Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save error:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  REPORT
        // ═══════════════════════════════════════════════════════════
        private static void ShowReport(CompressionReport r)
        {
            MessageBox.Show(
                $"╔══════════════════════════════════════╗\n" +
                $"║         COMPRESSION REPORT           ║\n" +
                $"╠══════════════════════════════════════╣\n" +
                $"  Algorithm          : {r.Algorithm}\n" +
                $"  Encoding Type      : {r.EncodingType}\n" +
                $"──────────────────────────────────────\n" +
                $"  Sample Rate        : {r.SampleRate:N0} Hz\n" +
                $"  Channels           : {r.Channels}\n" +
                $"  Quant. Levels      : {r.QuantizationLevels}\n" +
                $"  Original Bit Rate  : {r.OriginalBitRate / 1000.0:F0} kbps\n" +
                $"──────────────────────────────────────\n" +
                $"  Total Samples      : {r.SampleCount:N0}\n" +
                $"  Original Size      : {r.OriginalSizeKB:F2} KB\n" +
                $"  Compressed Size    : {r.CompressedSizeKB:F2} KB\n" +
                $"  Compression Ratio  : {r.Ratio:F2}x\n" +
                $"  Space Saved        : {r.SavingPercent:F1}%\n" +
                $"──────────────────────────────────────\n" +
                $"  Time Elapsed       : {r.ElapsedMs} ms\n" +
                $"╚══════════════════════════════════════╝",
                "Compression Report",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ═══════════════════════════════════════════════════════════
        //  DRAG & DROP
        // ═══════════════════════════════════════════════════════════
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0) return;

            string ext = Path.GetExtension(files[0]).ToLowerInvariant();
            if (ext == ".vlb")
                LoadVlb(files[0]);
            else
                LoadAudio(files[0]);
        }

        // ═══════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════
        private void SetUIBusy(bool busy)
        {
            btnCompress.Enabled = !busy;
            btnDecompress.Enabled = !busy;
            btnOpen.Enabled = !busy;
            btnSave.Enabled = !busy;
            btnCancel.Enabled = busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }
    }
}