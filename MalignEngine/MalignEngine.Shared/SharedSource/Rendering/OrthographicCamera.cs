using Silk.NET.OpenGLES;
using System.Numerics;

namespace MalignEngine
{
    public struct OrthographicCamera
    {
        public float Zoom;

        /*
        public float Rotation;

        public static float DegreesToRadians(float degrees)
        {
            return MathF.PI / 180f * degrees;
        }

        public Matrix4x4 ViewMatrix
        {
            get
            {
                if (RenderMode == RenderingMode.Orthographic)
                {
                    return Matrix4x4.Identity;
                }
                else
                {
                    var cameraDirection = Vector3.Zero;
                    cameraDirection.X = MathF.Cos(DegreesToRadians(EulerRotation.X)) * MathF.Cos(DegreesToRadians(EulerRotation.Y));
                    cameraDirection.Y = MathF.Sin(DegreesToRadians(EulerRotation.Y));
                    cameraDirection.Z = MathF.Sin(DegreesToRadians(EulerRotation.X)) * MathF.Cos(DegreesToRadians(EulerRotation.Y));

                    cameraDirection = Vector3.Normalize(cameraDirection);

                    return Matrix4x4.CreateLookAt(Position, Position + cameraDirection, Vector3.UnitY);
                }
            }
        }

        public Matrix4x4 ProjectionMatrix
        {
            get
            {
                if (Width == 0 || Height == 0)
                {
                    Width = 1;
                    Height = 1;
                }

                float aspectRatio = ((float)Width) / Height;

                if (RenderMode == RenderingMode.Orthographic)
                {

                    float left = Size * -aspectRatio + Position.X;
                    float right = Size * aspectRatio + Position.X;
                    float top = Size * -1.0f + Position.Y;
                    float bottom = Size * 1.0f + Position.Y;

                    Matrix4x4 orthographicMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, top, bottom, 0.001f, 10f);
                    return orthographicMatrix;
                }
                else
                {
                    return Matrix4x4.CreatePerspectiveFieldOfView(FOV, aspectRatio, 0.1f, 100.0f);
                }

                //float left = -aspectRatio + Position.X;
                //float right = aspectRatio + Position.X;
                //float top = -1.0f + Position.Y;
                //float bottom = 1.0f + Position.Y;


                //var model = Matrix4x4.CreateRotationY(0f);
                //var view = Matrix4x4.CreateLookAt(Position, Position + cameraDirection, Vector3.UnitY);
                //var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 3f, Width / Height, 0.1f, 100.0f);

                //return projection * view * model;

                //var orthographicMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, top, bottom, 0.001f, 10f);
                //var zoomMatrix = Matrix4x4.CreateScale(MathF.Exp(Zoom));
                //return orthographicMatrix * zoomMatrix * Matrix4x4.CreateFromQuaternion(Rotation);
            }
        }

        public Vector2 GetWorldSize()
        {
            var zero = Renderer.Current.Camera.ScreenToWorld(Vector2.Zero);
            var right = Renderer.Current.Camera.ScreenToWorld(new Vector2(Renderer.Current.Camera.Width, 0));
            var down = Renderer.Current.Camera.ScreenToWorld(new Vector2(0, Renderer.Current.Camera.Height));

            return new Vector2(Vector2.Distance(zero, right), Vector2.Distance(zero, down));
        }

        public Vector2 ScreenToWorld(Vector2 position)
        {
            position = new Vector2((position.X / Width - 0.5f) * 2f, (-position.Y / Height + 0.5f) * 2f);

            Matrix4x4.Invert(ProjectionMatrix, out Matrix4x4 matrix);

            position = Vector2.Transform(position, matrix);

            return position;
        }

        public Vector3 ScreenToWorld(Vector3 position)
        {
            return ScreenToWorld(position.ToVector2()).ToVector3();
        }

        public Vector2 WorldToScreen(Vector2 position)
        {
            position = Vector2.Transform(position, ProjectionMatrix);

            position = new Vector2(((position.X / 2f) + 0.5f) * Width, ((position.Y / 2f) + 0.5f) * Height);

            return position;
        }
        */
    }
}