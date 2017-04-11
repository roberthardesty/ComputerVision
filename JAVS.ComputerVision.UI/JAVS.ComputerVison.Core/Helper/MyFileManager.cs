using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAVS.ComputerVison.Core.Helper
{
    public class MyFileManager
    {
        public static bool SaveImage(Bitmap img, string path)
        {
            try
            {
                using (FileStream file = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)+"\\Pictures\\test-"+path+".png", FileMode.Create))
                {
                    img.Save(file, System.Drawing.Imaging.ImageFormat.Png);
                }
                return true;
            }
            catch(Exception e)
            {
                var test = e;
                return false;
            }            
        }
    }
}
