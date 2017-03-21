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

namespace JAVS.ComputerVision.UI.Controls
{
    /// <summary>
    /// Interaction logic for ResultsDisplay.xaml
    /// </summary>
    public partial class ResultsDisplay : UserControl
    {
        private BitmapSource _image;

        public event PropertyChangedEventHandler PropertyChanged;
        public MainWindow MyParent
        {
            get;set;
        }

        public ResultsDisplay()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public BitmapSource MyImage {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                NotifyPropertyChanged();
            }
        }

        public void UpdateFrame()
        {
            MyImage = MyParent.ImageOriginal;
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
