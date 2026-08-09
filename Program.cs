using Graphicdll;
using System.Globalization;
using System.Text.RegularExpressions;

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
            var culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Dictionary<string, string> arguments;
            string path;
            LoadDictionary(args, out arguments, out path);
            path = LoadPath(arguments, path);
            bool iSEco = LoadISecoValue(arguments);
            float LoadFloatFromParameter(string key, float defaultValue = 1.0f)
            {
                if (arguments.TryGetValue(key, out string value) && float.TryParse(value, out float result))
                {
                    return result;
                }
                return defaultValue;
            }
            if (path != null)
            {
                try
                {
                    Bitmap bitmap;
                    Bitmap bitmapOrginal = new Bitmap(path);
                    Size size;
                    if (LoadResolution(bitmapOrginal, out size, iSEco))
                    {
                        Console.WriteLine($"scaled from {bitmapOrginal.Size} to {size}");
                        bitmap = new Bitmap(bitmapOrginal, size);

                    }
                    else
                    {
                        bitmap = (Bitmap)bitmapOrginal.Clone();
                    }
                    Application.Run(new Form1((x) =>
                    {
                        new ContrastBrightnessSaturation(x, bitmap, bitmapOrginal,
                        LoadFloatFromParameter("minSaturation", 0),
                        LoadFloatFromParameter("maxSaturation", 3),
                        LoadFloatFromParameter("minExpo", 0),
                        LoadFloatFromParameter("maxExpo", 3),
                        LoadFloatFromParameter("minContrast", -1),
                        LoadFloatFromParameter("maxContrast", 3),
                        (int)LoadFloatFromParameter("minTemperature", -256),
                        (int)LoadFloatFromParameter("maxTemperature", 256),
                        (int)LoadFloatFromParameter("mintinta", -256),
                        (int)LoadFloatFromParameter("maxtinta", 256)
                        ).Show();
                    }, new Bitmap(bitmap)));

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("orpably too fast close app");
                }
            }
        }

        private static bool LoadISecoValue(Dictionary<string, string> arguments)
        {
            bool iSEco = false;
            if (arguments.TryGetValue("eco", out string value))
            {
                var tab = new string[] { "true", "eco" };
                iSEco = tab.Contains(value);
            }

            return iSEco;
        }

        private static bool LoadResolution(Bitmap bitmapOrginal, out Size size, bool isEco)
        {
            int maxPixel = isEco ? 2_073_600 : 82_944_000;
            return Resolution.ScaleResolutionOnlyIfHigher(bitmapOrginal.Size, maxPixel, out size);
        }

        private static void LoadDictionary(string[] args, out Dictionary<string, string> arguments, out string path)
        {
            arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    args[i] = args[i].Substring(1);
                }
                arguments.Add(args[i], i + 1 < args.Length ? args[++i] : string.Empty);
            }
            path = null;
        }

        private static string LoadPath(Dictionary<string, string> arguments, string path)
        {
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

            return path;
        }
    }
}