using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphicdll
{
    public static class Resolution
    {
        public static Size ScaleResolution(Size size, int outNumberPixel)
        {
            double numberPixel = size.Width * size.Height;
            numberPixel /= outNumberPixel;
            numberPixel = Math.Sqrt(numberPixel);
            return new Size((int)(size.Width / numberPixel), (int)(size.Height / numberPixel));
        }
        public static bool ScaleResolutionOnlyIfHigher(Size size, int outNumberPixel, out Size scal)
        {
            int numberOfPixel = size.Width * size.Height;
            if (numberOfPixel > outNumberPixel)
            {
                scal = ScaleResolution(size, outNumberPixel);
                return true;
            }
            else
            {
                scal = size;
                return false;
            }
        }
    }
}
