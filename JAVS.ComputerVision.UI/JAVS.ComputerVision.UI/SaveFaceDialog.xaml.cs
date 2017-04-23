using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JAVS.ComputerVision.Core.FacialRecognition;
using JAVS.ComputerVision.Core.FaceDetection;
using JAVS.ComputerVision.Core.Helper;
using Microsoft.Win32;
using System.Drawing;
using JAVS.ComputerVision.Core.Helper;
using JAVS.ComputerVision.Core.Detectors.FaceDetection;
using JAVS.ComputerVision.Core.FacialRecognition;
using JAVS.ComputerVision.Core.Interfaces;

namespace JAVS.ComputerVision.UI
{
    /// <summary>
    /// Interaction logic for SaveFaceDialog.xaml
    /// </summary>
    public partial class SaveFaceDialog : Window, INotifyPropertyChanged
    {
        private bool _cameraIsReady;
        private string _username;
        private BitmapSource _imageOriginal;
        private BitmapSource _imageFace;
        private DataStoreAccess _dataClient;
        private JAVSFaceCropper _detector;
        private TrainingEngine _trainer;
        private JAVSFacialRecognizer _recognizer;
        private ISourceManager _camera;
        //private FaceTrainer _trainer;

        #region Constructor & Properties
        public SaveFaceDialog()
        {
            _dataClient = new DataStoreAccess(@"C:\data\db\SQLite-Faces.db");

            _detector = new JAVSFaceCropper();
            _trainer = new TrainingEngine();
            _camera = new CameraManager();
            _camera.SetDetector(_detector);
            _cameraIsReady = _camera.IsReady();

            if (_cameraIsReady)
            {
                _camera.Start();
                _camera.NewFrame += AttachFrames;
            }

            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                NotifyPropertyChanged();
            }
        }

        public string ResultsString { get; set; }

        public bool IsFaceDetected { get { return true; } }

        public BitmapSource OriginalImage
        {
            get
            {
                return _imageOriginal;
            }
            set
            {
                _imageOriginal = value;
                NotifyPropertyChanged();
            }
        }

        public BitmapSource FaceImage
        {
            get
            {
                return _imageFace;
            }
            set
            {
                _imageFace = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
        #region Methods
        void AttachFrames(object sender, EventArgs e)
        {
            _camera.OriginalFrame.Freeze();
            OriginalImage = _camera.OriginalFrame;
            if (_camera.ProcessedFrames.Count() > 0)
            {
                _camera.ProcessedFrames[0].Freeze();
                FaceImage = _camera.ProcessedFrames[0];
            }
        }
        void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region User Events
        void SaveFace_Click(object sender, RoutedEventArgs e)
        {
            byte[] face = StreamConverter.ImageToByte(_imageFace.ToBitmap());
            ResultsString = _dataClient.SaveFace(_username, face);   
        }

        private void TrainFace_Click(object sender, RoutedEventArgs e)
        {
            List<Face> faces = _dataClient.CallFaces("ALL_USERS");
            //Save all the current saved faces to file for quality check
            for (int i =0; i< faces.Count(); i++)
            {
               var success = MyFileManager.SaveImage(StreamConverter.ByteToBitmap(faces[i].Image), i.ToString());
            }
            //Get data ready for training and train.
            byte[][] faceImages = faces.Select(face => face.Image.Clone() as byte[]).ToArray();
            int[] faceLabels = faces.Select(f => f.UserId).ToArray();
            //Echo results to UI
            ResultsString = _trainer.Train(faceImages, faceLabels)?
                 "Training Successfull" : "Training Failed";
            NotifyPropertyChanged("ResultsString");
        }

        private void TestRecognize_Click(object sender, RoutedEventArgs e)
        {
            _recognizer = new JAVSFacialRecognizer();
            int foundUserId = _recognizer.RecognizeUser(StreamConverter.ImageToByte(_imageFace.ToBitmap()));                
            ResultsString = _dataClient.GetUsername(foundUserId);
            NotifyPropertyChanged("ResultsString");
        }

        #endregion

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (_camera != null)
            {
                _camera.NewFrame -= AttachFrames;
                _camera.Dispose();
                FaceImage = null;
                OriginalImage = null;
            }
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == true)
            {
                _camera = new MyFileManager();
                _camera.SetDetector(_detector);
                _camera.Start(0, ofd.FileName);
                _camera.NewFrame += AttachFrames;
            }
        }
    }
}

