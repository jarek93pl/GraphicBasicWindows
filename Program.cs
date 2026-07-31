using Graphicdll;

namespace GraphicBasicWindows
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
           
            Dictionary<string, string> arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    args[i] = args[i].Substring(1);
                }
                arguments.Add(args[i], i + 1 < args.Length ? args[++i] : string.Empty);
            }
            float VD(string key, float defaultValue = 1.0f)
            {
                if (arguments.TryGetValue(key, out string value) && float.TryParse(value, out float result))
                {
                    return result;
                }
                return defaultValue;
            }
            string path = null;
            if (arguments.TryGetValue("i", out string inputImage))
            {
                path = inputImage;
            }
            if (path == null)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    path = dialog.FileName;
                }

            }
            if (path != null)
            {
                try
                {
                    Bitmap bitmap;
                    Bitmap bitmapOrginal = new Bitmap(path);
                    if (Graphicdll.Resolution.ScaleResolutionOnlyIfHigher(bitmapOrginal.Size, 8294400, out var size))
                    {
                        Console.WriteLine($"scaled from {bitmapOrginal.Size} to {size}");
                        bitmap = new Bitmap(bitmapOrginal, size);

                    }
                    else
                    {
                        bitmap = bitmapOrginal;
                    }
                    Application.Run(new Form1((x) =>
                    {
                        new ContrastBrightnessSaturation(x, bitmap, bitmapOrginal,
                        VD("minSaturation", 0),
                        VD("maxSaturation", 3),
                        VD("minExpo", 0),
                        VD("maxExpo", 3),
                        VD("minContrast", -1),
                        VD("maxContrast", 3),
                        (int)VD("minTemperature", -256),
                        (int)VD("maxTemperature", 256),
                        (int)VD("mintinta", -256),
                        (int)VD("maxtinta", 256)
                        ).Show();
                    }, new Bitmap(bitmap)));

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("orpably too fast close app");
                }
            }
        }
    }
}