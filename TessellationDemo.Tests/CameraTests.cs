using System;
using OpenTK.Mathematics;
using Xunit;
using TessellationDemo;

namespace TessellationDemo.Tests
{
    public class CameraTests
    {
        // ── Construction ────────────────────────────────────────────────

        [Fact]
        public void Constructor_SetsPosition()
        {
            var pos = new Vector3(1f, 2f, 3f);
            var cam = new Camera(pos);
            Assert.Equal(pos, cam.Position);
        }

        [Fact]
        public void Constructor_DefaultPitchIsZero()
        {
            var cam = new Camera(Vector3.Zero);
            Assert.Equal(0f, cam.Pitch, precision: 4);
        }

        [Fact]
        public void Constructor_DefaultYawIsMinusNinety()
        {
            var cam = new Camera(Vector3.Zero);
            Assert.Equal(-90f, cam.Yaw, precision: 4);
        }

        [Fact]
        public void Constructor_DefaultFovIs45()
        {
            var cam = new Camera(Vector3.Zero);
            Assert.Equal(45f, cam.Fov, precision: 4);
        }

        [Fact]
        public void Constructor_FrontVectorIsNormalized()
        {
            var cam = new Camera(Vector3.Zero);
            Assert.Equal(1f, cam.Front.Length, precision: 4);
        }

        [Fact]
        public void Constructor_UpVectorIsNormalized()
        {
            var cam = new Camera(Vector3.Zero);
            Assert.Equal(1f, cam.Up.Length, precision: 4);
        }

        [Fact]
        public void Constructor_RightVectorIsNormalized()
        {
            var cam = new Camera(Vector3.Zero);
            Assert.Equal(1f, cam.Right.Length, precision: 4);
        }

        // ── Default yaw (-90°) produces a -Z front direction ────────────

        [Fact]
        public void Constructor_DefaultFrontPointsNegativeZ()
        {
            // Yaw=-90, Pitch=0 → front=(cos(-90)*cos(0), sin(0), sin(-90)*cos(0))=(0,0,-1)
            var cam = new Camera(Vector3.Zero);
            Assert.Equal(0f, cam.Front.X, precision: 4);
            Assert.Equal(0f, cam.Front.Y, precision: 4);
            Assert.Equal(-1f, cam.Front.Z, precision: 4);
        }

        // ── Pitch clamping ──────────────────────────────────────────────

        [Fact]
        public void Pitch_ClampedAt89()
        {
            var cam = new Camera(Vector3.Zero);
            cam.Pitch = 200f;
            Assert.Equal(89f, cam.Pitch, precision: 4);
        }

        [Fact]
        public void Pitch_ClampedAtMinus89()
        {
            var cam = new Camera(Vector3.Zero);
            cam.Pitch = -200f;
            Assert.Equal(-89f, cam.Pitch, precision: 4);
        }

        [Fact]
        public void Pitch_AcceptsValidValue()
        {
            var cam = new Camera(Vector3.Zero);
            cam.Pitch = 45f;
            Assert.Equal(45f, cam.Pitch, precision: 4);
        }

        // ── Pitch setter triggers UpdateVectors ─────────────────────────

        [Fact]
        public void Pitch_UpdatesFrontY()
        {
            var cam = new Camera(Vector3.Zero);
            cam.Pitch = 45f;
            // sin(45°) ≈ 0.7071
            float expected = MathF.Sin(MathHelper.DegreesToRadians(45f));
            Assert.Equal(expected, cam.Front.Y, precision: 3);
        }

        // ── FOV clamping ────────────────────────────────────────────────

        [Fact]
        public void Fov_ClampedAt90()
        {
            var cam = new Camera(Vector3.Zero);
            cam.Fov = 200f;
            Assert.Equal(90f, cam.Fov, precision: 4);
        }

        [Fact]
        public void Fov_ClampedAt1()
        {
            var cam = new Camera(Vector3.Zero);
            cam.Fov = -10f;
            Assert.Equal(1f, cam.Fov, precision: 4);
        }

        [Fact]
        public void Fov_AcceptsValidValue()
        {
            var cam = new Camera(Vector3.Zero);
            cam.Fov = 60f;
            Assert.Equal(60f, cam.Fov, precision: 4);
        }

        // ── Yaw setter ──────────────────────────────────────────────────

        [Fact]
        public void Yaw_Setter_UpdatesFrontVector()
        {
            var cam = new Camera(Vector3.Zero) { Pitch = 0f };
            cam.Yaw = 0f; // cos(0)*cos(0)=1, sin(0)*cos(0)=0 → front=(1,0,0)
            Assert.Equal(1f, cam.Front.X, precision: 4);
            Assert.Equal(0f, cam.Front.Z, precision: 4);
        }

        // ── Mouse movement ──────────────────────────────────────────────

        [Fact]
        public void ProcessMouseMovement_UpdatesYaw()
        {
            var cam = new Camera(Vector3.Zero);
            float initialYaw = cam.Yaw;
            cam.ProcessMouseMovement(10f, 0f);
            // sensitivity=0.1 by default: Yaw += 10*0.1=1
            Assert.Equal(initialYaw + 1f, cam.Yaw, precision: 4);
        }

        [Fact]
        public void ProcessMouseMovement_UpdatesPitch()
        {
            var cam = new Camera(Vector3.Zero);
            float initialPitch = cam.Pitch;
            cam.ProcessMouseMovement(0f, 5f);
            Assert.Equal(MathHelper.Clamp(initialPitch + 0.5f, -89f, 89f), cam.Pitch, precision: 4);
        }

        [Fact]
        public void ProcessMouseMovement_RespectsSensitivity()
        {
            var cam = new Camera(Vector3.Zero);
            float initialYaw = cam.Yaw;
            cam.ProcessMouseMovement(10f, 0f, sensitivity: 0.2f);
            Assert.Equal(initialYaw + 2f, cam.Yaw, precision: 4);
        }

        // ── Mouse scroll ────────────────────────────────────────────────

        [Fact]
        public void ProcessMouseScroll_DecreasesFov()
        {
            var cam = new Camera(Vector3.Zero);
            float initialFov = cam.Fov;
            cam.ProcessMouseScroll(1f);
            Assert.Equal(initialFov - 1f, cam.Fov, precision: 4);
        }

        [Fact]
        public void ProcessMouseScroll_NegativeIncreaseFov()
        {
            var cam = new Camera(Vector3.Zero);
            float initialFov = cam.Fov;
            cam.ProcessMouseScroll(-2f);
            Assert.Equal(MathHelper.Clamp(initialFov + 2f, 1f, 90f), cam.Fov, precision: 4);
        }

        // ── Matrix generation ───────────────────────────────────────────

        [Fact]
        public void GetViewMatrix_ReturnsNonIdentity()
        {
            var cam = new Camera(new Vector3(0, 1.5f, 5));
            var view = cam.GetViewMatrix();
            Assert.NotEqual(Matrix4.Identity, view);
        }

        [Fact]
        public void GetProjectionMatrix_ReturnsNonIdentity()
        {
            var cam = new Camera(Vector3.Zero);
            var proj = cam.GetProjectionMatrix(16f / 9f);
            Assert.NotEqual(Matrix4.Identity, proj);
        }

        [Fact]
        public void GetProjectionMatrix_DifferentAspectRatiosProduceDifferentMatrices()
        {
            var cam = new Camera(Vector3.Zero);
            var proj1 = cam.GetProjectionMatrix(1f);
            var proj2 = cam.GetProjectionMatrix(16f / 9f);
            Assert.NotEqual(proj1, proj2);
        }

        [Fact]
        public void GetViewMatrix_ChangesWithPosition()
        {
            var cam1 = new Camera(new Vector3(0, 0, 5));
            var cam2 = new Camera(new Vector3(0, 0, 10));
            var v1 = cam1.GetViewMatrix();
            var v2 = cam2.GetViewMatrix();
            Assert.NotEqual(v1, v2);
        }

        // ── Right / Up orthogonality ────────────────────────────────────

        [Fact]
        public void FrontAndRight_AreOrthogonal()
        {
            var cam = new Camera(new Vector3(1, 2, 3));
            cam.Yaw = 30f;
            cam.Pitch = 20f;
            float dot = Vector3.Dot(cam.Front, cam.Right);
            Assert.Equal(0f, dot, precision: 4);
        }

        [Fact]
        public void FrontAndUp_AreOrthogonal()
        {
            var cam = new Camera(Vector3.Zero);
            cam.Yaw = 45f;
            cam.Pitch = 30f;
            float dot = Vector3.Dot(cam.Front, cam.Up);
            Assert.Equal(0f, dot, precision: 4);
        }
    }
}
