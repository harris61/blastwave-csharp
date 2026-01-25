using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlastWaveCSharp
{
    internal static class DataLoader
    {
        public static SignatureWaveData LoadSignatureWave(FileInfo[] filesWave, int samplingRate)
        {
            // First pass: read all files and determine maximum data length
            var fileDataList = new List<(string[] lines, int dataStartIndex, int dataLength)>();
            int maxDataLength = 0;

            foreach (FileInfo file in filesWave)
            {
                string[] lines = File.ReadAllLines(file.FullName);
                if (!FileParsing.TryGetSignatureMetadata(lines, out int fileSampleRate, out int dataStartIndex))
                {
                    throw new InvalidOperationException($"Invalid signature header: {file.Name}");
                }

                if (fileSampleRate != samplingRate)
                {
                    throw new InvalidOperationException($"Sampling rate mismatch in {file.Name}.");
                }

                int dataLength = lines.Length - dataStartIndex;
                if (dataLength <= 0)
                {
                    throw new InvalidOperationException($"Signature file has no data: {file.Name}");
                }

                fileDataList.Add((lines, dataStartIndex, dataLength));
                if (dataLength > maxDataLength)
                {
                    maxDataLength = dataLength;
                }
            }

            // Second pass: extract data using max length, zero-padding shorter files
            double[] sumTran = new double[maxDataLength];
            double[] sumVert = new double[maxDataLength];
            double[] sumLong = new double[maxDataLength];

            for (int fileIndex = 0; fileIndex < fileDataList.Count; fileIndex++)
            {
                var (lines, dataStartIndex, dataLength) = fileDataList[fileIndex];

                for (int i = 0; i < dataLength; i++)
                {
                    string line = lines[dataStartIndex + i];
                    if (!FileParsing.TryParseWaveLine(line, out double tran, out double vert, out double lon))
                    {
                        throw new InvalidOperationException($"Invalid signature data format: {filesWave[fileIndex].Name}");
                    }

                    sumTran[i] += tran;
                    sumVert[i] += vert;
                    sumLong[i] += lon;
                }
                // Samples beyond dataLength remain zero (zero-padding for shorter files)
            }

            double[] tranWave = new double[maxDataLength];
            double[] vertWave = new double[maxDataLength];
            double[] longWave = new double[maxDataLength];

            for (int i = 0; i < maxDataLength; i++)
            {
                tranWave[i] = sumTran[i] / filesWave.Length;
                vertWave[i] = sumVert[i] / filesWave.Length;
                longWave[i] = sumLong[i] / filesWave.Length;
            }

            return new SignatureWaveData
            {
                Tran = tranWave,
                Vert = vertWave,
                Long = longWave,
                Length = maxDataLength
            };
        }

        public static DelayScenarioData LoadDelayScenarios(FileInfo[] filesDelay, int ratioSps)
        {
            int[][] delays = new int[filesDelay.Length][];
            int maxDelay = 0;

            for (int i = 0; i < filesDelay.Length; i++)
            {
                string[] lines = File.ReadAllLines(filesDelay[i].FullName);
                if (lines.Length <= 1)
                {
                    throw new InvalidOperationException($"Delay file has no data: {filesDelay[i].Name}");
                }

                int dataLength = lines.Length - 1;
                int[] delayValues = new int[dataLength];
                for (int j = 0; j < dataLength; j++)
                {
                    if (!int.TryParse(lines[j + 1], out int value))
                    {
                        throw new InvalidOperationException($"Invalid delay data: {filesDelay[i].Name}");
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

        public static WeightData LoadWeights(string defaultDir, int fileCount)
        {
            var dirWeight = new DirectoryInfo(Path.Combine(defaultDir, "Explosive Weight"));
            FileInfo[] filesWeight = GetNumericFiles(dirWeight);
            if (filesWeight.Length != fileCount)
            {
                throw new InvalidOperationException("Weight file count does not match delay scenario count.");
            }

            double[][] weights = new double[fileCount][];
            for (int i = 0; i < fileCount; i++)
            {
                string[] lines = File.ReadAllLines(filesWeight[i].FullName);
                double[] data = new double[lines.Length];
                for (int j = 0; j < lines.Length; j++)
                {
                    if (!double.TryParse(lines[j], out double value))
                    {
                        throw new InvalidOperationException($"Invalid weight data: {filesWeight[i].Name}");
                    }
                    data[j] = value;
                }
                weights[i] = data;
            }

            return new WeightData { Weights = weights };
        }

        public static DistanceData LoadDistanceData(string defaultDir, int scenarioCount)
        {
            string distanceDir = Path.Combine(defaultDir, "Simulation Distance");
            string[] avgLines = File.ReadAllLines(Path.Combine(distanceDir, "distanceaverage.txt"));
            if (avgLines.Length == 0)
            {
                throw new InvalidOperationException("distanceaverage.txt is empty.");
            }

            double sumDistance = 0;
            for (int i = 0; i < avgLines.Length; i++)
            {
                if (!double.TryParse(avgLines[i], out double value))
                {
                    throw new InvalidOperationException("Invalid distance average data.");
                }
                sumDistance += value;
            }

            double averageDistance = sumDistance / avgLines.Length;
            if (averageDistance <= 0)
            {
                throw new InvalidOperationException("Invalid average distance.");
            }

            string[] simLines = File.ReadAllLines(Path.Combine(distanceDir, "distancesimulation.txt"));
            if (simLines.Length < scenarioCount)
            {
                throw new InvalidOperationException("Distance simulation count is less than scenario count.");
            }

            double[] ratios = new double[simLines.Length];
            for (int i = 0; i < simLines.Length; i++)
            {
                if (!double.TryParse(simLines[i], out double value))
                {
                    throw new InvalidOperationException("Invalid distance simulation data.");
                }
                ratios[i] = value / averageDistance;
            }

            return new DistanceData { Ratios = ratios };
        }

        public static void ValidateScenarioAlignment(DelayScenarioData delays, WeightData weights, DistanceData distances)
        {
            if (delays.Delays.Length != weights.Weights.Length)
            {
                throw new InvalidOperationException("Delay and weight file counts do not match.");
            }

            for (int i = 0; i < delays.Delays.Length; i++)
            {
                if (delays.Delays[i].Length != weights.Weights[i].Length)
                {
                    throw new InvalidOperationException($"Delay and weight data counts do not match for scenario {i + 1}.");
                }
            }

            if (distances.Ratios.Length < delays.Delays.Length)
            {
                throw new InvalidOperationException("Distance ratio count is less than scenario count.");
            }
        }

        public static FileInfo[] GetNumericFiles(DirectoryInfo dir)
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
                        throw new InvalidOperationException($"Duplicate file number {value} in {dir.Name}.");
                    }
                    map[value] = file;
                }
            }

            if (map.Count == 0)
            {
                throw new InvalidOperationException($"Folder {dir.Name} has no numbered files.");
            }

            int maxIndex = map.Keys.Max();
            var ordered = new List<FileInfo>(maxIndex);
            for (int i = 1; i <= maxIndex; i++)
            {
                if (!map.TryGetValue(i, out FileInfo? file))
                {
                    throw new InvalidOperationException($"Files in {dir.Name} must be numbered from 1 to {maxIndex}.");
                }
                ordered.Add(file);
            }

            return ordered.ToArray();
        }
    }
}
