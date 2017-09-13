using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Coloriz
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private Bitmap reserve;
        public void OpenPicture()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open);
                Image img = Image.FromStream(fs);
                fs.Close();
                pictureBox1.Image = img;
                //reserve = img.Clone() as Bitmap;
            }
        }

        private void ToolStripMenuItemOpenPicture_Click(object sender, EventArgs e)
        {
            OpenPicture();
        }

        private void buttonSegmentation_Click(object sender, EventArgs e)
        {
            Picture p = new Picture(pictureBox1.Image as Bitmap);
            p.GaussBlur((int)numericUpDownBlurRadius.Value);
            pictureBox1.Image = p.Segmentation((double)numericUpDownSegmentationThreshold.Value) as Image;
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            pictureBox1.Size = new Size(this.Width - 254, this.Height - 117);
        }
    }
}
