using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BlastWaveCSharp
{
    public partial class Form1 : Form
    {
        private const int DefaultSps = 1024;

        public Form1()
        {
            InitializeComponent();

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

                string defaultDir = GetDefaultDir();
                if (!Directory.Exists(defaultDir))
                {
                    ShowError($"Folder not found: {defaultDir}");
                    return;
                }

                if (inputs.SamplingRate % DefaultSps != 0)
                {
                    ShowError($"Sampling rate must be divisible by {DefaultSps}.");
                    return;
                }

                int ratioSps = inputs.SamplingRate / DefaultSps;
                SignatureWaveData signature = LoadSignatureWave(defaultDir, inputs.SignatureFileCount, inputs.MeasurementMs, ratioSps);
                DelayScenarioData delays = LoadDelayScenarios(defaultDir, inputs.DelayFileCount, ratioSps);
                WeightData weights = LoadWeights(defaultDir, inputs.DelayFileCount);
                DistanceData distances = LoadDistanceData(defaultDir, inputs.DelayFileCount);

                ValidateScenarioAlignment(delays, weights, distances);

                ComputationResult result = ComputeScenarioWaves(
                    signature,
                    delays,
                    weights,
                    distances,
                    inputs.FieldConstant,
                    inputs.SignatureWeight);

                UpdateUi(signature, result, inputs.SamplingRate);
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

            if (!int.TryParse(textBox1.Text, out int signatureFiles) || signatureFiles <= 0)
            {
                ShowError("Jumlah File Signature Wave harus angka > 0.");
                return false;
            }

            if (!int.TryParse(textBox2.Text, out int delayFiles) || delayFiles <= 0)
            {
                ShowError("Jumlah File Skenario Delay harus angka > 0.");
                return false;
            }

            if (!double.TryParse(textBox3.Text, out double fieldConstant) || fieldConstant <= 0)
            {
                ShowError("Konstanta Lapangan harus angka > 0.");
                return false;
            }

            if (!double.TryParse(textBox4.Text, out double signatureWeight) || signatureWeight <= 0)
            {
                ShowError("Muatan Signature Hole harus angka > 0.");
                return false;
            }

            if (!int.TryParse(textBox5.Text, out int samplingRate) || samplingRate <= 0)
            {
                ShowError("Sampling Rate harus angka > 0.");
                return false;
            }

            if (!int.TryParse(textBox7.Text, out int measurementMs) || measurementMs <= 0)
            {
                ShowError("Lama Pengukuran harus angka > 0.");
                return false;
            }

            inputs.SignatureFileCount = signatureFiles;
            inputs.DelayFileCount = delayFiles;
            inputs.FieldConstant = fieldConstant;
            inputs.SignatureWeight = signatureWeight;
            inputs.SamplingRate = samplingRate;
            inputs.MeasurementMs = measurementMs;
            return true;
        }

        private static SignatureWaveData LoadSignatureWave(string defaultDir, int fileCount, int measurementMs, int ratioSps)
        {
            var dirWave = new DirectoryInfo(Path.Combine(defaultDir, "Signature Wave"));
            FileInfo[] filesWave = GetNumericFiles(dirWave, fileCount);

            int startWave = 270 * ratioSps;
            int endWave = measurementMs * ratioSps;
            if (endWave <= startWave)
            {
                throw new InvalidOperationException("Lama Pengukuran terlalu kecil untuk window signature.");
            }

            int lengthWave = endWave - startWave;
            double[] sumTran = new double[lengthWave];
            double[] sumVert = new double[lengthWave];
            double[] sumLong = new double[lengthWave];

            foreach (FileInfo file in filesWave)
            {
                string[] lines = File.ReadAllLines(file.FullName);
                if (lines.Length <= endWave)
                {
                    throw new InvalidOperationException($"File signature terlalu pendek: {file.Name}");
                }

                for (int i = 0; i < lengthWave; i++)
                {
                    string line = lines[startWave + i];
                    if (!TryParseWaveLine(line, out double tran, out double vert, out double lon))
                    {
                        throw new InvalidOperationException($"Format data signature tidak valid: {file.Name}");
                    }

                    sumTran[i] += tran;
                    sumVert[i] += vert;
                    sumLong[i] += lon;
                }
            }

            double[] tranWave = new double[lengthWave];
            double[] vertWave = new double[lengthWave];
            double[] longWave = new double[lengthWave];

            for (int i = 0; i < lengthWave; i++)
            {
                tranWave[i] = sumTran[i] / fileCount;
                vertWave[i] = sumVert[i] / fileCount;
                longWave[i] = sumLong[i] / fileCount;
            }

            return new SignatureWaveData
            {
                Tran = tranWave,
                Vert = vertWave,
                Long = longWave,
                Length = lengthWave
            };
        }

        private static DelayScenarioData LoadDelayScenarios(string defaultDir, int fileCount, int ratioSps)
        {
            var dirDelay = new DirectoryInfo(Path.Combine(defaultDir, "Delay Scenario"));
            FileInfo[] filesDelay = GetNumericFiles(dirDelay, fileCount);

            int[][] delays = new int[fileCount][];
            int maxDelay = 0;

            for (int i = 0; i < fileCount; i++)
            {
                string[] lines = File.ReadAllLines(filesDelay[i].FullName);
                if (lines.Length <= 1)
                {
                    throw new InvalidOperationException($"Delay file tidak memiliki data: {filesDelay[i].Name}");
                }

                int dataLength = lines.Length - 1;
                int[] delayValues = new int[dataLength];
                for (int j = 0; j < dataLength; j++)
                {
                    if (!int.TryParse(lines[j + 1], out int value))
                    {
                        throw new InvalidOperationException($"Data delay tidak valid: {filesDelay[i].Name}");
                    }
                    delayValues[j] = value * ratioSps;
                }

                Array.Sort(delayValues);
                delays[i] = delayValues;
                if (delayValues.Length > 0 && delayValues[delayValues.Length - 1] > maxDelay)
                {
                    maxDelay = delayValues[delayValues.Length - 1];
                }
            }

            return new DelayScenarioData
            {
                Delays = delays,
                MaxDelay = maxDelay
            };
        }

        private static WeightData LoadWeights(string defaultDir, int fileCount)
        {
            var dirWeight = new DirectoryInfo(Path.Combine(defaultDir, "Explosive Weight"));
            FileInfo[] filesWeight = GetNumericFiles(dirWeight, fileCount);

            double[][] weights = new double[fileCount][];
            for (int i = 0; i < fileCount; i++)
            {
                string[] lines = File.ReadAllLines(filesWeight[i].FullName);
                double[] data = new double[lines.Length];
                for (int j = 0; j < lines.Length; j++)
                {
                    if (!double.TryParse(lines[j], out double value))
                    {
                        throw new InvalidOperationException($"Data weight tidak valid: {filesWeight[i].Name}");
                    }
                    data[j] = value;
                }
                weights[i] = data;
            }

            return new WeightData { Weights = weights };
        }

        private static DistanceData LoadDistanceData(string defaultDir, int scenarioCount)
        {
            string distanceDir = Path.Combine(defaultDir, "Simulation Distance");
            string[] avgLines = File.ReadAllLines(Path.Combine(distanceDir, "distanceaverage.txt"));
            if (avgLines.Length == 0)
            {
                throw new InvalidOperationException("distanceaverage.txt kosong.");
            }

            double sumDistance = 0;
            for (int i = 0; i < avgLines.Length; i++)
            {
                if (!double.TryParse(avgLines[i], out double value))
                {
                    throw new InvalidOperationException("Data distance average tidak valid.");
                }
                sumDistance += value;
            }

            double averageDistance = sumDistance / avgLines.Length;
            if (averageDistance <= 0)
            {
                throw new InvalidOperationException("Average distance tidak valid.");
            }

            string[] simLines = File.ReadAllLines(Path.Combine(distanceDir, "distancesimulation.txt"));
            if (simLines.Length < scenarioCount)
            {
                throw new InvalidOperationException("Jumlah distance simulation lebih sedikit dari skenario.");
            }

            double[] ratios = new double[simLines.Length];
            for (int i = 0; i < simLines.Length; i++)
            {
                if (!double.TryParse(simLines[i], out double value))
                {
                    throw new InvalidOperationException("Data distance simulation tidak valid.");
                }
                ratios[i] = value / averageDistance;
            }

            return new DistanceData { Ratios = ratios };
        }

        private static void ValidateScenarioAlignment(DelayScenarioData delays, WeightData weights, DistanceData distances)
        {
            if (delays.Delays.Length != weights.Weights.Length)
            {
                throw new InvalidOperationException("Jumlah file delay dan weight tidak sama.");
            }

            for (int i = 0; i < delays.Delays.Length; i++)
            {
                if (delays.Delays[i].Length != weights.Weights[i].Length)
                {
                    throw new InvalidOperationException($"Jumlah data delay dan weight tidak sama pada skenario {i + 1}.");
                }
            }

            if (distances.Ratios.Length < delays.Delays.Length)
            {
                throw new InvalidOperationException("Jumlah distance ratio kurang dari jumlah skenario.");
            }
        }

        private static ComputationResult ComputeScenarioWaves(
            SignatureWaveData signature,
            DelayScenarioData delays,
            WeightData weights,
            DistanceData distances,
            double fieldConstant,
            double signatureWeight)
        {
            int scenarioCount = delays.Delays.Length;
            int waveLength = signature.Length + delays.MaxDelay + 100;

            double[,] listTranWave = new double[scenarioCount, waveLength];
            double[,] listVertWave = new double[scenarioCount, waveLength];
            double[,] listLongWave = new double[scenarioCount, waveLength];
            double[,] listPvs = new double[scenarioCount, waveLength];

            for (int scenario = 0; scenario < scenarioCount; scenario++)
            {
                double distanceRatio = distances.Ratios[scenario];
                if (distanceRatio <= 0)
                {
                    throw new InvalidOperationException($"Distance ratio tidak valid pada skenario {scenario + 1}.");
                }

                int[] delayList = delays.Delays[scenario];
                double[] weightList = weights.Weights[scenario];

                for (int i = 0; i < delayList.Length; i++)
                {
                    int add = delayList[i];
                    double scale = Math.Pow(
                        (weightList[i] / signatureWeight) / Math.Pow(distanceRatio, 2),
                        0.5 * fieldConstant);

                    for (int sample = 0; sample < signature.Length; sample++)
                    {
                        int targetIndex = sample + add;
                        listTranWave[scenario, targetIndex] += signature.Tran[sample] * scale;
                        listVertWave[scenario, targetIndex] += signature.Vert[sample] * scale;
                        listLongWave[scenario, targetIndex] += signature.Long[sample] * scale;
                    }
                }
            }

            for (int scenario = 0; scenario < scenarioCount; scenario++)
            {
                for (int sample = 0; sample < waveLength; sample++)
                {
                    double tran = listTranWave[scenario, sample];
                    double vert = listVertWave[scenario, sample];
                    double lon = listLongWave[scenario, sample];
                    listPvs[scenario, sample] = Math.Sqrt(tran * tran + vert * vert + lon * lon);
                }
            }

            double[,] listPpv = new double[4, scenarioCount];
            for (int scenario = 0; scenario < scenarioCount; scenario++)
            {
                double[] tranPart = ExtractWave(listTranWave, scenario, waveLength);
                double[] vertPart = ExtractWave(listVertWave, scenario, waveLength);
                double[] longPart = ExtractWave(listLongWave, scenario, waveLength);
                double[] pvsPart = ExtractWave(listPvs, scenario, waveLength);

                listPpv[0, scenario] = GetPeakAbs(tranPart);
                listPpv[1, scenario] = GetPeakAbs(vertPart);
                listPpv[2, scenario] = GetPeakAbs(longPart);
                listPpv[3, scenario] = GetPeakAbs(pvsPart);
            }

            double[] tranParts = new double[scenarioCount];
            double[] vertParts = new double[scenarioCount];
            double[] longParts = new double[scenarioCount];
            double[] pvsParts = new double[scenarioCount];
            for (int i = 0; i < scenarioCount; i++)
            {
                tranParts[i] = listPpv[0, i];
                vertParts[i] = listPpv[1, i];
                longParts[i] = listPpv[2, i];
                pvsParts[i] = listPpv[3, i];
            }

            double ppvTranFull = GetMinAbs(tranParts, out int optTranIndex);
            double ppvVertFull = GetMinAbs(vertParts, out int optVertIndex);
            double ppvLongFull = GetMinAbs(longParts, out int optLongIndex);
            double ppvPvs = GetMinAbs(pvsParts, out int optPvsIndex);

            return new ComputationResult
            {
                WaveLength = waveLength,
                Tran = listTranWave,
                Vert = listVertWave,
                Long = listLongWave,
                Pvs = listPvs,
                Ppv = listPpv,
                OptTranIndex = optTranIndex,
                OptVertIndex = optVertIndex,
                OptLongIndex = optLongIndex,
                OptPvsIndex = optPvsIndex,
                OptTranPpv = ppvTranFull,
                OptVertPpv = ppvVertFull,
                OptLongPpv = ppvLongFull,
                OptPvsPpv = ppvPvs
            };
        }

        private void UpdateUi(SignatureWaveData signature, ComputationResult result, int samplingRate)
        {
            double ppvTranSignature = GetPeakAbs(signature.Tran);
            double ppvVertSignature = GetPeakAbs(signature.Vert);
            double ppvLongSignature = GetPeakAbs(signature.Long);

            label2.Text = $"Signature Wave(mm/s, 1/{samplingRate} ms)";
            label1.Text = $"Optimized Full Blast Wave(mm/s, 1/{samplingRate} ms)";
            label15.Text = $"Optimized Peak Vector Sum(mm/s, 1/{samplingRate} ms)";
            label9.Text = $"Signature Transversal Wave PPV = {ppvTranSignature} mm/s";
            label10.Text = $"Signature Vertical Wave PPV = {ppvVertSignature} mm/s";
            label11.Text = $"Signature Longitudinal Wave PPV = {ppvLongSignature} mm/s";
            label12.Text = $"Skenario {result.OptTranIndex + 1} Full Blast Transversal Wave PPV = {result.OptTranPpv} mm/s";
            label13.Text = $"Skenario {result.OptVertIndex + 1} Full Blast Vertical Wave PPV = {result.OptVertPpv} mm/s";
            label14.Text = $"Skenario {result.OptLongIndex + 1} Full Blast Longitudinal Wave PPV = {result.OptLongPpv} mm/s";
            label16.Text = $"Skenario {result.OptPvsIndex + 1} Peak Vector Sum PPV = {result.OptPvsPpv} mm/s";

            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Add("Skenario", "Skenario Delay");
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

            PlotSeries(chart4, ExtractWave(result.Tran, result.OptTranIndex, result.WaveLength), System.Drawing.Color.Turquoise);
            PlotSeries(chart5, ExtractWave(result.Vert, result.OptVertIndex, result.WaveLength), System.Drawing.Color.Blue);
            PlotSeries(chart6, ExtractWave(result.Long, result.OptLongIndex, result.WaveLength), System.Drawing.Color.Purple);
            PlotSeries(chart7, ExtractWave(result.Pvs, result.OptPvsIndex, result.WaveLength), System.Drawing.Color.Green);
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

        private static FileInfo[] GetNumericFiles(DirectoryInfo dir, int expectedCount)
        {
            if (!dir.Exists)
            {
                throw new InvalidOperationException($"Folder not found: {dir.FullName}");
            }

            FileInfo[] files = dir.GetFiles("*.txt");
            var map = new Dictionary<int, FileInfo>();
            foreach (FileInfo file in files)
            {
                if (int.TryParse(Path.GetFileNameWithoutExtension(file.Name), out int value) && value >= 1)
                {
                    if (map.ContainsKey(value))
                    {
                        throw new InvalidOperationException($"File bernomor {value} duplikat di {dir.Name}.");
                    }
                    map[value] = file;
                }
            }

            if (map.Count < expectedCount)
            {
                throw new InvalidOperationException($"Jumlah file di {dir.Name} lebih sedikit dari input.");
            }

            var ordered = new List<FileInfo>(expectedCount);
            for (int i = 1; i <= expectedCount; i++)
            {
                if (!map.TryGetValue(i, out FileInfo? file))
                {
                    throw new InvalidOperationException($"File di {dir.Name} harus bernomor mulai 1 sampai {expectedCount}.");
                }
                ordered.Add(file);
            }

            return ordered.ToArray();
        }

        private static bool TryParseWaveLine(string line, out double tran, out double vert, out double lon)
        {
            tran = 0;
            vert = 0;
            lon = 0;

            string[] tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 3)
            {
                return false;
            }

            return double.TryParse(tokens[0], out tran)
                && double.TryParse(tokens[1], out vert)
                && double.TryParse(tokens[2], out lon);
        }

        private static double GetPeakAbs(double[] values)
        {
            if (values.Length == 0)
            {
                return 0;
            }

            double best = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (Math.Abs(values[i]) > Math.Abs(best))
                {
                    best = values[i];
                }
            }
            return best;
        }

        private static double GetMinAbs(double[] values, out int index)
        {
            index = -1;
            if (values.Length == 0)
            {
                return 0;
            }

            double best = values[0];
            index = 0;
            for (int i = 1; i < values.Length; i++)
            {
                if (Math.Abs(values[i]) < Math.Abs(best))
                {
                    best = values[i];
                    index = i;
                }
            }
            return best;
        }

        private static double[] ExtractWave(double[,] source, int row, int length)
        {
            double[] data = new double[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = source[row, i];
            }
            return data;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private sealed class InputParams
        {
            public int SignatureFileCount { get; set; }
            public int DelayFileCount { get; set; }
            public int SamplingRate { get; set; }
            public int MeasurementMs { get; set; }
            public double FieldConstant { get; set; }
            public double SignatureWeight { get; set; }
        }

        private sealed class SignatureWaveData
        {
            public double[] Tran { get; set; } = Array.Empty<double>();
            public double[] Vert { get; set; } = Array.Empty<double>();
            public double[] Long { get; set; } = Array.Empty<double>();
            public int Length { get; set; }
        }

        private sealed class DelayScenarioData
        {
            public int[][] Delays { get; set; } = Array.Empty<int[]>();
            public int MaxDelay { get; set; }
        }

        private sealed class WeightData
        {
            public double[][] Weights { get; set; } = Array.Empty<double[]>();
        }

        private sealed class DistanceData
        {
            public double[] Ratios { get; set; } = Array.Empty<double>();
        }

        private sealed class ComputationResult
        {
            public int WaveLength { get; set; }
            public double[,] Tran { get; set; } = new double[0, 0];
            public double[,] Vert { get; set; } = new double[0, 0];
            public double[,] Long { get; set; } = new double[0, 0];
            public double[,] Pvs { get; set; } = new double[0, 0];
            public double[,] Ppv { get; set; } = new double[0, 0];
            public int OptTranIndex { get; set; }
            public int OptVertIndex { get; set; }
            public int OptLongIndex { get; set; }
            public int OptPvsIndex { get; set; }
            public double OptTranPpv { get; set; }
            public double OptVertPpv { get; set; }
            public double OptLongPpv { get; set; }
            public double OptPvsPpv { get; set; }
        }
    }
}

