using Graphicdll;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static Graphicdll.GraphicProcesing;

namespace GraphicBasicWindows
{
    public partial class ContrastBrightnessSaturation : Form
    {
        public ContrastBrightnessSaturation()
        {
            timerPerformance.Start();
        }
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
            Graphicdll.GraphicProcesing.ComputeAvg(Copy, lockedSource, ref avgt);
            this.AvgValue = avgt;
            MinStaturation = minStaturation;
            GapSaturation = (maxSaturation - minStaturation) / staturTrack.Maximum;
            MinExpo = minExpo;
            GapExpo = (maxExpo - minExpo) / expotrack.Maximum;
            MinContrast = minContrast;
            GapContrast = (maxContrast - minContrast) / contrastTrac.Maximum;
            MinTemperature = minTemp;
            MinTinta = minTinta;
            staturTrack_ValueChanged(this, EventArgs.Empty);
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
        public int MinTemperature { get; }
        public int MinTinta { get; }

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
            textBox1.Text = String.Format($"contrast : {contrast:0.00},saturation: {saturation:0.00} exposytion {exposytion:0.00} temperature {temperature:0.00} tint{tinta:0.00} ");
            Picture.Refresh();


        }
        void LoadParameters(out float saturation, out float exposytion, out float contrast, out int temperature, out int tinta)
        {
            saturation = staturTrack.Value * GapSaturation + MinStaturation;
            exposytion = expotrack.Value * GapExpo + MinExpo;
            contrast = contrastTrac.Value * GapContrast + MinContrast;
            temperature = tempBar.Value + MinTemperature;
            tinta = tintaBar.Value + MinTinta;
        }
        void setParameters(float saturation, float exposytion, float contrast, int temperature, int tinta)
        {
            SetProperty((x) => staturTrack.Value = x, saturation, GapSaturation, MinStaturation);
            SetProperty((x) => expotrack.Value = x, exposytion, GapExpo, MinExpo);
            SetProperty((x) => contrastTrac.Value = x, contrast, GapContrast, MinContrast);
            SetProperty((x) => tempBar.Value = x, temperature, 1, MinTemperature);
            SetProperty((x) => tintaBar.Value = x, tinta, 1, MinTinta);
        }
        public void SetProperty(Action<int> propertyForm, float value, float gapValue, float MinValue)
        {
            propertyForm((int)((value - MinValue) / gapValue));
        }
        public unsafe void BasicEditing4Parameter(Bitmap Obraz, float exposytion, float saturaion, float contrast, int temperature, int tinta)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var rectangle = Obraz.Size;
            BitmapData bp = Obraz.LockBits(new Rectangle(0, 0, Obraz.Width, Obraz.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            Enumerable.Range(0, rectangle.Height).AsParallel().ForAll(y =>
            {

                rgb* kr = (rgb*)((byte*)(bp.Scan0 + y * bp.Stride));
                rgb* inKr = (rgb*)((byte*)(lockedSource.Scan0 + y * bp.Stride));
                int width = rectangle.Width;
                for (int x = 0; x < width; x++, kr++, inKr++)
                {
                    ComputePixel(exposytion, saturaion, contrast, temperature, tinta, this.AvgValue, inKr, kr);

                }
            });

            Obraz.UnlockBits(bp);
            times.Add(stopwatch.ElapsedMilliseconds);

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
            setParameters(saturation: 1, exposytion: 1, contrast: 0, temperature: 0, tinta: 0);
        }
    }
}
