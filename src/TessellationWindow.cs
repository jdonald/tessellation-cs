using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TessellationDemo
{
    public class TessellationWindow : GameWindow
    {
        private Camera _camera;
        private int _vao, _vbo, _ebo;
        private Dictionary<string, ShaderProgram> _shaderPrograms;
        private ShaderProgram? _currentShader;
        private Geometry _geometry;

        private bool _firstMove = true;
        private Vector2 _lastMousePos;
        private bool _mouseCaptured = true;

        // Tessellation parameters
        private enum TessDomain { Triangles, Quads, Isolines }
        private enum TessSpacing { Equal, FractionalEven, FractionalOdd, Integer }
        private enum TessWinding { CW, CCW }
        private enum RenderMode { Color, Wireframe }

        private TessDomain _currentDomain = TessDomain.Triangles;
        private TessSpacing _currentSpacing = TessSpacing.Equal;
        private TessWinding _currentWinding = TessWinding.CCW;
        private RenderMode _renderMode = RenderMode.Color;
        private float _tessLevel = 4.0f;

        private bool _showHelp = true;

        public TessellationWindow()
            : base(GameWindowSettings.Default,
                new NativeWindowSettings()
                {
                    Size = new Vector2i(1280, 720),
                    Title = "OpenGL Tessellation Demo - C#",
                    APIVersion = new Version(4, 1),
                    Profile = ContextProfile.Core,
                    Flags = ContextFlags.ForwardCompatible
                })
        {
            _camera = new Camera(new Vector3(0, 1.5f, 5));
            _shaderPrograms = new Dictionary<string, ShaderProgram>();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ProgramPointSize);

            // Load all shader combinations
            LoadShaders();

            // Create geometry
            var humanoid = Geometry.CreateBlockyHumanoid();
            var tree = Geometry.CreateBlockyTree();

            // Combine geometries
            var vertices = new List<float>();
            vertices.AddRange(humanoid.Vertices);
            vertices.AddRange(tree.Vertices);

            var indices = new List<uint>();
            indices.AddRange(humanoid.Indices);
            uint offset = (uint)(humanoid.Vertices.Length / 6);
            foreach (var idx in tree.Indices)
            {
                indices.Add(idx + offset);
            }

            _geometry = new Geometry(vertices.ToArray(), indices.ToArray());

            // Setup VAO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _geometry.Vertices.Length * sizeof(float),
                _geometry.Vertices, BufferUsageHint.StaticDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _geometry.Indices.Length * sizeof(uint),
                _geometry.Indices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Color attribute
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            UpdateShader();

            CursorState = CursorState.Grabbed;

            Console.WriteLine("OpenGL Tessellation Demo loaded successfully!");
            Console.WriteLine($"OpenGL Version: {GL.GetString(StringName.Version)}");
            Console.WriteLine($"GLSL Version: {GL.GetString(StringName.ShadingLanguageVersion)}");
        }

        private void LoadShaders()
        {
            string[] domains = { "triangles", "quads", "isolines" };
            string[] spacings = { "equal_spacing", "fractional_even_spacing", "fractional_odd_spacing" };
            string[] spacingSuffixes = { "", "_fraceven", "_fracodd" };

            foreach (var domain in domains)
            {
                string tcsFile = domain == "triangles" ? "Shaders/tess_control.glsl" :
                                 domain == "quads" ? "Shaders/tess_control_quad.glsl" :
                                 "Shaders/tess_control_isoline.glsl";

                for (int i = 0; i < spacings.Length; i++)
                {
                    string key = $"{domain}_{spacings[i]}";
                    string tesFile = $"Shaders/tess_eval_{domain}{spacingSuffixes[i]}.glsl";

                    try
                    {
                        _shaderPrograms[key] = new ShaderProgram(
                            "Shaders/vertex.glsl",
                            tcsFile,
                            tesFile,
                            "Shaders/fragment.glsl"
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Failed to load shader {key}: {ex.Message}");
                    }
                }
            }
        }

        private void UpdateShader()
        {
            string domain = _currentDomain switch
            {
                TessDomain.Triangles => "triangles",
                TessDomain.Quads => "quads",
                TessDomain.Isolines => "isolines",
                _ => "triangles"
            };

            string spacing = _currentSpacing switch
            {
                TessSpacing.Equal => "equal_spacing",
                TessSpacing.FractionalEven => "fractional_even_spacing",
                TessSpacing.FractionalOdd => "fractional_odd_spacing",
                TessSpacing.Integer => "equal_spacing", // Fallback
                _ => "equal_spacing"
            };

            string key = $"{domain}_{spacing}";

            if (_shaderPrograms.TryGetValue(key, out var shader))
            {
                _currentShader = shader;
            }
            else
            {
                Console.WriteLine($"Warning: Shader {key} not found!");
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_currentShader == null) return;

            _currentShader.Use();

            // Update uniforms
            var view = _camera.GetViewMatrix();
            var projection = _camera.GetProjectionMatrix((float)Size.X / Size.Y);
            var mvp = view * projection;

            _currentShader.SetMatrix4("uModelViewProjection", mvp);
            _currentShader.SetFloat("uTessLevel", _tessLevel);
            _currentShader.SetBool("uWireframeMode", _renderMode == RenderMode.Wireframe);
            _currentShader.SetVector3("uWireframeColor", new Vector3(1.0f, 1.0f, 1.0f));

            // Set patch vertices based on domain
            int patchSize = _currentDomain switch
            {
                TessDomain.Triangles => 3,
                TessDomain.Quads => 4,
                TessDomain.Isolines => 4,
                _ => 3
            };
            GL.PatchParameter(PatchParameterInt.PatchVertices, patchSize);

            // Set polygon mode
            if (_renderMode == RenderMode.Wireframe)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Patches, _geometry.Indices.Length, DrawElementsType.UnsignedInt, 0);

            // Draw help text
            if (_showHelp)
            {
                DrawHelpText();
            }

            SwapBuffers();
        }

        private void DrawHelpText()
        {
            // Note: For simplicity, we'll just output to console
            // A full implementation would use text rendering
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            var keyboard = KeyboardState;
            var mouse = MouseState;

            if (keyboard.IsKeyPressed(Keys.Escape))
            {
                _mouseCaptured = !_mouseCaptured;
                CursorState = _mouseCaptured ? CursorState.Grabbed : CursorState.Normal;
            }

            if (keyboard.IsKeyPressed(Keys.H))
            {
                _showHelp = !_showHelp;
            }

            // Domain switching
            if (keyboard.IsKeyPressed(Keys.D1))
            {
                _currentDomain = TessDomain.Triangles;
                UpdateShader();
                Console.WriteLine("Domain: Triangles");
            }
            if (keyboard.IsKeyPressed(Keys.D2))
            {
                _currentDomain = TessDomain.Quads;
                UpdateShader();
                Console.WriteLine("Domain: Quads");
            }
            if (keyboard.IsKeyPressed(Keys.D3))
            {
                _currentDomain = TessDomain.Isolines;
                UpdateShader();
                Console.WriteLine("Domain: Isolines");
            }

            // Spacing switching
            if (keyboard.IsKeyPressed(Keys.Q))
            {
                _currentSpacing = TessSpacing.Equal;
                UpdateShader();
                Console.WriteLine("Spacing: Equal");
            }
            if (keyboard.IsKeyPressed(Keys.E))
            {
                _currentSpacing = TessSpacing.FractionalEven;
                UpdateShader();
                Console.WriteLine("Spacing: Fractional Even");
            }
            if (keyboard.IsKeyPressed(Keys.R))
            {
                _currentSpacing = TessSpacing.FractionalOdd;
                UpdateShader();
                Console.WriteLine("Spacing: Fractional Odd");
            }

            // Render mode
            if (keyboard.IsKeyPressed(Keys.M))
            {
                _renderMode = _renderMode == RenderMode.Color ? RenderMode.Wireframe : RenderMode.Color;
                Console.WriteLine($"Render Mode: {_renderMode}");
            }

            // Tessellation level
            if (keyboard.IsKeyDown(Keys.Equal) || keyboard.IsKeyDown(Keys.KeyPadAdd))
            {
                _tessLevel = Math.Min(_tessLevel + 0.5f, 64.0f);
                Console.WriteLine($"Tess Level: {_tessLevel}");
            }
            if (keyboard.IsKeyDown(Keys.Minus) || keyboard.IsKeyDown(Keys.KeyPadSubtract))
            {
                _tessLevel = Math.Max(_tessLevel - 0.5f, 1.0f);
                Console.WriteLine($"Tess Level: {_tessLevel}");
            }

            // Camera movement
            if (_mouseCaptured)
            {
                _camera.ProcessKeyboard(keyboard, (float)args.Time);

                if (_firstMove)
                {
                    _lastMousePos = new Vector2(mouse.X, mouse.Y);
                    _firstMove = false;
                }
                else
                {
                    float deltaX = mouse.X - _lastMousePos.X;
                    float deltaY = mouse.Y - _lastMousePos.Y;
                    _lastMousePos = new Vector2(mouse.X, mouse.Y);

                    _camera.ProcessMouseMovement(deltaX, -deltaY);
                }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            _camera.ProcessMouseScroll(e.OffsetY);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);

            foreach (var shader in _shaderPrograms.Values)
            {
                shader.Dispose();
            }
        }
    }
}
