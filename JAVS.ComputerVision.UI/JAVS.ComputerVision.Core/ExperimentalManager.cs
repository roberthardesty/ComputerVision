using JAVS.ComputerVision.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JAVS.ComputerVision.Core.Detectors;
using System.Windows.Media.Imaging;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.VideoSurveillance;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using JAVS.ComputerVision.Core.Helper;

namespace JAVS.ComputerVision.Core
{
    public class ExperimentalManager : ISourceManager
    {
        #region Member variables
        private static VideoCapture _source;
        private List<BitmapSource> _processedFrames;
        private BitmapSource _originalFrame;
        private MotionHistory _motionHistory;
        private Rectangle _recentMotion;
        private BackgroundSubtractor _backgroundSubtractor;
        private SourceStatistics _stats;
        private string faceFileName = @"C:\Users\roberth\MyProjects\ComputerVision\JAVS.ComputerVision.UI\JAVS.ComputerVision.Core\Data\CascadeData\haarcascade_frontalface_default.xml";

        #endregion

        public ExperimentalManager()
        {
            _backgroundSubtractor = new BackgroundSubtractorMOG2();
            //Can the parameters taken by this constructor be adjusted during capture?
            _motionHistory = new MotionHistory(
                .2, //in second, the duration of motion history you wants to keep
                0.1, //in second, maxDelta for cvCalcMotionGradient
                0.5); //in second, minDelta for cvCalcMotionGradient  
        }

        #region ISourceManager Implentation
        public Dictionary<string, ParameterProfile> CurrentParameters { get; set; }

        public BitmapSource OriginalFrame
        {
            get { return _originalFrame; }
            set { _originalFrame = value; }
        }
        public List<BitmapSource> ProcessedFrames
        {
            get { return _processedFrames; }
            set { _processedFrames = value; }
        }

        public event EventHandler NewFrame;

        public void Dispose()
        {
            if (_source == null) return;
            _source.ImageGrabbed -= ConvertFrame;
            _source.Stop();
            _source.Dispose();
            _source = null;
        }

        public bool IsReady()
        {
            try
            {
                _source = new VideoCapture();
            }
            catch
            {
                return false;
            }

            if (_source == null) return false;
            Dispose();
            return true;
        }

        public void SetDetector(IDetect detector)
        {
            //do nothing
        }

        public void Start(int captureSource = 0, string path = null)
        {
            if (_source == null)
                _source = new VideoCapture(captureSource);
            _source.Start();
            _source.ImageGrabbed -= ConvertFrame;
            _source.ImageGrabbed += ConvertFrame;
            _stats = new SourceStatistics(_source);
        }
        #endregion

        async void ConvertFrame(object sender, EventArgs e)
        {
            Mat capturedImage = new Mat();
            _processedFrames = new List<BitmapSource>();
            try
            {
                _source.Retrieve(capturedImage, 0);

            }
            catch (NullReferenceException ne)
            {

            }

            //List<IImage> processedImages = _detectionManager.ProcessFrame(capturedImage);
            //processedImages.ForEach((img) =>
            //{ _processedFrames.Add(MatConverter.ToBitmapSource(img)); });

            _originalFrame = MatConverter.ToBitmapSource(capturedImage);
            Mat originalClone = capturedImage.Clone();
            if(_recentMotion != null)
                CvInvoke.Rectangle(originalClone, _recentMotion, new Bgr(Color.Red).MCvScalar, 1);
            _processedFrames = new List<BitmapSource>() { MatConverter.ToBitmapSource(originalClone) };
            NewFrame?.Invoke(null, new EventArgs());

            KeyValuePair<bool, Rectangle> motionUpdate = await GetLargestMotionShape(capturedImage);
            if (motionUpdate.Key == false) return;
            //Copy and Draw around motion
            _recentMotion = motionUpdate.Value;
            Mat captureClone = capturedImage.Clone();
            CvInvoke.Rectangle(captureClone, _recentMotion, new Bgr(Color.Red).MCvScalar, 1);

            _processedFrames = new List<BitmapSource>()
            {
                MatConverter.ToBitmapSource(captureClone)
            };
            NewFrame?.Invoke(null, new EventArgs());

            Tuple<bool, Rectangle, Image<Gray, byte>> faceUpdate = await GetLargestFace(capturedImage, motionUpdate.Value);
            if (faceUpdate.Item1 == false) return;

            CvInvoke.Rectangle(captureClone, faceUpdate.Item2, new Bgr(Color.Red).MCvScalar, 3);
            _processedFrames[0] = MatConverter.ToBitmapSource(captureClone);
            NewFrame?.Invoke(null, new EventArgs());
        }

        async Task<KeyValuePair<bool, Rectangle>> GetLargestMotionShape(IImage original)
        {
            Mat foregroundBlobs = new Mat();
            Mat motionMask = new Mat();
            Mat segmentMask = new Mat();
            Rectangle largestMotion;

            double minMotion = 15;
            double minMotionArea = _stats.FrameArea / 20;

            _backgroundSubtractor.Apply(original, foregroundBlobs);
            _motionHistory.Update(foregroundBlobs);

            double[] minValues, maxValues;
            Point[] minLocation, maxLocation;
            _motionHistory.Mask.MinMax(out minValues, out maxValues, out minLocation, out maxLocation);
            //Mutiply the copy by a scalar array outputs motionMask
            using (ScalarArray myScalar = new ScalarArray(255.0 / maxValues[0]))
                CvInvoke.Multiply(_motionHistory.Mask, myScalar, motionMask, 1, Emgu.CV.CvEnum.DepthType.Cv8U);

            //Create the motion image
            Mat motionImage = new Mat(motionMask.Size.Height, motionMask.Size.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
            motionImage.SetTo(new MCvScalar(0));

            //Insert the motion mask into the blue channel of the motionImage
            CvInvoke.InsertChannel(motionMask, motionImage, 0);

            //Get the motion components
            using (VectorOfRect boundingRect = new VectorOfRect())
            {
                _motionHistory.GetMotionComponents(segmentMask, boundingRect);
                largestMotion = boundingRect.ToArray().OrderByDescending(motion => motion.Width*motion.Height).First();
                int area = largestMotion.Height * largestMotion.Width;              
                //find angle and pixel count for the motionComponent
                double angle, pixelCount;
                _motionHistory.MotionInfo(foregroundBlobs, largestMotion, out angle, out pixelCount);
                //reject component of motion pixel count is less than the min threshold
                if (pixelCount < area * minMotion || area < minMotionArea)
                    return new KeyValuePair<bool, Rectangle>(false, Rectangle.Empty);
                return new KeyValuePair<bool, Rectangle>(true, largestMotion);
            }
        }

        async Task<Tuple<bool, Rectangle, Image<Gray, byte>>> GetLargestFace(IImage original, Rectangle searchArea)
        {
            Image<Gray, byte> searchImage = ((Mat)original).ToImage<Gray, byte>().Copy(searchArea);

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
                    Rectangle largestFaceDetected = face.DetectMultiScale(
                        ugray,
                        1.15,
                        7,
                        new Size((int)Math.Floor(_stats.FrameWidth / 16), (int)Math.Floor(_stats.FrameWidth / 16))).OrderByDescending(found => found.Height*found.Width).First();
                    if (largestFaceDetected == null)
                        return new Tuple<bool, Rectangle, Image<Gray, byte>>(false, Rectangle.Empty, null);

                    Rectangle faceRectangle = new Rectangle(searchArea.X + largestFaceDetected.X, searchArea.Y + largestFaceDetected.Y, largestFaceDetected.Width, largestFaceDetected.Height);
                    var faceImage = searchImage.Copy(largestFaceDetected);
                    return new Tuple<bool, Rectangle, Image<Gray, byte>>(true, faceRectangle, faceImage);
                }
            }
        }
    }
}
