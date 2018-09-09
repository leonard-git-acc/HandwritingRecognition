using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.IO.Compression;

namespace HandwritingRecognition
{
    static class ParseDatabase
    {
        public static byte[][] ParseImages(string filePath, int amount)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            BinaryReader binaryReader = new BinaryReader(fileStream);

            int magicNum = binaryReader.ReadInt32();
            int imageCount = binaryReader.ReadInt32();
            int rowCount = binaryReader.ReadInt32();
            int columnsCount = binaryReader.ReadInt32();

            byte[][] images = new byte[amount][];

            for (int iter = 0; iter < images.Length; iter++)
            {
                byte[] image = new byte[28 * 28];

                for (int i = 0; i < image.Length; i++)
                {
                    image[i] = binaryReader.ReadByte();
                }

                images[iter] = image;
                Console.WriteLine(string.Format("Image {0} extracted from {1}", iter, filePath));
            }

            return images;
        }

        public static byte[] ParseLabels(string filePath, int amount)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            BinaryReader binaryReader = new BinaryReader(fileStream);

            int magicNum = binaryReader.ReadInt32();
            int labelCount = binaryReader.ReadInt32();

            byte[] labels = new byte[amount];

            for (int i = 0; i < labels.Length; i++)
            {
                byte label = binaryReader.ReadByte();
                labels[i] = label;
                Console.WriteLine(string.Format("Number {0} extracted from {1}", label, filePath));
            }

            return labels;
        }

        public static Bitmap ByteToBitmap(byte[] image, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            int pixelCount = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bmp.SetPixel(x, y, Color.FromArgb(255, image[pixelCount], image[pixelCount], image[pixelCount]));
                    pixelCount++;
                }
            }

            return bmp;
        }

        public static float[] ByteToFloat(byte[] image)
        {
            float[] img = new float[image.Length];

            for (int i = 0; i < image.Length; i++)
            {
                float prevVal = image[i];
                float newVal = prevVal * 1.0F / 255.0F;

                img[i] = (float)Math.Round(newVal, 2);
            }

            return img;
        }

        public static byte[] FloatToByte(float[] image)
        {
            byte[] img = new byte[image.Length];

            for (int i = 0; i < image.Length; i++)
            {
                float prevVal = image[i];
                float newVal = prevVal * 255.0F;

                img[i] = (byte)Math.Round(newVal, 2);
            }

            return img;
        }


    }


}
