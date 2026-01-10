using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            int jumlahFileWave = Int32.Parse(textBox1.Text);
            //int jumlahFileWave = 6;
            int lamaPengukuran = Int32.Parse(textBox7.Text);
            string mainFolder = "\\Blasting Data";
            string defaultDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + mainFolder;
            Console.WriteLine(defaultDir);
            DirectoryInfo dirWave = new DirectoryInfo($@"{defaultDir}\Signature Wave\");
            FileInfo[] filesWave = dirWave.GetFiles("*.txt");
            int[] namaFileIntWave = new int[jumlahFileWave];
            string[] namaFileStringWave = new string[jumlahFileWave];
            for (int a=0; a<jumlahFileWave; a++)
            {
                int valueWave = 0;
                int posWave = filesWave[a].Name.IndexOf(".");
                namaFileStringWave[a] = filesWave[a].Name;
                string cutNameWave = namaFileStringWave[a].Remove(posWave);
                Int32.TryParse(cutNameWave, out valueWave);
                namaFileIntWave[a] = valueWave;
            }
            Array.Sort(namaFileIntWave);

            for (int b=0; b<jumlahFileWave; b++)
            {
                namaFileStringWave[b] = namaFileIntWave[b].ToString();
            }

            List<string[]> dataListWave = new List<string[]>();
            int panjangLine = 36654;
            string[] assignDataWave = new string[panjangLine];
            for (int c=0; c<jumlahFileWave; c++)
            {
                //assignDataWave = System.IO.File.ReadAllLines($@"C:\Users\user\Desktop\Signature Wave\{namaFileStringWave[c]}.txt");
                assignDataWave = System.IO.File.ReadAllLines($@"{defaultDir}\Signature Wave\{namaFileStringWave[c]}.txt");
                dataListWave.Add(assignDataWave);
            }

            int defaultsps = 1024;
            //int sps = 4096;
            int sps = Int32.Parse(textBox5.Text);
            int rasiosps = sps / defaultsps;
            int startWave = 270*rasiosps;
            int endWave = lamaPengukuran*rasiosps;
            Console.WriteLine(endWave);
            int lengthWave = endWave - startWave;
            string[,] newDataStringWave = new string[jumlahFileWave, lengthWave];
            for (int d1=0; d1<jumlahFileWave; d1++)
            {
                for (int d2=0; d2<lengthWave; d2++)
                {
                    newDataStringWave[d1, d2] = dataListWave[d1][d2+startWave];
                }
            }

            double[,] tranWavePart = new double[jumlahFileWave, lengthWave];
            double[,] vertWavePart = new double[jumlahFileWave, lengthWave];
            double[,] longWavePart = new double[jumlahFileWave, lengthWave];
            double[,] waveList = new double[3, lengthWave];
            for (int e1=0; e1<jumlahFileWave; e1++)
            {
                for (int e2=0; e2<lengthWave; e2++)
                {
                    string[] tokens = new string[4];
                    tokens = newDataStringWave[e1, e2].Split(new string[] { "    	", "     ", "    " }, StringSplitOptions.None);
                    for (int e3=0; e3<3; e3++)
                    {
                        double value = 0;
                        if (Double.TryParse(tokens[e3], out value) && e3 == 0)
                        {
                            tranWavePart[e1, e2] = value;                            
                        }
                        if (Double.TryParse(tokens[e3], out value) && e3 == 1)
                        {
                            vertWavePart[e1, e2] = value;
                        }
                        if (Double.TryParse(tokens[e3], out value) && e3 == 2)
                        {
                            longWavePart[e1, e2] = value;
                        }
                    }
                }
            }

            double[] sumTran = new double[lengthWave];
            double[] sumVert = new double[lengthWave];
            double[] sumLong = new double[lengthWave];
            for (int f1=0; f1<jumlahFileWave; f1++)
            {
                for (int f2=0; f2<lengthWave; f2++)
                {
                    sumTran[f2] = sumTran[f2] + tranWavePart[f1, f2];
                    sumVert[f2] = sumVert[f2] + vertWavePart[f1, f2];
                    sumLong[f2] = sumLong[f2] + longWavePart[f1, f2];
                }
            }

            double[] tranWave = new double[lengthWave];
            double[] vertWave = new double[lengthWave];
            double[] longWave = new double[lengthWave];
            for (int g=0; g<lengthWave; g++)
            {
                tranWave[g] = sumTran[g]/jumlahFileWave;
                vertWave[g] = sumVert[g]/jumlahFileWave;
                longWave[g] = sumLong[g]/jumlahFileWave;
            }

            int jumlahFileDelay = Int32.Parse(textBox2.Text);
            //int jumlahFileDelay = 8;
            //DirectoryInfo dirDelay = new DirectoryInfo(@"C:\Users\user\Desktop\Delay Scenario\");
            DirectoryInfo dirDelay = new DirectoryInfo($@"{defaultDir}\Delay Scenario\");
            FileInfo[] filesDelay = dirDelay.GetFiles("*.txt");
            int[] namaFileIntDelay = new int[jumlahFileDelay];
            string[] namaFileStringDelay = new string[jumlahFileDelay];
            for (int h = 0; h < jumlahFileDelay; h++)
            {
                int valueDelay = 0;
                int posDelay = filesDelay[h].Name.IndexOf(".");
                namaFileStringDelay[h] = filesDelay[h].Name;
                string cutNameDelay = namaFileStringDelay[h].Remove(posDelay);
                Int32.TryParse(cutNameDelay, out valueDelay);
                namaFileIntDelay[h] = valueDelay;
            }
            Array.Sort(namaFileIntDelay);

            for (int i = 0; i < jumlahFileDelay; i++)
            {
                namaFileStringDelay[i] = namaFileIntDelay[i].ToString();
            }

            List<string[]> dataListStringDelay = new List<string[]>();
            for (int j = 0; j < jumlahFileDelay; j++)
            {
                var lineCount1 = System.IO.File.ReadAllLines($@"{defaultDir}\Delay Scenario\{namaFileStringDelay[j]}.txt").Length;
                string[] assignDataDelay = new string[lineCount1];
                assignDataDelay = System.IO.File.ReadAllLines($@"{defaultDir}\Delay Scenario\{namaFileStringDelay[j]}.txt");
                dataListStringDelay.Add(assignDataDelay);
            }

            int[] listLineDelay = new int[jumlahFileDelay];
            int startDelay = 1;
            List<string[]> newStringDataDelay = new List<string[]>();
            for (int k1 = 0; k1 < jumlahFileDelay; k1++)
            {
                var lineCount2 = System.IO.File.ReadAllLines($@"{defaultDir}\Delay Scenario\{namaFileStringDelay[k1]}.txt").Length;
                int lengthDelay = lineCount2-startDelay;
                listLineDelay[k1] = lengthDelay;
                string[] assignNewStringDataDelay = new string[lengthDelay];
                for (int k2=0; k2<lengthDelay; k2++)
                {
                    assignNewStringDataDelay[k2] = dataListStringDelay[k1][k2 + startDelay];
                    
                }
                newStringDataDelay.Add(assignNewStringDataDelay);
            }

            List<int[]> dataListIntDelay = new List<int[]>();
            for (int l1=0; l1<jumlahFileDelay; l1++)
            {
                int[] assignNewIntDataDelay = new int[listLineDelay[l1]];
                for (int l2=0; l2<listLineDelay[l1]; l2++)
                {
                    int values = 0;
                    Int32.TryParse(newStringDataDelay[l1][l2], out values);
                    assignNewIntDataDelay[l2] = values;
                }
                dataListIntDelay.Add(assignNewIntDataDelay);
            }

            int[] listMaxValueDelay = new int[jumlahFileDelay];
            int addLength = 0;
            List<int[]> newListDelay = new List<int[]>();
            for (int m1 = 0; m1 < jumlahFileDelay; m1++)
            {
                int[] sortDelay = new int[listLineDelay[m1]];
                for (int m2 = 0; m2 < listLineDelay[m1]; m2++)
                {
                    sortDelay[m2] = rasiosps*dataListIntDelay[m1][m2];
                }
                int maxValueDelay = sortDelay.Max();
                if (maxValueDelay>addLength)
                {
                    addLength = maxValueDelay;
                }
                listMaxValueDelay[m1] = maxValueDelay;
                Array.Sort(sortDelay);
                newListDelay.Add(sortDelay);
            }
            Console.WriteLine(addLength);
            DirectoryInfo dirWeight = new DirectoryInfo($@"{defaultDir}\Explosive Weight\");
            FileInfo[] filesWeight = dirDelay.GetFiles("*.txt");
            int[] namaFileIntWeight = new int[jumlahFileDelay];
            string[] namaFileStringWeight = new string[jumlahFileDelay];
            for (int n = 0; n < jumlahFileDelay; n++)
            {
                int valueWeight = 0;
                int posWeight = filesWeight[n].Name.IndexOf(".");
                namaFileStringWeight[n] = filesWeight[n].Name;
                string cutNameWeight = namaFileStringWeight[n].Remove(posWeight);
                Int32.TryParse(cutNameWeight, out valueWeight);
                namaFileIntWeight[n] = valueWeight;
            }
            Array.Sort(namaFileIntWeight);

            for (int o = 0; o < jumlahFileDelay; o++)
            {
                namaFileStringWeight[o] = namaFileIntWeight[o].ToString();
            }

            List<double[]> dataListWeight = new List<double[]>();
            for (int p = 0; p < jumlahFileDelay; p++)
            {
                var lineCount3 = System.IO.File.ReadAllLines($@"{defaultDir}\Explosive Weight\{namaFileStringWeight[p]}.txt").Length;
                string[] assignStringDataWeight = new string[lineCount3];
                double[] assignDoubleDataWeight = new double[lineCount3];
                assignStringDataWeight = System.IO.File.ReadAllLines($@"{defaultDir}\Explosive Weight\{namaFileStringWeight[p]}.txt");
                for (int p1 = 0; p1 < lineCount3; p1++)
                {
                    double valueWeight = 0;
                    Double.TryParse(assignStringDataWeight[p1], out valueWeight);
                    assignDoubleDataWeight[p1] = valueWeight;
                }
                dataListWeight.Add(assignDoubleDataWeight);
            }

            double konstantaLapangan = Double.Parse(textBox3.Text);
            //double konstantaLapangan = 2;
            double beratBahanPeledakSignature = Double.Parse(textBox4.Text);
            //double beratBahanPeledakSignature = 50;
            int waveLength = lengthWave + addLength + 100;
            double[,] listTranWave = new double[jumlahFileDelay, waveLength];
            double[,] listVertWave = new double[jumlahFileDelay, waveLength];
            double[,] listLongWave = new double[jumlahFileDelay, waveLength];
            double[,] listPVS = new double[jumlahFileDelay, waveLength];
            /*double distance1 = 143.97;
            double distance2 = 138.24;
            double distance3 = 130.95;
            double distance4 = 128.57;
            double distance5 = 122.81;
            double distance6 = 119.74;
            int jumlahDistance = 6;
            double averageDistance = (distance1 + distance2 + distance3 + distance4 + distance5 + distance6) / jumlahDistance;*/
            List<double[]> dataListDistanceAvg = new List<double[]>();
            var lineCount4 = System.IO.File.ReadAllLines($@"{defaultDir}\Simulation Distance\distanceaverage.txt").Length;
            double sumDistance = 0;
            string[] assignStringDataDistanceAvg = new string[lineCount4];
            double[] assignDoubleDataDistanceAvg = new double[lineCount4];
            assignStringDataDistanceAvg = System.IO.File.ReadAllLines($@"{defaultDir}\Simulation Distance\distanceaverage.txt");
            for (int jk = 0; jk < lineCount4; jk++)
            {
                double valueDistanceAvg = 0;
                Double.TryParse(assignStringDataDistanceAvg[jk], out valueDistanceAvg);
                assignDoubleDataDistanceAvg[jk] = valueDistanceAvg;
                sumDistance = sumDistance + assignDoubleDataDistanceAvg[jk];
            }
            dataListDistanceAvg.Add(assignDoubleDataDistanceAvg);

            List<double[]> dataListDistanceSimul = new List<double[]>();
            var lineCount5 = System.IO.File.ReadAllLines($@"{defaultDir}\Simulation Distance\distancesimulation.txt").Length;
            string[] assignStringDataDistanceSimul = new string[lineCount4];
            double[] assignDoubleDataDistanceSimul = new double[lineCount4];
            assignStringDataDistanceSimul = System.IO.File.ReadAllLines($@"{defaultDir}\Simulation Distance\distancesimulation.txt");
            for (int kj = 0; kj < lineCount5; kj++)
            {
                double valueDistanceSimul = 0;
                Double.TryParse(assignStringDataDistanceSimul[kj], out valueDistanceSimul);
                assignDoubleDataDistanceSimul[kj] = valueDistanceSimul;
            }
            dataListDistanceSimul.Add(assignDoubleDataDistanceSimul);
            
            //double averageDistance = Double.Parse(textBox5.Text); //130.71m
            //double simulatedDistance = Double.Parse(textBox6.Text);
            double averageDistance = sumDistance / lineCount4;
            //double distanceRatio = simulatedDistance / averageDistance;
            double[] distanceRatio = new double[lineCount5];
            for (int xx=0; xx<lineCount5; xx++)
            {
                distanceRatio[xx] = assignDoubleDataDistanceSimul[xx] / averageDistance;
            }

            for (int q=0; q<jumlahFileDelay; q++)
            {
                for (int q1=0; q1<listLineDelay[q]; q1++)
                {
                    int add = newListDelay[q][q1];
                    for (int q2=0; q2<lengthWave; q2++)
                    {
                        //Rumus PPV menggunakan USBM
                        listTranWave[q, q2 + add] = listTranWave[q, q2 + add] + (tranWave[q2] * (Math.Pow((dataListWeight[q][q1] / beratBahanPeledakSignature) / Math.Pow(distanceRatio[q], 2), 0.5 * konstantaLapangan)));
                        listVertWave[q, q2 + add] = listVertWave[q, q2 + add] + (vertWave[q2] * (Math.Pow((dataListWeight[q][q1] / beratBahanPeledakSignature) / Math.Pow(distanceRatio[q], 2), 0.5 * konstantaLapangan)));
                        listLongWave[q, q2 + add] = listLongWave[q, q2 + add] + (longWave[q2] * (Math.Pow((dataListWeight[q][q1] / beratBahanPeledakSignature) / Math.Pow(distanceRatio[q], 2), 0.5 * konstantaLapangan)));
                    }
                }
            }

            for (int z = 0; z < jumlahFileDelay; z++)
            {
                for (int z1 = 0; z1 < waveLength; z1++)
                {
                     listPVS[z, z1] = listPVS[z, z1] + Math.Sqrt(Math.Pow(listTranWave[z, z1], 2) + Math.Pow(listVertWave[z, z1], 2) + Math.Pow(listLongWave[z, z1], 2));
                }
            }


            double[,] listPPV = new double[4, jumlahFileDelay];
            for (int r=0; r<jumlahFileDelay; r++)
            {
                double[] tranPart = new double[waveLength];
                double[] vertPart = new double[waveLength];
                double[] longPart = new double[waveLength];
                double[] pvsPart = new double[waveLength];
                for (int r1=0; r1<waveLength; r1++)
                {
                    tranPart[r1] = listTranWave[r, r1];
                    vertPart[r1] = listVertWave[r, r1];
                    longPart[r1] = listLongWave[r, r1];
                    pvsPart[r1] = listPVS[r, r1];
                }
                listPPV[0, r] = tranPart.Max();
                if (Math.Abs(tranPart.Min()) > tranPart.Max())
                {
                    listPPV[0, r] = tranPart.Min();
                }
                listPPV[1, r] = vertPart.Max();
                if (Math.Abs(vertPart.Min()) > vertPart.Max())
                {
                    listPPV[1, r] = vertPart.Min();
                }
                listPPV[2, r] = longPart.Max();
                if (Math.Abs(longPart.Min()) > longPart.Max())
                {
                    listPPV[2, r] = longPart.Min();
                }
                listPPV[3, r] = pvsPart.Max();
                if (Math.Abs(pvsPart.Min()) > pvsPart.Max())
                {
                    listPPV[3, r] = pvsPart.Min();
                }
            }

            double[] tranParts = new double[jumlahFileDelay];
            double[] vertParts = new double[jumlahFileDelay];
            double[] longParts = new double[jumlahFileDelay];
            double[] pvsParts = new double[jumlahFileDelay];
            for (int s=0; s<jumlahFileDelay; s++)
            {
                tranParts[s] = listPPV[0, s];
                vertParts[s] = listPPV[1, s];
                longParts[s] = listPPV[2, s];
                pvsParts[s] = listPPV[3, s];
            }

            double ppvTranSignature = tranWave.Max();
            if (Math.Abs(tranWave.Min()) > tranWave.Max())
            {
                ppvTranSignature = tranWave.Min();
            }
            double ppvVertSignature = vertWave.Max();
            if (Math.Abs(vertWave.Min()) > vertWave.Max())
            {
                ppvVertSignature = vertWave.Min();
            }
            double ppvLongSignature = longWave.Max();
            if (Math.Abs(longWave.Min()) > longWave.Max())
            {
                ppvLongSignature = longWave.Min();
            }

            double ppvTranFull = tranParts.Min(element => Math.Abs(element));
            if (Array.IndexOf(tranParts, ppvTranFull) == -1)
            {
                ppvTranFull = ppvTranFull * -1;
            }
            double ppvVertFull = vertParts.Min(element => Math.Abs(element));
            if (Array.IndexOf(vertParts, ppvVertFull) == -1)
            {
                ppvVertFull = ppvVertFull * -1;
            }
            double ppvLongFull = longParts.Min(element => Math.Abs(element));
            if (Array.IndexOf(longParts, ppvLongFull) == -1)
            {
                ppvLongFull = ppvLongFull * -1;
            }
            double ppvPVS = pvsParts.Min(element => Math.Abs(element));
            if (Array.IndexOf(pvsParts, ppvPVS) == -1)
            {
                ppvPVS = ppvPVS * -1;
            }

            int indeksOptimumTran = Array.IndexOf(tranParts, ppvTranFull);
            int indeksOptimumVert = Array.IndexOf(vertParts, ppvVertFull);
            int indeksOptimumLong = Array.IndexOf(longParts, ppvLongFull);
            int indeksOptimumPVS = Array.IndexOf(pvsParts, ppvPVS);

            Console.WriteLine(lineCount4);
            Console.WriteLine(sumDistance);
            Console.WriteLine(averageDistance);
            Console.WriteLine(jumlahFileDelay);
            for (int kl=0; kl<lineCount5; kl++)
            {
                Console.WriteLine(distanceRatio[kl]);
            }

            double[] tesTran = new double[waveLength];
            double[] tesVert = new double[waveLength];
            double[] tesLong = new double[waveLength];
            double[] tesPVS = new double[waveLength];
            for (int tes=0; tes<waveLength; tes++)
            {
                tesTran[tes] = listTranWave[indeksOptimumTran, tes];
                tesVert[tes] = listVertWave[indeksOptimumVert, tes];
                tesLong[tes] = listLongWave[indeksOptimumLong, tes];
                tesPVS[tes] = listPVS[indeksOptimumPVS, tes];
            }

            label2.Text = $"Signature Wave(mm/s, 1/{sps} ms)";
            label1.Text = $"Optimized Full Blast Wave(mm/s, 1/{sps} ms)";
            label15.Text = $"Optimized Peak Vector Sum(mm/s, 1/{sps} ms)";
            label9.Text = $"Signature Transversal Wave PPV = {ppvTranSignature} mm/s";
            label10.Text = $"Signature Vertical Wave PPV = {ppvVertSignature} mm/s";
            label11.Text = $"Signature Longitudinal Wave PPV = {ppvLongSignature} mm/s";
            label12.Text = $"Skenario {indeksOptimumTran+1} Full Blast Transversal Wave PPV = {ppvTranFull} mm/s";
            label13.Text = $"Skenario {indeksOptimumVert+1} Full Blast Vertical Wave PPV = {ppvVertFull} mm/s";
            label14.Text = $"Skenario {indeksOptimumLong+1} Full Blast Longitudinal Wave PPV = {ppvLongFull} mm/s";
            label16.Text = $"Skenario {indeksOptimumPVS+1} Peak Vector Sum PPV = {ppvPVS} mm/s";

            dataGridView1.Columns.Add("Skenario", "Skenario Delay");
            dataGridView1.Columns.Add("Value", "PPV Tran (mm/s)");
            dataGridView1.Columns.Add("Value", "PPV Vert (mm/s)");
            dataGridView1.Columns.Add("Value", "PPV Long (mm/s)");
            dataGridView1.Columns.Add("Value", "PPV PVS (mm/s)");

            for (int sa = 0; sa < jumlahFileDelay; sa++)
            {
                dataGridView1.Rows.Add(new object[] { sa + 1, listPPV[0, sa], listPPV[1, sa], listPPV[2, sa], listPPV[3, sa]});
            }

            chart1.Series.Clear();
            var series1 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series1",
                Color = System.Drawing.Color.Turquoise,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line
            };

            this.chart1.Series.Add(series1);

            for (int i = 0; i < lengthWave; i++)
            {
                series1.Points.AddXY(i, tranWave[i]);
            }
            /*cartesianChart1.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = $"Transversal Wave Skenario {indeksOptimumTran}",
                    Values = new ChartValues<double> (tranWave),
                    LineSmoothness = 0,
                    PointGeometry = null,
                    StrokeThickness = 0.75,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(3, 181, 252)),
                    Fill = System.Windows.Media.Brushes.Transparent,
                }
            };*/
            chart2.Series.Clear();
            var series2 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series2",
                Color = System.Drawing.Color.Blue,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line
            };

            this.chart2.Series.Add(series2);

            for (int i = 0; i < lengthWave; i++)
            {
                series2.Points.AddXY(i, vertWave[i]);
            }
            /*cartesianChart2.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = $"Vertical Wave Skenario {indeksOptimumVert}",
                    Values = new ChartValues<double> (vertWave),
                    LineSmoothness = 0,
                    PointGeometry = null,
                    StrokeThickness = 0.75,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(24, 3, 252)),
                    Fill = System.Windows.Media.Brushes.Transparent,
                }
            };*/
            chart3.Series.Clear();
            var series3 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series3",
                Color = System.Drawing.Color.Purple,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line
            };

            this.chart3.Series.Add(series3);

            for (int i = 0; i < lengthWave; i++)
            {
                series3.Points.AddXY(i, longWave[i]);
            }

            /*cartesianChart3.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = $"Longitudinal Wave Skenario {indeksOptimumLong}",
                    Values = new ChartValues<double> (longWave),
                    LineSmoothness = 0,
                    PointGeometry = null,
                    StrokeThickness = 0.75,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(186, 3, 252)),
                    Fill = System.Windows.Media.Brushes.Transparent, 
                }
            };*/
            chart4.Series.Clear();
            var series4 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series4",
                Color = System.Drawing.Color.Turquoise,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line
            };

            this.chart4.Series.Add(series4);

            for (int i = 0; i < waveLength; i++)
            {
                series4.Points.AddXY(i, tesTran[i]);
            }
            /*cartesianChart4.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = $"Optimized Transversal Wave Skenario {indeksOptimumTran}",
                    Values = new ChartValues<double>(tesTran),
                    LineSmoothness = 0,
                    PointGeometry = null,
                    StrokeThickness = 0.75,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(3, 181, 252)),
                    Fill = System.Windows.Media.Brushes.Transparent,
                }
            };*/
            chart5.Series.Clear();
            var series5 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series5",
                Color = System.Drawing.Color.Blue,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line
            };

            this.chart5.Series.Add(series5);

            for (int i = 0; i < waveLength; i++)
            {
                series5.Points.AddXY(i, tesVert[i]);
            }
            /*cartesianChart5.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = $"Optimized Vertical Wave Skenario {indeksOptimumVert}",
                    Values = new ChartValues<double>(tesVert),
                    LineSmoothness = 0,
                    PointGeometry = null,
                    StrokeThickness = 0.75,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(24, 3, 252)),
                    Fill = System.Windows.Media.Brushes.Transparent,
                }
            };*/
            chart6.Series.Clear();
            var series6 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series6",
                Color = System.Drawing.Color.Purple,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line
            };

            this.chart6.Series.Add(series6);

            for (int i = 0; i < waveLength; i++)
            {
                series6.Points.AddXY(i, tesLong[i]);
            }
            /*cartesianChart6.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = $"Optimized Longitudinal Wave Skenario {indeksOptimumLong}",
                    Values = new ChartValues<double>(tesLong),
                    LineSmoothness = 0,
                    PointGeometry = null,
                    StrokeThickness = 0.75,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(186, 3, 252)),
                    Fill = System.Windows.Media.Brushes.Transparent,
                }
            };*/
            chart7.Series.Clear();
            var series7 = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Series7",
                Color = System.Drawing.Color.Green,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line
            };

            this.chart7.Series.Add(series7);

            for (int i = 0; i < waveLength; i++)
            {
                series7.Points.AddXY(i, tesPVS[i]);
            }
            /*cartesianChart7.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = $"Optimized Peak Vector Sum {indeksOptimumPVS}",
                    Values = new ChartValues<double>(tesPVS),
                    LineSmoothness = 0,
                    PointGeometry = null,
                    StrokeThickness = 0.75,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0)),
                    Fill = System.Windows.Media.Brushes.Transparent,
                }
            };*/
        }
    }
}