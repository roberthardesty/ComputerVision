using Emgu.CV;
using JAVS.ComputerVision.Core.Detectors;
using JAVS.ComputerVision.Core.Interfaces;
using JAVS.ComputerVison.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace JAVS.ComputerVision.Core.Helper
{
    public class CameraManager : ISourceManager
    {
        private static VideoCapture _source;
        private IDetect _detectionManager;
        private List<BitmapSource> _processedFrames;
        private BitmapSource _originalFrame;

        public CameraManager()
        {
          
        }

        public int FrameCount;
        //this exposes the detector's adustable parameters to the ui
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
        //This allows you to make sure the camera is ready.
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
        
        public void SetDetector(IDetect detect)
        {
            _detectionManager = detect;
        }
        //This starts the capture process and wires an event up to the source.
        public void Start(int captureSource = 0, string path = null)
        {
            if(_source == null)
                _source = new VideoCapture(captureSource);
            _source.Start();
            _source.ImageGrabbed -= ConvertFrame;
            _source.ImageGrabbed += ConvertFrame;
        }
        //This event is exposed to the UI to let it know when a new frame is ready
        public event EventHandler NewFrame;

        //This is where the magic happens. Handles each frame from the camera and performs image processing.
        //Once frames are finished being processed and converted to UI friendly format the event is triggered to alert ui
        void ConvertFrame(object sender, EventArgs e)
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

            List<IImage> processedImages = _detectionManager.ProcessFrame(capturedImage);
            processedImages.ForEach((img) =>
            { _processedFrames.Add(MatConverter.ToBitmapSource(img)); });

            _originalFrame = MatConverter.ToBitmapSource(capturedImage);
            
            NewFrame?.Invoke(null, new EventArgs());
        }
        //This ends the capture process and removes the videoCapture class from memory
        public void Dispose()
        {
            if (_source == null) return;
            _source.ImageGrabbed -= ConvertFrame;
            _source.Stop();
            _source.Dispose();
            _source = null;
        }

    }
}
