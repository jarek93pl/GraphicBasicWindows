using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        }
        public ContrastBrightnessSaturation(PictureBox picture, Bitmap source, float minStaturation, float maxSaturation, float minExpo, float maxExpo, float minContrast, float maxContrast, int minTemp, int maxTemp, int minTinta, int maxTinta)
        {
            InitializeComponent();
            Picture = picture;
            Source = source;
            Copy = new Bitmap(source);
            picture.Image = Copy;
            lockedSource = Source.LockBits(new Rectangle(0, 0, Copy.Width, Copy.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            long avgt = 0;
            Graphicdll.GraphicProcesing.ComputeAvg(Copy, lockedSource, ref avgt);
            this.Avg = avgt;
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
        public Bitmap Copy { get; }
        public float MinStaturation { get; }
        public float GapSaturation { get; }
        public float MinExpo { get; }
        public float GapExpo { get; }
        public float MinContrast { get; }
        public float GapContrast { get; }
        public int MinTemperature { get; }
        public int MinTinta { get; }
        public long Avg { get; set; }

        private void staturTrack_DataContextChanged(object sender, EventArgs e)
        {

        }
        private async void staturTrack_ValueChanged(object sender, EventArgs e)
        {

            float saturation = staturTrack.Value * GapSaturation + MinStaturation;
            float exposytion = expotrack.Value * GapExpo + MinExpo;
            float contrast = contrastTrac.Value * GapContrast + MinContrast;
            int temperature = tempBar.Value + MinTemperature;
            int tinta = tintaBar.Value + MinTinta;
            BasicEditing4Parameter(Copy, exposytion, saturation, contrast, temperature, tinta);
            textBox1.Text = String.Format($"contrast : {contrast:0.00},saturation: {saturation:0.00} exposytion {exposytion:0.00} temperature {temperature:0.00} ");
            Picture.Refresh();
        }
        public unsafe void BasicEditing4Parameter(Bitmap Obraz, float exposytion, float saturaion, float contrast, int temperature, int tinta)
        {
            var rectangle = Obraz.Size;
            BitmapData bp = Obraz.LockBits(new Rectangle(0, 0, Obraz.Width, Obraz.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            Enumerable.Range(0, rectangle.Height).AsParallel().ForAll(y =>
            {

                rgb* kr = (rgb*)((byte*)(bp.Scan0 + y * bp.Stride));
                rgb* inKr = (rgb*)((byte*)(lockedSource.Scan0 + y * bp.Stride));
                for (int x = 0; x < rectangle.Width; x++, kr++, inKr++)
                {
                    ComputePixel(exposytion, saturaion, contrast, temperature, tinta, this.Avg, inKr, kr);

                }
            });


            Obraz.UnlockBits(bp);

        }
        private void ContrastBrightnessSaturation_Load(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
