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
        private bool display;
        private bool trainingActive;
        private int ImageSize = 28;
        private int count;
        private int trainingAmount = 60000;
        private int totalSuccess = 0;
        private int good = 0;// kleine Statistik um die Akkuratheit zu messen
        private int outputNum;
        private Thread trainThread;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            loading = true;
            Brain = new Brain(28 * 28, 10, 3, 16, -0.5F, 0.5F, false, Neuron.ReLU);
            Brain = new Brain(new FileStream(@"network.brainStream", FileMode.Open), Neuron.ReLU);//new Brain(File.ReadAllText(@"network.brain"),28 * 28, 10, 3, 16);
            Thread loadThread = new Thread(new ThreadStart(loadData));
            loadThread.Start();

            trainingActive = true;

            void loadData()
            {
                if(trainingActive)
                {
                    trainingAmount = 60000;
                    Images = ParseDatabase.ParseImages( @"train-images.idx3-ubyte", trainingAmount);
                    Labels = ParseDatabase.ParseLabels( @"train-labels.idx1-ubyte", trainingAmount);
                }
                else
                {
                    trainingAmount = 10000;
                    Images = ParseDatabase.ParseImages(@"t10k-images.idx3-ubyte", trainingAmount);
                    Labels = ParseDatabase.ParseLabels(@"t10k-labels.idx1-ubyte", trainingAmount);
                }

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
            float[] output = Brain.Think(ParseDatabase.ByteToFloat(Images[count]), Neuron.ReLU);
            outputNum = Training.OutputNumber(output);


            if (Labels[count] == outputNum) good++;
            float accuracy = good / (count + 1);
            Console.Write(" accuracy: " + accuracy + "\r");
            outputNum_label.Text = outputNum+ " accuracy: " + accuracy;
            show_output(output);

            this.Refresh();
        }

        private void training()
        {
            float tweakAmount = 0.0005F;
            count = 0;
            bool Active = true;

            while (Active)
            {
                float[] output = Brain.Think(ParseDatabase.ByteToFloat(Images[count]), Neuron.ReLU);
                outputNum = Training.OutputNumber(output);
                float cost = Training.CalculateCost(output, Labels[count]);
                Training.Backpropagate(Brain, tweakAmount, outputNum, Labels[count]);
                int expected = Labels[count];

                // Normalerweise trennt man die Prüfdaten von den Trainingsdaten,
                // um zu gucken ob das Netz nicht nur auswendig lernt, sondern auch generalisiert
                if (count % 1000 == 0) good = 0; //reset statistic every 1000 steps
                if (expected == outputNum)
                {
                    totalSuccess++;
                    good++;
                }

                double accuracy = good / (count % 1000 + 1.0);
                int percent = (int) (100 * accuracy);

                int totalAccuracy = Convert.ToInt32(totalSuccess * 100 / (count + 1));

                Console.Write("accuracy: " + percent + "% cost: "+ cost + "total:" + totalAccuracy +  "% \r");

                if(display)
                {
                    this.Invoke(new MethodInvoker(delegate {
                        this.Refresh();
                        outputNum_label.Text =outputNum + " accuracy: " + percent+ "%";
                        show_output(output); 
                    }));
                }

                count++;

                if (count >= trainingAmount - 1)
                {
                    count = 0;
                    totalSuccess = 0;
                    string path =  @"save.brain";
                    File.Create(path).Close();
                    File.WriteAllText(path, Brain.BrainStructureString);

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

        private void save_button_Click(object sender, EventArgs e)
        {
            /*string path = @"save.brain";
            File.Create(path).Close();
            File.WriteAllText(path, Brain.StringifyBrainStructure());*/

            FileStream fstream = new FileStream(@"save.brainStream", FileMode.Create);
            MemoryStream stream = Brain.BuildStructureStream();
            stream.WriteTo(fstream);
            fstream.Close();
        }

        private void display_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            display = display_checkBox.Checked;
        }
    }
}