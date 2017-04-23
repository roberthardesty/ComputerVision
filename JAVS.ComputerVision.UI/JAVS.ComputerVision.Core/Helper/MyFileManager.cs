using Emgu.CV;
using Emgu.CV.Structure;
using JAVS.ComputerVision.Core.Helper;
using JAVS.ComputerVision.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using JAVS.ComputerVision.Core.Detectors;

namespace JAVS.ComputerVision.Core.Helper
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
        public BitmapSource OriginalFrame { get; set; }
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
           // if (_videoFile != null && _videoFile.IsOpened) return;

            _videoFile = new VideoCapture(path);
            _videoFile.Start();
            _videoFile.ImageGrabbed -= ProcessFrame;
            _videoFile.ImageGrabbed += ProcessFrame;
             SourceStatistics stats = GetVideoFileProperties();
            stats.FPS = 10;
            stats.FrameHeight = 480;
            stats.FrameWidth = 600;
            SetVideoFileProperties(stats);
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
                var props = GetVideoFileProperties();

            }
            catch(Exception ex)
            {

            }
            OriginalFrame = MatConverter.ToBitmapSource(frame);
            ProcessedFrames = new List<BitmapSource>();
            List<IImage> proccessedFrames = _detectionManager.ProcessFrame(frame);
            proccessedFrames.ForEach(pFrame =>
            { ProcessedFrames.Add(MatConverter.ToBitmapSource(pFrame)); });

            NewFrame.Invoke(null, new EventArgs());
        }

        SourceStatistics GetVideoFileProperties()
        {
            return new SourceStatistics(_videoFile);
        }
        void SetVideoFileProperties(SourceStatistics stats)
        {
            foreach (var stat in stats.AllStats.Keys)
                _videoFile.SetCaptureProperty(stat, stats.AllStats[stat]);
        }
        #endregion
    }
 
}
