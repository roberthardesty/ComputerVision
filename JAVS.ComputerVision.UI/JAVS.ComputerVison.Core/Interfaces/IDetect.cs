using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAVS.ComputerVison.Core.Interfaces
{
    public interface IDetect
    {
        List<IImage> ProcessFrame(IImage original);
        int ProcessCount();
    }
}
