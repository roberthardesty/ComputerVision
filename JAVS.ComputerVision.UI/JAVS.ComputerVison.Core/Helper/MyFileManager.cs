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
        private IDetect _detectionManager;

        public event EventHandler NewFrame;

        public Dictionary<string, ParameterProfile> CurrentParameters
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsReady()
        {
            return true;
        }

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
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
