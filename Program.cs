using System;

namespace TessellationDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var opts = CliOptions.Parse(args);

            if (opts.DumpImage)
            {
                RunHeadless(opts);
                return;
            }

            PrintInteractiveHelp();

            try
            {
                using var window = new TessellationWindow();
                window.Run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to start window: {ex.Message}");
                Console.Error.WriteLine("If you have no display, try: --dump-image output.png");
                Environment.Exit(1);
            }
        }

        private static void RunHeadless(CliOptions opts)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  OpenGL Tessellation Demo - Image Dump");
            Console.WriteLine("===========================================");
            Console.WriteLine($"  Domain    : {opts.Domain}");
            Console.WriteLine($"  Spacing   : {opts.Spacing}");
            Console.WriteLine($"  Tess Level: {opts.TessLevel}");
            Console.WriteLine($"  Wireframe : {opts.Wireframe}");
            Console.WriteLine($"  Resolution: {opts.Width}x{opts.Height}");
            Console.WriteLine($"  Output    : {opts.OutputPath}");
            Console.WriteLine();

            try
            {
                using var renderer = new HeadlessRenderer(opts);
                renderer.Run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Headless rendering failed: {ex.Message}");
                Console.Error.WriteLine();
                Console.Error.WriteLine("This usually means OpenGL 4.1 is not available.");
                Console.Error.WriteLine("On Linux, try enabling software rendering:");
                Console.Error.WriteLine("  export LIBGL_ALWAYS_SOFTWARE=1");
                Console.Error.WriteLine("  xvfb-run -s '-screen 0 1280x720x24' dotnet run -- --dump-image out.png");
                Environment.Exit(1);
            }
        }

        private static void PrintInteractiveHelp()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  OpenGL 4.1 Tessellation Demo - C#");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine("Controls:");
            Console.WriteLine("  WASD + Mouse    - Pan camera");
            Console.WriteLine("  Space/Shift     - Move up/down");
            Console.WriteLine("  Mouse Wheel     - Zoom in/out");
            Console.WriteLine("  ESC             - Toggle mouse capture");
            Console.WriteLine();
            Console.WriteLine("Tessellation Controls:");
            Console.WriteLine("  1/2/3           - Domain (Triangles/Quads/Isolines)");
            Console.WriteLine("  Q/E/R           - Spacing (Equal/FracEven/FracOdd)");
            Console.WriteLine("  M               - Toggle wireframe mode");
            Console.WriteLine("  +/-             - Increase/decrease LOD level");
            Console.WriteLine("  H               - Toggle help");
            Console.WriteLine();
            Console.WriteLine("Run with --help for CLI image-dump options.");
            Console.WriteLine();
            Console.WriteLine("Starting application...");
            Console.WriteLine();
        }
    }
}
