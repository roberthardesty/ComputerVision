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
using JAVS.ComputerVison.Core.Helper;

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
        private JavsFacesEmgu _detector;
        private CameraManager _camera;
        private List<Bitmap> _savedFaces = new List<Bitmap>();
        //private FaceTrainer _trainer;

        #region Constructor & Properties
        public SaveFaceDialog()
        {
            _dataClient = new DataStoreAccess(@"C:\data\db\SQLite-Faces.db");
            List<string> usernames = _dataClient.GetAllUsernames();
            if (usernames != null)
                foreach (var name in usernames)
                    foreach (var face in _dataClient.CallFaces(name))
                        _savedFaces.Add(StreamConverter.ByteToBitmap(face.Image));

            _detector = new JavsFacesEmgu();
            _camera = new CameraManager();
            _camera.SetDetector(_detector);
            _cameraIsReady = _camera.CanCapture();

            if (_cameraIsReady)
            {
                _camera.GetImages();
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

        public List<Bitmap> SavedFaces
        {
            get { return _savedFaces; }
            set
            {
                _savedFaces = value;
                NotifyPropertyChanged();
            }
        }
        #endregion
        #region Methods
        void AttachFrames(object sender, EventArgs e)
        {
            _camera.OriginalFrame.Freeze();
            OriginalImage = _camera.OriginalFrame;

            _camera.ProcessedFrames[0].Freeze();
            _imageFace = _camera.ProcessedFrames[0];

            NotifyPropertyChanged("FaceImage");
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
            _dataClient.SaveFace(_username, face);   
        }

        private void TrainFace_Click(object sender, RoutedEventArgs e)
        {
            for(int i =0; i<_savedFaces.Count(); i++)
            {
               var success = MyFileManager.SaveImage(_savedFaces[i], i.ToString());
            }

        }

        #endregion

    }
}
