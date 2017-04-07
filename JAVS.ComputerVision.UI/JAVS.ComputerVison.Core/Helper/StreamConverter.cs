using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace JAVS.ComputerVision.Core.Helper
{
    public class StreamConverter
    {
        void BitmapSourceToStream(BitmapSource bitmap)
        {
            Bitmap tempBitmap = bitmap.ToBitmap();
        }
    }
}
