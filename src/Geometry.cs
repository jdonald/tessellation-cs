using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace TessellationDemo
{
    public class Geometry
    {
        public float[] Vertices { get; set; }
        public uint[] Indices { get; set; }

        public Geometry(float[] vertices, uint[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }

        public static Geometry CreateBlockyHumanoid()
        {
            var vertices = new List<float>();
            var indices = new List<uint>();

            // Helper function to add a quad (for tessellation patches)
            void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 color)
            {
                uint baseIndex = (uint)(vertices.Count / 6);

                // Position + Color for each vertex
                vertices.AddRange(new[] { p0.X, p0.Y, p0.Z, color.X, color.Y, color.Z });
                vertices.AddRange(new[] { p1.X, p1.Y, p1.Z, color.X, color.Y, color.Z });
                vertices.AddRange(new[] { p2.X, p2.Y, p2.Z, color.X, color.Y, color.Z });
                vertices.AddRange(new[] { p3.X, p3.Y, p3.Z, color.X, color.Y, color.Z });

                // Two triangles per quad (as patches)
                indices.AddRange(new uint[] { baseIndex, baseIndex + 1, baseIndex + 2 });
                indices.AddRange(new uint[] { baseIndex, baseIndex + 2, baseIndex + 3 });
            }

            // Humanoid colors
            Vector3 bodyColor = new Vector3(0.8f, 0.3f, 0.3f);  // Red torso
            Vector3 headColor = new Vector3(0.9f, 0.7f, 0.6f);  // Peach head
            Vector3 limbColor = new Vector3(0.4f, 0.4f, 0.8f);  // Blue limbs

            float offset = -1.5f; // Shift humanoid to the left

            // HEAD (cube)
            float headSize = 0.3f;
            float headY = 1.2f;
            AddCube(vertices, indices, new Vector3(offset, headY, 0), headSize, headColor);

            // TORSO (rectangular prism)
            float torsoWidth = 0.4f;
            float torsoHeight = 0.6f;
            float torsoDepth = 0.25f;
            Vector3 torsoPos = new Vector3(offset, 0.6f, 0);
            AddBox(vertices, indices, torsoPos, torsoWidth, torsoHeight, torsoDepth, bodyColor);

            // LEFT ARM
            float armWidth = 0.15f;
            float armLength = 0.5f;
            Vector3 leftArmPos = new Vector3(offset - torsoWidth / 2 - armWidth / 2, 0.8f, 0);
            AddBox(vertices, indices, leftArmPos, armWidth, armLength, armWidth, limbColor);

            // RIGHT ARM
            Vector3 rightArmPos = new Vector3(offset + torsoWidth / 2 + armWidth / 2, 0.8f, 0);
            AddBox(vertices, indices, rightArmPos, armWidth, armLength, armWidth, limbColor);

            // LEFT LEG
            float legWidth = 0.15f;
            float legLength = 0.6f;
            Vector3 leftLegPos = new Vector3(offset - 0.1f, 0.0f, 0);
            AddBox(vertices, indices, leftLegPos, legWidth, legLength, legWidth, limbColor);

            // RIGHT LEG
            Vector3 rightLegPos = new Vector3(offset + 0.1f, 0.0f, 0);
            AddBox(vertices, indices, rightLegPos, legWidth, legLength, legWidth, limbColor);

            return new Geometry(vertices.ToArray(), indices.ToArray());
        }

        public static Geometry CreateBlockyTree()
        {
            var vertices = new List<float>();
            var indices = new List<uint>();

            Vector3 trunkColor = new Vector3(0.4f, 0.25f, 0.1f);  // Brown
            Vector3 leavesColor = new Vector3(0.2f, 0.7f, 0.2f);  // Green

            float offset = 1.5f; // Shift tree to the right

            // TRUNK (tall rectangular prism)
            float trunkWidth = 0.2f;
            float trunkHeight = 1.0f;
            Vector3 trunkPos = new Vector3(offset, 0.5f, 0);
            AddBox(vertices, indices, trunkPos, trunkWidth, trunkHeight, trunkWidth, trunkColor);

            // LEAVES (pyramid made of stacked cubes)
            float leafSize = 0.6f;
            Vector3 leavesPos1 = new Vector3(offset, 1.2f, 0);
            AddCube(vertices, indices, leavesPos1, leafSize, leavesColor);

            float leafSize2 = 0.5f;
            Vector3 leavesPos2 = new Vector3(offset, 1.5f, 0);
            AddCube(vertices, indices, leavesPos2, leafSize2, leavesColor);

            float leafSize3 = 0.3f;
            Vector3 leavesPos3 = new Vector3(offset, 1.8f, 0);
            AddCube(vertices, indices, leavesPos3, leafSize3, leavesColor);

            return new Geometry(vertices.ToArray(), indices.ToArray());
        }

        private static void AddCube(List<float> vertices, List<uint> indices, Vector3 center, float size, Vector3 color)
        {
            AddBox(vertices, indices, center, size, size, size, color);
        }

        private static void AddBox(List<float> vertices, List<uint> indices, Vector3 center, float width, float height, float depth, Vector3 color)
        {
            float hw = width / 2;
            float hh = height / 2;
            float hd = depth / 2;

            Vector3[] corners = new Vector3[8]
            {
                center + new Vector3(-hw, -hh, -hd),
                center + new Vector3( hw, -hh, -hd),
                center + new Vector3( hw,  hh, -hd),
                center + new Vector3(-hw,  hh, -hd),
                center + new Vector3(-hw, -hh,  hd),
                center + new Vector3( hw, -hh,  hd),
                center + new Vector3( hw,  hh,  hd),
                center + new Vector3(-hw,  hh,  hd)
            };

            // Add quads for each face
            AddQuad(vertices, indices, corners[0], corners[1], corners[2], corners[3], color); // Front
            AddQuad(vertices, indices, corners[5], corners[4], corners[7], corners[6], color); // Back
            AddQuad(vertices, indices, corners[4], corners[0], corners[3], corners[7], color); // Left
            AddQuad(vertices, indices, corners[1], corners[5], corners[6], corners[2], color); // Right
            AddQuad(vertices, indices, corners[3], corners[2], corners[6], corners[7], color); // Top
            AddQuad(vertices, indices, corners[4], corners[5], corners[1], corners[0], color); // Bottom
        }

        private static void AddQuad(List<float> vertices, List<uint> indices, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 color)
        {
            uint baseIndex = (uint)(vertices.Count / 6);

            vertices.AddRange(new[] { p0.X, p0.Y, p0.Z, color.X, color.Y, color.Z });
            vertices.AddRange(new[] { p1.X, p1.Y, p1.Z, color.X, color.Y, color.Z });
            vertices.AddRange(new[] { p2.X, p2.Y, p2.Z, color.X, color.Y, color.Z });
            vertices.AddRange(new[] { p3.X, p3.Y, p3.Z, color.X, color.Y, color.Z });

            indices.AddRange(new uint[] { baseIndex, baseIndex + 1, baseIndex + 2 });
            indices.AddRange(new uint[] { baseIndex, baseIndex + 2, baseIndex + 3 });
        }
    }
}
