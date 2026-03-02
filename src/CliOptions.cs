using System;

namespace TessellationDemo
{
    public class CliOptions
    {
        public bool DumpImage { get; init; }
        public string OutputPath { get; init; } = "tessellation_output.png";
        public float TessLevel { get; init; } = 4.0f;
        public string Domain { get; init; } = "triangles";
        public string Spacing { get; init; } = "equal";
        public int Width { get; init; } = 1280;
        public int Height { get; init; } = 720;
        public bool Wireframe { get; init; }

        public static CliOptions Parse(string[] args)
        {
            bool dumpImage = false;
            string outputPath = "tessellation_output.png";
            float tessLevel = 4.0f;
            string domain = "triangles";
            string spacing = "equal";
            int width = 1280;
            int height = 720;
            bool wireframe = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--dump-image":
                        dumpImage = true;
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                            outputPath = args[++i];
                        break;

                    case "--tess-level":
                        if (i + 1 < args.Length && float.TryParse(args[++i], out float tl))
                            tessLevel = Math.Clamp(tl, 1f, 64f);
                        break;

                    case "--domain":
                        if (i + 1 < args.Length)
                            domain = args[++i].ToLower();
                        break;

                    case "--spacing":
                        if (i + 1 < args.Length)
                            spacing = args[++i].ToLower();
                        break;

                    case "--width":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int w))
                            width = Math.Max(64, w);
                        break;

                    case "--height":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int h))
                            height = Math.Max(64, h);
                        break;

                    case "--wireframe":
                        wireframe = true;
                        break;

                    case "--help":
                    case "-h":
                        PrintHelp();
                        Environment.Exit(0);
                        break;
                }
            }

            return new CliOptions
            {
                DumpImage = dumpImage,
                OutputPath = outputPath,
                TessLevel = tessLevel,
                Domain = domain,
                Spacing = spacing,
                Width = width,
                Height = height,
                Wireframe = wireframe,
            };
        }

        public static void PrintHelp()
        {
            Console.WriteLine("Usage: TessellationDemo [options]");
            Console.WriteLine();
            Console.WriteLine("Interactive Mode (default):");
            Console.WriteLine("  dotnet run                           Launch the interactive OpenGL window");
            Console.WriteLine();
            Console.WriteLine("Image Dump Mode:");
            Console.WriteLine("  --dump-image [path]   Render one frame to an image file and exit");
            Console.WriteLine("                        (default output: tessellation_output.png)");
            Console.WriteLine();
            Console.WriteLine("Rendering Options (apply to both modes when dumping):");
            Console.WriteLine("  --tess-level <n>      Tessellation level 1–64 (default: 4)");
            Console.WriteLine("  --domain <type>       triangles | quads | isolines (default: triangles)");
            Console.WriteLine("  --spacing <type>      equal | fraceven | fracodd  (default: equal)");
            Console.WriteLine("  --wireframe           Enable wireframe overlay");
            Console.WriteLine("  --width  <pixels>     Output image width  (default: 1280)");
            Console.WriteLine("  --height <pixels>     Output image height (default: 720)");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run -- --dump-image out.png --tess-level 16 --domain triangles");
            Console.WriteLine("  dotnet run -- --dump-image wire.png --wireframe --tess-level 8 --domain quads");
        }
    }
}
