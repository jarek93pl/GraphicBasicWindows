using GraphicBasicWindows.Graphicdll;
using Graphicdll;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static Graphicdll.GraphicProcesing;

namespace GraphicBasicWindows
{
    public unsafe partial class ContrastBrightnessSaturation : Form
    {
        public int numberTheard = Environment.ProcessorCount;
        public ContrastBrightnessSaturation()
        {
            timerPerformance.Start();
            consoleReader.Start();
        }
        byte[] imageR, imageG, imageB, imageAvg;
        public ContrastBrightnessSaturation(PictureBox picture, Bitmap source, Bitmap sourceOrginalSize, float minStaturation, float maxSaturation, float minExpo, float maxExpo, float minContrast, float maxContrast, int minTemp, int maxTemp, int minTinta, int maxTinta)
        {
            InitializeComponent();
            Picture = picture;
            Source = source;
            Copy = new Bitmap(source);
            SourceorginalSize = sourceOrginalSize;
            picture.Image = Copy;
            lockedSource = Source.LockBits(new Rectangle(0, 0, Copy.Width, Copy.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            long avgt = 0;
            GraphicProcesingForConstImage.ComputeAvg(Copy, lockedSource, out imageR, out imageG, out imageB, out imageAvg, ref avgt);
            this.AvgValue = avgt;
            MinStaturation = minStaturation;
            GapSaturation = (maxSaturation - minStaturation) / staturTrack.Maximum;
            MinExpo = minExpo;
            GapExpo = (maxExpo - minExpo) / expotrack.Maximum;
            MinContrast = minContrast;
            GapContrast = (maxContrast - minContrast) / contrastTrac.Maximum;
            tintaBar.Minimum = minTinta;
            tintaBar.Maximum = maxTinta;
            tempBar.Minimum = minTemp;
            tempBar.Maximum = maxTemp;
            ResetParameters();
        }
        BitmapData lockedSource;
        public PictureBox Picture { get; }
        public Bitmap Source { get; }
        public Bitmap SourceorginalSize { get; }
        public Bitmap Copy { get; }
        public float MinStaturation { get; }
        public float GapSaturation { get; }
        public float MinExpo { get; }
        public float GapExpo { get; }
        public float MinContrast { get; }
        public float GapContrast { get; }

        public long AvgValue { get; set; }
        private void staturTrack_DataContextChanged(object sender, EventArgs e)
        {

        }
        List<long> times = new List<long>();
        private async void staturTrack_ValueChanged(object sender, EventArgs e)
        {
            float saturation, exposytion, contrast;
            int temperature, tinta;
            LoadParameters(out saturation, out exposytion, out contrast, out temperature, out tinta);
            BasicEditing4Parameter(Copy, exposytion, saturation, contrast, temperature, tinta);
            textBox1.Lines = new string[]   {
                String.Format($"contrast : {contrast:0.00},saturation: {saturation:0.00} exposytion {exposytion:0.00} temperature {temperature:0.00} tint{tinta:0.00} "),
                String.Format($"setcolor;{saturation:0.00};{exposytion:0.00};{contrast:0.00};{temperature:0};{tinta:0} ")

                    };
            Picture.Refresh();


        }
        void LoadParameters(out float saturation, out float exposytion, out float contrast, out int temperature, out int tinta)
        {
            saturation = staturTrack.Value * GapSaturation + MinStaturation;
            exposytion = expotrack.Value * GapExpo + MinExpo;
            contrast = contrastTrac.Value * GapContrast + MinContrast;
            temperature = tempBar.Value;
            tinta = tintaBar.Value;
        }
        void setParameters(float saturation, float exposytion, float contrast, int temperature, int tinta)
        {
            SetProperty((x) => staturTrack.Value = x, saturation, GapSaturation, MinStaturation);
            SetProperty((x) => expotrack.Value = x, exposytion, GapExpo, MinExpo);
            SetProperty((x) => contrastTrac.Value = x, contrast, GapContrast, MinContrast);
            SetProperty((x) => tempBar.Value = x, temperature, 1, 0);
            SetProperty((x) => tintaBar.Value = x, tinta, 1, 0);
        }
        public void SetProperty(Action<int> propertyForm, float value, float gapValue, float MinValue)
        {
            propertyForm((int)((value - MinValue) / gapValue));
        }
        public unsafe void BasicEditing4Parameter(Bitmap Obraz, float exposytion, float saturaion, float contrast, int temperature, int tinta)
        {
            int brightMutilper = 1;
            int bitSiwthNumberForExposytion;
            CalcuateBright(exposytion, out brightMutilper, out bitSiwthNumberForExposytion);
            Vector<short> brightMultipeV = new Vector<short>((short)brightMutilper);
            Vector<short> avgImageV = new Vector<short>((short)AvgValue);
            Vector<float> contrastV = new Vector<float>(contrast);
            Vector<float> saturaionV = new Vector<float>(saturaion);
            Vector<short> vR = new Vector<short>((short)(-temperature - tinta));
            Vector<short> vG = new Vector<short>((short)(tinta * 2));
            Vector<short> vB = new Vector<short>((short)(temperature - tinta));
            Stopwatch stopwatch = Stopwatch.StartNew();
            var rectangle = Obraz.Size;
            BitmapData bp = Obraz.LockBits(new Rectangle(0, 0, Obraz.Width, Obraz.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int sizeVector = Vector<byte>.Count;
            int limitX = (bp.Width / sizeVector) * sizeVector;
            Enumerable.Range(0, rectangle.Height).AsParallel().WithDegreeOfParallelism(numberTheard).ForAll(y =>
            {
                int startPoint = y * rectangle.Width;
                rgb* kr = (rgb*)((byte*)(bp.Scan0 + y * bp.Stride));
                for (int i = 0; i < limitX; i += sizeVector)
                {

                    Vector<short> SC1G, SC2G;
                    Vector<short> APUS1G, APUS2G;
                    {
                        Vector<short> APS1;
                        Vector<short> APS2;
                        Vector<byte> AP = new Vector<byte>(new ReadOnlySpan<byte>(imageAvg, i + startPoint, sizeVector));
                        Vector<ushort> APUS1;
                        Vector<ushort> APUS2;
                        Vector.Widen(AP, out APUS1, out APUS2);
                        APS1 = APUS1.As<ushort, short>();
                        APS2 = APUS2.As<ushort, short>();

                        APUS1G = APS1; APUS2G = APS2;
                        APS1 -= avgImageV;
                        APS2 -= avgImageV;
                        Vector<int> SFC1, SFC2, SFC3, SFC4;
                        Vector.Widen(APS1, out SFC1, out SFC2);
                        Vector.Widen(APS2, out SFC3, out SFC4);
                        Vector<float> SFC1F = Vector.ConvertToSingle(SFC1), SFC2F = Vector.ConvertToSingle(SFC2);
                        Vector<float> SFC3F = Vector.ConvertToSingle(SFC3), SFC4F = Vector.ConvertToSingle(SFC4);
                        SFC1F *= contrastV;
                        SFC2F *= contrastV;
                        SFC3F *= contrastV;
                        SFC4F *= contrastV;
                        SFC1 = Vector.ConvertToInt32(SFC1F);
                        SFC2 = Vector.ConvertToInt32(SFC2F);
                        SFC3 = Vector.ConvertToInt32(SFC3F);
                        SFC4 = Vector.ConvertToInt32(SFC4F);
                        SC1G = Vector.Narrow(SFC1, SFC2);
                        SC2G = Vector.Narrow(SFC3, SFC4);
                    }
                    Vector<byte> colorR;
                    {
                        var span = new ReadOnlySpan<byte>(imageR, i + startPoint, sizeVector);
                        colorR = LoadColor(brightMultipeV, saturaionV, vR, SC1G, SC2G, APUS1G, APUS2G, span, bitSiwthNumberForExposytion);
                    }

                    Vector<byte> colorG;
                    {
                        var span = new ReadOnlySpan<byte>(imageG, i + startPoint, sizeVector);
                        colorG = LoadColor(brightMultipeV, saturaionV, vG, SC1G, SC2G, APUS1G, APUS2G, span, bitSiwthNumberForExposytion);
                    }
                    Vector<byte> colorB;
                    {
                        var span = new ReadOnlySpan<byte>(imageB, i + startPoint, sizeVector);
                        colorB = LoadColor(brightMultipeV, saturaionV, vB, SC1G, SC2G, APUS1G, APUS2G, span, bitSiwthNumberForExposytion);
                    }
                    for (int indexSave = 0; indexSave < sizeVector; indexSave++, kr++)
                    {
                        rgb color;
                        color.r = colorR[indexSave];
                        color.g = colorG[indexSave];
                        color.b = colorB[indexSave];
                        *kr = color;

                    }
                }

                rgb* inKr = (rgb*)((byte*)(lockedSource.Scan0 + y * bp.Stride));
                inKr += limitX;
                int width = rectangle.Width;
                for (int x = limitX; x < width; x++, kr++, inKr++)
                {
                    ComputePixel(exposytion, saturaion, contrast, temperature, tinta, this.AvgValue, inKr, kr);

                }
            });

            Obraz.UnlockBits(bp);
            times.Add(stopwatch.ElapsedMilliseconds);

        }

        private static void CalcuateBright(float exposytion, out int brightMutilper, out int bitSiwthNumberForExposytion)
        {
            bitSiwthNumberForExposytion = 7;
            if (exposytion > 2)
            {
                exposytion /= 4;
                bitSiwthNumberForExposytion = 5;
            }
            else if (exposytion > 1)
            {
                exposytion /= 2;
                bitSiwthNumberForExposytion = 6;
            }
            brightMutilper = (int)(128 * exposytion);
            brightMutilper--;

        }

        private static Vector<byte> LoadColor(Vector<short> brightMultipe, Vector<float> saturaionV, Vector<short> vR, Vector<short> SC1G, Vector<short> SC2G, Vector<short> APUS1G, Vector<short> APUS2G, ReadOnlySpan<byte> spanIn, int bitSiwthNumberForExposytion)
        {
            Vector<short> brightSift = new Vector<short>((short)8);
            Vector<short> vMax255 = new Vector<short>(255);
            Vector<short> rVS1, rVS2;
            Vector<short> rVS1C, rVS2C;
            {
                Vector<byte> rV = new Vector<byte>(spanIn);
                Vector<ushort> rVSU1, rVSU2;
                Vector.Widen(rV, out rVSU1, out rVSU2);
                rVS1 = rVSU1.As<ushort, short>();
                rVS2 = rVSU2.As<ushort, short>();
            }
            rVS1C = rVS1;
            rVS2C = rVS2;
            rVS1 -= APUS1G;//(*kr).r - avgPixel)
            rVS2 -= APUS2G;
            {
                Vector<short> rVSC1, rVSC2;
                rVSC1 = rVS1;
                rVSC2 = rVS2;
                Vector<float> rVSF1, rVSF2, rVSF3, rVSF4;
                Vector<int> rVIU1, rVIU2, rVIU3, rVIU4;
                Vector.Widen(rVSC1, out rVIU1, out rVIU2);
                Vector.Widen(rVSC2, out rVIU3, out rVIU4);
                rVSF1 = Vector.ConvertToSingle(rVIU1);
                rVSF2 = Vector.ConvertToSingle(rVIU2);
                rVSF3 = Vector.ConvertToSingle(rVIU3);
                rVSF4 = Vector.ConvertToSingle(rVIU4);
                rVSF1 *= saturaionV;
                rVSF2 *= saturaionV;
                rVSF3 *= saturaionV;
                rVSF4 *= saturaionV;
                rVIU1 = Vector.ConvertToInt32(rVSF1);
                rVIU2 = Vector.ConvertToInt32(rVSF2);
                rVIU3 = Vector.ConvertToInt32(rVSF3);
                rVIU4 = Vector.ConvertToInt32(rVSF4);

                rVSC1 = Vector.Narrow(rVIU1, rVIU2);
                rVSC2 = Vector.Narrow(rVIU3, rVIU4);

                rVS1 = Vector.Negate(rVS1);
                rVS2 = Vector.Negate(rVS2);
                rVS1 += rVSC1;
                rVS2 += rVSC2;

            }
            rVS1 += rVS1C;
            rVS2 += rVS2C;
            rVS1 += vR;
            rVS2 += vR;
            rVS1 += SC1G;
            rVS2 += SC2G;
            rVS1 = Vector.Max(rVS1, Vector<short>.Zero);
            rVS2 = Vector.Max(rVS2, Vector<short>.Zero);
            rVS1 = Vector.Min(rVS1, vMax255);
            rVS2 = Vector.Min(rVS2, vMax255);
            rVS1 = Vector.Multiply<short>(rVS1, brightMultipe);
            rVS2 = Vector.Multiply<short>(rVS2, brightMultipe);
            rVS1 = Vector.ShiftRightArithmetic(rVS1, bitSiwthNumberForExposytion);
            rVS2 = Vector.ShiftRightArithmetic(rVS2, bitSiwthNumberForExposytion);
            rVS1 = Vector.Min(rVS1, vMax255);
            rVS2 = Vector.Min(rVS2, vMax255);
            Vector<byte> colorEnd;
            {
                Vector<ushort> rVSOutU1 = rVS1.As<short, ushort>(), rVSOutU2 = rVS2.As<short, ushort>();
                colorEnd = Vector.Narrow(rVSOutU1, rVSOutU2);
            }

            return colorEnd;
        }

        private void ContrastBrightnessSaturation_Load(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {

                float saturation, exposytion, contrast;
                int temperature, tinta;
                LoadParameters(out saturation, out exposytion, out contrast, out temperature, out tinta);
                Bitmap imagetoProcesTemp = (Bitmap)SourceorginalSize.Clone();
                GraphicProcesing.MultiThreadEditing4Parameter(imagetoProcesTemp, exposytion, saturation, contrast, temperature, tinta);
                imagetoProcesTemp.Save(saveFileDialog1.FileName);
                imagetoProcesTemp.Dispose();
            }
        }


        private void timerPerformance_Tick(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            if (times.Count > 0)
            {
                sb.Append("time :");
                foreach (var item in times)
                {
                    sb.Append(item.ToString()); ;
                    sb.Append(",");
                }
                Console.WriteLine(sb.ToString());
                times.Clear();

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ResetParameters();
        }

        private void ResetParameters()
        {
            setParameters(saturation: 1, exposytion: 1, contrast: 0, temperature: 0, tinta: 0);
        }

        Task<string> pastReadConsoleTask;
        private async void consoleReader_Tick(object sender, EventArgs e)
        {
            if (pastReadConsoleTask != null)
            {
                if (pastReadConsoleTask.IsCompleted)
                {
                    string[] strings = pastReadConsoleTask.Result.Split(";");
                    if (strings.Length > 0)
                    {
                        switch (strings[0])
                        {
                            case "numberthread":
                                if (strings.Length > 1)
                                {
                                    Console.WriteLine($"exec1");
                                    SetNumberThread(new Span<string>(strings, 1, 1));
                                }
                                break;
                            case "setcolor":
                                if (strings.Length > 5)
                                {
                                    Console.WriteLine($"exec2");
                                    SetColor(new Span<string>(strings, 1, 5));
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    StartNewConsoleReadTask();
                }
            }
            else
            {
                StartNewConsoleReadTask();
            }
        }

        private void SetColor(Span<string> span)
        {
            try
            {
                setParameters(saturation: Convert.ToSingle(span[0]), exposytion: Convert.ToSingle(span[1]), contrast: Convert.ToSingle(span[2]), temperature: (int)Convert.ToSingle(span[3]), tinta: (int)Convert.ToSingle(span[4]));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

            }
        }

        private void SetNumberThread(Span<string> span)
        {
            numberTheard = Convert.ToInt32(span[0]);
        }

        void StartNewConsoleReadTask()
        {
            pastReadConsoleTask = Task<string>.Run(() => Console.ReadLine());
        }
    }
}