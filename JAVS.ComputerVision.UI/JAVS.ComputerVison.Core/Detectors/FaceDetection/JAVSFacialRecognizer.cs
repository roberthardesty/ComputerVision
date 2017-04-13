using JAVS.ComputerVision.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using JAVS.ComputerVision.Core.Helper;

namespace JAVS.ComputerVision.Core.Detectors.FaceDetection
{
    public class JAVSFacialRecognizer : IDetect
    {
        private EigenFaceRecognizer _faceRecognizer;


        public JAVSFacialRecognizer()
        {
            _faceRecognizer = new EigenFaceRecognizer();
            try
            {
                _faceRecognizer.Load(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents\\test-recognizer.yaml");

            }
            catch
            {
                var fail = "fail";
            }
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

        public int RecognizeUser(byte[] userImage)
        {
            Image<Gray, byte> convertedImage = StreamConverter.ByteToImageResize(userImage);
            return _faceRecognizer.Predict(convertedImage).Label;
        }
    }
}
