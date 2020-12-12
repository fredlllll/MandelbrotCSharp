using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotCSharp
{
    class ImageSaver
    {
        readonly List<Task> imageSavingTasks = new List<Task>();
        readonly List<Task> startedImageSavingTasks = new List<Task>();
        readonly ArrayPool<byte> arrayPoolByte;

        public ImageSaver(ArrayPool<byte> arrayPoolByte)
        {
            this.arrayPoolByte = arrayPoolByte;
        }

        public static void DirectSave(byte[] pixels, int width, int height, string output)
        {
            using (var img = Image.LoadPixelData<L8>(pixels, width, height))
            {
                img.SaveAsPng(output);
            }
        }

        public void Enqueue(byte[] pixels, int width, int height, string output)
        {
            Task t = new Task(() =>
            {
                using (var img = Image.LoadPixelData<L8>(pixels, width, height))
                {
                    img.SaveAsJpeg(output);
                }
                arrayPoolByte.Put(pixels);

                lock (startedImageSavingTasks)
                {
                    startedImageSavingTasks.RemoveAll((t) => (t.Id == Task.CurrentId));
                }
            });
            imageSavingTasks.Add(t);
            if (imageSavingTasks.Count >= MandelSettings.Instance.AccumulateImageCount)
            {
                foreach (var tt in imageSavingTasks)
                {
                    tt.Start();
                    lock (startedImageSavingTasks)
                    {
                        startedImageSavingTasks.Add(tt);
                    }
                }
                imageSavingTasks.Clear();
            }
        }

        public void Wait()
        {
            if (imageSavingTasks.Count > 0)
            {
                foreach (var tt in imageSavingTasks)
                {
                    tt.Start();
                    lock (startedImageSavingTasks)
                    {
                        startedImageSavingTasks.Add(tt);
                    }
                }
                imageSavingTasks.Clear();
            }
            Task[] tasksToWaitFor = Array.Empty<Task>();
            lock (startedImageSavingTasks)
            {
                if (startedImageSavingTasks.Count > 0)
                {
                    tasksToWaitFor = startedImageSavingTasks.ToArray();
                }
            }
            foreach (var tt in tasksToWaitFor)
            {
                tt.Wait();
            }
        }
    }
}
