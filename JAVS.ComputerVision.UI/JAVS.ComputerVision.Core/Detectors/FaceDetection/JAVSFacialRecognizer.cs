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
using System.Drawing;
using JAVS.ComputerVision.Core.FacialRecognition;

namespace JAVS.ComputerVision.Core.Detectors.FaceDetection
{
    public class JAVSFacialRecognizer : BaseDetector, IDetect
    {
        private DataStoreAccess _dataClient;
        private EigenFaceRecognizer _faceRecognizer;
        private string _faceFileName => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + 
            @"\MyProjects\ComputerVision\JAVS.ComputerVision.UI\JAVS.ComputerVision.Core\Data\CascadeData\haarcascade_frontalface_default.xml";

        public JAVSFacialRecognizer()
        {
            AdjustableParameters = new Dictionary<string, ParameterProfile>();
            _dataClient = new DataStoreAccess(@"C:\data\db\SQLite-Faces.db");
            LoadRecognizer();
            LoadParameters();
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

        public string LoadRecognizer(string path = null)
        {
            _faceRecognizer = new EigenFaceRecognizer();
            try
            {
                if (path == null)
                    path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents\\test-recognizer.yaml";
                _faceRecognizer.Load(path);
                return "Loaded Successfully";       
            }
            catch
            {
                return "Something went wrong!";
            }
        }

        public List<IImage> ProcessFrame(IImage original)
        {
            using (CascadeClassifier faceClassifier = new CascadeClassifier(_faceFileName))
            {
                List<Rectangle> faces = new List<Rectangle>();
                using (UMat ugray = new UMat())
                {
                    CvInvoke.CvtColor(original, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                    //normalizes brightness and increases contrast of the image
                    CvInvoke.EqualizeHist(ugray, ugray);

                    //Detect the faces  from the gray scale image and store the locations as rectangle
                    //The first dimensional is the channel
                    //The second dimension is the index of the rectangle in the specific channel
                    Rectangle[] facesDetected = faceClassifier.DetectMultiScale(
                        ugray,
                        AdjustableParameters["ScaleStepRatio"].CurrentValue,
                        (int)AdjustableParameters["MinimumNieghbors"].CurrentValue,
                        new Size((int)AdjustableParameters["MinimumSearchSize"].CurrentValue, (int)AdjustableParameters["MinimumSearchSize"].CurrentValue));

                    faces.AddRange(facesDetected);

                }
                List<IImage> croppedFaces = CopyAndCrop(original, faces.ToArray());
                IImage originalCopy = original.Clone() as IImage;
                for(int i = 0; i < croppedFaces.Count(); i++)
                {
                    int foundFaceId = RecognizeUser(StreamConverter.ImageToByte(croppedFaces[i].Bitmap));
                    string foundFaceName = _dataClient.GetUsername(foundFaceId);
                    CvInvoke.PutText(originalCopy, foundFaceName, faces[i].Location - new Size(0, faces[i].Height/3), Emgu.CV.CvEnum.FontFace.HersheyTriplex, 2, new Bgr(Color.RoyalBlue).MCvScalar);             
                }
                return new List<IImage>() { originalCopy };
            }
        }

        public int RecognizeUser(byte[] userImage)
        {
            Image<Gray, byte> convertedImage = StreamConverter.ByteToImageResize(userImage);
            return _faceRecognizer.Predict(convertedImage).Label;
        }

        void LoadParameters()
        {
            AdjustableParameters["ScaleStepRatio"] = new ParameterProfile
            {
                Description = "Ratio of the new scale to the old scale when stepping up scales",
                MaxValue = 3,
                MinValue = 1.001,
                CurrentValue = 1.1,
                Interval = 0.001
            };
            AdjustableParameters["MinimumNieghbors"] = new ParameterProfile
            {
                Description = "Minimum number of nearby matching features to qualify as face",
                MaxValue = 25,
                MinValue = 1,
                CurrentValue = 7,
                Interval = 1
            };
            AdjustableParameters["MinimumSearchSize"] = new ParameterProfile
            {
                Description = "Minimum Length/Width of Search Square in pixels",
                MaxValue = 200,
                MinValue = 1,
                CurrentValue = 40,
                Interval = 1
            };
           
        }

    }
}
