using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Kernel
{
    static class Kernel
    {
        public static float[] ConvolutionKernel(float[] img, int width, int height, float[][] kernel, int kernelStepSize, float divisor, float offset)
        {
            if (kernel.Length % 2 == 0)
            {
                throw new Exception("Only kernels with odd length allowed!");
            }

            float[][] image = StreamToImage(img, width, height);
            float[][] extendedImg = new float[image.Length + (kernel.Length - 1) / 2][];
            float kernelValue = 0.0F;
            int startVal = 0;

            int k = (kernel.Length - 1) / 2;
            for (int y = k; y < extendedImg.Length; y++)
            {
                extendedImg[y] = new float[image[y - k].Length + k];

                for (int x = k; x < extendedImg[y].Length; x++)
                {
                    extendedImg[y][x] = image[y - k][x - k];
                }
            }

            for (int i = 0; i < k; i++)
            {
                extendedImg[i] = image[i];
            }

            for (int iter = 0; iter < image.Length; iter++)
            {
                for (int i = 0; i < k; i++)
                {
                    extendedImg[iter + k][i] = image[iter][i];
                }
            }
            image = extendedImg;

            foreach (float[] item in kernel)
            {
                foreach (float val in item)
                {
                    if(val > 0)
                        kernelValue += val;
                }
            }

            int newHeight = height / kernelStepSize;
            int newWidth = width / kernelStepSize;

            float[][] newImage = new float[newHeight][];
            int heightCount = 0;
            for (int ynew = startVal; ynew < newHeight; ynew++)
            {
                newImage[ynew] = new float[newWidth];
                int widthCount = 0;

                for (int xnew = startVal; xnew < newWidth; xnew++)
                {
                    float sum = 0.0F;

                    for (int y = 0; y < kernel.Length; y++)
                    {
                        for (int x = 0; x < kernel.Length; x++)
                        {
                            sum += kernel[y][x] * image[Math.Min(y + heightCount, height - 1)][Math.Min(x + widthCount, width - 1)];
                        }
                    }
                    newImage[ynew][xnew] = Math.Abs(sum / kernelValue + offset);

                    widthCount = Math.Min(kernelStepSize + widthCount, width - 1);
                }
                heightCount = Math.Min(kernelStepSize + heightCount, height - 1);
            }

            return ImageToStream(newImage);
        }

        public static float[][] CreateKernel(int length)
        {
            float[][] kernel = new float[length][];

            for (int i = 0; i < length; i++)
            {
                kernel[i] = new float[length];
            }

            return kernel;
        }

        public static float[][] StreamToImage(float[] imgStream, int width, int height)
        {
            float[][] img = new float[height][];
            int pixelCount = 0;

            for (int y = 0; y < height; y++)
            {
                img[y] = new float[width];

                for (int x = 0; x < width; x++)
                {
                    img[y][x] = imgStream[pixelCount];
                    pixelCount++;
                }
            }

            return img;
        }

        public static float[] ImageToStream(float[][] img)
        {
            float[] imgStream = new float[img.Length * img[0].Length];
            int pixelCount = 0;

            for (int y = 0; y < img.Length; y++)
            {
                for (int x = 0; x < img[y].Length; x++)
                {
                    imgStream[pixelCount] = img[y][x];
                    pixelCount++;
                }
            }

            return imgStream;
        }

        public static float[][] BitmapToImage(Bitmap bitmap)
        {
            float[][] img = new float[bitmap.Height][];
            for (int y = 0; y < bitmap.Height; y++)
            {
                img[y] = new float[bitmap.Width];

                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    img[y][x] = (float)((color.R + color.G + color.B)) / 3.0F / 255.0F;
                }
            }

            return img;
        }

        public static float[] ConvoluteImage(float[] img, int width, int height)
        {
            float[][] kernel1 = Kernel.CreateKernel(5);
            kernel1[0][0] = 1; kernel1[0][1] = 4; kernel1[0][2] = 6; kernel1[0][3] = 4; kernel1[0][4] = 1;
            kernel1[1][0] = 4; kernel1[1][1] = 16; kernel1[1][2] = 24; kernel1[1][3] = 16; kernel1[1][4] = 4;
            kernel1[2][0] = 6; kernel1[2][1] = 24; kernel1[2][2] = 36; kernel1[2][3] = 24; kernel1[2][4] = 6;
            kernel1[3][0] = 4; kernel1[3][1] = 16; kernel1[3][2] = 24; kernel1[3][3] = 16; kernel1[3][4] = 4;
            kernel1[4][0] = 1; kernel1[4][1] = 4; kernel1[4][2] = 6; kernel1[4][3] = 4; kernel1[4][4] = 1;

            float[][] kernel2 = Kernel.CreateKernel(5);
            kernel2[0][0] = 0; kernel2[0][1] = 0; kernel2[0][2] = 0; kernel2[0][3] = 0; kernel2[0][4] = 0;
            kernel2[1][0] = 0; kernel2[1][1] = 0; kernel2[1][2] = 0; kernel2[1][3] = 0; kernel2[1][4] = 0;
            kernel2[2][0] = 0; kernel2[2][1] = 1; kernel2[2][2] = -1; kernel2[2][3] = 0; kernel2[2][4] = 0;
            kernel2[3][0] = 0; kernel2[3][1] = 0; kernel2[3][2] = 0; kernel2[3][3] = 0; kernel2[3][4] = 0;
            kernel2[4][0] = 0; kernel2[4][1] = 0; kernel2[4][2] = 0; kernel2[4][3] = 0; kernel2[4][4] = 0;

            float[][] kernel3 = Kernel.CreateKernel(5);
            kernel3[0][0] = 0; kernel3[0][1] = 0; kernel3[0][2] = 0; kernel3[0][3] = 0; kernel3[0][4] = 0;
            kernel3[1][0] = 0; kernel3[1][1] = 0; kernel3[1][2] = 1; kernel3[1][3] = 0; kernel3[1][4] = 0;
            kernel3[2][0] = 0; kernel3[2][1] = 1; kernel3[2][2] = -4; kernel3[2][3] = 1; kernel3[2][4] = 0;
            kernel3[3][0] = 0; kernel3[3][1] = 0; kernel3[3][2] = 1; kernel3[3][3] = 0; kernel3[3][4] = 0;
            kernel3[4][0] = 0; kernel3[4][1] = 0; kernel3[4][2] = 0; kernel3[4][3] = 0; kernel3[4][4] = 0;

            float[][] kernel4 = Kernel.CreateKernel(5);
            kernel4[0][0] = 1; kernel4[0][1] = 2; kernel4[0][2] = 3; kernel4[0][3] = 2; kernel4[0][4] = 1;
            kernel4[1][0] = 0; kernel4[1][1] = 0; kernel4[1][2] = 0; kernel4[1][3] = 0; kernel4[1][4] = 0;
            kernel4[2][0] = 0; kernel4[2][1] = 0; kernel4[2][2] = 0; kernel4[2][3] = 0; kernel4[2][4] = 0;
            kernel4[3][0] = 0; kernel4[3][1] = 0; kernel4[3][2] = 0; kernel4[3][3] = 0; kernel4[3][4] = 0;
            kernel4[4][0] = -1; kernel4[4][1] = -2; kernel4[4][2] = -3; kernel4[4][3] = -2; kernel4[4][4] = -1;

            float[][] kernel5 = Kernel.CreateKernel(5);
            kernel5[0][0] = 0; kernel5[0][1] = 0; kernel5[0][2] = 0; kernel5[0][3] = 0; kernel5[0][4] = 0;
            kernel5[1][0] = 0; kernel5[1][1] = 0; kernel5[1][2] = -1; kernel5[1][3] = 0; kernel5[1][4] = 0;
            kernel5[2][0] = 0; kernel5[2][1] = -1; kernel5[2][2] = 5; kernel5[2][3] = -1; kernel5[2][4] = 0;
            kernel5[3][0] = 0; kernel5[3][1] = 0; kernel5[3][2] = -1; kernel5[3][3] = 0; kernel5[3][4] = 0;
            kernel5[4][0] = 0; kernel5[4][1] = 0; kernel5[4][2] = 0; kernel5[4][3] = 0; kernel5[4][4] = 0;

            float[][][] kernels = new float[][][] { kernel1, kernel2, kernel3, kernel4, kernel5 };
            int stepSize = 2;
            float[] newImage = new float[25 * 25];
            int pixelCount = 0;

            for (int layer1 = 0; layer1 < kernels.Length; layer1++)
            {
                float[] img1 = Kernel.ConvolutionKernel(img, width, height, kernels[layer1], stepSize, 1, -0.0F);

                for (int layer2 = 0; layer2 < kernels.Length; layer2++)
                {
                    float[] img2 = Kernel.ConvolutionKernel(img1, (int)Math.Sqrt(img1.Length), (int)Math.Sqrt(img1.Length), kernels[layer2], stepSize, 1, -0.0F);

                    for (int layer3 = 0; layer3 < kernels.Length; layer3++)
                    {
                        float[] img3 = Kernel.ConvolutionKernel(img2, (int)Math.Sqrt(img2.Length), (int)Math.Sqrt(img2.Length), kernels[layer3], stepSize, 1, -0.0F);

                        for (int layer4 = 0; layer4 < kernels.Length; layer4++)
                        {
                            newImage[pixelCount] = Kernel.ConvolutionKernel(img3, (int)Math.Sqrt(img3.Length), (int)Math.Sqrt(img3.Length), kernels[layer4], stepSize, 1, -0.0F)[0];
                            pixelCount++;
                        }
                    }
                }
            }

            return newImage;
        }
    }
}
