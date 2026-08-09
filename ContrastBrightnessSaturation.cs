using GraphicBasicWindows.Graphicdll;
using Graphicdll;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
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
        public ContrastBrightnessSaturation(Form1 picture, Bitmap source, Bitmap sourceOrginalSize, float minStaturation, float maxSaturation, float minExpo, float maxExpo, float minContrast, float maxContrast, int minTemp, int maxTemp, int minTinta, int maxTinta)
        {
            Form1 = picture;
            InitializeComponent();
            Picture = picture.MainImage;
            Source = source;
            Copy = new Bitmap(source);
            SourceorginalSize = sourceOrginalSize;
            picture.MainImage.Image = Copy;
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
            ComputeTrackBarParameter();
        }
        private void ComputeTrackBarParameter()
        {
            const int MaxSizeOneDim = 20_000;
            factorToMultiple = 1f / ImageScaler.Value;
            int trackMax = ImageScaler.Maximum;
            float MaxScaler = trackMax * factorToMultiple;
            float maxWidth = MaxScaler * Copy.Width;
            float maxHeight = MaxScaler * Copy.Height;
            float max = Math.Max(maxWidth, maxHeight);
            if (max > MaxSizeOneDim)
            {
                factorToMultiple *= MaxSizeOneDim / max;
                ImageScaler.Value = (int)(1f / factorToMultiple);
            }
        }

        BitmapData lockedSource;
        public Form1 Form1 { get; set; }
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

            int contrastMutilper = 1;
            int contrastNumberForcontrast;

            int saturaionMutilper = 1;
            int saturaionNumberForsaturaion;

            bool contrastisPostive = contrast > 0;
            float ContrastforVector = Math.Abs(contrast);
            CalcuateProperty(exposytion, out brightMutilper, out bitSiwthNumberForExposytion);
            CalcuateProperty(ContrastforVector, out contrastMutilper, out contrastNumberForcontrast);
            CalcuateProperty(saturaion, out saturaionMutilper, out saturaionNumberForsaturaion);
            Vector<short> brightMultipeV = new Vector<short>((short)brightMutilper);
            Vector<short> avgImageV = new Vector<short>((short)AvgValue);
            Vector<short> contrastV = new Vector<short>((short)contrastMutilper);
            Vector<short> saturaionV = new Vector<short>((short)saturaionMutilper);
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

                        Vector<short> rVC1 = Vector.Abs(APS1), rVC2 = Vector.Abs(APS2);
                        rVC1 = Vector.Multiply<short>(rVC1, contrastV);
                        rVC2 = Vector.Multiply<short>(rVC2, contrastV);
                        rVC1 = Vector.ShiftRightArithmetic(rVC1, contrastNumberForcontrast);
                        rVC2 = Vector.ShiftRightArithmetic(rVC2, contrastNumberForcontrast);
                        SC1G = -rVC1;
                        SC2G = -rVC2;
                        SC1G = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(APS1, Vector<short>.Zero), rVC1, SC1G);
                        SC2G = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(APS2, Vector<short>.Zero), rVC2, SC2G);

                        if (!contrastisPostive)
                        {
                            SC1G = -SC1G;
                            SC2G = -SC2G;
                        }
                    }
                    Vector<byte> V0;
                    {
                        var span = new ReadOnlySpan<byte>(imageR, i + startPoint, sizeVector);
                        V0 = LoadColor(brightMultipeV, saturaionV, saturaionNumberForsaturaion, vR, SC1G, SC2G, APUS1G, APUS2G, span, bitSiwthNumberForExposytion);
                    }

                    Vector<byte> V1;
                    {
                        var span = new ReadOnlySpan<byte>(imageG, i + startPoint, sizeVector);
                        V1 = LoadColor(brightMultipeV, saturaionV, saturaionNumberForsaturaion, vG, SC1G, SC2G, APUS1G, APUS2G, span, bitSiwthNumberForExposytion);
                    }
                    Vector<byte> V2;
                    {
                        var span = new ReadOnlySpan<byte>(imageB, i + startPoint, sizeVector);
                        V2 = LoadColor(brightMultipeV, saturaionV, saturaionNumberForsaturaion, vB, SC1G, SC2G, APUS1G, APUS2G, span, bitSiwthNumberForExposytion);
                    }

                    byte* data = stackalloc byte[3 * sizeVector];
                    byte* dataC = data;
                    for (int indexSave = 0; indexSave < sizeVector; indexSave++, dataC += 3)
                    {
                        dataC[0] = V0[indexSave];
                        dataC[1] = V1[indexSave];
                        dataC[2] = V2[indexSave];

                    }
                    byte* point = (byte*)(nint)kr;
                    V0 = new Vector<byte>(new ReadOnlySpan<byte>(data, sizeVector));
                    V1 = new Vector<byte>(new ReadOnlySpan<byte>(data + sizeVector, sizeVector));
                    V2 = new Vector<byte>(new ReadOnlySpan<byte>(data + (sizeVector << 1), sizeVector));
                    Vector.StoreUnsafe(V0, ref point[0]);
                    Vector.StoreUnsafe(V1, ref point[sizeVector]);
                    Vector.StoreUnsafe(V2, ref point[sizeVector << 1]);
                    kr += sizeVector;
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

        private static void CalcuateProperty(float exposytion, out int brightMutilper, out int bitSiwthNumberForExposytion)
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

        }

        private static Vector<byte> LoadColor(Vector<short> brightMultipe, Vector<short> saturaionV, int saturationBitmove, Vector<short> vR, Vector<short> SC1G, Vector<short> SC2G, Vector<short> APUS1G, Vector<short> APUS2G, ReadOnlySpan<byte> spanIn, int bitSiwthNumberForExposytion)
        {
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

                Vector<short> sVC1 = Vector.Abs(rVSC1), sVC2 = Vector.Abs(rVSC2);
                sVC1 = Vector.Multiply<short>(sVC1, saturaionV);
                sVC2 = Vector.Multiply<short>(sVC2, saturaionV);
                sVC1 = Vector.ShiftRightArithmetic(sVC1, saturationBitmove);
                sVC2 = Vector.ShiftRightArithmetic(sVC2, saturationBitmove);
                Vector<short> NsVC1 = -sVC1;
                Vector<short> NsVC2 = -sVC2;
                rVSC1 = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(rVSC1, Vector<short>.Zero), sVC1, NsVC1);
                rVSC2 = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(rVSC2, Vector<short>.Zero), sVC2, NsVC2);



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
            rVS1 = Vector.Clamp(rVS1, Vector<short>.Zero, vMax255);
            rVS2 = Vector.Clamp(rVS2, Vector<short>.Zero, vMax255);
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

        float factorToMultiple;
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            Form1.ImagesSize = new Size((int)(factorToMultiple * (Copy.Width * ImageScaler.Value)), (int)(factorToMultiple * (Copy.Height * ImageScaler.Value)));
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }
    }
}