using JAVS.ComputerVision.Core.Interfaces;
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


namespace JAVS.ComputerVision.Core.Detectors.PedestrianDetection
{
    public class JavsPerson : BaseDetector, IDetect
    {
        public Dictionary<string, ParameterProfile> AdjustableParameters { get; set; }

        public string DisplayName { get { return "Person Detection (EmguCV)"; } }

        public JavsPerson()
        {
            AdjustableParameters = new Dictionary<string, ParameterProfile>();
            AdjustableParameters["Scale"] = new ParameterProfile
            {
                Description = "Coefficient of the detection window increase",
                MaxValue = 2,
                MinValue = 1,
                CurrentValue = 1.05,
                Interval = 0.01
            };
            AdjustableParameters["SimilarityThreshold"] = new ParameterProfile
            {
                Description = "Minimum similarity Threshold, 0 = no grouping",
                MaxValue = 10,
                MinValue = 0,
                CurrentValue = 2,
                Interval = 0.5
            };
            AdjustableParameters["MeanShiftGrouping"] = new ParameterProfile
            {
                Description = "Use (1) or don't use (0) Mean Shift Grouping",
                MaxValue = 1,
                MinValue = 0,
                CurrentValue = 0,
                Interval = 1
            };
        }

        int IDetect.ProcessCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<IImage> ProcessFrame(IImage original)
        {
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

                        MCvObjectDetection[] peopleFound = peopleDescriptor
                            .DetectMultiScale(original, 0, default(Size), default(Size), AdjustableParameters["Scale"].CurrentValue ,AdjustableParameters["SimilarityThreshold"].CurrentValue, AdjustableParameters["MeanShiftGrouping"].CurrentValue==1);
                        peopleRegion = new Rectangle[peopleFound.Length];
                        for (int i = 0; i < peopleFound.Length; i++)
                            peopleRegion[i] = peopleFound[i].Rect;
                    }
                }

                IImage copy = CopyAndDraw(original, peopleRegion);
                return new List<IImage> { copy };
            }
        }
    }
}
