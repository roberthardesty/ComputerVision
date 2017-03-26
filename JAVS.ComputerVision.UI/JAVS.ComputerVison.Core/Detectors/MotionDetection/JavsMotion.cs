using Emgu.CV;
using Emgu.CV.VideoSurveillance;
using JAVS.ComputerVison.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using JAVS.ComputerVison.Core.Interfaces;
using JAVS.ComputerVison.Core.Detectors;

namespace JAVS.ComputerVison.Core.MotionDetection
{
    public class JavsMotion : BaseDetector, IDetect
    {
        private MotionHistory _motionHistory;
        private BackgroundSubtractor _backgroundSubtractor;

        public string DisplayName { get { return "Motion Detection (EmguCV)"; } }

        public Dictionary<string, ParameterProfile> AdjustableParameters { get; set; }

        int IDetect.ProcessCount
        {
            get
            {
                return 2;
            }
        }

        public JavsMotion()
        {
            AdjustableParameters = new Dictionary<string, ParameterProfile>();
            AdjustableParameters["MinMotionArea"] = new ParameterProfile
            {
                Description = "Minimum Motion Size Threshold",
                MaxValue = 1000,
                MinValue = 5,
                CurrentValue = 100,
                Interval = 1
            };
            AdjustableParameters["MinMotionDistance"] = new ParameterProfile
            {
                Description = "Minimum Motion Distance Threshold",
                MaxValue = 1,
                MinValue = 0.005,
                CurrentValue = 0.05,
                Interval = 0.005
            };
            //Try out various background subtractors
            _backgroundSubtractor = new BackgroundSubtractorMOG2();
            //Can the parameters taken by this constructor be adjusted during capture?
            _motionHistory = new MotionHistory(
                1.0, //in second, the duration of motion history you wants to keep
                0.05, //in second, maxDelta for cvCalcMotionGradient
                0.5); //in second, minDelta for cvCalcMotionGradient    
        }

        public List<IImage> ProcessFrame(IImage original)
        {
            List<IImage> processedImages = new List<IImage>();
            Mat foregroundBlobs = new Mat();
            Mat motionMask = new Mat();
            Mat segmentMask = new Mat();
            //threshold to define the minimum motion area
            double minArea = AdjustableParameters["MinMotionArea"].CurrentValue;
            //threshold to define the minimun motion 1/20th of the size of the bounding blob
            double minMotion = AdjustableParameters["MinMotionDistance"].CurrentValue;
            Rectangle[] motionComponents;

            _backgroundSubtractor.Apply(original, foregroundBlobs);

            //update the motion-history
            _motionHistory.Update(foregroundBlobs);

            //Get a copy of the mask and enhance its color
            double[] minValues, maxValues;
            Point[] minLocation, maxLocation;
            _motionHistory.Mask.MinMax(out minValues, out maxValues, out minLocation, out maxLocation);
            //Mutiply the copy by a scalar array outputs motionMask
            using (ScalarArray myScalar = new ScalarArray(255.0 / maxValues[0]))
                CvInvoke.Multiply(_motionHistory.Mask, myScalar, motionMask, 1, Emgu.CV.CvEnum.DepthType.Cv8U);

            //Create the motion image
            Mat motionImage = new Mat(motionMask.Size.Height, motionMask.Size.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
            motionImage.SetTo(new MCvScalar(0));

            //Insert the motion mask into the blue channel of the motionImage
            CvInvoke.InsertChannel(motionMask, motionImage, 0);

            //Get the motion components
            using (VectorOfRect boundingRect = new VectorOfRect())
            {
                _motionHistory.GetMotionComponents(segmentMask, boundingRect);
                motionComponents = boundingRect.ToArray();
            }

            //Loop through the motion components
            foreach (Rectangle component in motionComponents)
            {
                int area = component.Width * component.Height;
                //reject components that are smaller that the threshold
                if (area < minArea) continue;

                //find angle and pixel count for the motionComponent
                double angle, pixelCount;
                _motionHistory.MotionInfo(foregroundBlobs, component, out angle, out pixelCount);
                //reject component of motion pixel count is less than the min threshold
                if (pixelCount < area * minMotion) continue;

                //draw motions in red
                DrawMotion(motionImage, component, angle, new Bgr(Color.Red));
            }

            // find and draw the overall motion angle
            double overallAngle, overallMotionPixelCount;

            _motionHistory.MotionInfo(foregroundBlobs, new Rectangle(Point.Empty, motionMask.Size), out overallAngle, out overallMotionPixelCount);
            DrawMotion(motionImage, new Rectangle(Point.Empty, motionMask.Size), overallAngle, new Bgr(Color.Green));

            processedImages.Add(foregroundBlobs);
            processedImages.Add(motionImage);
            processedImages.Add(segmentMask);
            return processedImages;
        }

        private static void DrawMotion(IInputOutputArray image, Rectangle motionRegion, double angle, Bgr color)
        {
            //CvInvoke.Rectangle(image, motionRegion, new MCvScalar(255, 255, 0));
            float circleRadius = (motionRegion.Width + motionRegion.Height) >> 2;
            Point center = new Point(motionRegion.X + (motionRegion.Width >> 1), motionRegion.Y + (motionRegion.Height >> 1));

            CircleF circle = new CircleF(
               center,
               circleRadius);

            int xDirection = (int)(Math.Cos(angle * (Math.PI / 180.0)) * circleRadius);
            int yDirection = (int)(Math.Sin(angle * (Math.PI / 180.0)) * circleRadius);
            Point pointOnCircle = new Point(
                center.X + xDirection,
                center.Y - yDirection);
            LineSegment2D line = new LineSegment2D(center, pointOnCircle);
            CvInvoke.Circle(image, Point.Round(circle.Center), (int)circle.Radius, color.MCvScalar);
            CvInvoke.Line(image, line.P1, line.P2, color.MCvScalar);
        }
    }
}
