namespace GraphicBasicWindows
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Form1(Action<PictureBox> action, Bitmap source)
        {
            InitializeComponent();
            pictureBox2.Image = source;
            checkBox1_CheckedChanged(this, EventArgs.Empty);
            action(pictureBox1);
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
    }
}
