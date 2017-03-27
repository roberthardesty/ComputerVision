using Emgu.CV;
using JAVS.ComputerVison.Core.Detectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAVS.ComputerVison.Core.Interfaces
{
    public interface IDetect
    {
        string DisplayName { get; }
        Dictionary<string, ParameterProfile> AdjustableParameters { get; set; }
        List<IImage> ProcessFrame(IImage original);
        int ProcessCount { get; }
    }
}
