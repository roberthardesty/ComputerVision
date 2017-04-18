using JAVS.ComputerVision.Core.Detectors;
using JAVS.ComputerVision.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace JAVS.ComputerVison.Core.Interfaces
{
    interface ISourceManager
    {
        Dictionary<string, ParameterProfile> CurrentParameters { get; set; }
        List<BitmapSource> ProcessedFrames { get; set; }
        event EventHandler NewFrame;
        bool IsReady();
        void SetDetector(IDetect detector);
        void Start(int captureSource = 0, string path = null);
        void Dispose();        
    }
}
