using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Simulation;

namespace HandwritingRecognition
{
    public partial class MainForm : Form
    {
        DrawPad Pad;
        Brain Brain;

        public MainForm()
        {
            InitializeComponent();

            Pad = new DrawPad();
            Pad.Location = new Point(0, 0);
            Pad.Size = new Size(600, 600);
            Pad.ImageSize = new Size(600, 600);
            Pad.LineWidth = 25;
            Pad.BackColor = Color.Black;
            Controls.Add(Pad);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //string structure = File.ReadAllText(@"network.brain");
            //Brain = new Brain(structure, 28 * 28, 10, 3, 16);
            Brain = new Brain(new FileStream(@"network.brainStream", FileMode.Open));
        }

        private void Ok_button_Click(object sender, EventArgs e)
        {
            Pad.RescaleImage(28, 28);
            Pad.CenterImage();
            Pad.Image.Save(@"img.png");

            float[] input = Pad.ImageToFloat();
            float[] output = Brain.Think(input);
            out_label.Text = Training.OutputNumber(output).ToString();
            output_label.Text = string.Empty;

            for (int i = 0; i < output.Length; i++)
            {
                output_label.Text += i + ": " + output[i].ToString("0.00") + "\n";
            }
        }

        private void Reset_button_Click(object sender, EventArgs e)
        {
            Pad.ResetImage();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            int padsize = this.Height;
            Pad.Size = new Size(padsize, padsize);

            if (this.WindowState == FormWindowState.Minimized)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
        }
    }
}
