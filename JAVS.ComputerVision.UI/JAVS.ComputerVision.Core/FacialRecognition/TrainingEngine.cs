using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using JAVS.ComputerVision.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAVS.ComputerVision.Core.FacialRecognition
{
    public class TrainingEngine
    {
        private EigenFaceRecognizer _faceRecognizer = new EigenFaceRecognizer();


        public bool Train(byte[][] faces, int[] labels)
        {
            Image<Gray, byte>[] resizedFaces = new Image<Gray, byte>[faces.Length];
            for(int i = 0; i < faces.Length; i++)
            {
                resizedFaces[i] = StreamConverter.ByteToImageResize(faces[i]).Clone();
            }
            _faceRecognizer.Train(resizedFaces, labels);
            _faceRecognizer.Save(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)+"\\Documents\\test-recognizer.yaml");
            return true;
        }
    }
}
