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
            string[] defaultArgs = { "-i", @"E:\OneDrive\Pulpit\fb activity\2025.07.21  MosaicWitch\tesEdit\img0.png", "-minSaturation", "0.0", "-maxSaturation", "2.0", "-minExpo", "-2.0", "-maxExpo", "2.0", "-minContrast", "0.0", "-maxContrast", "2.0" };
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
            Bitmap bitmap = new Bitmap(arguments.TryGetValue("i", out string inputImage) ? inputImage : "img0.png");
            Application.Run(new Form1((x) =>
            {
                new ContrastBrightnessSaturation(x, bitmap,
                VD("minSaturation", 0),
                VD("maxSaturation", 2),
                VD("minExpo", 0),
                VD("maxExpo", 3),
                VD("minContrast", 0),
                VD("maxContrast", 2)
                ).Show();
            }));
        }
    }
}