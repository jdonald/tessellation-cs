using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace TessellationDemo
{
    /// <summary>
    /// Renders a single frame off-screen to an image file and exits.
    /// Requires an OpenGL 4.1-capable context (real GPU or Mesa llvmpipe).
    /// </summary>
    public class HeadlessRenderer : GameWindow
    {
        private readonly CliOptions _options;

        private Camera _camera;
        private int _vao, _vbo, _ebo;
        private int _fbo, _colorRbo, _depthRbo;
        private Dictionary<string, ShaderProgram> _shaderPrograms;
        private ShaderProgram? _currentShader;
        private Geometry? _geometry;
        private bool _rendered;

        public HeadlessRenderer(CliOptions options)
            : base(
                GameWindowSettings.Default,
                new NativeWindowSettings
                {
                    ClientSize = new Vector2i(options.Width, options.Height),
                    Title = "TessellationDemo - headless",
                    APIVersion = new Version(4, 1),
                    Profile = ContextProfile.Core,
                    Flags = ContextFlags.ForwardCompatible,
                    StartVisible = false,
                    StartFocused = false,
                    WindowBorder = WindowBorder.Hidden,
                })
        {
            _options = options;
            _camera = new Camera(new Vector3(0, 1.5f, 5));
            _shaderPrograms = new Dictionary<string, ShaderProgram>();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            Console.WriteLine($"OpenGL Version : {GL.GetString(StringName.Version)}");
            Console.WriteLine($"GLSL Version   : {GL.GetString(StringName.ShadingLanguageVersion)}");

            GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            // ── Offscreen framebuffer ────────────────────────────────────
            _fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

            _colorRbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _colorRbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
                RenderbufferStorage.Rgba8, _options.Width, _options.Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                RenderbufferTarget.Renderbuffer, _colorRbo);

            _depthRbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthRbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
                RenderbufferStorage.DepthComponent24, _options.Width, _options.Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                RenderbufferTarget.Renderbuffer, _depthRbo);

            var fbStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (fbStatus != FramebufferErrorCode.FramebufferComplete)
                throw new Exception($"Framebuffer incomplete: {fbStatus}");

            // ── Shaders ──────────────────────────────────────────────────
            LoadShaders();

            // ── Geometry ─────────────────────────────────────────────────
            var humanoid = Geometry.CreateBlockyHumanoid();
            var tree = Geometry.CreateBlockyTree();

            var vertices = new List<float>();
            vertices.AddRange(humanoid.Vertices);
            vertices.AddRange(tree.Vertices);

            var indices = new List<uint>();
            indices.AddRange(humanoid.Indices);
            uint offset = (uint)(humanoid.Vertices.Length / 6);
            foreach (var idx in tree.Indices)
                indices.Add(idx + offset);

            _geometry = new Geometry(vertices.ToArray(), indices.ToArray());

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                _geometry.Vertices.Length * sizeof(float),
                _geometry.Vertices, BufferUsageHint.StaticDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                _geometry.Indices.Length * sizeof(uint),
                _geometry.Indices, BufferUsageHint.StaticDraw);

            // Position attribute (location=0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Color attribute (location=1)
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            SelectShader();
        }

        private void LoadShaders()
        {
            string[] domains = { "triangles", "quads", "isolines" };
            string[] spacings = { "equal_spacing", "fractional_even_spacing", "fractional_odd_spacing" };
            string[] spacingSuffixes = { "", "_fraceven", "_fracodd" };

            foreach (var domain in domains)
            {
                string tcsFile = domain switch
                {
                    "triangles" => "Shaders/tess_control.glsl",
                    "quads" => "Shaders/tess_control_quad.glsl",
                    _ => "Shaders/tess_control_isoline.glsl",
                };

                for (int i = 0; i < spacings.Length; i++)
                {
                    string key = $"{domain}_{spacings[i]}";
                    string tesFile = $"Shaders/tess_eval_{domain}{spacingSuffixes[i]}.glsl";
                    try
                    {
                        _shaderPrograms[key] = new ShaderProgram(
                            "Shaders/vertex.glsl", tcsFile, tesFile, "Shaders/fragment.glsl");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: could not load shader {key}: {ex.Message}");
                    }
                }
            }
        }

        private void SelectShader()
        {
            string spacingKey = _options.Spacing switch
            {
                "fraceven" => "fractional_even_spacing",
                "fracodd" => "fractional_odd_spacing",
                _ => "equal_spacing",
            };

            string domainKey = _options.Domain switch
            {
                "quads" => "quads",
                "isolines" => "isolines",
                _ => "triangles",
            };

            string key = $"{domainKey}_{spacingKey}";
            if (_shaderPrograms.TryGetValue(key, out var shader))
                _currentShader = shader;
            else
                Console.WriteLine($"Warning: shader {key} not found, rendering may fail");
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if (_rendered) return;
            _rendered = true;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            GL.Viewport(0, 0, _options.Width, _options.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_currentShader != null && _geometry != null)
            {
                _currentShader.Use();

                var view = _camera.GetViewMatrix();
                var projection = _camera.GetProjectionMatrix((float)_options.Width / _options.Height);
                var mvp = view * projection;

                _currentShader.SetMatrix4("uModelViewProjection", mvp);
                _currentShader.SetFloat("uTessLevel", _options.TessLevel);
                _currentShader.SetBool("uWireframeMode", _options.Wireframe);
                _currentShader.SetVector3("uWireframeColor", new Vector3(1f, 1f, 1f));

                int patchSize = _options.Domain switch
                {
                    "quads" => 4,
                    "isolines" => 4,
                    _ => 3,
                };
                GL.PatchParameter(PatchParameterInt.PatchVertices, patchSize);

                GL.PolygonMode(MaterialFace.FrontAndBack,
                    _options.Wireframe ? PolygonMode.Line : PolygonMode.Fill);

                GL.BindVertexArray(_vao);
                GL.DrawElements(PrimitiveType.Patches,
                    _geometry.Indices.Length, DrawElementsType.UnsignedInt, 0);
            }

            // Read pixels from FBO (OpenGL origin is bottom-left)
            int w = _options.Width, h = _options.Height;
            byte[] pixels = new byte[w * h * 4];
            GL.ReadPixels(0, 0, w, h, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            // Flip vertically (OpenGL Y=0 is bottom; image Y=0 is top)
            FlipVertical(pixels, w, h);

            SavePng(pixels, w, h, _options.OutputPath);
            Console.WriteLine($"Saved: {Path.GetFullPath(_options.OutputPath)}");

            Close();
        }

        private static void FlipVertical(byte[] data, int width, int height)
        {
            int rowBytes = width * 4;
            byte[] tmp = new byte[rowBytes];
            for (int y = 0; y < height / 2; y++)
            {
                int top = y * rowBytes;
                int bot = (height - 1 - y) * rowBytes;
                System.Buffer.BlockCopy(data, top, tmp, 0, rowBytes);
                System.Buffer.BlockCopy(data, bot, data, top, rowBytes);
                System.Buffer.BlockCopy(tmp, 0, data, bot, rowBytes);
            }
        }

        private static void SavePng(byte[] rgba, int width, int height, string outputPath)
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            using var img = SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba32>(rgba, width, height);
            using var fs = System.IO.File.Create(outputPath);
            img.Save(fs, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteFramebuffer(_fbo);
            GL.DeleteRenderbuffer(_colorRbo);
            GL.DeleteRenderbuffer(_depthRbo);
            foreach (var s in _shaderPrograms.Values)
                s.Dispose();
        }
    }
}
