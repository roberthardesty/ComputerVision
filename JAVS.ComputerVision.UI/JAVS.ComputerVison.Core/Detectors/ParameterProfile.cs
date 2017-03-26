using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAVS.ComputerVison.Core.Detectors
{
    public class ParameterProfile
    {
        public string Description { get; set; }
        public double MaxValue { get; set; }
        public double CurrentValue { get; set; }
        public double MinValue { get; set; }
        public double Interval { get; set; }
    }
}
