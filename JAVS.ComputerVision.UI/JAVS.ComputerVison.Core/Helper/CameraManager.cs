using Emgu.CV;
using JAVS.ComputerVison.Core.Detectors;
using JAVS.ComputerVison.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace JAVS.ComputerVison.Core.Helper
{
    public class CameraManager
    {
        private static VideoCapture _source;
        private IDetect _detectionManager;
        private List<BitmapSource> _processedFrames;
        private BitmapSource _originalFrame;

        public int FrameCount;

        public Dictionary<string, ParameterProfile> CurrentParameters
        {
            get { return _detectionManager.AdjustableParameters; }
            set { _detectionManager.AdjustableParameters = value; }
        }

        public List<BitmapSource> ProcessedFrames
        {
            get { return _processedFrames; }
            set { _processedFrames = value; }
        }

        public BitmapSource OriginalFrame
        {
            get { return _originalFrame; }
            set { _originalFrame = value; }
        }

        public static bool CanCapture()
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
            _source.Dispose();
            return true;
        }

        public void SetDetector(IDetect detect)
        {
            _detectionManager = detect;
        }
        public void GetImages()
        {
            _source = new VideoCapture();
            _source.Start();
            _source.ImageGrabbed += ConvertFrame;
        }

        public event EventHandler StartCamera;

        void ConvertFrame(object sender, EventArgs e)
        {
            Mat capturedImage = new Mat();
            _processedFrames = new List<BitmapSource>();
            List<IImage> processedImages;


            _source.Retrieve(capturedImage, 0);

            processedImages = _detectionManager.ProcessFrame(capturedImage);

            foreach(IImage frame in processedImages)
            {
                _processedFrames.Add(MatConverter.ToBitmapSource(frame));
            }

            _originalFrame = MatConverter.ToBitmapSource(capturedImage);
            
            StartCamera?.Invoke(null, new EventArgs());
        }

        public void Dispose()
        {
            _source.ImageGrabbed -= ConvertFrame;
            _source.Stop();
            _source.Dispose();
            _source = null;
        }

    }
}
