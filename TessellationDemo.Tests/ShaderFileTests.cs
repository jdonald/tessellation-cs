using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace TessellationDemo.Tests
{
    /// <summary>
    /// Validates GLSL shader source files without needing a GPU/OpenGL context.
    /// </summary>
    public class ShaderFileTests
    {
        private static string ShaderDir()
        {
            // Try the output directory (shaders copied there by build)
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var candidate = Path.Combine(assemblyDir, "Shaders");
            if (Directory.Exists(candidate)) return candidate;

            // Fall back to traversing up to the repo root
            var dir = assemblyDir;
            for (int i = 0; i < 6; i++)
            {
                var p = Path.Combine(dir, "Shaders");
                if (Directory.Exists(p)) return p;
                dir = Path.GetDirectoryName(dir) ?? dir;
            }
            return candidate; // return non-existent path; tests will fail with a clear message
        }

        private string ReadShader(string filename)
        {
            var path = Path.Combine(ShaderDir(), filename);
            Assert.True(File.Exists(path), $"Shader file not found: {path}");
            return File.ReadAllText(path);
        }

        // ── File existence ───────────────────────────────────────────────

        [Theory]
        [InlineData("vertex.glsl")]
        [InlineData("fragment.glsl")]
        [InlineData("tess_control.glsl")]
        [InlineData("tess_control_quad.glsl")]
        [InlineData("tess_control_isoline.glsl")]
        [InlineData("tess_eval_triangles.glsl")]
        [InlineData("tess_eval_triangles_fraceven.glsl")]
        [InlineData("tess_eval_triangles_fracodd.glsl")]
        [InlineData("tess_eval_quads.glsl")]
        [InlineData("tess_eval_quads_fraceven.glsl")]
        [InlineData("tess_eval_quads_fracodd.glsl")]
        [InlineData("tess_eval_isolines.glsl")]
        [InlineData("tess_eval_isolines_fraceven.glsl")]
        [InlineData("tess_eval_isolines_fracodd.glsl")]
        public void ShaderFile_Exists(string filename)
        {
            var path = Path.Combine(ShaderDir(), filename);
            Assert.True(File.Exists(path), $"Missing shader file: {path}");
        }

        // ── Version directive ────────────────────────────────────────────

        [Theory]
        [InlineData("vertex.glsl")]
        [InlineData("fragment.glsl")]
        [InlineData("tess_control.glsl")]
        [InlineData("tess_control_quad.glsl")]
        [InlineData("tess_control_isoline.glsl")]
        [InlineData("tess_eval_triangles.glsl")]
        [InlineData("tess_eval_quads.glsl")]
        [InlineData("tess_eval_isolines.glsl")]
        public void ShaderFile_HasVersion410(string filename)
        {
            var src = ReadShader(filename);
            Assert.Contains("#version 410", src);
        }

        // ── Main function ─────────────────────────────────────────────────

        [Theory]
        [InlineData("vertex.glsl")]
        [InlineData("fragment.glsl")]
        [InlineData("tess_control.glsl")]
        [InlineData("tess_control_quad.glsl")]
        [InlineData("tess_control_isoline.glsl")]
        [InlineData("tess_eval_triangles.glsl")]
        [InlineData("tess_eval_quads.glsl")]
        [InlineData("tess_eval_isolines.glsl")]
        public void ShaderFile_HasMainFunction(string filename)
        {
            var src = ReadShader(filename);
            Assert.Matches(new Regex(@"void\s+main\s*\(\s*\)"), src);
        }

        // ── Vertex shader specifics ───────────────────────────────────────

        [Fact]
        public void VertexShader_HasPositionInput()
        {
            var src = ReadShader("vertex.glsl");
            Assert.Contains("aPosition", src);
        }

        [Fact]
        public void VertexShader_HasColorInput()
        {
            var src = ReadShader("vertex.glsl");
            Assert.Contains("aColor", src);
        }

        [Fact]
        public void VertexShader_SetsGlPosition()
        {
            var src = ReadShader("vertex.glsl");
            Assert.Contains("gl_Position", src);
        }

        // ── Fragment shader specifics ─────────────────────────────────────

        [Fact]
        public void FragmentShader_HasWireframeModeUniform()
        {
            var src = ReadShader("fragment.glsl");
            Assert.Contains("uWireframeMode", src);
        }

        // ── Tessellation control shaders ──────────────────────────────────

        [Fact]
        public void TessControlTriangles_DeclaresThreeVertices()
        {
            var src = ReadShader("tess_control.glsl");
            Assert.Contains("layout(vertices = 3)", src);
        }

        [Fact]
        public void TessControlQuad_DecaresFourVertices()
        {
            var src = ReadShader("tess_control_quad.glsl");
            Assert.Contains("layout(vertices = 4)", src);
        }

        [Fact]
        public void TessControlIsoline_DecaresFourVertices()
        {
            var src = ReadShader("tess_control_isoline.glsl");
            Assert.Contains("layout(vertices = 4)", src);
        }

        [Theory]
        [InlineData("tess_control.glsl")]
        [InlineData("tess_control_quad.glsl")]
        [InlineData("tess_control_isoline.glsl")]
        public void TessControlShader_SetsTessLevelOuter(string filename)
        {
            var src = ReadShader(filename);
            Assert.Contains("gl_TessLevelOuter", src);
        }

        [Theory]
        [InlineData("tess_control.glsl")]
        [InlineData("tess_control_quad.glsl")]
        public void TessControlShader_SetsTessLevelInner(string filename)
        {
            // Isolines only use outer levels, triangles and quads use inner too
            var src = ReadShader(filename);
            Assert.Contains("gl_TessLevelInner", src);
        }

        [Theory]
        [InlineData("tess_control.glsl")]
        [InlineData("tess_control_quad.glsl")]
        [InlineData("tess_control_isoline.glsl")]
        public void TessControlShader_HasTessLevelUniform(string filename)
        {
            var src = ReadShader(filename);
            Assert.Contains("uTessLevel", src);
        }

        // ── Tessellation evaluation shaders ──────────────────────────────

        [Theory]
        [InlineData("tess_eval_triangles.glsl", "equal_spacing")]
        [InlineData("tess_eval_triangles_fraceven.glsl", "fractional_even_spacing")]
        [InlineData("tess_eval_triangles_fracodd.glsl", "fractional_odd_spacing")]
        public void TessEvalTriangles_HasCorrectSpacing(string filename, string spacing)
        {
            var src = ReadShader(filename);
            Assert.Contains(spacing, src);
            Assert.Contains("triangles", src);
        }

        [Theory]
        [InlineData("tess_eval_quads.glsl", "equal_spacing")]
        [InlineData("tess_eval_quads_fraceven.glsl", "fractional_even_spacing")]
        [InlineData("tess_eval_quads_fracodd.glsl", "fractional_odd_spacing")]
        public void TessEvalQuads_HasCorrectSpacing(string filename, string spacing)
        {
            var src = ReadShader(filename);
            Assert.Contains(spacing, src);
            Assert.Contains("quads", src);
        }

        [Theory]
        [InlineData("tess_eval_isolines.glsl", "equal_spacing")]
        [InlineData("tess_eval_isolines_fraceven.glsl", "fractional_even_spacing")]
        [InlineData("tess_eval_isolines_fracodd.glsl", "fractional_odd_spacing")]
        public void TessEvalIsolines_HasCorrectSpacing(string filename, string spacing)
        {
            var src = ReadShader(filename);
            Assert.Contains(spacing, src);
            Assert.Contains("isolines", src);
        }

        [Theory]
        [InlineData("tess_eval_triangles.glsl")]
        [InlineData("tess_eval_quads.glsl")]
        [InlineData("tess_eval_isolines.glsl")]
        public void TessEvalShader_HasMvpUniform(string filename)
        {
            var src = ReadShader(filename);
            Assert.Contains("uModelViewProjection", src);
        }

        [Theory]
        [InlineData("tess_eval_triangles.glsl")]
        [InlineData("tess_eval_quads.glsl")]
        [InlineData("tess_eval_isolines.glsl")]
        public void TessEvalShader_SetsGlPosition(string filename)
        {
            var src = ReadShader(filename);
            Assert.Contains("gl_Position", src);
        }

        // ── Shader file is non-empty ──────────────────────────────────────

        [Theory]
        [InlineData("vertex.glsl")]
        [InlineData("fragment.glsl")]
        [InlineData("tess_control.glsl")]
        [InlineData("tess_eval_triangles.glsl")]
        public void ShaderFile_IsNotEmpty(string filename)
        {
            var src = ReadShader(filename);
            Assert.True(src.Length > 50, $"Shader {filename} is suspiciously short ({src.Length} chars)");
        }

        // ── No obvious syntax errors (basic heuristics) ───────────────────

        [Theory]
        [InlineData("vertex.glsl")]
        [InlineData("fragment.glsl")]
        [InlineData("tess_control.glsl")]
        [InlineData("tess_eval_triangles.glsl")]
        public void ShaderFile_BalancedBraces(string filename)
        {
            var src = ReadShader(filename);
            int depth = 0;
            foreach (char c in src)
            {
                if (c == '{') depth++;
                else if (c == '}') depth--;
            }
            Assert.Equal(0, depth);
        }
    }
}
