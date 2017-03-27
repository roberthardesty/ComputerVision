using JAVS.ComputerVison.Core.Detectors;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for LabeledIncrementor.xaml
    /// </summary>
    public partial class LabeledIncrementor : UserControl
    {
        private ParameterProfile _currentProfile;

        public LabeledIncrementor(ParameterProfile profile)
        {
            InitializeComponent();
            _currentProfile = profile;
            txtNum.Text = profile.CurrentValue.ToString();
            IncrementorDescription.Text = profile.Description;          
        }

        public double NumValue
        {
            get { return _currentProfile.CurrentValue; }
            set
            {
                _currentProfile.CurrentValue = value;
                txtNum.Text = value.ToString();
            }
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            NumValue += _currentProfile.Interval;
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            NumValue -= _currentProfile.Interval;
        }

        private void txtNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNum == null)
            {
                return;
            }
            double target;
            if (!double.TryParse(txtNum.Text, out target))
            {
                _currentProfile.CurrentValue = target;
                txtNum.Text = target.ToString();
            }
        }
    }
}
