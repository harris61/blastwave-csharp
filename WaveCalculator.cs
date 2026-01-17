using System;

namespace BlastWaveCSharp
{
    internal static class WaveCalculator
    {
        public static ComputationResult ComputeScenarioWaves(
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
                    throw new InvalidOperationException($"Invalid distance ratio for scenario {scenario + 1}.");
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

            double[,] listPpv = new double[4, scenarioCount];
            for (int scenario = 0; scenario < scenarioCount; scenario++)
            {
                double peakTran = 0;
                double peakVert = 0;
                double peakLong = 0;
                double peakPvs = 0;

                for (int sample = 0; sample < waveLength; sample++)
                {
                    double tran = listTranWave[scenario, sample];
                    double vert = listVertWave[scenario, sample];
                    double lon = listLongWave[scenario, sample];
                    double pvs = Math.Sqrt(tran * tran + vert * vert + lon * lon);

                    listPvs[scenario, sample] = pvs;

                    if (Math.Abs(tran) > Math.Abs(peakTran))
                    {
                        peakTran = tran;
                    }

                    if (Math.Abs(vert) > Math.Abs(peakVert))
                    {
                        peakVert = vert;
                    }

                    if (Math.Abs(lon) > Math.Abs(peakLong))
                    {
                        peakLong = lon;
                    }

                    if (Math.Abs(pvs) > Math.Abs(peakPvs))
                    {
                        peakPvs = pvs;
                    }
                }

                listPpv[0, scenario] = peakTran;
                listPpv[1, scenario] = peakVert;
                listPpv[2, scenario] = peakLong;
                listPpv[3, scenario] = peakPvs;
            }

            double ppvTranFull = 0;
            double ppvVertFull = 0;
            double ppvLongFull = 0;
            double ppvPvs = 0;
            int optTranIndex = 0;
            int optVertIndex = 0;
            int optLongIndex = 0;
            int optPvsIndex = 0;

            for (int scenario = 0; scenario < scenarioCount; scenario++)
            {
                double tran = listPpv[0, scenario];
                double vert = listPpv[1, scenario];
                double lon = listPpv[2, scenario];
                double pvs = listPpv[3, scenario];

                if (scenario == 0 || Math.Abs(tran) < Math.Abs(ppvTranFull))
                {
                    ppvTranFull = tran;
                    optTranIndex = scenario;
                }

                if (scenario == 0 || Math.Abs(vert) < Math.Abs(ppvVertFull))
                {
                    ppvVertFull = vert;
                    optVertIndex = scenario;
                }

                if (scenario == 0 || Math.Abs(lon) < Math.Abs(ppvLongFull))
                {
                    ppvLongFull = lon;
                    optLongIndex = scenario;
                }

                if (scenario == 0 || Math.Abs(pvs) < Math.Abs(ppvPvs))
                {
                    ppvPvs = pvs;
                    optPvsIndex = scenario;
                }
            }

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

        public static double GetPeakAbs(double[] values)
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

        public static double[] ExtractWave(double[,] source, int row, int length)
        {
            double[] data = new double[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = source[row, i];
            }
            return data;
        }
    }
}
