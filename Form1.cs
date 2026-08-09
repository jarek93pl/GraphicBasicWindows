using System.Windows.Forms;

namespace GraphicBasicWindows
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }
        public Size ImagesSize
        {
            set
            {
                float pastX = pictureBox1.Size.Width;
                float scaler = pastX / value.Width;
                
                pictureBox1.Size = value;
                pictureBox2.Size = value;
                panel1.VerticalScroll.Value = (int)(panel1.VerticalScroll.Value / scaler);
                panel1.HorizontalScroll.Value = (int)(panel1.HorizontalScroll.Value / scaler);
            }
        }
        public PictureBox MainImage { get => pictureBox1; }
        public Form1(Action<Form1> action, Bitmap source)
        {
            InitializeComponent();
            pictureBox2.Size = source.Size;
            pictureBox1.Size = source.Size;
            pictureBox2.Image = source;
            checkBox1_CheckedChanged(this, EventArgs.Empty);
            action(this);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                pictureBox1.Visible = false;
                pictureBox2.Visible = true;
            }
            else
            {
                pictureBox1.Visible = true;
                pictureBox2.Visible = false;
            }
        }

        private void panel1_Scroll(object sender, ScrollEventArgs e)
        {

        }
    }
}
