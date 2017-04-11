using JAVS.ComputerVision.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Face;

namespace JAVS.ComputerVision.Core.Detectors.FaceDetection
{
    class JAVSFacialRecognizer : IDetect
    {
        private EigenFaceRecognizer _faceRecognizer;


        public JAVSFacialRecognizer()
        {
            _faceRecognizer = new EigenFaceRecognizer();
            _faceRecognizer.Load(Environment.SpecialFolder.UserProfile + "\\Documents\\test-recognizer");
        }
        public Dictionary<string, ParameterProfile> AdjustableParameters { get; set; }

        public string DisplayName
        {
            get
            {
                return "Facial Recognition";
            }
        }

        public int ProcessCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<IImage> ProcessFrame(IImage original)
        {

            return null;
        }
    }
}
