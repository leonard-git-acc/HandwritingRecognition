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
        public Bitmap DisplayImage { get; set; }
        public Size ImageSize { get => imageSize; set => OnImageResize(value); }
        public int LineWidth { get; set; }
        public bool AlreadyDrawn { get; set; }

        private Size imageSize;
        private Graphics ImageGraphics;

        public DrawPad()
        {
            this.imageSize = new Size(10, 10);
            this.LineWidth = 10;
            this.AlreadyDrawn = false;
            this.ResetImage();
            this.DoubleBuffered = true;
            this.Paint += OnPaint;
            this.MouseMove += OnMouseDraw;
            this.MouseUp += OnMouseUp;
            this.MouseDown += OnMouseDown;
        }

        public void ResetImage()
        {
            this.Image = new Bitmap(imageSize.Width, imageSize.Height);

            this.AlreadyDrawn = false;
            this.DisplayImage = this.Image;
            this.Refresh();
        }

        public void RescaleImage(int newWidth, int newHeight)
        {
            this.Image = new Bitmap(this.Image, newWidth, newHeight);
        }

        public void CenterImage()
        {
            if (AlreadyDrawn)
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
                        if (color.A > 0)
                        {
                            if (x < xleft)
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

                int newHeight = 20;
                int newWidth = Math.Min(newHeight * img.Width / img.Height, 20);

                img = new Bitmap(img, newWidth, newHeight);
                Image = new Bitmap(Image.Width, Image.Height);


                //calculate center of mass
                float meanX = 0;
                float meanY = 0;
                float mass = 0;

                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        float value = img.GetPixel(x, y).A / 255.0F;
                        meanX += x * value;
                        meanY += y * value;
                        mass += value;
                    }
                }
                meanX = meanX / mass;
                meanY = meanY / mass;

                //img.SetPixel((int)meanX, (int)meanY, Color.Green);

                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {


                        Image.SetPixel(maxMin(x + Convert.ToInt32(Image.Width / 2 - meanX), Image.Width - 1, 0),
                                       maxMin(y + Convert.ToInt32(Image.Height / 2 - meanY), Image.Height - 1, 0),
                                       img.GetPixel(x, y));
                    }
                }

            }

            int maxMin(int val, int max, int min)
            {
                return Math.Max(Math.Min(val, max), min);
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
                    img[pixelCount] = (float)color.A / 255.0F;
                    pixelCount++;
                }
            }

            return img;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(DisplayImage, 0, 0, Size.Width, Size.Height);
        }

        private void OnImageResize(Size _size)
        {
            imageSize = _size;
            this.ResetImage();
        }

        Point? prevPoint = null;
        private void OnMouseDraw(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Image = DisplayImage;
                this.ImageGraphics = Graphics.FromImage(Image);
                Color black = Color.FromArgb(255, 255, 255, 255);

                Point imgPoint = ConvertPoint(e.Location);

                if (prevPoint != null)
                    ImageGraphics.DrawLine(new Pen(new SolidBrush(black), LineWidth * 2), (Point)prevPoint, imgPoint);
                DrawCircle(ImageGraphics, imgPoint.X, imgPoint.Y, LineWidth, black);

                DisplayImage = Image;
                prevPoint = imgPoint;
                AlreadyDrawn = true;
                this.Refresh();
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            prevPoint = null;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            //    this.ImageGraphics = Graphics.FromImage(Image);
            //    Color black = Color.FromArgb(255, 255, 255, 255);
            //    Point imgPoint = ConvertPoint(e.Location);
            //    DrawCircle(ImageGraphics, imgPoint.X, imgPoint.Y, LineWidth, black);
            //    prevPoint = imgPoint;
            //    this.Refresh();
            //}
        }

        private Point ConvertPoint(Point pt)
        {
            int x = Math.Max(Math.Min(Convert.ToInt32((float)pt.X * imageSize.Width / Size.Width), Image.Width - 1), 0);
            int y = Math.Max(Math.Min(Convert.ToInt32((float)pt.Y * imageSize.Height / Size.Height), Image.Height - 1), 0);

            return new Point(x, y);
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

        private void DrawCircle(Graphics imgGraphics, int x, int y, int radius, Color color)
        {
            imgGraphics.FillEllipse(new SolidBrush(color), new Rectangle(x - radius, y - radius, radius * 2, radius * 2));
        }
    }
}
