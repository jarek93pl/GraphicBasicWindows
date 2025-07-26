namespace GraphicBasicWindows
{
    partial class ContrastBrightnessSaturation
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            contrastTrac = new TrackBar();
            expotrack = new TrackBar();
            staturTrack = new TrackBar();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            textBox1 = new TextBox();
            ((System.ComponentModel.ISupportInitialize)contrastTrac).BeginInit();
            ((System.ComponentModel.ISupportInitialize)expotrack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)staturTrack).BeginInit();
            SuspendLayout();
            // 
            // contrastTrac
            // 
            contrastTrac.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            contrastTrac.Location = new Point(12, 81);
            contrastTrac.Maximum = 200;
            contrastTrac.Name = "contrastTrac";
            contrastTrac.Size = new Size(776, 69);
            contrastTrac.TabIndex = 0;
            contrastTrac.Value = 100;
            contrastTrac.ValueChanged += staturTrack_ValueChanged;
            contrastTrac.DataContextChanged += staturTrack_DataContextChanged;
            // 
            // expotrack
            // 
            expotrack.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            expotrack.Location = new Point(12, 191);
            expotrack.Maximum = 200;
            expotrack.Name = "expotrack";
            expotrack.Size = new Size(776, 69);
            expotrack.TabIndex = 1;
            expotrack.Value = 100;
            expotrack.ValueChanged += staturTrack_ValueChanged;
            expotrack.DataContextChanged += staturTrack_DataContextChanged;
            // 
            // staturTrack
            // 
            staturTrack.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            staturTrack.Location = new Point(12, 298);
            staturTrack.Maximum = 200;
            staturTrack.Name = "staturTrack";
            staturTrack.Size = new Size(776, 69);
            staturTrack.TabIndex = 2;
            staturTrack.Value = 100;
            staturTrack.ValueChanged += staturTrack_ValueChanged;
            staturTrack.DataContextChanged += staturTrack_DataContextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(23, 31);
            label1.Name = "label1";
            label1.Size = new Size(79, 25);
            label1.TabIndex = 3;
            label1.Text = "Contrast";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(23, 153);
            label2.Name = "label2";
            label2.Size = new Size(103, 25);
            label2.TabIndex = 4;
            label2.Text = "Exposytion:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(23, 252);
            label3.Name = "label3";
            label3.Size = new Size(100, 25);
            label3.TabIndex = 5;
            label3.Text = "saturation :";
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.Location = new Point(12, 383);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(776, 31);
            textBox1.TabIndex = 6;
            // 
            // ContrastBrightnessSaturation
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(804, 469);
            Controls.Add(textBox1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(staturTrack);
            Controls.Add(expotrack);
            Controls.Add(contrastTrac);
            Name = "ContrastBrightnessSaturation";
            Text = "ContrastBrightnessSaturation";
            Load += ContrastBrightnessSaturation_Load;
            ((System.ComponentModel.ISupportInitialize)contrastTrac).EndInit();
            ((System.ComponentModel.ISupportInitialize)expotrack).EndInit();
            ((System.ComponentModel.ISupportInitialize)staturTrack).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TrackBar contrastTrac;
        private TrackBar expotrack;
        private TrackBar staturTrack;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox textBox1;
    }
}