using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProceduralItems
{

    public partial class Form1 : Form
    {
        Bitmap surface = new Bitmap(64, 64);
        public Form1()
        {
            InitializeComponent();
            ProceduralItem.LoadParts();
            numericUpDown1.Maximum = int.MaxValue;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            string[] names = ProceduralItem.AssembleNew(surface, ref i);
            numericUpDown1.Value = i;
            Random random = new Random(i);

            label2.Text = ProceduralName.Construct(i) + " " + ProceduralName.Upper(names[random.Next(0, names.Length)]);

            pictureBox1.InterpolationMode = InterpolationMode.NearestNeighbor;
            pictureBox1.Image = surface;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int i = (int)numericUpDown1.Value;
            string[] names = ProceduralItem.AssembleNew(surface, ref i);
            numericUpDown1.Value = i;
            Random random = new Random(i);

            label2.Text = ProceduralName.Construct(i) + " " + ProceduralName.Upper(names[random.Next(0, names.Length)]);
            label3.Text = random.Next(1, 21).ToString() + " Damage";
            label4.Text = random.Next(0, 3).ToString() + "." + random.Next(0, 10).ToString() + " Speed";
            pictureBox1.InterpolationMode = InterpolationMode.NearestNeighbor;
            pictureBox1.Image = surface;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            surface.Save(Path.Combine(Directory.GetCurrentDirectory(), "saved", label2.Text.Replace(" ", "_") + "-" + numericUpDown1.Value + ".png"), ImageFormat.Png);
        }
    }
}
