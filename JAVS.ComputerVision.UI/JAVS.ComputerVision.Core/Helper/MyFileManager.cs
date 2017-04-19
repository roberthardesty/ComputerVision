using Emgu.CV;
using Emgu.CV.Structure;
using JAVS.ComputerVision.Core.Helper;
using JAVS.ComputerVision.Core.Interfaces;
using JAVS.ComputerVison.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using JAVS.ComputerVision.Core.Detectors;

namespace JAVS.ComputerVison.Core.Helper
{
    public class MyFileManager : ISourceManager
    {
        private string _filepath;
        private IDetect _detectionManager;
        private VideoCapture _videoFile;

        public event EventHandler NewFrame;

        public Dictionary<string, ParameterProfile> CurrentParameters
        { get {return _detectionManager.AdjustableParameters; }
          set { _detectionManager.AdjustableParameters = value; }
        }

        public bool IsReady()
        {
            return true;
        }

        public string FilePath
        {
            get { return _filepath; }
            set
            {
                if (File.Exists(value))
                    _filepath = value;
            }
        }

        public List<BitmapSource> ProcessedFrames { get; set; }

        public MyFileManager() { }
        public MyFileManager(IDetect detector)
        {
            _detectionManager = detector;
        }

        #region Actions

        public List<BitmapSource> OpenAndProcess(string path)
        {
            List<BitmapSource> processedFrames = new List<BitmapSource>();
            List<IImage> sources = _detectionManager.ProcessFrame(new Image<Gray, byte>(path));
            sources.ForEach((img) =>
            { processedFrames.Add(MatConverter.ToBitmapSource(img)); });
            return processedFrames;
        }
        public static bool SaveImage(Bitmap img, string path)
        {
            try
            {
                using (FileStream file = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)+"\\Pictures\\test-"+path+".png", FileMode.Create))
                {
                    img.Save(file, System.Drawing.Imaging.ImageFormat.Png);
                }
                return true;
            }
            catch(Exception e)
            {
                var test = e;
                return false;
            }            
        }
        public void SetDetector(IDetect detect)
        {
            _detectionManager = detect;
        }

        public void Start(int captureSource, string path)
        {
            if (_videoFile != null && _videoFile.IsOpened) return;

            _videoFile = new VideoCapture(_filepath);
            _videoFile.Start();
            _videoFile.ImageGrabbed -= ProcessFrame;
            _videoFile.ImageGrabbed += ProcessFrame;
        }

        public void Dispose()
        {
            if (_videoFile == null) return;
            _videoFile.ImageGrabbed -= ProcessFrame;
            _videoFile.Stop();
            _videoFile.Dispose();
            _videoFile = null;
        }
        #endregion

        #region Private Methods
        void ProcessFrame(object sender, EventArgs e)
        {
            Mat frame = new Mat();
            try
            {
                _videoFile.Retrieve(frame, 0);

            }
            catch(Exception ex)
            {

            }
            ProcessedFrames = new List<BitmapSource>() { MatConverter.ToBitmapSource(frame) };
            List<IImage> proccessedFrames = _detectionManager.ProcessFrame(frame);
            proccessedFrames.ForEach(pFrame =>
            { ProcessedFrames.Add(MatConverter.ToBitmapSource(pFrame)); });

            NewFrame.Invoke(null, new EventArgs());
        }
        #endregion
    }
}
