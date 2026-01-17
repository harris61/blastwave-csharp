using System;
using System.IO;

namespace BlastWaveCSharp
{
    internal static class FileParsing
    {
        public static int GetSamplingRate(FileInfo signatureFile)
        {
            string[] lines = File.ReadAllLines(signatureFile.FullName);
            if (!TryGetSignatureMetadata(lines, out int samplingRate, out _))
            {
                throw new InvalidOperationException($"Invalid signature header: {signatureFile.Name}");
            }

            return samplingRate;
        }

        public static bool TryGetSignatureMetadata(string[] lines, out int samplingRate, out int dataStartIndex)
        {
            samplingRate = 0;
            dataStartIndex = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = NormalizeLine(lines[i]);
                if (samplingRate == 0 && TryParseSampleRateLine(line, out int value))
                {
                    samplingRate = value;
                }

                if (dataStartIndex < 0 && TryParseWaveLine(line, out _, out _, out _))
                {
                    dataStartIndex = i;
                }
            }

            return samplingRate > 0 && dataStartIndex >= 0;
        }

        public static bool TryParseSampleRateLine(string line, out int samplingRate)
        {
            samplingRate = 0;
            line = NormalizeLine(line);
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            string normalized = line.Replace(" ", string.Empty);
            if (!normalized.StartsWith("SampleRate", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            int colonIndex = line.IndexOf(':');
            string valuePart = colonIndex >= 0
                ? line[(colonIndex + 1)..]
                : line;

            int firstDigit = valuePart.IndexOfAny("0123456789".ToCharArray());
            if (firstDigit < 0)
            {
                return false;
            }

            valuePart = valuePart[firstDigit..].Trim();

            string[] tokens = valuePart.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                return false;
            }

            return int.TryParse(tokens[0], out samplingRate) && samplingRate > 0;
        }

        public static bool TryParseWaveLine(string line, out double tran, out double vert, out double lon)
        {
            tran = 0;
            vert = 0;
            lon = 0;

            line = NormalizeLine(line);
            string[] tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 3)
            {
                return false;
            }

            return double.TryParse(tokens[0], out tran)
                && double.TryParse(tokens[1], out vert)
                && double.TryParse(tokens[2], out lon);
        }

        public static string NormalizeLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return string.Empty;
            }

            return line.Trim().Trim('"');
        }
    }
}
