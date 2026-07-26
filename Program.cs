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
            string[] defaultArgs = { "-i", @"E:\OneDrive\Pulpit\fb activity\2025.07.21  MosaicWitch\tesEdit\img0.png", "-minSaturation", "0.0", "-maxSaturation", "2.0", "-minExpo", "0", "-maxExpo", "2.0", "-minContrast", "-2.0", "-maxContrast", "2.0", "-minTemperature", "-100", "-maxTemperature", "100", "-mintinta", "-100", "-maxtinta", "100" };
            if (args.Length > 0)
            {
                defaultArgs[1] = args[0];
            }
            Dictionary<string, string> arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            args = defaultArgs;
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
            if (path != null)
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

                    Bitmap bitmapOrginal = new Bitmap(path);
                    Bitmap bitmap = new Bitmap(bitmapOrginal, Resolution.ScaleResolution(bitmapOrginal.Size, 8294400));
                    Application.Run(new Form1((x) =>
                    {
                        new ContrastBrightnessSaturation(x, bitmap, bitmapOrginal,
                        VD("minSaturation", 0),
                        VD("maxSaturation", 2),
                        VD("minExpo", 0),
                        VD("maxExpo", 3),
                        VD("minContrast", 0),
                        VD("maxContrast", 2),
                        (int)VD("minTemperature", -100),
                        (int)VD("maxTemperature", 100),
                        (int)VD("mintinta", -100),
                        (int)VD("maxtinta", 100)
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