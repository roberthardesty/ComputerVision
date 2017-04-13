using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace JAVS.ComputerVision.Core.Helper
{
    public class StreamConverter
    {
        public static byte[] ImageToByte(Bitmap img)
        {
            using (var stream = new MemoryStream())
            {
                var copy = new Bitmap(img);
                copy.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static Image<Gray,byte>  ByteToImage(byte[] bytes)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Count());
                return new Image<Gray, byte>(new Bitmap(stream));
            }
        }

        public static BitmapSource ByteToBitmapSource(byte[] bytes)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Count());

                using (System.Drawing.Bitmap source = new Bitmap(stream))
                {
                    IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                    BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        ptr,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                    DeleteObject(ptr); //release the HBitmap
                    return bs;
                }
            }
        }

        public static Bitmap ByteToBitmap(byte[] bytes)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Count());

                Bitmap source = new Bitmap(stream);
                return source;
            }
        }

        public static Image<Gray, byte> ByteToImageResize(byte[] bytes)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Count());

                Image<Gray, byte> source = new Image<Gray, byte>(new Bitmap(stream));
                
                source.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);
                return source;
                
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
    }
}
