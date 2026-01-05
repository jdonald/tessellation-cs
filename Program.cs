using System;

namespace TessellationDemo
{
    class Program
    {
        static void Main(string[] args)
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
            Console.WriteLine("Starting application...");
            Console.WriteLine();

            using (var window = new TessellationWindow())
            {
                window.Run();
            }
        }
    }
}
