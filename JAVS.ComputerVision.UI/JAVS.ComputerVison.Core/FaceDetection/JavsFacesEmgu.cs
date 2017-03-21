using JAVS.ComputerVison.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV.Structure;
#if !(__IOS__ || NETFX_CORE)
using Emgu.CV.Cuda;
#endif

namespace JAVS.ComputerVison.Core.FaceDetection
{
    public class JavsFacesEmgu : IDetect
    {
        private Stopwatch watch;

        private long detectionTime;

        private string faceFileName = "";

        private string eyeFileName = "";
        public int ProcessCount()
        {
            return 1;
        }
        public List<IImage> ProcessFrame(IImage original)
        {
            List<Rectangle> faces = new List<Rectangle>();
            List<Rectangle> eyes = new List<Rectangle>();
            using (InputArray OriginalImage = original.GetInputArray())
            {
                //Cuda Conditionals
#if !(__IOS__ || NETFX_CORE)
                if (OriginalImage.Kind == InputArray.Type.CudaGpuMat && CudaInvoke.HasCuda)
                {
                    return new List<IImage> { CudaDetect(original, faces, eyes) };
                }
                else
#endif
                {
                    return new List<IImage> { Detect(original, faces, eyes) };
                }
            }
        }

        private IImage Detect(IImage original, List<Rectangle> faces, List<Rectangle> eyes)
        {
        //Read the HaarCascade objects
        using (CascadeClassifier face = new CascadeClassifier(faceFileName))
        using (CascadeClassifier eye = new CascadeClassifier(eyeFileName))
        {
            watch = Stopwatch.StartNew();

            using (UMat ugray = new UMat())
            {
                CvInvoke.CvtColor(original, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                //normalizes brightness and increases contrast of the image
                CvInvoke.EqualizeHist(ugray, ugray);

                //Detect the faces  from the gray scale image and store the locations as rectangle
                //The first dimensional is the channel
                //The second dimension is the index of the rectangle in the specific channel
                Rectangle[] facesDetected = face.DetectMultiScale(
                    ugray,
                    1.1,
                    10,
                    new Size(20, 20));

                faces.AddRange(facesDetected);

                foreach (Rectangle f in facesDetected)
                {
                    //Get the region of interest on the faces
                    using (UMat faceRegion = new UMat(ugray, f))
                    {
                        Rectangle[] eyesDetected = eye.DetectMultiScale(
                            faceRegion,
                            1.1,
                            10,
                            new Size(20, 20));

                        foreach (Rectangle e in eyesDetected)
                        {
                            Rectangle eyeRect = e;
                            eyeRect.Offset(f.X, f.Y);
                            eyes.Add(eyeRect);
                        }
                    }
                }
            }
            watch.Stop();
                return AttachFacesToImage(original, faces, eyes);
        }
    }

        private IImage CudaDetect(IImage original, List<Rectangle> faces, List<Rectangle> eyes)
        {
                using (CudaCascadeClassifier face = new CudaCascadeClassifier(faceFileName))
                using (CudaCascadeClassifier eye = new CudaCascadeClassifier(eyeFileName))
                {
                    face.ScaleFactor = 1.1;
                    face.MinNeighbors = 10;
                    face.MinObjectSize = Size.Empty;
                    eye.ScaleFactor = 1.1;
                    eye.MinNeighbors = 10;
                    eye.MinObjectSize = Size.Empty;
                    watch = Stopwatch.StartNew();
                    using (CudaImage<Bgr, Byte> gpuImage = new CudaImage<Bgr, byte>(original))
                    using (CudaImage<Gray, Byte> gpuGray = gpuImage.Convert<Gray, Byte>())
                    using (GpuMat region = new GpuMat())
                    {
                        face.DetectMultiScale(gpuGray, region);
                        Rectangle[] faceRegion = face.Convert(region);
                        faces.AddRange(faceRegion);
                        foreach (Rectangle f in faceRegion)
                        {
                            using (CudaImage<Gray, Byte> faceImg = gpuGray.GetSubRect(f))
                            {
                                //For some reason a clone is required.
                                //Might be a bug of CudaCascadeClassifier in opencv
                                using (CudaImage<Gray, Byte> clone = faceImg.Clone(null))
                                using (GpuMat eyeRegionMat = new GpuMat())
                                {
                                    eye.DetectMultiScale(clone, eyeRegionMat);
                                    Rectangle[] eyeRegion = eye.Convert(eyeRegionMat);
                                    foreach (Rectangle e in eyeRegion)
                                    {
                                        Rectangle eyeRect = e;
                                        eyeRect.Offset(f.X, f.Y);
                                        eyes.Add(eyeRect);
                                    }
                                }
                            }
                        }
                    }
                    watch.Stop();
                }
                return AttachFacesToImage(original, faces, eyes);
                //return eyes;
        }

        public IImage AttachFacesToImage(IImage image, List<Rectangle> faces, List<Rectangle> eyes)
        {
            foreach (Rectangle face in faces)
                CvInvoke.Rectangle(image, face, new Bgr(Color.Red).MCvScalar, 2);
            foreach (Rectangle eye in eyes)
                CvInvoke.Rectangle(image, eye, new Bgr(Color.Blue).MCvScalar, 2);

            return image;
        }
    }
}


