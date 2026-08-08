using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Graphicdll.GraphicProcesing;

namespace GraphicBasicWindows.Graphicdll
{
    internal class GraphicProcesingForConstImage
    {

        public static unsafe void ComputeAvg(Bitmap Obraz, BitmapData bp, out byte[] r, out byte[] g, out byte[] b, out byte[] avg, ref long j)
        {
            r = new byte[(Obraz.Width * Obraz.Height)];
            g = new byte[(Obraz.Width * Obraz.Height)];
            b = new byte[(Obraz.Width * Obraz.Height)];
            avg = new byte[(Obraz.Width * Obraz.Height)];
            int globalindex = 0;
            for (int y = 0; y < Obraz.Height; y++)
            {

                rgb* kr = (rgb*)((byte*)(bp.Scan0 + y * bp.Stride));
                for (int x = 0; x < Obraz.Width; x++, kr++, globalindex++)
                {
                    int sum = 0;
                    sum += r[globalindex] = (*kr).r;
                    sum += g[globalindex] = (*kr).g;
                    sum += b[globalindex] = (*kr).b;
                    avg[globalindex] = (byte)(sum / 3);
                    j += sum;
                }
            }
            j /= (Obraz.Width * Obraz.Height * 3);
        }
    }
}
