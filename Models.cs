using System;

namespace BlastWaveCSharp
{
    internal sealed class SignatureWaveData
    {
        public double[] Tran { get; set; } = Array.Empty<double>();
        public double[] Vert { get; set; } = Array.Empty<double>();
        public double[] Long { get; set; } = Array.Empty<double>();
        public int Length { get; set; }
    }

    internal sealed class DelayScenarioData
    {
        public int[][] Delays { get; set; } = Array.Empty<int[]>();
        public int MaxDelay { get; set; }
    }

    internal sealed class WeightData
    {
        public double[][] Weights { get; set; } = Array.Empty<double[]>();
    }

    internal sealed class DistanceData
    {
        public double[] Ratios { get; set; } = Array.Empty<double>();
    }

    internal sealed class ComputationResult
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
