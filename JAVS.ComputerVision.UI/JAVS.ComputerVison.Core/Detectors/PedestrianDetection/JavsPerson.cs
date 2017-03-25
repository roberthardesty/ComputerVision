using JAVS.ComputerVison.Core.Interfaces;
using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using System.Diagnostics;
using Emgu.CV.Util;
#if !(__IOS__ || NETFX_CORE)
using Emgu.CV.Cuda;
#endif


namespace JAVS.ComputerVison.Core.Detectors.PedestrianDetection
{
    public class JavsPerson : IDetect
    {
        public string DisplayName { get { return "Person Detection (EmguCV)"; } }

        public int ProcessCount()
        {
            return 1;
        }

        public List<IImage> ProcessFrame(IImage original)
        {
            IImage originalClone = (IImage)original.Clone();
            Rectangle[] peopleRegion;

            using (InputArray iaImage = original.GetInputArray())
            {
#if !(__IOS__ || NETFX_CORE)
                //if the input array is a GpuMat
                //check if there is a compatible Cuda device to run pedestrian detection
                if (iaImage.Kind == InputArray.Type.CudaGpuMat)
                {
                    //this is the Cuda version
                    using (CudaHOG des = new CudaHOG(new Size(64, 128), new Size(16, 16), new Size(8, 8), new Size(8, 8)))
                    {
                        des.SetSVMDetector(des.GetDefaultPeopleDetector());

                        using (GpuMat cudaBgra = new GpuMat())
                        using (VectorOfRect vr = new VectorOfRect())
                        {
                            CudaInvoke.CvtColor(original, cudaBgra, ColorConversion.Bgr2Bgra);
                            des.DetectMultiScale(cudaBgra, vr);
                            peopleRegion = vr.ToArray();
                        }
                    }
                }
                else
#endif
                {
                    //this is the CPU/OpenCL version
                    using (HOGDescriptor peopleDescriptor = new HOGDescriptor())
                    {
                        peopleDescriptor.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());

                        MCvObjectDetection[] peopleFound = peopleDescriptor.DetectMultiScale(original);
                        peopleRegion = new Rectangle[peopleFound.Length];
                        for (int i = 0; i < peopleFound.Length; i++)
                            peopleRegion[i] = peopleFound[i].Rect;
                    }
                }
                foreach (var person in peopleRegion)
                    CvInvoke.Rectangle(originalClone, person, new Bgr(Color.Red).MCvScalar, 2);
                return new List<IImage> { originalClone };
            }
        }
    }
}
