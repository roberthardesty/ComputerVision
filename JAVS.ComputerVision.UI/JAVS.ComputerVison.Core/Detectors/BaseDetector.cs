using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAVS.ComputerVison.Core.Detectors
{
    public class BaseDetector
    {

        protected IImage CopyAndDraw(IImage original, Rectangle[] shapes)
        {
            IImage copy = (IImage)original.Clone();

            foreach (Rectangle shape in shapes)
                CvInvoke.Rectangle(copy, shape, new Bgr(Color.Red).MCvScalar, 3); 

            return copy;
        }

        protected IImage CopyAndDraw(IImage original, Rectangle[] shapes, Rectangle bounds)
        {
            IImage copy = (IImage)original.Clone();

            foreach (Rectangle shape in shapes)
            {
                if(bounds!=null)
                    CvInvoke.Rectangle(copy, bounds, new Bgr(Color.DodgerBlue).MCvScalar, 2);

                if (bounds.Contains(shape))
                    CvInvoke.Rectangle(copy, shape, new Bgr(Color.Green).MCvScalar, 3);
                else
                    CvInvoke.Rectangle(copy, shape, new Bgr(Color.Red).MCvScalar, 3);
            }               

            return copy;
        }
    }
}
