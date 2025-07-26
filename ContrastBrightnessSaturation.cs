using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GraphicBasicWindows
{
    public partial class ContrastBrightnessSaturation : Form
    {
        public ContrastBrightnessSaturation()
        {

        }
        public ContrastBrightnessSaturation(PictureBox picture, Bitmap source, float minStaturation, float maxSaturation, float minExpo, float maxExpo, float minContrast, float maxContrast)
        {
            InitializeComponent();
            Picture = picture;
            Source = source;
            MinStaturation = minStaturation;
            GapSaturation = (maxSaturation - minStaturation) / staturTrack.Maximum;
            MinExpo = minExpo;
            GapExpo = (maxExpo - minExpo) / expotrack.Maximum;
            MinContrast = minContrast;
            GapContrast = (maxContrast - minContrast) / contrastTrac.Maximum;
            staturTrack_ValueChanged(this, EventArgs.Empty);
        }

        public PictureBox Picture { get; }
        public Bitmap Source { get; }
        public float MinStaturation { get; }
        public float GapSaturation { get; }
        public float MinExpo { get; }
        public float GapExpo { get; }
        public float MinContrast { get; }
        public float GapContrast { get; }

        private void staturTrack_DataContextChanged(object sender, EventArgs e)
        {

        }
        private async void staturTrack_ValueChanged(object sender, EventArgs e)
        {

            float saturation = staturTrack.Value * GapSaturation + MinStaturation;
            float exposytion = expotrack.Value * GapExpo + MinExpo;
            float contrast = contrastTrac.Value * GapContrast + MinContrast;
            Bitmap editedImage = await Task<Bitmap>.Factory.StartNew(() =>
            {
                Bitmap editedImage = new Bitmap(threadLocalBitmap.Value);
                Graphicdll.GraphicProcesing.BasicEditing4Parameter(editedImage, exposytion, saturation, contrast);

                last?.Dispose();
                last = editedImage;
                return editedImage;
            });
            try
            {
                textBox1.Text = saturation.ToString($"contrast : {contrast},saturation: {saturation} exposytion {exposytion} ");
                Picture.Image = editedImage;
            }
            catch (Exception ex)
            {
                textBox1.Text = "err";
            }
            GC.Collect();
        }

        private void ContrastBrightnessSaturation_Load(object sender, EventArgs e)
        {

        }
    }
}
