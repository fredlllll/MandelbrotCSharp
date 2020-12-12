using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;

namespace MandelbrotCSharp
{
    class MandelField
    {
        struct Pixel
        {
            public Complex c, z;
            public int i;
            public bool done;

            public Pixel(Complex c, double limit)
            {
                this.c = c;
                z = c;
                i = 0;
                done = z.Magnitude > limit;
            }
        }

        Pixel[] field = null;
        int currentIteration = 0;
        bool running = false;
        readonly Stopwatch swIteration = new Stopwatch(), swConversion = new Stopwatch();

        //changeable stuff
        int imageWidth;
        public int ImageWidth
        {
            get { return imageWidth; }
            set { if (!running) { imageWidth = value; } }
        }
        int imageHeight;
        public int ImageHeight
        {
            get { return imageHeight; }
            set { if (!running) { imageHeight = value; } }
        }
        double limit;
        public double Limit
        {
            get { return limit; }
            set { if (!running) { limit = value; } }
        }
        int iterations;
        public int Iterations
        {
            get { return iterations; }
            set { if (!running) { iterations = value; } }
        }
        int iterationOffset;
        public int IterationOffset
        {
            get { return iterationOffset; }
            set { if (!running) { iterationOffset = value; } }
        }
        double minX, maxX, minY, maxY;
        public double MinX
        {
            get { return minX; }
            set { if (!running) { minX = value; } }
        }
        public double MaxX
        {
            get { return maxX; }
            set { if (!running) { maxX = value; } }
        }
        public double MinY
        {
            get { return minY; }
            set { if (!running) { minY = value; } }
        }
        public double MaxY
        {
            get { return maxY; }
            set { if (!running) { maxY = value; } }
        }

        public MandelField(int imageWidth = 1024, int imageHeight = 1024, int iterations = 50, int iterationOffset = 0, double limit = 4, double minX = -2.3, double maxX = 0.7, double minY = -1.5, double maxY = 1.5)
        {
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            this.iterations = iterations;
            this.iterationOffset = iterationOffset;
            this.limit = limit;
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

            int pixels = imageWidth * imageHeight;
            if (field == null || field.Length != pixels)
            {
                field = new Pixel[imageWidth * imageHeight];
            }
            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    Complex c = new Complex(minX + (x / imageWidthF * xDiff), minY + (y / imageHeightF * yDiff));
                    field[y * imageWidth + x] = new Pixel(c, limit);
                }
            }
        }

        private void Iterate()
        {
            Console.WriteLine("Iteration " + currentIteration);
            swIteration.Start();
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
                        if (!p.done)
                        {
                            p.z = p.z * p.z + p.c;
                            if (p.z.Magnitude > limit)
                            {
                                p.done = true;
                            }
                            p.i++;
                            field[i] = p;
                        }
                    }
                });
                tasks[j] = t;
                t.Start();
            }
            foreach (var t in tasks)
            {
                t.Wait();
            }
            swIteration.Stop();

            currentIteration++;
        }

        readonly ArrayPool<byte> arrayPoolByte = new ArrayPool<byte>();
        private byte[] GetValues()
        {
            swConversion.Start();

            int pixels = imageWidth * imageHeight;
            byte[] vals = arrayPoolByte.Get(pixels);

            double actualIterations = iterations - iterationOffset;
            byte notDoneVal = (byte)(255 * ((currentIteration - iterationOffset) / actualIterations));

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
                        if (p.done)
                        {
                            double perc = p.i - iterationOffset;
                            perc /= actualIterations;
                            vals[i] = (byte)(255 * perc);
                        }
                        else
                        {
                            vals[i] = notDoneVal;
                        }
                    }
                });
                tasks[j] = t;
                t.Start();
            }
            foreach (var t in tasks)
            {
                t.Wait();
            }

            swConversion.Stop();

            return vals;
        }

        ImageSaver imageSaver = null;
        private void SaveImage()
        {
            if (imageSaver == null)
            {
                imageSaver = new ImageSaver(arrayPoolByte);
            }
            var imgValues = GetValues();
            imageSaver.Enqueue(imgValues, imageWidth, imageHeight, "brot_" + currentIteration + ".jpg");
        }

        public void Run()
        {
            running = true;
            currentIteration = 0;
            PrepareField();
            if (iterationOffset == 0)
            {
                SaveImage();
            }
            for (int i = 0; i < iterationOffset; i++)
            {
                Iterate();
            }
            for (int i = 0; i < iterations - iterationOffset; i++)
            {
                Iterate();
                SaveImage();
            }
            imageSaver.Wait();
            running = false;
            Console.WriteLine("Total Iteration Time: " + swIteration.Elapsed.TotalMilliseconds + "ms");
            Console.WriteLine("Total Conversion Time: " + swConversion.Elapsed.TotalMilliseconds + "ms");
        }
    }
}
