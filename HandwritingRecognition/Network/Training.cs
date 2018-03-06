using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation
{
    public static class Training
    {
        public static int OutputNumber(float[] networkOutputs)
        {
            int num = 0;
            float highestFloat = 0.0F;

            for (int i = 0; i < networkOutputs.Length; i++)
            {
                if (networkOutputs[i] > highestFloat)
                {
                    highestFloat = networkOutputs[i];
                    num = i;
                }
            }

            return num;
        }

        public static float CalculateCost(float[] networkOutputs, int targetOutput)
        {
            float cost = 0.0F;

            float[] targetOutputs = new float[networkOutputs.Length];
            targetOutputs[targetOutput] = 1.0F;

            for (int i = 0; i < networkOutputs.Length; i++)
            {
                cost += Math.Max(0, networkOutputs[i]) - targetOutputs[i];
            }

            return cost;
        }

        public static float[][][] SumChanges(float[][][] total, float[][][] current)
        {
            float[][][] sum = new float[total.Length][][];

            for (int layerNum = 0; layerNum < total.Length; layerNum++)
            {
                sum[layerNum] = new float[total[layerNum].Length][];

                for (int neuronNum = 0; neuronNum < total[layerNum].Length; neuronNum++)
                {
                    sum[layerNum][neuronNum] = new float[total[layerNum][neuronNum].Length];

                    for (int weightNum = 0; weightNum < total[layerNum][neuronNum].Length; weightNum++)
                    {
                        sum[layerNum][neuronNum][weightNum] = total[layerNum][neuronNum][weightNum] + current[layerNum][neuronNum][weightNum];
                    }
                }
            }

            return sum;
        }

        public static void ApplyChanges(Brain brain, float[][][][] changes)
        {
            Neuron[][] layers = new Neuron[brain.HiddenLayers.Length + 1][];
            Array.Copy(brain.AllLayers, 1, layers, 0, brain.AllLayers.Length - 1);

            for (int layerNum = 0; layerNum < layers.Length; layerNum++)
            {
                Neuron[] layer = layers[layerNum];

                for (int neuronNum = 0; neuronNum < layer.Length; neuronNum++)
                {
                    Neuron neuron = layer[neuronNum];

                    for (int weightNum = 0; weightNum < neuron.Weight.Length; weightNum++)
                    {
                        float change = 0.0F;

                        for (int i = 0; i < changes.Length; i++)
                        {
                            change += changes[i][layerNum][neuronNum][weightNum];
                        }
                        change = change / changes.Length;

                        neuron.Weight[weightNum] += change;
                    }
                }
            }
        }

        static public float[][][] Backpropagate(Brain brain, float TweakAmount, int outputNum, int expectedNum)
        {
            Neuron[][] layers = new Neuron[brain.HiddenLayers.Length + 1][];
            Array.Copy(brain.AllLayers, 1, layers, 0, brain.AllLayers.Length - 1);
            float[] targetOutput = new float[10];
            targetOutput[expectedNum] = 1.0F;

            float[][][] allChanges = new float[layers.Length][][];

            for (int layerNum = layers.Length - 1; layerNum >= 0; layerNum--)
            {
                Neuron[] layer = layers[layerNum];
                float[][] neuronChanges = new float[layer.Length][];

                for (int neuronNum = 0; neuronNum < layer.Length; neuronNum++)
                {
                    Neuron neuron = layer[neuronNum];
                    float[] weightChanges = new float[neuron.Weight.Length];

                    for (int i = 0; i < neuron.Weight.Length; i++)
                    {
                        float deltaW = 0.0F;
                        if (neuron.Type == Neuron.NeuronType.HiddenNeuron)
                        {
                            float delta_i = Delta_i(layers, layerNum, neuronNum, targetOutput);
                            float activation_j = brain.AllLayers[layerNum][i].Activation;

                            deltaW = DeltaW(TweakAmount, delta_i, activation_j);
                        }
                        else
                        {
                            float delta_i = Delta_i(neuron, targetOutput[neuronNum], (float)Math.Tanh(neuron.Activation));
                            float activation_j = brain.AllLayers[layerNum][i].Activation;
                            deltaW = DeltaW(TweakAmount, delta_i, activation_j);
                        }

                        if (!float.IsNaN(deltaW))
                        {
                            weightChanges[i] = deltaW;
                            neuron.Weight[i] += deltaW;
                        }
                    }
                    neuronChanges[neuronNum] = weightChanges;
                }
                allChanges[layerNum] = neuronChanges;
            }

            return allChanges;
        }

        private static float DeltaW(float tweakAmount, float delta_i, float activation_j)
        {
            return tweakAmount * delta_i * activation_j;
        }

        private static float Delta_i(Neuron neuron, float targetOutput, float currentOutput)
        {
            neuron.Delta_i = derivative(neuron.Activation) * (targetOutput - currentOutput);
            return neuron.Delta_i;
        }

        private static float Delta_i(Neuron[][] layers, int targetLayer, int targetNeuron, float[] targetOutput)
        {
            Neuron neuron = layers[targetLayer][targetNeuron];
            int prevLayer = targetLayer + 1;

            float sum = 0.0F;
            for (int i = 0; i < layers[prevLayer].Length; i++)
            {
                Neuron prevNeuron = layers[prevLayer][i];
                sum += prevNeuron.Delta_i * prevNeuron.Weight[targetNeuron];
            }
            neuron.Delta_i = derivative(neuron.Activation) * sum;

            return neuron.Delta_i;
        }

        private static float derivative(float x)
        {
            //return (float)((4 * Math.Pow(Math.E, 2 * x + 2)) / Math.Pow(Math.Pow(Math.E, 2 * x) + Math.Pow(Math.E, 2), 2));
            return (float)((1 - Math.Tanh(x)));
            return Neuron.Sigmoid(x) * (1 - Neuron.Sigmoid(x));
        }
    }
}
