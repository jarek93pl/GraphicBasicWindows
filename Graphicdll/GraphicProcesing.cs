using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace Graphicdll
{
    public static class GraphicProcesing
    {

        public static unsafe void BasicEditing4Parameter(Bitmap Obraz, float exposytion, float saturaion, float contrast, int temperature, int tinta)
        {
            long avg = 0;
            BitmapData bp = Obraz.LockBits(new Rectangle(0, 0, Obraz.Width, Obraz.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            ComputeAvg(Obraz, bp, ref avg);
            for (int y = 0; y < Obraz.Height; y++)
            {

                rgb* kr = (rgb*)((byte*)(bp.Scan0 + y * bp.Stride));
                for (int x = 0; x < Obraz.Width; x++, kr++)
                {
                    ComputePixel(exposytion, saturaion, contrast, temperature, tinta, avg, kr, kr);

                }
            }


            Obraz.UnlockBits(bp);

        }
        public static unsafe void MultiThreadEditing4Parameter(Bitmap Obraz, float exposytion, float saturaion, float contrast, int temperature, int tinta)
        {
            long avg = 0;
            BitmapData bp = Obraz.LockBits(new Rectangle(0, 0, Obraz.Width, Obraz.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            ComputeAvg(Obraz, bp, ref avg);
            int width = Obraz.Width;
            int height = Obraz.Height;
            Enumerable.Range(0, height).AsParallel().ForAll(y =>
            {

                rgb* kr = (rgb*)((byte*)(bp.Scan0 + y * bp.Stride));
                rgb* inKr = (rgb*)((byte*)(bp.Scan0 + y * bp.Stride));
                for (int x = 0; x < width; x++, kr++, inKr++)
                {
                    ComputePixel(exposytion, saturaion, contrast, temperature, tinta, avg, inKr, kr);

                }
            });

            Obraz.UnlockBits(bp);

        }
        public static unsafe void ComputePixel(float exposytion, float saturaion, float contrast, int temperature, int tinta, long avg, rgb* kr, rgb* outpix)
        {
            int avgPixel = 0;
            avgPixel = (*kr).r;
            avgPixel += (*kr).g;
            avgPixel += (*kr).b;
            avgPixel /= 3;
            float sumFromContrast = (avgPixel - avg) * contrast;
            float r = (*kr).r;
            float g = (*kr).g;
            float b = (*kr).b;
            r += -((*kr).r - avgPixel) + ((*kr).r - avgPixel) * saturaion;
            g += -((*kr).g - avgPixel) + ((*kr).g - avgPixel) * saturaion;
            b += -((*kr).b - avgPixel) + ((*kr).b - avgPixel) * saturaion;
            r += sumFromContrast - temperature - tinta;
            g += sumFromContrast + (tinta << 1);
            b += sumFromContrast + temperature - tinta;
            r *= exposytion;
            g *= exposytion;
            b *= exposytion;
            if (r < 0) { r = 0; }
            if (g < 0) { g = 0; }
            if (b < 0) { b = 0; }
            if (r > 255) { r = 255; }
            if (g > 255) { g = 255; }
            if (b > 255) { b = 255; }
            (*outpix).r = (byte)r;
            (*outpix).g = (byte)g;
            (*outpix).b = (byte)b;
        }
        public static long ImageDifrence(string path1, string path2)
        {
            Bitmap bitmap1 = (Bitmap)Bitmap.FromFile(path1);
            Bitmap bitmap2 = (Bitmap)Bitmap.FromFile(path2);
            long diffrence = 0;
            unsafe
            {
                var data1 = bitmap1.LockBits(new Rectangle(0, 0, bitmap1.Width, bitmap1.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                var data2 = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                for (int y = 0; y < bitmap1.Height; y++)
                {
                    rgb* pixel1 = (rgb*)(data1.Scan0 + data1.Stride * y);
                    rgb* pixel2 = (rgb*)(data2.Scan0 + data2.Stride * y);
                    for (int x = 0; x < bitmap1.Width; x++, pixel1++, pixel2++)
                    {
                        diffrence += Math.Abs(pixel1->r - pixel2->r);
                        diffrence += Math.Abs(pixel1->g - pixel2->g);
                        diffrence += Math.Abs(pixel1->b - pixel2->b);
                    }
                }
                bitmap1.UnlockBits(data1);
                bitmap2.UnlockBits(data2);
            }
            bitmap1.Dispose();
            bitmap2.Dispose();
            return diffrence;
        }
        public static Bitmap DrawFramesGrid(Bitmap source, int x, int y, int lineThickness, float minFromMozaic)
        {
            Bitmap returned = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(returned);
            g.Clear(Color.Transparent);

            int widthrect = (source.Width / x);
            int heightrect = (source.Height / y);

            for (int i = 0; i < source.Width; i++)
            {
                DrawLine((widthrect * i), 0, (widthrect * i), y * heightrect);
            }

            for (int i = 0; i < y; i++)
            {
                DrawLine(0, (heightrect * i), widthrect * x, (heightrect * i));
            }
            return returned;
            void DrawLine(int startX, int startY, int endX, int endY)
            {
                int difrentX = endX - startX;
                int difrentY = endY - startY;
                Vector2 startVecotr = new Vector2(startX, startY);
                Vector2 endVecotr = new Vector2(endX, endY);
                if (difrentX < difrentY)
                {
                    float curentDif = difrentY;
                    for (int i = 0; i < curentDif; i++)
                    {
                        Vector2 centerPlacePixe = new Vector2(startX, startY + i);
                        for (int j = -lineThickness; j < lineThickness; j++)
                        {
                            Vector2 toDraw = centerPlacePixe;
                            toDraw.X += j;
                            DrawPixel(toDraw, j);
                        }

                    }
                }
                else
                {
                    float curentDif = difrentX;
                    for (int i = 0; i < curentDif; i++)
                    {
                        Vector2 centerPlacePixe = new Vector2(startX + i, startY);
                        for (int j = -lineThickness; j < lineThickness; j++)
                        {
                            Vector2 toDraw = centerPlacePixe;
                            toDraw.Y += j;
                            DrawPixel(toDraw, j);
                        }

                    }
                }
            }
            void DrawPixel(Vector2 place, int lenghtFromCenter)
            {
                float alpha = 1 - (Math.Abs(lenghtFromCenter) / (float)lineThickness);
                int x = (int)place.X;
                int y = (int)place.Y;
                if (alpha < minFromMozaic)
                {
                    alpha = minFromMozaic;
                }
                if (y >= 0 && x >= 0 && x < source.Width && y < source.Height)
                {
                    Color cSource = source.GetPixel(x, y);
                    int r = (int)cSource.R;
                    int g = (int)cSource.G;
                    int b = (int)cSource.B;
                    int apha = (int)(alpha * 255);
                    Color ck = returned.GetPixel(x, y);
                    if (ck.A < apha)
                    {
                        returned.SetPixel(x, y, Color.FromArgb(apha, r, g, b));
                    }
                }
            }

        }
        public static unsafe void ComputeAvg(Bitmap Obraz, BitmapData bp, ref long j)
        {
            for (int y = 0; y < Obraz.Height; y++)
            {

                rgb* kr = (rgb*)((byte*)(bp.Scan0 + y * bp.Stride));
                for (int x = 0; x < Obraz.Width; x++, kr++)
                {
                    j += (*kr).r;
                    j += (*kr).g;
                    j += (*kr).b;
                }
            }
            j /= (Obraz.Width * Obraz.Height * 3);
        }

        public struct rgb
        {
            public byte r, g, b;
        }
    }
}
