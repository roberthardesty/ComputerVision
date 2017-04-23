using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAVS.ComputerVision.Core.Detectors
{
    public class SourceStatistics
    {
        public double FPS { get; set; }
        public double FrameHeight { get; set; }
        public double FrameWidth { get; set; }
        public double FrameArea { get { return FrameHeight * FrameWidth; } }
        public double TotalFrames { get; set; }
        public double Zoom { get; set; }
        public double Seconds { get { return TotalFrames / FPS; } }
        public Dictionary<Emgu.CV.CvEnum.CapProp, double> AllStats
        {
            get
            {
                var stats = new Dictionary<Emgu.CV.CvEnum.CapProp, double>();
                stats.Add(Emgu.CV.CvEnum.CapProp.Fps, FPS);
                stats.Add(Emgu.CV.CvEnum.CapProp.FrameHeight, FrameHeight);
                stats.Add(Emgu.CV.CvEnum.CapProp.FrameWidth, FrameWidth);
                stats.Add(Emgu.CV.CvEnum.CapProp.FrameCount, TotalFrames);
                stats.Add(Emgu.CV.CvEnum.CapProp.Zoom, Zoom);
                return stats;
            }
        }
        public SourceStatistics(VideoCapture capture)
        {
            FPS = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
            FrameHeight = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight);
            FrameWidth = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth);
            TotalFrames = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
            Zoom = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Zoom);
        }
    }
}
