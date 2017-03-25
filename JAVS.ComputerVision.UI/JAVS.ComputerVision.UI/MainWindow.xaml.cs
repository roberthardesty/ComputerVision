using JAVS.ComputerVision.UI.Controls;
using JAVS.ComputerVison.Core.Detectors.PedestrianDetection;
using JAVS.ComputerVison.Core.FaceDetection;
using JAVS.ComputerVison.Core.Helper;
using JAVS.ComputerVison.Core.Interfaces;
using JAVS.ComputerVison.Core.MotionDetection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JAVS.ComputerVision.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private BitmapSource _imageOriginal;
        private CameraManager _camera;
        private List<BitmapSource> _processedImages;
        private IDetect _selectedDetector;
        private List<IDetect> _detectors = new List<IDetect>()
        {
            new JavsFacesEmgu(),
            new JavsMotion(),
            new JavsPerson(),
        };

        public event PropertyChangedEventHandler PropertyChanged;
        public bool CameraReady => CameraManager.CanCapture();
        public IDetect SelectedDetector
        {
            get { return _selectedDetector; }
            set
            {
                _selectedDetector = value;
                NotifyPropertyChanged();
            }
        }
        public List<IDetect> Detectors
        {
            get { return _detectors; }
        }
        public BitmapSource ImageOriginal
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

        public List<BitmapSource> ProcessedImages
        {
            get
            {
                return _processedImages;
            }
            set
            {
                _processedImages = value;
                NotifyPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        void AttachFrames(object sender, EventArgs e)
        {
            _processedImages = new List<BitmapSource>();
            for(int i = 0; i < _camera.ProcessedFrames.Count;i++)
            {
                _camera.ProcessedFrames[i].Freeze();
                _processedImages.Add(_camera.ProcessedFrames[i]);
            }
            _camera.OriginalFrame.Freeze();
            ImageOriginal = _camera.OriginalFrame;
            NotifyPropertyChanged("ProcessedImages");
        }

        void buttonCloseDevice_Click(object sender, RoutedEventArgs e)
        {
            if (_camera!=null)
            {
                _camera.StartCamera -= AttachFrames;
                _camera.Dispose();
                _camera = null;
            }
        }

        void buttonOpenDevice_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDetector != null && CameraReady)
            {
                _camera = new CameraManager(_selectedDetector);
                _camera.GetImages();
                _camera.StartCamera += AttachFrames;
            }
        }

        void DetectorSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_camera != null)
            {
                _camera.StartCamera -= AttachFrames;
                _camera.Dispose();
            }
        }

        void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
