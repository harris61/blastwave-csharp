using LiveCharts;
using LiveCharts.Wpf; //The WPF controls
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

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
            //string[] data = System.IO.File.ReadAllLines(@"C:\Users\user\Desktop\Signature Wave\6.txt");
            int jumlahFile = 6;
            DirectoryInfo dir = new DirectoryInfo(@"C:\Users\user\Desktop\Signature Wave\");
            FileInfo[] files = dir.GetFiles("*.txt");
            int[] namaFileInt = new int[jumlahFile];
            string[] namaFileString = new string[jumlahFile];
            for (int f=0; f<jumlahFile; f++)
            {
                int value = 0;
                int pos = files[f].Name.IndexOf(".");
                namaFileString[f] = files[f].Name;
                string cutName = namaFileString[f].Remove(pos);
                Int32.TryParse(cutName, out value);
                namaFileInt[f] = value;
            }
            Array.Sort(namaFileInt);

            for (int c=0; c<jumlahFile; c++)
            {
                namaFileString[c] = namaFileInt[c].ToString();
            }

            //List<List<String>> dataList = new List<List<String>>(); //Creates new nested List
            //dataList.Add(new List<String>()); //Adds new sub List
            List<string[]> dataList = new List<string[]>();
            int panjangLine = 36654;
            string[] assignData = new string[panjangLine];
            for (int d=0; d<jumlahFile; d++)
            {
                assignData = System.IO.File.ReadAllLines($@"C:\Users\user\Desktop\Signature Wave\{namaFileString[d]}.txt");
                dataList.Add(assignData);
            }
            Console.WriteLine($"{dataList[0][0]} {dataList[1][1]} {dataList[2][2]}");
            //string[] data = System.IO.File.ReadAllLines($@"C:\Users\user\Desktop\Signature Wave\{namaFile[0]}");
            //Console.WriteLine($"{data[0]} {data[1]} {data[2]}");

            int startWave = 950;
            int endWave = 2450;
            int length = endWave - startWave;
            string[,] newDataString = new string[jumlahFile, length];
            for (int w=0; w<jumlahFile; w++)
            {
                for (int w1=0; w1<length; w1++)
                {
                    newDataString[w, w1] = dataList[w][w1+startWave];
                }
            }

            double[,] tranWavePart = new double[jumlahFile, length];
            double[,] vertWavePart = new double[jumlahFile, length];
            double[,] longWavePart = new double[jumlahFile, length];
            double[,] waveList = new double[3, length];
            for (int i1=0; i1<jumlahFile; i1++)
            {
                for (int i2=0; i2<length; i2++)
                {
                    string[] tokens = new string[4];
                    tokens = newDataString[i1, i2].Split(new string[] { "    	", "     ", "    " }, StringSplitOptions.None);
                    for (int i3=0; i3<3; i3++)
                    {
                        double value = 0;
                        if (Double.TryParse(tokens[i3], out value) && i3 == 0)
                        {
                            tranWavePart[i1, i2] = value;                            
                        }
                        if (Double.TryParse(tokens[i3], out value) && i3 == 1)
                        {
                            vertWavePart[i1, i2] = value;
                        }
                        if (Double.TryParse(tokens[i3], out value) && i3 == 2)
                        {
                            longWavePart[i1, i2] = value;
                        }
                    }
                }
            }

            double[] sumTran = new double[length];
            double[] sumVert = new double[length];
            double[] sumLong = new double[length];
            for (int j1=0; j1<jumlahFile; j1++)
            {
                for (int j2=0; j2<length; j2++)
                {
                    sumTran[j2] = sumTran[j2] + tranWavePart[j1, j2];
                    sumVert[j2] = sumVert[j2] + vertWavePart[j1, j2];
                    sumLong[j2] = sumLong[j2] + longWavePart[j1, j2];
                }
            }

            double[] tranWave = new double[length];
            double[] vertWave = new double[length];
            double[] longWave = new double[length];
            for (int k=0; k<length; k++)
            {
                tranWave[k] = sumTran[k]/jumlahFile;
                vertWave[k] = sumVert[k]/jumlahFile;
                longWave[k] = sumTran[k]/jumlahFile;
                Console.WriteLine(tranWave[k]);
            }

            //int input1 = Int32.Parse(textBox1.Text);
            //int input2 = Int32.Parse(textBox2.Text);
            //int lineLength = input2 - input1;

            /*int startWave = 950;
            int endWave = 2450;
            int length = endWave - startWave;
            double[,] wave = new double[length, 4];
            double[] tranWave = new double[length];
            double[] vertWave = new double[length];
            double[] longWave = new double[length];

            string[] newData = new string[length];
            for (int a = 0; a < length; a++)
            {
                newData[a] = data[a + startWave];
            }

            for (int i=0; i<length; i++)
            {
                string[] tokens = new string[4];
                tokens = newData[i].Split(new string[] { "    	", "     ", "    " }, StringSplitOptions.None);

                for (int j = 0; j < 3; j++)
                {
                    double value = 0;
                    if (Double.TryParse(tokens[j], out value))
                    {
                        wave[i, j] = value;
                    }
                }
                tranWave[i] = wave[i, 0];
                vertWave[i] = wave[i, 1];
                longWave[i] = wave[i, 2];
                Console.WriteLine($"{tranWave[i]} {vertWave[i]} {longWave[i]}");
            }*/

            cartesianChart1.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Transversal Wave",
                    Values = new ChartValues<double> (tranWave),
                    LineSmoothness = 0,
                    PointGeometry = null,
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(3, 181, 252)),
                    Fill = System.Windows.Media.Brushes.Transparent,
                }
            };

            cartesianChart2.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Vertical Wave",
                    Values = new ChartValues<double> (vertWave),
                    LineSmoothness = 0,
                    PointGeometry = null,
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(24, 3, 252)),
                    Fill = System.Windows.Media.Brushes.Transparent,
                }
            };

            cartesianChart3.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Longitudinal Wave",
                    Values = new ChartValues<double> (longWave),
                    LineSmoothness = 0,
                    PointGeometry = null,
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(186, 3, 252)),
                    Fill = System.Windows.Media.Brushes.Transparent, 
                }
            };
        }

        private void Label2_Click(object sender, EventArgs e)
        {

        }
    }
}