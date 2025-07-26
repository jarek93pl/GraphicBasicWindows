namespace GraphicBasicWindows
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Form1(Action<PictureBox> action)
        {
            InitializeComponent();
            action(pictureBox1);
        }
    }
}
