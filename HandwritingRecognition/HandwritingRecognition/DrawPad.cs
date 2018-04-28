using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace HandwritingRecognition
{
    class DrawPad : Control
    {
        public Bitmap Image { get; set; }
        public Size ImageSize { get => imageSize; set => OnImageResize(value); }
        public int LineWidth;

        private Size imageSize;

        public DrawPad()
        {
            this.imageSize = new Size(10, 10);
            this.LineWidth = 10;
            this.ResetImage();
            this.DoubleBuffered = true;
            this.Paint += OnPaint;
            this.MouseMove += OnMouseDraw;
        }

        public void ResetImage()
        {
            this.Image = new Bitmap(imageSize.Width, imageSize.Height);

            for (int y = 0; y < imageSize.Height; y++)
            {
                for (int x = 0; x < imageSize.Width; x++)
                {
                    Image.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                }
            }

            this.Refresh();
        }

        public void RescaleImage(int newWidth, int newHeight)
        {
            this.Image = new Bitmap(this.Image, newWidth, newHeight);
        }

        public void CenterImage()
        {
            Bitmap img;
            int xleft = Image.Width;
            int xright = 0;
            int ytop = Image.Height;
            int ybottom = 0;

            //xleft
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    Color color = Image.GetPixel(x, y);
                    if(color.A > 0)
                    {
                        if(x < xleft)
                        {
                            xleft = x;
                        }
                    }
                }
            }

            //xright
            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = Image.Width - 1; x >= 0; x--)
                {
                    Color color = Image.GetPixel(x, y);
                    if (color.A > 0)
                    {
                        if (x > xright)
                        {
                            xright = x;
                        }
                    }
                }
            }

            //ytop
            for (int x = 0; x < Image.Width; x++)
            {
                for (int y = 0; y < Image.Height; y++)
                {
                    Color color = Image.GetPixel(x, y);
                    if (color.A > 0)
                    {
                        if (y < ytop)
                        {
                            ytop = y;
                        }
                    }
                }
            }

            //yright
            for (int x = 0; x < Image.Width; x++)
            {
                for (int y = Image.Height - 1; y >= 0; y--)
                {
                    Color color = Image.GetPixel(x, y);
                    if (color.A > 0)
                    {
                        if (y > ybottom)
                        {
                            ybottom = y;
                        }
                    }
                }
            }

            img = new Bitmap(xright - xleft + 1, ybottom - ytop + 1);

            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    img.SetPixel(x, y, Image.GetPixel(x + xleft, y + ytop));
                }
            }

            int newHeight = 20;//Convert.ToInt32(Image.Height - Image.Height / 3.5);
            int newWidth = Math.Min(newHeight * img.Width / img.Height, 20);

            img = new Bitmap(img, newWidth, newHeight);
            Image = new Bitmap(Image.Width, Image.Height);

            //calculate center of mass
            float meanX = 0;
            float meanY = 0;
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    meanX += x * (img.GetPixel(x, y).A / 255.0F + 0.75F);
                    meanY += y * (img.GetPixel(x, y).A / 255.0F + 0.75F);
                }
            }
            meanX = meanX / (img.Height * img.Width);
            meanY = meanY / (img.Height * img.Width);

            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    Image.SetPixel(Math.Min(x + Convert.ToInt32(Image.Width / 2 - meanX), Image.Width - 1),
                                   Math.Min(y + Convert.ToInt32(Image.Height / 2 - meanY), Image.Height - 1),
                                   img.GetPixel(x, y));
                }
            }
        }

        public float[] ImageToFloat()
        {
            float[] img = new float[Image.Height * Image.Width];
            int pixelCount = 0;

            for (int y = 0; y < Image.Height; y++)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    Color color = Image.GetPixel(x, y);
                    img[pixelCount] = (float) color.A / 255.0F;
                    pixelCount++;
                }
            }

            return img;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(Image, 0, 0, Size.Width, Size.Height);
        }

        private void OnImageResize(Size _size)
        {
            imageSize = _size;
            this.ResetImage();
        }

        private void OnMouseDraw(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                Color black = Color.FromArgb(255, 255, 255, 255);
                int imgX = Math.Max(Math.Min(Convert.ToInt32((float)e.Location.X * imageSize.Width / Size.Width), Image.Width - 1), 0);
                int imgY = Math.Max(Math.Min(Convert.ToInt32((float)e.Location.Y * imageSize.Height / Size.Height), Image.Height - 1), 0);

                //CircleBresenham(Image, imgX, imgY, LineWidth, black);
                DrawCircle(Image, imgX, imgY, LineWidth, black);

                this.Refresh();
            }
        }

        private void CircleBresenham(Bitmap img, int xc, int yc, int radius, Color color)
        {
            for (int r = 1; r < radius; r++)
            {
                int x = 0, y = r;
                int d = 1 - r;
                DrawPoints(img, xc, yc, x, y, color);
                while (x < y)
                {
                    if (d < 0)
                        d += 2 * x + 2;
                    else
                    {
                        d += 2 * (x - y) + 5;
                        y--;
                    }
                    x++;
                    DrawPoints(img, xc, yc, x, y, color);
                }
            }
        }

        private void DrawPoints(Bitmap img, int xc, int yc, int a, int b, Color color)
        {
            img.SetPixel(borderX(xc + a), borderY(yc + b), color);
            img.SetPixel(borderX(xc - a), borderY(yc + b), color);
            img.SetPixel(borderX(xc - a), borderY(yc - b), color);
            img.SetPixel(borderX(xc + a), borderY(yc - b), color);
            img.SetPixel(borderX(xc + b), borderY(yc + a), color);
            img.SetPixel(borderX(xc - b), borderY(yc + a), color);
            img.SetPixel(borderX(xc - b), borderY(yc - a), color);
            img.SetPixel(borderX(xc + b), borderY(yc - a), color);

            int borderX(int val)
            {
                return Math.Min(Math.Max(val, 0), img.Width - 1);
            }

            int borderY(int val)
            {
                return Math.Min(Math.Max(val, 0), img.Height - 1);
            }
        }

        private void DrawCircle(Bitmap img, int x, int y, int radius, Color color)
        {
            Graphics e = Graphics.FromImage(img);
            e.FillEllipse(new SolidBrush(color), new Rectangle(x - radius, y - radius, radius * 2, radius * 2));
        }
    }
}
