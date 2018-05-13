using System;
using System.Collections.Generic;
using System.Text;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace Simulation
{
    static class TransformImage
    {
        public static SKBitmap CenterImage(SKBitmap bitmap)
        {
            SKBitmap bmp;
            SKBitmap Bitmap = bitmap;

            int xleft = Bitmap.Width;
            int xright = 0;
            int ytop = Bitmap.Height;
            int ybottom = 0;

            //xleft
            for (int y = 0; y < Bitmap.Height; y++)
            {
                for (int x = 0; x < Bitmap.Width; x++)
                {
                    SKColor color = Bitmap.GetPixel(x, y);
                    if (color.Red > 0)
                    {
                        if (x < xleft)
                        {
                            xleft = x;
                        }
                    }
                }
            }

            //xright
            for (int y = 0; y < Bitmap.Height; y++)
            {
                for (int x = Bitmap.Width - 1; x >= 0; x--)
                {
                    SKColor color = Bitmap.GetPixel(x, y);
                    if (color.Red > 0)
                    {
                        if (x > xright)
                        {
                            xright = x;
                        }
                    }
                }
            }

            //ytop
            for (int x = 0; x < Bitmap.Width; x++)
            {
                for (int y = 0; y < Bitmap.Height; y++)
                {
                    SKColor color = Bitmap.GetPixel(x, y);
                    if (color.Red > 0)
                    {
                        if (y < ytop)
                        {
                            ytop = y;
                        }
                    }
                }
            }

            //yright
            for (int x = 0; x < Bitmap.Width; x++)
            {
                for (int y = Bitmap.Height - 1; y >= 0; y--)
                {
                    SKColor color = Bitmap.GetPixel(x, y);
                    if (color.Red > 0)
                    {
                        if (y > ybottom)
                        {
                            ybottom = y;
                        }
                    }
                }
            }

            bmp = new SKBitmap(xright - xleft + 1, ybottom - ytop + 1);

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    bmp.SetPixel(x, y, Bitmap.GetPixel(x + xleft, y + ytop));
                }
            }

            int newHeight = 20;
            int newWidth = Math.Min(newHeight * bmp.Width / bmp.Height, 20);

            bmp = bmp.Resize(new SKImageInfo(newWidth, newHeight), SKBitmapResizeMethod.Box);

            var pixels = new SKColor[28 * 28];
            for(int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = SKColors.Black;
            }
            Bitmap = new SKBitmap(28, 28);
            Bitmap.Pixels = pixels;

            //calculate center of mass
            float meanX = 0;
            float meanY = 0;
            float mass = 0;

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    float value = bmp.GetPixel(x, y).Red / 255.0F;
                    meanX += x * value;
                    meanY += y * value;
                    mass += value;
                }
            }
            meanX = meanX / mass;
            meanY = meanY / mass;

            //bmp.SetPixel((int)meanX, (int)meanY, SKColors.Green);

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Bitmap.SetPixel(maxMin(x + Convert.ToInt32(Bitmap.Width / 2 - meanX), Bitmap.Width - 1, 0),
                                   maxMin(y + Convert.ToInt32(Bitmap.Height / 2 - meanY), Bitmap.Height - 1, 0),
                                   bmp.GetPixel(x, y));
                }
            }

            return Bitmap;

            int maxMin(int val, int max, int min)
            {
                return Math.Max(Math.Min(val, max), min);
            }
        }

        public static float[] ImageToFloat(SKBitmap bitmap)
        {
            float[] bmp = new float[bitmap.Height * bitmap.Width];
            int pixelCount = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    SKColor color = bitmap.GetPixel(x, y);
                    bmp[pixelCount] = (color.Red + color.Green + color.Blue) / 4F / 255.0F;
                    pixelCount++;
                }
            }

            return bmp;
        }
    }
}
