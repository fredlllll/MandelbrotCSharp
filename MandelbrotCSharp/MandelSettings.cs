using System;
using System.Text.Json.Serialization;

namespace MandelbrotCSharp
{
    public class MandelSettings
    {
        public int ImageWidth { get; set; } = 1024;
        public int ImageHeight { get; set; } = 1024;
        public int Iterations { get; set; } = 1000;
        public int IterationOffset { get; set; } = 300;
        public double Limit { get; set; } = 4;
        public double Xmin { get; set; } = -1.156312629975057006603;
        public double Xmax { get; set; } = -1.156312619124043845739;
        public double Ymin { get; set; } = 0.277977299547980381799;
        public double Ymax { get; set; } = 0.277977307686240252447;
        public int AccumulateImageCount { get; set; } = Environment.ProcessorCount;
        public string ImageOutput { get; set; } = "brot.png";
        public string ProcessingType { get; set; } = "brot";//"brot" or "field"

        [JsonIgnore]
        public static MandelSettings Instance { get; set; }
    }
}
