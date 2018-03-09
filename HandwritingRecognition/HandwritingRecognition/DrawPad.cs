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

        private Size imageSize;

        public DrawPad()
        {
            this.imageSize = new Size(28, 28);
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
                Color black = Color.FromArgb(255, 0, 0, 0);
                int imgX = Math.Max(Math.Min(Convert.ToInt32((float)e.Location.X * imageSize.Width / Size.Width), Image.Width - 1), 0);
                int imgY = Math.Max(Math.Min(Convert.ToInt32((float)e.Location.Y * imageSize.Height / Size.Height), Image.Height - 1), 0);
                
                this.Image.SetPixel(imgX, imgY, black);
                this.Image.SetPixel(Math.Min(imgX + 1, Image.Width - 1), imgY, black);
                this.Image.SetPixel(Math.Max(imgX - 1, 0), imgY, black);
                this.Image.SetPixel(imgX, Math.Min(imgY + 1, Image.Height - 1), black);
                this.Image.SetPixel(imgX, Math.Max(imgY - 1, 0), black);

                this.Refresh();
            }
        }

        public float[] ImageToFloat()
        {
            float[] img = new float[Image.Height * Image.Height];
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
    }
}
