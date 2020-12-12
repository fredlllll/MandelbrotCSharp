using System;
using System.Diagnostics;
using System.Text.Json;
using System.IO;

namespace MandelbrotCSharp
{
    class Program
    {
        static MandelSettings settings;
        static void LoadSettings()
        {
            settings = new MandelSettings();
            if (File.Exists("settings.json"))
            {
                settings = JsonSerializer.Deserialize<MandelSettings>(File.ReadAllText("settings.json"));
            }
            else
            {
                File.WriteAllText("settings.json", JsonSerializer.Serialize(settings, new JsonSerializerOptions() { WriteIndented = true }));
            }
            MandelSettings.Instance = settings;
        }

        static void MakeMandelField()
        {
            var f = new MandelField(settings.ImageWidth, settings.ImageHeight, settings.Iterations, settings.IterationOffset, settings.Limit, settings.Xmin, settings.Xmax, settings.Ymin, settings.Ymax);
            f.Run();
        }

        static void MakeMandelbrot()
        {
            var b = new Mandelbrot(settings.ImageWidth, settings.ImageHeight, settings.Iterations, settings.Limit, settings.Xmin, settings.Xmax, settings.Ymin, settings.Ymax);
            b.Run();
        }

        static void Main(string[] args)
        {
            LoadSettings();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (settings.ProcessingType.Equals("brot"))
            {
                MakeMandelbrot();
            }
            else if (settings.ProcessingType.Equals("field"))
            {
                MakeMandelField();
                Console.WriteLine("Use 'ffmpeg -start_number " + settings.IterationOffset + " -r 30/1 -i brot_%d.png -c:v libx264 -crf 10 -vf fps=30 out.mp4' to process frames into video");
            }
            else { throw new Exception("unknown processing type: " + settings.ProcessingType); }
            sw.Stop();
            Console.WriteLine("Total Elapsed: " + sw.Elapsed.TotalMilliseconds + "ms");
        }
    }
}
