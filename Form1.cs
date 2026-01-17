using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BlastWaveCSharp
{
    public partial class Form1 : Form
    {
        private const int DefaultSps = 1024;
        private string _dataDirectory = string.Empty;
        private readonly Dictionary<Control, int> _bottomControlBaseTop = new Dictionary<Control, int>();

        public Form1()
        {
            InitializeComponent();
            _dataDirectory = GetDefaultDir();
            UpdateDirectoryLabel();
            UpdateMetaInfo(0, 0, 0);
            CacheBottomControlPositions();
            LayoutBottomSection();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            var previousCursor = System.Windows.Forms.Cursor.Current;
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            try
            {
                if (!TryGetInputs(out InputParams inputs))
                {
                    return;
                }

                string dataDir = _dataDirectory;
                if (!Directory.Exists(dataDir))
                {
                    ShowError($"Folder not found: {dataDir}");
                    return;
                }

                FileInfo[] signatureFiles = DataLoader.GetNumericFiles(new DirectoryInfo(Path.Combine(dataDir, "Signature Wave")));
                if (signatureFiles.Length == 0)
                {
                    ShowError("Signature Wave folder is empty.");
                    return;
                }

                int samplingRate = FileParsing.GetSamplingRate(signatureFiles[0]);
                if (samplingRate % DefaultSps != 0)
                {
                    ShowError($"Sampling rate must be divisible by {DefaultSps}.");
                    return;
                }

                int ratioSps = samplingRate / DefaultSps;
                SignatureWaveData signature = DataLoader.LoadSignatureWave(signatureFiles, inputs.MeasurementMs, ratioSps, samplingRate);

                FileInfo[] delayFiles = DataLoader.GetNumericFiles(new DirectoryInfo(Path.Combine(dataDir, "Delay Scenario")));
                UpdateMetaInfo(signatureFiles.Length, delayFiles.Length, samplingRate);
                DelayScenarioData delays = DataLoader.LoadDelayScenarios(delayFiles, ratioSps);
                WeightData weights = DataLoader.LoadWeights(dataDir, delayFiles.Length);
                DistanceData distances = DataLoader.LoadDistanceData(dataDir, delayFiles.Length);

                DataLoader.ValidateScenarioAlignment(delays, weights, distances);

                ComputationResult result = WaveCalculator.ComputeScenarioWaves(
                    signature,
                    delays,
                    weights,
                    distances,
                    inputs.FieldConstant,
                    inputs.SignatureWeight);

                ResultWriter.WriteResults(dataDir, result);
                UpdateUi(signature, result, samplingRate);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to calculate PPV.{Environment.NewLine}{ex.Message}");
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = previousCursor;
                button1.Enabled = true;
            }
        }

        private static string GetDefaultDir()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Blasting Data");
        }

        private bool TryGetInputs(out InputParams inputs)
        {
            inputs = new InputParams();

            if (!double.TryParse(textBox3.Text, out double fieldConstant))
            {
                ShowError("Area Constant (B) must be a number.");
                return false;
            }

            if (!double.TryParse(textBox4.Text, out double signatureWeight) || signatureWeight <= 0)
            {
                ShowError("Signature Hole Charge must be > 0.");
                return false;
            }

            if (!int.TryParse(textBox7.Text, out int measurementMs) || measurementMs <= 0)
            {
                ShowError("Measurement duration must be > 0.");
                return false;
            }

            inputs.FieldConstant = fieldConstant;
            inputs.SignatureWeight = signatureWeight;
            inputs.MeasurementMs = measurementMs;
            return true;
        }

        private void UpdateUi(SignatureWaveData signature, ComputationResult result, int samplingRate)
        {
            double ppvTranSignature = WaveCalculator.GetPeakAbs(signature.Tran);
            double ppvVertSignature = WaveCalculator.GetPeakAbs(signature.Vert);
            double ppvLongSignature = WaveCalculator.GetPeakAbs(signature.Long);

            label2.Text = $"Signature Wave(mm/s, 1/{samplingRate} ms)";
            label1.Text = $"Optimized Full Blast Wave(mm/s, 1/{samplingRate} ms)";
            label15.Text = $"Optimized Peak Vector Sum(mm/s, 1/{samplingRate} ms)";
            label9.Text = $"Signature Transversal Wave PPV = {ppvTranSignature} mm/s";
            label10.Text = $"Signature Vertical Wave PPV = {ppvVertSignature} mm/s";
            label11.Text = $"Signature Longitudinal Wave PPV = {ppvLongSignature} mm/s";
            label12.Text = $"Scenario {result.OptTranIndex + 1} Full Blast Transversal Wave PPV = {result.OptTranPpv} mm/s";
            label13.Text = $"Scenario {result.OptVertIndex + 1} Full Blast Vertical Wave PPV = {result.OptVertPpv} mm/s";
            label14.Text = $"Scenario {result.OptLongIndex + 1} Full Blast Longitudinal Wave PPV = {result.OptLongPpv} mm/s";
            label16.Text = $"Scenario {result.OptPvsIndex + 1} Peak Vector Sum PPV = {result.OptPvsPpv} mm/s";

            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Add("Scenario", "Delay Scenario");
            dataGridView1.Columns.Add("PpvTran", "PPV Tran (mm/s)");
            dataGridView1.Columns.Add("PpvVert", "PPV Vert (mm/s)");
            dataGridView1.Columns.Add("PpvLong", "PPV Long (mm/s)");
            dataGridView1.Columns.Add("PpvPvs", "PPV PVS (mm/s)");

            int scenarioCount = result.Ppv.GetLength(1);
            for (int i = 0; i < scenarioCount; i++)
            {
                dataGridView1.Rows.Add(new object[]
                {
                    i + 1,
                    result.Ppv[0, i],
                    result.Ppv[1, i],
                    result.Ppv[2, i],
                    result.Ppv[3, i]
                });
            }

            PlotSeries(chart1, signature.Tran, System.Drawing.Color.Turquoise);
            PlotSeries(chart2, signature.Vert, System.Drawing.Color.Blue);
            PlotSeries(chart3, signature.Long, System.Drawing.Color.Purple);

            PlotSeries(chart4, WaveCalculator.ExtractWave(result.Tran, result.OptTranIndex, result.WaveLength), System.Drawing.Color.Turquoise);
            PlotSeries(chart5, WaveCalculator.ExtractWave(result.Vert, result.OptVertIndex, result.WaveLength), System.Drawing.Color.Blue);
            PlotSeries(chart6, WaveCalculator.ExtractWave(result.Long, result.OptLongIndex, result.WaveLength), System.Drawing.Color.Purple);
            PlotSeries(chart7, WaveCalculator.ExtractWave(result.Pvs, result.OptPvsIndex, result.WaveLength), System.Drawing.Color.Green);
        }

        private static void PlotSeries(System.Windows.Forms.DataVisualization.Charting.Chart chart, double[] data, System.Drawing.Color color)
        {
            chart.Series.Clear();
            var series = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series",
                Color = color,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line
            };

            chart.Series.Add(series);
            for (int i = 0; i < data.Length; i++)
            {
                series.Points.AddXY(i, data[i]);
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void LinkLabelAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLink("https://www.linkedin.com/in/harristio-adam/");
        }

        private void LinkLabelSupervisor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLink("https://itb.ac.id/staf/profil/ganda-marihot-simangunsong");
        }

        private void LinkLabelGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLink("https://github.com/harris61/blastwave-csharp");
        }

        private void OpenLink(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ShowError($"Failed to open link.{Environment.NewLine}{ex.Message}");
            }
        }

        private void DataDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select Blasting Data directory",
                SelectedPath = _dataDirectory
            };

            if (dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                _dataDirectory = dialog.SelectedPath;
                UpdateDirectoryLabel();
            }
        }

        private void UpdateDirectoryLabel()
        {
            label22.Text = $"Data Directory: {_dataDirectory}";
        }

        private void UpdateMetaInfo(int signatureCount, int delayCount, int sampleRate)
        {
            label23.Text = $"Signature Files: {signatureCount} | Delay Files: {delayCount} | Sample Rate: {sampleRate} sps | Note: Sample rate across signature waves must match. | USBM: v = K (D / sqrt(Qmax))^-b (Duvall & Petkof)";
            LayoutBottomSection();
        }

        private void CacheBottomControlPositions()
        {
            Control[] controls =
            {
                label1,
                label2,
                label9,
                label10,
                label11,
                label12,
                label13,
                label14,
                label15,
                label16,
                dataGridView1,
                tableLayoutPanel1,
                chart7
            };

            _bottomControlBaseTop.Clear();
            foreach (Control control in controls)
            {
                _bottomControlBaseTop[control] = control.Top;
            }
        }

        private void LayoutBottomSection()
        {
            if (_bottomControlBaseTop.Count == 0)
            {
                return;
            }

            int bottomStart = flowLayoutPanel2.Bottom + 8;
            int baseTop = _bottomControlBaseTop[label2];
            int delta = bottomStart - baseTop;

            foreach (var pair in _bottomControlBaseTop)
            {
                pair.Key.Top = pair.Value + delta;
            }
        }

        private sealed class InputParams
        {
            public int MeasurementMs { get; set; }
            public double FieldConstant { get; set; }
            public double SignatureWeight { get; set; }
        }
    }
}

