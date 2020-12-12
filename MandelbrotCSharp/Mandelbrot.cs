using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotCSharp
{
    class Mandelbrot
    {
        struct Pixel
        {
            public Complex c;
            public int i;

            public Pixel(Complex c)
            {
                this.c = c;
                i = -1;
            }
        }

        Pixel[] field = null;
        int imageWidth;
        int imageHeight;

        double limit, limitSquared;
        int iterations;
        double minX, maxX, minY, maxY;

        public Mandelbrot(int imageWidth = 1024, int imageHeight = 1024, int iterations = 50, double limit = 4, double minX = -2.3, double maxX = 0.7, double minY = -1.5, double maxY = 1.5)
        {
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            this.iterations = iterations;
            this.limit = limit;
            this.limitSquared = limit * limit;
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }

        private void PrepareField()
        {
            double imageWidthF = imageWidth;
            double imageHeightF = imageHeight;
            double xDiff = maxX - minX;
            double yDiff = maxY - minY;

            field = new Pixel[imageWidth * imageHeight];

            int perTile = imageHeight / Environment.ProcessorCount;
            Task[] tasks = new Task[Environment.ProcessorCount];
            for (int tileIndex = 0; tileIndex < tasks.Length; tileIndex++)
            {
                int yFrom = tileIndex * perTile;
                int yTo = yFrom + perTile;
                if (tileIndex == tasks.Length - 1)
                {
                    yTo = imageHeight;
                }
                var t = new Task(() =>
                {
                    for (int y = yFrom; y < yTo; y++)
                    {
                        int indexOffset = y * imageWidth;
                        double yOffset = y / imageHeightF * yDiff;
                        for (int x = 0; x < imageWidth; x++)
                        {
                            field[indexOffset + x] = new Pixel(new Complex(minX + (x / imageWidthF * xDiff), minY + yOffset));
                        }
                    }
                });
                tasks[tileIndex] = t;
                t.Start();
            }
            foreach (var tt in tasks)
            {
                tt.Wait();
            }
        }

        private void Iterate()
        {
            int perTile = field.Length / Environment.ProcessorCount;
            Task[] tasks = new Task[Environment.ProcessorCount];
            for (int tileIndex = 0; tileIndex < tasks.Length; tileIndex++)
            {
                int from = tileIndex * perTile;
                int to = from + perTile;
                if (tileIndex == tasks.Length - 1)
                {
                    to = field.Length;
                }
                var t = new Task(() =>
                {
                    Complex z;
                    for (int pixelIndex = from; pixelIndex < to; pixelIndex++)
                    {
                        Pixel p = field[pixelIndex];
                        Complex c = p.c;
                        z = 0;
                        for (int j = 0; j < iterations; j++)
                        {
                            z = z * z + c;
                            if (z.Real * z.Imaginary > limitSquared)
                            {
                                p.i = j;
                                break;
                            }
                        }
                        if (p.i == -1)
                        {
                            p.i = iterations;
                        }
                        field[pixelIndex] = p;
                    }
                });
                tasks[tileIndex] = t;
                t.Start();
            }
            foreach (var t in tasks)
            {
                t.Wait();
            }
        }

        private byte[] GetValues()
        {
            byte[] vals = new byte[field.Length];

            int perTile = field.Length / Environment.ProcessorCount;
            Task[] tasks = new Task[Environment.ProcessorCount];
            for (int j = 0; j < tasks.Length; j++)
            {
                int from = perTile * j;
                int to = perTile * (j + 1);
                if (j == tasks.Length - 1)
                {
                    to = field.Length;
                }
                var t = new Task(() =>
                {
                    for (int i = from; i < to; i++)
                    {
                        Pixel p = field[i];
                        double perc = p.i;
                        perc /= iterations;
                        vals[i] = (byte)(255 * perc);
                    }
                });
                tasks[j] = t;
                t.Start();
            }
            foreach (var t in tasks)
            {
                t.Wait();
            }

            return vals;
        }

        private void SaveImage()
        {
            var imgValues = GetValues();
            ImageSaver.DirectSave(imgValues, imageWidth, imageHeight, MandelSettings.Instance.ImageOutput);
        }

        public void Run()
        {
            PrepareField();
            Iterate();
            SaveImage();
        }
    }
}
