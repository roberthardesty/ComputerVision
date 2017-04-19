using JAVS.ComputerVision.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace JAVS.ComputerVision.Core.Detectors.FaceDetection
{
    public class JAVSFaceTrainer : IDetect
    {
        public Dictionary<string, ParameterProfile> AdjustableParameters { get; set; }

        public string DisplayName { get { return "Learn New Face"; } }
        public int ProcessCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<IImage> ProcessFrame(IImage original)
        {
            throw new NotImplementedException();
        }
    }
}