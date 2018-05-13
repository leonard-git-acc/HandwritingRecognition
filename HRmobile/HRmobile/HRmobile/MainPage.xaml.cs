using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using TouchTracking;
using Simulation;


namespace HRmobile
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : ContentPage
	{
		public MainPage (Stream[] assets)
		{
			InitializeComponent();
            neuralNetwork = new Brain(assets[0]);
        }

        SKCanvas canvas;
        SKSurface surface;

        Dictionary<long, SKPath> inProgressPaths = new Dictionary<long, SKPath>();
        List<SKPath> completedPaths = new List<SKPath>();

        SKPaint drawPaint = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };
        SKPaint backPaint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = Color.FromRgba(0, 0, 0, 255).ToSKColor(),
            
        };

        float drawSize;
        bool saveImage = false;
        string path;
        SKBitmap bitmap;
        SKImage image;

        Brain neuralNetwork;

        private void Canvas_Paint(object sender, SKPaintSurfaceEventArgs e)
        {
            surface = e.Surface;
            canvas = surface.Canvas;

            float width = e.Info.Width;
            float height = e.Info.Height;
            drawSize = Math.Min(width, height);
            drawPaint.StrokeWidth = drawSize / 10F;

            canvas.Clear(SKColors.Gray);
            canvas.DrawRect(new SKRect(0, 0, drawSize, drawSize), backPaint);

            foreach (SKPath path in completedPaths)
            {
                canvas.DrawPath(path, drawPaint);
            }

            foreach (SKPath path in inProgressPaths.Values)
            {
                canvas.DrawPath(path, drawPaint);
            }

            if(saveImage && completedPaths.Count != 0)
            {
                image = surface.Snapshot().Subset(new SKRectI(0, 0, (int)drawSize, (int)drawSize));
                bitmap = SKBitmap.Decode(image.Encode());
                bitmap = TransformImage.CenterImage(bitmap);
                RecognizeImage(bitmap);
                saveImage = false;

                //FileStream fstream = new FileStream("/storage/sdcard/Pictures/img.png", FileMode.Create);
                //SKImage.FromBitmap(bitmap).Encode().AsStream().CopyTo(fstream);
                //fstream.Close();
            }
        }

        private void Grid_Touched(object sender, TouchActionEventArgs e)
        {
            double drawSize = Math.Min(canvasView.Width, canvasView.Height);
            double strokeWidth = drawPaint.StrokeWidth / 6;
            Point location = new Point(MinMax(e.Location.X, strokeWidth, drawSize - strokeWidth), MinMax(e.Location.Y, strokeWidth, drawSize - strokeWidth));

            float MinMax(double value, double min, double max)
            {
                return (float)Math.Min(Math.Max(value, min), max);
            }

            switch (e.Type)
            {
                case TouchActionType.Pressed:
                    if (!inProgressPaths.ContainsKey(e.Id))
                    {
                        SKPath path = new SKPath();
                        path.MoveTo(ConvertToPixel(location));
                        inProgressPaths.Add(e.Id, path);
                        canvasView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Moved:
                    if (inProgressPaths.ContainsKey(e.Id))
                    {
                        SKPath path = inProgressPaths[e.Id];
                        path.LineTo(ConvertToPixel(location));
                        canvasView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                    if (inProgressPaths.ContainsKey(e.Id))
                    {
                        completedPaths.Add(inProgressPaths[e.Id]);
                        inProgressPaths.Remove(e.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Cancelled:
                    if (inProgressPaths.ContainsKey(e.Id))
                    {
                        inProgressPaths.Remove(e.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;
            }

            SKPoint ConvertToPixel(Point pt)
            {
                return new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width),
                                   (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));
            }
        }

        private void OK_button_Clicked(object sender, EventArgs e)
        {
            saveImage = true;
            canvasView.InvalidateSurface();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

        private void Reset_button_Clicked(object sender, EventArgs e)
        {
            inProgressPaths = new Dictionary<long, SKPath>();
            completedPaths = new List<SKPath>();
            canvasView.InvalidateSurface();
            Output_label.Text = "_";
        }

        private void RecognizeImage(SKBitmap bitmap)
        {
            float[] bmp = TransformImage.ImageToFloat(bitmap);
            float[] output = neuralNetwork.Think(bmp);
            int result = Training.OutputNumber(output);
            Output_label.Text = result.ToString();
        }

    }
}