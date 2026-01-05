using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TessellationDemo
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Front { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }

        private float _yaw = -90.0f;
        private float _pitch = 0.0f;
        private float _fov = 45.0f;

        public float Yaw
        {
            get => _yaw;
            set
            {
                _yaw = value;
                UpdateVectors();
            }
        }

        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = MathHelper.Clamp(value, -89.0f, 89.0f);
                UpdateVectors();
            }
        }

        public float Fov
        {
            get => _fov;
            set => _fov = MathHelper.Clamp(value, 1.0f, 90.0f);
        }

        public Camera(Vector3 position)
        {
            Position = position;
            Up = Vector3.UnitY;
            UpdateVectors();
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        public Matrix4 GetProjectionMatrix(float aspectRatio)
        {
            return Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(Fov),
                aspectRatio,
                0.1f,
                100.0f
            );
        }

        public void ProcessKeyboard(KeyboardState keyboard, float deltaTime)
        {
            float velocity = 2.5f * deltaTime;

            if (keyboard.IsKeyDown(Keys.W))
                Position += Front * velocity;
            if (keyboard.IsKeyDown(Keys.S))
                Position -= Front * velocity;
            if (keyboard.IsKeyDown(Keys.A))
                Position -= Right * velocity;
            if (keyboard.IsKeyDown(Keys.D))
                Position += Right * velocity;
            if (keyboard.IsKeyDown(Keys.Space))
                Position += Up * velocity;
            if (keyboard.IsKeyDown(Keys.LeftShift))
                Position -= Up * velocity;
        }

        public void ProcessMouseMovement(float xOffset, float yOffset, float sensitivity = 0.1f)
        {
            xOffset *= sensitivity;
            yOffset *= sensitivity;

            Yaw += xOffset;
            Pitch += yOffset;
        }

        public void ProcessMouseScroll(float yOffset)
        {
            Fov -= yOffset;
        }

        private void UpdateVectors()
        {
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            Front = Vector3.Normalize(front);

            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}
