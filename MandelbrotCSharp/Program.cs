using System;
using System.Diagnostics;
using System.Text.Json;
using System.IO;

namespace MandelSpeedTest
{
    class Program
    {
        static MandelSettings settings;
        static void MakeMandel()
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
            /*double xmin = -2.2;
            double xmax = 0.7;
            double ymin = -1.5;
            double ymax = 1.5;*/
            var f = new MandelField(settings.ImageWidth, settings.ImageHeight, settings.Iterations, settings.IterationOffset, settings.Limit, settings.Xmin, settings.Xmax, settings.Ymin, settings.Ymax);
            f.Run();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Cores to use: " + Environment.ProcessorCount);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            MakeMandel();
            sw.Stop();
            Console.WriteLine("Elapsed: " + sw.Elapsed);
            Console.WriteLine("Use 'ffmpeg -start_number " + settings.IterationOffset + " -r 30/1 -i brot_%d.png -c:v libx264 -crf 10 -vf fps=30 out.mp4' to process frames into video");
            //Console.WriteLine("Press Enter to exit");
            //Console.ReadLine();
            //ffmpeg -start_number 325 -r 30/1 -i brot_%02d.png -c:v libx264 -crf 10  -vf fps=30 out.mp4
        }
    }
}
