using Simulation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Simulation.Kernel;

namespace HandwritingRecognition
{
    public partial class MainForm : Form
    {
        public Brain Brain;
        public byte[][] Images;
        public byte[] Labels;

        private bool loading;
        private int ImageSize = 28;
        private int count;
        private int outputNum;
        private Thread trainThread;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            loading = true;
            Brain = new Brain(28 * 28, 10, 2, 16);
            Thread loadThread = new Thread(new ThreadStart(loadData));
            loadThread.Start();

            void loadData()
            {
                Images = ParseDatabase.ParseImages(Application.StartupPath + @"\train-images-idx3-ubyte");
                Labels = ParseDatabase.ParseLabels(Application.StartupPath + @"\train-labels-idx1-ubyte");

                loading = false;
            }
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            if (!loading)
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                Bitmap bmp = ParseDatabase.ByteToBitmap(Images[count], ImageSize, ImageSize);
                Rectangle rec = new Rectangle(10, 10, 200, 200);
                e.Graphics.DrawImage(bmp, rec);
                bmp.Dispose();
            }
        }

        private void train_button_Click(object sender, EventArgs e)
        {
            count++;
            if (trainThread == null)
            {
                trainThread = new Thread(new ThreadStart(training));
                trainThread.Start();
            }
            float[] output = Brain.Think(ParseDatabase.ByteToFloat(Images[count]));
            outputNum = Training.OutputNumber(output);
            outputNum_label.Text = outputNum.ToString();
            show_output(output);

            this.Refresh();
        }

        private void training()
        {
            float tweakAmount = 0.001F;
            count = 0;
            bool Active = true;

            while (Active)
            {
                float[] output = Brain.Think(ParseDatabase.ByteToFloat(Images[count]));
                outputNum = Training.OutputNumber(output);
                float cost = Training.CalculateCost(output, Labels[count]);
                Training.Backpropagate(Brain, tweakAmount, outputNum, Labels[count]);

                Console.WriteLine(cost);

                this.Invoke(new MethodInvoker(delegate {
                    this.Refresh();
                    outputNum_label.Text = outputNum.ToString();
                    show_output(output); 
                }));

                count++;

                if (count >= 59999)
                {
                    count = 0;
                    string path = Application.StartupPath + @"\save.brain";
                    File.Create(path).Close();
                    File.WriteAllText(path, Brain.BrainStructure);

                    DialogResult res = MessageBox.Show("Finished", "Training", MessageBoxButtons.OKCancel);
                    if (res.HasFlag(DialogResult.Cancel))
                    {
                        Active = false;
                    }
                }
            }
        }

        private void show_output(float[] output)
        {
            output_label.Text = string.Empty;

            for (int i = 0; i < output.Length; i++)
            {
                output_label.Text += i + ": " + output[i].ToString("0.00") + "\n";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = Application.StartupPath + @"\save.brain";
            File.Create(path).Close();
            File.WriteAllText(path, Brain.StringifyBrainStructure());
        }
    }
}
