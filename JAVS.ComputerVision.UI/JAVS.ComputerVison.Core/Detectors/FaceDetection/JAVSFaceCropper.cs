using JAVS.ComputerVision.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV.Structure;
using JAVS.ComputerVision.Core.Detectors;
#if !(__IOS__ || NETFX_CORE)
using Emgu.CV.Cuda;
#endif

namespace JAVS.ComputerVision.Core.FaceDetection
{
    public class JAVSFaceCropper : BaseDetector, IDetect
    {
        private Stopwatch watch;

//      private string uppertorsoFileName = @"C:\Users\roberth\MyProjects\ComputerVision\JAVS.ComputerVision.UI\JAVS.ComputerVison.Core\Data\CascadeData\haarcascade_fullbody.xml";

        private string faceFileName = @"C:\Users\roberth\MyProjects\ComputerVision\JAVS.ComputerVision.UI\JAVS.ComputerVison.Core\Data\CascadeData\haarcascade_frontalface_default.xml";

        private string eyeFileName = @"C:\Users\roberth\MyProjects\ComputerVision\JAVS.ComputerVision.UI\JAVS.ComputerVison.Core\Data\CascadeData\haarcascade_eye_tree_eyeglasses.xml";

        public JAVSFaceCropper()
        {
            LoadParameters();
        }

        public string DisplayName { get { return "Face Detection (Crop)"; } }

        public Dictionary<string, ParameterProfile> AdjustableParameters { get; set; }

        int IDetect.ProcessCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<IImage> ProcessFrame(IImage original)
        {
            List<Rectangle> faces = new List<Rectangle>();
            List<Rectangle> eyes = new List<Rectangle>();
            using (InputArray OriginalImage = original.GetInputArray())
            {

                //Cuda Conditionals
#if !(__IOS__ || NETFX_CORE)
                if (OriginalImage.Kind == InputArray.Type.CudaGpuMat && CudaInvoke.HasCuda)
                {
                    return new List<IImage> { CudaDetect(original, faces, eyes) };
                }
                else
#endif
                {
                    return Detect(original, faces, eyes);
                }
            }
        }

        List<IImage> Detect(IImage original, List<Rectangle> faces, List<Rectangle> eyes)
        {
            //Read the HaarCascade objects
            using (CascadeClassifier face = new CascadeClassifier(faceFileName))
           // using (CascadeClassifier eye = new CascadeClassifier(eyeFileName))
            {
                using (UMat ugray = new UMat())
                {
                    CvInvoke.CvtColor(original, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                    //normalizes brightness and increases contrast of the image
                    CvInvoke.EqualizeHist(ugray, ugray);

                    //Detect the faces  from the gray scale image and store the locations as rectangle
                    //The first dimensional is the channel
                    //The second dimension is the index of the rectangle in the specific channel
                    Rectangle[] facesDetected = face.DetectMultiScale(
                        ugray,
                        AdjustableParameters["ScaleStepRatio"].CurrentValue,
                        (int)AdjustableParameters["MinimumNieghbors"].CurrentValue,
                        new Size((int)AdjustableParameters["MinimumSearchSize"].CurrentValue, (int)AdjustableParameters["MinimumSearchSize"].CurrentValue));

                    faces.AddRange(facesDetected);

                }
                return CopyAndCrop(original, faces.ToArray());
            }
        }

        IImage CudaDetect(IImage original, List<Rectangle> faces, List<Rectangle> eyes)
        {
            using (CudaCascadeClassifier face = new CudaCascadeClassifier(faceFileName))
                using (CudaCascadeClassifier eye = new CudaCascadeClassifier(eyeFileName))
                {
                    face.ScaleFactor = 1.1;
                    face.MinNeighbors = 10;
                    face.MinObjectSize = Size.Empty;
                    eye.ScaleFactor = 1.1;
                    eye.MinNeighbors = 10;
                    eye.MinObjectSize = Size.Empty;
                    using (CudaImage<Bgr, Byte> gpuImage = new CudaImage<Bgr, byte>(original))
                    using (CudaImage<Gray, Byte> gpuGray = gpuImage.Convert<Gray, Byte>())
                    using (GpuMat region = new GpuMat())
                    {
                        face.DetectMultiScale(gpuGray, region);
                        Rectangle[] faceRegion = face.Convert(region);
                        faces.AddRange(faceRegion);
                        foreach (Rectangle f in faceRegion)
                        {
                            using (CudaImage<Gray, Byte> faceImg = gpuGray.GetSubRect(f))
                            {
                                //For some reason a clone is required.
                                //Might be a bug of CudaCascadeClassifier in opencv
                                using (CudaImage<Gray, Byte> clone = faceImg.Clone(null))
                                using (GpuMat eyeRegionMat = new GpuMat())
                                {
                                    eye.DetectMultiScale(clone, eyeRegionMat);
                                    Rectangle[] eyeRegion = eye.Convert(eyeRegionMat);
                                    foreach (Rectangle e in eyeRegion)
                                    {
                                        Rectangle eyeRect = e;
                                        eyeRect.Offset(f.X, f.Y);
                                        eyes.Add(eyeRect);
                                    }
                                }
                            }
                        }
                    }
                }
            IImage copy = CopyAndDraw(original, faces.ToArray());
            copy = CopyAndDraw(copy, eyes.ToArray());
            return copy;
                //return eyes;
        }

        void LoadParameters()
        {
            AdjustableParameters = new Dictionary<string, ParameterProfile>();
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
            AdjustableParameters["BoundsX"] = new ParameterProfile
            {
                Description = "X location of bounding rectangle",
                MaxValue = 10000,
                MinValue = 0,
                CurrentValue = 190,
                Interval = 10
            };
            AdjustableParameters["BoundsY"] = new ParameterProfile
            {
                Description = "Y location of bounding rectangle",
                MaxValue = 10000,
                MinValue = 0,
                CurrentValue = 60,
                Interval = 10
            };
            AdjustableParameters["BoundsHeight"] = new ParameterProfile
            {
                Description = "Height of bounding rectangle",
                MaxValue = 10000,
                MinValue = 0,
                CurrentValue = 380,
                Interval = 10
            };
            AdjustableParameters["BoundsWidth"] = new ParameterProfile
            {
                Description = "Width of bounding rectangle",
                MaxValue = 10000,
                MinValue = 0,
                CurrentValue = 240,
                Interval = 10
            };
        }
    }
}


