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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageDatabaseUI 
{
    /// <summary>
    /// Interaction logic for ImageControl.xaml
    /// </summary>
    public partial class ImageControl : UserControl, INotifyPropertyChanged
    {
        private string _username;
        private BitmapSource _faceImageBitmapSource;

        public ImageControl()
        {
            InitializeComponent();
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
        public BitmapSource FaceImageBitmapSource
        {
            get { return _faceImageBitmapSource; }
            set
            {
                _faceImageBitmapSource = value;
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
