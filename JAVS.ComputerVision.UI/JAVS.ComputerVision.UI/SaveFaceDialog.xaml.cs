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

namespace JAVS.ComputerVision.UI
{
    /// <summary>
    /// Interaction logic for SaveFaceDialog.xaml
    /// </summary>
    public partial class SaveFaceDialog : Window, INotifyPropertyChanged
    {
        private BitmapSource _imageOriginal;
        private BitmapSource _imageFace;
        private DataStoreAccess _dataClient;
        private JavsFacesEmgu _detector;
        //private FaceTrainer _trainer;

        public event PropertyChangedEventHandler PropertyChanged;

        public SaveFaceDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public bool IsFaceDetected { get; set; }

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


        void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
