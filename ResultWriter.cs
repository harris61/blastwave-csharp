using System.Globalization;
using System.IO;

namespace BlastWaveCSharp
{
    internal static class ResultWriter
    {
        public static void WriteResults(string dataDir, ComputationResult result)
        {
            WriteSeries(Path.Combine(dataDir, "result_Tran.txt"),
                WaveCalculator.ExtractWave(result.Tran, result.OptTranIndex, result.WaveLength));
            WriteSeries(Path.Combine(dataDir, "result_Vert.txt"),
                WaveCalculator.ExtractWave(result.Vert, result.OptVertIndex, result.WaveLength));
            WriteSeries(Path.Combine(dataDir, "result_Long.txt"),
                WaveCalculator.ExtractWave(result.Long, result.OptLongIndex, result.WaveLength));
            WriteSeries(Path.Combine(dataDir, "result_PVS.txt"),
                WaveCalculator.ExtractWave(result.Pvs, result.OptPvsIndex, result.WaveLength));
        }

        private static void WriteSeries(string path, double[] data)
        {
            string[] lines = new string[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                lines[i] = data[i].ToString("G17", CultureInfo.InvariantCulture);
            }

            File.WriteAllLines(path, lines);
        }
    }
}
