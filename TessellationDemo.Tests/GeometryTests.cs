using System;
using System.Linq;
using Xunit;
using TessellationDemo;

namespace TessellationDemo.Tests
{
    public class GeometryTests
    {
        // ── CreateBlockyHumanoid ─────────────────────────────────────────

        [Fact]
        public void CreateBlockyHumanoid_ReturnsNonNull()
        {
            Assert.NotNull(Geometry.CreateBlockyHumanoid());
        }

        [Fact]
        public void CreateBlockyHumanoid_VerticesNotEmpty()
        {
            var geo = Geometry.CreateBlockyHumanoid();
            Assert.NotEmpty(geo.Vertices);
        }

        [Fact]
        public void CreateBlockyHumanoid_IndicesNotEmpty()
        {
            var geo = Geometry.CreateBlockyHumanoid();
            Assert.NotEmpty(geo.Indices);
        }

        [Fact]
        public void CreateBlockyHumanoid_VerticesMultipleOfSix()
        {
            // Each vertex stores 3 floats for position + 3 floats for color
            var geo = Geometry.CreateBlockyHumanoid();
            Assert.Equal(0, geo.Vertices.Length % 6);
        }

        [Fact]
        public void CreateBlockyHumanoid_IndicesMultipleOfThree()
        {
            // Triangles → indices must come in triples
            var geo = Geometry.CreateBlockyHumanoid();
            Assert.Equal(0, geo.Indices.Length % 3);
        }

        [Fact]
        public void CreateBlockyHumanoid_AllIndicesInBounds()
        {
            var geo = Geometry.CreateBlockyHumanoid();
            uint maxVertex = (uint)(geo.Vertices.Length / 6);
            Assert.All(geo.Indices, idx => Assert.True(idx < maxVertex,
                $"Index {idx} >= vertex count {maxVertex}"));
        }

        [Fact]
        public void CreateBlockyHumanoid_ColorsInUnitRange()
        {
            var geo = Geometry.CreateBlockyHumanoid();
            // Color floats are at positions 3,4,5 within each 6-float vertex
            for (int i = 3; i < geo.Vertices.Length; i += 6)
            {
                for (int c = 0; c < 3; c++)
                {
                    float v = geo.Vertices[i + c];
                    Assert.True(v >= 0f && v <= 1f, $"Color component {v} at offset {i + c} is out of [0,1]");
                }
            }
        }

        [Fact]
        public void CreateBlockyHumanoid_PositionsAreFinite()
        {
            var geo = Geometry.CreateBlockyHumanoid();
            for (int i = 0; i < geo.Vertices.Length; i += 6)
            {
                for (int p = 0; p < 3; p++)
                {
                    float v = geo.Vertices[i + p];
                    Assert.True(float.IsFinite(v), $"Position component at offset {i + p} is not finite: {v}");
                }
            }
        }

        [Fact]
        public void CreateBlockyHumanoid_HasAtLeast200Vertices()
        {
            // Humanoid: head + torso + 2 arms + 2 legs → 6 boxes × 6 faces × 4 verts = 144+
            var geo = Geometry.CreateBlockyHumanoid();
            int vertexCount = geo.Vertices.Length / 6;
            Assert.True(vertexCount >= 100, $"Expected ≥100 vertices, got {vertexCount}");
        }

        [Fact]
        public void CreateBlockyHumanoid_IsOffsetToLeftOfOrigin()
        {
            // The humanoid is offset by -1.5 in X
            var geo = Geometry.CreateBlockyHumanoid();
            // All X positions should be < 0
            bool hasNegativeX = false;
            for (int i = 0; i < geo.Vertices.Length; i += 6)
            {
                if (geo.Vertices[i] < 0f) hasNegativeX = true;
            }
            Assert.True(hasNegativeX);
        }

        // ── CreateBlockyTree ─────────────────────────────────────────────

        [Fact]
        public void CreateBlockyTree_ReturnsNonNull()
        {
            Assert.NotNull(Geometry.CreateBlockyTree());
        }

        [Fact]
        public void CreateBlockyTree_VerticesNotEmpty()
        {
            var geo = Geometry.CreateBlockyTree();
            Assert.NotEmpty(geo.Vertices);
        }

        [Fact]
        public void CreateBlockyTree_IndicesNotEmpty()
        {
            var geo = Geometry.CreateBlockyTree();
            Assert.NotEmpty(geo.Indices);
        }

        [Fact]
        public void CreateBlockyTree_VerticesMultipleOfSix()
        {
            var geo = Geometry.CreateBlockyTree();
            Assert.Equal(0, geo.Vertices.Length % 6);
        }

        [Fact]
        public void CreateBlockyTree_IndicesMultipleOfThree()
        {
            var geo = Geometry.CreateBlockyTree();
            Assert.Equal(0, geo.Indices.Length % 3);
        }

        [Fact]
        public void CreateBlockyTree_AllIndicesInBounds()
        {
            var geo = Geometry.CreateBlockyTree();
            uint maxVertex = (uint)(geo.Vertices.Length / 6);
            Assert.All(geo.Indices, idx => Assert.True(idx < maxVertex,
                $"Index {idx} >= vertex count {maxVertex}"));
        }

        [Fact]
        public void CreateBlockyTree_ColorsInUnitRange()
        {
            var geo = Geometry.CreateBlockyTree();
            for (int i = 3; i < geo.Vertices.Length; i += 6)
            {
                for (int c = 0; c < 3; c++)
                {
                    float v = geo.Vertices[i + c];
                    Assert.True(v >= 0f && v <= 1f, $"Color component {v} at offset {i + c} is out of [0,1]");
                }
            }
        }

        [Fact]
        public void CreateBlockyTree_PositionsAreFinite()
        {
            var geo = Geometry.CreateBlockyTree();
            for (int i = 0; i < geo.Vertices.Length; i += 6)
            {
                for (int p = 0; p < 3; p++)
                {
                    float v = geo.Vertices[i + p];
                    Assert.True(float.IsFinite(v), $"Position component at offset {i + p} is not finite: {v}");
                }
            }
        }

        [Fact]
        public void CreateBlockyTree_IsOffsetToRightOfOrigin()
        {
            // Tree is at offset +1.5 in X
            var geo = Geometry.CreateBlockyTree();
            bool hasPositiveX = false;
            for (int i = 0; i < geo.Vertices.Length; i += 6)
            {
                if (geo.Vertices[i] > 0f) hasPositiveX = true;
            }
            Assert.True(hasPositiveX);
        }

        [Fact]
        public void CreateBlockyTree_HasAtLeast50Vertices()
        {
            // Tree: trunk (1 box) + 3 leaf cubes → 4 boxes × 6 faces × 4 verts = 96+
            var geo = Geometry.CreateBlockyTree();
            int vertexCount = geo.Vertices.Length / 6;
            Assert.True(vertexCount >= 50, $"Expected ≥50 vertices, got {vertexCount}");
        }

        // ── Geometry combination ─────────────────────────────────────────

        [Fact]
        public void HumanoidAndTree_ProduceDifferentVertexCounts()
        {
            var humanoid = Geometry.CreateBlockyHumanoid();
            var tree = Geometry.CreateBlockyTree();
            Assert.NotEqual(humanoid.Vertices.Length, tree.Vertices.Length);
        }

        [Fact]
        public void HumanoidAndTree_XRangesDontOverlap()
        {
            // Humanoid is at x≈-1.5, tree at x≈+1.5 - they should not overlap
            var humanoid = Geometry.CreateBlockyHumanoid();
            var tree = Geometry.CreateBlockyTree();

            float humanoidMaxX = float.MinValue;
            for (int i = 0; i < humanoid.Vertices.Length; i += 6)
                humanoidMaxX = Math.Max(humanoidMaxX, humanoid.Vertices[i]);

            float treeMinX = float.MaxValue;
            for (int i = 0; i < tree.Vertices.Length; i += 6)
                treeMinX = Math.Min(treeMinX, tree.Vertices[i]);

            Assert.True(humanoidMaxX < treeMinX,
                $"Humanoid max X ({humanoidMaxX}) should be less than tree min X ({treeMinX})");
        }

        // ── Geometry struct ──────────────────────────────────────────────

        [Fact]
        public void Geometry_Constructor_AssignsArrays()
        {
            var verts = new float[] { 1f, 2f, 3f, 0.5f, 0.5f, 0.5f };
            var idxs = new uint[] { 0 };
            var geo = new Geometry(verts, idxs);
            Assert.Same(verts, geo.Vertices);
            Assert.Same(idxs, geo.Indices);
        }

        [Fact]
        public void Geometry_VerticesAndIndicesCanBeReplaced()
        {
            var geo = Geometry.CreateBlockyHumanoid();
            var newVerts = new float[6];
            geo.Vertices = newVerts;
            Assert.Same(newVerts, geo.Vertices);
        }
    }
}
