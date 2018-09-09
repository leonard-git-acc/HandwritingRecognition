using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Simulation
{
    public class Brain : ICloneable
    {
        public string BrainStructureString;
        public Stream BrainStructureStream;
        public Neuron[][] AllLayers;
        public Neuron[][] HiddenLayers;
        public Neuron[] Inputs;
        public Neuron[] Outputs;

        private Func<float, float> activationFunction;

        public Brain()
        {

        }

        public Brain(int inputCount, int outputCount, int layerCount, int neuronsPerLayer, float minWeightVal, float maxWeightVal, bool addDisconnectedWeights, Func<float, float> activationFunc)
        {
            activationFunction = activationFunc;
            GenerateNeurons(inputCount, outputCount, layerCount, neuronsPerLayer);
            GenerateBrainStructure(minWeightVal, maxWeightVal, addDisconnectedWeights);
            // Save structure as string
            BrainStructureString = StringifyBrainStructure();
        }

        public Brain(string brainStructure, int inputCount, int outputCount, int layerCount, int neuronsPerLayer, Func<float, float> activationFunc)
        {
            activationFunction = activationFunc;
            GenerateNeurons(inputCount, outputCount, layerCount, neuronsPerLayer);
            ParseBrainStructurString(brainStructure);
            BrainStructureString = StringifyBrainStructure();
        }

        public Brain(Stream brainStructure, Func<float, float> activationFunc)
        {
            activationFunction = activationFunc;
            ParseStructureStream(brainStructure);
        }

        public float[] Think(float[] input, Func<float, float> outFunc) // Compute inputs with the network and return outputs
        {
            EraseMemory();
            //read in all given inputs and set the value of the input neurons
            for (int i = 0; i < Inputs.Length; i++)
            {
                Inputs[i].SetValue(Inputs[i], input[i]);
            }

            //get all computed data from output neurons
            float[] output = new float[Outputs.Length];
            for (int i = 0; i < Outputs.Length; i++)
            {
                output[i] = outFunc(Outputs[i].Activation);
            }

            return output;
        }



        public MemoryStream BuildStructureStream()
        {
            MemoryStream structure = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(structure);

            binaryWriter.Write(AllLayers.Length); // Layer Amount

            for (int iter = 0; iter < AllLayers.Length; iter++)
            {
                binaryWriter.Write(AllLayers[iter].Length); // Neuron Amount
            }

            for (int iteration = 1; iteration < AllLayers.Length; iteration++)//which layer
            {
                for (int iter = 0; iter < AllLayers[iteration].Length; iter++)//which neuron
                {
                    Neuron targetNeuron = AllLayers[iteration][iter]; // neuron, that gets output

                    for (int i = 0; i < targetNeuron.Weight.Length; i++)
                    {
                        binaryWriter.Write(targetNeuron.Weight[i]);
                    }
                }
            }

            return structure;
        }

        private void ParseStructureStream(Stream structure)
        {
            BinaryReader reader = new BinaryReader(structure);

            int layerAmount = reader.ReadInt32();
            int inputCount = reader.ReadInt32();
            int[] hiddenCount = new int[layerAmount - 2];

            for (int i = 0; i < layerAmount - 2; i++)
            {
                hiddenCount[i] = reader.ReadInt32();
            }

            int outputCount = reader.ReadInt32();

            GenerateNeurons(inputCount, outputCount, layerAmount - 2, hiddenCount[0]);

            for (int i = 0; i < inputCount; i++)
            {
                Inputs[i].OutputConnections = HiddenLayers[0];
            }

            for (int iteration = 0; iteration < hiddenCount.Length; iteration++)
            {
                for (int iter = 0; iter < hiddenCount[iteration]; iter++)
                {
                    Neuron targetNeuron = HiddenLayers[iteration][iter];
                    targetNeuron.InputConnections = AllLayers[iteration];
                    targetNeuron.OutputConnections = AllLayers[iteration + 2];

                    for (int i = 0; i < targetNeuron.Weight.Length; i++)
                    {
                        targetNeuron.Weight[i] = reader.ReadSingle();
                    }
                }
            }

            for (int iter = 0; iter < outputCount; iter++)
            {
                Neuron targetNeuron = Outputs[iter];
                targetNeuron.InputConnections = HiddenLayers[HiddenLayers.Length - 1];

                for (int i = 0; i < targetNeuron.Weight.Length; i++)
                {
                    targetNeuron.Weight[i] = reader.ReadSingle();
                }
            }

            structure.Seek(0, SeekOrigin.Begin);
        }

        private void EraseMemory() // Resets all neuron activations
        {
            for (int iter = 0; iter < AllLayers.Length; iter++)
            {
                for (int i = 0; i < AllLayers[iter].Length; i++)
                {
                    AllLayers[iter][i].Activation = 0.0F;
                    AllLayers[iter][i].receivedInputs = 0;
                }
            }
        }

        private void GenerateNeurons(int inputCount, int outputCount, int layerCount, int neuronsPerLayer)
        {
            // Create container for neurons
            Inputs = new Neuron[inputCount];
            Outputs = new Neuron[outputCount];
            HiddenLayers = new Neuron[layerCount][];

            // Generate input neurons
            for (int i = 0; i < Inputs.Length; i++)
            {
                Inputs[i] = new Neuron();
                Inputs[i].Type = Neuron.NeuronType.InputNeuron;
                Inputs[i].ID = "Input" + i;
                Inputs[i].LayerIndex = i;
                Inputs[i].InputConnections = new Neuron[inputCount];
                Inputs[i].Weight = new float[neuronsPerLayer];
                Inputs[i].ActivationFunction = activationFunction;
            }

            //Generate output neurons
            for (int i = 0; i < Outputs.Length; i++)
            {
                Outputs[i] = new Neuron();
                Outputs[i].Type = Neuron.NeuronType.OutputNeuron;
                Outputs[i].ID = "Output" + i;
                Outputs[i].LayerIndex = i;
                Outputs[i].InputConnections = new Neuron[neuronsPerLayer];
                Outputs[i].Weight = new float[neuronsPerLayer];
                Outputs[i].ActivationFunction = activationFunction;
            }

            //Generate input neurons
            for (int iteration = 1; iteration < HiddenLayers.Length; iteration++)
            {
                HiddenLayers[iteration] = new Neuron[neuronsPerLayer];
                for (int i = 0; i < HiddenLayers[iteration].Length; i++)
                {
                    HiddenLayers[iteration][i] = new Neuron();
                    HiddenLayers[iteration][i].Type = Neuron.NeuronType.HiddenNeuron;
                    HiddenLayers[iteration][i].ID = "Neuron" + i;
                    HiddenLayers[iteration][i].LayerIndex = i;
                    HiddenLayers[iteration][i].InputConnections = new Neuron[neuronsPerLayer];
                    HiddenLayers[iteration][i].Weight = new float[neuronsPerLayer];
                    HiddenLayers[iteration][i].ActivationFunction = activationFunction;
                }
            }

            //Generate input neurons of the first layer, to configure them with the inputs
            HiddenLayers[0] = new Neuron[neuronsPerLayer];
            for (int i = 0; i < HiddenLayers[0].Length; i++)
            {
                HiddenLayers[0][i] = new Neuron();
                HiddenLayers[0][i].Type = Neuron.NeuronType.HiddenNeuron;
                HiddenLayers[0][i].ID = "Neuron" + i;
                HiddenLayers[0][i].LayerIndex = i;
                HiddenLayers[0][i].InputConnections = new Neuron[inputCount];
                HiddenLayers[0][i].Weight = new float[inputCount];
                HiddenLayers[0][i].ActivationFunction = activationFunction;
            }

            AllLayers = new Neuron[HiddenLayers.Length + 2][];
            Array.Copy(HiddenLayers, 0, AllLayers, 1, HiddenLayers.Length);
            AllLayers[0] = Inputs;
            AllLayers[AllLayers.Length - 1] = Outputs;

        }

        private void GenerateBrainStructure(float minWeightVal, float maxWeightVal, bool addDisconnectedWeights) // Randomly generates the weights of the neuron
        {
            int min = (int)(minWeightVal * 100);
            int max = (int)(maxWeightVal * 100);

            for (int iteration = 0; iteration < AllLayers.Length - 1; iteration++) // which layer
            {
                for (int iter = 0; iter < AllLayers[iteration].Length; iter++) // which neuron
                {
                    Neuron targetNeuron = AllLayers[iteration][iter]; // neuron, that gets outputs
                    int outputCount = AllLayers[iteration + 1].Length; // Amount of outputs
                    targetNeuron.OutputConnections = new Neuron[outputCount];
                    targetNeuron.Bias = RandomNumber.Between(-10, 10);

                    for (int i = 0; i < AllLayers[iteration + 1].Length; i++)
                    {
                        Neuron outputNeuron = AllLayers[iteration + 1][i]; // Neuron that will receive input
                        int inputCount = outputNeuron.InputConnectionsCount;

                        targetNeuron.OutputConnections[i] = outputNeuron;

                        outputNeuron.InputConnections[inputCount] = AllLayers[iteration][iter];
                        outputNeuron.Weight[inputCount] = RandomNumber.Between(min, max) / 100.0F;

                        if (addDisconnectedWeights)
                            outputNeuron.Weight[inputCount] *= RandomNumber.Between(-1, 1);

                        outputNeuron.InputConnectionsCount++;
                    }
                }
            }
        }

        public string StringifyBrainStructure() // Stringify the weights of the neurons
        {
            string outputInfromation = "";
            for (int iteration = 0; iteration < AllLayers.Length; iteration++)//which layer
            {
                outputInfromation += "\nL" + iteration + ":\n";

                for (int iter = 0; iter < AllLayers[iteration].Length; iter++)//which neuron
                {
                    Neuron targetNeuron = AllLayers[iteration][iter]; // neuron, that gets output
                    outputInfromation += "\n\tN" + iter + ":\n";

                    for (int i = 0; i < targetNeuron.Weight.Length; i++)
                    {
                        outputInfromation += "\n\t\tW" + i + ":" + targetNeuron.Weight[i];
                    }
                }
            }

            return outputInfromation;
        }

        private void ParseBrainStructurString(string _brainStructure) // Parse a given brainStructure string
        {
            string brainStructure = _brainStructure.Replace("\n", string.Empty).Replace("\t", string.Empty);
            string brainStructureSubstring = brainStructure;

            string[] stringLayers = new string[brainStructure.Count(c => c == 'L')];
            string[][] stringNeurons = new string[stringLayers.Length][];
            float[][][] weights = new float[stringLayers.Length][][];

            for (int i = 0; i < stringLayers.Length; i++) // all Layers
            {
                int start = brainStructureSubstring.IndexOf('L');
                int end = brainStructureSubstring.Substring(start + 1).IndexOf('L');
                if (end < 0)
                    end = brainStructureSubstring.Length - 1;

                string str = brainStructureSubstring.Substring(start, end - start + 1);
                brainStructureSubstring = brainStructureSubstring.Replace(str, string.Empty);
                stringLayers[i] = str;
            }

            for (int iter = 0; iter < stringLayers.Length; iter++) // all Neurons
            {
                stringNeurons[iter] = new string[stringLayers[iter].Count(c => c == 'N')];
                string layerSubstring = stringLayers[iter].Substring(3);

                for (int i = 0; i < stringNeurons[iter].Length; i++)
                {
                    int start = layerSubstring.IndexOf('N');
                    int end = layerSubstring.Substring(start + 1).IndexOf('N');
                    if (end < 0)
                        end = layerSubstring.Length - 1;

                    string str = layerSubstring.Substring(start, end - start + 1);
                    layerSubstring = layerSubstring.Replace(str, string.Empty);
                    stringNeurons[iter][i] = str;
                }
            }

            for (int iteration = 0; iteration < stringLayers.Length; iteration++) // all Weights
            {
                weights[iteration] = new float[stringNeurons[iteration].Length][];
                string layerSubstring = stringLayers[iteration].Substring(3);

                for (int iter = 0; iter < stringNeurons[iteration].Length; iter++)
                {
                    weights[iteration][iter] = new float[stringNeurons[iteration][iter].Count(c => c == 'W')];
                    string neuronSubstring = stringNeurons[iteration][iter].Substring(stringNeurons[iteration][iter].IndexOf(':') + 1);

                    for (int i = 0; i < weights[iteration][iter].Length; i++)
                    {
                        int start = neuronSubstring.IndexOf('W');
                        int end = neuronSubstring.Substring(start + 1).IndexOf('W');
                        if (end < 0)
                            end = neuronSubstring.Length - 1;

                        string str = neuronSubstring.Substring(start, end - start + 1);
                        neuronSubstring = neuronSubstring.Replace(str, string.Empty);
                        weights[iteration][iter][i] = float.Parse(str.Substring(str.IndexOf(':') + 1));
                    }

                    AllLayers[iteration][iter].InputConnections = AllLayers[Math.Max(0, iteration - 1)];
                    AllLayers[iteration][iter].InputConnectionsCount = AllLayers[Math.Max(0, iteration - 1)].Length - 1;
                    AllLayers[iteration][iter].OutputConnections = AllLayers[Math.Min(AllLayers.Length - 1, iteration + 1)];
                    AllLayers[iteration][iter].Weight = weights[iteration][iter];
                }
            }
        }

        public object Clone()
        {
            Brain brain = new Brain();
            brain.BrainStructureString = this.BrainStructureString;
            brain.Inputs = new Neuron[this.Inputs.Length];
            brain.Outputs = new Neuron[this.Outputs.Length];
            brain.HiddenLayers = new Neuron[this.HiddenLayers.Length][];
            brain.AllLayers = new Neuron[this.HiddenLayers.Length + 2][];

            brain.activationFunction = this.activationFunction;

            for (int i = 0; i < Inputs.Length; i++)
            {
                brain.Inputs[i] = this.Inputs[i].Clone() as Neuron;
            }

            for (int i = 0; i < Outputs.Length; i++)
            {
                brain.Outputs[i] = this.Outputs[i].Clone() as Neuron;
            }

            for (int iter = 0; iter < HiddenLayers.Length; iter++)
            {
                brain.HiddenLayers[iter] = new Neuron[HiddenLayers[iter].Length];

                for (int i = 0; i < HiddenLayers[iter].Length; i++)
                {
                    brain.HiddenLayers[iter][i] = this.HiddenLayers[iter][i].Clone() as Neuron;
                }
            }

            Array.Copy(brain.HiddenLayers, 0, brain.AllLayers, 1, brain.HiddenLayers.Length);
            brain.AllLayers[0] = brain.Inputs;
            brain.AllLayers[AllLayers.Length - 1] = brain.Outputs;

            for (int iter = 0; iter < brain.AllLayers.Length; iter++)
            {
                for (int i = 0; i < brain.AllLayers[iter].Length; i++)
                {
                    Neuron neuron = brain.AllLayers[iter][i];
                    neuron.InputConnections = brain.AllLayers[Math.Max(0, iter - 1)];
                    neuron.OutputConnections = brain.AllLayers[Math.Min(brain.AllLayers.Length - 1, iter + 1)];
                }
            }

            return brain;
        }
    }

    public class Neuron : ICloneable
    {
        public enum NeuronType { HiddenNeuron, InputNeuron, OutputNeuron }
        public string ID; // Name
        public int LayerIndex; // Index in the layer
        public NeuronType Type;
        public Neuron[] InputConnections;
        public int InputConnectionsCount = 0;
        public Neuron[] OutputConnections;

        public float Activation = 0.0F;
        public float Bias;
        public float[] Weight;
        public int receivedInputs = 0;

        public Func<float, float> ActivationFunction;
        public float Delta_i;

        public void SetValue(Neuron sender, float value)
        {
            if (Type == NeuronType.InputNeuron)
            {
                Activation = value;
                SendValue();
            }

            else if (Type == NeuronType.OutputNeuron)
            {
                int i = sender.LayerIndex;
                Activation += value * Weight[i];
            }

            else
            {
                int i = sender.LayerIndex;
                Activation += value * Weight[i];
                receivedInputs++;

                if (receivedInputs == InputConnections.Length)
                {
                    SendValue();
                }
            }
        }

        private void SendValue()
        {
            for (int i = 0; i < OutputConnections.Length; i++)
            {

                if (!float.IsNaN(ActivationFunction(Activation)))
                    OutputConnections[i].SetValue(this, ActivationFunction(Activation));
                else
                    OutputConnections[i].SetValue(this, 0);
            }
        }

        public static float Sigmoid(float x) // sigmoid function
        {
            return (float)(1 / (1 + Math.Exp(-x)));
        }

        public static float ReLU(float x)
        {
            return (float)Math.Max(0, x);
        }


        public object Clone()
        {
            Neuron neuron = new Neuron();
            neuron.Type = this.Type;
            neuron.ID = this.ID;
            neuron.LayerIndex = this.LayerIndex;
            neuron.Bias = this.Bias;
            neuron.Weight = new float[this.Weight.Length];
            neuron.ActivationFunction = this.ActivationFunction;

            for (int i = 0; i < Weight.Length; i++)
            {
                neuron.Weight[i] = this.Weight[i];
            }

            return neuron;
        }
    }
}
